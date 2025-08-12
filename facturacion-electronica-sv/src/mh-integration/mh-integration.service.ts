import { Injectable, Logger, BadRequestException, InternalServerErrorException } from '@nestjs/common';
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

  private readonly env: string;
  private readonly mhUser: string | undefined;
  private readonly mhPassword: string | undefined;
  private readonly mhMock: boolean;

  constructor(private configService: ConfigService) {
    // Leer una sola vez para que los tests que mockean get() no causen recursión
    this.env = this.configService.get('MH_ENVIRONMENT', 'test');
    const suffix = this.env === 'production' ? '_PROD' : '_TEST';
    
    // Usar configService para respetar mocks del test
    this.baseUrl = this.configService.get(`MH_BASE_URL${suffix}`);
    this.authUrl = this.configService.get(`MH_AUTH_URL${suffix}`);
    this.receptionUrl = this.configService.get(`MH_RECEPTION_URL${suffix}`);
    this.cancellationUrl = this.configService.get(`MH_CANCELLATION_URL${suffix}`);
    this.batchUrl = this.configService.get(`MH_BATCH_URL${suffix}`);
    this.batchQueryUrl = this.configService.get(`MH_BATCH_QUERY_URL${suffix}`);
    this.contingencyUrl = this.configService.get(`MH_CONTINGENCY_URL${suffix}`);

    // Cachear credenciales - usar configService para respetar mocks
    this.mhUser = this.configService.get('MH_USER');
    this.mhPassword = this.configService.get('MH_PASSWORD');
    this.mhMock = this.configService.get('MH_MOCK') === 'true';
  }

  /**
   * Autenticar con el Ministerio de Hacienda
   */
  private getSuffix(): '_PROD' | '_TEST' {
    const env = this.configService.get('MH_ENVIRONMENT', 'test');
    return env === 'production' ? '_PROD' : '_TEST';
  }

  private getAuthUrl(): string {
    const suffix = this.getSuffix();
    return this.configService.get(`MH_AUTH_URL${suffix}`);
  }

  private getReceptionUrl(): string {
    const suffix = this.getSuffix();
    return this.configService.get(`MH_RECEPTION_URL${suffix}`);
  }

  private getCancellationUrl(): string {
    const suffix = this.getSuffix();
    return this.configService.get(`MH_CANCELLATION_URL${suffix}`);
  }

  private getBaseUrl(): string {
    // Derivar base URL a partir de la URL de recepción configurada
    const reception = this.getReceptionUrl();
    if (reception) {
      return reception.replace(/\/fesv\/recepciondte\/?$/, '');
    }
    return this.configService.get(`MH_BASE_URL${this.getSuffix()}`) || '';
  }

  async authenticate(): Promise<string> {
    try {
      // Modo mock: devolver token simulado
      if (this.mhMock) {
        return 'Bearer mock_token_123';
      }

      const user = this.mhUser;
      const password = this.mhPassword;

      if (!user || !password) {
        throw new BadRequestException('Credenciales del MH no configuradas');
      }

      // Las pruebas esperan body JSON y header application/json
      const response: AxiosResponse = await axios.post(
        this.getAuthUrl(),
        { user, pwd: password },
        {
          headers: {
            'Content-Type': 'application/json',
          },
        }
      );

      const data = response.data;
      if (data?.status === 'OK' && data?.body?.token) {
        this.logger.log('Autenticación exitosa con el MH');
        return data.body.token;
      }

      if (data?.status === 'ERROR') {
        throw new BadRequestException(data?.descripcionMsg || 'Error en autenticación con el MH');
      }

      throw new InternalServerErrorException('Respuesta inválida de autenticación del MH');
    } catch (error) {
      this.logger.error('Error en autenticación MH:', error.message);
      if (error instanceof BadRequestException || error instanceof InternalServerErrorException) {
        throw error;
      }
      throw new InternalServerErrorException(`Error de autenticación: ${error.message}`);
    }
  }

 // Alias en español para compatibilidad con pruebas y C#
 async autenticar(): Promise<string> {
   return this.authenticate();
 }

 /**
  * Enviar DTE al Ministerio de Hacienda
  */
 async sendDTE(dteData: any): Promise<any> {
    try {
      const token = await this.autenticar();

      const response: AxiosResponse = await axios.post(
        this.getReceptionUrl(),
        dteData,
        {
          headers: {
            'Content-Type': 'application/json',
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

 // Alias en español
 async enviarDTE(documentoFirmado: any): Promise<any> {
   try {
     if (this.mhMock) {
       return {
         selloRecibido: 'SELLO_MOCK_123',
         fhProcesamiento: new Date().toISOString(),
         estado: 'PROCESADO',
         codigoMsg: '001',
         descripcionMsg: 'DTE procesado correctamente (mock)'
       };
     }
     const token = await this.autenticar();
     const ambiente = (this.configService.get('MH_ENVIRONMENT', 'test')) === 'production' ? '01' : '00';
     const idEnvio = Date.now() + Math.floor(Math.random() * 1000);
     const payload = {
       ambiente,
       idEnvio,
       version: 1,
       tipoDte: '01',
       documento: documentoFirmado,
     };

     const response: AxiosResponse = await axios.post(
       this.getReceptionUrl(),
       payload,
       {
         headers: {
           'Content-Type': 'application/json',
           'Authorization': token,
         },
       }
     );

     const data = response.data;
     if (data?.status === 'RECIBIDO' && data?.body) {
       return {
         selloRecibido: data.body.selloRecibido,
         fhProcesamiento: data.body.fhProcesamiento,
         estado: 'PROCESADO',
         codigoMsg: data.body.codigoMsg || '001',
         descripcionMsg: data.body.descripcionMsg || 'DTE procesado correctamente',
       };
     }
     if (data?.status === 'RECHAZADO') {
       throw new BadRequestException(data?.body?.descripcionMsg || 'DTE rechazado por MH');
     }

     throw new InternalServerErrorException('Respuesta inválida del MH al enviar DTE');
   } catch (error) {
     this.logger.error('Error enviando DTE al MH:', error.message);
     if (error instanceof BadRequestException || error instanceof InternalServerErrorException) {
       throw error;
     }
     throw new InternalServerErrorException(`Error enviando DTE: ${error.message}`);
   }
 }

 /**
  * Anular DTE en el Ministerio de Hacienda
  */
 async cancelDTE(cancellationData: any): Promise<any> {
    try {
      if (this.mhMock) {
        return {
          selloRecibido: 'ANULACION_SELLO_MOCK',
          fhProcesamiento: new Date().toISOString(),
          estado: 'PROCESADO',
          descripcionMsg: 'Anulación procesada correctamente (mock)'
        };
      }
      const token = await this.autenticar();

      const response: AxiosResponse = await axios.post(
        this.getCancellationUrl(),
        cancellationData,
        {
          headers: {
            'Content-Type': 'application/json',
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

 // Alias en español
 async anularDTE(datosAnulacion: any): Promise<any> {
   try {
     if (this.mhMock) {
       return {
         selloRecibido: 'ANULACION_SELLO_MOCK',
         fhProcesamiento: new Date().toISOString(),
         estado: 'PROCESADO',
         descripcionMsg: 'Anulación procesada correctamente (mock)'
       };
     }
     const token = await this.autenticar();

     const response: AxiosResponse = await axios.post(
       this.getCancellationUrl(),
       datosAnulacion,
       {
         headers: {
           'Content-Type': 'application/json',
           'Authorization': token,
         },
       }
     );

     const data = response.data;
     
     // Si el estado es PROCESADO, devolver respuesta aplanada
     if (data?.status === 'PROCESADO' || data?.body?.clasificaMsg === 'PROCESADO') {
       return {
         selloRecibido: data.body?.selloRecibido || data.selloRecibido,
         fhProcesamiento: data.body?.fhProcesamiento || data.fhProcesamiento,
         estado: 'PROCESADO',
         descripcionMsg: data.body?.descripcionMsg || data.descripcionMsg || 'Anulación procesada correctamente'
       };
     }
     
     // Si el estado es RECHAZADO, lanzar BadRequestException
     if (data?.status === 'RECHAZADO' || data?.body?.clasificaMsg === 'RECHAZADO') {
       throw new BadRequestException(data.body?.descripcionMsg || data.descripcionMsg || 'DTE no puede ser anulado');
     }

     // Respuesta inválida
     throw new InternalServerErrorException('Respuesta inválida del MH al anular DTE');
   } catch (error) {
     this.logger.error('Error anulando DTE en el MH:', error.message);
     if (error instanceof BadRequestException || error instanceof InternalServerErrorException) {
       throw error;
     }
     throw new InternalServerErrorException(`Error anulando DTE: ${error.message}`);
   }
 }

 /**
  * Enviar lote de DTEs al Ministerio de Hacienda
  */
 async sendBatch(batchData: any): Promise<any> {
    try {
      const token = await this.autenticar();

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

 // Alias en español
 async consultarDTE(codigoGeneracion: string): Promise<any> {
   try {
     if (this.mhMock) {
       if (/^[A-F0-9-]{36,}$/.test(codigoGeneracion)) {
         return {
           codigoGeneracion,
           selloRecibido: 'SELLO_MOCK_123',
           estado: 'PROCESADO',
           fhProcesamiento: new Date().toISOString(),
         };
       }
       throw new BadRequestException('DTE no encontrado o respuesta inválida');
     }
     const token = await this.autenticar();
     const response: AxiosResponse = await axios.get(
       `${this.getBaseUrl()}/fesv/consultadte/${codigoGeneracion}`,
       {
         headers: {
           'User-Agent': 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36',
           'Authorization': token,
         },
       }
     );

     const data = response.data;
     if (data?.status === 'OK' && data?.body) {
       return {
         codigoGeneracion: data.body.codigoGeneracion,
         selloRecibido: data.body.selloRecibido,
         estado: data.body.estado,
         fhProcesamiento: data.body.fhProcesamiento,
       };
     }

     throw new BadRequestException('DTE no encontrado o respuesta inválida');
   } catch (error) {
     this.logger.error('Error consultando DTE en el MH:', error.message);
     throw new BadRequestException(`Error consultando DTE: ${error.message}`);
   }
 }

 /**
  * Consultar estado de lote en el Ministerio de Hacienda
  */
 async queryBatch(batchCode: string): Promise<any> {
    try {
      const token = await this.autenticar();

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
      const token = await this.autenticar();

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
      await this.autenticar();
      return true;
    } catch (error) {
      this.logger.warn('No hay conectividad con el MH:', error.message);
      return false;
    }
  }
}