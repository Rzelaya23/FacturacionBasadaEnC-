import { Injectable } from '@nestjs/common';
import { v4 as uuidv4 } from 'uuid';

@Injectable()
export class CodigoGeneracionService {

  /**
   * Generar código de generación único (UUID v4 en mayúsculas)
   * Formato: A1B2C3D4-E5F6-7890-ABCD-123456789012
   */
  generateCodigoGeneracion(): string {
    return uuidv4().toUpperCase();
  }

  /**
   * Generar número de control según formato del MH
   * Formato: DTE-{tipoDte}-{establecimiento}-{puntoVenta}-{correlativo}
   * Ejemplo: DTE-01-0001-0001-000000000000001
   */
  generateNumeroControl(
    tipoDte: string,
    establecimiento: string = '0001',
    puntoVenta: string = '0001',
    correlativo?: number
  ): string {
    // Si no se proporciona correlativo, generar uno basado en timestamp
    const correlativoFinal = correlativo || this.generateCorrelativo();
    
    // Formatear correlativo a 15 dígitos con ceros a la izquierda
    const correlativoFormateado = correlativoFinal.toString().padStart(15, '0');
    
    // Formatear establecimiento y punto de venta a 4 dígitos
    const establecimientoFormateado = establecimiento.padStart(4, '0');
    const puntoVentaFormateado = puntoVenta.padStart(4, '0');
    
    return `DTE-${tipoDte}-${establecimientoFormateado}-${puntoVentaFormateado}-${correlativoFormateado}`;
  }

  /**
   * Generar correlativo basado en timestamp + random
   * Esto asegura unicidad temporal
   */
  private generateCorrelativo(): number {
    const timestamp = Date.now();
    const random = Math.floor(Math.random() * 1000);
    return timestamp * 1000 + random;
  }

  /**
   * Validar formato de código de generación
   */
  validateCodigoGeneracion(codigo: string): boolean {
    const uuidPattern = /^[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}$/;
    return uuidPattern.test(codigo);
  }

  /**
   * Validar formato de número de control
   */
  validateNumeroControl(numeroControl: string): boolean {
    const pattern = /^DTE-\d{2}-\d{4}-\d{4}-\d{15}$/;
    return pattern.test(numeroControl);
  }

  /**
   * Extraer información del número de control
   */
  parseNumeroControl(numeroControl: string): {
    tipoDte: string;
    establecimiento: string;
    puntoVenta: string;
    correlativo: string;
  } | null {
    const match = numeroControl.match(/^DTE-(\d{2})-(\d{4})-(\d{4})-(\d{15})$/);
    
    if (!match) {
      return null;
    }

    return {
      tipoDte: match[1],
      establecimiento: match[2],
      puntoVenta: match[3],
      correlativo: match[4]
    };
  }

  /**
   * Generar fecha y hora de emisión en formato requerido
   */
  generateFechaHoraEmision(): { fecEmi: string; horEmi: string } {
    const now = new Date();
    
    // Fecha en formato YYYY-MM-DD
    const fecEmi = now.toISOString().split('T')[0];
    
    // Hora en formato HH:mm:ss
    const horEmi = now.toTimeString().split(' ')[0];
    
    return { fecEmi, horEmi };
  }

  /**
   * Generar identificación completa para DTE
   */
  generateIdentificacionCompleta(
    tipoDte: string,
    ambiente: string = '00',
    tipoModelo: number = 1,
    tipoOperacion: number = 1,
    establecimiento?: string,
    puntoVenta?: string,
    correlativo?: number
  ): {
    version: number;
    ambiente: string;
    tipoDte: string;
    numeroControl: string;
    codigoGeneracion: string;
    tipoModelo: number;
    tipoOperacion: number;
    fecEmi: string;
    horEmi: string;
    tipoMoneda: string;
  } {
    const { fecEmi, horEmi } = this.generateFechaHoraEmision();
    
    return {
      version: 1,
      ambiente,
      tipoDte,
      numeroControl: this.generateNumeroControl(tipoDte, establecimiento, puntoVenta, correlativo),
      codigoGeneracion: this.generateCodigoGeneracion(),
      tipoModelo,
      tipoOperacion,
      fecEmi,
      horEmi,
      tipoMoneda: 'USD'
    };
  }
}