import { Module, Global } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { TypeOrmModule } from '@nestjs/typeorm';

// Services
import { CacheService } from '../services/cache.service';
import { PerformanceService } from '../services/performance.service';
import { LoggerService } from '../services/logger.service';
import { HealthService } from '../services/health.service';

// Controllers
import { HealthController } from '../controllers/health.controller';

// Entities
import { Dte } from '../entities/dte.entity';

/**
 * Módulo Global de Optimización
 * Proporciona servicios de cache, performance, logging y health checks
 */
@Global()
@Module({
  imports: [
    ConfigModule,
    TypeOrmModule.forFeature([Dte])
  ],
  providers: [
    CacheService,
    PerformanceService,
    LoggerService,
    HealthService
  ],
  controllers: [
    HealthController
  ],
  exports: [
    CacheService,
    PerformanceService,
    LoggerService,
    HealthService
  ]
})
export class OptimizationModule {
  constructor(
    private cacheService: CacheService,
    private performanceService: PerformanceService
  ) {
    // Configurar limpieza automática de cache cada 10 minutos
    setInterval(() => {
      this.cacheService.cleanExpired();
    }, 600000);

    // Configurar limpieza de métricas cada hora
    setInterval(() => {
      this.performanceService.clearOldMetrics();
    }, 3600000);
  }
}