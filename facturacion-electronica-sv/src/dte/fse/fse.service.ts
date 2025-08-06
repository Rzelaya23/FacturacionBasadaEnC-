import { Injectable, Logger, BadRequestException, InternalServerErrorException } from '@nestjs/common';
import { CreateFseDto } from './dto/create-fse.dto';
import { CodigoGeneracionService } from '../../common/services/codigo-generacion.service';
import { FirmadorService } from '../../firmador/firmador.service';
import { MhIntegrationService } from '../../mh-integration/mh-integration.service';

@Injectable()
export class FseService {
  private readonly logger = new Logger(FseService.name);

  constructor(
    private readonly codigoGeneracionService: CodigoGeneracionService,
    private readonly firmadorService: FirmadorService,
    private readonly mhIntegrationService: MhIntegrationService,
  ) {}

  async create(createFseDto: CreateFseDto) {
    try {
      this.logger.log('Iniciando creación de FSE');

      // 1. Validar datos de entrada
      this.validateFseData(createFseDto);

      // 2. Generar códigos automáticos si no están presentes
      if (!createFseDto.identificacion.codigoGeneracion) {
        const identificacion = this.codigoGeneracionService.generateIdentificacionCompleta('14');
        createFseDto.identificacion = { ...createFseDto.identificacion, ...identificacion };
      }

      // 3. Preparar el documento FSE
      const fseDocument = this.buildFseDocument(createFseDto);

      // 4. Firmar el documento
      this.logger.log('Firmando documento FSE');
      const documentoFirmado = await this.firmadorService.signFSE(fseDocument);

      // 5. Enviar al Ministerio de Hacienda
      this.logger.log('Enviando FSE al Ministerio de Hacienda');
      const respuestaMH = await this.mhIntegrationService.sendDTE(documentoFirmado.body);

      this.logger.log('FSE creado exitosamente');
      return {
        success: true,
        codigoGeneracion: createFseDto.identificacion.codigoGeneracion,
        numeroControl: createFseDto.identificacion.numeroControl,
        selloRecibido: respuestaMH.selloRecibido,
        fhProcesamiento: respuestaMH.fhProcesamiento,
        clasificaMsg: respuestaMH.clasificaMsg,
        codigoMsg: respuestaMH.codigoMsg,
        descripcionMsg: respuestaMH.descripcionMsg,
        observaciones: respuestaMH.observaciones
      };

    } catch (error) {
      this.logger.error('Error al crear FSE', error.stack);
      if (error instanceof BadRequestException) {
        throw error;
      }
      throw new InternalServerErrorException('Error interno al procesar FSE');
    }
  }

  validateFseData(createFseDto: CreateFseDto): void {
    // Validar que hay al menos un ítem
    if (!createFseDto.cuerpoDocumento || createFseDto.cuerpoDocumento.length === 0) {
      throw new BadRequestException('Debe incluir al menos un ítem en el cuerpo del documento');
    }

    // Validar que los números de ítem son consecutivos
    createFseDto.cuerpoDocumento.forEach((item, index) => {
      if (item.numItem !== index + 1) {
        throw new BadRequestException(`El número de ítem debe ser consecutivo. Esperado: ${index + 1}, Recibido: ${item.numItem}`);
      }
    });

    // Validar totales
    this.validateTotales(createFseDto);

    // Validar sujeto excluido
    this.validateSujetoExcluido(createFseDto.sujetoExcluido);
  }

