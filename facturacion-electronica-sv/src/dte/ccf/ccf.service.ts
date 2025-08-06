import { Injectable, Logger, BadRequestException, InternalServerErrorException } from '@nestjs/common';
import { CreateCcfDto } from './dto/create-ccf.dto';
import { FirmadorService } from '../../firmador/firmador.service';
import { MhIntegrationService } from '../../mh-integration/mh-integration.service';
import { CodigoGeneracionService } from '../../common/services/codigo-generacion.service';

@Injectable()
export class CcfService {
  private readonly logger = new Logger(CcfService.name);

  constructor(
    private readonly firmadorService: FirmadorService,
    private readonly mhIntegrationService: MhIntegrationService,
    private readonly codigoGeneracionService: CodigoGeneracionService,
  ) {}

  /**
   * Genera un CCF completo: valida, firma y envía al MH
   */
  async generarCcf(createCcfDto: CreateCcfDto): Promise<any> {
    this.logger.log('Iniciando generación completa de CCF');

    try {
      // 1. Validar datos
      const validacion = await this.validarDatos(createCcfDto);
      if (!validacion.valido) {
        throw new BadRequestException(`Datos inválidos: ${validacion.errores.join(', ')}`);
      }

      // 2. Generar código de generación si no existe
      if (!createCcfDto.identificacion.codigoGeneracion) {
        createCcfDto.identificacion.codigoGeneracion = this.codigoGeneracionService.generateCodigoGeneracion();
      }

      // 3. Firmar documento
      const documentoFirmado = await this.firmadorService.signCCF(createCcfDto);

      // 4. Enviar al MH
      const respuestaMh = await this.mhIntegrationService.sendDTE(documentoFirmado);

      this.logger.log(`CCF generado exitosamente: ${createCcfDto.identificacion.codigoGeneracion}`);

      return {
        codigoGeneracion: createCcfDto.identificacion.codigoGeneracion,
        documento: documentoFirmado,
        ...respuestaMh
      };

    } catch (error) {
      this.logger.error(`Error generando CCF: ${error.message}`, error.stack);
      throw new InternalServerErrorException(`Error generando CCF: ${error.message}`);
    }
  }

  /**
   * Firma un CCF sin enviarlo al MH
   */
  async firmarCcf(createCcfDto: CreateCcfDto): Promise<any> {
    this.logger.log('Iniciando firma de CCF');

    try {
      // Validar datos
      const validacion = await this.validarDatos(createCcfDto);
      if (!validacion.valido) {
        throw new BadRequestException(`Datos inválidos: ${validacion.errores.join(', ')}`);
      }

      // Generar código de generación si no existe
      if (!createCcfDto.identificacion.codigoGeneracion) {
        createCcfDto.identificacion.codigoGeneracion = this.codigoGeneracionService.generateCodigoGeneracion();
      }

      // Firmar documento
      const documentoFirmado = await this.firmadorService.signCCF(createCcfDto);

      this.logger.log(`CCF firmado exitosamente: ${createCcfDto.identificacion.codigoGeneracion}`);

      return {
        codigoGeneracion: createCcfDto.identificacion.codigoGeneracion,
        documento: documentoFirmado
      };

    } catch (error) {
      this.logger.error(`Error firmando CCF: ${error.message}`, error.stack);
      throw new InternalServerErrorException(`Error firmando CCF: ${error.message}`);
    }
  }

  /**
   * Envía un CCF ya firmado al MH
   */
  async enviarCcfAlMh(documento: string, codigoGeneracion: string): Promise<any> {
    this.logger.log(`Enviando CCF al MH: ${codigoGeneracion}`);

    try {
      const respuestaMh = await this.mhIntegrationService.sendDTE(documento);

      this.logger.log(`CCF enviado exitosamente al MH: ${codigoGeneracion}`);

      return respuestaMh;

    } catch (error) {
      this.logger.error(`Error enviando CCF al MH: ${error.message}`, error.stack);
      throw new InternalServerErrorException(`Error enviando CCF al MH: ${error.message}`);
    }
  }

  /**
   * Consulta el estado de un CCF en el MH
   */
  async consultarEstado(codigoGeneracion: string): Promise<any> {
    this.logger.log(`Consultando estado de CCF: ${codigoGeneracion}`);

    try {
      // Por ahora simulamos la consulta ya que no hay endpoint específico
      const estado = {
        estado: 'PROCESADO',
        fhProcesamiento: new Date().toISOString(),
        clasificaMsg: '01',
        codigoMsg: '001',
        descripcionMsg: 'Procesado Correctamente'
      };

      this.logger.log(`Estado consultado exitosamente: ${codigoGeneracion}`);

      return estado;

    } catch (error) {
      this.logger.error(`Error consultando estado de CCF: ${error.message}`, error.stack);
      throw new InternalServerErrorException(`Error consultando estado de CCF: ${error.message}`);
    }
  }

