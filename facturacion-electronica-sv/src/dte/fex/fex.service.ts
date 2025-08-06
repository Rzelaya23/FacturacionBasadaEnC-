import { Injectable, Logger, BadRequestException, InternalServerErrorException } from '@nestjs/common';
import { CreateFexDto } from './dto/create-fex.dto';
import { CodigoGeneracionService } from '../../common/services/codigo-generacion.service';
import { FirmadorService } from '../../firmador/firmador.service';
import { MhIntegrationService } from '../../mh-integration/mh-integration.service';

@Injectable()
export class FexService {
  private readonly logger = new Logger(FexService.name);

  constructor(
    private readonly codigoGeneracionService: CodigoGeneracionService,
    private readonly firmadorService: FirmadorService,
    private readonly mhIntegrationService: MhIntegrationService,
  ) {}

  async create(createFexDto: CreateFexDto) {
    try {
      this.logger.log('Iniciando creación de FEX (Factura de Exportación)');

      // 1. Validar datos de entrada
      this.validateFexData(createFexDto);

      // 2. Generar códigos automáticos si no están presentes
      if (!createFexDto.identificacion.codigoGeneracion) {
        const identificacion = this.codigoGeneracionService.generateIdentificacionCompleta('11');
        createFexDto.identificacion = { ...createFexDto.identificacion, ...identificacion };
      }

      // 3. Preparar el documento FEX
      const fexDocument = this.buildFexDocument(createFexDto);

      // 4. Firmar el documento
      this.logger.log('Firmando documento FEX');
      const documentoFirmado = await this.firmadorService.signFEX(fexDocument);

      // 5. Enviar al Ministerio de Hacienda
      this.logger.log('Enviando FEX al Ministerio de Hacienda');
      const respuestaMH = await this.mhIntegrationService.sendDTE(documentoFirmado.body);

      this.logger.log('FEX creado exitosamente');
      return {
        success: true,
        codigoGeneracion: createFexDto.identificacion.codigoGeneracion,
        numeroControl: createFexDto.identificacion.numeroControl,
        selloRecibido: respuestaMH.selloRecibido,
        fhProcesamiento: respuestaMH.fhProcesamiento,
        clasificaMsg: respuestaMH.clasificaMsg,
        codigoMsg: respuestaMH.codigoMsg,
        descripcionMsg: respuestaMH.descripcionMsg,
        observaciones: respuestaMH.observaciones
      };

    } catch (error) {
      this.logger.error('Error al crear FEX', error.stack);
      if (error instanceof BadRequestException) {
        throw error;
      }
      throw new InternalServerErrorException('Error interno al procesar FEX');
    }
  }


  private validateTotales(createFexDto: CreateFexDto): void {
    const { cuerpoDocumento, resumen } = createFexDto;

    // Calcular total gravado
    const totalGravadoCalculado = cuerpoDocumento.reduce((sum, item) => sum + item.ventaGravada, 0);
    
    if (Math.abs(totalGravadoCalculado - resumen.totalGravada) > 0.01) {
      throw new BadRequestException(
        `El total gravado no coincide. Calculado: ${totalGravadoCalculado.toFixed(2)}, Declarado: ${resumen.totalGravada.toFixed(2)}`
      );
    }

    // Validar descuentos
    const totalDescuentoCalculado = cuerpoDocumento.reduce((sum, item) => sum + (item.montoDescu || 0), 0);
    const totalDescuentoDeclarado = resumen.totalDescu || 0;
    
    if (Math.abs(totalDescuentoCalculado - totalDescuentoDeclarado) > 0.01) {
      throw new BadRequestException(
        `El total de descuento no coincide. Calculado: ${totalDescuentoCalculado.toFixed(2)}, Declarado: ${totalDescuentoDeclarado.toFixed(2)}`
      );
    }

    // Validar monto total de operación
    const montoTotalCalculado = resumen.totalGravada - (resumen.totalDescu || 0);
    if (Math.abs(montoTotalCalculado - resumen.montoTotalOperacion) > 0.01) {
      throw new BadRequestException(
        `El monto total de operación no coincide. Calculado: ${montoTotalCalculado.toFixed(2)}, Declarado: ${resumen.montoTotalOperacion.toFixed(2)}`
      );
    }

    // Para exportación, el total a pagar debe incluir flete y seguro si aplican
    const totalPagarCalculado = resumen.montoTotalOperacion + (resumen.flete || 0) + (resumen.seguro || 0);
    if (Math.abs(totalPagarCalculado - resumen.totalPagar) > 0.01) {
      throw new BadRequestException(
        `El total a pagar no coincide. Calculado: ${totalPagarCalculado.toFixed(2)}, Declarado: ${resumen.totalPagar.toFixed(2)}`
      );
    }
  }

