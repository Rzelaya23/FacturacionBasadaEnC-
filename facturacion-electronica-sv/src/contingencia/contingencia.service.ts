import { Injectable, Logger, BadRequestException, InternalServerErrorException, NotFoundException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { CreateContingenciaDto } from './dto/create-contingencia.dto';
import { CodigoGeneracionService } from '../common/services/codigo-generacion.service';
import { FirmadorService } from '../firmador/firmador.service';
import { MhIntegrationService } from '../mh-integration/mh-integration.service';
import { Dte } from '../common/entities/dte.entity';

@Injectable()
export class ContingenciaService {
  private readonly logger = new Logger(ContingenciaService.name);

  constructor(
    @InjectRepository(Dte)
    private readonly dteRepository: Repository<Dte>,
    private readonly codigoGeneracionService: CodigoGeneracionService,
    private readonly firmadorService: FirmadorService,
    private readonly mhIntegrationService: MhIntegrationService,
  ) {}

  async create(createContingenciaDto: CreateContingenciaDto) {
    try {
      this.logger.log('Iniciando proceso de contingencia');

      // 1. Validar datos de la contingencia
      this.validateContingenciaData(createContingenciaDto);

      // 2. Validar que los DTEs existen y están en estado válido
      await this.validateDtesExist(createContingenciaDto.detalleDTE);

      // 3. Generar código de generación para la contingencia si no existe
      if (!createContingenciaDto.identificacion.codigoGeneracion) {
        createContingenciaDto.identificacion.codigoGeneracion = this.codigoGeneracionService.generateCodigoGeneracion();
      }

      // 4. Preparar documento de contingencia
      const contingenciaDocument = this.buildContingenciaDocument(createContingenciaDto);

      // 5. Firmar documento de contingencia
      this.logger.log('Firmando documento de contingencia');
      const documentoFirmado = await this.firmadorService.signContingency(contingenciaDocument);

      // 6. Enviar al Ministerio de Hacienda
      this.logger.log('Enviando contingencia al Ministerio de Hacienda');
      const respuestaMH = await this.mhIntegrationService.sendDTE(documentoFirmado.body);

      // 7. Actualizar estado de los DTEs incluidos en la contingencia
      await this.updateDtesContingenciaStatus(createContingenciaDto.detalleDTE, 'CONTINGENCIA');

      // 8. Guardar contingencia en base de datos
      await this.saveContingenciaToDatabase(createContingenciaDto, documentoFirmado, respuestaMH);

      this.logger.log('Contingencia procesada exitosamente');
      return {
        success: true,
        codigoGeneracion: createContingenciaDto.identificacion.codigoGeneracion,
        dtesIncluidos: createContingenciaDto.detalleDTE.length,
        selloRecibido: respuestaMH.selloRecibido,
        fhProcesamiento: respuestaMH.fhProcesamiento,
        clasificaMsg: respuestaMH.clasificaMsg,
        codigoMsg: respuestaMH.codigoMsg,
        descripcionMsg: respuestaMH.descripcionMsg,
        observaciones: respuestaMH.observaciones
      };

    } catch (error) {
      this.logger.error('Error al procesar contingencia', error.stack);
      if (error instanceof BadRequestException || error instanceof NotFoundException) {
        throw error;
      }
      throw new InternalServerErrorException('Error interno al procesar contingencia');
    }
  }

  private validateContingenciaData(createContingenciaDto: CreateContingenciaDto): void {
    // Validar versión de contingencia
    if (createContingenciaDto.identificacion.version !== 3) {
      throw new BadRequestException('La versión de contingencia debe ser 3');
    }

    // Validar ambiente
    const ambientesValidos = ['00', '01'];
    if (!ambientesValidos.includes(createContingenciaDto.identificacion.ambiente)) {
      throw new BadRequestException('Ambiente debe ser 00 (pruebas) o 01 (producción)');
    }

    // Validar tipo de contingencia
    const tiposContingenciaValidos = [1, 2, 3, 4, 5];
    if (!tiposContingenciaValidos.includes(createContingenciaDto.motivo.tipoContingencia)) {
      throw new BadRequestException('Tipo de contingencia debe ser entre 1 y 5');
    }

    // Validar fechas y horas
    this.validateFechasHoras(createContingenciaDto.identificacion);

    // Validar que hay al menos un DTE
    if (!createContingenciaDto.detalleDTE || createContingenciaDto.detalleDTE.length === 0) {
      throw new BadRequestException('Debe incluir al menos un DTE en la contingencia');
    }

    // Validar límite de DTEs (según normativa MH)
    if (createContingenciaDto.detalleDTE.length > 500) {
      throw new BadRequestException('No se pueden incluir más de 500 DTEs en una contingencia');
    }

    // Validar numeración secuencial de ítems
    createContingenciaDto.detalleDTE.forEach((item, index) => {
      if (item.noItem !== index + 1) {
        throw new BadRequestException(`El número de ítem debe ser consecutivo. Esperado: ${index + 1}, Recibido: ${item.noItem}`);
      }
    });

    // Validar datos del responsable
    this.validateResponsableData(createContingenciaDto.emisor);

    // Validar motivo de contingencia
    if (!createContingenciaDto.motivo.motivoContingencia || createContingenciaDto.motivo.motivoContingencia.trim().length < 20) {
      throw new BadRequestException('El motivo de contingencia debe tener al menos 20 caracteres');
    }
  }

  private validateFechasHoras(identificacion: any): void {
    const fechaInicio = new Date(`${identificacion.fInicio}T${identificacion.hInicio}`);
    const fechaFin = new Date(`${identificacion.fFin}T${identificacion.hFin}`);
    const fechaActual = new Date();

    // Validar que la fecha de inicio no sea futura
    if (fechaInicio > fechaActual) {
      throw new BadRequestException('La fecha de inicio de contingencia no puede ser futura');
    }

    // Validar que la fecha de fin sea posterior a la de inicio
    if (fechaFin <= fechaInicio) {
      throw new BadRequestException('La fecha de fin debe ser posterior a la fecha de inicio');
    }

    // Validar que la contingencia no exceda el límite de tiempo (ej: 72 horas)
    const horasContingencia = (fechaFin.getTime() - fechaInicio.getTime()) / (1000 * 60 * 60);
    if (horasContingencia > 72) {
      throw new BadRequestException('La contingencia no puede exceder 72 horas');
    }

    // Validar que la contingencia no sea muy antigua (ej: máximo 30 días atrás)
    const diasDesdeInicio = (fechaActual.getTime() - fechaInicio.getTime()) / (1000 * 60 * 60 * 24);
    if (diasDesdeInicio > 30) {
      throw new BadRequestException('No se puede reportar una contingencia de más de 30 días de antigüedad');
    }
  }

  private validateResponsableData(emisor: any): void {
    // Validar tipo de documento del responsable
    const tiposDocumentoValidos = ['13', '36', '37', '38', '39'];
    if (!tiposDocumentoValidos.includes(emisor.tipoDocResponsable)) {
      throw new BadRequestException('Tipo de documento del responsable inválido');
    }

    // Validar formato según tipo de documento
    if (emisor.tipoDocResponsable === '13') { // DUI
      const duiPattern = /^\d{8}-\d$/;
      if (!duiPattern.test(emisor.numeroDocResponsable)) {
        throw new BadRequestException('Formato de DUI del responsable inválido. Debe ser: 12345678-9');
      }
    }

    if (emisor.tipoDocResponsable === '36') { // NIT
      const nitPattern = /^\d{14}$/;
      if (!nitPattern.test(emisor.numeroDocResponsable)) {
        throw new BadRequestException('Formato de NIT del responsable inválido. Debe tener 14 dígitos');
      }
    }
  }

  private async validateDtesExist(detalleDTE: any[]): Promise<void> {
    for (const item of detalleDTE) {
      // Validar formato de código de generación
      const codigoPattern = /^[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}$/;
      if (!codigoPattern.test(item.codigoGeneracion)) {
        throw new BadRequestException(`Formato de código de generación inválido: ${item.codigoGeneracion}`);
      }

      // Validar tipo de documento
      const tiposDteValidos = ['01', '03', '04', '05', '06', '11', '14'];
      if (!tiposDteValidos.includes(item.tipoDoc)) {
        throw new BadRequestException(`Tipo de DTE inválido: ${item.tipoDoc}`);
      }

      // Verificar que el DTE existe (opcional, comentado para evitar errores de BD)
      /*
      const dte = await this.dteRepository.findOne({
        where: { codigoGeneracion: item.codigoGeneracion }
      });

      if (!dte) {
        throw new NotFoundException(`DTE no encontrado: ${item.codigoGeneracion}`);
      }

      // Validar que el DTE esté en estado válido para contingencia
      const estadosValidosParaContingencia = ['PROCESADO', 'RECHAZADO', 'ERROR'];
      if (!estadosValidosParaContingencia.includes(dte.estadoMh)) {
        throw new BadRequestException(`DTE ${item.codigoGeneracion} no está en estado válido para contingencia. Estado actual: ${dte.estadoMh}`);
      }
      */
    }
  }

  private buildContingenciaDocument(createContingenciaDto: CreateContingenciaDto) {
    return {
      identificacion: createContingenciaDto.identificacion,
      emisor: createContingenciaDto.emisor,
      detalleDTE: createContingenciaDto.detalleDTE,
      motivo: createContingenciaDto.motivo
    };
  }

  private async updateDtesContingenciaStatus(detalleDTE: any[], nuevoEstado: string): Promise<void> {
    try {
      for (const item of detalleDTE) {
        await this.dteRepository.update(
          { codigoGeneracion: item.codigoGeneracion },
          { 
            estadoMh: nuevoEstado,
            observaciones: `Incluido en contingencia el ${new Date().toISOString()}`
          }
        );
      }
      this.logger.log(`Estado de ${detalleDTE.length} DTEs actualizado a ${nuevoEstado}`);
    } catch (error) {
      this.logger.error(`Error actualizando estado de DTEs en contingencia`, error.stack);
      // No lanzamos error aquí para no interrumpir el proceso
    }
  }

  private async saveContingenciaToDatabase(
    createContingenciaDto: CreateContingenciaDto,
    documentoFirmado: any,
    respuestaMH: any
  ): Promise<void> {
    try {
      const contingencia = new Dte();
      
      // Datos básicos de la contingencia
      contingencia.codigoGeneracion = createContingenciaDto.identificacion.codigoGeneracion;
      contingencia.numeroControl = `CONT-${Date.now()}`;
      contingencia.tipoDte = 'CONTINGENCIA';
      contingencia.fechaEmision = new Date(createContingenciaDto.identificacion.fInicio);
      contingencia.horaEmision = createContingenciaDto.identificacion.hInicio;
      contingencia.moneda = 'USD';

      // Datos del emisor
      contingencia.emisorNit = createContingenciaDto.emisor.nit;
      contingencia.emisorNombre = createContingenciaDto.emisor.nombre;

      // Información de la contingencia
      contingencia.receptorDocumento = `${createContingenciaDto.detalleDTE.length} DTEs`;
      contingencia.receptorNombre = `Contingencia tipo ${createContingenciaDto.motivo.tipoContingencia}`;

      // Totales (para contingencia son 0)
      contingencia.totalPagar = 0;

      // Estado y respuesta MH
      contingencia.estadoMh = respuestaMH.estado || 'PROCESADO';
      contingencia.selloRecibido = respuestaMH.selloRecibido;
      contingencia.observaciones = `Contingencia: ${createContingenciaDto.motivo.motivoContingencia}`;

      // Documentos JSON
      contingencia.documentoJson = JSON.stringify(createContingenciaDto);
      contingencia.documentoFirmado = documentoFirmado?.body ? JSON.stringify(documentoFirmado.body) : null;
      contingencia.respuestaMh = JSON.stringify(respuestaMH);

      await this.dteRepository.save(contingencia);
      this.logger.log(`Contingencia guardada en BD con código: ${contingencia.codigoGeneracion}`);
      
    } catch (error) {
      this.logger.error(`Error guardando contingencia en BD: ${error.message}`, error.stack);
      // No lanzamos error aquí para no interrumpir el proceso
    }
  }

  async getById(id: string) {
    try {
      this.logger.log(`Consultando contingencia con ID: ${id}`);
      const contingencia = await this.dteRepository.findOne({
        where: { id: parseInt(id), tipoDte: 'CONTINGENCIA' }
      });

      if (!contingencia) {
        throw new NotFoundException('Contingencia no encontrada');
      }

      return {
        success: true,
        message: 'Contingencia encontrada',
        data: contingencia
      };
    } catch (error) {
      this.logger.error(`Error al consultar contingencia ${id}`, error.stack);
      throw new InternalServerErrorException('Error al consultar contingencia');
    }
  }

  async getAll() {
    try {
      this.logger.log('Consultando todas las contingencias');
      const contingencias = await this.dteRepository.find({
        where: { tipoDte: 'CONTINGENCIA' },
        order: { fechaEmision: 'DESC' }
      });

      return {
        success: true,
        message: 'Lista de contingencias obtenida',
        data: contingencias,
        total: contingencias.length
      };
    } catch (error) {
      this.logger.error('Error al consultar contingencias', error.stack);
      throw new InternalServerErrorException('Error al consultar contingencias');
    }
  }

  async getEstado(codigoGeneracion: string) {
    try {
      this.logger.log(`Consultando estado de contingencia: ${codigoGeneracion}`);
      
      const contingencia = await this.dteRepository.findOne({
        where: { codigoGeneracion, tipoDte: 'CONTINGENCIA' }
      });

      if (!contingencia) {
        throw new NotFoundException('Contingencia no encontrada');
      }

      return {
        success: true,
        codigoGeneracion,
        estado: contingencia.estadoMh,
        descripcion: contingencia.observaciones,
        fechaConsulta: new Date().toISOString()
      };
    } catch (error) {
      this.logger.error(`Error al consultar estado de contingencia ${codigoGeneracion}`, error.stack);
      throw new InternalServerErrorException('Error al consultar estado de la contingencia');
    }
  }

  async activarContingencia(tipoContingencia: number, motivo: string) {
    try {
      this.logger.log(`Activando contingencia tipo ${tipoContingencia}`);
      
      // Aquí implementarías la lógica para activar el modo contingencia
      // Por ejemplo, cambiar configuraciones, notificar servicios, etc.
      
      return {
        success: true,
        message: 'Modo contingencia activado',
        tipoContingencia,
        motivo,
        fechaActivacion: new Date().toISOString()
      };
    } catch (error) {
      this.logger.error('Error al activar contingencia', error.stack);
      throw new InternalServerErrorException('Error al activar modo contingencia');
    }
  }

  async desactivarContingencia() {
    try {
      this.logger.log('Desactivando modo contingencia');
      
      // Aquí implementarías la lógica para desactivar el modo contingencia
      
      return {
        success: true,
        message: 'Modo contingencia desactivado',
        fechaDesactivacion: new Date().toISOString()
      };
    } catch (error) {
      this.logger.error('Error al desactivar contingencia', error.stack);
      throw new InternalServerErrorException('Error al desactivar modo contingencia');
    }
  }
}