import { Test, TestingModule } from '@nestjs/testing';
import { BadRequestException, InternalServerErrorException } from '@nestjs/common';
import { getRepositoryToken } from '@nestjs/typeorm';
import { Repository } from 'typeorm';

import { FeService } from './fe.service';
import { CreateFacturaElectronicaDto } from './dto/create-factura-electronica.dto';
import { FirmadorService } from '../../firmador/firmador.service';
import { MhIntegrationService } from '../../mh-integration/mh-integration.service';
import { CodigoGeneracionService } from '../../common/services/codigo-generacion.service';
import { Dte } from '../../common/entities/dte.entity';

describe('FeService', () => {
  let service: FeService;
  let mockDteRepository: Partial<Repository<Dte>>;
  let mockFirmadorService: Partial<FirmadorService>;
  let mockMhIntegrationService: Partial<MhIntegrationService>;
  let mockCodigoGeneracionService: Partial<CodigoGeneracionService>;

  beforeEach(async () => {
    // Mock del repositorio
    mockDteRepository = {
      save: jest.fn(),
      findOne: jest.fn(),
      query: jest.fn().mockResolvedValue([{ result: 1 }])
    };

    // Mock del servicio firmador
    mockFirmadorService = {
      signFE: jest.fn().mockResolvedValue({
        body: { documento: 'documento-firmado' }
      }),
      checkSignerAvailability: jest.fn().mockResolvedValue(true)
    };

    // Mock del servicio MH
    mockMhIntegrationService = {
      sendDTE: jest.fn().mockResolvedValue({
        estado: 'PROCESADO',
        selloRecibido: '2024-08-04T10:30:00.000Z',
        clasificaMsg: '01',
        codigoMsg: '001',
        descripcionMsg: 'DTE procesado correctamente'
      }),
      checkConnectivity: jest.fn().mockResolvedValue(true)
    };

    // Mock del servicio de códigos
    mockCodigoGeneracionService = {
      generateIdentificacionCompleta: jest.fn().mockReturnValue({
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
      })
    };

    const module: TestingModule = await Test.createTestingModule({
      providers: [
        FeService,
        {
          provide: getRepositoryToken(Dte),
          useValue: mockDteRepository
        },
        {
          provide: FirmadorService,
          useValue: mockFirmadorService
        },
        {
          provide: MhIntegrationService,
          useValue: mockMhIntegrationService
        },
        {
          provide: CodigoGeneracionService,
          useValue: mockCodigoGeneracionService
        }
      ]
    }).compile();

    service = module.get<FeService>(FeService);
  });

  it('should be defined', () => {
    expect(service).toBeDefined();
  });

  describe('create', () => {
    const validFeDto: CreateFacturaElectronicaDto = {
      identificacion: {
        version: 1,
        ambiente: '00',
        tipoDte: '01',
        numeroControl: 'DTE-01-00000001-000000000000001',
        codigoGeneracion: '12345678-1234-1234-1234-123456789012',
        tipoModelo: 1,
        tipoOperacion: 1,
        tipoContingencia: null,
        motivoContin: null,
        fecEmi: '2024-08-04',
        horEmi: '10:30:00',
        tipoMoneda: 'USD'
      },
      emisor: {
        nit: '06142803901121',
        nrc: '1234567',
        nombre: 'EMPRESA DE PRUEBA S.A. DE C.V.',
        codActividad: '01111',
        descActividad: 'Cultivo de maíz',
        nombreComercial: 'EMPRESA PRUEBA',
        tipoEstablecimiento: '01',
        direccion: {
          departamento: '06',
          municipio: '14',
          complemento: 'COLONIA ESCALON, CALLE PRINCIPAL #123'
        },
        telefono: '22501234',
        correo: 'empresa@prueba.com'
      },
      receptor: {
        nit: null,
        nrc: null,
        nombre: 'CLIENTE DE PRUEBA',
        codActividad: null,
        descActividad: null,
        direccion: {
          departamento: '06',
          municipio: '14',
          complemento: 'COLONIA MIRAMONTE, AVENIDA PRINCIPAL #456'
        },
        telefono: '22509876',
        correo: 'cliente@prueba.com'
      },
      cuerpoDocumento: [
        {
          numItem: 1,
          tipoItem: 2,
          numeroDocumento: null,
          cantidad: 10,
          codigo: 'PROD001',
          codTributo: null,
          uniMedida: 59,
          descripcion: 'PRODUCTO DE PRUEBA',
          precioUni: 5.75,
          montoDescu: 0,
          ventaNoSuj: 0,
          ventaExenta: 0,
          ventaGravada: 57.50,
          tributos: ['20'],
          psv: 5.75,
          noGravado: 0,
          ivaItem: 7.48
        }
      ],
      resumen: {
        totalNoSuj: 0,
        totalExenta: 0,
        totalGravada: 57.50,
        subTotalVentas: 57.50,
        descuNoSuj: 0,
        descuExenta: 0,
        descuGravada: 0,
        porcentajeDescuento: 0,
        totalDescu: 0,
        tributos: [
          {
            codigo: '20',
            descripcion: 'Impuesto al Valor Agregado 13%',
            valor: 7.48
          }
        ],
        subTotal: 57.50,
        ivaRete1: 0,
        reteRenta: 0,
        montoTotalOperacion: 64.98,
        totalNoGravado: 0,
        totalPagar: 64.98,
        totalLetras: 'SESENTA Y CUATRO 98/100 DÓLARES',
        totalIva: 7.48,
        saldoFavor: 0,
        condicionOperacion: 1,
        pagos: [
          {
            codigo: '01',
            montoPago: 64.98,
            referencia: 'EFECTIVO',
            plazo: '01',
            periodo: 0
          }
        ],
        numPagoElectronico: null
      },
      extension: {
        nombEntrega: 'Juan Pérez',
        docuEntrega: '12345678-9',
        nombRecibe: 'María García',
        docuRecibe: '98765432-1',
        observaciones: 'Entrega en horario de oficina'
      }
    };

    it('should create FE successfully', async () => {
      const result = await service.create(validFeDto);

      expect(result).toMatchObject({
        codigoGeneracion: validFeDto.identificacion.codigoGeneracion,
        numeroControl: validFeDto.identificacion.numeroControl
      });
      expect(mockFirmadorService.signFE).toHaveBeenCalled();
      expect(mockMhIntegrationService.sendDTE).toHaveBeenCalled();
      expect(mockDteRepository.save).toHaveBeenCalled();
    });
  });

  describe('validate', () => {
    it('should validate FE successfully', async () => {
      const validDto = {
        identificacion: {
          tipoDte: '01',
          codigoGeneracion: '12345678-1234-1234-1234-123456789012',
          fecEmi: '2024-08-04'
        },
        cuerpoDocumento: [
          {
            numItem: 1,
            tipoItem: 2,
            numeroDocumento: null,
            cantidad: 1,
            codigo: 'PROD001',
            codTributo: null,
            uniMedida: 59,
            descripcion: 'PRODUCTO DE PRUEBA',
            precioUni: 10.00,
            montoDescu: 0,
            ventaNoSuj: 0,
            ventaExenta: 0,
            ventaGravada: 10.00,
            tributos: ['20'],
            psv: 10.00,
            noGravado: 0,
            ivaItem: 1.30
          }
        ],
        resumen: {
          totalNoSuj: 0,
          totalExenta: 0,
          totalGravada: 10.00,
          totalPagar: 11.30
        }
      };

      const result = await service.validate(validDto as any);

      expect(result.valid).toBe(true);
      expect(result.errors).toHaveLength(0);
    });
  });

  describe('generateCodes', () => {
    it('should generate codes successfully', async () => {
      const result = await service.generateCodes();

      expect(result).toMatchObject({
        codigoGeneracion: expect.any(String),
        numeroControl: expect.any(String),
        tipoDte: '01'
      });
      expect(mockCodigoGeneracionService.generateIdentificacionCompleta).toHaveBeenCalledWith('01');
    });
  });

  describe('healthCheck', () => {
    it('should return health status', async () => {
      const result = await service.healthCheck();

      expect(result).toMatchObject({
        firmador: true,
        mh: true,
        database: true
      });
    });
  });
});