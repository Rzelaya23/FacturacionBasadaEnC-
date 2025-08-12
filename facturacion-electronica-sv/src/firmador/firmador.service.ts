import { Injectable, Logger, BadRequestException, InternalServerErrorException } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import axios, { AxiosResponse } from 'axios';

@Injectable()
export class FirmadorService {
  private readonly logger = new Logger(FirmadorService.name);
  private readonly signerUrl: string;

  constructor(private configService: ConfigService) {
    this.signerUrl = this.configService.get('SIGNER_URL') || 'http://localhost:8113/firmardocumento/';
  }

  private isMock(): boolean {
    return this.configService.get('SIGNER_MOCK') === 'true';
  }

  // Alias y métodos esperados por pruebas/C#
  async firmarDocumento(documentData: any): Promise<any> {
    try {
      // MOCK: si está habilitado, devolver firmado simulado
      if (this.isMock()) {
        return {
          status: 'OK',
          body: 'signed_document_base64_mock',
          descripcion: 'Documento firmado (mock)'
        };
      }

      // Validación mínima de estructura (compatibilidad con C# y tests):
      // Requerimos al menos emisor, cuerpoDocumento y resumen.
      if (!documentData?.emisor || !documentData?.cuerpoDocumento || !documentData?.resumen) {
        throw new BadRequestException('Documento inválido para firmar');
      }

      const response: AxiosResponse = await axios.post(
        this.signerUrl,
        documentData,
        {
          headers: { 'Content-Type': 'application/json' },
          timeout: 30000,
        }
      );

      const data = response.data;
      if (typeof data !== 'object') {
        throw new InternalServerErrorException('Respuesta inválida del servicio de firmado');
      }
      if (data?.status === 'OK' && data?.body) {
        return {
          status: data.status,
          body: data.body,
          descripcion: data.descripcion || 'Documento firmado correctamente',
        };
      }

      // Manejo de errores conocido
      if (data?.status === 'ERROR') {
        throw new InternalServerErrorException(data.descripcion || 'Error en el servicio de firmado');
      }

      // Respuesta inválida/inesperada del firmador
      throw new BadRequestException('Respuesta del firmador incompleta');
    } catch (error) {
      this.logger.error('Error en firmarDocumento:', error.message);

      if (error.message?.includes('ECONNREFUSED')) {
        throw new BadRequestException('Servicio de firmado no disponible');
      }
      if (error.message?.includes('timeout')) {
        throw new InternalServerErrorException('Timeout en el servicio de firmado');
      }

      if (error instanceof BadRequestException || error instanceof InternalServerErrorException) {
        throw error;
      }

      throw new InternalServerErrorException(`Error en firmado digital: ${error.message}`);
    }
  }

  async verificarDisponibilidad(): Promise<boolean> {
    try {
      if (this.isMock()) return true;
      const healthUrl = this.configService.get('SIGNER_HEALTH_URL') || this.signerUrl.replace(/firmardocumento\/?$/, 'health');
      const response = await axios.get(healthUrl, { timeout: 5000 });
      return (response.data?.status === 'OK' || response.data === 'OK') || response.status === 200;
    } catch (error) {
      this.logger.warn('Servicio de firmado no disponible:', error.message);
      return false;
    }
  }

  async obtenerInformacionCertificado(): Promise<any> {
    try {
      if (this.isMock()) {
        return {
          emisor: 'AUTORIDAD CERTIFICADORA (mock)',
          titular: 'EMPRESA DE PRUEBA S.A. DE C.V.',
          nit: '06142803901121',
          validoDesde: new Date(Date.now() - 86400000).toISOString(),
          validoHasta: new Date(Date.now() + 86400000 * 365).toISOString(),
          estado: 'VIGENTE'
        };
      }
      const certInfoUrl = this.configService.get('SIGNER_CERT_INFO_URL') || this.signerUrl.replace(/firmardocumento\/?$/, 'certinfo');
      const response = await axios.get(certInfoUrl, { timeout: 10000 });
      const raw = response.data;
      if (raw?.status === 'ERROR') {
        throw new BadRequestException('Certificado no encontrado');
      }
      const data = raw?.body || raw;
      if (!data) {
        throw new BadRequestException('Certificado no encontrado');
      }
      return data;
    } catch (error) {
      if (error.message?.includes('no encontrado')) {
        throw new BadRequestException('Certificado no encontrado');
      }
      throw new InternalServerErrorException(`Error obteniendo información de certificado: ${error.message}`);
    }
  }

