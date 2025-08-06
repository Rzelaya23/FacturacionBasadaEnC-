// Global test setup
import 'reflect-metadata';

// Mock console methods to reduce noise in tests
global.console = {
  ...console,
  // Uncomment to ignore specific log levels
  // log: jest.fn(),
  // debug: jest.fn(),
  // info: jest.fn(),
  // warn: jest.fn(),
  // error: jest.fn(),
};

// Global test utilities
global.testUtils = {
  // Helper para crear DTOs de prueba válidos
  createValidFeDto: () => ({
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
      tipoDocumento: '36',
      numDocumento: '12345678901234',
      nrc: '7654321',
      nombre: 'CLIENTE DE PRUEBA',
      codActividad: '01111',
      descActividad: 'Cultivo de maíz',
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
        tributos: ['20']
      }
    ],
    resumen: {
      totalNoSuj: 0,
      totalExenta: 0,
      totalGravada: 10.00,
      subTotalVentas: 10.00,
      descuNoSuj: 0,
      descuExenta: 0,
      descuGravada: 0,
      porcentajeDescuento: 0,
      totalDescu: 0,
      tributos: [
        {
          codigo: '20',
          descripcion: 'Impuesto al Valor Agregado 13%',
          valor: 1.30
        }
      ],
      subTotal: 10.00,
      ivaRete1: 0,
      reteRenta: 0,
      montoTotalOperacion: 11.30,
      totalNoGravado: 0,
      totalPagar: 11.30,
      totalLetras: 'ONCE 30/100 DÓLARES',
      totalIva: 1.30,
      saldoFavor: 0,
      condicionOperacion: 1,
      pagos: [
        {
          codigo: '01',
          montoPago: 11.30,
          referencia: 'EFECTIVO',
          plazo: '01',
          periodo: 0
        }
      ],
      numPagoElectronico: null
    }
  }),

  // Helper para crear identificación válida
  createValidIdentificacion: () => ({
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
  }),

  // Helper para crear respuesta del MH válida
  createValidMhResponse: () => ({
    selloRecibido: 'SELLO123456789',
    fhProcesamiento: '2024-08-04T10:30:00.000Z',
    estado: 'PROCESADO',
    codigoMsg: '001',
    descripcionMsg: 'DTE procesado correctamente'
  }),

  // Helper para crear documento firmado mock
  createMockSignedDocument: () => 'eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...',

  // Helper para validar estructura de DTE
  validateDteStructure: (dte: any) => {
    const requiredFields = ['identificacion', 'emisor', 'receptor', 'cuerpoDocumento', 'resumen'];
    return requiredFields.every(field => dte.hasOwnProperty(field));
  },

  // Helper para generar NITs válidos para pruebas
  generateValidNit: () => '06142803901121',
  
  // Helper para generar códigos de generación válidos
  generateValidCodigoGeneracion: () => 'A1B2C3D4-E5F6-7890-ABCD-123456789012',
  
  // Helper para generar números de control válidos
  generateValidNumeroControl: (tipoDte = '01') => `DTE-${tipoDte}-0001-0001-000000000000001`,
};

// Extend global types
declare global {
  namespace NodeJS {
    interface Global {
      testUtils: any;
    }
  }
}

// Setup para mocks comunes
beforeEach(() => {
  // Reset all mocks before each test
  jest.clearAllMocks();
});

// Cleanup después de cada test
afterEach(() => {
  // Cleanup any test-specific setup
  jest.restoreAllMocks();
});