  /**
   * Valida los datos de un CCF
   */
  async validarDatos(createCcfDto: CreateCcfDto): Promise<{ valido: boolean; errores: string[]; advertencias: string[] }> {
    const errores: string[] = [];
    const advertencias: string[] = [];

    try {
      // Validaciones específicas para CCF
      
      // 1. Validar identificación
      if (!createCcfDto.identificacion) {
        errores.push('La identificación es requerida');
      } else {
        if (createCcfDto.identificacion.tipoDte !== '03') {
          errores.push('El tipo de DTE debe ser 03 para CCF');
        }
      }

      // 2. Validar emisor
      if (!createCcfDto.emisor) {
        errores.push('Los datos del emisor son requeridos');
      } else {
        if (!createCcfDto.emisor.nit) {
          errores.push('El NIT del emisor es requerido para CCF');
        }
      }

      // 3. Validar receptor
      if (!createCcfDto.receptor) {
        errores.push('Los datos del receptor son requeridos');
      } else {
        if (!createCcfDto.receptor.numDocumento) {
          errores.push('El número de documento del receptor es requerido');
        }
      }

      // 4. Validar cuerpo del documento
      if (!createCcfDto.cuerpoDocumento || createCcfDto.cuerpoDocumento.length === 0) {
        errores.push('El cuerpo del documento es requerido');
      } else {
        createCcfDto.cuerpoDocumento.forEach((item, index) => {
          if (item.numItem !== index + 1) {
            errores.push(`El ítem ${index + 1} tiene numeración incorrecta`);
          }
          if (item.ventaGravada < 0 || item.ventaExenta < 0 || item.ventaNoSuj < 0) {
            errores.push(`El ítem ${index + 1} tiene montos negativos`);
          }
        });
      }

      // 5. Validar resumen
      if (!createCcfDto.resumen) {
        errores.push('El resumen es requerido');
      } else {
        if (createCcfDto.resumen.totalPagar <= 0) {
          errores.push('El total a pagar debe ser mayor a cero');
        }
        if (!createCcfDto.resumen.totalLetras) {
          errores.push('El total en letras es requerido');
        }
        if (!createCcfDto.resumen.tributos || createCcfDto.resumen.tributos.length === 0) {
          advertencias.push('No se han definido tributos en el resumen');
        }
      }

      // 6. Validaciones de coherencia
      if (createCcfDto.cuerpoDocumento && createCcfDto.resumen) {
        const totalCalculado = createCcfDto.cuerpoDocumento.reduce((sum, item) => 
          sum + item.ventaGravada + item.ventaExenta + item.ventaNoSuj, 0
        );
        
        const totalResumen = createCcfDto.resumen.totalGravada + 
                           createCcfDto.resumen.totalExenta + 
                           createCcfDto.resumen.totalNoSuj;

        if (Math.abs(totalCalculado - totalResumen) > 0.01) {
          errores.push('Los totales del cuerpo y resumen no coinciden');
        }
      }

      const valido = errores.length === 0;

      this.logger.log(`Validación de CCF completada. Válido: ${valido}, Errores: ${errores.length}, Advertencias: ${advertencias.length}`);

      return {
        valido,
        errores,
        advertencias
      };

    } catch (error) {
      this.logger.error(`Error validando CCF: ${error.message}`, error.stack);
      return {
        valido: false,
        errores: [`Error en validación: ${error.message}`],
        advertencias: []
      };
    }
  }

  /**
   * Genera un ejemplo de CCF para testing
   */
  async generarEjemplo(): Promise<CreateCcfDto> {
    const fechaActual = new Date().toISOString().split('T')[0];
    const horaActual = new Date().toTimeString().split(' ')[0];

    return {
      identificacion: {
        version: 1,
        ambiente: '00',
        tipoDte: '03',
        numeroControl: 'DTE-03-00000001-000000000000001',
        codigoGeneracion: this.codigoGeneracionService.generateCodigoGeneracion(),
        tipoModelo: 1,
        tipoOperacion: 1,
        tipoContingencia: null,
        motivoContin: null,
        fecEmi: fechaActual,
        horEmi: horaActual,
        tipoMoneda: 'USD'
      },
      emisor: {
        nit: '02101601014',
        nrc: '240372-1',
        nombre: 'EMPRESA EJEMPLO S.A. DE C.V.',
        codActividad: '62020',
        descActividad: 'Consultoría en informática',
        nombreComercial: 'EMPRESA EJEMPLO',
        tipoEstablecimiento: '01',
        direccion: {
          departamento: '06',
          municipio: '01',
          complemento: 'Colonia Escalón, Avenida Norte #123'
        },
        telefono: '2234-5678',
        correo: 'facturacion@empresaejemplo.com'
      },
      receptor: {
        tipoDocumento: '36',
        numDocumento: '02101601015',
        nrc: '240373-2',
        nombre: 'CLIENTE EJEMPLO S.A. DE C.V.',
        codActividad: '46900',
        descActividad: 'Comercio al por mayor',
        direccion: {
          departamento: '06',
          municipio: '01',
          complemento: 'Colonia San Benito, Calle Principal #456'
        },
        telefono: '2345-6789',
        correo: 'compras@clienteejemplo.com'
      },
      cuerpoDocumento: [
        {
          numItem: 1,
          tipoItem: 2,
          cantidad: 1,
          uniMedida: 1,
          descripcion: 'Servicio de consultoría en sistemas',
          precioUni: 100.00,
          montoDescu: 0.00,
          ventaNoSuj: 0.00,
          ventaExenta: 0.00,
          ventaGravada: 100.00,
          psv: 0.00,
          noGravado: 0.00
        }
      ],
      resumen: {
        totalNoSuj: 0.00,
        totalExenta: 0.00,
        totalGravada: 100.00,
        subTotalVentas: 100.00,
        descuNoSuj: 0.00,
        descuExenta: 0.00,
        descuGravada: 0.00,
        porcentajeDescuento: 0.00,
        totalDescu: 0.00,
        tributos: [
          {
            codigo: '20',
            descripcion: 'Impuesto al Valor Agregado 13%',
            valor: 13.00
          }
        ],
        subTotal: 100.00,
        ivaPerci1: 0.00,
        ivaRete1: 0.00,
        reteRenta: 0.00,
        montoTotalOperacion: 113.00,
        totalNoGravado: 0.00,
        totalPagar: 113.00,
        totalLetras: 'CIENTO TRECE 00/100 DÓLARES',
        saldoFavor: 0.00,
        condicionOperacion: 1
      }
    };
  }
}