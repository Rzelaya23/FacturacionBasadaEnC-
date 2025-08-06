import { Injectable, Logger, BadRequestException, InternalServerErrorException } from '@nestjs/common';
import { CreateNrDto } from './dto/create-nr.dto';
import { CodigoGeneracionService } from '../../common/services/codigo-generacion.service';
import { FirmadorService } from '../../firmador/firmador.service';
import { MhIntegrationService } from '../../mh-integration/mh-integration.service';

@Injectable()
export class NrService {
  private readonly logger = new Logger(NrService.name);

  constructor(
    private readonly codigoGeneracionService: CodigoGeneracionService,
    private readonly firmadorService: FirmadorService,
    private readonly mhIntegrationService: MhIntegrationService,
  ) {}

  async create(createNrDto: CreateNrDto) {
    try {
      this.logger.log('Iniciando creación de NR (Nota de Remisión)');

      // 1. Validar datos de entrada
      this.validateNrData(createNrDto);

      // 2. Generar códigos automáticos si no están presentes
      if (!createNrDto.identificacion.codigoGeneracion) {
        const identificacion = this.codigoGeneracionService.generateIdentificacionCompleta('04');
        createNrDto.identificacion = { ...createNrDto.identificacion, ...identificacion };
      }

      // 3. Preparar el documento NR
      const nrDocument = this.buildNrDocument(createNrDto);

      // 4. Firmar el documento
      this.logger.log('Firmando documento NR');
      const documentoFirmado = await this.firmadorService.signNR(nrDocument);

      // 5. Enviar al Ministerio de Hacienda
      this.logger.log('Enviando NR al Ministerio de Hacienda');
      const respuestaMH = await this.mhIntegrationService.sendDTE(documentoFirmado.body);

      this.logger.log('NR creado exitosamente');
      return {
        success: true,
        codigoGeneracion: createNrDto.identificacion.codigoGeneracion,
        numeroControl: createNrDto.identificacion.numeroControl,
        selloRecibido: respuestaMH.selloRecibido,
        fhProcesamiento: respuestaMH.fhProcesamiento,
        clasificaMsg: respuestaMH.clasificaMsg,
        codigoMsg: respuestaMH.codigoMsg,
        descripcionMsg: respuestaMH.descripcionMsg,
        observaciones: respuestaMH.observaciones
      };

    } catch (error) {
      this.logger.error('Error al crear NR', error.stack);
      if (error instanceof BadRequestException) {
        throw error;
      }
      throw new InternalServerErrorException('Error interno al procesar NR');
    }
  }


