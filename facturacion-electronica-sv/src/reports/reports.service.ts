import { Injectable, Logger, BadRequestException, InternalServerErrorException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository, Between, In } from 'typeorm';
import { ReportFiltersDto, DashboardFiltersDto } from './dto/report-filters.dto';
import { Dte } from '../common/entities/dte.entity';

@Injectable()
export class ReportsService {
  private readonly logger = new Logger(ReportsService.name);

  constructor(
    @InjectRepository(Dte)
    private readonly dteRepository: Repository<Dte>,
  ) {}

  async getDashboard(filters: DashboardFiltersDto) {
    try {
      this.logger.log('Generando dashboard principal');

      const fechaReferencia = filters.fechaReferencia ? new Date(filters.fechaReferencia) : new Date();
      const periodo = filters.periodo || 'mes';

      // Calcular fechas del período
      const { fechaInicio, fechaFin } = this.calcularPeriodo(fechaReferencia, periodo);

      // Obtener métricas principales
      const [
        totalDtes,
        dtesPorEstado,
        dtesPorTipo,
        ventasTotales,
        tendenciaDiaria,
        topEmisores
      ] = await Promise.all([
        this.getTotalDtes(fechaInicio, fechaFin),
        this.getDtesPorEstado(fechaInicio, fechaFin),
        this.getDtesPorTipo(fechaInicio, fechaFin),
        this.getVentasTotales(fechaInicio, fechaFin),
        this.getTendenciaDiaria(fechaInicio, fechaFin),
        this.getTopEmisores(fechaInicio, fechaFin, 10)
      ]);

      // Comparación con período anterior si se solicita
      let comparacion = null;
      if (filters.incluirComparacion) {
        comparacion = await this.getComparacionPeriodoAnterior(fechaInicio, fechaFin, periodo);
      }

      return {
        success: true,
        periodo: {
          tipo: periodo,
          fechaInicio: fechaInicio.toISOString().split('T')[0],
          fechaFin: fechaFin.toISOString().split('T')[0]
        },
        metricas: {
          totalDtes,
          dtesPorEstado,
          dtesPorTipo,
          ventasTotales,
          tendenciaDiaria,
          topEmisores
        },
        comparacion,
        fechaGeneracion: new Date().toISOString()
      };

    } catch (error) {
      this.logger.error('Error generando dashboard', error.stack);
      throw new InternalServerErrorException('Error al generar dashboard');
    }
  }

  async getReporteVentas(filters: ReportFiltersDto) {
    try {
      this.logger.log('Generando reporte de ventas');

      const whereConditions = this.buildWhereConditions(filters);

      // Consulta principal de ventas
      const ventasQuery = this.dteRepository.createQueryBuilder('dte')
        .select([
          'DATE(dte.fechaEmision) as fecha',
          'dte.tipoDte as tipoDte',
          'COUNT(*) as cantidad',
          'SUM(dte.totalPagar) as totalVentas',
          'AVG(dte.totalPagar) as promedioVenta',
          'MIN(dte.totalPagar) as ventaMinima',
          'MAX(dte.totalPagar) as ventaMaxima'
        ])
        .where(whereConditions.where, whereConditions.parameters)
        .groupBy('DATE(dte.fechaEmision), dte.tipoDte')
        .orderBy('fecha', 'DESC')
        .addOrderBy('tipoDte', 'ASC');

      const ventas = await ventasQuery.getRawMany();

      // Resumen general
      const resumenQuery = this.dteRepository.createQueryBuilder('dte')
        .select([
          'COUNT(*) as totalDocumentos',
          'SUM(dte.totalPagar) as ventasTotales',
          'AVG(dte.totalPagar) as promedioGeneral'
        ])
        .where(whereConditions.where, whereConditions.parameters);

      const resumen = await resumenQuery.getRawOne();

      return {
        success: true,
        filtros: filters,
        resumen: {
          totalDocumentos: parseInt(resumen.totalDocumentos) || 0,
          ventasTotales: parseFloat(resumen.ventasTotales) || 0,
          promedioGeneral: parseFloat(resumen.promedioGeneral) || 0
        },
        detalle: ventas.map(v => ({
          fecha: v.fecha,
          tipoDte: v.tipoDte,
          cantidad: parseInt(v.cantidad),
          totalVentas: parseFloat(v.totalVentas),
          promedioVenta: parseFloat(v.promedioVenta),
          ventaMinima: parseFloat(v.ventaMinima),
          ventaMaxima: parseFloat(v.ventaMaxima)
        })),
        fechaGeneracion: new Date().toISOString()
      };

    } catch (error) {
      this.logger.error('Error generando reporte de ventas', error.stack);
      throw new InternalServerErrorException('Error al generar reporte de ventas');
    }
  }

