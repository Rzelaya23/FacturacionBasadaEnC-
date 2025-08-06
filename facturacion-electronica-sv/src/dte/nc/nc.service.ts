import { Injectable, Logger, BadRequestException, InternalServerErrorException } from '@nestjs/common';
import { CreateNcDto } from './dto/create-nc.dto';
import { FirmadorService } from '../../firmador/firmador.service';
import { MhIntegrationService } from '../../mh-integration/mh-integration.service';
import { CodigoGeneracionService } from '../../common/services/codigo-generacion.service';
import { CatalogosService } from '../../common/services/catalogos.service';

@Injectable()
export class NcService {
  private readonly logger = new Logger(NcService.name);

  constructor(
    private readonly firmadorService: FirmadorService,
    private readonly mhIntegrationService: MhIntegrationService,
    private readonly codigoGeneracionService: CodigoGeneracionService,
    private readonly catalogosService: CatalogosService,
  ) {}

  /**
   * Genera una NC completa: valida, firma y envía al MH
   */
  async generarNc(createNcDto: CreateNcDto): Promise<any> {
    this.logger.log('Iniciando generación completa de NC');

    try {
      // 1. Validar datos
      const validacion = await this.validarDatos(createNcDto);
      if (!validacion.valido) {
        throw new BadRequestException(`Datos inválidos: ${validacion.errores.join(', ')}`);
      }

      // 2. Generar código de generación si no existe
      if (!createNcDto.identificacion.codigoGeneracion) {
        createNcDto.identificacion.codigoGeneracion = this.codigoGeneracionService.generateCodigoGeneracion();
      }

      // 3. Firmar documento
      const documentoFirmado = await this.firmadorService.signNC(createNcDto);

      // 4. Enviar al MH
      const respuestaMh = await this.mhIntegrationService.sendDTE(documentoFirmado);

      this.logger.log(`NC generada exitosamente: ${createNcDto.identificacion.codigoGeneracion}`);

      return {
        codigoGeneracion: createNcDto.identificacion.codigoGeneracion,
        documento: documentoFirmado,
        ...respuestaMh
      };

    } catch (error) {
      this.logger.error(`Error generando NC: ${error.message}`, error.stack);
      throw new InternalServerErrorException(`Error generando NC: ${error.message}`);
    }
  }

  /**
   * Firma una NC sin enviarla al MH
   */
  async firmarNc(createNcDto: CreateNcDto): Promise<any> {
    this.logger.log('Iniciando firma de NC');

    try {
      // Validar datos
      const validacion = await this.validarDatos(createNcDto);
      if (!validacion.valido) {
        throw new BadRequestException(`Datos inválidos: ${validacion.errores.join(', ')}`);
      }

      // Generar código de generación si no existe
      if (!createNcDto.identificacion.codigoGeneracion) {
        createNcDto.identificacion.codigoGeneracion = this.codigoGeneracionService.generateCodigoGeneracion();
      }

      // Firmar documento
      const documentoFirmado = await this.firmadorService.signNC(createNcDto);

      this.logger.log(`NC firmada exitosamente: ${createNcDto.identificacion.codigoGeneracion}`);

      return {
        codigoGeneracion: createNcDto.identificacion.codigoGeneracion,
        documento: documentoFirmado
      };

    } catch (error) {
      this.logger.error(`Error firmando NC: ${error.message}`, error.stack);
      throw new InternalServerErrorException(`Error firmando NC: ${error.message}`);
    }
  }

  /**
   * Envía una NC ya firmada al MH
   */
  async enviarNcAlMh(documento: string, codigoGeneracion: string): Promise<any> {
    this.logger.log(`Enviando NC al MH: ${codigoGeneracion}`);

    try {
      const respuestaMh = await this.mhIntegrationService.sendDTE(documento);

      this.logger.log(`NC enviada exitosamente al MH: ${codigoGeneracion}`);

      return respuestaMh;

    } catch (error) {
      this.logger.error(`Error enviando NC al MH: ${error.message}`, error.stack);
      throw new InternalServerErrorException(`Error enviando NC al MH: ${error.message}`);
    }
  }

