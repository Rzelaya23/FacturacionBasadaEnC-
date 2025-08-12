import { Injectable, LoggerService as NestLoggerService } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';

/**
 * Servicio de Logging Estructurado Avanzado
 * Proporciona logging con contexto y métricas
 */
@Injectable()
export class LoggerService implements NestLoggerService {
  private readonly logLevel: string;
  private readonly context: string = 'Application';

  constructor(private configService: ConfigService) {
    this.logLevel = this.configService.get<string>('LOG_LEVEL', 'info');
  }

  /**
   * Log de información general
   */
  log(message: string, context?: string, meta?: any): void {
    this.writeLog('info', message, context, meta);
  }

  /**
   * Log de errores
   */
  error(message: string, trace?: string, context?: string, meta?: any): void {
    this.writeLog('error', message, context, { ...meta, trace });
  }

  /**
   * Log de advertencias
   */
  warn(message: string, context?: string, meta?: any): void {
    this.writeLog('warn', message, context, meta);
  }

  /**
   * Log de debug (solo en desarrollo)
   */
  debug(message: string, context?: string, meta?: any): void {
    this.writeLog('debug', message, context, meta);
  }

  /**
   * Log de información detallada
   */
  verbose(message: string, context?: string, meta?: any): void {
    this.writeLog('verbose', message, context, meta);
  }

  /**
   * Log específico para DTEs
   */
  logDTE(
    action: string,
    tipoDte: string,
    codigoGeneracion: string,
    status: 'success' | 'error' | 'warning',
    details?: any
  ): void {
    const logData = {
      action,
      tipoDte,
      codigoGeneracion,
      status,
      details,
      timestamp: new Date().toISOString(),
      module: 'DTE'
    };

    const message = `DTE ${action} - ${tipoDte} - ${codigoGeneracion} - ${status.toUpperCase()}`;
    
    if (status === 'error') {
      this.error(message, undefined, 'DTE', logData);
    } else if (status === 'warning') {
      this.warn(message, 'DTE', logData);
    } else {
      this.log(message, 'DTE', logData);
    }
  }

  /**
   * Log específico para integración MH
   */
  logMH(
    operation: string,
    endpoint: string,
    status: 'success' | 'error',
    duration: number,
    details?: any
  ): void {
    const logData = {
      operation,
      endpoint,
      status,
      duration,
      details,
      timestamp: new Date().toISOString(),
      module: 'MH_INTEGRATION'
    };

    const message = `MH ${operation} - ${endpoint} - ${status.toUpperCase()} - ${duration}ms`;
    
    if (status === 'error') {
      this.error(message, undefined, 'MH', logData);
    } else {
      this.log(message, 'MH', logData);
    }
  }

  /**
   * Log específico para firmado digital
   */
  logFirmador(
    operation: string,
    documentType: string,
    status: 'success' | 'error',
    duration: number,
    details?: any
  ): void {
    const logData = {
      operation,
      documentType,
      status,
      duration,
      details,
      timestamp: new Date().toISOString(),
      module: 'FIRMADOR'
    };

    const message = `FIRMADOR ${operation} - ${documentType} - ${status.toUpperCase()} - ${duration}ms`;
    
    if (status === 'error') {
      this.error(message, undefined, 'FIRMADOR', logData);
    } else {
      this.log(message, 'FIRMADOR', logData);
    }
  }

  /**
   * Log de métricas de performance
   */
  logMetrics(operation: string, metrics: any): void {
    const logData = {
      operation,
      metrics,
      timestamp: new Date().toISOString(),
      module: 'METRICS'
    };

    this.log(`METRICS ${operation}`, 'METRICS', logData);
  }

  /**
   * Log de auditoría
   */
  logAudit(
    user: string,
    action: string,
    resource: string,
    result: 'success' | 'failure',
    details?: any
  ): void {
    const logData = {
      user,
      action,
      resource,
      result,
      details,
      timestamp: new Date().toISOString(),
      module: 'AUDIT'
    };

    const message = `AUDIT ${user} - ${action} - ${resource} - ${result.toUpperCase()}`;
    this.log(message, 'AUDIT', logData);
  }

  /**
   * Escribir log con formato estructurado
   */
  private writeLog(level: string, message: string, context?: string, meta?: any): void {
    if (!this.shouldLog(level)) {
      return;
    }

    const logEntry = {
      level,
      message,
      context: context || this.context,
      timestamp: new Date().toISOString(),
      pid: process.pid,
      ...meta
    };

    // En producción, esto se enviaría a un sistema de logging como ELK Stack
    // Por ahora, usamos console con formato JSON
    const output = JSON.stringify(logEntry, null, this.isProduction() ? 0 : 2);
    
    switch (level) {
      case 'error':
        console.error(output);
        break;
      case 'warn':
        console.warn(output);
        break;
      case 'debug':
      case 'verbose':
        console.debug(output);
        break;
      default:
        console.log(output);
    }
  }

  /**
   * Determinar si se debe loggear según el nivel configurado
   */
  private shouldLog(level: string): boolean {
    const levels = ['error', 'warn', 'info', 'debug', 'verbose'];
    const currentLevelIndex = levels.indexOf(this.logLevel);
    const messageLevelIndex = levels.indexOf(level);
    
    return messageLevelIndex <= currentLevelIndex;
  }

  /**
   * Verificar si estamos en producción
   */
  private isProduction(): boolean {
    return this.configService.get<string>('NODE_ENV') === 'production';
  }
}