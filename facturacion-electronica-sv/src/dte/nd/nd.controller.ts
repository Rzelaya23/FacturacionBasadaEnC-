import { Controller, Post, Body, Get, Param, HttpCode, HttpStatus, Logger } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse, ApiParam } from '@nestjs/swagger';
import { NdService } from './nd.service';
import { CreateNdDto } from './dto/create-nd.dto';

@ApiTags('Nota de Débito (ND)')
@Controller('dte/nd')
export class NdController {
  private readonly logger = new Logger(NdController.name);

  constructor(private readonly ndService: NdService) {}

  @Post()
  @HttpCode(HttpStatus.CREATED)
  @ApiOperation({
    summary: 'Crear Nota de Débito',
    description: `
    Crea una nueva Nota de Débito (ND) y la procesa completamente:
    
    **Proceso completo:**
    1. ✅ Validación de datos de entrada
    2. ✅ Generación de código único
    3. ✅ Firmado digital del documento
    4. ✅ Almacenamiento en base de datos
    5. ✅ Envío al Ministerio de Hacienda
    6. ✅ Actualización de estado
    
    **Características de la ND:**
    - Documento que incrementa el valor de una factura original
    - Debe referenciar documentos relacionados
    - Requiere validación de totales
    - Genera código de generación único
    
    **Validaciones automáticas:**
    - Coherencia de totales calculados vs declarados
    - Formato de NIT y documentos
    - Campos obligatorios según normativa MH
    - Documentos relacionados válidos
    `
  })
  @ApiResponse({
    status: 201,
    description: 'Nota de Débito creada exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Nota de Débito creada exitosamente' },
        data: {
          type: 'object',
          properties: {
            codigoGeneracion: { type: 'string', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' },
            numeroControl: { type: 'string', example: 'DTE-04-00000001-000000000000001' },
            selloRecibido: { type: 'string', example: 'SELLO-MH-123456789' },
            estado: { type: 'string', example: 'PROCESADO' },
            fechaHoraProcesamiento: { type: 'string', example: '2024-01-15T10:30:00.000Z' },
            documentoFirmado: { type: 'string', description: 'Documento JSON firmado digitalmente' },
            respuestaMH: { type: 'object', description: 'Respuesta completa del Ministerio de Hacienda' }
          }
        }
      }
    }
  })
  @ApiResponse({
    status: 400,
    description: 'Datos de entrada inválidos',
    schema: {
      type: 'object',
      properties: {
        statusCode: { type: 'number', example: 400 },
        message: { type: 'string', example: 'Los totales no coinciden. Calculado: 100.00, Declarado: 95.00' },
        error: { type: 'string', example: 'Bad Request' }
      }
    }
  })
  @ApiResponse({
    status: 500,
    description: 'Error interno del servidor',
    schema: {
      type: 'object',
      properties: {
        statusCode: { type: 'number', example: 500 },
        message: { type: 'string', example: 'Error interno al procesar Nota de Débito' },
        error: { type: 'string', example: 'Internal Server Error' }
      }
    }
  })
  async create(@Body() createNdDto: CreateNdDto) {
    this.logger.log('🚀 Solicitud de creación de Nota de Débito recibida');
    this.logger.log(`📋 Número de control: ${createNdDto.identificacion?.numeroControl}`);
    this.logger.log(`🏢 Emisor: ${createNdDto.emisor?.nombre} (${createNdDto.emisor?.nit})`);
    this.logger.log(`👤 Receptor: ${createNdDto.receptor?.nombre || 'N/A'}`);
    this.logger.log(`💰 Total: $${createNdDto.resumen?.totalPagar || 0}`);
    this.logger.log(`📄 Documentos relacionados: ${createNdDto.documentoRelacionado?.length || 0}`);
    this.logger.log(`📦 Ítems: ${createNdDto.cuerpoDocumento?.length || 0}`);

    const startTime = Date.now();
    
    try {
      const result = await this.ndService.create(createNdDto);
      
      const processingTime = Date.now() - startTime;
      this.logger.log(`✅ Nota de Débito procesada exitosamente en ${processingTime}ms`);
      this.logger.log(`🔑 Código generado: ${result.data.codigoGeneracion}`);
      this.logger.log(`📋 Estado: ${result.data.estado}`);
      
      return result;
    } catch (error) {
      const processingTime = Date.now() - startTime;
      this.logger.error(`❌ Error al procesar Nota de Débito en ${processingTime}ms:`, error.message);
      throw error;
    }
  }

  @Get()
  @ApiOperation({
    summary: 'Listar todas las Notas de Débito',
    description: `
    Obtiene una lista de todas las Notas de Débito registradas en el sistema.
    
    **Información incluida:**
    - Código de generación único
    - Número de control
    - Datos del emisor y receptor
    - Estado actual del documento
    - Fechas de creación y actualización
    - Monto total
    
    **Ordenamiento:** Por fecha de creación (más recientes primero)
    `
  })
  @ApiResponse({
    status: 200,
    description: 'Lista de Notas de Débito obtenida exitosamente',
    schema: {
      type: 'array',
      items: {
        type: 'object',
        properties: {
          id: { type: 'number', example: 1 },
          codigoGeneracion: { type: 'string', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' },
          numeroControl: { type: 'string', example: 'DTE-04-00000001-000000000000001' },
          tipoDocumento: { type: 'string', example: '04' },
          emisorNit: { type: 'string', example: '12345678901234' },
          emisorNombre: { type: 'string', example: 'EMPRESA EJEMPLO S.A. DE C.V.' },
          receptorNit: { type: 'string', example: '98765432109876' },
          receptorNombre: { type: 'string', example: 'CLIENTE EJEMPLO' },
          montoTotal: { type: 'number', example: 113.00 },
          estado: { type: 'string', example: 'PROCESADO' },
          fechaCreacion: { type: 'string', example: '2024-01-15T10:30:00.000Z' },
          fechaActualizacion: { type: 'string', example: '2024-01-15T10:30:15.000Z' }
        }
      }
    }
  })
  async findAll() {
    this.logger.log('📋 Solicitud de listado de Notas de Débito');
    
    const startTime = Date.now();
    const result = await this.ndService.findAll();
    const processingTime = Date.now() - startTime;
    
    this.logger.log(`✅ ${result.length} Notas de Débito obtenidas en ${processingTime}ms`);
    
    return result;
  }

  @Get(':codigoGeneracion')
  @ApiOperation({
    summary: 'Obtener Nota de Débito por código',
    description: `
    Busca y retorna una Nota de Débito específica usando su código de generación único.
    
    **Información completa incluida:**
    - Todos los datos del documento
    - Estado actual y histórico
    - Documento JSON original
    - Documento firmado
    - Respuesta del Ministerio de Hacienda
    `
  })
  @ApiParam({
    name: 'codigoGeneracion',
    description: 'Código de generación único de la Nota de Débito',
    example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890'
  })
  @ApiResponse({
    status: 200,
    description: 'Nota de Débito encontrada',
    schema: {
      type: 'object',
      properties: {
        id: { type: 'number', example: 1 },
        codigoGeneracion: { type: 'string', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' },
        numeroControl: { type: 'string', example: 'DTE-04-00000001-000000000000001' },
        tipoDocumento: { type: 'string', example: '04' },
        emisorNit: { type: 'string', example: '12345678901234' },
        emisorNombre: { type: 'string', example: 'EMPRESA EJEMPLO S.A. DE C.V.' },
        receptorNit: { type: 'string', example: '98765432109876' },
        receptorNombre: { type: 'string', example: 'CLIENTE EJEMPLO' },
        montoTotal: { type: 'number', example: 113.00 },
        estado: { type: 'string', example: 'PROCESADO' },
        documentoJson: { type: 'string', description: 'Documento original en JSON' },
        documentoFirmado: { type: 'string', description: 'Documento firmado digitalmente' },
        respuestaMh: { type: 'string', description: 'Respuesta del Ministerio de Hacienda' },
        fechaCreacion: { type: 'string', example: '2024-01-15T10:30:00.000Z' },
        fechaActualizacion: { type: 'string', example: '2024-01-15T10:30:15.000Z' }
      }
    }
  })
  @ApiResponse({
    status: 400,
    description: 'Nota de Débito no encontrada',
    schema: {
      type: 'object',
      properties: {
        statusCode: { type: 'number', example: 400 },
        message: { type: 'string', example: 'Nota de Débito no encontrada: A1B2C3D4-E5F6-7890-ABCD-EF1234567890' },
        error: { type: 'string', example: 'Bad Request' }
      }
    }
  })
  async findOne(@Param('codigoGeneracion') codigoGeneracion: string) {
    this.logger.log(`🔍 Búsqueda de Nota de Débito: ${codigoGeneracion}`);
    
    const startTime = Date.now();
    const result = await this.ndService.findOne(codigoGeneracion);
    const processingTime = Date.now() - startTime;
    
    this.logger.log(`✅ Nota de Débito encontrada en ${processingTime}ms`);
    this.logger.log(`📋 Estado: ${result.estadoMh}`);
    this.logger.log(`💰 Monto: $${result.totalPagar}`);
    
    return result;
  }

  @Post(':codigoGeneracion/regenerar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({
    summary: 'Regenerar y reenviar Nota de Débito',
    description: `
    Regenera una Nota de Débito existente con un nuevo código de generación y la reenvía al Ministerio de Hacienda.
    
    **Casos de uso:**
    - Reenvío por errores temporales del MH
    - Regeneración por cambios en configuración
    - Reprocesamiento de documentos fallidos
    
    **Proceso:**
    1. Busca la ND original
    2. Genera nuevo código de generación
    3. Actualiza fechas y horas
    4. Reprocesa completamente el documento
    `
  })
  @ApiParam({
    name: 'codigoGeneracion',
    description: 'Código de generación de la Nota de Débito a regenerar',
    example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890'
  })
  @ApiResponse({
    status: 200,
    description: 'Nota de Débito regenerada exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Nota de Débito regenerada exitosamente' },
        data: {
          type: 'object',
          properties: {
            codigoGeneracionOriginal: { type: 'string', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' },
            codigoGeneracionNuevo: { type: 'string', example: 'B2C3D4E5-F6G7-8901-BCDE-F23456789012' },
            estado: { type: 'string', example: 'PROCESADO' }
          }
        }
      }
    }
  })
  async regenerateAndResend(@Param('codigoGeneracion') codigoGeneracion: string) {
    this.logger.log(`🔄 Regenerando Nota de Débito: ${codigoGeneracion}`);
    
    const startTime = Date.now();
    
    try {
      const result = await this.ndService.regenerateAndResend(codigoGeneracion);
      
      const processingTime = Date.now() - startTime;
      this.logger.log(`✅ Nota de Débito regenerada exitosamente en ${processingTime}ms`);
      this.logger.log(`🔑 Nuevo código: ${result.data.codigoGeneracion}`);
      
      return {
        success: true,
        message: 'Nota de Débito regenerada exitosamente',
        data: {
          codigoGeneracionOriginal: codigoGeneracion,
          codigoGeneracionNuevo: result.data.codigoGeneracion,
          estado: result.data.estado,
          processingTime: `${processingTime}ms`
        }
      };
    } catch (error) {
      const processingTime = Date.now() - startTime;
      this.logger.error(`❌ Error al regenerar Nota de Débito en ${processingTime}ms:`, error.message);
      throw error;
    }
  }

  @Get('estadisticas/resumen')
  @ApiOperation({
    summary: 'Obtener estadísticas de Notas de Débito',
    description: `
    Proporciona un resumen estadístico de todas las Notas de Débito en el sistema.
    
    **Métricas incluidas:**
    - Total de NDs procesadas
    - NDs exitosas vs con errores
    - Porcentaje de éxito
    - Tendencias de procesamiento
    `
  })
  @ApiResponse({
    status: 200,
    description: 'Estadísticas obtenidas exitosamente',
    schema: {
      type: 'object',
      properties: {
        total: { type: 'number', example: 150 },
        procesados: { type: 'number', example: 142 },
        errores: { type: 'number', example: 8 },
        porcentajeExito: { type: 'string', example: '94.67' }
      }
    }
  })
  async getEstadisticas() {
    this.logger.log('📊 Solicitud de estadísticas de Notas de Débito');
    
    const startTime = Date.now();
    const result = await this.ndService.getEstadisticas();
    const processingTime = Date.now() - startTime;
    
    this.logger.log(`✅ Estadísticas obtenidas en ${processingTime}ms`);
    this.logger.log(`📈 Total: ${result.total}, Éxito: ${result.porcentajeExito}%`);
    
    return result;
  }
}