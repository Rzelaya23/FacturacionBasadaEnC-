import { Injectable } from '@nestjs/common';
import { InjectConnection } from '@nestjs/typeorm';
import { Connection } from 'typeorm';
import { ConfigService } from '@nestjs/config';
import axios from 'axios';

@Injectable()
export class AppService {
  constructor(
    @InjectConnection() private readonly connection: Connection,
    private readonly configService: ConfigService,
  ) {}
  getHello() {
    return {
      message: 'Sistema de Facturaci√≥n Electr√≥nica El Salvador - API funcionando correctamente',
      timestamp: new Date().toISOString(),
      version: '1.0.0',
      environment: process.env.NODE_ENV || 'development'
    };
  }

  async healthCheck() {
    const [dbStatus, mhStatus] = await Promise.all([
      this.checkDatabaseConnection(),
      this.checkMhConnection()
    ]);

    return {
      status: dbStatus && mhStatus ? 'OK' : 'DEGRADED',
      timestamp: new Date().toISOString(),
      uptime: process.uptime(),
      database: dbStatus ? 'Connected' : 'Disconnected',
      mh_connection: mhStatus ? 'Available' : 'Unavailable',
      memory: {
        used: Math.round(process.memoryUsage().heapUsed / 1024 / 1024 * 100) / 100,
        total: Math.round(process.memoryUsage().heapTotal / 1024 / 1024 * 100) / 100
      }
    };
  }

  /**
   * Verificar conexi®Æn real a la base de datos
   */
  private async checkDatabaseConnection(): Promise<boolean> {
    try {
      await this.connection.query('SELECT 1');
      return true;
    } catch (error) {
      console.error('Database connection error:', error.message);
      return false;
    }
  }

  /**
   * Verificar conexi®Æn real al Ministerio de Hacienda
   */
  private async checkMhConnection(): Promise<boolean> {
    try {
      const environment = this.configService.get('MH_ENVIRONMENT', 'test');
      const baseUrl = environment === 'production' 
        ? this.configService.get('MH_BASE_URL_PROD')
        : this.configService.get('MH_BASE_URL_TEST');
      
      if (!baseUrl) return false;

      const response = await axios.get(`${baseUrl}/health`, { 
        timeout: 5000,
        validateStatus: () => true // Accept any status code
      });
      
      return response.status < 500; // Consider 4xx as available but unauthorized
    } catch (error) {
      console.error('MH connection error:', error.message);
      return false;
    }
  }
}