  async getReporteDtes(filters: ReportFiltersDto) {
    try {
      this.logger.log('Generando reporte de DTEs');

      const whereConditions = this.buildWhereConditions(filters);
      const page = filters.page || 1;
      const limit = filters.limit || 50;
      const offset = (page - 1) * limit;

      // Consulta principal con paginación
      const [dtes, total] = await this.dteRepository.findAndCount({
        where: whereConditions.whereObject,
        order: { fechaEmision: 'DESC', horaEmision: 'DESC' },
        skip: offset,
        take: limit
      });

      // Estadísticas del conjunto filtrado
      const estadisticas = await this.getEstadisticasFiltradas(whereConditions);

      return {
        success: true,
        filtros: filters,
        paginacion: {
          page,
          limit,
          total,
          totalPages: Math.ceil(total / limit)
        },
        estadisticas,
        dtes: dtes.map(dte => ({
          id: dte.id,
          codigoGeneracion: dte.codigoGeneracion,
          numeroControl: dte.numeroControl,
          tipoDte: dte.tipoDte,
          fechaEmision: dte.fechaEmision,
          horaEmision: dte.horaEmision,
          emisorNit: dte.emisorNit,
          emisorNombre: dte.emisorNombre,
          receptorNombre: dte.receptorNombre,
          totalPagar: dte.totalPagar,
          estadoMh: dte.estadoMh,
          observaciones: dte.observaciones
        })),
        fechaGeneracion: new Date().toISOString()
      };

    } catch (error) {
      this.logger.error('Error generando reporte de DTEs', error.stack);
      throw new InternalServerErrorException('Error al generar reporte de DTEs');
    }
  }

  async getEstadisticasGenerales() {
    try {
      this.logger.log('Generando estadísticas generales');

      const [
        totalDtesHistorico,
        dtesPorMes,
        estadisticasPorTipo,
        emisoresMasActivos,
        rendimientoPorHora
      ] = await Promise.all([
        this.getTotalDtesHistorico(),
        this.getDtesPorMes(12), // Últimos 12 meses
        this.getEstadisticasPorTipo(),
        this.getEmisoresMasActivos(20),
        this.getRendimientoPorHora()
      ]);

      return {
        success: true,
        estadisticas: {
          totalDtesHistorico,
          dtesPorMes,
          estadisticasPorTipo,
          emisoresMasActivos,
          rendimientoPorHora
        },
        fechaGeneracion: new Date().toISOString()
      };

    } catch (error) {
      this.logger.error('Error generando estadísticas generales', error.stack);
      throw new InternalServerErrorException('Error al generar estadísticas generales');
    }
  }

  // ==================== MÉTODOS PRIVADOS ====================

  private calcularPeriodo(fechaReferencia: Date, periodo: string) {
    const fechaFin = new Date(fechaReferencia);
    const fechaInicio = new Date(fechaReferencia);

    switch (periodo) {
      case 'dia':
        fechaInicio.setHours(0, 0, 0, 0);
        fechaFin.setHours(23, 59, 59, 999);
        break;
      case 'semana':
        const diaSemana = fechaInicio.getDay();
        fechaInicio.setDate(fechaInicio.getDate() - diaSemana);
        fechaFin.setDate(fechaInicio.getDate() + 6);
        break;
      case 'mes':
        fechaInicio.setDate(1);
        fechaFin.setMonth(fechaFin.getMonth() + 1, 0);
        break;
      case 'trimestre':
        const mesActual = fechaInicio.getMonth();
        const inicioTrimestre = Math.floor(mesActual / 3) * 3;
        fechaInicio.setMonth(inicioTrimestre, 1);
        fechaFin.setMonth(inicioTrimestre + 3, 0);
        break;
      case 'año':
        fechaInicio.setMonth(0, 1);
        fechaFin.setMonth(11, 31);
        break;
    }

    return { fechaInicio, fechaFin };
  }

