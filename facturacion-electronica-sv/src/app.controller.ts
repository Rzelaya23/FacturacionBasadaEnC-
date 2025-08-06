import { Controller, Get } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse } from '@nestjs/swagger';
import { AppService } from './app.service';

@ApiTags('Sistema')
@Controller()
export class AppController {
  constructor(private readonly appService: AppService) {}

  @Get()
  @ApiOperation({ summary: 'Verificar estado del sistema' })
  @ApiResponse({ 
    status: 200, 
    description: 'Sistema funcionando correctamente',
    schema: {
      type: 'object',
      properties: {
        message: { type: 'string' },
        timestamp: { type: 'string' },
        version: { type: 'string' },
        environment: { type: 'string' }
      }
    }
  })
  getHello() {
    return this.appService.getHello();
  }

  @Get('health')
  @ApiOperation({ summary: 'Health check del sistema' })
  @ApiResponse({ 
    status: 200, 
    description: 'Estado de salud del sistema',
    schema: {
      type: 'object',
      properties: {
        status: { type: 'string' },
        timestamp: { type: 'string' },
        uptime: { type: 'number' },
        database: { type: 'string' },
        mh_connection: { type: 'string' }
      }
    }
  })
  healthCheck() {
    return this.appService.healthCheck();
  }
}