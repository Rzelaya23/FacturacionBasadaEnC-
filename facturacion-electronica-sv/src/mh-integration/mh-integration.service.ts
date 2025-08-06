import { Injectable, Logger, BadRequestException } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import axios, { AxiosResponse } from 'axios';

@Injectable()
export class MhIntegrationService {
  private readonly logger = new Logger(MhIntegrationService.name);
  private readonly baseUrl: string;
  private readonly authUrl: string;
  private readonly receptionUrl: string;
  private readonly cancellationUrl: string;
  private readonly batchUrl: string;
  private readonly batchQueryUrl: string;
  private readonly contingencyUrl: string;

  constructor(private configService: ConfigService) {
    const environment = this.configService.get('MH_ENVIRONMENT', 'test');
    const suffix = environment === 'production' ? '_PROD' : '_TEST';
    
    this.baseUrl = this.configService.get(`MH_BASE_URL${suffix}`);
    this.authUrl = this.configService.get(`MH_AUTH_URL${suffix}`);
    this.receptionUrl = this.configService.get(`MH_RECEPTION_URL${suffix}`);
    this.cancellationUrl = this.configService.get(`MH_CANCELLATION_URL${suffix}`);
    this.batchUrl = this.configService.get(`MH_BATCH_URL${suffix}`);
    this.batchQueryUrl = this.configService.get(`MH_BATCH_QUERY_URL${suffix}`);
    this.contingencyUrl = this.configService.get(`MH_CONTINGENCY_URL${suffix}`);
  }

  /**
   * Autenticar con el Ministerio de Hacienda
   */
  async authenticate(): Promise<string> {
    try {
      const user = this.configService.get('MH_USER');
      const password = this.configService.get('MH_PASSWORD');

      if (!user || !password) {
        throw new BadRequestException('Credenciales del MH no configuradas');
      }

      const response: AxiosResponse = await axios.post(
        this.authUrl,
        `user=${user}&pwd=${password}`,
        {
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'User-Agent': 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36',
          },
        }
      );

      if (response.data?.status === 'OK' && response.data?.body?.token) {
        this.logger.log('Autenticación exitosa con el MH');
        return response.data.body.token;
      }

      throw new BadRequestException('Error en autenticación con el MH');
    } catch (error) {
      this.logger.error('Error en autenticación MH:', error.message);
      throw new BadRequestException(`Error de autenticación: ${error.message}`);
    }
  }

  /**
   * Enviar DTE al Ministerio de Hacienda
   */
  async sendDTE(dteData: any): Promise<any> {
    try {
      const token = await this.authenticate();

      const response: AxiosResponse = await axios.post(
        this.receptionUrl,
        dteData,
        {
          headers: {
            'Content-Type': 'application/json',
            'User-Agent': 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36',
            'Authorization': token,
          },
        }
      );

      this.logger.log('DTE enviado exitosamente al MH');
      return response.data;
    } catch (error) {
      this.logger.error('Error enviando DTE al MH:', error.message);
      throw new BadRequestException(`Error enviando DTE: ${error.message}`);
    }
  }

  /**
   * Anular DTE en el Ministerio de Hacienda
   */
  async cancelDTE(cancellationData: any): Promise<any> {
    try {
      const token = await this.authenticate();

      const response: AxiosResponse = await axios.post(
        this.cancellationUrl,
        cancellationData,
        {
          headers: {
            'Content-Type': 'application/json',
            'User-Agent': 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36',
            'Authorization': token,
          },
        }
      );

      this.logger.log('DTE anulado exitosamente en el MH');
      return response.data;
    } catch (error) {
      this.logger.error('Error anulando DTE en el MH:', error.message);
      throw new BadRequestException(`Error anulando DTE: ${error.message}`);
    }
  }

  /**
   * Enviar lote de DTEs al Ministerio de Hacienda
   */
  async sendBatch(batchData: any): Promise<any> {
    try {
      const token = await this.authenticate();

      const response: AxiosResponse = await axios.post(
        this.batchUrl,
        batchData,
        {
          headers: {
            'Content-Type': 'application/json',
            'User-Agent': 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36',
            'Authorization': token,
          },
        }
      );

      this.logger.log('Lote enviado exitosamente al MH');
      return response.data;
    } catch (error) {
      this.logger.error('Error enviando lote al MH:', error.message);
      throw new BadRequestException(`Error enviando lote: ${error.message}`);
    }
  }

  /**
   * Consultar estado de lote en el Ministerio de Hacienda
   */
  async queryBatch(batchCode: string): Promise<any> {
    try {
      const token = await this.authenticate();

      const response: AxiosResponse = await axios.get(
        `${this.batchQueryUrl}${batchCode}`,
        {
          headers: {
            'User-Agent': 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36',
            'Authorization': token,
          },
        }
      );

      this.logger.log(`Consulta de lote ${batchCode} exitosa`);
      return response.data;
    } catch (error) {
      this.logger.error('Error consultando lote en el MH:', error.message);
      throw new BadRequestException(`Error consultando lote: ${error.message}`);
    }
  }

  /**
   * Enviar contingencia al Ministerio de Hacienda
   */
  async sendContingency(contingencyData: any): Promise<any> {
    try {
      const token = await this.authenticate();

      const response: AxiosResponse = await axios.post(
        this.contingencyUrl,
        contingencyData,
        {
          headers: {
            'Content-Type': 'application/json',
            'User-Agent': 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36',
            'Authorization': token,
          },
        }
      );

      this.logger.log('Contingencia enviada exitosamente al MH');
      return response.data;
    } catch (error) {
      this.logger.error('Error enviando contingencia al MH:', error.message);
      throw new BadRequestException(`Error enviando contingencia: ${error.message}`);
    }
  }

  /**
   * Verificar conectividad con el Ministerio de Hacienda
   */
  async checkConnectivity(): Promise<boolean> {
    try {
      await this.authenticate();
      return true;
    } catch (error) {
      this.logger.warn('No hay conectividad con el MH:', error.message);
      return false;
    }
  }
}