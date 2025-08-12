import {
  Injectable,
  NestInterceptor,
  ExecutionContext,
  CallHandler,
  Logger,
} from '@nestjs/common';
import { Observable } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { Request, Response } from 'express';
import { v4 as uuidv4 } from 'uuid';

/**
 * Interceptor de Logging Estructurado
 * Registra todas las requests/responses con información detallada
 */
@Injectable()
export class LoggingInterceptor implements NestInterceptor {
  private readonly logger = new Logger('HTTP');

  intercept(context: ExecutionContext, next: CallHandler): Observable<any> {
    const request = context.switchToHttp().getRequest<Request>();
    const response = context.switchToHttp().getResponse<Response>();
    
    // Generar trace ID único para seguimiento
    const traceId = request.headers['x-trace-id'] as string || uuidv4();
    request.headers['x-trace-id'] = traceId;
    response.setHeader('X-Trace-ID', traceId);

    const { method, url, ip, headers } = request;
    const userAgent = headers['user-agent'] || '';
    const startTime = Date.now();

    // Log de request entrante
    this.logger.log(
      `→ ${method} ${url}`,
      {
        traceId,
        method,
        url,
        ip,
        userAgent,
        timestamp: new Date().toISOString(),
        type: 'REQUEST'
      }
    );

    return next.handle().pipe(
      tap((data) => {
        const duration = Date.now() - startTime;
        const { statusCode } = response;

        // Log de response exitosa
        this.logger.log(
          `← ${method} ${url} ${statusCode} - ${duration}ms`,
          {
            traceId,
            method,
            url,
            statusCode,
            duration,
            responseSize: this.getResponseSize(data),
            timestamp: new Date().toISOString(),
            type: 'RESPONSE'
          }
        );
      }),
      catchError((error) => {
        const duration = Date.now() - startTime;
        const statusCode = error.status || 500;

        // Log de response con error
        this.logger.error(
          `← ${method} ${url} ${statusCode} - ${duration}ms - ERROR`,
          {
            traceId,
            method,
            url,
            statusCode,
            duration,
            error: error.message,
            timestamp: new Date().toISOString(),
            type: 'ERROR_RESPONSE'
          }
        );

        throw error;
      })
    );
  }

  /**
   * Calcular tamaño aproximado de la respuesta
   */
  private getResponseSize(data: any): number {
    try {
      return JSON.stringify(data).length;
    } catch {
      return 0;
    }
  }
}