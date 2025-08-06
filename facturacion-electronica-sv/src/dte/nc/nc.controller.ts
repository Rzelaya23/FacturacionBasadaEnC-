import { Controller, Post, Body, Get, Param, HttpCode, HttpStatus, Logger } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse, ApiParam } from '@nestjs/swagger';
import { NcService } from './nc.service';
import { CreateNcDto } from './dto/create-nc.dto';

@ApiTags('Nota de Crédito (NC)')
@Controller('dte/nc')
export class NcController {
  private readonly logger = new Logger(NcController.name);

  constructor(private readonly ncService: NcService) {}

  @Post('generar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Generar NC',
    description: 'Genera una Nota de Crédito completa con firma digital y envío al MH'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'NC generada exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'NC generada exitosamente' },
        data: {
          type: 'object',
          properties: {
            codigoGeneracion: { type: 'string', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' },
            selloRecibido: { type: 'string', example: 'MH123456789' },
            fhProcesamiento: { type: 'string', example: '2024-07-15T10:30:00.000Z' },
            clasificaMsg: { type: 'string', example: '01' },
            codigoMsg: { type: 'string', example: '001' },
            descripcionMsg: { type: 'string', example: 'Procesado Correctamente' },
            observaciones: { type: 'array', items: { type: 'string' } }
          }
        }
      }
    }
  })
  @ApiResponse({ status: 400, description: 'Datos inválidos' })
  @ApiResponse({ status: 500, description: 'Error interno del servidor' })
  async generarNc(@Body() createNcDto: CreateNcDto) {
    this.logger.log('Iniciando generación de NC');
    
    try {
      const result = await this.ncService.generarNc(createNcDto);
      
      this.logger.log(`NC generada exitosamente: ${result.codigoGeneracion}`);
      
      return {
        success: true,
        message: 'NC generada exitosamente',
        data: result
      };
    } catch (error) {
      this.logger.error(`Error generando NC: ${error.message}`, error.stack);
      throw error;
    }
  }

  @Post('firmar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Firmar NC',
    description: 'Firma digitalmente una NC sin enviarla al MH'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'NC firmada exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'NC firmada exitosamente' },
        data: {
          type: 'object',
          properties: {
            documento: { type: 'string', description: 'JSON del documento firmado' },
            codigoGeneracion: { type: 'string', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' }
          }
        }
      }
    }
  })
  @ApiResponse({ status: 400, description: 'Datos inválidos' })
  @ApiResponse({ status: 500, description: 'Error interno del servidor' })
  async firmarNc(@Body() createNcDto: CreateNcDto) {
    this.logger.log('Iniciando firma de NC');
    
    try {
      const result = await this.ncService.firmarNc(createNcDto);
      
      this.logger.log(`NC firmada exitosamente: ${result.codigoGeneracion}`);
      
      return {
        success: true,
        message: 'NC firmada exitosamente',
        data: result
      };
    } catch (error) {
      this.logger.error(`Error firmando NC: ${error.message}`, error.stack);
      throw error;
    }
  }

  @Post('enviar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Enviar NC al MH',
    description: 'Envía una NC ya firmada al Ministerio de Hacienda'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'NC enviada exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'NC enviada al MH exitosamente' },
        data: {
          type: 'object',
          properties: {
            selloRecibido: { type: 'string', example: 'MH123456789' },
            fhProcesamiento: { type: 'string', example: '2024-07-15T10:30:00.000Z' },
            clasificaMsg: { type: 'string', example: '01' },
            codigoMsg: { type: 'string', example: '001' },
            descripcionMsg: { type: 'string', example: 'Procesado Correctamente' },
            observaciones: { type: 'array', items: { type: 'string' } }
          }
        }
      }
    }
  })
  @ApiResponse({ status: 400, description: 'Datos inválidos' })
  @ApiResponse({ status: 500, description: 'Error interno del servidor' })
  async enviarNc(@Body() body: { documento: string; codigoGeneracion: string }) {
    this.logger.log(`Iniciando envío de NC al MH: ${body.codigoGeneracion}`);
    
    try {
      const result = await this.ncService.enviarNcAlMh(body.documento, body.codigoGeneracion);
      
      this.logger.log(`NC enviada exitosamente al MH: ${body.codigoGeneracion}`);
      
      return {
        success: true,
        message: 'NC enviada al MH exitosamente',
        data: result
      };
    } catch (error) {
      this.logger.error(`Error enviando NC al MH: ${error.message}`, error.stack);
      throw error;
    }
  }

  @Get('consultar/:codigoGeneracion')
  @ApiOperation({ 
    summary: 'Consultar estado de NC',
    description: 'Consulta el estado de una NC en el MH por su código de generación'
  })
  @ApiParam({ name: 'codigoGeneracion', description: 'Código de generación de la NC' })
  @ApiResponse({ 
    status: 200, 
    description: 'Estado de la NC consultado exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Estado consultado exitosamente' },
        data: {
          type: 'object',
          properties: {
            estado: { type: 'string', example: 'PROCESADO' },
            fhProcesamiento: { type: 'string', example: '2024-07-15T10:30:00.000Z' },
            clasificaMsg: { type: 'string', example: '01' },
            codigoMsg: { type: 'string', example: '001' },
            descripcionMsg: { type: 'string', example: 'Procesado Correctamente' }
          }
        }
      }
    }
  })
  @ApiResponse({ status: 404, description: 'NC no encontrada' })
  @ApiResponse({ status: 500, description: 'Error interno del servidor' })
  async consultarEstado(@Param('codigoGeneracion') codigoGeneracion: string) {
    this.logger.log(`Consultando estado de NC: ${codigoGeneracion}`);
    
    try {
      const result = await this.ncService.consultarEstado(codigoGeneracion);
      
      this.logger.log(`Estado consultado exitosamente: ${codigoGeneracion}`);
      
      return {
        success: true,
        message: 'Estado consultado exitosamente',
        data: result
      };
    } catch (error) {
      this.logger.error(`Error consultando estado de NC: ${error.message}`, error.stack);
      throw error;
    }
  }

  @Post('validar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Validar datos de NC',
    description: 'Valida los datos de una NC sin procesarla'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Datos validados exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'Datos válidos' },
        data: {
          type: 'object',
          properties: {
            valido: { type: 'boolean', example: true },
            errores: { type: 'array', items: { type: 'string' } },
            advertencias: { type: 'array', items: { type: 'string' } }
          }
        }
      }
    }
  })
  @ApiResponse({ status: 400, description: 'Datos inválidos' })
  async validarDatos(@Body() createNcDto: CreateNcDto) {
    this.logger.log('Validando datos de NC');
    
    try {
      const result = await this.ncService.validarDatos(createNcDto);
      
      this.logger.log('Datos de NC validados exitosamente');
      
      return {
        success: true,
        message: result.valido ? 'Datos válidos' : 'Datos con errores',
        data: result
      };
    } catch (error) {
      this.logger.error(`Error validando datos de NC: ${error.message}`, error.stack);
      throw error;
    }
  }

  @Get('ejemplo')
  @ApiOperation({ 
    summary: 'Obtener ejemplo de NC',
    description: 'Retorna un ejemplo de estructura de datos para NC'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Ejemplo de NC',
    type: CreateNcDto
  })
  async obtenerEjemplo() {
    this.logger.log('Generando ejemplo de NC');
    
    const ejemplo = await this.ncService.generarEjemplo();
    
    return {
      success: true,
      message: 'Ejemplo de NC generado',
      data: ejemplo
    };
  }
}