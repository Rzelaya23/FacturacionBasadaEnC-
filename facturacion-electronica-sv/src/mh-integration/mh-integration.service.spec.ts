import { Test, TestingModule } from '@nestjs/testing';
import { ConfigService } from '@nestjs/config';
import { BadRequestException, InternalServerErrorException } from '@nestjs/common';
import { MhIntegrationService } from './mh-integration.service';
import axios from 'axios';

// Mock axios
jest.mock('axios');
const mockedAxios = axios as jest.Mocked<typeof axios>;

describe('MhIntegrationService', () => {
  let service: MhIntegrationService;
  let configService: ConfigService;

  const mockConfigService = {
    get: jest.fn((key: string) => {
      const config = {
        'MH_ENVIRONMENT': 'test',
        'MH_USER': 'test_user',
        'MH_PASSWORD': 'test_password',
        'MH_AUTH_URL_TEST': 'https://apitest.dtes.mh.gob.sv/seguridad/auth',
        'MH_RECEPTION_URL_TEST': 'https://apitest.dtes.mh.gob.sv/fesv/recepciondte',
        'MH_CANCELLATION_URL_TEST': 'https://apitest.dtes.mh.gob.sv/fesv/anulardte',
        'MH_AUTH_URL_PROD': 'https://api.dtes.mh.gob.sv/seguridad/auth',
        'MH_RECEPTION_URL_PROD': 'https://api.dtes.mh.gob.sv/fesv/recepciondte',
        'MH_CANCELLATION_URL_PROD': 'https://api.dtes.mh.gob.sv/fesv/anulardte',
      };
      return config[key];
    }),
  };

  beforeEach(async () => {
    const module: TestingModule = await Test.createTestingModule({
      providers: [
        MhIntegrationService,
        {
          provide: ConfigService,
          useValue: mockConfigService,
        },
      ],
    }).compile();

    service = module.get<MhIntegrationService>(MhIntegrationService);
    configService = module.get<ConfigService>(ConfigService);

    // Reset mocks
    jest.clearAllMocks();
  });

  it('should be defined', () => {
    expect(service).toBeDefined();
  });

  describe('autenticar', () => {
    it('should authenticate successfully in test environment', async () => {
      const mockAuthResponse = {
        data: {
          status: 'OK',
          body: {
            token: 'test_token_123456789'
          }
        }
      };

      mockedAxios.post.mockResolvedValue(mockAuthResponse);

      const result = await service.autenticar();

      expect(result).toBe('test_token_123456789');
      expect(mockedAxios.post).toHaveBeenCalledWith(
        'https://apitest.dtes.mh.gob.sv/seguridad/auth',
        {
          user: 'test_user',
          pwd: 'test_password'
        },
        {
          headers: {
            'Content-Type': 'application/json'
          }
        }
      );
    });

    it('should authenticate successfully in production environment', async () => {
      mockConfigService.get.mockImplementation((key: string) => {
        if (key === 'MH_ENVIRONMENT') return 'production';
        return mockConfigService.get(key);
      });

      const mockAuthResponse = {
        data: {
          status: 'OK',
          body: {
            token: 'prod_token_123456789'
          }
        }
      };

      mockedAxios.post.mockResolvedValue(mockAuthResponse);

      const result = await service.autenticar();

      expect(result).toBe('prod_token_123456789');
      expect(mockedAxios.post).toHaveBeenCalledWith(
        'https://api.dtes.mh.gob.sv/seguridad/auth',
        expect.any(Object),
        expect.any(Object)
      );
    });

    it('should handle authentication errors', async () => {
      const mockErrorResponse = {
        data: {
          status: 'ERROR',
          descripcionMsg: 'Credenciales inválidas'
        }
      };

      mockedAxios.post.mockResolvedValue(mockErrorResponse);

      await expect(service.autenticar()).rejects.toThrow(BadRequestException);
    });

    it('should handle network errors', async () => {
      mockedAxios.post.mockRejectedValue(new Error('Network error'));

      await expect(service.autenticar()).rejects.toThrow(InternalServerErrorException);
    });

    it('should handle missing token in response', async () => {
      const mockInvalidResponse = {
        data: {
          status: 'OK',
          body: {} // Sin token
        }
      };

      mockedAxios.post.mockResolvedValue(mockInvalidResponse);

      await expect(service.autenticar()).rejects.toThrow(InternalServerErrorException);
    });
  });

  describe('enviarDTE', () => {
    const mockDocumentoFirmado = 'documento_firmado_base64_string';
    const mockToken = 'valid_token_123456789';

    beforeEach(() => {
      // Mock successful authentication
      jest.spyOn(service, 'autenticar').mockResolvedValue(mockToken);
    });

    it('should send DTE successfully', async () => {
      const mockSendResponse = {
        data: {
          status: 'RECIBIDO',
          body: {
            selloRecibido: 'SELLO123456789',
            fhProcesamiento: '2024-08-04T10:30:00.000Z',
            clasificaMsg: 'PROCESADO',
            codigoMsg: '001',
            descripcionMsg: 'DTE procesado correctamente'
          }
        }
      };

      mockedAxios.post.mockResolvedValue(mockSendResponse);

      const result = await service.enviarDTE(mockDocumentoFirmado);

      expect(result).toEqual({
        selloRecibido: 'SELLO123456789',
        fhProcesamiento: '2024-08-04T10:30:00.000Z',
        estado: 'PROCESADO',
        codigoMsg: '001',
        descripcionMsg: 'DTE procesado correctamente'
      });

      expect(mockedAxios.post).toHaveBeenCalledWith(
        'https://apitest.dtes.mh.gob.sv/fesv/recepciondte',
        {
          ambiente: '00',
          idEnvio: expect.any(Number),
          version: 1,
          tipoDte: '01',
          documento: mockDocumentoFirmado
        },
        {
          headers: {
            'Content-Type': 'application/json',
            'Authorization': mockToken
          }
        }
      );
    });

    it('should handle DTE rejection', async () => {
      const mockRejectionResponse = {
        data: {
          status: 'RECHAZADO',
          body: {
            clasificaMsg: 'RECHAZADO',
            codigoMsg: '002',
            descripcionMsg: 'DTE con errores de validación',
            observaciones: ['NIT del emisor inválido', 'Monto total incorrecto']
          }
        }
      };

      mockedAxios.post.mockResolvedValue(mockRejectionResponse);

      await expect(service.enviarDTE(mockDocumentoFirmado)).rejects.toThrow(BadRequestException);
    });

    it('should handle authentication failure during send', async () => {
      jest.spyOn(service, 'autenticar').mockRejectedValue(new Error('Auth failed'));

      await expect(service.enviarDTE(mockDocumentoFirmado)).rejects.toThrow(InternalServerErrorException);
    });

    it('should handle network errors during send', async () => {
      mockedAxios.post.mockRejectedValue(new Error('Network timeout'));

      await expect(service.enviarDTE(mockDocumentoFirmado)).rejects.toThrow(InternalServerErrorException);
    });

    it('should generate unique idEnvio for each request', async () => {
      const mockResponse = {
        data: {
          status: 'RECIBIDO',
          body: {
            selloRecibido: 'SELLO123',
            clasificaMsg: 'PROCESADO'
          }
        }
      };

      mockedAxios.post.mockResolvedValue(mockResponse);

      // Enviar dos DTEs
      await service.enviarDTE(mockDocumentoFirmado);
      await service.enviarDTE(mockDocumentoFirmado);

      // Verificar que se llamó dos veces con diferentes idEnvio
      expect(mockedAxios.post).toHaveBeenCalledTimes(2);
      
      const firstCall = mockedAxios.post.mock.calls[0][1];
      const secondCall = mockedAxios.post.mock.calls[1][1];
      
      expect(firstCall.idEnvio).not.toBe(secondCall.idEnvio);
    });
  });

  describe('anularDTE', () => {
    const mockDatosAnulacion = {
      codigoGeneracion: 'A1B2C3D4-E5F6-7890-ABCD-123456789012',
      motivo: {
        tipoAnulacion: 1,
        motivoAnulacion: 'Error en datos del cliente',
        nombreResponsable: 'Juan Pérez',
        tipDocResponsable: '36',
        numDocResponsable: '12345678901234',
        nombreSolicita: 'María García',
        tipDocSolicita: '36',
        numDocSolicita: '98765432109876'
      }
    };

    beforeEach(() => {
      jest.spyOn(service, 'autenticar').mockResolvedValue('valid_token');
    });

    it('should cancel DTE successfully', async () => {
      const mockCancelResponse = {
        data: {
          status: 'PROCESADO',
          body: {
            selloRecibido: 'ANULACION_SELLO123',
            fhProcesamiento: '2024-08-04T11:00:00.000Z',
            clasificaMsg: 'PROCESADO',
            descripcionMsg: 'Anulación procesada correctamente'
          }
        }
      };

      mockedAxios.post.mockResolvedValue(mockCancelResponse);

      const result = await service.anularDTE(mockDatosAnulacion);

      expect(result).toEqual({
        selloRecibido: 'ANULACION_SELLO123',
        fhProcesamiento: '2024-08-04T11:00:00.000Z',
        estado: 'PROCESADO',
        descripcionMsg: 'Anulación procesada correctamente'
      });

      expect(mockedAxios.post).toHaveBeenCalledWith(
        'https://apitest.dtes.mh.gob.sv/fesv/anulardte',
        expect.objectContaining({
          codigoGeneracion: 'A1B2C3D4-E5F6-7890-ABCD-123456789012'
        }),
        expect.objectContaining({
          headers: expect.objectContaining({
            'Authorization': 'valid_token'
          })
        })
      );
    });

    it('should handle cancellation rejection', async () => {
      const mockRejectionResponse = {
        data: {
          status: 'RECHAZADO',
          body: {
            clasificaMsg: 'RECHAZADO',
            descripcionMsg: 'DTE no puede ser anulado'
          }
        }
      };

      mockedAxios.post.mockResolvedValue(mockRejectionResponse);

      await expect(service.anularDTE(mockDatosAnulacion)).rejects.toThrow(BadRequestException);
    });
  });

  describe('consultarDTE', () => {
    it('should query DTE status successfully', async () => {
      jest.spyOn(service, 'autenticar').mockResolvedValue('valid_token');

      const mockQueryResponse = {
        data: {
          status: 'OK',
          body: {
            codigoGeneracion: 'A1B2C3D4-E5F6-7890-ABCD-123456789012',
            selloRecibido: 'SELLO123456789',
            estado: 'PROCESADO',
            fhProcesamiento: '2024-08-04T10:30:00.000Z'
          }
        }
      };

      mockedAxios.get.mockResolvedValue(mockQueryResponse);

      const result = await service.consultarDTE('A1B2C3D4-E5F6-7890-ABCD-123456789012');

      expect(result).toEqual({
        codigoGeneracion: 'A1B2C3D4-E5F6-7890-ABCD-123456789012',
        selloRecibido: 'SELLO123456789',
        estado: 'PROCESADO',
        fhProcesamiento: '2024-08-04T10:30:00.000Z'
      });
    });

    it('should handle DTE not found', async () => {
      jest.spyOn(service, 'autenticar').mockResolvedValue('valid_token');

      const mockNotFoundResponse = {
        data: {
          status: 'ERROR',
          descripcionMsg: 'DTE no encontrado'
        }
      };

      mockedAxios.get.mockResolvedValue(mockNotFoundResponse);

      await expect(service.consultarDTE('INVALID-CODE')).rejects.toThrow(BadRequestException);
    });
  });

  describe('C# compatibility tests', () => {
    it('should use same authentication format as C# system', async () => {
      // Basado en el análisis del código C#, verificar formato de autenticación
      const mockAuthResponse = {
        data: {
          status: 'OK',
          body: {
            token: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'
          }
        }
      };

      mockedAxios.post.mockResolvedValue(mockAuthResponse);

      const token = await service.autenticar();

      // Verificar que el formato del request es compatible con C#
      expect(mockedAxios.post).toHaveBeenCalledWith(
        expect.any(String),
        {
          user: 'test_user',
          pwd: 'test_password'
        },
        {
          headers: {
            'Content-Type': 'application/json'
          }
        }
      );

      expect(token).toContain('Bearer');
    });

    it('should send DTE with same structure as C# system', async () => {
      jest.spyOn(service, 'autenticar').mockResolvedValue('Bearer token123');

      const mockResponse = {
        data: {
          status: 'RECIBIDO',
          body: {
            selloRecibido: 'SELLO123',
            clasificaMsg: 'PROCESADO'
          }
        }
      };

      mockedAxios.post.mockResolvedValue(mockResponse);

      await service.enviarDTE('documento_firmado');

      // Verificar estructura del payload compatible con C#
      const payload = mockedAxios.post.mock.calls[0][1];
      
      expect(payload).toMatchObject({
        ambiente: expect.any(String),
        idEnvio: expect.any(Number),
        version: 1,
        tipoDte: '01',
        documento: 'documento_firmado'
      });

      // Verificar headers compatibles
      const headers = mockedAxios.post.mock.calls[0][2].headers;
      expect(headers).toMatchObject({
        'Content-Type': 'application/json',
        'Authorization': 'Bearer token123'
      });
    });
  });
});