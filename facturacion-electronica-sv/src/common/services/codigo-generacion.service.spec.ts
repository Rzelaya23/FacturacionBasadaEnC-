import { Test, TestingModule } from '@nestjs/testing';
import { CodigoGeneracionService } from './codigo-generacion.service';

describe('CodigoGeneracionService', () => {
  let service: CodigoGeneracionService;

  beforeEach(async () => {
    const module: TestingModule = await Test.createTestingModule({
      providers: [CodigoGeneracionService],
    }).compile();

    service = module.get<CodigoGeneracionService>(CodigoGeneracionService);
  });

  it('should be defined', () => {
    expect(service).toBeDefined();
  });

  describe('generateCodigoGeneracion', () => {
    it('should generate a valid UUID v4 in uppercase', () => {
      const codigo = service.generateCodigoGeneracion();
      
      // Verificar formato UUID v4 en mayúsculas
      const uuidPattern = /^[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}$/;
      expect(codigo).toMatch(uuidPattern);
      
      // Verificar que está en mayúsculas
      expect(codigo).toBe(codigo.toUpperCase());
      
      // Verificar longitud
      expect(codigo).toHaveLength(36);
    });

    it('should generate unique codes', () => {
      const codigo1 = service.generateCodigoGeneracion();
      const codigo2 = service.generateCodigoGeneracion();
      
      expect(codigo1).not.toBe(codigo2);
    });
  });

  describe('generateNumeroControl', () => {
    it('should generate valid numero control with default values', () => {
      const numeroControl = service.generateNumeroControl('01');
      
      // Verificar formato: DTE-01-0001-0001-000000000000001
      const pattern = /^DTE-01-0001-0001-\d{15}$/;
      expect(numeroControl).toMatch(pattern);
    });

    it('should generate numero control with custom values', () => {
      const numeroControl = service.generateNumeroControl('03', '0002', '0003', 123);
      
      expect(numeroControl).toBe('DTE-03-0002-0003-000000000000123');
    });

    it('should pad establishment and punto venta with zeros', () => {
      const numeroControl = service.generateNumeroControl('01', '1', '2', 1);
      
      expect(numeroControl).toBe('DTE-01-0001-0002-000000000000001');
    });

    it('should handle large correlativo numbers', () => {
      const numeroControl = service.generateNumeroControl('01', '0001', '0001', 999999999999999);
      
      expect(numeroControl).toBe('DTE-01-0001-0001-999999999999999');
    });
  });

  describe('validateCodigoGeneracion', () => {
    it('should validate correct codigo generacion', () => {
      const validCodigo = 'A1B2C3D4-E5F6-7890-ABCD-123456789012';
      expect(service.validateCodigoGeneracion(validCodigo)).toBe(true);
    });

    it('should reject invalid codigo generacion', () => {
      const invalidCodigos = [
        'invalid-uuid',
        'a1b2c3d4-e5f6-7890-abcd-123456789012', // lowercase
        'A1B2C3D4-E5F6-7890-ABCD-12345678901', // too short
        'A1B2C3D4-E5F6-7890-ABCD-1234567890123', // too long
        '',
        null,
        undefined
      ];

      invalidCodigos.forEach(codigo => {
        expect(service.validateCodigoGeneracion(codigo as string)).toBe(false);
      });
    });
  });

  describe('validateNumeroControl', () => {
    it('should validate correct numero control', () => {
      const validNumero = 'DTE-01-0001-0001-000000000000001';
      expect(service.validateNumeroControl(validNumero)).toBe(true);
    });

    it('should reject invalid numero control', () => {
      const invalidNumeros = [
        'DTE-1-0001-0001-000000000000001', // tipo DTE sin padding
        'DTE-01-001-0001-000000000000001', // establecimiento sin padding
        'DTE-01-0001-001-000000000000001', // punto venta sin padding
        'DTE-01-0001-0001-00000000000001', // correlativo muy corto
        'DTE-01-0001-0001-0000000000000001', // correlativo muy largo
        'invalid-format',
        '',
        null,
        undefined
      ];

      invalidNumeros.forEach(numero => {
        expect(service.validateNumeroControl(numero as string)).toBe(false);
      });
    });
  });

  describe('parseNumeroControl', () => {
    it('should parse valid numero control', () => {
      const numeroControl = 'DTE-03-0002-0005-000000000000123';
      const parsed = service.parseNumeroControl(numeroControl);
      
      expect(parsed).toEqual({
        tipoDte: '03',
        establecimiento: '0002',
        puntoVenta: '0005',
        correlativo: '000000000000123'
      });
    });

    it('should return null for invalid numero control', () => {
      const invalidNumero = 'invalid-format';
      const parsed = service.parseNumeroControl(invalidNumero);
      
      expect(parsed).toBeNull();
    });
  });

  describe('generateFechaHoraEmision', () => {
    it('should generate valid fecha and hora', () => {
      const { fecEmi, horEmi } = service.generateFechaHoraEmision();
      
      // Verificar formato fecha YYYY-MM-DD
      const fechaPattern = /^\d{4}-\d{2}-\d{2}$/;
      expect(fecEmi).toMatch(fechaPattern);
      
      // Verificar formato hora HH:mm:ss
      const horaPattern = /^\d{2}:\d{2}:\d{2}$/;
      expect(horEmi).toMatch(horaPattern);
      
      // Verificar que la fecha es válida
      const fecha = new Date(fecEmi);
      expect(fecha.getTime()).not.toBeNaN();
    });

    it('should generate current date and time', () => {
      const before = new Date();
      const { fecEmi, horEmi } = service.generateFechaHoraEmision();
      const after = new Date();
      
      const fechaGenerada = new Date(`${fecEmi}T${horEmi}`);
      
      expect(fechaGenerada.getTime()).toBeGreaterThanOrEqual(before.getTime() - 1000);
      expect(fechaGenerada.getTime()).toBeLessThanOrEqual(after.getTime() + 1000);
    });
  });

  describe('generateIdentificacionCompleta', () => {
    it('should generate complete identification with default values', () => {
      const identificacion = service.generateIdentificacionCompleta('01');
      
      expect(identificacion).toMatchObject({
        version: 1,
        ambiente: '00',
        tipoDte: '01',
        tipoModelo: 1,
        tipoOperacion: 1,
        tipoMoneda: 'USD'
      });
      
      // Verificar que se generaron los campos requeridos
      expect(identificacion.numeroControl).toMatch(/^DTE-01-0001-0001-\d{15}$/);
      expect(identificacion.codigoGeneracion).toMatch(/^[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}$/);
      expect(identificacion.fecEmi).toMatch(/^\d{4}-\d{2}-\d{2}$/);
      expect(identificacion.horEmi).toMatch(/^\d{2}:\d{2}:\d{2}$/);
    });

    it('should generate complete identification with custom values', () => {
      const identificacion = service.generateIdentificacionCompleta(
        '03', // tipoDte
        '01', // ambiente
        2,    // tipoModelo
        2,    // tipoOperacion
        '0005', // establecimiento
        '0010', // puntoVenta
        999     // correlativo
      );
      
      expect(identificacion).toMatchObject({
        version: 1,
        ambiente: '01',
        tipoDte: '03',
        numeroControl: 'DTE-03-0005-0010-000000000000999',
        tipoModelo: 2,
        tipoOperacion: 2,
        tipoMoneda: 'USD'
      });
    });
  });

  describe('Integration with C# reference', () => {
    it('should generate codes compatible with C# system format', () => {
      // Basado en el análisis del código C#, verificar compatibilidad
      const identificacion = service.generateIdentificacionCompleta('01');
      
      // Verificar que los formatos coinciden con lo esperado por el MH
      expect(identificacion.version).toBe(1);
      expect(identificacion.ambiente).toMatch(/^0[0-1]$/); // 00 o 01
      expect(identificacion.tipoDte).toMatch(/^0[1-9]|1[1-4]$/); // 01-14
      expect(identificacion.tipoMoneda).toBe('USD');
      
      // Verificar que el código de generación es válido
      expect(service.validateCodigoGeneracion(identificacion.codigoGeneracion)).toBe(true);
      expect(service.validateNumeroControl(identificacion.numeroControl)).toBe(true);
    });
  });
});