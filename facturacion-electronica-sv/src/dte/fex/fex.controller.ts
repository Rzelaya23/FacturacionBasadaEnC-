import { Controller, Post, Body, Get, Param, HttpCode, HttpStatus, Logger } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse, ApiParam } from '@nestjs/swagger';
import { FexService } from './fex.service';
import { CreateFexDto } from './dto/create-fex.dto';

@ApiTags('FEX - Factura de Exportación')
@Controller('dte/fex')
export class FexController {
  private readonly logger = new Logger(FexController.name);

  constructor(private readonly fexService: FexService) {}

  @Post()
  @HttpCode(HttpStatus.CREATED)
  @ApiOperation({ 
    summary: 'Crear FEX (Factura de Exportación)',
    description: 'Crea una nueva Factura de Exportación, la firma digitalmente y la envía al Ministerio de Hacienda'
  })
  @ApiResponse({ 
    status: 201, 
    description: 'FEX creada exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        codigoGeneracion: { type: 'string', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' },
        numeroControl: { type: 'string', example: 'DTE-11-0001-0001-0000001' },
        selloRecibido: { type: 'string', example: 'SELLO_MH_12345' },
        fhProcesamiento: { type: 'string', example: '2024-01-15T10:30:00.000Z' },
        clasificaMsg: { type: 'string', example: 'ACEPTADO' },
        codigoMsg: { type: 'string', example: '001' },
        descripcionMsg: { type: 'string', example: 'Documento procesado correctamente' },
        observaciones: { type: 'array', items: { type: 'string' } }
      }
    }
  })
  @ApiResponse({ 
    status: 400, 
    description: 'Datos inválidos',
    schema: {
      type: 'object',
      properties: {
        statusCode: { type: 'number', example: 400 },
        message: { type: 'string', example: 'El total gravado no coincide' },
        error: { type: 'string', example: 'Bad Request' }
      }
    }
  })
  @ApiResponse({ 
    status: 500, 
    description: 'Error interno del servidor' 
  })
  async create(@Body() createFexDto: CreateFexDto) {
    this.logger.log('Recibida solicitud para crear FEX');
    return this.fexService.create(createFexDto);
  }

  @Get()
  @ApiOperation({ 
    summary: 'Obtener todas las FEX',
    description: 'Obtiene una lista de todas las Facturas de Exportación creadas'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Lista de FEX obtenida exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Lista de FEX obtenida' },
        data: { type: 'array', items: { type: 'object' } },
        total: { type: 'number', example: 0 }
      }
    }
  })
  async findAll() {
    this.logger.log('Recibida solicitud para obtener todas las FEX');
    return this.fexService.getAll();
  }

  @Get(':id')
  @ApiOperation({ 
    summary: 'Obtener FEX por ID',
    description: 'Obtiene una Factura de Exportación específica por su ID'
  })
  @ApiParam({ 
    name: 'id', 
    description: 'ID único de la FEX',
    example: '123e4567-e89b-12d3-a456-426614174000'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'FEX encontrada',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'FEX encontrada' },
        data: {
          type: 'object',
          properties: {
            id: { type: 'string' },
            estado: { type: 'string', example: 'PROCESADO' },
            fechaCreacion: { type: 'string', example: '2024-01-15T10:30:00.000Z' }
          }
        }
      }
    }
  })
  @ApiResponse({ 
    status: 404, 
    description: 'FEX no encontrada' 
  })
  async findOne(@Param('id') id: string) {
    this.logger.log(`Recibida solicitud para obtener FEX con ID: ${id}`);
    return this.fexService.getById(id);
  }

  @Post(':id/invalidar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Invalidar FEX',
    description: 'Invalida una Factura de Exportación específica'
  })
  @ApiParam({ 
    name: 'id', 
    description: 'ID único de la FEX a invalidar',
    example: '123e4567-e89b-12d3-a456-426614174000'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'FEX invalidada exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'FEX invalidada correctamente' },
        data: {
          type: 'object',
          properties: {
            id: { type: 'string' },
            estado: { type: 'string', example: 'INVALIDADO' },
            fechaInvalidacion: { type: 'string', example: '2024-01-15T10:30:00.000Z' }
          }
        }
      }
    }
  })
  @ApiResponse({ 
    status: 404, 
    description: 'FEX no encontrada' 
  })
  async invalidate(@Param('id') id: string) {
    this.logger.log(`Recibida solicitud para invalidar FEX con ID: ${id}`);
    return this.fexService.invalidate(id);
  }

  @Get('estado/:codigoGeneracion')
  @ApiOperation({ 
    summary: 'Consultar estado de FEX en MH',
    description: 'Consulta el estado actual de una FEX en el Ministerio de Hacienda'
  })
  @ApiParam({ 
    name: 'codigoGeneracion', 
    description: 'Código de generación de la FEX',
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
        descripcion: { type: 'string', example: 'Documento procesado correctamente' },
        fechaConsulta: { type: 'string', example: '2024-01-15T10:30:00.000Z' }
      }
    }
  })
  @ApiResponse({ 
    status: 404, 
    description: 'FEX no encontrada en MH' 
  })
  async getEstado(@Param('codigoGeneracion') codigoGeneracion: string) {
    this.logger.log(`Recibida solicitud para consultar estado de FEX: ${codigoGeneracion}`);
    return this.fexService.getEstado(codigoGeneracion);
  }

  @Get('ejemplo')
  @ApiOperation({ 
    summary: 'Obtener ejemplo de FEX',
    description: 'Obtiene un ejemplo completo de Factura de Exportación para referencia'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Ejemplo de FEX obtenido exitosamente',
    schema: {
      type: 'object',
      properties: {
        identificacion: { type: 'object' },
        emisor: { type: 'object' },
        receptor: { type: 'object' },
        cuerpoDocumento: { type: 'array' },
        resumen: { type: 'object' }
      }
    }
  })
  async getExample() {
    this.logger.log('Recibida solicitud para obtener ejemplo de FEX');
    return this.fexService.getExample();
  }

  @Post('validar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Validar FEX sin enviar',
    description: 'Valida la estructura y datos de una FEX sin enviarla al Ministerio de Hacienda'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'FEX válida',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'FEX válida' },
        warnings: { type: 'array', items: { type: 'string' } }
      }
    }
  })
  @ApiResponse({ 
    status: 400, 
    description: 'FEX inválida' 
  })
  async validate(@Body() createFexDto: CreateFexDto) {
    this.logger.log('Recibida solicitud para validar FEX');
    
    try {
      // Validar usando el servicio
      this.fexService.validateFexData(createFexDto);
      return {
        success: true,
        message: 'FEX válida',
        warnings: []
      };
    } catch (error) {
      return {
        success: false,
        message: 'FEX inválida',
        errors: [error.message]
      };
    }
  }
}