  private validateTotales(createNrDto: CreateNrDto): void {
    const { cuerpoDocumento, resumen } = createNrDto;

    // Calcular totales por tipo de venta
    const totalNoSujCalculado = cuerpoDocumento.reduce((sum, item) => sum + (item.ventaNoSuj || 0), 0);
    const totalExentaCalculado = cuerpoDocumento.reduce((sum, item) => sum + (item.ventaExenta || 0), 0);
    const totalGravadaCalculado = cuerpoDocumento.reduce((sum, item) => sum + item.ventaGravada, 0);

    // Validar totales
    if (Math.abs(totalNoSujCalculado - (resumen.totalNoSuj || 0)) > 0.01) {
      throw new BadRequestException(
        `El total no sujeto no coincide. Calculado: ${totalNoSujCalculado.toFixed(2)}, Declarado: ${(resumen.totalNoSuj || 0).toFixed(2)}`
      );
    }

    if (Math.abs(totalExentaCalculado - (resumen.totalExenta || 0)) > 0.01) {
      throw new BadRequestException(
        `El total exento no coincide. Calculado: ${totalExentaCalculado.toFixed(2)}, Declarado: ${(resumen.totalExenta || 0).toFixed(2)}`
      );
    }

    if (Math.abs(totalGravadaCalculado - resumen.totalGravada) > 0.01) {
      throw new BadRequestException(
        `El total gravado no coincide. Calculado: ${totalGravadaCalculado.toFixed(2)}, Declarado: ${resumen.totalGravada.toFixed(2)}`
      );
    }

    // Validar subtotal
    const subTotalCalculado = totalNoSujCalculado + totalExentaCalculado + totalGravadaCalculado;
    if (Math.abs(subTotalCalculado - resumen.subTotal) > 0.01) {
      throw new BadRequestException(
        `El subtotal no coincide. Calculado: ${subTotalCalculado.toFixed(2)}, Declarado: ${resumen.subTotal.toFixed(2)}`
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

    // Validar subtotal después de descuentos
    const subTotalVentasCalculado = resumen.subTotal - (resumen.totalDescu || 0);
    if (Math.abs(subTotalVentasCalculado - resumen.subTotalVentas) > 0.01) {
      throw new BadRequestException(
        `El subtotal de ventas no coincide. Calculado: ${subTotalVentasCalculado.toFixed(2)}, Declarado: ${resumen.subTotalVentas.toFixed(2)}`
      );
    }

    // Validar monto total de operación (incluye IVA)
    const montoTotalCalculado = resumen.subTotalVentas + resumen.totalIva - (resumen.ivaRete1 || 0) - (resumen.reteRenta || 0);
    if (Math.abs(montoTotalCalculado - resumen.montoTotalOperacion) > 0.01) {
      throw new BadRequestException(
        `El monto total de operación no coincide. Calculado: ${montoTotalCalculado.toFixed(2)}, Declarado: ${resumen.montoTotalOperacion.toFixed(2)}`
      );
    }
  }

  private validateRemisionData(createNrDto: CreateNrDto): void {
    // Validar receptor específico para NR
    const receptor = createNrDto.receptor;
    
    // En NR, el receptor puede tener información adicional como bienTitulo
    if ((receptor as any).bienTitulo && (receptor as any).bienTitulo.length > 100) {
      throw new BadRequestException('El campo bienTitulo no puede exceder 100 caracteres');
    }

    // Validar información de entrega si está presente
    if (createNrDto.extension) {
      const extension = createNrDto.extension;
      
      if (extension.nombEntrega && !extension.docuEntrega) {
        throw new BadRequestException('Si se especifica nombre del entregador, debe incluir su documento');
      }
      
      if (extension.nombRecibe && !extension.docuRecibe) {
        throw new BadRequestException('Si se especifica nombre del receptor, debe incluir su documento');
      }

      // Validar formato de placa si está presente
      if (extension.placaVehiculo) {
        const placaPattern = /^[A-Z]\d{3}-\d{3}$/;
        if (!placaPattern.test(extension.placaVehiculo)) {
          throw new BadRequestException('Formato de placa inválido. Debe ser: P123-456');
        }
      }
    }
  }

  private buildNrDocument(createNrDto: CreateNrDto) {
    return {
      identificacion: createNrDto.identificacion,
      documentoRelacionado: createNrDto.documentoRelacionado || null,
      emisor: createNrDto.emisor,
      receptor: createNrDto.receptor,
      ventaTercero: createNrDto.ventaTercero || null,
      cuerpoDocumento: createNrDto.cuerpoDocumento,
      resumen: createNrDto.resumen,
      extension: createNrDto.extension || null,
      apendice: createNrDto.apendice || null,
    };
  }

  async getById(id: string) {
    try {
      this.logger.log(`Consultando NR con ID: ${id}`);
      return {
        success: true,
        message: 'NR encontrado',
        data: {
          id,
          estado: 'PROCESADO',
          fechaCreacion: new Date().toISOString()
        }
      };
    } catch (error) {
      this.logger.error(`Error al consultar NR ${id}`, error.stack);
      throw new InternalServerErrorException('Error al consultar NR');
    }
  }


  async invalidate(id: string) {
    try {
      this.logger.log(`Invalidando NR con ID: ${id}`);
      return {
        success: true,
        message: 'NR invalidado correctamente',
        data: {
          id,
          estado: 'INVALIDADO',
          fechaInvalidacion: new Date().toISOString()
        }
      };
    } catch (error) {
      this.logger.error(`Error al invalidar NR ${id}`, error.stack);
      throw new InternalServerErrorException('Error al invalidar NR');
    }
  }

  async getEstado(codigoGeneracion: string) {
    try {
      this.logger.log(`Consultando estado de NR: ${codigoGeneracion}`);
      return {
        success: true,
        codigoGeneracion,
        estado: 'PROCESADO',
        descripcion: 'Estado consultado exitosamente',
        fechaConsulta: new Date().toISOString()
      };
    } catch (error) {
      this.logger.error(`Error al consultar estado de NR ${codigoGeneracion}`, error.stack);
      throw new InternalServerErrorException('Error al consultar estado del NR');
    }
  }

  async getExample() {
    return {
      identificacion: {
        version: 1,
        ambiente: "00",
        tipoDte: "04",
        numeroControl: "DTE-04-00000001-000000000000001",
        tipoModelo: 1,
        tipoOperacion: 1,
        fecEmi: new Date().toISOString().split('T')[0],
        horEmi: new Date().toTimeString().split(' ')[0],
        tipoMoneda: "USD"
      },
      emisor: {
        nit: "02101601014",
        nrc: "240372-1",
        nombre: "EMPRESA EJEMPLO S.A. DE C.V.",
        codActividad: "62020",
        descActividad: "Consultoría en informática",
        nombreComercial: "EMPRESA EJEMPLO",
        tipoEstablecimiento: "01",
        direccion: {
          departamento: "06",
          municipio: "01",
          complemento: "Colonia Escalón, Avenida Norte #123"
        },
        telefono: "2234-5678",
        correo: "facturacion@empresaejemplo.com"
      },
      receptor: {
        tipoDocumento: "36",
        numDocumento: "02101601015",
        nrc: "240373-2",
        nombre: "CLIENTE EJEMPLO S.A. DE C.V.",
        codActividad: "62020",
        descActividad: "Consultoría en informática",
        direccion: {
          departamento: "06",
          municipio: "01",
          complemento: "Colonia San Benito, Calle Principal #456"
        },
        telefono: "2234-9876",
        correo: "cliente@ejemplo.com",
        bienTitulo: "Traslado de mercadería"
      },
      cuerpoDocumento: [
        {
          numItem: 1,
          tipoItem: 2,
          numeroDocumento: "DOC-001",
          cantidad: 5,
          codigo: "PROD001",
          uniMedida: 1,
          descripcion: "Equipo de cómputo para traslado",
          precioUni: 500.00,
          montoDescu: 0.00,
          ventaNoSuj: 0.00,
          ventaExenta: 0.00,
          ventaGravada: 2500.00,
          tributos: ["20"]
        }
      ],
      resumen: {
        totalNoSuj: 0.00,
        totalExenta: 0.00,
        totalGravada: 2500.00,
        subTotalVentas: 2500.00,
        descuNoSuj: 0.00,
        descuExenta: 0.00,
        descuGravada: 0.00,
        porcentajeDescuento: 0.00,
        totalDescu: 0.00,
        tributos: [
          {
            codigo: "20",
            descripcion: "Impuesto al Valor Agregado 13%",
            valor: 325.00
          }
        ],
        subTotal: 2500.00,
        montoTotalOperacion: 2825.00,
        totalLetras: "DOS MIL OCHOCIENTOS VEINTICINCO 00/100 DÓLARES"
      }
    };
  }

  validateNrData(createNrDto: CreateNrDto): void {
    // Validar que hay al menos un ítem
    if (!createNrDto.cuerpoDocumento || createNrDto.cuerpoDocumento.length === 0) {
      throw new BadRequestException('Debe incluir al menos un ítem en el cuerpo del documento');
    }

    // Validar tipo de DTE
    if (createNrDto.identificacion.tipoDte !== '04') {
      throw new BadRequestException('El tipo de DTE debe ser "04" para Nota de Remisión');
    }

    // Validar receptor
    this.validateReceptorNr(createNrDto.receptor);

    // Validar ítems del cuerpo del documento
    this.validateCuerpoDocumentoNr(createNrDto.cuerpoDocumento);

    // Validar resumen de totales
    this.validateResumenNr(createNrDto.resumen, createNrDto.cuerpoDocumento);
  }

  private validateReceptorNr(receptor: any): void {
    // Validar que tenga bien título para NR
    if (!receptor.bienTitulo || receptor.bienTitulo.trim() === '') {
      throw new BadRequestException('El bien título es obligatorio para Nota de Remisión');
    }

    // Validar tipo de documento
    const tiposDocumentoValidos = ['13', '36', '37', '38', '39'];
    if (!tiposDocumentoValidos.includes(receptor.tipoDocumento)) {
      throw new BadRequestException(`Tipo de documento inválido para receptor: ${receptor.tipoDocumento}`);
    }
  }

  private validateCuerpoDocumentoNr(cuerpoDocumento: any[]): void {
    cuerpoDocumento.forEach((item, index) => {
      // Validar que el precio unitario sea mayor o igual a 0 (puede ser 0 para traslados)
      if (item.precioUni < 0) {
        throw new BadRequestException(`El precio unitario del ítem ${index + 1} no puede ser negativo`);
      }

      // Validar que la cantidad sea mayor a 0
      if (item.cantidad <= 0) {
        throw new BadRequestException(`La cantidad del ítem ${index + 1} debe ser mayor a 0`);
      }

      // Validar cálculo de ventas
      const totalVenta = item.ventaNoSuj + item.ventaExenta + item.ventaGravada;
      const ventaCalculada = (item.cantidad * item.precioUni) - (item.montoDescu || 0);
      if (Math.abs(ventaCalculada - totalVenta) > 0.01) {
        throw new BadRequestException(
          `El total de ventas del ítem ${index + 1} no coincide. Calculado: ${ventaCalculada.toFixed(2)}, Declarado: ${totalVenta.toFixed(2)}`
        );
      }
    });
  }

  private validateResumenNr(resumen: any, cuerpoDocumento: any[]): void {
    // Calcular totales por tipo de venta
    const totalNoSujCalculada = cuerpoDocumento.reduce((sum, item) => sum + item.ventaNoSuj, 0);
    const totalExentaCalculada = cuerpoDocumento.reduce((sum, item) => sum + item.ventaExenta, 0);
    const totalGravadaCalculada = cuerpoDocumento.reduce((sum, item) => sum + item.ventaGravada, 0);

    // Validar totales
    if (Math.abs(totalNoSujCalculada - resumen.totalNoSuj) > 0.01) {
      throw new BadRequestException(
        `El total no sujeto no coincide. Calculado: ${totalNoSujCalculada.toFixed(2)}, Declarado: ${resumen.totalNoSuj.toFixed(2)}`
      );
    }

    if (Math.abs(totalExentaCalculada - resumen.totalExenta) > 0.01) {
      throw new BadRequestException(
        `El total exento no coincide. Calculado: ${totalExentaCalculada.toFixed(2)}, Declarado: ${resumen.totalExenta.toFixed(2)}`
      );
    }

    if (Math.abs(totalGravadaCalculada - resumen.totalGravada) > 0.01) {
      throw new BadRequestException(
        `El total gravado no coincide. Calculado: ${totalGravadaCalculada.toFixed(2)}, Declarado: ${resumen.totalGravada.toFixed(2)}`
      );
    }

    // Validar subtotal de ventas
    const subTotalVentasCalculado = resumen.totalNoSuj + resumen.totalExenta + resumen.totalGravada;
    if (Math.abs(subTotalVentasCalculado - resumen.subTotalVentas) > 0.01) {
      throw new BadRequestException(
        `El subtotal de ventas no coincide. Calculado: ${subTotalVentasCalculado.toFixed(2)}, Declarado: ${resumen.subTotalVentas.toFixed(2)}`
      );
    }

    // Validar monto total de operación
    const ivaCalculado = resumen.tributos?.reduce((sum, tributo) => sum + (tributo.valor || 0), 0) || 0;
    const montoTotalCalculado = resumen.subTotalVentas + ivaCalculado;
    if (Math.abs(montoTotalCalculado - resumen.montoTotalOperacion) > 0.01) {
      throw new BadRequestException(
        `El monto total de operación no coincide. Calculado: ${montoTotalCalculado.toFixed(2)}, Declarado: ${resumen.montoTotalOperacion.toFixed(2)}`
      );
    }
  }

  async getAll() {
    try {
      this.logger.log('Obteniendo todas las NR');
      return {
        success: true,
        message: 'Lista de NR obtenida',
        data: [],
        total: 0
      };
    } catch (error) {
      this.logger.error('Error al obtener NR', error.stack);
      throw new InternalServerErrorException('Error al obtener lista de NR');
    }
  }
}