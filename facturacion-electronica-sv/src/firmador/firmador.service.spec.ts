import { Test, TestingModule } from '@nestjs/testing';
import { ConfigService } from '@nestjs/config';
import { InternalServerErrorException } from '@nestjs/common';
import { FirmadorService } from './firmador.service';
import axios from 'axios';

// Mock axios
jest.mock('axios');
const mockedAxios = axios as jest.Mocked<typeof axios>;

describe('FirmadorService', () => {
  let service: FirmadorService;
  let configService: ConfigService;

  const mockConfigService = {
    get: jest.fn((key: string) => {
      const config = {
        'SIGNER_URL': 'http://localhost:8113/firmardocumento/',
        'NODE_ENV': 'development'
      };
      return config[key];
    }),
  };

  beforeEach(async () => {
    const module: TestingModule = await Test.createTestingModule({
      providers: [
        FirmadorService,
        {
          provide: ConfigService,
          useValue: mockConfigService,
        },
      ],
    }).compile();

    service = module.get<FirmadorService>(FirmadorService);
    configService = module.get<ConfigService>(ConfigService);

    jest.clearAllMocks();
  });

  it('should be defined', () => {
    expect(service).toBeDefined();
  });

  describe('firmarDocumento', () => {
    const mockDocumentoJson = {
      identificacion: {
        version: 1,
        ambiente: '00',
        tipoDte: '01',
        numeroControl: 'DTE-01-0001-0001-000000000000001',
        codigoGeneracion: 'A1B2C3D4-E5F6-7890-ABCD-123456789012',
        tipoModelo: 1,
        tipoOperacion: 1,
        fecEmi: '2024-08-04',
        horEmi: '10:30:00',
        tipoMoneda: 'USD'
      },
      emisor: {
        nit: '06142803901121',
        nrc: '1234567',
        nombre: 'EMPRESA DE PRUEBA S.A. DE C.V.'
      },
      receptor: {
        tipoDocumento: '36',
        numDocumento: '12345678901234',
        nombre: 'CLIENTE DE PRUEBA'
      },
      cuerpoDocumento: [
        {
          numItem: 1,
          tipoItem: 2,
          cantidad: 1,
          codigo: 'PROD001',
          descripcion: 'PRODUCTO DE PRUEBA',
          precioUni: 10.00,
          ventaGravada: 10.00
        }
      ],
      resumen: {
        totalGravada: 10.00,
        totalPagar: 11.30,
        totalIva: 1.30
      }
    };

    it('should sign document successfully', async () => {
      const mockSignResponse = {
        data: {
          status: 'OK',
          body: 'eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZGVudGlmaWNhY2lvbiI6eyJ2ZXJzaW9uIjoxLCJhbWJpZW50ZSI6IjAwIiwidGlwb0R0ZSI6IjAxIn19.signature_base64',
          descripcion: 'Documento firmado correctamente'
        }
      };

      mockedAxios.post.mockResolvedValue(mockSignResponse);

      const result = await service.firmarDocumento(mockDocumentoJson);

      expect(result).toEqual({
        body: 'eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZGVudGlmaWNhY2lvbiI6eyJ2ZXJzaW9uIjoxLCJhbWJpZW50ZSI6IjAwIiwidGlwb0R0ZSI6IjAxIn19.signature_base64',
        status: 'OK',
        descripcion: 'Documento firmado correctamente'
      });

      expect(mockedAxios.post).toHaveBeenCalledWith(
        'http://localhost:8113/firmardocumento/',
        mockDocumentoJson,
        {
          headers: {
            'Content-Type': 'application/json'
          },
          timeout: 30000
        }
      );
    });

    it('should handle signing service errors', async () => {
      const mockErrorResponse = {
        data: {
          status: 'ERROR',
          descripcion: 'Error en el certificado de firma'
        }
      };

      mockedAxios.post.mockResolvedValue(mockErrorResponse);

      await expect(service.firmarDocumento(mockDocumentoJson))
        .rejects.toThrow(InternalServerErrorException);
    });

    it('should handle network timeout', async () => {
      mockedAxios.post.mockRejectedValue(new Error('timeout of 30000ms exceeded'));

      await expect(service.firmarDocumento(mockDocumentoJson))
        .rejects.toThrow(InternalServerErrorException);
    });

    it('should handle connection refused', async () => {
      mockedAxios.post.mockRejectedValue(new Error('connect ECONNREFUSED 127.0.0.1:8113'));

      await expect(service.firmarDocumento(mockDocumentoJson))
        .rejects.toThrow('Servicio de firmado no disponible');
    });

    it('should handle invalid JSON response', async () => {
      const mockInvalidResponse = {
        data: 'invalid json response'
      };

      mockedAxios.post.mockResolvedValue(mockInvalidResponse);

      await expect(service.firmarDocumento(mockDocumentoJson))
        .rejects.toThrow(InternalServerErrorException);
    });

    it('should handle missing body in response', async () => {
      const mockIncompleteResponse = {
        data: {
          status: 'OK',
          descripcion: 'Firmado correctamente'
          // Sin 'body'
        }
      };

      mockedAxios.post.mockResolvedValue(mockIncompleteResponse);

      await expect(service.firmarDocumento(mockDocumentoJson))
        .rejects.toThrow('Respuesta del firmador incompleta');
    });

    it('should validate document structure before signing', async () => {
      const invalidDocument = {
        // Documento sin campos requeridos
        identificacion: {}
      };

      await expect(service.firmarDocumento(invalidDocument))
        .rejects.toThrow('Documento inválido para firmar');
    });

    it('should handle large documents', async () => {
      const largeDocument = {
        ...mockDocumentoJson,
        cuerpoDocumento: Array.from({ length: 1000 }, (_, i) => ({
          numItem: i + 1,
          tipoItem: 2,
          cantidad: 1,
          codigo: `PROD${i.toString().padStart(3, '0')}`,
          descripcion: `PRODUCTO DE PRUEBA ${i + 1}`,
          precioUni: 10.00,
          ventaGravada: 10.00
        }))
      };

      const mockSignResponse = {
        data: {
          status: 'OK',
          body: 'large_document_signed_base64',
          descripcion: 'Documento grande firmado correctamente'
        }
      };

      mockedAxios.post.mockResolvedValue(mockSignResponse);

      const result = await service.firmarDocumento(largeDocument);

      expect(result.status).toBe('OK');
      expect(result.body).toBe('large_document_signed_base64');
    });
  });

  describe('verificarDisponibilidad', () => {
    it('should verify service availability', async () => {
      const mockHealthResponse = {
        data: {
          status: 'OK',
          version: '1.0.0',
          descripcion: 'Servicio de firmado operativo'
        }
      };

      mockedAxios.get.mockResolvedValue(mockHealthResponse);

      const result = await service.verificarDisponibilidad();

      expect(result).toBe(true);
      expect(mockedAxios.get).toHaveBeenCalledWith(
        'http://localhost:8113/health',
        { timeout: 5000 }
      );
    });

    it('should return false when service is unavailable', async () => {
      mockedAxios.get.mockRejectedValue(new Error('Service unavailable'));

      const result = await service.verificarDisponibilidad();

      expect(result).toBe(false);
    });
  });

  describe('obtenerInformacionCertificado', () => {
    it('should get certificate information', async () => {
      const mockCertInfo = {
        data: {
          status: 'OK',
          body: {
            emisor: 'AUTORIDAD CERTIFICADORA DE EL SALVADOR',
            titular: 'EMPRESA DE PRUEBA S.A. DE C.V.',
            nit: '06142803901121',
            validoDesde: '2024-01-01T00:00:00.000Z',
            validoHasta: '2025-12-31T23:59:59.999Z',
            estado: 'VIGENTE'
          }
        }
      };

      mockedAxios.get.mockResolvedValue(mockCertInfo);

      const result = await service.obtenerInformacionCertificado();

      expect(result).toEqual({
        emisor: 'AUTORIDAD CERTIFICADORA DE EL SALVADOR',
        titular: 'EMPRESA DE PRUEBA S.A. DE C.V.',
        nit: '06142803901121',
        validoDesde: '2024-01-01T00:00:00.000Z',
        validoHasta: '2025-12-31T23:59:59.999Z',
        estado: 'VIGENTE'
      });
    });

    it('should handle certificate not found', async () => {
      const mockErrorResponse = {
        data: {
          status: 'ERROR',
          descripcion: 'Certificado no encontrado'
        }
      };

      mockedAxios.get.mockResolvedValue(mockErrorResponse);

      await expect(service.obtenerInformacionCertificado())
        .rejects.toThrow('Certificado no encontrado');
    });
  });

  describe('C# compatibility tests', () => {
    it('should use same request format as C# ConsumoFirmador', async () => {
      // Basado en el análisis de FacturacionElectronica/ConsumoFirmador/
      const mockDocument = testUtils.createValidFeDto();
      
      const mockSignResponse = {
        data: {
          status: 'OK',
          body: 'signed_document_base64',
          descripcion: 'Firmado correctamente'
        }
      };

      mockedAxios.post.mockResolvedValue(mockSignResponse);

      await service.firmarDocumento(mockDocument);

      // Verificar que el formato del request es compatible con C#
      expect(mockedAxios.post).toHaveBeenCalledWith(
        'http://localhost:8113/firmardocumento/',
        mockDocument,
        {
          headers: {
            'Content-Type': 'application/json'
          },
          timeout: 30000
        }
      );
    });

    it('should handle same response format as C# system', async () => {
      const mockDocument = testUtils.createValidFeDto();
      
      // Formato de respuesta compatible con C#
      const mockCSharpResponse = {
        data: {
          status: 'OK',
          body: 'eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...',
          descripcion: 'Documento firmado exitosamente'
        }
      };

      mockedAxios.post.mockResolvedValue(mockCSharpResponse);

      const result = await service.firmarDocumento(mockDocument);

      expect(result).toMatchObject({
        status: 'OK',
        body: expect.stringMatching(/^eyJ/), // JWT format
        descripcion: expect.any(String)
      });
    });

    it('should handle same error format as C# system', async () => {
      const mockDocument = testUtils.createValidFeDto();
      
      // Formato de error compatible con C#
      const mockCSharpError = {
        data: {
          status: 'ERROR',
          descripcion: 'Error en certificado digital',
          codigo: 'CERT_001'
        }
      };

      mockedAxios.post.mockResolvedValue(mockCSharpError);

      await expect(service.firmarDocumento(mockDocument))
        .rejects.toThrow(InternalServerErrorException);
    });
  });

  describe('Performance tests', () => {
    it('should handle concurrent signing requests', async () => {
      const mockDocument = testUtils.createValidFeDto();
      
      const mockResponse = {
        data: {
          status: 'OK',
          body: 'concurrent_signed_document',
          descripcion: 'Firmado correctamente'
        }
      };

      mockedAxios.post.mockResolvedValue(mockResponse);

      // Simular 5 requests concurrentes
      const promises = Array.from({ length: 5 }, () => 
        service.firmarDocumento(mockDocument)
      );

      const results = await Promise.all(promises);

      expect(results).toHaveLength(5);
      results.forEach(result => {
        expect(result.status).toBe('OK');
        expect(result.body).toBe('concurrent_signed_document');
      });

      expect(mockedAxios.post).toHaveBeenCalledTimes(5);
    });

    it('should respect timeout configuration', async () => {
      const mockDocument = testUtils.createValidFeDto();
      
      // Simular timeout
      mockedAxios.post.mockImplementation(() => 
        new Promise((_, reject) => 
          setTimeout(() => reject(new Error('timeout of 30000ms exceeded')), 100)
        )
      );

      const startTime = Date.now();
      
      await expect(service.firmarDocumento(mockDocument))
        .rejects.toThrow(InternalServerErrorException);
      
      const endTime = Date.now();
      
      // Verificar que no esperó más del timeout configurado + margen
      expect(endTime - startTime).toBeLessThan(35000);
    });
  });
});