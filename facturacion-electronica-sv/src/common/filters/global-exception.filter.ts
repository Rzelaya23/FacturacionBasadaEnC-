import {
  ExceptionFilter,
  Catch,
  ArgumentsHost,
  HttpException,
  HttpStatus,
  Logger,
} from '@nestjs/common';
import { Request, Response } from 'express';
import { v4 as uuidv4 } from 'uuid';

/**
 * Filtro Global de Excepciones
 * Maneja todos los errores de forma centralizada y consistente
 */
@Catch()
export class GlobalExceptionFilter implements ExceptionFilter {
  private readonly logger = new Logger(GlobalExceptionFilter.name);

  catch(exception: unknown, host: ArgumentsHost): void {
    const ctx = host.switchToHttp();
    const response = ctx.getResponse<Response>();
    const request = ctx.getRequest<Request>();
    
    const traceId = request.headers['x-trace-id'] as string || uuidv4();
    const timestamp = new Date().toISOString();

    let status: number;
    let errorCode: string;
    let message: string;
    let details: any = null;

    if (exception instanceof HttpException) {
      status = exception.getStatus();
      const exceptionResponse = exception.getResponse();
      
      if (typeof exceptionResponse === 'object' && exceptionResponse !== null) {
        const responseObj = exceptionResponse as any;
        message = responseObj.message || exception.message;
        errorCode = this.getErrorCode(status, responseObj.error);
        details = responseObj.details || null;
      } else {
        message = exceptionResponse as string;
        errorCode = this.getErrorCode(status);
      }
    } else if (exception instanceof Error) {
      status = HttpStatus.INTERNAL_SERVER_ERROR;
      message = 'Error interno del servidor';
      errorCode = 'INTERNAL_SERVER_ERROR';
      
      // Log completo del error para debugging
      this.logger.error(
        `Unhandled error: ${exception.message}`,
        exception.stack,
        { traceId, url: request.url, method: request.method }
      );
    } else {
      status = HttpStatus.INTERNAL_SERVER_ERROR;
      message = 'Error desconocido';
      errorCode = 'UNKNOWN_ERROR';
      
      this.logger.error(
        `Unknown exception type: ${typeof exception}`,
        JSON.stringify(exception),
        { traceId }
      );
    }

    // Estructura de respuesta estandarizada
    const errorResponse = {
      success: false,
      error: {
        code: errorCode,
        message,
        details,
        timestamp,
        traceId,
        path: request.url,
        method: request.method
      }
    };

    // Log estructurado del error
    this.logger.error(
      `HTTP ${status} - ${errorCode}: ${message}`,
      {
        traceId,
        status,
        errorCode,
        message,
        details,
        url: request.url,
        method: request.method,
        userAgent: request.headers['user-agent'],
        ip: request.ip,
        timestamp
      }
    );

    response.status(status).json(errorResponse);
  }

  /**
   * Mapear códigos de error HTTP a códigos de negocio
   */
  private getErrorCode(status: number, error?: string): string {
    const errorMappings: Record<number, string> = {
      400: 'BAD_REQUEST',
      401: 'UNAUTHORIZED',
      403: 'FORBIDDEN',
      404: 'NOT_FOUND',
      409: 'CONFLICT',
      422: 'VALIDATION_ERROR',
      429: 'RATE_LIMIT_EXCEEDED',
      500: 'INTERNAL_SERVER_ERROR',
      502: 'BAD_GATEWAY',
      503: 'SERVICE_UNAVAILABLE',
      504: 'GATEWAY_TIMEOUT'
    };

    // Códigos específicos de facturación electrónica
    if (error) {
      const specificMappings: Record<string, string> = {
        'DTE_VALIDATION_FAILED': 'DTE_VALIDATION_FAILED',
        'MH_CONNECTION_ERROR': 'MH_CONNECTION_ERROR',
        'FIRMADOR_ERROR': 'FIRMADOR_ERROR',
        'CERTIFICATE_ERROR': 'CERTIFICATE_ERROR',
        'SEQUENCE_ERROR': 'SEQUENCE_ERROR',
        'DUPLICATE_DTE': 'DUPLICATE_DTE',
        'INVALID_RANGE': 'INVALID_RANGE'
      };

      if (specificMappings[error]) {
        return specificMappings[error];
      }
    }

    return errorMappings[status] || 'UNKNOWN_ERROR';
  }
}