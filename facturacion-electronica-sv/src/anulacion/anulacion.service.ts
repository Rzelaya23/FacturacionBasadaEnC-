import { Injectable, Logger, BadRequestException, InternalServerErrorException, NotFoundException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { CreateAnulacionDto } from './dto/create-anulacion.dto';
import { CodigoGeneracionService } from '../common/services/codigo-generacion.service';
import { FirmadorService } from '../firmador/firmador.service';
import { MhIntegrationService } from '../mh-integration/mh-integration.service';
import { Dte } from '../common/entities/dte.entity';

@Injectable()
export class AnulacionService {
  private readonly logger = new Logger(AnulacionService.name);

  constructor(
    @InjectRepository(Dte)
    private readonly dteRepository: Repository<Dte>,
    private readonly codigoGeneracionService: CodigoGeneracionService,
    private readonly firmadorService: FirmadorService,
    private readonly mhIntegrationService: MhIntegrationService,
  ) {}

  async create(createAnulacionDto: CreateAnulacionDto) {
    try {
      this.logger.log('Iniciando proceso de anulación de DTE');

      // 1. Validar que el documento existe y puede ser anulado
      await this.validateDocumentExists(createAnulacionDto.documento.codigoGeneracion);

      // 2. Validar datos de la anulación
      this.validateAnulacionData(createAnulacionDto);

      // 3. Generar código de generación para la anulación si no existe
      if (!createAnulacionDto.identificacion.codigoGeneracion) {
        createAnulacionDto.identificacion.codigoGeneracion = this.codigoGeneracionService.generateCodigoGeneracion();
      }

      // 4. Establecer fecha y hora si no están presentes
      if (!createAnulacionDto.identificacion.fecAnula) {
        const { fecEmi, horEmi } = this.codigoGeneracionService.generateFechaHoraEmision();
        createAnulacionDto.identificacion.fecAnula = fecEmi;
        createAnulacionDto.identificacion.horAnula = horEmi;
      }

      // 5. Preparar documento de anulación
      const anulacionDocument = this.buildAnulacionDocument(createAnulacionDto);

      // 6. Firmar documento de anulación
      this.logger.log('Firmando documento de anulación');
      const documentoFirmado = await this.firmadorService.signCancellation(anulacionDocument);

      // 7. Enviar al Ministerio de Hacienda
      this.logger.log('Enviando anulación al Ministerio de Hacienda');
      const respuestaMH = await this.mhIntegrationService.sendDTE(documentoFirmado.body);

      // 8. Actualizar estado del documento original
      await this.updateOriginalDocumentStatus(createAnulacionDto.documento.codigoGeneracion, 'ANULADO');

      // 9. Guardar anulación en base de datos
      await this.saveAnulacionToDatabase(createAnulacionDto, documentoFirmado, respuestaMH);

      this.logger.log('Anulación procesada exitosamente');
      return {
        success: true,
        codigoGeneracion: createAnulacionDto.identificacion.codigoGeneracion,
        codigoGeneracionOriginal: createAnulacionDto.documento.codigoGeneracion,
        selloRecibido: respuestaMH.selloRecibido,
        fhProcesamiento: respuestaMH.fhProcesamiento,
        clasificaMsg: respuestaMH.clasificaMsg,
        codigoMsg: respuestaMH.codigoMsg,
        descripcionMsg: respuestaMH.descripcionMsg,
        observaciones: respuestaMH.observaciones
      };

    } catch (error) {
      this.logger.error('Error al procesar anulación', error.stack);
      if (error instanceof BadRequestException || error instanceof NotFoundException) {
        throw error;
      }
      throw new InternalServerErrorException('Error interno al procesar anulación');
    }
  }

  private async validateDocumentExists(codigoGeneracion: string): Promise<Dte> {
    const documento = await this.dteRepository.findOne({
      where: { codigoGeneracion }
    });

    if (!documento) {
      throw new NotFoundException(`No se encontró el documento con código de generación: ${codigoGeneracion}`);
    }

    // Validar que el documento no esté ya anulado
    if (documento.estadoMh === 'ANULADO') {
      throw new BadRequestException('El documento ya se encuentra anulado');
    }

    // Validar que el documento esté en estado válido para anulación
    const estadosValidosParaAnulacion = ['PROCESADO', 'ACEPTADO', 'RECHAZADO'];
    if (!estadosValidosParaAnulacion.includes(documento.estadoMh)) {
      throw new BadRequestException(`El documento no puede ser anulado. Estado actual: ${documento.estadoMh}`);
    }

    // Validar que no hayan pasado más de X días (según normativa MH)
    const fechaEmision = new Date(documento.fechaEmision);
    const fechaActual = new Date();
    const diasTranscurridos = Math.floor((fechaActual.getTime() - fechaEmision.getTime()) / (1000 * 60 * 60 * 24));
    
    if (diasTranscurridos > 30) { // Ejemplo: 30 días límite
      throw new BadRequestException('No se puede anular un documento con más de 30 días de antigüedad');
    }

    return documento;
  }

  private validateAnulacionData(createAnulacionDto: CreateAnulacionDto): void {
    // Validar versión de anulación
    if (createAnulacionDto.identificacion.version !== 2) {
      throw new BadRequestException('La versión de anulación debe ser 2');
    }

    // Validar ambiente
    const ambientesValidos = ['00', '01'];
    if (!ambientesValidos.includes(createAnulacionDto.identificacion.ambiente)) {
      throw new BadRequestException('Ambiente debe ser 00 (pruebas) o 01 (producción)');
    }

    // Validar tipo de anulación
    const tiposAnulacionValidos = [1, 2, 3]; // Según normativa MH
    if (!tiposAnulacionValidos.includes(createAnulacionDto.motivo.tipoAnulacion)) {
      throw new BadRequestException('Tipo de anulación debe ser 1, 2 o 3');
    }

    // Validar motivo de anulación
    if (!createAnulacionDto.motivo.motivoAnulacion || createAnulacionDto.motivo.motivoAnulacion.trim().length < 10) {
      throw new BadRequestException('El motivo de anulación debe tener al menos 10 caracteres');
    }

    // Validar datos del responsable
    this.validateResponsableData(createAnulacionDto.motivo);

    // Validar formato de documentos
    this.validateDocumentFormats(createAnulacionDto);
  }

  private validateResponsableData(motivo: any): void {
    // Validar tipo de documento del responsable
    const tiposDocumentoValidos = ['13', '36', '37', '38', '39']; // DUI, NIT, etc.
    if (!tiposDocumentoValidos.includes(motivo.tipDocResponsable)) {
      throw new BadRequestException('Tipo de documento del responsable inválido');
    }

    // Validar formato según tipo de documento
    if (motivo.tipDocResponsable === '13') { // DUI
      const duiPattern = /^\d{8}-\d$/;
      if (!duiPattern.test(motivo.numDocResponsable)) {
        throw new BadRequestException('Formato de DUI del responsable inválido. Debe ser: 12345678-9');
      }
    }

    if (motivo.tipDocResponsable === '36') { // NIT
      const nitPattern = /^\d{14}$/;
      if (!nitPattern.test(motivo.numDocResponsable)) {
        throw new BadRequestException('Formato de NIT del responsable inválido. Debe tener 14 dígitos');
      }
    }
  }

  private validateDocumentFormats(createAnulacionDto: CreateAnulacionDto): void {
    // Validar código de generación
    const codigoPattern = /^[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}$/;
    if (!codigoPattern.test(createAnulacionDto.documento.codigoGeneracion)) {
      throw new BadRequestException('Formato de código de generación inválido');
    }

    // Validar número de control
    const numeroControlPattern = /^DTE-\d{2}-\d{4}-\d{4}-\d{7,15}$/;
    if (!numeroControlPattern.test(createAnulacionDto.documento.numeroControl)) {
      throw new BadRequestException('Formato de número de control inválido');
    }

    // Validar tipo de DTE
    const tiposDteValidos = ['01', '03', '04', '05', '06', '11', '14'];
    if (!tiposDteValidos.includes(createAnulacionDto.documento.tipoDte)) {
      throw new BadRequestException('Tipo de DTE inválido para anulación');
    }
  }

  private buildAnulacionDocument(createAnulacionDto: CreateAnulacionDto) {
    return {
      identificacion: createAnulacionDto.identificacion,
      emisor: createAnulacionDto.emisor,
      documento: createAnulacionDto.documento,
      motivo: createAnulacionDto.motivo
    };
  }

  private async updateOriginalDocumentStatus(codigoGeneracion: string, nuevoEstado: string): Promise<void> {
    try {
      await this.dteRepository.update(
        { codigoGeneracion },
        { 
          estadoMh: nuevoEstado,
          observaciones: `Documento anulado el ${new Date().toISOString()}`
        }
      );
      this.logger.log(`Estado del documento ${codigoGeneracion} actualizado a ${nuevoEstado}`);
    } catch (error) {
      this.logger.error(`Error actualizando estado del documento ${codigoGeneracion}`, error.stack);
      // No lanzamos error aquí para no interrumpir el proceso de anulación
    }
  }

  private async saveAnulacionToDatabase(
    createAnulacionDto: CreateAnulacionDto,
    documentoFirmado: any,
    respuestaMH: any
  ): Promise<void> {
    try {
      const anulacion = new Dte();
      
      // Datos básicos de la anulación
      anulacion.codigoGeneracion = createAnulacionDto.identificacion.codigoGeneracion;
      anulacion.numeroControl = `ANUL-${Date.now()}`; // Número de control para la anulación
      anulacion.tipoDte = 'ANULACION';
      anulacion.fechaEmision = new Date(createAnulacionDto.identificacion.fecAnula);
      anulacion.horaEmision = createAnulacionDto.identificacion.horAnula;
      anulacion.moneda = 'USD';

      // Datos del emisor
      anulacion.emisorNit = createAnulacionDto.emisor.nit;
      anulacion.emisorNombre = createAnulacionDto.emisor.nombre;

      // Datos del documento anulado
      anulacion.receptorDocumento = createAnulacionDto.documento.codigoGeneracion; // Referencia al documento original
      anulacion.receptorNombre = `Anulación de ${createAnulacionDto.documento.tipoDte}`;

      // Totales (para anulación son 0 o el monto anulado)
      anulacion.totalPagar = createAnulacionDto.documento.montoIva || 0;

      // Estado y respuesta MH
      anulacion.estadoMh = respuestaMH.estado || 'PROCESADO';
      anulacion.selloRecibido = respuestaMH.selloRecibido;
      anulacion.observaciones = `Anulación: ${createAnulacionDto.motivo.motivoAnulacion}`;

      // Documentos JSON
      anulacion.documentoJson = JSON.stringify(createAnulacionDto);
      anulacion.documentoFirmado = documentoFirmado?.body ? JSON.stringify(documentoFirmado.body) : null;
      anulacion.respuestaMh = JSON.stringify(respuestaMH);

      await this.dteRepository.save(anulacion);
      this.logger.log(`Anulación guardada en BD con código: ${anulacion.codigoGeneracion}`);
      
    } catch (error) {
      this.logger.error(`Error guardando anulación en BD: ${error.message}`, error.stack);
      // No lanzamos error aquí para no interrumpir el proceso
    }
  }

  async getById(id: string) {
    try {
      this.logger.log(`Consultando anulación con ID: ${id}`);
      const anulacion = await this.dteRepository.findOne({
        where: { id: parseInt(id) }
      });

      if (!anulacion) {
        throw new NotFoundException('Anulación no encontrada');
      }

      return {
        success: true,
        message: 'Anulación encontrada',
        data: anulacion
      };
    } catch (error) {
      this.logger.error(`Error al consultar anulación ${id}`, error.stack);
      throw new InternalServerErrorException('Error al consultar anulación');
    }
  }

  async getAll() {
    try {
      this.logger.log('Consultando todas las anulaciones');
      const anulaciones = await this.dteRepository.find({
        where: { tipoDte: 'ANULACION' },
        order: { fechaEmision: 'DESC' }
      });

      return {
        success: true,
        message: 'Lista de anulaciones obtenida',
        data: anulaciones,
        total: anulaciones.length
      };
    } catch (error) {
      this.logger.error('Error al consultar anulaciones', error.stack);
      throw new InternalServerErrorException('Error al consultar anulaciones');
    }
  }

  async getEstado(codigoGeneracion: string) {
    try {
      this.logger.log(`Consultando estado de anulación: ${codigoGeneracion}`);
      
      const anulacion = await this.dteRepository.findOne({
        where: { codigoGeneracion, tipoDte: 'ANULACION' }
      });

      if (!anulacion) {
        throw new NotFoundException('Anulación no encontrada');
      }

      return {
        success: true,
        codigoGeneracion,
        estado: anulacion.estadoMh,
        descripcion: anulacion.observaciones,
        fechaConsulta: new Date().toISOString()
      };
    } catch (error) {
      this.logger.error(`Error al consultar estado de anulación ${codigoGeneracion}`, error.stack);
      throw new InternalServerErrorException('Error al consultar estado de la anulación');
    }
  }

  async getAnulacionesByDocumento(codigoGeneracionOriginal: string) {
    try {
      this.logger.log(`Consultando anulaciones del documento: ${codigoGeneracionOriginal}`);
      
      const anulaciones = await this.dteRepository.find({
        where: { 
          tipoDte: 'ANULACION',
          receptorDocumento: codigoGeneracionOriginal 
        },
        order: { fechaEmision: 'DESC' }
      });

      return {
        success: true,
        message: 'Anulaciones del documento obtenidas',
        data: anulaciones,
        total: anulaciones.length
      };
    } catch (error) {
      this.logger.error(`Error al consultar anulaciones del documento ${codigoGeneracionOriginal}`, error.stack);
      throw new InternalServerErrorException('Error al consultar anulaciones del documento');
    }
  }
}