  /**
   * Firmar documento DTE
   */
  // Mantener método original en inglés
  async signDocument(documentData: any): Promise<any> {
    try {
      this.logger.log('Iniciando proceso de firmado digital');

      const response: AxiosResponse = await axios.post(
        this.signerUrl,
        documentData,
        {
          headers: {
            'Content-Type': 'application/json',
          },
          timeout: 30000, // 30 segundos timeout
        }
      );

      if (response.data) {
        this.logger.log('Documento firmado exitosamente');
        return response.data;
      }

      throw new BadRequestException('Respuesta vacía del servicio de firmado');
    } catch (error) {
      this.logger.error('Error en el servicio de firmado:', error.message);
      
      if (error.code === 'ECONNREFUSED') {
        throw new BadRequestException('Servicio de firmado no disponible. Verifique que esté ejecutándose en el puerto 8113');
      }
      
      if (error.code === 'ETIMEDOUT') {
        throw new BadRequestException('Timeout en el servicio de firmado');
      }

      throw new BadRequestException(`Error en firmado digital: ${error.message}`);
    }
  }

  /**
   * Verificar disponibilidad del servicio de firmado
   */
  // Mantener método original para compatibilidad
  async checkSignerAvailability(): Promise<boolean> {
    try {
      const response = await axios.get(this.signerUrl, { timeout: 5000 }); // legacy behavior: GET signerUrl base
      return response.status === 200;
    } catch (error) {
      this.logger.warn('Servicio de firmado no disponible:', error.message);
      return false;
    }
  }

  /**
   * Preparar documento para firmado según tipo de DTE
   */
  prepareDocumentForSigning(dteType: string, documentData: any): any {
    const signingDocument = {
      nit: documentData.emisor?.nit,
      activo: true,
      passwordPri: this.configService.get('MH_PASSWORD_PRIVATE'),
      dteJson: documentData,
    };

    this.logger.log(`Documento preparado para firmado - Tipo: ${dteType}`);
    return signingDocument;
  }

  /**
   * Firmar Factura Electrónica (FE)
   */
  async signFE(feData: any): Promise<any> {
    const signingDoc = this.prepareDocumentForSigning('FE', feData);
    return this.signDocument(signingDoc);
  }

  /**
   * Firmar Comprobante de Crédito Fiscal (CCF)
   */
  async signCCF(ccfData: any): Promise<any> {
    const signingDoc = this.prepareDocumentForSigning('CCF', ccfData);
    return this.signDocument(signingDoc);
  }

  /**
   * Firmar Nota de Crédito (NC)
   */
  async signNC(ncData: any): Promise<any> {
    const signingDoc = this.prepareDocumentForSigning('NC', ncData);
    return this.signDocument(signingDoc);
  }

  /**
   * Firmar Nota de Débito (ND)
   */
  async signND(ndData: any): Promise<any> {
    const signingDoc = this.prepareDocumentForSigning('ND', ndData);
    return this.signDocument(signingDoc);
  }

  /**
   * Firmar Nota de Remisión (NR)
   */
  async signNR(nrData: any): Promise<any> {
    const signingDoc = this.prepareDocumentForSigning('NR', nrData);
    return this.signDocument(signingDoc);
  }

  /**
   * Firmar Factura Sujeto Excluido (FSE)
   */
  async signFSE(fseData: any): Promise<any> {
    const signingDoc = this.prepareDocumentForSigning('FSE', fseData);
    return this.signDocument(signingDoc);
  }

  /**
   * Firmar Factura de Exportación (FEX)
   */
  async signFEX(fexData: any): Promise<any> {
    const signingDoc = this.prepareDocumentForSigning('FEX', fexData);
    return this.signDocument(signingDoc);
  }

  /**
   * Firmar documento de anulación
   */
  async signCancellation(cancellationData: any): Promise<any> {
    const signingDoc = this.prepareDocumentForSigning('ANULACION', cancellationData);
    return this.signDocument(signingDoc);
  }

  /**
   * Firmar documento de contingencia
   */
  async signContingency(contingencyData: any): Promise<any> {
    const signingDoc = this.prepareDocumentForSigning('CONTINGENCIA', contingencyData);
    return this.signDocument(signingDoc);
  }
}