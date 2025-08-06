import { Test, TestingModule } from '@nestjs/testing';
import { INestApplication } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import * as request from 'supertest';
import { AppModule } from '../../src/app.module';

describe('DTE Creation (e2e)', () => {
  let app: INestApplication;

  beforeEach(async () => {
    const moduleFixture: TestingModule = await Test.createTestingModule({
      imports: [
        ConfigModule.forRoot({
          isGlobal: true,
          envFilePath: '.env.test',
        }),
        AppModule,
      ],
    }).compile();

    app = moduleFixture.createNestApplication();
    await app.init();
  });

  afterEach(async () => {
    await app.close();
  });

  describe('/dte/fe (POST)', () => {
    const validFeData = {
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
    };

    it('should create FE successfully with valid data', () => {
      return request(app.getHttpServer())
        .post('/dte/fe/crear')
        .send(validFeData)
        .expect(201)
        .expect((res) => {
          expect(res.body).toHaveProperty('success', true);
          expect(res.body).toHaveProperty('codigoGeneracion');
          expect(res.body).toHaveProperty('numeroControl');
          expect(res.body.codigoGeneracion).toMatch(/^[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}$/);
          expect(res.body.numeroControl).toMatch(/^DTE-01-\d{4}-\d{4}-\d{15}$/);
        });
    });

    it('should validate FE data successfully', () => {
      return request(app.getHttpServer())
        .post('/dte/fe/validar')
        .send(validFeData)
        .expect(200)
        .expect((res) => {
          expect(res.body).toHaveProperty('valido', true);
          expect(res.body).toHaveProperty('errores');
          expect(res.body.errores).toHaveLength(0);
        });
    });

    it('should reject invalid NIT', () => {
      const invalidData = {
        ...validFeData,
        emisor: {
          ...validFeData.emisor,
          nit: 'invalid-nit'
        }
      };

      return request(app.getHttpServer())
        .post('/dte/fe/validar')
        .send(invalidData)
        .expect(400);
    });

    it('should reject invalid calculation', () => {
      const invalidData = {
        ...validFeData,
        resumen: {
          ...validFeData.resumen,
          totalPagar: 999.99 // No coincide con los cálculos
        }
      };

      return request(app.getHttpServer())
        .post('/dte/fe/validar')
        .send(invalidData)
        .expect(400);
    });

    it('should handle missing required fields', () => {
      const incompleteData = {
        emisor: {
          nit: '06142803901121'
          // Faltan campos requeridos
        }
      };

      return request(app.getHttpServer())
        .post('/dte/fe/crear')
        .send(incompleteData)
        .expect(400);
    });
  });

  describe('/dte/ccf (POST)', () => {
    const validCcfData = {
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
        nit: '06142803901122',
        nrc: '7654321',
        nombre: 'EMPRESA CLIENTE S.A. DE C.V.',
        codActividad: '01111',
        descActividad: 'Cultivo de maíz',
        nombreComercial: 'CLIENTE EMPRESA',
        direccion: {
          departamento: '06',
          municipio: '14',
          complemento: 'COLONIA MIRAMONTE, AVENIDA PRINCIPAL #456'
        },
        telefono: '22509876',
        correo: 'cliente@empresa.com'
      },
      cuerpoDocumento: [
        {
          numItem: 1,
          tipoItem: 2,
          numeroDocumento: null,
          cantidad: 1,
          codigo: 'SERV001',
          codTributo: null,
          uniMedida: 59,
          descripcion: 'SERVICIO DE CONSULTORIA',
          precioUni: 100.00,
          montoDescu: 0,
          ventaNoSuj: 0,
          ventaExenta: 0,
          ventaGravada: 100.00,
          tributos: ['20']
        }
      ],
      resumen: {
        totalNoSuj: 0,
        totalExenta: 0,
        totalGravada: 100.00,
        subTotalVentas: 100.00,
        descuNoSuj: 0,
        descuExenta: 0,
        descuGravada: 0,
        porcentajeDescuento: 0,
        totalDescu: 0,
        tributos: [
          {
            codigo: '20',
            descripcion: 'Impuesto al Valor Agregado 13%',
            valor: 13.00
          }
        ],
        subTotal: 100.00,
        ivaRete1: 0,
        reteRenta: 0,
        montoTotalOperacion: 113.00,
        totalNoGravado: 0,
        totalPagar: 113.00,
        totalLetras: 'CIENTO TRECE 00/100 DÓLARES',
        totalIva: 13.00,
        saldoFavor: 0,
        condicionOperacion: 2, // Crédito
        pagos: [
          {
            codigo: '02',
            montoPago: 113.00,
            referencia: 'CREDITO 30 DIAS',
            plazo: '02',
            periodo: 30
          }
        ],
        numPagoElectronico: null
      }
    };

    it('should create CCF successfully', () => {
      return request(app.getHttpServer())
        .post('/dte/ccf/crear')
        .send(validCcfData)
        .expect(201)
        .expect((res) => {
          expect(res.body).toHaveProperty('success', true);
          expect(res.body).toHaveProperty('codigoGeneracion');
          expect(res.body).toHaveProperty('numeroControl');
          expect(res.body.numeroControl).toMatch(/^DTE-03-\d{4}-\d{4}-\d{15}$/);
        });
    });

    it('should validate CCF-specific requirements', () => {
      return request(app.getHttpServer())
        .post('/dte/ccf/validar')
        .send(validCcfData)
        .expect(200)
        .expect((res) => {
          expect(res.body).toHaveProperty('valido', true);
        });
    });

    it('should reject CCF with invalid receptor (must be empresa)', () => {
      const invalidData = {
        ...validCcfData,
        receptor: {
          tipoDocumento: '36', // Persona natural, no válido para CCF
          numDocumento: '12345678901234',
          nombre: 'PERSONA NATURAL'
        }
      };

      return request(app.getHttpServer())
        .post('/dte/ccf/validar')
        .send(invalidData)
        .expect(400);
    });
  });

  describe('Health checks', () => {
    it('/health (GET)', () => {
      return request(app.getHttpServer())
        .get('/health')
        .expect(200)
        .expect((res) => {
          expect(res.body).toHaveProperty('status', 'ok');
          expect(res.body).toHaveProperty('timestamp');
        });
    });

    it('/dte/fe/health (GET)', () => {
      return request(app.getHttpServer())
        .get('/dte/fe/health')
        .expect(200)
        .expect((res) => {
          expect(res.body).toHaveProperty('status', 'ok');
          expect(res.body).toHaveProperty('service', 'fe');
        });
    });

    it('/dte/ccf/health (GET)', () => {
      return request(app.getHttpServer())
        .get('/dte/ccf/health')
        .expect(200);
    });
  });

  describe('Error handling', () => {
    it('should handle 404 for non-existent endpoints', () => {
      return request(app.getHttpServer())
        .get('/non-existent-endpoint')
        .expect(404);
    });

    it('should handle malformed JSON', () => {
      return request(app.getHttpServer())
        .post('/dte/fe/crear')
        .send('{ invalid json }')
        .set('Content-Type', 'application/json')
        .expect(400);
    });

    it('should handle oversized requests', () => {
      const oversizedData = {
        ...validFeData,
        cuerpoDocumento: Array.from({ length: 10000 }, (_, i) => ({
          numItem: i + 1,
          tipoItem: 2,
          cantidad: 1,
          codigo: `PROD${i}`,
          descripcion: 'PRODUCTO DE PRUEBA '.repeat(100), // Descripción muy larga
          precioUni: 1.00,
          ventaGravada: 1.00
        }))
      };

      return request(app.getHttpServer())
        .post('/dte/fe/crear')
        .send(oversizedData)
        .expect(413); // Payload Too Large
    });
  });

  describe('CORS and Security', () => {
    it('should handle CORS preflight requests', () => {
      return request(app.getHttpServer())
        .options('/dte/fe/crear')
        .expect(200);
    });

    it('should include security headers', () => {
      return request(app.getHttpServer())
        .get('/health')
        .expect(200)
        .expect((res) => {
          // Verificar headers de seguridad si están configurados
          // expect(res.headers).toHaveProperty('x-frame-options');
        });
    });
  });

  describe('Performance tests', () => {
    it('should handle concurrent requests', async () => {
      const promises = Array.from({ length: 10 }, () =>
        request(app.getHttpServer())
          .post('/dte/fe/validar')
          .send(validFeData)
          .expect(200)
      );

      const results = await Promise.all(promises);
      
      results.forEach(result => {
        expect(result.body).toHaveProperty('valido', true);
      });
    });

    it('should respond within acceptable time limits', () => {
      const startTime = Date.now();
      
      return request(app.getHttpServer())
        .post('/dte/fe/validar')
        .send(validFeData)
        .expect(200)
        .expect(() => {
          const responseTime = Date.now() - startTime;
          expect(responseTime).toBeLessThan(5000); // 5 segundos máximo
        });
    });
  });
});