  private validateTotales(createFseDto: CreateFseDto): void {
    const { cuerpoDocumento, resumen } = createFseDto;

    // Calcular total de compras
    const totalCompraCalculado = cuerpoDocumento.reduce((sum, item) => sum + item.compra, 0);
    
    if (Math.abs(totalCompraCalculado - resumen.totalCompra) > 0.01) {
      throw new BadRequestException(
        `El total de compra no coincide. Calculado: ${totalCompraCalculado.toFixed(2)}, Declarado: ${resumen.totalCompra.toFixed(2)}`
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

    // Validar subtotal
    const subtotalCalculado = resumen.totalCompra - (resumen.totalDescu || 0);
    if (Math.abs(subtotalCalculado - resumen.subTotal) > 0.01) {
      throw new BadRequestException(
        `El subtotal no coincide. Calculado: ${subtotalCalculado.toFixed(2)}, Declarado: ${resumen.subTotal.toFixed(2)}`
      );
    }

    // Validar total a pagar
    const totalPagarCalculado = resumen.subTotal - (resumen.ivaRete1 || 0) - (resumen.reteRenta || 0);
    if (Math.abs(totalPagarCalculado - resumen.totalPagar) > 0.01) {
      throw new BadRequestException(
        `El total a pagar no coincide. Calculado: ${totalPagarCalculado.toFixed(2)}, Declarado: ${resumen.totalPagar.toFixed(2)}`
      );
    }
  }

  private validateSujetoExcluido(sujetoExcluido: any): void {
    // Validar tipo de documento
    const tiposDocumentoValidos = ['13', '36', '37', '38', '39'];
    if (!tiposDocumentoValidos.includes(sujetoExcluido.tipoDocumento)) {
      throw new BadRequestException(`Tipo de documento inválido para sujeto excluido: ${sujetoExcluido.tipoDocumento}`);
    }

    // Validar formato de documento según tipo
    if (sujetoExcluido.tipoDocumento === '13') { // DUI
      const duiPattern = /^\d{8}-\d$/;
      if (!duiPattern.test(sujetoExcluido.numDocumento)) {
        throw new BadRequestException('Formato de DUI inválido. Debe ser: 12345678-9');
      }
    }
  }

  private buildFseDocument(createFseDto: CreateFseDto) {
    return {
      identificacion: createFseDto.identificacion,
      emisor: createFseDto.emisor,
      sujetoExcluido: createFseDto.sujetoExcluido,
      cuerpoDocumento: createFseDto.cuerpoDocumento,
      resumen: createFseDto.resumen,
      apendice: createFseDto.apendice || null,
    };
  }

  async getById(id: string) {
    try {
      this.logger.log(`Consultando FSE con ID: ${id}`);
      // Aquí implementarías la lógica para consultar por ID
      // Por ahora retornamos un placeholder
      return {
        success: true,
        message: 'FSE encontrado',
        data: {
          id,
          estado: 'PROCESADO',
          fechaCreacion: new Date().toISOString()
        }
      };
    } catch (error) {
      this.logger.error(`Error al consultar FSE ${id}`, error.stack);
      throw new InternalServerErrorException('Error al consultar FSE');
    }
  }

  async getAll() {
    try {
      this.logger.log('Consultando todos los FSE');
      // Aquí implementarías la lógica para consultar todos
      // Por ahora retornamos un placeholder
      return {
        success: true,
        message: 'Lista de FSE obtenida',
        data: [],
        total: 0
      };
    } catch (error) {
      this.logger.error('Error al consultar FSE', error.stack);
      throw new InternalServerErrorException('Error al consultar FSE');
    }
  }

  async invalidate(id: string) {
    try {
      this.logger.log(`Invalidando FSE con ID: ${id}`);
      // Aquí implementarías la lógica para invalidar
      // Por ahora retornamos un placeholder
      return {
        success: true,
        message: 'FSE invalidado correctamente',
        data: {
          id,
          estado: 'INVALIDADO',
          fechaInvalidacion: new Date().toISOString()
        }
      };
    } catch (error) {
      this.logger.error(`Error al invalidar FSE ${id}`, error.stack);
      throw new InternalServerErrorException('Error al invalidar FSE');
    }
  }

  async getEstado(codigoGeneracion: string) {
    try {
      this.logger.log(`Consultando estado de FSE: ${codigoGeneracion}`);
      // TODO: Implementar consulta de estado al MH cuando esté disponible
      // const respuesta = await this.mhIntegrationService.checkDTEStatus(codigoGeneracion);
      
      return {
        success: true,
        codigoGeneracion,
        estado: 'PROCESADO', // Placeholder
        descripcion: 'Estado consultado exitosamente',
        fechaConsulta: new Date().toISOString()
      };
    } catch (error) {
      this.logger.error(`Error al consultar estado de FSE ${codigoGeneracion}`, error.stack);
      throw new InternalServerErrorException('Error al consultar estado del FSE');
    }
  }

  async getExample() {
    return {
      identificacion: {
        version: 1,
        ambiente: "00",
        tipoDte: "14",
        numeroControl: "DTE-14-00000001-000000000000001",
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
        direccion: {
          departamento: "06",
          municipio: "01",
          complemento: "Colonia Escalón, Avenida Norte #123"
        },
        telefono: "2234-5678",
        correo: "facturacion@empresaejemplo.com"
      },
      sujetoExcluido: {
        tipoDocumento: "13",
        numDocumento: "12345678-9",
        nombre: "Juan Carlos Pérez",
        codActividad: "01111",
        descActividad: "Cultivo de maíz",
        direccion: {
          departamento: "06",
          municipio: "01",
          complemento: "Cantón El Ejemplo, Caserío La Esperanza"
        },
        telefono: "7123-4567",
        correo: "juan.perez@email.com"
      },
      cuerpoDocumento: [
        {
          numItem: 1,
          tipoItem: 1,
          cantidad: 100,
          codigo: "MAIZ001",
          uniMedida: 99,
          descripcion: "Quintal de maíz blanco",
          precioUni: 25.00,
          montoDescu: 0.00,
          compra: 2500.00
        }
      ],
      resumen: {
        totalCompra: 2500.00,
        descu: 0.00,
        totalDescu: 0.00,
        subTotal: 2500.00,
        ivaRete1: 0.00,
        reteRenta: 0.00,
        totalPagar: 2500.00,
        totalLetras: "DOS MIL QUINIENTOS 00/100 DÓLARES",
        condicionOperacion: 1
      }
    };
  }

}