  private buildWhereConditions(filters: ReportFiltersDto) {
    const conditions = [];
    const parameters = {};
    const whereObject = {};

    if (filters.fechaInicio && filters.fechaFin) {
      conditions.push('dte.fechaEmision BETWEEN :fechaInicio AND :fechaFin');
      parameters['fechaInicio'] = filters.fechaInicio;
      parameters['fechaFin'] = filters.fechaFin;
      whereObject['fechaEmision'] = Between(new Date(filters.fechaInicio), new Date(filters.fechaFin));
    }

    if (filters.tiposDte && filters.tiposDte.length > 0) {
      conditions.push('dte.tipoDte IN (:...tiposDte)');
      parameters['tiposDte'] = filters.tiposDte;
      whereObject['tipoDte'] = In(filters.tiposDte);
    }

    if (filters.estados && filters.estados.length > 0) {
      conditions.push('dte.estadoMh IN (:...estados)');
      parameters['estados'] = filters.estados;
      whereObject['estadoMh'] = In(filters.estados);
    }

    if (filters.emisorNit) {
      conditions.push('dte.emisorNit = :emisorNit');
      parameters['emisorNit'] = filters.emisorNit;
      whereObject['emisorNit'] = filters.emisorNit;
    }

    if (filters.montoMinimo !== undefined) {
      conditions.push('dte.totalPagar >= :montoMinimo');
      parameters['montoMinimo'] = filters.montoMinimo;
    }

    if (filters.montoMaximo !== undefined) {
      conditions.push('dte.totalPagar <= :montoMaximo');
      parameters['montoMaximo'] = filters.montoMaximo;
    }

    return {
      where: conditions.length > 0 ? conditions.join(' AND ') : '1=1',
      parameters,
      whereObject
    };
  }

  private async getTotalDtes(fechaInicio: Date, fechaFin: Date): Promise<number> {
    try {
      const count = await this.dteRepository.count({
        where: {
          fechaEmision: Between(fechaInicio, fechaFin)
        }
      });
      return count;
    } catch (error) {
      this.logger.error('Error obteniendo total de DTEs', error.stack);
      return 0;
    }
  }

  private async getDtesPorEstado(fechaInicio: Date, fechaFin: Date): Promise<any[]> {
    try {
      // Simulamos datos por ahora
      return [
        { estado: 'PROCESADO', cantidad: 150, porcentaje: 75.0 },
        { estado: 'RECHAZADO', cantidad: 30, porcentaje: 15.0 },
        { estado: 'PENDIENTE', cantidad: 20, porcentaje: 10.0 }
      ];
    } catch (error) {
      this.logger.error('Error obteniendo DTEs por estado', error.stack);
      return [];
    }
  }

  private async getDtesPorTipo(fechaInicio: Date, fechaFin: Date): Promise<any[]> {
    try {
      // Simulamos datos por ahora
      return [
        { tipo: '01', nombre: 'Factura Electrónica', cantidad: 80, porcentaje: 40.0 },
        { tipo: '03', nombre: 'Comprobante Crédito Fiscal', cantidad: 60, porcentaje: 30.0 },
        { tipo: '05', nombre: 'Nota de Crédito', cantidad: 30, porcentaje: 15.0 },
        { tipo: '06', nombre: 'Nota de Débito', cantidad: 20, porcentaje: 10.0 },
        { tipo: '14', nombre: 'Factura Sujeto Excluido', cantidad: 10, porcentaje: 5.0 }
      ];
    } catch (error) {
      this.logger.error('Error obteniendo DTEs por tipo', error.stack);
      return [];
    }
  }

  private async getVentasTotales(fechaInicio: Date, fechaFin: Date): Promise<any> {
    try {
      // Simulamos datos por ahora
      return {
        total: 125000.50,
        promedio: 625.00,
        maximo: 5000.00,
        minimo: 15.75
      };
    } catch (error) {
      this.logger.error('Error obteniendo ventas totales', error.stack);
      return { total: 0, promedio: 0, maximo: 0, minimo: 0 };
    }
  }

  private async getTendenciaDiaria(fechaInicio: Date, fechaFin: Date): Promise<any[]> {
    try {
      // Simulamos datos por ahora
      const dias = [];
      const fechaActual = new Date(fechaInicio);
      
      while (fechaActual <= fechaFin) {
        dias.push({
          fecha: fechaActual.toISOString().split('T')[0],
          cantidad: Math.floor(Math.random() * 50) + 10,
          ventas: Math.floor(Math.random() * 10000) + 1000
        });
        fechaActual.setDate(fechaActual.getDate() + 1);
      }
      
      return dias;
    } catch (error) {
      this.logger.error('Error obteniendo tendencia diaria', error.stack);
      return [];
    }
  }

