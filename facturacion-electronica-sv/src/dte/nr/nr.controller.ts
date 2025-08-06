import { Controller, Post, Body, Get, Param, HttpCode, HttpStatus, Logger } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse, ApiParam } from '@nestjs/swagger';
import { NrService } from './nr.service';
import { CreateNrDto } from './dto/create-nr.dto';

@ApiTags('NR - Nota de Remisión')
@Controller('dte/nr')
export class NrController {
  private readonly logger = new Logger(NrController.name);

  constructor(private readonly nrService: NrService) {}

  @Post()
  @HttpCode(HttpStatus.CREATED)
  @ApiOperation({ 
    summary: 'Crear NR (Nota de Remisión)',
    description: 'Crea una nueva Nota de Remisión, la firma digitalmente y la envía al Ministerio de Hacienda'
  })
  @ApiResponse({ 
    status: 201, 
    description: 'NR creada exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        codigoGeneracion: { type: 'string', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' },
        numeroControl: { type: 'string', example: 'DTE-04-0001-0001-0000001' },
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
  async create(@Body() createNrDto: CreateNrDto) {
    this.logger.log('Recibida solicitud para crear NR');
    return this.nrService.create(createNrDto);
  }

  @Get()
  @ApiOperation({ 
    summary: 'Obtener todas las NR',
    description: 'Obtiene una lista de todas las Notas de Remisión creadas'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Lista de NR obtenida exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Lista de NR obtenida' },
        data: { type: 'array', items: { type: 'object' } },
        total: { type: 'number', example: 0 }
      }
    }
  })
  async findAll() {
    this.logger.log('Recibida solicitud para obtener todas las NR');
    return this.nrService.getAll();
  }

  @Get(':id')
  @ApiOperation({ 
    summary: 'Obtener NR por ID',
    description: 'Obtiene una Nota de Remisión específica por su ID'
  })
  @ApiParam({ 
    name: 'id', 
    description: 'ID único de la NR',
    example: '123e4567-e89b-12d3-a456-426614174000'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'NR encontrada',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'NR encontrada' },
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
    description: 'NR no encontrada' 
  })
  async findOne(@Param('id') id: string) {
    this.logger.log(`Recibida solicitud para obtener NR con ID: ${id}`);
    return this.nrService.getById(id);
  }

  @Post(':id/invalidar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Invalidar NR',
    description: 'Invalida una Nota de Remisión específica'
  })
  @ApiParam({ 
    name: 'id', 
    description: 'ID único de la NR a invalidar',
    example: '123e4567-e89b-12d3-a456-426614174000'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'NR invalidada exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'NR invalidada correctamente' },
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
    description: 'NR no encontrada' 
  })
  async invalidate(@Param('id') id: string) {
    this.logger.log(`Recibida solicitud para invalidar NR con ID: ${id}`);
    return this.nrService.invalidate(id);
  }

  @Get('estado/:codigoGeneracion')
  @ApiOperation({ 
    summary: 'Consultar estado de NR en MH',
    description: 'Consulta el estado actual de una NR en el Ministerio de Hacienda'
  })
  @ApiParam({ 
    name: 'codigoGeneracion', 
    description: 'Código de generación de la NR',
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
    description: 'NR no encontrada en MH' 
  })
  async getEstado(@Param('codigoGeneracion') codigoGeneracion: string) {
    this.logger.log(`Recibida solicitud para consultar estado de NR: ${codigoGeneracion}`);
    return this.nrService.getEstado(codigoGeneracion);
  }

  @Get('ejemplo')
  @ApiOperation({ 
    summary: 'Obtener ejemplo de NR',
    description: 'Obtiene un ejemplo completo de Nota de Remisión para referencia'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Ejemplo de NR obtenido exitosamente',
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
    this.logger.log('Recibida solicitud para obtener ejemplo de NR');
    return this.nrService.getExample();
  }

  @Post('validar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Validar NR sin enviar',
    description: 'Valida la estructura y datos de una NR sin enviarla al Ministerio de Hacienda'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'NR válida',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'NR válida' },
        warnings: { type: 'array', items: { type: 'string' } }
      }
    }
  })
  @ApiResponse({ 
    status: 400, 
    description: 'NR inválida' 
  })
  async validate(@Body() createNrDto: CreateNrDto) {
    this.logger.log('Recibida solicitud para validar NR');
    
    try {
      // Validar usando el servicio
      this.nrService.validateNrData(createNrDto);
      return {
        success: true,
        message: 'NR válida',
        warnings: []
      };
    } catch (error) {
      return {
        success: false,
        message: 'NR inválida',
        errors: [error.message]
      };
    }
  }
}