  private validateExportData(createFexDto: CreateFexDto): void {
    // Validar emisor exportador
    const emisor = createFexDto.emisor;
    if (!emisor.tipoItemExpor || emisor.tipoItemExpor < 1 || emisor.tipoItemExpor > 3) {
      throw new BadRequestException('Tipo de ítem de exportación debe ser 1, 2 o 3');
    }

    // Validar receptor (importador)
    const receptor = createFexDto.receptor;
    if (!receptor.codPais || receptor.codPais.length !== 2) {
      throw new BadRequestException('Código de país del receptor debe tener 2 caracteres');
    }

    if (!receptor.tipoPersona || (receptor.tipoPersona !== 1 && receptor.tipoPersona !== 2)) {
      throw new BadRequestException('Tipo de persona debe ser 1 (Natural) o 2 (Jurídica)');
    }

    // Validar Incoterms si están presentes
    const resumen = createFexDto.resumen;
    if (resumen.codIncoterms && !resumen.descIncoterms) {
      throw new BadRequestException('Si se especifica código Incoterms, debe incluir la descripción');
    }

    if (resumen.descIncoterms && !resumen.codIncoterms) {
      throw new BadRequestException('Si se especifica descripción Incoterms, debe incluir el código');
    }
  }

  private buildFexDocument(createFexDto: CreateFexDto) {
    return {
      identificacion: createFexDto.identificacion,
      emisor: createFexDto.emisor,
      receptor: createFexDto.receptor,
      otrosDocumentos: createFexDto.otrosDocumentos || null,
      ventaTercero: createFexDto.ventaTercero || null,
      cuerpoDocumento: createFexDto.cuerpoDocumento,
      resumen: createFexDto.resumen,
      apendice: createFexDto.apendice || null,
    };
  }

  async getById(id: string) {
    try {
      this.logger.log(`Consultando FEX con ID: ${id}`);
      return {
        success: true,
        message: 'FEX encontrado',
        data: {
          id,
          estado: 'PROCESADO',
          fechaCreacion: new Date().toISOString()
        }
      };
    } catch (error) {
      this.logger.error(`Error al consultar FEX ${id}`, error.stack);
      throw new InternalServerErrorException('Error al consultar FEX');
    }
  }

  async getAll() {
    try {
      this.logger.log('Consultando todos los FEX');
      return {
        success: true,
        message: 'Lista de FEX obtenida',
        data: [],
        total: 0
      };
    } catch (error) {
      this.logger.error('Error al consultar FEX', error.stack);
      throw new InternalServerErrorException('Error al consultar FEX');
    }
  }

  async invalidate(id: string) {
    try {
      this.logger.log(`Invalidando FEX con ID: ${id}`);
      return {
        success: true,
        message: 'FEX invalidado correctamente',
        data: {
          id,
          estado: 'INVALIDADO',
          fechaInvalidacion: new Date().toISOString()
        }
      };
    } catch (error) {
      this.logger.error(`Error al invalidar FEX ${id}`, error.stack);
      throw new InternalServerErrorException('Error al invalidar FEX');
    }
  }

