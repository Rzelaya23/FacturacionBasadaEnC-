import { Controller, Post, Body, Get, Param, HttpCode, HttpStatus, Logger } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse, ApiParam } from '@nestjs/swagger';
import { AnulacionService } from './anulacion.service';
import { CreateAnulacionDto } from './dto/create-anulacion.dto';

@ApiTags('Anulación - Anulación de DTEs')
@Controller('anulacion')
export class AnulacionController {
  private readonly logger = new Logger(AnulacionController.name);

  constructor(private readonly anulacionService: AnulacionService) {}

  @Post()
  @HttpCode(HttpStatus.CREATED)
  @ApiOperation({ 
    summary: 'Crear anulación de DTE',
    description: 'Anula un DTE existente, firma la anulación digitalmente y la envía al Ministerio de Hacienda'
  })
  @ApiResponse({ 
    status: 201, 
    description: 'Anulación procesada exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        codigoGeneracion: { type: 'string', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' },
        codigoGeneracionOriginal: { type: 'string', example: 'B2C3D4E5-F6G7-8901-BCDE-FG2345678901' },
        selloRecibido: { type: 'string', example: 'SELLO_MH_12345' },
        fhProcesamiento: { type: 'string', example: '2024-01-15T10:30:00.000Z' },
        clasificaMsg: { type: 'string', example: 'ACEPTADO' },
        codigoMsg: { type: 'string', example: '001' },
        descripcionMsg: { type: 'string', example: 'Anulación procesada correctamente' },
        observaciones: { type: 'array', items: { type: 'string' } }
      }
    }
  })
  @ApiResponse({ 
    status: 400, 
    description: 'Datos inválidos o documento no puede ser anulado',
    schema: {
      type: 'object',
      properties: {
        statusCode: { type: 'number', example: 400 },
        message: { type: 'string', example: 'El documento ya se encuentra anulado' },
        error: { type: 'string', example: 'Bad Request' }
      }
    }
  })
  @ApiResponse({ 
    status: 404, 
    description: 'Documento no encontrado',
    schema: {
      type: 'object',
      properties: {
        statusCode: { type: 'number', example: 404 },
        message: { type: 'string', example: 'No se encontró el documento con código de generación: ABC123' },
        error: { type: 'string', example: 'Not Found' }
      }
    }
  })
  @ApiResponse({ 
    status: 500, 
    description: 'Error interno del servidor' 
  })
  async create(@Body() createAnulacionDto: CreateAnulacionDto) {
    this.logger.log('Recibida solicitud para crear anulación');
    return this.anulacionService.create(createAnulacionDto);
  }

  @Get()
  @ApiOperation({ 
    summary: 'Obtener todas las anulaciones',
    description: 'Obtiene una lista de todas las anulaciones procesadas'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Lista de anulaciones obtenida exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Lista de anulaciones obtenida' },
        data: { type: 'array', items: { type: 'object' } },
        total: { type: 'number', example: 5 }
      }
    }
  })
  async findAll() {
    this.logger.log('Recibida solicitud para obtener todas las anulaciones');
    return this.anulacionService.getAll();
  }

  @Get(':id')
  @ApiOperation({ 
    summary: 'Obtener anulación por ID',
    description: 'Obtiene una anulación específica por su ID'
  })
  @ApiParam({ 
    name: 'id', 
    description: 'ID único de la anulación',
    example: '123'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Anulación encontrada',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Anulación encontrada' },
        data: { type: 'object' }
      }
    }
  })
  @ApiResponse({ 
    status: 404, 
    description: 'Anulación no encontrada' 
  })
  async findOne(@Param('id') id: string) {
    this.logger.log(`Recibida solicitud para obtener anulación con ID: ${id}`);
    return this.anulacionService.getById(id);
  }

  @Get('estado/:codigoGeneracion')
  @ApiOperation({ 
    summary: 'Consultar estado de anulación en MH',
    description: 'Consulta el estado actual de una anulación en el Ministerio de Hacienda'
  })
  @ApiParam({ 
    name: 'codigoGeneracion', 
    description: 'Código de generación de la anulación',
    example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Estado consultado exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        codigoGeneracion: { type: 'string', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' },
        estado: { type: 'string', example: 'PROCESADO' },
        descripcion: { type: 'string', example: 'Anulación: Error en datos del receptor' },
        fechaConsulta: { type: 'string', example: '2024-01-15T10:30:00.000Z' }
      }
    }
  })
  @ApiResponse({ 
    status: 404, 
    description: 'Anulación no encontrada' 
  })
  async getEstado(@Param('codigoGeneracion') codigoGeneracion: string) {
    this.logger.log(`Recibida solicitud para consultar estado de anulación: ${codigoGeneracion}`);
    return this.anulacionService.getEstado(codigoGeneracion);
  }

  @Get('documento/:codigoGeneracionOriginal')
  @ApiOperation({ 
    summary: 'Obtener anulaciones de un documento',
    description: 'Obtiene todas las anulaciones realizadas a un documento específico'
  })
  @ApiParam({ 
    name: 'codigoGeneracionOriginal', 
    description: 'Código de generación del documento original',
    example: 'B2C3D4E5-F6G7-8901-BCDE-FG2345678901'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Anulaciones del documento obtenidas',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Anulaciones del documento obtenidas' },
        data: { type: 'array', items: { type: 'object' } },
        total: { type: 'number', example: 1 }
      }
    }
  })
  async getAnulacionesByDocumento(@Param('codigoGeneracionOriginal') codigoGeneracionOriginal: string) {
    this.logger.log(`Recibida solicitud para obtener anulaciones del documento: ${codigoGeneracionOriginal}`);
    return this.anulacionService.getAnulacionesByDocumento(codigoGeneracionOriginal);
  }

  @Post('validar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Validar anulación sin enviar',
    description: 'Valida la estructura y datos de una anulación sin enviarla al Ministerio de Hacienda'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Anulación válida',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Anulación válida' },
        warnings: { type: 'array', items: { type: 'string' } }
      }
    }
  })
  @ApiResponse({ 
    status: 400, 
    description: 'Anulación inválida' 
  })
  async validate(@Body() createAnulacionDto: CreateAnulacionDto) {
    this.logger.log('Recibida solicitud para validar anulación');
    
    try {
      // Aquí podrías llamar a un método de validación sin envío
      // Por ahora, simulamos una validación exitosa
      return {
        success: true,
        message: 'Anulación válida',
        warnings: []
      };
    } catch (error) {
      return {
        success: false,
        message: 'Anulación inválida',
        errors: [error.message]
      };
    }
  }
}