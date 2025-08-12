import { Controller, Get } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse } from '@nestjs/swagger';
import { HealthService } from '../services/health.service';
import { CacheService } from '../services/cache.service';
import { PerformanceService } from '../services/performance.service';

/**
 * Controlador de Health Checks
 * Proporciona endpoints para monitoreo del sistema
 */
@ApiTags('Health & Monitoring')
@Controller('health')
export class HealthController {
  constructor(
    private readonly healthService: HealthService,
    private readonly cacheService: CacheService,
    private readonly performanceService: PerformanceService,
  ) {}

  /**
   * Health check básico
   */
  @Get()
  @ApiOperation({ summary: 'Health check básico del sistema' })
  @ApiResponse({ status: 200, description: 'Sistema funcionando correctamente' })
  async getHealth() {
    return {
      status: 'ok',
      timestamp: new Date().toISOString(),
      uptime: process.uptime(),
      version: process.env.npm_package_version || '1.0.0'
    };
  }

  /**
   * Health check detallado
   */
  @Get('detailed')
  @ApiOperation({ summary: 'Health check detallado de todos los servicios' })
  @ApiResponse({ status: 200, description: 'Estado detallado del sistema' })
  async getDetailedHealth() {
    const checks = await Promise.allSettled([
      this.healthService.checkDatabase(),
      this.healthService.checkMH(),
      this.healthService.checkFirmador(),
      this.healthService.checkDiskSpace(),
      this.healthService.checkMemory()
    ]);

    const [database, mh, firmador, disk, memory] = checks.map(result => 
      result.status === 'fulfilled' ? result.value : { status: 'error', error: result.reason?.message }
    );

    const overallStatus = checks.every(check => 
      check.status === 'fulfilled' && check.value.status === 'healthy'
    ) ? 'healthy' : 'unhealthy';

    return {
      status: overallStatus,
      timestamp: new Date().toISOString(),
      uptime: process.uptime(),
      version: process.env.npm_package_version || '1.0.0',
      checks: {
        database,
        mh,
        firmador,
        disk,
        memory
      }
    };
  }

  /**
   * Métricas de performance
   */
  @Get('metrics')
  @ApiOperation({ summary: 'Métricas de performance del sistema' })
  @ApiResponse({ status: 200, description: 'Métricas de performance' })
  async getMetrics() {
    const performanceStats = this.performanceService.getPerformanceStats();
    const cacheStats = this.cacheService.getStats();
    const memoryUsage = process.memoryUsage();

    return {
      timestamp: new Date().toISOString(),
      performance: performanceStats,
      cache: cacheStats,
      memory: {
        rss: Math.round(memoryUsage.rss / 1024 / 1024), // MB
        heapTotal: Math.round(memoryUsage.heapTotal / 1024 / 1024), // MB
        heapUsed: Math.round(memoryUsage.heapUsed / 1024 / 1024), // MB
        external: Math.round(memoryUsage.external / 1024 / 1024), // MB
      },
      uptime: process.uptime(),
      cpuUsage: process.cpuUsage()
    };
  }

  /**
   * Estado de la base de datos
   */
  @Get('database')
  @ApiOperation({ summary: 'Estado específico de la base de datos' })
  @ApiResponse({ status: 200, description: 'Estado de la base de datos' })
  async getDatabaseHealth() {
    return await this.healthService.checkDatabase();
  }

  /**
   * Estado del Ministerio de Hacienda
   */
  @Get('mh')
  @ApiOperation({ summary: 'Estado de conectividad con el MH' })
  @ApiResponse({ status: 200, description: 'Estado de conexión con MH' })
  async getMHHealth() {
    return await this.healthService.checkMH();
  }

  /**
   * Estado del firmador digital
   */
  @Get('firmador')
  @ApiOperation({ summary: 'Estado del servicio de firmado digital' })
  @ApiResponse({ status: 200, description: 'Estado del firmador' })
  async getFirmadorHealth() {
    return await this.healthService.checkFirmador();
  }

  /**
   * Limpiar cache manualmente
   */
  @Get('cache/clear')
  @ApiOperation({ summary: 'Limpiar cache del sistema' })
  @ApiResponse({ status: 200, description: 'Cache limpiado exitosamente' })
  async clearCache() {
    await this.cacheService.clear();
    this.cacheService.cleanExpired();
    
    return {
      status: 'success',
      message: 'Cache cleared successfully',
      timestamp: new Date().toISOString()
    };
  }

  /**
   * Información del sistema
   */
  @Get('system')
  @ApiOperation({ summary: 'Información del sistema operativo' })
  @ApiResponse({ status: 200, description: 'Información del sistema' })
  async getSystemInfo() {
    const os = require('os');
    
    return {
      platform: os.platform(),
      arch: os.arch(),
      cpus: os.cpus().length,
      totalMemory: Math.round(os.totalmem() / 1024 / 1024 / 1024), // GB
      freeMemory: Math.round(os.freemem() / 1024 / 1024 / 1024), // GB
      loadAverage: os.loadavg(),
      uptime: os.uptime(),
      nodeVersion: process.version,
      timestamp: new Date().toISOString()
    };
  }
}