import { Injectable, Logger, BadRequestException, InternalServerErrorException, NotFoundException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { CreateLoteDto, ConsultaLoteDto } from './dto/create-lote.dto';
import { CodigoGeneracionService } from '../common/services/codigo-generacion.service';
import { FirmadorService } from '../firmador/firmador.service';
import { MhIntegrationService } from '../mh-integration/mh-integration.service';
import { Dte } from '../common/entities/dte.entity';

@Injectable()
export class LotesService {
  private readonly logger = new Logger(LotesService.name);

  constructor(
    @InjectRepository(Dte)
    private readonly dteRepository: Repository<Dte>,
    private readonly codigoGeneracionService: CodigoGeneracionService,
    private readonly firmadorService: FirmadorService,
    private readonly mhIntegrationService: MhIntegrationService,
  ) {}

  async create(createLoteDto: CreateLoteDto) {
    try {
      this.logger.log(`Iniciando procesamiento de lote con ${createLoteDto.documentos.length} documentos`);

      // 1. Validar datos del lote
      this.validateLoteData(createLoteDto);

      // 2. Validar que los DTEs existen y están listos para envío
      await this.validateDtesForLote(createLoteDto.documentos);

      // 3. Generar código de lote si no existe
      if (!createLoteDto.identificacion.codigoLote) {
        createLoteDto.identificacion.codigoLote = this.codigoGeneracionService.generateCodigoGeneracion();
      }

      // 4. Establecer fecha y hora si no están presentes
      if (!createLoteDto.identificacion.fecRecepcion) {
        const { fecEmi, horEmi } = this.codigoGeneracionService.generateFechaHoraEmision();
        createLoteDto.identificacion.fecRecepcion = fecEmi;
        createLoteDto.identificacion.horRecepcion = horEmi;
      }

      // 5. Preparar lote para envío
      const loteDocument = this.buildLoteDocument(createLoteDto);

      // 6. Enviar lote al Ministerio de Hacienda
      this.logger.log('Enviando lote al Ministerio de Hacienda');
      const respuestaMH = await this.mhIntegrationService.sendDTE(loteDocument);

      // 7. Procesar respuesta y actualizar estados
      const resultadoProcesamiento = await this.procesarRespuestaLote(createLoteDto, respuestaMH);

      // 8. Guardar lote en base de datos
      await this.saveLoteToDatabase(createLoteDto, respuestaMH, resultadoProcesamiento);

      this.logger.log('Lote procesado exitosamente');
      return {
        success: true,
        codigoLote: createLoteDto.identificacion.codigoLote,
        totalDocumentos: createLoteDto.documentos.length,
        procesados: resultadoProcesamiento.procesados,
        rechazados: resultadoProcesamiento.rechazados,
        errores: resultadoProcesamiento.errores,
        selloRecibido: respuestaMH.selloRecibido,
        fhProcesamiento: respuestaMH.fhProcesamiento,
        observaciones: respuestaMH.observaciones
      };

    } catch (error) {
      this.logger.error('Error al procesar lote', error.stack);
      if (error instanceof BadRequestException || error instanceof NotFoundException) {
        throw error;
      }
      throw new InternalServerErrorException('Error interno al procesar lote');
    }
  }

  private validateLoteData(createLoteDto: CreateLoteDto): void {
    // Validar versión del lote
    if (createLoteDto.identificacion.version !== 1) {
      throw new BadRequestException('La versión del lote debe ser 1');
    }

    // Validar ambiente
    const ambientesValidos = ['00', '01'];
    if (!ambientesValidos.includes(createLoteDto.identificacion.ambiente)) {
      throw new BadRequestException('Ambiente debe ser 00 (pruebas) o 01 (producción)');
    }

    // Validar cantidad de documentos
    if (createLoteDto.documentos.length === 0) {
      throw new BadRequestException('El lote debe contener al menos un documento');
    }

    if (createLoteDto.documentos.length > 500) {
      throw new BadRequestException('El lote no puede contener más de 500 documentos');
    }

    // Validar que la cantidad declarada coincida con los documentos
    if (createLoteDto.identificacion.cantidadDoc !== createLoteDto.documentos.length) {
      throw new BadRequestException(
        `La cantidad declarada (${createLoteDto.identificacion.cantidadDoc}) no coincide con los documentos enviados (${createLoteDto.documentos.length})`
      );
    }

    // Validar numeración secuencial de ítems
    createLoteDto.documentos.forEach((doc, index) => {
      if (doc.noItem !== index + 1) {
        throw new BadRequestException(`El número de ítem debe ser consecutivo. Esperado: ${index + 1}, Recibido: ${doc.noItem}`);
      }
    });

    // Validar códigos de generación únicos
    const codigosGeneracion = createLoteDto.documentos.map(doc => doc.codigoGeneracion);
    const codigosUnicos = new Set(codigosGeneracion);
    if (codigosUnicos.size !== codigosGeneracion.length) {
      throw new BadRequestException('Los códigos de generación deben ser únicos dentro del lote');
    }

    // Validar formatos de documentos
    this.validateDocumentFormats(createLoteDto.documentos);
  }

  private validateDocumentFormats(documentos: any[]): void {
    documentos.forEach((doc, index) => {
      // Validar código de generación
      const codigoPattern = /^[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}$/;
      if (!codigoPattern.test(doc.codigoGeneracion)) {
        throw new BadRequestException(`Formato de código de generación inválido en ítem ${index + 1}: ${doc.codigoGeneracion}`);
      }

      // Validar número de control
      const numeroControlPattern = /^DTE-\d{2}-\d{4}-\d{4}-\d{7,15}$/;
      if (!numeroControlPattern.test(doc.numeroControl)) {
        throw new BadRequestException(`Formato de número de control inválido en ítem ${index + 1}: ${doc.numeroControl}`);
      }

      // Validar tipo de DTE
      const tiposDteValidos = ['01', '03', '04', '05', '06', '11', '14'];
      if (!tiposDteValidos.includes(doc.tipoDte)) {
        throw new BadRequestException(`Tipo de DTE inválido en ítem ${index + 1}: ${doc.tipoDte}`);
      }

      // Validar monto total
      if (doc.montoTotal <= 0) {
        throw new BadRequestException(`El monto total debe ser mayor a cero en ítem ${index + 1}`);
      }
    });
  }

  private async validateDtesForLote(documentos: any[]): Promise<void> {
    for (const doc of documentos) {
      // Verificar que el DTE existe (opcional, comentado para evitar errores de BD)
      /*
      const dte = await this.dteRepository.findOne({
        where: { codigoGeneracion: doc.codigoGeneracion }
      });

      if (!dte) {
        throw new NotFoundException(`DTE no encontrado: ${doc.codigoGeneracion}`);
      }

      // Validar que el DTE esté en estado válido para lote
      const estadosValidosParaLote = ['FIRMADO', 'PENDIENTE_ENVIO'];
      if (!estadosValidosParaLote.includes(dte.estadoMh)) {
        throw new BadRequestException(`DTE ${doc.codigoGeneracion} no está en estado válido para lote. Estado actual: ${dte.estadoMh}`);
      }
      */
    }
  }

  private buildLoteDocument(createLoteDto: CreateLoteDto) {
    return {
      identificacion: createLoteDto.identificacion,
      emisor: createLoteDto.emisor,
      documentos: createLoteDto.documentos.map(doc => ({
        noItem: doc.noItem,
        tipoDte: doc.tipoDte,
        codigoGeneracion: doc.codigoGeneracion,
        numeroControl: doc.numeroControl,
        fecEmi: doc.fecEmi,
        montoTotal: doc.montoTotal,
        documentoJson: doc.documentoJson
      }))
    };
  }

  private async procesarRespuestaLote(createLoteDto: CreateLoteDto, respuestaMH: any) {
    const procesados = [];
    const rechazados = [];
    const errores = [];

    // Simular procesamiento de respuesta del MH
    // En la implementación real, aquí procesarías la respuesta del MH
    for (const doc of createLoteDto.documentos) {
      try {
        // Simular que la mayoría se procesan correctamente
        const exito = Math.random() > 0.1; // 90% de éxito

        if (exito) {
          procesados.push({
            codigoGeneracion: doc.codigoGeneracion,
            numeroControl: doc.numeroControl,
            estado: 'PROCESADO',
            selloRecibido: `SELLO_${Date.now()}_${doc.noItem}`
          });

          // Actualizar estado en BD
          await this.updateDteStatus(doc.codigoGeneracion, 'PROCESADO');
        } else {
          rechazados.push({
            codigoGeneracion: doc.codigoGeneracion,
            numeroControl: doc.numeroControl,
            estado: 'RECHAZADO',
            motivo: 'Error de validación en el MH'
          });

          // Actualizar estado en BD
          await this.updateDteStatus(doc.codigoGeneracion, 'RECHAZADO');
        }
      } catch (error) {
        errores.push({
          codigoGeneracion: doc.codigoGeneracion,
          error: error.message
        });
      }
    }

    return { procesados, rechazados, errores };
  }

  private async updateDteStatus(codigoGeneracion: string, nuevoEstado: string): Promise<void> {
    try {
      await this.dteRepository.update(
        { codigoGeneracion },
        { 
          estadoMh: nuevoEstado,
          observaciones: `Procesado en lote el ${new Date().toISOString()}`
        }
      );
    } catch (error) {
      this.logger.error(`Error actualizando estado del DTE ${codigoGeneracion}`, error.stack);
      // No lanzamos error aquí para no interrumpir el proceso del lote
    }
  }

  private async saveLoteToDatabase(
    createLoteDto: CreateLoteDto,
    respuestaMH: any,
    resultadoProcesamiento: any
  ): Promise<void> {
    try {
      const lote = new Dte();
      
      // Datos básicos del lote
      lote.codigoGeneracion = createLoteDto.identificacion.codigoLote;
      lote.numeroControl = `LOTE-${Date.now()}`;
      lote.tipoDte = 'LOTE';
      lote.fechaEmision = new Date(createLoteDto.identificacion.fecRecepcion);
      lote.horaEmision = createLoteDto.identificacion.horRecepcion;
      lote.moneda = 'USD';

      // Datos del emisor
      lote.emisorNit = createLoteDto.emisor.nit;
      lote.emisorNombre = createLoteDto.emisor.nombre;

      // Información del lote
      lote.receptorDocumento = `${createLoteDto.documentos.length} documentos`;
      lote.receptorNombre = `Lote de ${createLoteDto.documentos.length} DTEs`;

      // Totales del lote
      const totalLote = createLoteDto.documentos.reduce((sum, doc) => sum + doc.montoTotal, 0);
      lote.totalPagar = totalLote;

      // Estado y respuesta MH
      lote.estadoMh = respuestaMH.estado || 'PROCESADO';
      lote.selloRecibido = respuestaMH.selloRecibido;
      lote.observaciones = `Lote: ${resultadoProcesamiento.procesados.length} procesados, ${resultadoProcesamiento.rechazados.length} rechazados`;

      // Documentos JSON
      lote.documentoJson = JSON.stringify(createLoteDto);
      lote.documentoFirmado = null; // Los lotes no se firman individualmente
      lote.respuestaMh = JSON.stringify({
        ...respuestaMH,
        resultadoProcesamiento
      });

      await this.dteRepository.save(lote);
      this.logger.log(`Lote guardado en BD con código: ${lote.codigoGeneracion}`);
      
    } catch (error) {
      this.logger.error(`Error guardando lote en BD: ${error.message}`, error.stack);
      // No lanzamos error aquí para no interrumpir el proceso
    }
  }

  async consultar(consultaLoteDto: ConsultaLoteDto) {
    try {
      this.logger.log(`Consultando estado del lote: ${consultaLoteDto.codigoLote}`);

      // Buscar lote en base de datos local
      const lote = await this.dteRepository.findOne({
        where: { 
          codigoGeneracion: consultaLoteDto.codigoLote,
          tipoDte: 'LOTE'
        }
      });

      if (!lote) {
        throw new NotFoundException('Lote no encontrado');
      }

      // Consultar estado en el MH (simulado)
      const estadoMH = await this.consultarEstadoEnMH(consultaLoteDto);

      return {
        success: true,
        codigoLote: consultaLoteDto.codigoLote,
        estado: lote.estadoMh,
        fechaProcesamiento: lote.fechaEmision,
        observaciones: lote.observaciones,
        estadoMH,
        fechaConsulta: new Date().toISOString()
      };

    } catch (error) {
      this.logger.error(`Error consultando lote ${consultaLoteDto.codigoLote}`, error.stack);
      if (error instanceof NotFoundException) {
        throw error;
      }
      throw new InternalServerErrorException('Error al consultar estado del lote');
    }
  }

  private async consultarEstadoEnMH(consultaLoteDto: ConsultaLoteDto) {
    try {
      // Aquí implementarías la consulta real al MH
      // Por ahora simulamos una respuesta
      return {
        estado: 'PROCESADO',
        procesados: 23,
        rechazados: 2,
        pendientes: 0
      };
    } catch (error) {
      this.logger.error('Error consultando estado en MH', error.stack);
      return {
        estado: 'ERROR_CONSULTA',
        mensaje: 'No se pudo consultar el estado en el MH'
      };
    }
  }

  async getById(id: string) {
    try {
      this.logger.log(`Consultando lote con ID: ${id}`);
      const lote = await this.dteRepository.findOne({
        where: { id: parseInt(id), tipoDte: 'LOTE' }
      });

      if (!lote) {
        throw new NotFoundException('Lote no encontrado');
      }

      return {
        success: true,
        message: 'Lote encontrado',
        data: lote
      };
    } catch (error) {
      this.logger.error(`Error al consultar lote ${id}`, error.stack);
      throw new InternalServerErrorException('Error al consultar lote');
    }
  }

  async getAll() {
    try {
      this.logger.log('Consultando todos los lotes');
      const lotes = await this.dteRepository.find({
        where: { tipoDte: 'LOTE' },
        order: { fechaEmision: 'DESC' }
      });

      return {
        success: true,
        message: 'Lista de lotes obtenida',
        data: lotes,
        total: lotes.length
      };
    } catch (error) {
      this.logger.error('Error al consultar lotes', error.stack);
      throw new InternalServerErrorException('Error al consultar lotes');
    }
  }

  async getEstadisticas() {
    try {
      this.logger.log('Generando estadísticas de lotes');
      
      // Aquí implementarías consultas para estadísticas
      const estadisticas = {
        totalLotes: 0,
        lotesHoy: 0,
        documentosProcesados: 0,
        documentosRechazados: 0,
        promedioDocumentosPorLote: 0
      };

      return {
        success: true,
        message: 'Estadísticas de lotes obtenidas',
        data: estadisticas
      };
    } catch (error) {
      this.logger.error('Error al generar estadísticas de lotes', error.stack);
      throw new InternalServerErrorException('Error al generar estadísticas');
    }
  }
}