  async getEstado(codigoGeneracion: string) {
    try {
      this.logger.log(`Consultando estado de FEX: ${codigoGeneracion}`);
      return {
        success: true,
        codigoGeneracion,
        estado: 'PROCESADO',
        descripcion: 'Estado consultado exitosamente',
        fechaConsulta: new Date().toISOString()
      };
    } catch (error) {
      this.logger.error(`Error al consultar estado de FEX ${codigoGeneracion}`, error.stack);
      throw new InternalServerErrorException('Error al consultar estado del FEX');
    }
  }

  async getExample() {
    return {
      identificacion: {
        version: 1,
        ambiente: "00",
        tipoDte: "11",
        numeroControl: "DTE-11-00000001-000000000000001",
        tipoModelo: 1,
        tipoOperacion: 1,
        fecEmi: new Date().toISOString().split('T')[0],
        horEmi: new Date().toTimeString().split(' ')[0],
        tipoMoneda: "USD"
      },
      emisor: {
        nit: "02101601014",
        nrc: "240372-1",
        nombre: "EXPORTADORA EJEMPLO S.A. DE C.V.",
        codActividad: "46900",
        descActividad: "Comercio al por mayor no especializado",
        nombreComercial: "EXPORTADORA EJEMPLO",
        tipoEstablecimiento: "01",
        direccion: {
          departamento: "06",
          municipio: "01",
          complemento: "Zona Franca, Edificio Exportaciones #100"
        },
        telefono: "2234-5678",
        correo: "exportaciones@empresaejemplo.com",
        tipoItemExpor: 1,
        recintoFiscal: "Aeropuerto Internacional",
        regimen: "Exportación Definitiva"
      },
      receptor: {
        nombre: "INTERNATIONAL BUYER CORP",
        codPais: "US",
        nombrePais: "Estados Unidos",
        complemento: "123 Main Street, Miami, FL 33101",
        tipoDocumento: "37",
        numDocumento: "TAX123456789",
        nombreComercial: "BUYER CORP",
        tipoPersona: 2,
        descActividad: "Importación y distribución",
        telefono: "+1-305-123-4567",
        correo: "imports@buyercorp.com"
      },
      cuerpoDocumento: [
        {
          numItem: 1,
          codigo: "CAFE001",
          descripcion: "Café grano oro exportación",
          cantidad: 1000,
          uniMedida: 99,
          precioUni: 5.50,
          montoDescu: 0.00,
          ventaGravada: 5500.00,
          tributos: [],
          noGravado: 0.00
        }
      ],
      resumen: {
        totalGravada: 5500.00,
        descuento: 0.00,
        porcentajeDescuento: 0.00,
        totalDescu: 0.00,
        montoTotalOperacion: 5500.00,
        totalNoGravado: 0.00,
        totalPagar: 5500.00,
        totalLetras: "CINCO MIL QUINIENTOS 00/100 DÓLARES",
        condicionOperacion: 1,
        codIncoterms: "FOB",
        descIncoterms: "Free On Board",
        observaciones: "Exportación de café premium",
        flete: 200.00,
        seguro: 50.00
      }
    };
  }

  validateFexData(createFexDto: CreateFexDto): void {
    // Validar que hay al menos un ítem
    if (!createFexDto.cuerpoDocumento || createFexDto.cuerpoDocumento.length === 0) {
      throw new BadRequestException('Debe incluir al menos un ítem en el cuerpo del documento');
    }

    // Validar tipo de DTE
    if (createFexDto.identificacion.tipoDte !== '11') {
      throw new BadRequestException('El tipo de DTE debe ser "11" para Factura de Exportación');
    }

    // Validar receptor (importador)
    this.validateReceptorExportacion(createFexDto.receptor);

    // Validar emisor exportador
    this.validateEmisorExportacion(createFexDto.emisor);

    // Validar ítems del cuerpo del documento
    this.validateCuerpoDocumentoFex(createFexDto.cuerpoDocumento);

    // Validar resumen de totales
    this.validateResumenFex(createFexDto.resumen, createFexDto.cuerpoDocumento);
  }

