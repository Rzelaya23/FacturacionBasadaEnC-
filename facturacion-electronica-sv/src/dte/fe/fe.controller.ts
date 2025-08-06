import { Controller, Post, Body, Get, Param, HttpCode, HttpStatus, Logger } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse, ApiBearerAuth } from '@nestjs/swagger';
import { FeService } from './fe.service';
import { CreateFacturaElectronicaDto } from './dto/create-factura-electronica.dto';

@ApiTags('Factura Electrónica (FE)')
@Controller('dte/fe')
export class FeController {
  private readonly logger = new Logger(FeController.name);

  constructor(private readonly feService: FeService) {}

  @Post()
  @HttpCode(HttpStatus.CREATED)
  @ApiOperation({ 
    summary: 'Crear y enviar Factura Electrónica',
    description: 'Crea una nueva Factura Electrónica (FE), la firma digitalmente y la envía al Ministerio de Hacienda'
  })
  @ApiResponse({ 
    status: 201, 
    description: 'Factura Electrónica creada y enviada exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Factura Electrónica procesada exitosamente' },
        data: {
          type: 'object',
          properties: {
            codigoGeneracion: { type: 'string', example: 'A1B2C3D4-E5F6-7890-ABCD-123456789012' },
            numeroControl: { type: 'string', example: 'DTE-01-00000001-000000000000001' },
            selloRecibido: { type: 'string', example: '2024-01-15T14:30:00.000Z' },
            estado: { type: 'string', example: 'PROCESADO' },
            clasificaMsg: { type: 'string', example: '01' },
            codigoMsg: { type: 'string', example: '001' },
            descripcionMsg: { type: 'string', example: 'DTE procesado correctamente' }
          }
        }
      }
    }
  })
  @ApiResponse({ 
    status: 400, 
    description: 'Error en validación de datos',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: false },
        message: { type: 'string', example: 'Error en validación de datos' },
        errors: { type: 'array', items: { type: 'string' } }
      }
    }
  })
  @ApiResponse({ 
    status: 500, 
    description: 'Error interno del servidor',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: false },
        message: { type: 'string', example: 'Error interno del servidor' },
        error: { type: 'string' }
      }
    }
  })
  async create(@Body() createFeDto: CreateFacturaElectronicaDto) {
    try {
      this.logger.log(`Iniciando procesamiento de FE - Código: ${createFeDto.identificacion.codigoGeneracion}`);
      
      const result = await this.feService.create(createFeDto);
      
      this.logger.log(`FE procesada exitosamente - Código: ${createFeDto.identificacion.codigoGeneracion}`);
      
      return {
        success: true,
        message: 'Factura Electrónica procesada exitosamente',
        data: result
      };
    } catch (error) {
      this.logger.error(`Error procesando FE: ${error.message}`, error.stack);
      throw error;
    }
  }

  @Post('sign')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Solo firmar Factura Electrónica',
    description: 'Firma digitalmente una Factura Electrónica sin enviarla al MH (útil para pruebas)'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Factura Electrónica firmada exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Factura Electrónica firmada exitosamente' },
        data: {
          type: 'object',
          properties: {
            documento: { type: 'string', description: 'Documento firmado en base64' },
            codigoGeneracion: { type: 'string' },
            numeroControl: { type: 'string' }
          }
        }
      }
    }
  })
  async signOnly(@Body() createFeDto: CreateFacturaElectronicaDto) {
    try {
      this.logger.log(`Iniciando firmado de FE - Código: ${createFeDto.identificacion.codigoGeneracion}`);
      
      const result = await this.feService.signOnly(createFeDto);
      
      return {
        success: true,
        message: 'Factura Electrónica firmada exitosamente',
        data: result
      };
    } catch (error) {
      this.logger.error(`Error firmando FE: ${error.message}`, error.stack);
      throw error;
    }
  }

  @Post('validate')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Validar estructura de Factura Electrónica',
    description: 'Valida la estructura y datos de una Factura Electrónica sin procesarla'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Validación exitosa',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Factura Electrónica válida' },
        data: {
          type: 'object',
          properties: {
            valid: { type: 'boolean', example: true },
            warnings: { type: 'array', items: { type: 'string' } },
            calculatedTotals: {
              type: 'object',
              properties: {
                subTotal: { type: 'number' },
                totalIva: { type: 'number' },
                totalPagar: { type: 'number' }
              }
            }
          }
        }
      }
    }
  })
  async validate(@Body() createFeDto: CreateFacturaElectronicaDto) {
    try {
      this.logger.log(`Validando FE - Código: ${createFeDto.identificacion.codigoGeneracion}`);
      
      const result = await this.feService.validate(createFeDto);
      
      return {
        success: true,
        message: 'Factura Electrónica válida',
        data: result
      };
    } catch (error) {
      this.logger.error(`Error validando FE: ${error.message}`, error.stack);
      throw error;
    }
  }

  @Get('status/:codigoGeneracion')
  @ApiOperation({ 
    summary: 'Consultar estado de Factura Electrónica',
    description: 'Consulta el estado actual de una FE en el Ministerio de Hacienda'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Estado consultado exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Estado consultado exitosamente' },
        data: {
          type: 'object',
          properties: {
            codigoGeneracion: { type: 'string' },
            estado: { type: 'string', example: 'PROCESADO' },
            fechaProcesamiento: { type: 'string' },
            observaciones: { type: 'string' }
          }
        }
      }
    }
  })
  async getStatus(@Param('codigoGeneracion') codigoGeneracion: string) {
    try {
      this.logger.log(`Consultando estado de FE - Código: ${codigoGeneracion}`);
      
      const result = await this.feService.getStatus(codigoGeneracion);
      
      return {
        success: true,
        message: 'Estado consultado exitosamente',
        data: result
      };
    } catch (error) {
      this.logger.error(`Error consultando estado FE: ${error.message}`, error.stack);
      throw error;
    }
  }

  @Get('generate-codes')
  @ApiOperation({ 
    summary: 'Generar códigos automáticos para FE',
    description: 'Genera código de generación y número de control automáticamente para una nueva FE'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Códigos generados exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Códigos generados exitosamente' },
        data: {
          type: 'object',
          properties: {
            codigoGeneracion: { type: 'string', example: 'A1B2C3D4-E5F6-7890-ABCD-123456789012' },
            numeroControl: { type: 'string', example: 'DTE-01-0001-0001-000000000000001' },
            fecEmi: { type: 'string', example: '2024-01-15' },
            horEmi: { type: 'string', example: '14:30:00' }
          }
        }
      }
    }
  })
  async generateCodes() {
    try {
      const result = await this.feService.generateCodes();
      
      return {
        success: true,
        message: 'Códigos generados exitosamente',
        data: result
      };
    } catch (error) {
      this.logger.error(`Error generando códigos: ${error.message}`, error.stack);
      throw error;
    }
  }

  @Get('health')
  @ApiOperation({ 
    summary: 'Verificar salud del módulo FE',
    description: 'Verifica que todos los servicios necesarios para FE estén disponibles'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Estado de salud del módulo FE',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Módulo FE operativo' },
        data: {
          type: 'object',
          properties: {
            firmador: { type: 'boolean', example: true },
            mh: { type: 'boolean', example: true },
            database: { type: 'boolean', example: true }
          }
        }
      }
    }
  })
  async healthCheck() {
    try {
      const result = await this.feService.healthCheck();
      
      return {
        success: true,
        message: 'Módulo FE operativo',
        data: result
      };
    } catch (error) {
      this.logger.error(`Error en health check FE: ${error.message}`, error.stack);
      throw error;
    }
  }
}