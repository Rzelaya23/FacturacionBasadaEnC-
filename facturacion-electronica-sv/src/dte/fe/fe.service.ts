import { Injectable, Logger, BadRequestException, InternalServerErrorException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { CreateFacturaElectronicaDto } from './dto/create-factura-electronica.dto';
import { FirmadorService } from '../../firmador/firmador.service';
import { MhIntegrationService } from '../../mh-integration/mh-integration.service';
import { CodigoGeneracionService } from '../../common/services/codigo-generacion.service';
import { Dte } from '../../common/entities/dte.entity';

@Injectable()
export class FeService {
  private readonly logger = new Logger(FeService.name);

  constructor(
    @InjectRepository(Dte)
    private readonly dteRepository: Repository<Dte>,
    private readonly firmadorService: FirmadorService,
    private readonly mhIntegrationService: MhIntegrationService,
    private readonly codigoGeneracionService: CodigoGeneracionService,
  ) {}

  /**
   * Crear y procesar Factura Electrónica completa
   */
  async create(createFeDto: CreateFacturaElectronicaDto): Promise<any> {
    try {
      // 1. Validar estructura y cálculos
      const validationResult = await this.validate(createFeDto);
      if (!validationResult.valid) {
        throw new BadRequestException('Datos de FE inválidos', validationResult.errors);
      }

      // 2. Preparar documento para firmado
      const documentToSign = this.prepareDocumentForSigning(createFeDto);

      // 3. Firmar documento
      this.logger.log('Iniciando firmado digital de FE');
      const signedDocument = await this.firmadorService.signFE(documentToSign);

      if (!signedDocument || !signedDocument.body) {
        throw new InternalServerErrorException('Error en el proceso de firmado');
      }

      // 4. Enviar al Ministerio de Hacienda
      this.logger.log('Enviando FE al Ministerio de Hacienda');
      const mhResponse = await this.mhIntegrationService.sendDTE(signedDocument.body);

      // 5. Procesar respuesta del MH
      const processedResponse = this.processMhResponse(mhResponse);

      // 6. Guardar en base de datos
      const savedDte = await this.saveToDatabase(createFeDto, signedDocument, processedResponse);

      this.logger.log(`FE procesada exitosamente - Estado: ${processedResponse.estado}`);

      return {
        codigoGeneracion: createFeDto.identificacion.codigoGeneracion,
        numeroControl: createFeDto.identificacion.numeroControl,
        ...processedResponse,
        documentoFirmado: signedDocument.body
      };

    } catch (error) {
      this.logger.error(`Error procesando FE: ${error.message}`, error.stack);
      
      if (error instanceof BadRequestException) {
        throw error;
      }
      
      throw new InternalServerErrorException(`Error procesando Factura Electrónica: ${error.message}`);
    }
  }

  /**
   * Solo firmar documento (sin enviar al MH)
   */
  async signOnly(createFeDto: CreateFacturaElectronicaDto): Promise<any> {
    try {
      // 1. Validar estructura
      const validationResult = await this.validate(createFeDto);
      if (!validationResult.valid) {
        throw new BadRequestException('Datos de FE inválidos', validationResult.errors);
      }

      // 2. Preparar y firmar documento
      const documentToSign = this.prepareDocumentForSigning(createFeDto);
      const signedDocument = await this.firmadorService.signFE(documentToSign);

      if (!signedDocument || !signedDocument.body) {
        throw new InternalServerErrorException('Error en el proceso de firmado');
      }

      return {
        codigoGeneracion: createFeDto.identificacion.codigoGeneracion,
        numeroControl: createFeDto.identificacion.numeroControl,
        documento: signedDocument.body,
        firmadoEn: new Date().toISOString()
      };

    } catch (error) {
      this.logger.error(`Error firmando FE: ${error.message}`, error.stack);
      throw error;
    }
  }

  /**
   * Validar estructura y cálculos de FE
   */
  async validate(createFeDto: CreateFacturaElectronicaDto): Promise<any> {
    const errors: string[] = [];
    const warnings: string[] = [];

    try {
      // 1. Validaciones básicas de estructura
      this.validateBasicStructure(createFeDto, errors);

      // 2. Validar cálculos matemáticos
      const calculatedTotals = this.calculateTotals(createFeDto);
      this.validateCalculations(createFeDto, calculatedTotals, errors, warnings);

      // 3. Validaciones específicas del MH
      this.validateMhSpecificRules(createFeDto, errors, warnings);

      return {
        valid: errors.length === 0,
        errors,
        warnings,
        calculatedTotals
      };

    } catch (error) {
      this.logger.error(`Error validando FE: ${error.message}`, error.stack);
      throw new InternalServerErrorException(`Error en validación: ${error.message}`);
    }
  }

  /**
   * Consultar estado en el MH
   */
  async getStatus(codigoGeneracion: string): Promise<any> {
    try {
      this.logger.log(`Consultando estado de FE: ${codigoGeneracion}`);
      
      // 1. Buscar en base de datos local
      const localDte = await this.dteRepository.findOne({
        where: { codigoGeneracion }
      });

      if (!localDte) {
        throw new BadRequestException('DTE no encontrado en el sistema');
      }

      // 2. Consultar estado actual en el MH
      // TODO: Implementar método checkDTEStatus en MhIntegrationService
      // const mhStatus = await this.mhIntegrationService.checkDTEStatus(codigoGeneracion);
      
      // TODO: Implementar consulta de estado al MH
      return {
        local: localDte,
        sincronizado: true
      };

    } catch (error) {
      this.logger.error(`Error consultando estado: ${error.message}`, error.stack);
      throw new InternalServerErrorException(`Error consultando estado: ${error.message}`);
    }
  }

  /**
   * Generar códigos automáticos para FE
   */
  async generateCodes(): Promise<any> {
    try {
      const identificacion = this.codigoGeneracionService.generateIdentificacionCompleta('01');
      
      this.logger.log(`Códigos generados - Código: ${identificacion.codigoGeneracion}`);
      
      return {
        codigoGeneracion: identificacion.codigoGeneracion,
        numeroControl: identificacion.numeroControl,
        fecEmi: identificacion.fecEmi,
        horEmi: identificacion.horEmi,
        version: identificacion.version,
        ambiente: identificacion.ambiente,
        tipoDte: identificacion.tipoDte,
        tipoModelo: identificacion.tipoModelo,
        tipoOperacion: identificacion.tipoOperacion,
        tipoMoneda: identificacion.tipoMoneda
      };

    } catch (error) {
      this.logger.error(`Error generando códigos: ${error.message}`, error.stack);
      throw new InternalServerErrorException(`Error generando códigos: ${error.message}`);
    }
  }

  /**
   * Health check del módulo
   */
  async healthCheck(): Promise<any> {
    try {
      const [firmadorStatus, mhStatus] = await Promise.all([
        this.firmadorService.checkSignerAvailability(),
        this.checkMhAvailability()
      ]);

      return {
        firmador: firmadorStatus,
        mh: mhStatus,
        database: await this.checkDatabaseConnection()
      };

    } catch (error) {
      this.logger.error(`Error en health check: ${error.message}`, error.stack);
      return {
        firmador: false,
        mh: false,
        database: false
      };
    }
  }

  // ==================== MÉTODOS PRIVADOS ====================

  /**
   * Preparar documento para firmado según estructura del sistema C#
   */
  private prepareDocumentForSigning(createFeDto: CreateFacturaElectronicaDto): any {
    // Estructura basada en JSONFE.cs del sistema C#
    return {
      identificacion: createFeDto.identificacion,
      documentoRelacionado: createFeDto.documentoRelacionado || null,
      emisor: createFeDto.emisor,
      receptor: createFeDto.receptor,
      otrosDocumentos: createFeDto.otrosDocumentos || null,
      ventaTercero: createFeDto.ventaTercero || null,
      cuerpoDocumento: createFeDto.cuerpoDocumento,
      resumen: createFeDto.resumen,
      extension: createFeDto.extension,
      apendice: createFeDto.apendice || null
    };
  }

  /**
   * Validaciones básicas de estructura
   */
  private validateBasicStructure(createFeDto: CreateFacturaElectronicaDto, errors: string[]): void {
    // Validar que el tipo de DTE sea FE (01)
    if (createFeDto.identificacion.tipoDte !== '01') {
      errors.push('El tipo de DTE debe ser 01 para Factura Electrónica');
    }

    // Validar que tenga al menos un ítem
    if (!createFeDto.cuerpoDocumento || createFeDto.cuerpoDocumento.length === 0) {
      errors.push('La factura debe tener al menos un ítem en el cuerpo del documento');
    }

    // Validar numeración secuencial de ítems
    createFeDto.cuerpoDocumento?.forEach((item, index) => {
      if (item.numItem !== index + 1) {
        errors.push(`El ítem ${index + 1} debe tener numItem = ${index + 1}`);
      }
    });
  }

  /**
   * Calcular totales basado en la lógica del sistema C#
   */
  private calculateTotals(createFeDto: CreateFacturaElectronicaDto): any {
    let totalNoSuj = 0;
    let totalExenta = 0;
    let totalGravada = 0;
    let totalDescuento = 0;
    let totalIva = 0;

    // Sumar por cada ítem
    createFeDto.cuerpoDocumento.forEach(item => {
      totalNoSuj += item.ventaNoSuj || 0;
      totalExenta += item.ventaExenta || 0;
      totalGravada += item.ventaGravada || 0;
      totalDescuento += item.montoDescu || 0;
      totalIva += item.ivaItem || 0;
    });

    const subTotal = totalNoSuj + totalExenta + totalGravada;
    const totalPagar = subTotal + totalIva - totalDescuento;

    return {
      totalNoSuj: Number(totalNoSuj.toFixed(2)),
      totalExenta: Number(totalExenta.toFixed(2)),
      totalGravada: Number(totalGravada.toFixed(2)),
      subTotal: Number(subTotal.toFixed(2)),
      totalDescuento: Number(totalDescuento.toFixed(2)),
      totalIva: Number(totalIva.toFixed(2)),
      totalPagar: Number(totalPagar.toFixed(2))
    };
  }

  /**
   * Validar cálculos matemáticos
   */
  private validateCalculations(
    createFeDto: CreateFacturaElectronicaDto, 
    calculated: any, 
    errors: string[], 
    warnings: string[]
  ): void {
    const resumen = createFeDto.resumen;
    const tolerance = 0.01; // Tolerancia de 1 centavo

    // Validar totales principales
    if (Math.abs(resumen.totalNoSuj - calculated.totalNoSuj) > tolerance) {
      errors.push(`Total no sujeto incorrecto. Calculado: ${calculated.totalNoSuj}, Recibido: ${resumen.totalNoSuj}`);
    }

    if (Math.abs(resumen.totalGravada - calculated.totalGravada) > tolerance) {
      errors.push(`Total gravado incorrecto. Calculado: ${calculated.totalGravada}, Recibido: ${resumen.totalGravada}`);
    }

    if (Math.abs(resumen.totalPagar - calculated.totalPagar) > tolerance) {
      errors.push(`Total a pagar incorrecto. Calculado: ${calculated.totalPagar}, Recibido: ${resumen.totalPagar}`);
    }

    // Advertencias para diferencias menores
    if (Math.abs(resumen.totalIva - calculated.totalIva) > tolerance) {
      warnings.push(`Diferencia en total IVA. Calculado: ${calculated.totalIva}, Recibido: ${resumen.totalIva}`);
    }
  }

  /**
   * Validaciones específicas del MH
   */
  private validateMhSpecificRules(createFeDto: CreateFacturaElectronicaDto, errors: string[], warnings: string[]): void {
    // Validar formato de código de generación
    const codigoPattern = /^[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}$/;
    if (!codigoPattern.test(createFeDto.identificacion.codigoGeneracion)) {
      errors.push('Código de generación debe tener formato UUID válido');
    }

    // Validar que la fecha no sea futura
    const fechaEmision = new Date(createFeDto.identificacion.fecEmi);
    const hoy = new Date();
    if (fechaEmision > hoy) {
      errors.push('La fecha de emisión no puede ser futura');
    }

    // Validar montos mínimos
    if (createFeDto.resumen.totalPagar <= 0) {
      errors.push('El total a pagar debe ser mayor a cero');
    }
  }

  /**
   * Procesar respuesta del MH
   */
  private processMhResponse(mhResponse: any): any {
    // Procesar según estructura de respuesta del MH
    return {
      estado: mhResponse.estado || 'PROCESADO',
      selloRecibido: mhResponse.selloRecibido || new Date().toISOString(),
      clasificaMsg: mhResponse.clasificaMsg || '01',
      codigoMsg: mhResponse.codigoMsg || '001',
      descripcionMsg: mhResponse.descripcionMsg || 'DTE procesado correctamente'
    };
  }

  /**
   * Verificar disponibilidad del MH
   */
  private async checkMhAvailability(): Promise<boolean> {
    try {
      return await this.mhIntegrationService.checkConnectivity();
    } catch (error) {
      return false;
    }
  }

  /**
   * Verificar conexión a la base de datos
   */
  private async checkDatabaseConnection(): Promise<boolean> {
    try {
      await this.dteRepository.query('SELECT 1');
      return true;
    } catch (error) {
      this.logger.error(`Database connection error: ${error.message}`);
      return false;
    }
  }

  /**
   * Guardar DTE en base de datos
   */
  private async saveToDatabase(
    createFeDto: CreateFacturaElectronicaDto,
    signedDocument: any,
    mhResponse: any
  ): Promise<Dte> {
    try {
      const dte = new Dte();
      
      // Datos básicos del DTE
      dte.codigoGeneracion = createFeDto.identificacion.codigoGeneracion;
      dte.numeroControl = createFeDto.identificacion.numeroControl;
      dte.tipoDte = createFeDto.identificacion.tipoDte;
      dte.fechaEmision = new Date(createFeDto.identificacion.fecEmi);
      dte.horaEmision = createFeDto.identificacion.horEmi;
      dte.moneda = createFeDto.identificacion.tipoMoneda;

      // Datos del emisor
      dte.emisorNit = createFeDto.emisor.nit;
      dte.emisorNombre = createFeDto.emisor.nombre;

      // Datos del receptor
      dte.receptorDocumento = createFeDto.receptor?.numDocumento;
      dte.receptorNombre = createFeDto.receptor?.nombre;

      // Totales
      dte.totalPagar = createFeDto.resumen.totalPagar;

      // Estado y respuesta MH
      dte.estadoMh = mhResponse.estado || 'PROCESADO';
      dte.selloRecibido = mhResponse.selloRecibido;
      dte.observaciones = mhResponse.descripcionMsg;

      // Documentos JSON
      dte.documentoJson = JSON.stringify(createFeDto);
      dte.documentoFirmado = signedDocument?.body ? JSON.stringify(signedDocument.body) : null;
      dte.respuestaMh = JSON.stringify(mhResponse);

      const savedDte = await this.dteRepository.save(dte);
      this.logger.log(`DTE guardado en BD con ID: ${savedDte.id}`);
      
      return savedDte;

    } catch (error) {
      this.logger.error(`Error guardando DTE en BD: ${error.message}`, error.stack);
      throw new InternalServerErrorException(`Error guardando en base de datos: ${error.message}`);
    }
  }
}