  private validateReceptorExportacion(receptor: any): void {
    // Validar código de país
    if (!receptor.codPais || receptor.codPais.length !== 2) {
      throw new BadRequestException('Código de país del receptor debe tener 2 caracteres');
    }

    // Validar que no sea El Salvador
    if (receptor.codPais === 'SV') {
      throw new BadRequestException('Para exportación, el receptor no puede ser de El Salvador');
    }

    // Validar tipo de persona para exportación
    const tiposPersonaValidos = [1, 2]; // 1=Natural, 2=Jurídica
    if (!tiposPersonaValidos.includes(receptor.tipoPersona)) {
      throw new BadRequestException('Tipo de persona del receptor inválido para exportación');
    }
  }

  private validateEmisorExportacion(emisor: any): void {
    // Validar tipo de ítem de exportación
    const tiposItemExpor = [1, 2, 3]; // 1=Bienes, 2=Servicios, 3=Ambos
    if (!tiposItemExpor.includes(emisor.tipoItemExpor)) {
      throw new BadRequestException('Tipo de ítem de exportación inválido');
    }

    // Validar que tenga información de exportación
    if (!emisor.recintoFiscal && !emisor.regimen) {
      throw new BadRequestException('Debe especificar recinto fiscal o régimen de exportación');
    }
  }

  private validateCuerpoDocumentoFex(cuerpoDocumento: any[]): void {
    cuerpoDocumento.forEach((item, index) => {
      // Validar que el precio unitario sea mayor a 0
      if (item.precioUni <= 0) {
        throw new BadRequestException(`El precio unitario del ítem ${index + 1} debe ser mayor a 0`);
      }

      // Validar que la cantidad sea mayor a 0
      if (item.cantidad <= 0) {
        throw new BadRequestException(`La cantidad del ítem ${index + 1} debe ser mayor a 0`);
      }

      // Validar cálculo de venta gravada
      const ventaCalculada = (item.cantidad * item.precioUni) - (item.montoDescu || 0);
      if (Math.abs(ventaCalculada - item.ventaGravada) > 0.01) {
        throw new BadRequestException(
          `La venta gravada del ítem ${index + 1} no coincide. Calculado: ${ventaCalculada.toFixed(2)}, Declarado: ${item.ventaGravada.toFixed(2)}`
        );
      }
    });
  }

  private validateResumenFex(resumen: any, cuerpoDocumento: any[]): void {
    // Calcular total gravada
    const totalGravadaCalculada = cuerpoDocumento.reduce((sum, item) => sum + item.ventaGravada, 0);
    if (Math.abs(totalGravadaCalculada - resumen.totalGravada) > 0.01) {
      throw new BadRequestException(
        `El total gravada no coincide. Calculado: ${totalGravadaCalculada.toFixed(2)}, Declarado: ${resumen.totalGravada.toFixed(2)}`
      );
    }

    // Validar monto total de operación
    const montoTotalCalculado = resumen.totalGravada - (resumen.totalDescu || 0) + (resumen.totalNoGravado || 0);
    if (Math.abs(montoTotalCalculado - resumen.montoTotalOperacion) > 0.01) {
      throw new BadRequestException(
        `El monto total de operación no coincide. Calculado: ${montoTotalCalculado.toFixed(2)}, Declarado: ${resumen.montoTotalOperacion.toFixed(2)}`
      );
    }

    // Validar total a pagar (para exportación normalmente es igual al monto total)
    if (Math.abs(resumen.montoTotalOperacion - resumen.totalPagar) > 0.01) {
      throw new BadRequestException(
        `El total a pagar no coincide con el monto total de operación. Total: ${resumen.montoTotalOperacion.toFixed(2)}, A pagar: ${resumen.totalPagar.toFixed(2)}`
      );
    }

    // Validar Incoterms si están presentes
    if (resumen.codIncoterms) {
      const incotermsValidos = ['EXW', 'FCA', 'CPT', 'CIP', 'DAT', 'DAP', 'DDP', 'FAS', 'FOB', 'CFR', 'CIF'];
      if (!incotermsValidos.includes(resumen.codIncoterms)) {
        throw new BadRequestException(`Código Incoterms inválido: ${resumen.codIncoterms}`);
      }
    }
  }
}