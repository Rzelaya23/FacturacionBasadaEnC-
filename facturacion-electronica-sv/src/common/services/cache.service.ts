import { Injectable, Logger } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';

/**
 * Servicio de Cache Inteligente
 * Optimiza performance reduciendo llamadas al MH y BD
 */
@Injectable()
export class CacheService {
  private readonly logger = new Logger(CacheService.name);
  private cache = new Map<string, { data: any; expiry: number }>();
  private readonly defaultTtl: number;

  constructor(private configService: ConfigService) {
    this.defaultTtl = this.configService.get<number>('CACHE_TTL', 300000); // 5 minutos default
  }

  /**
   * Obtener valor del cache
   */
  async get<T>(key: string): Promise<T | null> {
    const cached = this.cache.get(key);
    
    if (!cached) {
      this.logger.debug(`Cache MISS: ${key}`);
      return null;
    }

    if (Date.now() > cached.expiry) {
      this.cache.delete(key);
      this.logger.debug(`Cache EXPIRED: ${key}`);
      return null;
    }

    this.logger.debug(`Cache HIT: ${key}`);
    return cached.data as T;
  }

  /**
   * Establecer valor en cache
   */
  async set(key: string, value: any, ttl?: number): Promise<void> {
    const expiry = Date.now() + (ttl || this.defaultTtl);
    this.cache.set(key, { data: value, expiry });
    this.logger.debug(`Cache SET: ${key} (TTL: ${ttl || this.defaultTtl}ms)`);
  }

  /**
   * Eliminar valor del cache
   */
  async del(key: string): Promise<void> {
    this.cache.delete(key);
    this.logger.debug(`Cache DEL: ${key}`);
  }

  /**
   * Limpiar todo el cache
   */
  async clear(): Promise<void> {
    this.cache.clear();
    this.logger.log('Cache cleared completely');
  }

  /**
   * Obtener estadísticas del cache
   */
  getStats(): { size: number; keys: string[] } {
    return {
      size: this.cache.size,
      keys: Array.from(this.cache.keys())
    };
  }

  /**
   * Cache con función de fallback
   */
  async getOrSet<T>(
    key: string, 
    fallbackFn: () => Promise<T>, 
    ttl?: number
  ): Promise<T> {
    const cached = await this.get<T>(key);
    
    if (cached !== null) {
      return cached;
    }

    try {
      const value = await fallbackFn();
      await this.set(key, value, ttl);
      return value;
    } catch (error) {
      this.logger.error(`Error in fallback function for key ${key}:`, error);
      throw error;
    }
  }

  /**
   * Cache específico para catálogos del MH
   */
  async getCatalogo<T>(tipo: string): Promise<T | null> {
    return this.get<T>(`catalogo:${tipo}`);
  }

  async setCatalogo(tipo: string, data: any): Promise<void> {
    // Catálogos del MH se cachean por 1 hora (raramente cambian)
    await this.set(`catalogo:${tipo}`, data, 3600000);
  }

  /**
   * Cache específico para tokens de autenticación
   */
  async getToken(service: string): Promise<string | null> {
    return this.get<string>(`token:${service}`);
  }

  async setToken(service: string, token: string, expiresIn: number): Promise<void> {
    // Tokens se cachean hasta 5 minutos antes de expirar
    const ttl = Math.max(expiresIn - 300000, 60000);
    await this.set(`token:${service}`, token, ttl);
  }

  /**
   * Cache específico para validaciones
   */
  async getValidation(hash: string): Promise<any | null> {
    return this.get(`validation:${hash}`);
  }

  async setValidation(hash: string, result: any): Promise<void> {
    // Validaciones se cachean por 10 minutos
    await this.set(`validation:${hash}`, result, 600000);
  }

  /**
   * Limpiar cache expirado (mantenimiento)
   */
  cleanExpired(): void {
    const now = Date.now();
    let cleaned = 0;

    for (const [key, value] of this.cache.entries()) {
      if (now > value.expiry) {
        this.cache.delete(key);
        cleaned++;
      }
    }

    if (cleaned > 0) {
      this.logger.log(`Cleaned ${cleaned} expired cache entries`);
    }
  }
}