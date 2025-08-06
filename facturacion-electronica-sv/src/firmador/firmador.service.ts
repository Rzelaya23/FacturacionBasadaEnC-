import { Injectable, Logger, BadRequestException } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import axios, { AxiosResponse } from 'axios';

@Injectable()
export class FirmadorService {
  private readonly logger = new Logger(FirmadorService.name);
  private readonly signerUrl: string;

  constructor(private configService: ConfigService) {
    this.signerUrl = this.configService.get('SIGNER_URL', 'http://localhost:8113/firmardocumento/');
  }

  /**
   * Firmar documento DTE
   */
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
  async checkSignerAvailability(): Promise<boolean> {
    try {
      const response = await axios.get(this.signerUrl, { timeout: 5000 });
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