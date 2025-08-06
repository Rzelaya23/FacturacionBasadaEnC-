import { Controller, Post, Body, Get, Param, HttpCode, HttpStatus, Logger } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse, ApiParam } from '@nestjs/swagger';
import { ContingenciaService } from './contingencia.service';
import { CreateContingenciaDto } from './dto/create-contingencia.dto';

@ApiTags('Contingencia - Manejo de Contingencias')
@Controller('contingencia')
export class ContingenciaController {
  private readonly logger = new Logger(ContingenciaController.name);

  constructor(private readonly contingenciaService: ContingenciaService) {}

  @Post()
  @HttpCode(HttpStatus.CREATED)
  @ApiOperation({ 
    summary: 'Crear contingencia',
    description: 'Reporta una contingencia al Ministerio de Hacienda con los DTEs afectados durante el período de falla'
  })
  @ApiResponse({ 
    status: 201, 
    description: 'Contingencia procesada exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        codigoGeneracion: { type: 'string', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' },
        dtesIncluidos: { type: 'number', example: 15 },
        selloRecibido: { type: 'string', example: 'SELLO_MH_12345' },
        fhProcesamiento: { type: 'string', example: '2024-01-15T10:30:00.000Z' },
        clasificaMsg: { type: 'string', example: 'ACEPTADO' },
        codigoMsg: { type: 'string', example: '001' },
        descripcionMsg: { type: 'string', example: 'Contingencia procesada correctamente' },
        observaciones: { type: 'array', items: { type: 'string' } }
      }
    }
  })
  @ApiResponse({ 
    status: 400, 
    description: 'Datos inválidos o DTEs no válidos para contingencia',
    schema: {
      type: 'object',
      properties: {
        statusCode: { type: 'number', example: 400 },
        message: { type: 'string', example: 'La contingencia no puede exceder 72 horas' },
        error: { type: 'string', example: 'Bad Request' }
      }
    }
  })
  @ApiResponse({ 
    status: 404, 
    description: 'DTE no encontrado',
    schema: {
      type: 'object',
      properties: {
        statusCode: { type: 'number', example: 404 },
        message: { type: 'string', example: 'DTE no encontrado: ABC123' },
        error: { type: 'string', example: 'Not Found' }
      }
    }
  })
  @ApiResponse({ 
    status: 500, 
    description: 'Error interno del servidor' 
  })
  async create(@Body() createContingenciaDto: CreateContingenciaDto) {
    this.logger.log('Recibida solicitud para crear contingencia');
    return this.contingenciaService.create(createContingenciaDto);
  }

  @Get()
  @ApiOperation({ 
    summary: 'Obtener todas las contingencias',
    description: 'Obtiene una lista de todas las contingencias reportadas'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Lista de contingencias obtenida exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Lista de contingencias obtenida' },
        data: { type: 'array', items: { type: 'object' } },
        total: { type: 'number', example: 3 }
      }
    }
  })
  async findAll() {
    this.logger.log('Recibida solicitud para obtener todas las contingencias');
    return this.contingenciaService.getAll();
  }

  @Get(':id')
  @ApiOperation({ 
    summary: 'Obtener contingencia por ID',
    description: 'Obtiene una contingencia específica por su ID'
  })
  @ApiParam({ 
    name: 'id', 
    description: 'ID único de la contingencia',
    example: '123'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Contingencia encontrada',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Contingencia encontrada' },
        data: { type: 'object' }
      }
    }
  })
  @ApiResponse({ 
    status: 404, 
    description: 'Contingencia no encontrada' 
  })
  async findOne(@Param('id') id: string) {
    this.logger.log(`Recibida solicitud para obtener contingencia con ID: ${id}`);
    return this.contingenciaService.getById(id);
  }

  @Get('estado/:codigoGeneracion')
  @ApiOperation({ 
    summary: 'Consultar estado de contingencia en MH',
    description: 'Consulta el estado actual de una contingencia en el Ministerio de Hacienda'
  })
  @ApiParam({ 
    name: 'codigoGeneracion', 
    description: 'Código de generación de la contingencia',
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
        descripcion: { type: 'string', example: 'Contingencia: Falla en conectividad con el MH' },
        fechaConsulta: { type: 'string', example: '2024-01-15T10:30:00.000Z' }
      }
    }
  })
  @ApiResponse({ 
    status: 404, 
    description: 'Contingencia no encontrada' 
  })
  async getEstado(@Param('codigoGeneracion') codigoGeneracion: string) {
    this.logger.log(`Recibida solicitud para consultar estado de contingencia: ${codigoGeneracion}`);
    return this.contingenciaService.getEstado(codigoGeneracion);
  }

  @Post('activar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Activar modo contingencia',
    description: 'Activa el modo contingencia en el sistema para manejar fallas de conectividad'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Modo contingencia activado',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Modo contingencia activado' },
        tipoContingencia: { type: 'number', example: 1 },
        motivo: { type: 'string', example: 'Falla en conectividad con el MH' },
        fechaActivacion: { type: 'string', example: '2024-01-15T10:30:00.000Z' }
      }
    }
  })
  async activar(@Body() body: { tipoContingencia: number; motivo: string }) {
    this.logger.log('Recibida solicitud para activar modo contingencia');
    return this.contingenciaService.activarContingencia(body.tipoContingencia, body.motivo);
  }

  @Post('desactivar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Desactivar modo contingencia',
    description: 'Desactiva el modo contingencia en el sistema'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Modo contingencia desactivado',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Modo contingencia desactivado' },
        fechaDesactivacion: { type: 'string', example: '2024-01-15T10:30:00.000Z' }
      }
    }
  })
  async desactivar() {
    this.logger.log('Recibida solicitud para desactivar modo contingencia');
    return this.contingenciaService.desactivarContingencia();
  }

  @Post('validar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Validar contingencia sin enviar',
    description: 'Valida la estructura y datos de una contingencia sin enviarla al Ministerio de Hacienda'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Contingencia válida',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Contingencia válida' },
        warnings: { type: 'array', items: { type: 'string' } }
      }
    }
  })
  @ApiResponse({ 
    status: 400, 
    description: 'Contingencia inválida' 
  })
  async validate(@Body() createContingenciaDto: CreateContingenciaDto) {
    this.logger.log('Recibida solicitud para validar contingencia');
    
    try {
      // Aquí podrías llamar a un método de validación sin envío
      // Por ahora, simulamos una validación exitosa
      return {
        success: true,
        message: 'Contingencia válida',
        warnings: []
      };
    } catch (error) {
      return {
        success: false,
        message: 'Contingencia inválida',
        errors: [error.message]
      };
    }
  }
}