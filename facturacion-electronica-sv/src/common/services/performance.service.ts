import { Injectable, Logger } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';

/**
 * Servicio de Optimización de Performance
 * Maneja pools de conexiones, timeouts y retry logic
 */
@Injectable()
export class PerformanceService {
  private readonly logger = new Logger(PerformanceService.name);
  private readonly metrics = new Map<string, number[]>();

  constructor(private configService: ConfigService) {}

  /**
   * Ejecutar operación con retry exponential backoff
   */
  async executeWithRetry<T>(
    operation: () => Promise<T>,
    operationName: string,
    maxRetries: number = 3,
    baseDelay: number = 1000
  ): Promise<T> {
    let lastError: Error;
    
    for (let attempt = 1; attempt <= maxRetries; attempt++) {
      try {
        const startTime = Date.now();
        const result = await operation();
        const duration = Date.now() - startTime;
        
        this.recordMetric(operationName, duration);
        this.logger.debug(`${operationName} completed in ${duration}ms (attempt ${attempt})`);
        
        return result;
      } catch (error) {
        lastError = error;
        
        if (attempt === maxRetries) {
          this.logger.error(`${operationName} failed after ${maxRetries} attempts:`, error);
          break;
        }

        const delay = baseDelay * Math.pow(2, attempt - 1);
        this.logger.warn(`${operationName} failed (attempt ${attempt}), retrying in ${delay}ms:`, error.message);
        
        await this.sleep(delay);
      }
    }

    throw lastError;
  }

  /**
   * Ejecutar operación con timeout
   */
  async executeWithTimeout<T>(
    operation: () => Promise<T>,
    timeoutMs: number,
    operationName: string
  ): Promise<T> {
    const timeoutPromise = new Promise<never>((_, reject) => {
      setTimeout(() => {
        reject(new Error(`${operationName} timed out after ${timeoutMs}ms`));
      }, timeoutMs);
    });

    const startTime = Date.now();
    
    try {
      const result = await Promise.race([operation(), timeoutPromise]);
      const duration = Date.now() - startTime;
      
      this.recordMetric(operationName, duration);
      this.logger.debug(`${operationName} completed in ${duration}ms`);
      
      return result;
    } catch (error) {
      const duration = Date.now() - startTime;
      this.logger.error(`${operationName} failed after ${duration}ms:`, error);
      throw error;
    }
  }

  /**
   * Ejecutar múltiples operaciones en paralelo con límite de concurrencia
   */
  async executeInBatches<T, R>(
    items: T[],
    operation: (item: T) => Promise<R>,
    batchSize: number = 5,
    operationName: string = 'batch_operation'
  ): Promise<R[]> {
    const results: R[] = [];
    const startTime = Date.now();

    for (let i = 0; i < items.length; i += batchSize) {
      const batch = items.slice(i, i + batchSize);
      const batchPromises = batch.map(item => operation(item));
      
      try {
        const batchResults = await Promise.all(batchPromises);
        results.push(...batchResults);
        
        this.logger.debug(`Processed batch ${Math.floor(i / batchSize) + 1}/${Math.ceil(items.length / batchSize)} for ${operationName}`);
      } catch (error) {
        this.logger.error(`Batch ${Math.floor(i / batchSize) + 1} failed for ${operationName}:`, error);
        throw error;
      }
    }

    const duration = Date.now() - startTime;
    this.recordMetric(operationName, duration);
    this.logger.log(`${operationName} completed ${items.length} items in ${duration}ms`);

    return results;
  }

  /**
   * Circuit breaker pattern
   */
  async executeWithCircuitBreaker<T>(
    operation: () => Promise<T>,
    operationName: string,
    failureThreshold: number = 5,
    resetTimeoutMs: number = 60000
  ): Promise<T> {
    const circuitKey = `circuit:${operationName}`;
    const circuit = this.getCircuitState(circuitKey);

    // Si el circuito está abierto, verificar si es tiempo de intentar de nuevo
    if (circuit.state === 'OPEN') {
      if (Date.now() - circuit.lastFailureTime < resetTimeoutMs) {
        throw new Error(`Circuit breaker is OPEN for ${operationName}`);
      } else {
        circuit.state = 'HALF_OPEN';
        this.logger.log(`Circuit breaker for ${operationName} is now HALF_OPEN`);
      }
    }

    try {
      const result = await operation();
      
      // Operación exitosa - resetear circuito
      if (circuit.state === 'HALF_OPEN') {
        circuit.state = 'CLOSED';
        circuit.failureCount = 0;
        this.logger.log(`Circuit breaker for ${operationName} is now CLOSED`);
      }

      return result;
    } catch (error) {
      circuit.failureCount++;
      circuit.lastFailureTime = Date.now();

      if (circuit.failureCount >= failureThreshold) {
        circuit.state = 'OPEN';
        this.logger.error(`Circuit breaker for ${operationName} is now OPEN after ${circuit.failureCount} failures`);
      }

      throw error;
    }
  }

  /**
   * Registrar métrica de performance
   */
  private recordMetric(operationName: string, duration: number): void {
    if (!this.metrics.has(operationName)) {
      this.metrics.set(operationName, []);
    }

    const metrics = this.metrics.get(operationName);
    metrics.push(duration);

    // Mantener solo las últimas 100 métricas
    if (metrics.length > 100) {
      metrics.shift();
    }
  }

  /**
   * Obtener estadísticas de performance
   */
  getPerformanceStats(): Record<string, any> {
    const stats: Record<string, any> = {};

    for (const [operation, durations] of this.metrics.entries()) {
      if (durations.length === 0) continue;

      const sorted = [...durations].sort((a, b) => a - b);
      const avg = durations.reduce((a, b) => a + b, 0) / durations.length;
      const p50 = sorted[Math.floor(sorted.length * 0.5)];
      const p95 = sorted[Math.floor(sorted.length * 0.95)];
      const p99 = sorted[Math.floor(sorted.length * 0.99)];

      stats[operation] = {
        count: durations.length,
        avg: Math.round(avg),
        min: Math.min(...durations),
        max: Math.max(...durations),
        p50,
        p95,
        p99
      };
    }

    return stats;
  }

  /**
   * Obtener estado del circuit breaker
   */
  private getCircuitState(key: string): any {
    if (!this.circuitStates) {
      this.circuitStates = new Map();
    }

    if (!this.circuitStates.has(key)) {
      this.circuitStates.set(key, {
        state: 'CLOSED',
        failureCount: 0,
        lastFailureTime: 0
      });
    }

    return this.circuitStates.get(key);
  }

  private circuitStates = new Map<string, any>();

  /**
   * Utilidad para sleep
   */
  private sleep(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  /**
   * Limpiar métricas antiguas
   */
  clearOldMetrics(): void {
    this.metrics.clear();
    this.logger.log('Performance metrics cleared');
  }
}