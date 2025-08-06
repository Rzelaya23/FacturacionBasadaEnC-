import { Controller, Post, Body, Get, Param, HttpCode, HttpStatus, Logger } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse, ApiParam } from '@nestjs/swagger';
import { CcfService } from './ccf.service';
import { CreateCcfDto } from './dto/create-ccf.dto';

@ApiTags('Comprobante Crédito Fiscal (CCF)')
@Controller('dte/ccf')
export class CcfController {
  private readonly logger = new Logger(CcfController.name);

  constructor(private readonly ccfService: CcfService) {}

  @Post('generar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Generar CCF',
    description: 'Genera un Comprobante de Crédito Fiscal completo con firma digital y envío al MH'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'CCF generado exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'CCF generado exitosamente' },
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
  async generarCcf(@Body() createCcfDto: CreateCcfDto) {
    this.logger.log('Iniciando generación de CCF');
    
    try {
      const result = await this.ccfService.generarCcf(createCcfDto);
      
      this.logger.log(`CCF generado exitosamente: ${result.codigoGeneracion}`);
      
      return {
        success: true,
        message: 'CCF generado exitosamente',
        data: result
      };
    } catch (error) {
      this.logger.error(`Error generando CCF: ${error.message}`, error.stack);
      throw error;
    }
  }

  @Post('firmar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Firmar CCF',
    description: 'Firma digitalmente un CCF sin enviarlo al MH'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'CCF firmado exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'CCF firmado exitosamente' },
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
  async firmarCcf(@Body() createCcfDto: CreateCcfDto) {
    this.logger.log('Iniciando firma de CCF');
    
    try {
      const result = await this.ccfService.firmarCcf(createCcfDto);
      
      this.logger.log(`CCF firmado exitosamente: ${result.codigoGeneracion}`);
      
      return {
        success: true,
        message: 'CCF firmado exitosamente',
        data: result
      };
    } catch (error) {
      this.logger.error(`Error firmando CCF: ${error.message}`, error.stack);
      throw error;
    }
  }

  @Post('enviar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Enviar CCF al MH',
    description: 'Envía un CCF ya firmado al Ministerio de Hacienda'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'CCF enviado exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        message: { type: 'string', example: 'CCF enviado al MH exitosamente' },
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
  async enviarCcf(@Body() body: { documento: string; codigoGeneracion: string }) {
    this.logger.log(`Iniciando envío de CCF al MH: ${body.codigoGeneracion}`);
    
    try {
      const result = await this.ccfService.enviarCcfAlMh(body.documento, body.codigoGeneracion);
      
      this.logger.log(`CCF enviado exitosamente al MH: ${body.codigoGeneracion}`);
      
      return {
        success: true,
        message: 'CCF enviado al MH exitosamente',
        data: result
      };
    } catch (error) {
      this.logger.error(`Error enviando CCF al MH: ${error.message}`, error.stack);
      throw error;
    }
  }

  @Get('consultar/:codigoGeneracion')
  @ApiOperation({ 
    summary: 'Consultar estado de CCF',
    description: 'Consulta el estado de un CCF en el MH por su código de generación'
  })
  @ApiParam({ name: 'codigoGeneracion', description: 'Código de generación del CCF' })
  @ApiResponse({ 
    status: 200, 
    description: 'Estado del CCF consultado exitosamente',
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
  @ApiResponse({ status: 404, description: 'CCF no encontrado' })
  @ApiResponse({ status: 500, description: 'Error interno del servidor' })
  async consultarEstado(@Param('codigoGeneracion') codigoGeneracion: string) {
    this.logger.log(`Consultando estado de CCF: ${codigoGeneracion}`);
    
    try {
      const result = await this.ccfService.consultarEstado(codigoGeneracion);
      
      this.logger.log(`Estado consultado exitosamente: ${codigoGeneracion}`);
      
      return {
        success: true,
        message: 'Estado consultado exitosamente',
        data: result
      };
    } catch (error) {
      this.logger.error(`Error consultando estado de CCF: ${error.message}`, error.stack);
      throw error;
    }
  }

  @Post('validar')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Validar datos de CCF',
    description: 'Valida los datos de un CCF sin procesarlo'
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
  async validarDatos(@Body() createCcfDto: CreateCcfDto) {
    this.logger.log('Validando datos de CCF');
    
    try {
      const result = await this.ccfService.validarDatos(createCcfDto);
      
      this.logger.log('Datos de CCF validados exitosamente');
      
      return {
        success: true,
        message: result.valido ? 'Datos válidos' : 'Datos con errores',
        data: result
      };
    } catch (error) {
      this.logger.error(`Error validando datos de CCF: ${error.message}`, error.stack);
      throw error;
    }
  }

  @Get('ejemplo')
  @ApiOperation({ 
    summary: 'Obtener ejemplo de CCF',
    description: 'Retorna un ejemplo de estructura de datos para CCF'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Ejemplo de CCF',
    type: CreateCcfDto
  })
  async obtenerEjemplo() {
    this.logger.log('Generando ejemplo de CCF');
    
    const ejemplo = await this.ccfService.generarEjemplo();
    
    return {
      success: true,
      message: 'Ejemplo de CCF generado',
      data: ejemplo
    };
  }
}