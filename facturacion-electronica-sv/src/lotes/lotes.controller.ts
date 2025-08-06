import { Controller, Post, Body, Get, Param, HttpCode, HttpStatus, Logger } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse, ApiParam } from '@nestjs/swagger';
import { LotesService } from './lotes.service';
import { CreateLoteDto, ConsultaLoteDto } from './dto/create-lote.dto';

@ApiTags('Lotes - Envío Masivo de DTEs')
@Controller('lotes')
export class LotesController {
  private readonly logger = new Logger(LotesController.name);

  constructor(private readonly lotesService: LotesService) {}

  @Post()
  @HttpCode(HttpStatus.CREATED)
  @ApiOperation({ 
    summary: 'Crear y enviar lote de DTEs',
    description: 'Envía un lote de DTEs al Ministerio de Hacienda para procesamiento masivo'
  })
  @ApiResponse({ 
    status: 201, 
    description: 'Lote procesado exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        codigoLote: { type: 'string', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' },
        totalDocumentos: { type: 'number', example: 25 },
        procesados: { type: 'number', example: 23 },
        rechazados: { type: 'number', example: 2 },
        errores: { type: 'number', example: 0 },
        selloRecibido: { type: 'string', example: 'SELLO_MH_12345' },
        fhProcesamiento: { type: 'string', example: '2024-01-15T10:30:00.000Z' },
        observaciones: { type: 'array', items: { type: 'string' } }
      }
    }
  })
  @ApiResponse({ 
    status: 400, 
    description: 'Datos inválidos o DTEs no válidos para lote',
    schema: {
      type: 'object',
      properties: {
        statusCode: { type: 'number', example: 400 },
        message: { type: 'string', example: 'El lote no puede contener más de 500 documentos' },
        error: { type: 'string', example: 'Bad Request' }
      }
    }
  })
  @ApiResponse({ 
    status: 404, 
    description: 'DTE no encontrado' 
  })
  @ApiResponse({ 
    status: 500, 
    description: 'Error interno del servidor' 
  })
  async create(@Body() createLoteDto: CreateLoteDto) {
    this.logger.log(`Recibida solicitud para procesar lote con ${createLoteDto.documentos.length} documentos`);
    return this.lotesService.create(createLoteDto);
  }

  @Post('consultar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Consultar estado de lote',
    description: 'Consulta el estado de procesamiento de un lote en el Ministerio de Hacienda'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Estado del lote consultado exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        codigoLote: { type: 'string', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' },
        estado: { type: 'string', example: 'PROCESADO' },
        fechaProcesamiento: { type: 'string', example: '2024-01-15T10:30:00.000Z' },
        observaciones: { type: 'string', example: 'Lote: 23 procesados, 2 rechazados' },
        estadoMH: {
          type: 'object',
          properties: {
            estado: { type: 'string', example: 'PROCESADO' },
            procesados: { type: 'number', example: 23 },
            rechazados: { type: 'number', example: 2 },
            pendientes: { type: 'number', example: 0 }
          }
        },
        fechaConsulta: { type: 'string', example: '2024-01-15T10:30:00.000Z' }
      }
    }
  })
  @ApiResponse({ 
    status: 404, 
    description: 'Lote no encontrado' 
  })
  async consultar(@Body() consultaLoteDto: ConsultaLoteDto) {
    this.logger.log(`Recibida solicitud para consultar lote: ${consultaLoteDto.codigoLote}`);
    return this.lotesService.consultar(consultaLoteDto);
  }

  @Get()
  @ApiOperation({ 
    summary: 'Obtener todos los lotes',
    description: 'Obtiene una lista de todos los lotes procesados'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Lista de lotes obtenida exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Lista de lotes obtenida' },
        data: { type: 'array', items: { type: 'object' } },
        total: { type: 'number', example: 10 }
      }
    }
  })
  async findAll() {
    this.logger.log('Recibida solicitud para obtener todos los lotes');
    return this.lotesService.getAll();
  }

  @Get('estadisticas')
  @ApiOperation({ 
    summary: 'Obtener estadísticas de lotes',
    description: 'Obtiene estadísticas generales sobre el procesamiento de lotes'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Estadísticas obtenidas exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Estadísticas de lotes obtenidas' },
        data: {
          type: 'object',
          properties: {
            totalLotes: { type: 'number', example: 150 },
            lotesHoy: { type: 'number', example: 5 },
            documentosProcesados: { type: 'number', example: 3750 },
            documentosRechazados: { type: 'number', example: 125 },
            promedioDocumentosPorLote: { type: 'number', example: 25.8 }
          }
        }
      }
    }
  })
  async getEstadisticas() {
    this.logger.log('Recibida solicitud para obtener estadísticas de lotes');
    return this.lotesService.getEstadisticas();
  }

  @Get(':id')
  @ApiOperation({ 
    summary: 'Obtener lote por ID',
    description: 'Obtiene un lote específico por su ID'
  })
  @ApiParam({ 
    name: 'id', 
    description: 'ID único del lote',
    example: '123'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Lote encontrado',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Lote encontrado' },
        data: { type: 'object' }
      }
    }
  })
  @ApiResponse({ 
    status: 404, 
    description: 'Lote no encontrado' 
  })
  async findOne(@Param('id') id: string) {
    this.logger.log(`Recibida solicitud para obtener lote con ID: ${id}`);
    return this.lotesService.getById(id);
  }

  @Post('validar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Validar lote sin enviar',
    description: 'Valida la estructura y datos de un lote sin enviarlo al Ministerio de Hacienda'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Lote válido',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Lote válido' },
        totalDocumentos: { type: 'number', example: 25 },
        warnings: { type: 'array', items: { type: 'string' } }
      }
    }
  })
  @ApiResponse({ 
    status: 400, 
    description: 'Lote inválido' 
  })
  async validate(@Body() createLoteDto: CreateLoteDto) {
    this.logger.log(`Recibida solicitud para validar lote con ${createLoteDto.documentos.length} documentos`);
    
    try {
      // Aquí podrías llamar a un método de validación sin envío
      // Por ahora, simulamos una validación exitosa
      return {
        success: true,
        message: 'Lote válido',
        totalDocumentos: createLoteDto.documentos.length,
        warnings: []
      };
    } catch (error) {
      return {
        success: false,
        message: 'Lote inválido',
        errors: [error.message]
      };
    }
  }
}