  private async getTopEmisores(fechaInicio: Date, fechaFin: Date, limit: number): Promise<any[]> {
    try {
      // Simulamos datos por ahora
      return [
        { nit: '06140506901012', nombre: 'EMPRESA A S.A. DE C.V.', cantidad: 45, ventas: 25000.00 },
        { nit: '06140506901013', nombre: 'EMPRESA B S.A. DE C.V.', cantidad: 38, ventas: 22000.00 },
        { nit: '06140506901014', nombre: 'EMPRESA C S.A. DE C.V.', cantidad: 32, ventas: 18000.00 }
      ];
    } catch (error) {
      this.logger.error('Error obteniendo top emisores', error.stack);
      return [];
    }
  }

  private async getComparacionPeriodoAnterior(fechaInicio: Date, fechaFin: Date, periodo: string): Promise<any> {
    try {
      // Simulamos comparación por ahora
      return {
        totalDtes: { actual: 200, anterior: 180, cambio: 11.1 },
        ventas: { actual: 125000.50, anterior: 115000.00, cambio: 8.7 },
        promedio: { actual: 625.00, anterior: 638.89, cambio: -2.2 }
      };
    } catch (error) {
      this.logger.error('Error obteniendo comparación', error.stack);
      return null;
    }
  }

  private async getEstadisticasFiltradas(whereConditions: any): Promise<any> {
    try {
      // Simulamos estadísticas por ahora
      return {
        totalDocumentos: 150,
        ventasTotales: 85000.00,
        promedioVenta: 566.67,
        dtesPorEstado: {
          PROCESADO: 120,
          RECHAZADO: 20,
          PENDIENTE: 10
        }
      };
    } catch (error) {
      this.logger.error('Error obteniendo estadísticas filtradas', error.stack);
      return {};
    }
  }

  private async getTotalDtesHistorico(): Promise<number> {
    try {
      const count = await this.dteRepository.count();
      return count;
    } catch (error) {
      this.logger.error('Error obteniendo total histórico', error.stack);
      return 0;
    }
  }

  private async getDtesPorMes(meses: number): Promise<any[]> {
    try {
      // Simulamos datos por ahora
      const datos = [];
      const fechaActual = new Date();
      
      for (let i = meses - 1; i >= 0; i--) {
        const fecha = new Date(fechaActual.getFullYear(), fechaActual.getMonth() - i, 1);
        datos.push({
          mes: fecha.toISOString().substring(0, 7),
          cantidad: Math.floor(Math.random() * 500) + 100,
          ventas: Math.floor(Math.random() * 100000) + 20000
        });
      }
      
      return datos;
    } catch (error) {
      this.logger.error('Error obteniendo DTEs por mes', error.stack);
      return [];
    }
  }

  private async getEstadisticasPorTipo(): Promise<any[]> {
    try {
      // Simulamos datos por ahora
      return [
        { tipo: '01', nombre: 'Factura Electrónica', total: 1200, activo: true },
        { tipo: '03', nombre: 'Comprobante Crédito Fiscal', total: 800, activo: true },
        { tipo: '05', nombre: 'Nota de Crédito', total: 150, activo: true },
        { tipo: '06', nombre: 'Nota de Débito', total: 100, activo: true },
        { tipo: '14', nombre: 'Factura Sujeto Excluido', total: 75, activo: true }
      ];
    } catch (error) {
      this.logger.error('Error obteniendo estadísticas por tipo', error.stack);
      return [];
    }
  }

  private async getEmisoresMasActivos(limit: number): Promise<any[]> {
    try {
      // Simulamos datos por ahora
      return [
        { nit: '06140506901012', nombre: 'EMPRESA A S.A. DE C.V.', totalDtes: 450, ventasTotales: 250000.00 },
        { nit: '06140506901013', nombre: 'EMPRESA B S.A. DE C.V.', totalDtes: 380, ventasTotales: 220000.00 },
        { nit: '06140506901014', nombre: 'EMPRESA C S.A. DE C.V.', totalDtes: 320, ventasTotales: 180000.00 }
      ];
    } catch (error) {
      this.logger.error('Error obteniendo emisores más activos', error.stack);
      return [];
    }
  }

  private async getRendimientoPorHora(): Promise<any[]> {
    try {
      // Simulamos datos por ahora
      const horas = [];
      for (let i = 0; i < 24; i++) {
        horas.push({
          hora: i.toString().padStart(2, '0') + ':00',
          cantidad: Math.floor(Math.random() * 30) + 5
        });
      }
      return horas;
    } catch (error) {
      this.logger.error('Error obteniendo rendimiento por hora', error.stack);
      return [];
    }
  }
}