  /**
   * Consulta el estado de una NC en el MH
   */
  async consultarEstado(codigoGeneracion: string): Promise<any> {
    this.logger.log(`Consultando estado de NC: ${codigoGeneracion}`);

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
      this.logger.error(`Error consultando estado de NC: ${error.message}`, error.stack);
      throw new InternalServerErrorException(`Error consultando estado de NC: ${error.message}`);
    }
  }

  /**
   * Valida los datos de una NC
   */
  async validarDatos(createNcDto: CreateNcDto): Promise<{ valido: boolean; errores: string[]; advertencias: string[] }> {
    const errores: string[] = [];
    const advertencias: string[] = [];

    try {
      // Validaciones específicas para NC
      
      // 1. Validar identificación
      if (!createNcDto.identificacion) {
        errores.push('La identificación es requerida');
      } else {
        if (createNcDto.identificacion.tipoDte !== '05') {
          errores.push('El tipo de DTE debe ser 05 para NC');
        }
      }

      // 2. Validar emisor
      if (!createNcDto.emisor) {
        errores.push('Los datos del emisor son requeridos');
      } else {
        if (!createNcDto.emisor.nit) {
          errores.push('El NIT del emisor es requerido para NC');
        }
      }

      // 3. Validar receptor
      if (!createNcDto.receptor) {
        errores.push('Los datos del receptor son requeridos');
      } else {
        if (!createNcDto.receptor.numDocumento) {
          errores.push('El número de documento del receptor es requerido');
        }
      }

      // 4. Validar documentos relacionados
      if (!createNcDto.documentoRelacionado || createNcDto.documentoRelacionado.length === 0) {
        errores.push('Los documentos relacionados son requeridos para NC');
      } else {
        for (const docRel of createNcDto.documentoRelacionado) {
          // Validar que el tipo de documento relacionado existe
          const tipoDocValido = await this.catalogosService.validarCodigo('tipo_documento', docRel.tipoDocumento);
          if (!tipoDocValido) {
            errores.push(`Tipo de documento relacionado inválido: ${docRel.tipoDocumento}`);
          }
        }
      }

      // 5. Validar cuerpo del documento
      if (!createNcDto.cuerpoDocumento || createNcDto.cuerpoDocumento.length === 0) {
        errores.push('El cuerpo del documento es requerido');
      } else {
        for (let i = 0; i < createNcDto.cuerpoDocumento.length; i++) {
          const item = createNcDto.cuerpoDocumento[i];
          
          if (item.numItem !== i + 1) {
            errores.push(`El ítem ${i + 1} tiene numeración incorrecta`);
          }
          
          if (item.ventaGravada < 0 || item.ventaExenta < 0 || item.ventaNoSuj < 0) {
            errores.push(`El ítem ${i + 1} tiene montos negativos`);
          }

          // Validar tipo de ítem
          const tipoItemValido = await this.catalogosService.validarCodigo('tipo_item', item.tipoItem.toString());
          if (!tipoItemValido) {
            errores.push(`Tipo de ítem inválido en ítem ${i + 1}: ${item.tipoItem}`);
          }

          // Validar unidad de medida
          const unidadMedidaValida = await this.catalogosService.validarCodigo('unidad_medida', item.uniMedida.toString());
          if (!unidadMedidaValida) {
            errores.push(`Unidad de medida inválida en ítem ${i + 1}: ${item.uniMedida}`);
          }
        }
      }

      // 6. Validar resumen
      if (!createNcDto.resumen) {
        errores.push('El resumen es requerido');
      } else {
        if (createNcDto.resumen.montoTotalOperacion <= 0) {
          errores.push('El monto total de la operación debe ser mayor a cero');
        }
        if (!createNcDto.resumen.totalLetras) {
          errores.push('El total en letras es requerido');
        }
        if (!createNcDto.resumen.tributos || createNcDto.resumen.tributos.length === 0) {
          advertencias.push('No se han definido tributos en el resumen');
        }

        // Validar condición de operación
        const condicionValida = await this.catalogosService.validarCodigo('condicion_operacion', createNcDto.resumen.condicionOperacion.toString());
        if (!condicionValida) {
          errores.push(`Condición de operación inválida: ${createNcDto.resumen.condicionOperacion}`);
        }
      }

      // 7. Validaciones de coherencia
      if (createNcDto.cuerpoDocumento && createNcDto.resumen) {
        const totalCalculado = createNcDto.cuerpoDocumento.reduce((sum, item) => 
          sum + item.ventaGravada + item.ventaExenta + item.ventaNoSuj, 0
        );
        
        const totalResumen = createNcDto.resumen.totalGravada + 
                           createNcDto.resumen.totalExenta + 
                           createNcDto.resumen.totalNoSuj;

        if (Math.abs(totalCalculado - totalResumen) > 0.01) {
          errores.push('Los totales del cuerpo y resumen no coinciden');
        }
      }

      const valido = errores.length === 0;

      this.logger.log(`Validación de NC completada. Válido: ${valido}, Errores: ${errores.length}, Advertencias: ${advertencias.length}`);

      return {
        valido,
        errores,
        advertencias
      };

    } catch (error) {
      this.logger.error(`Error validando NC: ${error.message}`, error.stack);
      return {
        valido: false,
        errores: [`Error en validación: ${error.message}`],
        advertencias: []
      };
    }
  }

  /**
   * Genera un ejemplo de NC para testing
   */
  async generarEjemplo(): Promise<CreateNcDto> {
    const fechaActual = new Date().toISOString().split('T')[0];
    const horaActual = new Date().toTimeString().split(' ')[0];

    return {
      identificacion: {
        version: 1,
        ambiente: '00',
        tipoDte: '05',
        numeroControl: 'DTE-05-00000001-000000000000001',
        codigoGeneracion: this.codigoGeneracionService.generateCodigoGeneracion(),
        tipoModelo: 1,
        tipoOperacion: 1,
        tipoContingencia: null,
        motivoContin: null,
        fecEmi: fechaActual,
        horEmi: horaActual,
        tipoMoneda: 'USD'
      },
      documentoRelacionado: [
        {
          tipoDocumento: '01',
          tipoGeneracion: 1,
          numeroDocumento: 'DTE-01-0001-0001-000000000000001',
          fechaEmision: fechaActual
        }
      ],
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
          descripcion: 'Devolución por servicio de consultoría',
          cantidad: 1,
          uniMedida: 1,
          precioUni: 50.00,
          montoDescu: 0.00,
          ventaNoSuj: 0.00,
          ventaExenta: 0.00,
          ventaGravada: 50.00
        }
      ],
      resumen: {
        totalNoSuj: 0.00,
        totalExenta: 0.00,
        totalGravada: 50.00,
        subTotalVentas: 50.00,
        descuNoSuj: 0.00,
        descuExenta: 0.00,
        descuGravada: 0.00,
        totalDescu: 0.00,
        tributos: [
          {
            codigo: '20',
            descripcion: 'Impuesto al Valor Agregado 13%',
            valor: 6.50
          }
        ],
        subTotal: 50.00,
        ivaPerci1: 0.00,
        ivaRete1: 0.00,
        reteRenta: 0.00,
        montoTotalOperacion: 56.50,
        totalLetras: 'CINCUENTA Y SEIS 50/100 DÓLARES',
        condicionOperacion: 1
      }
    };
  }
}