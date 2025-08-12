import { Injectable, Logger, BadRequestException, InternalServerErrorException } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import axios, { AxiosInstance, AxiosResponse } from 'axios';
import { CacheService } from '../common/services/cache.service';
import { PerformanceService } from '../common/services/performance.service';
import { LoggerService } from '../common/services/logger.service';

/**
 * Servicio Optimizado de Integración con MH
 * Incluye cache, retry logic, circuit breaker y métricas
 */
@Injectable()
export class MhIntegrationOptimizedService {
  private readonly logger = new Logger(MhIntegrationOptimizedService.name);
  private readonly httpClient: AxiosInstance;
  private readonly baseUrl: string;

  constructor(
    private configService: ConfigService,
    private cacheService: CacheService,
    private performanceService: PerformanceService,
    private loggerService: LoggerService,
  ) {
    this.baseUrl = this.configService.get<string>('MH_BASE_URL');
    
    this.httpClient = axios.create({
      baseURL: this.baseUrl,
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json',
        'User-Agent': 'FacturacionElectronica-NestJS/1.0'
      }
    });

    this.setupInterceptors();
  }

  /**
   * Configurar interceptors para logging y manejo de errores
   */
  private setupInterceptors(): void {
    // Request interceptor
    this.httpClient.interceptors.request.use(
      (config: any) => {
        config.metadata = { startTime: Date.now() };
        this.logger.debug(`→ ${config.method?.toUpperCase()} ${config.url}`);
        return config;
      },
      (error) => {
        this.logger.error('Request interceptor error:', error);
        return Promise.reject(error);
      }
    );

    // Response interceptor
    this.httpClient.interceptors.response.use(
      (response: any) => {
        const duration = Date.now() - response.config.metadata?.startTime;
        this.logger.debug(`← ${response.status} ${response.config.url} - ${duration}ms`);
        return response;
      },
      (error: any) => {
        const duration = Date.now() - error.config?.metadata?.startTime;
        const status = error.response?.status || 'NETWORK_ERROR';
        this.logger.error(`← ${status} ${error.config?.url} - ${duration}ms - ERROR: ${error.message}`);
        return Promise.reject(error);
      }
    );
  }

  /**
   * Autenticar con el Ministerio de Hacienda (con cache y retry)
   */
  async authenticate(): Promise<string> {
    // Verificar token en cache
    const cachedToken = await this.cacheService.getToken('mh');
    if (cachedToken) {
      this.loggerService.logMH('authenticate', '/seguridad/auth', 'success', 0, { source: 'cache' });
      return cachedToken;
    }

    return this.performanceService.executeWithCircuitBreaker(
      async () => {
        return this.performanceService.executeWithRetry(
          async () => {
            const startTime = Date.now();
            
            const authData = {
              user: this.configService.get<string>('MH_USER'),
              pwd: this.configService.get<string>('MH_PASSWORD')
            };

            const response = await this.httpClient.post('/seguridad/auth', authData);
            const duration = Date.now() - startTime;
            
            if (response.data?.body?.token) {
              const token = response.data.body.token;
              const expiresIn = response.data.body.expires_in || 3600; // segundos
              
              // Cachear token (5 minutos antes de expirar)
              await this.cacheService.setToken('mh', token, (expiresIn - 300) * 1000);
              
              this.loggerService.logMH('authenticate', '/seguridad/auth', 'success', duration, { 
                expiresIn,
                cached: true 
              });
              
              return token;
            }

            throw new BadRequestException('Respuesta de autenticación inválida del MH');
          },
          'mh_authenticate',
          3,
          2000
        );
      },
      'mh_authenticate',
      5,
      60000
    ).catch(error => {
      this.loggerService.logMH('authenticate', '/seguridad/auth', 'error', 0, { 
        error: error.message 
      });
      
      if (error.response?.status === 401) {
        throw new BadRequestException('Credenciales inválidas para MH');
      }
      
      throw new InternalServerErrorException(`Error de autenticación MH: ${error.message}`);
    });
  }

  /**
   * Enviar DTE al MH (optimizado)
   */
  async sendDTE(documentoFirmado: any): Promise<any> {
    return this.performanceService.executeWithTimeout(
      async () => {
        const token = await this.authenticate();
        const startTime = Date.now();

        const headers = {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        };

        const response = await this.httpClient.post('/fesv/recepciondte', documentoFirmado, { headers });
        const duration = Date.now() - startTime;

        this.loggerService.logMH('sendDTE', '/fesv/recepciondte', 'success', duration, {
          codigoGeneracion: documentoFirmado.identificacion?.codigoGeneracion,
          tipoDte: documentoFirmado.identificacion?.tipoDte
        });

        return this.processMHResponse(response.data);
      },
      45000, // 45 segundos timeout
      'mh_send_dte'
    ).catch(error => {
      this.loggerService.logMH('sendDTE', '/fesv/recepciondte', 'error', 0, {
        error: error.message,
        codigoGeneracion: documentoFirmado.identificacion?.codigoGeneracion
      });

      if (error.response?.status === 401) {
        // Token expirado, limpiar cache
        this.cacheService.del('token:mh');
        throw new BadRequestException('Token expirado, reintente la operación');
      }

      throw new InternalServerErrorException(`Error enviando DTE al MH: ${error.message}`);
    });
  }

  /**
   * Consultar estado de DTE (con cache)
   */
  async consultarDTE(codigoGeneracion: string): Promise<any> {
    const cacheKey = `dte_status:${codigoGeneracion}`;
    
    return this.cacheService.getOrSet(
      cacheKey,
      async () => {
        const token = await this.authenticate();
        const startTime = Date.now();

        const headers = {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        };

        const response = await this.httpClient.get(`/fesv/consultadte/${codigoGeneracion}`, { headers });
        const duration = Date.now() - startTime;

        this.loggerService.logMH('consultarDTE', '/fesv/consultadte', 'success', duration, {
          codigoGeneracion
        });

        return response.data;
      },
      300000 // Cache por 5 minutos
    ).catch(error => {
      this.loggerService.logMH('consultarDTE', '/fesv/consultadte', 'error', 0, {
        error: error.message,
        codigoGeneracion
      });

      throw new InternalServerErrorException(`Error consultando DTE: ${error.message}`);
    });
  }

  /**
   * Anular DTE (optimizado)
   */
  async anularDTE(datosAnulacion: any): Promise<any> {
    return this.performanceService.executeWithRetry(
      async () => {
        const token = await this.authenticate();
        const startTime = Date.now();

        const headers = {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        };

        const response = await this.httpClient.post('/fesv/anulardte', datosAnulacion, { headers });
        const duration = Date.now() - startTime;

        this.loggerService.logMH('anularDTE', '/fesv/anulardte', 'success', duration, {
          codigoGeneracion: datosAnulacion.identificacion?.codigoGeneracion
        });

        return response.data;
      },
      'mh_anular_dte',
      3,
      3000
    ).catch(error => {
      this.loggerService.logMH('anularDTE', '/fesv/anulardte', 'error', 0, {
        error: error.message,
        codigoGeneracion: datosAnulacion.identificacion?.codigoGeneracion
      });

      throw new InternalServerErrorException(`Error anulando DTE: ${error.message}`);
    });
  }

  /**
   * Enviar lote de DTEs (procesamiento en paralelo optimizado)
   */
  async enviarLote(lote: any[]): Promise<any[]> {
    return this.performanceService.executeInBatches(
      lote,
      async (dte) => {
        return this.sendDTE(dte);
      },
      3, // Procesar de 3 en 3 para no sobrecargar el MH
      'mh_enviar_lote'
    );
  }

  /**
   * Obtener catálogos del MH (con cache de larga duración)
   */
  async getCatalogos(tipo: string): Promise<any> {
    return this.cacheService.getOrSet(
      `catalogo:${tipo}`,
      async () => {
        const token = await this.authenticate();
        const startTime = Date.now();

        const headers = {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        };

        const response = await this.httpClient.get(`/fesv/catalogos/${tipo}`, { headers });
        const duration = Date.now() - startTime;

        this.loggerService.logMH('getCatalogos', `/fesv/catalogos/${tipo}`, 'success', duration, {
          tipo,
          itemCount: response.data?.length || 0
        });

        return response.data;
      },
      3600000 // Cache por 1 hora (catálogos cambian raramente)
    );
  }

  /**
   * Verificar conectividad con el MH
   */
  async checkConnectivity(): Promise<boolean> {
    try {
      const response = await this.performanceService.executeWithTimeout(
        () => this.httpClient.get('/health'),
        5000,
        'mh_connectivity_check'
      );
      
      return response.status < 500;
    } catch (error) {
      this.logger.warn(`MH connectivity check failed: ${error.message}`);
      return false;
    }
  }

  /**
   * Obtener métricas de integración con MH
   */
  getIntegrationMetrics(): any {
    const performanceStats = this.performanceService.getPerformanceStats();
    const cacheStats = this.cacheService.getStats();

    return {
      performance: {
        authenticate: performanceStats['mh_authenticate'] || null,
        sendDTE: performanceStats['mh_send_dte'] || null,
        consultarDTE: performanceStats['mh_consultar_dte'] || null,
        anularDTE: performanceStats['mh_anular_dte'] || null
      },
      cache: {
        totalKeys: cacheStats.size,
        mhKeys: cacheStats.keys.filter(key => key.startsWith('token:mh') || key.startsWith('catalogo:')).length
      },
      connectivity: {
        baseUrl: this.baseUrl,
        lastCheck: new Date().toISOString()
      }
    };
  }

  /**
   * Procesar respuesta del MH
   */
  private processMHResponse(response: any): any {
    if (!response) {
      throw new BadRequestException('Respuesta vacía del MH');
    }

    // Normalizar respuesta según estructura del MH
    return {
      estado: response.estado || response.status || 'PROCESADO',
      selloRecibido: response.selloRecibido || response.timestamp || new Date().toISOString(),
      clasificaMsg: response.clasificaMsg || response.classification || '01',
      codigoMsg: response.codigoMsg || response.code || '001',
      descripcionMsg: response.descripcionMsg || response.message || 'DTE procesado correctamente',
      observaciones: response.observaciones || response.observations || null,
      codigoGeneracion: response.codigoGeneracion || response.generationCode || null
    };
  }

  /**
   * Limpiar cache relacionado con MH
   */
  async clearMHCache(): Promise<void> {
    const cacheStats = this.cacheService.getStats();
    const mhKeys = cacheStats.keys.filter(key => 
      key.startsWith('token:mh') || 
      key.startsWith('catalogo:') || 
      key.startsWith('dte_status:')
    );

    for (const key of mhKeys) {
      await this.cacheService.del(key);
    }

    this.logger.log(`Cleared ${mhKeys.length} MH cache entries`);
  }
}