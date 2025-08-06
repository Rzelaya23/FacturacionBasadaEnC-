import { ApiProperty } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsString, IsNumber, IsOptional, IsArray, ValidateNested, IsDecimal, Min, Max } from 'class-validator';
import { DireccionDto } from '../../../common/dto/direccion.dto';
import { IdentificacionDto } from '../../../common/dto/identificacion.dto';
import { EmisorDto } from '../../../common/dto/emisor.dto';
import { ReceptorDto } from '../../../common/dto/receptor.dto';

export class CuerpoDocumentoNrDto {
  @ApiProperty({ description: 'Número de ítem', example: 1 })
  @IsNumber()
  @Min(1)
  numItem: number;

  @ApiProperty({ description: 'Tipo de ítem', example: 1 })
  @IsNumber()
  @Min(1)
  @Max(4)
  tipoItem: number;

  @ApiProperty({ description: 'Número de documento relacionado', example: 'DOC-001', required: false })
  @IsOptional()
  @IsString()
  numeroDocumento?: string;

  @ApiProperty({ description: 'Cantidad', example: 10.5 })
  @IsNumber({ maxDecimalPlaces: 8 })
  @Min(0)
  cantidad: number;

  @ApiProperty({ description: 'Código del producto/servicio', example: 'PROD001', required: false })
  @IsOptional()
  @IsString()
  codigo?: string;

  @ApiProperty({ description: 'Código de tributo', required: false })
  @IsOptional()
  codTributo?: any;

  @ApiProperty({ description: 'Unidad de medida', example: 99 })
  @IsNumber()
  @Min(1)
  uniMedida: number;

  @ApiProperty({ description: 'Descripción del producto/servicio', example: 'Producto de ejemplo' })
  @IsString()
  descripcion: string;

  @ApiProperty({ description: 'Precio unitario', example: 15.75 })
  @IsNumber({ maxDecimalPlaces: 4 })
  @Min(0)
  precioUni: number;

  @ApiProperty({ description: 'Monto de descuento', example: 2.50, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  montoDescu?: number;

  @ApiProperty({ description: 'Venta no sujeta', example: 0.00, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  ventaNoSuj?: number;

  @ApiProperty({ description: 'Venta exenta', example: 0.00, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  ventaExenta?: number;

  @ApiProperty({ description: 'Venta gravada', example: 163.25 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  ventaGravada: number;

  @ApiProperty({ description: 'Tributos aplicables', type: [String], required: false })
  @IsOptional()
  @IsArray()
  @IsString({ each: true })
  tributos?: string[];
}

export class ResumenNrDto {
  @ApiProperty({ description: 'Total de operaciones no sujetas', example: 0.00, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalNoSuj?: number;

  @ApiProperty({ description: 'Total de operaciones exentas', example: 0.00, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalExenta?: number;

  @ApiProperty({ description: 'Total de operaciones gravadas', example: 163.25 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalGravada: number;

  @ApiProperty({ description: 'Sub total (suma de operaciones)', example: 163.25 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  subTotal: number;

  @ApiProperty({ description: 'Total de descuentos', example: 2.50, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalDescu?: number;

  @ApiProperty({ description: 'Sub total después de descuentos', example: 160.75 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  subTotalVentas: number;

  @ApiProperty({ description: 'Total de IVA', example: 20.90 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalIva: number;

  @ApiProperty({ description: 'IVA retenido', example: 0.00, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  ivaRete1?: number;

  @ApiProperty({ description: 'Retención de renta', example: 0.00, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  reteRenta?: number;

  @ApiProperty({ description: 'Monto total de la operación', example: 181.65 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  montoTotalOperacion: number;

  @ApiProperty({ description: 'Total en letras', example: 'CIENTO OCHENTA Y UN DÓLARES CON 65/100' })
  @IsString()
  totalLetras: string;

  @ApiProperty({ description: 'Condición de operación', example: 1 })
  @IsNumber()
  @Min(1)
  @Max(3)
  condicionOperacion: number;

  @ApiProperty({ description: 'Formas de pago', required: false })
  @IsOptional()
  pagos?: any;

  @ApiProperty({ description: 'Número de percepción', required: false })
  @IsOptional()
  @IsString()
  numPagoElectronico?: string;

  @ApiProperty({ description: 'Observaciones', required: false })
  @IsOptional()
  @IsString()
  observaciones?: string;
}

export class ExtensionNrDto {
  @ApiProperty({ description: 'Nombre del entregador', example: 'Juan Pérez', required: false })
  @IsOptional()
  @IsString()
  nombEntrega?: string;

  @ApiProperty({ description: 'Documento del entregador', example: '12345678-9', required: false })
  @IsOptional()
  @IsString()
  docuEntrega?: string;

  @ApiProperty({ description: 'Nombre del receptor', example: 'María García', required: false })
  @IsOptional()
  @IsString()
  nombRecibe?: string;

  @ApiProperty({ description: 'Documento del receptor', example: '98765432-1', required: false })
  @IsOptional()
  @IsString()
  docuRecibe?: string;

  @ApiProperty({ description: 'Observaciones adicionales', required: false })
  @IsOptional()
  @IsString()
  observaciones?: string;

  @ApiProperty({ description: 'Placas del vehículo', example: 'P123-456', required: false })
  @IsOptional()
  @IsString()
  placaVehiculo?: string;
}

export class CreateNrDto {
  @ApiProperty({ description: 'Datos de identificación del DTE', type: IdentificacionDto })
  @ValidateNested()
  @Type(() => IdentificacionDto)
  identificacion: IdentificacionDto;

  @ApiProperty({ description: 'Documento relacionado', required: false })
  @IsOptional()
  documentoRelacionado?: any;

  @ApiProperty({ description: 'Datos del emisor', type: EmisorDto })
  @ValidateNested()
  @Type(() => EmisorDto)
  emisor: EmisorDto;

  @ApiProperty({ description: 'Datos del receptor', type: ReceptorDto })
  @ValidateNested()
  @Type(() => ReceptorDto)
  receptor: ReceptorDto;

  @ApiProperty({ description: 'Información de venta a terceros', required: false })
  @IsOptional()
  ventaTercero?: any;

  @ApiProperty({ description: 'Cuerpo del documento - ítems', type: [CuerpoDocumentoNrDto] })
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => CuerpoDocumentoNrDto)
  cuerpoDocumento: CuerpoDocumentoNrDto[];

  @ApiProperty({ description: 'Resumen de totales', type: ResumenNrDto })
  @ValidateNested()
  @Type(() => ResumenNrDto)
  resumen: ResumenNrDto;

  @ApiProperty({ description: 'Información de extensión (entrega)', type: ExtensionNrDto, required: false })
  @IsOptional()
  @ValidateNested()
  @Type(() => ExtensionNrDto)
  extension?: ExtensionNrDto;

  @ApiProperty({ description: 'Información adicional', required: false })
  @IsOptional()
  apendice?: any;
}