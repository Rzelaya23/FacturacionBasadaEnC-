import { Controller, Post, Body, Get, Param, HttpCode, HttpStatus, Logger } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse, ApiParam } from '@nestjs/swagger';
import { FseService } from './fse.service';
import { CreateFseDto } from './dto/create-fse.dto';

@ApiTags('FSE - Factura de Sujeto Excluido')
@Controller('dte/fse')
export class FseController {
  private readonly logger = new Logger(FseController.name);

  constructor(private readonly fseService: FseService) {}

  @Post()
  @HttpCode(HttpStatus.CREATED)
  @ApiOperation({ 
    summary: 'Crear FSE (Factura de Sujeto Excluido)',
    description: 'Crea una nueva Factura de Sujeto Excluido, la firma digitalmente y la envía al Ministerio de Hacienda'
  })
  @ApiResponse({ 
    status: 201, 
    description: 'FSE creada exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        codigoGeneracion: { type: 'string', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' },
        numeroControl: { type: 'string', example: 'DTE-14-1234567890123-001' },
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
        message: { type: 'string', example: 'El total de compra no coincide' },
        error: { type: 'string', example: 'Bad Request' }
      }
    }
  })
  @ApiResponse({ 
    status: 500, 
    description: 'Error interno del servidor' 
  })
  async create(@Body() createFseDto: CreateFseDto) {
    this.logger.log('Recibida solicitud para crear FSE');
    return this.fseService.create(createFseDto);
  }

  @Get('ejemplo')
  @ApiOperation({ 
    summary: 'Obtener ejemplo de FSE',
    description: 'Obtiene un ejemplo completo de Factura de Sujeto Excluido para referencia'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Ejemplo de FSE obtenido exitosamente',
    schema: {
      type: 'object',
      properties: {
        identificacion: { type: 'object' },
        emisor: { type: 'object' },
        sujetoExcluido: { type: 'object' },
        cuerpoDocumento: { type: 'array' },
        resumen: { type: 'object' }
      }
    }
  })
  async getExample() {
    this.logger.log('Recibida solicitud para obtener ejemplo de FSE');
    return this.fseService.getExample();
  }

  @Get()
  @ApiOperation({ 
    summary: 'Obtener todas las FSE',
    description: 'Obtiene una lista de todas las Facturas de Sujeto Excluido creadas'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Lista de FSE obtenida exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Lista de FSE obtenida' },
        data: { type: 'array', items: { type: 'object' } },
        total: { type: 'number', example: 0 }
      }
    }
  })
  async findAll() {
    this.logger.log('Recibida solicitud para obtener todas las FSE');
    return this.fseService.getAll();
  }

  @Get(':id')
  @ApiOperation({ 
    summary: 'Obtener FSE por ID',
    description: 'Obtiene una Factura de Sujeto Excluido específica por su ID'
  })
  @ApiParam({ 
    name: 'id', 
    description: 'ID único de la FSE',
    example: '123e4567-e89b-12d3-a456-426614174000'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'FSE encontrada',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'FSE encontrada' },
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
    description: 'FSE no encontrada' 
  })
  async findOne(@Param('id') id: string) {
    this.logger.log(`Recibida solicitud para obtener FSE con ID: ${id}`);
    return this.fseService.getById(id);
  }

  @Post(':id/invalidar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Invalidar FSE',
    description: 'Invalida una Factura de Sujeto Excluido específica'
  })
  @ApiParam({ 
    name: 'id', 
    description: 'ID único de la FSE a invalidar',
    example: '123e4567-e89b-12d3-a456-426614174000'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'FSE invalidada exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'FSE invalidada correctamente' },
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
    description: 'FSE no encontrada' 
  })
  async invalidate(@Param('id') id: string) {
    this.logger.log(`Recibida solicitud para invalidar FSE con ID: ${id}`);
    return this.fseService.invalidate(id);
  }

  @Get('estado/:codigoGeneracion')
  @ApiOperation({ 
    summary: 'Consultar estado de FSE en MH',
    description: 'Consulta el estado actual de una FSE en el Ministerio de Hacienda'
  })
  @ApiParam({ 
    name: 'codigoGeneracion', 
    description: 'Código de generación de la FSE',
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
    description: 'FSE no encontrada en MH' 
  })
  async getEstado(@Param('codigoGeneracion') codigoGeneracion: string) {
    this.logger.log(`Recibida solicitud para consultar estado de FSE: ${codigoGeneracion}`);
    return this.fseService.getEstado(codigoGeneracion);
  }

  @Post('validar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Validar FSE sin enviar',
    description: 'Valida la estructura y datos de una FSE sin enviarla al Ministerio de Hacienda'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'FSE válida',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'FSE válida' },
        warnings: { type: 'array', items: { type: 'string' } }
      }
    }
  })
  @ApiResponse({ 
    status: 400, 
    description: 'FSE inválida' 
  })
  async validate(@Body() createFseDto: CreateFseDto) {
    this.logger.log('Recibida solicitud para validar FSE');
    
    try {
      // Validar usando el servicio
      this.fseService.validateFseData(createFseDto);
      return {
        success: true,
        message: 'FSE válida',
        warnings: []
      };
    } catch (error) {
      return {
        success: false,
        message: 'FSE inválida',
        errors: [error.message]
      };
    }
  }
}