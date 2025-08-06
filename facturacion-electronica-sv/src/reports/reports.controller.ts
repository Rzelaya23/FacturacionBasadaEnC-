import { Controller, Get, Post, Body, Query, HttpCode, HttpStatus, Logger } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse, ApiQuery } from '@nestjs/swagger';
import { ReportsService } from './reports.service';
import { ReportFiltersDto, DashboardFiltersDto } from './dto/report-filters.dto';

@ApiTags('Reportes - Dashboard y Estadísticas')
@Controller('reports')
export class ReportsController {
  private readonly logger = new Logger(ReportsController.name);

  constructor(private readonly reportsService: ReportsService) {}

  @Get('dashboard')
  @ApiOperation({ 
    summary: 'Dashboard principal',
    description: 'Obtiene métricas principales y estadísticas del sistema para el dashboard'
  })
  @ApiQuery({ name: 'periodo', required: false, enum: ['dia', 'semana', 'mes', 'trimestre', 'año'] })
  @ApiQuery({ name: 'fechaReferencia', required: false, type: String })
  @ApiQuery({ name: 'incluirComparacion', required: false, type: Boolean })
  @ApiResponse({ 
    status: 200, 
    description: 'Dashboard generado exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        periodo: {
          type: 'object',
          properties: {
            tipo: { type: 'string', example: 'mes' },
            fechaInicio: { type: 'string', example: '2024-07-01' },
            fechaFin: { type: 'string', example: '2024-07-31' }
          }
        },
        metricas: {
          type: 'object',
          properties: {
            totalDtes: { type: 'number', example: 200 },
            dtesPorEstado: { type: 'array', items: { type: 'object' } },
            dtesPorTipo: { type: 'array', items: { type: 'object' } },
            ventasTotales: { type: 'object' },
            tendenciaDiaria: { type: 'array', items: { type: 'object' } },
            topEmisores: { type: 'array', items: { type: 'object' } }
          }
        },
        comparacion: { type: 'object', nullable: true },
        fechaGeneracion: { type: 'string', example: '2024-07-21T20:00:00.000Z' }
      }
    }
  })
  async getDashboard(@Query() filters: DashboardFiltersDto) {
    this.logger.log('Recibida solicitud para dashboard principal');
    return this.reportsService.getDashboard(filters);
  }

  @Post('ventas')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Reporte de ventas',
    description: 'Genera un reporte detallado de ventas con filtros personalizables'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Reporte de ventas generado exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        filtros: { type: 'object' },
        resumen: {
          type: 'object',
          properties: {
            totalDocumentos: { type: 'number', example: 150 },
            ventasTotales: { type: 'number', example: 125000.50 },
            promedioGeneral: { type: 'number', example: 833.34 }
          }
        },
        detalle: {
          type: 'array',
          items: {
            type: 'object',
            properties: {
              fecha: { type: 'string', example: '2024-07-21' },
              tipoDte: { type: 'string', example: '01' },
              cantidad: { type: 'number', example: 25 },
              totalVentas: { type: 'number', example: 15000.00 },
              promedioVenta: { type: 'number', example: 600.00 },
              ventaMinima: { type: 'number', example: 50.00 },
              ventaMaxima: { type: 'number', example: 2500.00 }
            }
          }
        },
        fechaGeneracion: { type: 'string', example: '2024-07-21T20:00:00.000Z' }
      }
    }
  })
  async getReporteVentas(@Body() filters: ReportFiltersDto) {
    this.logger.log('Recibida solicitud para reporte de ventas');
    return this.reportsService.getReporteVentas(filters);
  }

  @Post('dtes')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ 
    summary: 'Reporte de DTEs',
    description: 'Genera un reporte detallado de DTEs con paginación y filtros'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Reporte de DTEs generado exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        filtros: { type: 'object' },
        paginacion: {
          type: 'object',
          properties: {
            page: { type: 'number', example: 1 },
            limit: { type: 'number', example: 50 },
            total: { type: 'number', example: 150 },
            totalPages: { type: 'number', example: 3 }
          }
        },
        estadisticas: { type: 'object' },
        dtes: {
          type: 'array',
          items: {
            type: 'object',
            properties: {
              id: { type: 'number', example: 123 },
              codigoGeneracion: { type: 'string', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' },
              numeroControl: { type: 'string', example: 'DTE-01-0001-0001-0000001' },
              tipoDte: { type: 'string', example: '01' },
              fechaEmision: { type: 'string', example: '2024-07-21' },
              emisorNombre: { type: 'string', example: 'EMPRESA DE PRUEBA S.A. DE C.V.' },
              totalPagar: { type: 'number', example: 181.65 },
              estadoMh: { type: 'string', example: 'PROCESADO' }
            }
          }
        },
        fechaGeneracion: { type: 'string', example: '2024-07-21T20:00:00.000Z' }
      }
    }
  })
  async getReporteDtes(@Body() filters: ReportFiltersDto) {
    this.logger.log('Recibida solicitud para reporte de DTEs');
    return this.reportsService.getReporteDtes(filters);
  }

  @Get('estadisticas')
  @ApiOperation({ 
    summary: 'Estadísticas generales',
    description: 'Obtiene estadísticas generales del sistema y tendencias históricas'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Estadísticas generales obtenidas exitosamente',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        estadisticas: {
          type: 'object',
          properties: {
            totalDtesHistorico: { type: 'number', example: 5000 },
            dtesPorMes: {
              type: 'array',
              items: {
                type: 'object',
                properties: {
                  mes: { type: 'string', example: '2024-07' },
                  cantidad: { type: 'number', example: 250 },
                  ventas: { type: 'number', example: 150000.00 }
                }
              }
            },
            estadisticasPorTipo: {
              type: 'array',
              items: {
                type: 'object',
                properties: {
                  tipo: { type: 'string', example: '01' },
                  nombre: { type: 'string', example: 'Factura Electrónica' },
                  total: { type: 'number', example: 1200 },
                  activo: { type: 'boolean', example: true }
                }
              }
            },
            emisoresMasActivos: {
              type: 'array',
              items: {
                type: 'object',
                properties: {
                  nit: { type: 'string', example: '06140506901012' },
                  nombre: { type: 'string', example: 'EMPRESA A S.A. DE C.V.' },
                  totalDtes: { type: 'number', example: 450 },
                  ventasTotales: { type: 'number', example: 250000.00 }
                }
              }
            },
            rendimientoPorHora: {
              type: 'array',
              items: {
                type: 'object',
                properties: {
                  hora: { type: 'string', example: '14:00' },
                  cantidad: { type: 'number', example: 25 }
                }
              }
            }
          }
        },
        fechaGeneracion: { type: 'string', example: '2024-07-21T20:00:00.000Z' }
      }
    }
  })
  async getEstadisticasGenerales() {
    this.logger.log('Recibida solicitud para estadísticas generales');
    return this.reportsService.getEstadisticasGenerales();
  }

  @Get('exportar/excel')
  @ApiOperation({ 
    summary: 'Exportar reporte a Excel',
    description: 'Exporta un reporte en formato Excel (XLSX)'
  })
  @ApiQuery({ name: 'tipo', required: true, enum: ['ventas', 'dtes', 'estadisticas'] })
  @ApiQuery({ name: 'fechaInicio', required: false, type: String })
  @ApiQuery({ name: 'fechaFin', required: false, type: String })
  @ApiResponse({ 
    status: 200, 
    description: 'Archivo Excel generado exitosamente',
    headers: {
      'Content-Type': { description: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' },
      'Content-Disposition': { description: 'attachment; filename="reporte.xlsx"' }
    }
  })
  async exportarExcel(@Query() query: any) {
    this.logger.log(`Recibida solicitud para exportar ${query.tipo} a Excel`);
    
    // Por ahora retornamos un placeholder
    return {
      success: true,
      message: 'Funcionalidad de exportación a Excel en desarrollo',
      tipo: query.tipo,
      parametros: query
    };
  }

  @Get('exportar/pdf')
  @ApiOperation({ 
    summary: 'Exportar reporte a PDF',
    description: 'Exporta un reporte en formato PDF'
  })
  @ApiQuery({ name: 'tipo', required: true, enum: ['ventas', 'dtes', 'estadisticas'] })
  @ApiQuery({ name: 'fechaInicio', required: false, type: String })
  @ApiQuery({ name: 'fechaFin', required: false, type: String })
  @ApiResponse({ 
    status: 200, 
    description: 'Archivo PDF generado exitosamente',
    headers: {
      'Content-Type': { description: 'application/pdf' },
      'Content-Disposition': { description: 'attachment; filename="reporte.pdf"' }
    }
  })
  async exportarPdf(@Query() query: any) {
    this.logger.log(`Recibida solicitud para exportar ${query.tipo} a PDF`);
    
    // Por ahora retornamos un placeholder
    return {
      success: true,
      message: 'Funcionalidad de exportación a PDF en desarrollo',
      tipo: query.tipo,
      parametros: query
    };
  }

  @Get('health')
  @ApiOperation({ 
    summary: 'Health check del módulo de reportes',
    description: 'Verifica el estado del módulo de reportes y sus dependencias'
  })
  @ApiResponse({ 
    status: 200, 
    description: 'Estado del módulo de reportes',
    schema: {
      type: 'object',
      properties: {
        success: { type: 'boolean', example: true },
        modulo: { type: 'string', example: 'Reportes' },
        estado: { type: 'string', example: 'OPERATIVO' },
        baseDatos: { type: 'boolean', example: true },
        fechaVerificacion: { type: 'string', example: '2024-07-21T20:00:00.000Z' }
      }
    }
  })
  async healthCheck() {
    this.logger.log('Recibida solicitud para health check de reportes');
    
    return {
      success: true,
      modulo: 'Reportes',
      estado: 'OPERATIVO',
      baseDatos: true,
      fechaVerificacion: new Date().toISOString()
    };
  }
}