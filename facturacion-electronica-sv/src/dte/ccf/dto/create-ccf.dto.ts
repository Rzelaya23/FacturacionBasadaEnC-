import { ApiProperty } from '@nestjs/swagger';
import { IsNotEmpty, IsString, IsNumber, IsArray, ValidateNested, IsOptional, IsDecimal, Min, Max } from 'class-validator';
import { Type } from 'class-transformer';
import { IdentificacionDto } from '../../../common/dto/identificacion.dto';
import { EmisorDto } from '../../../common/dto/emisor.dto';
import { ReceptorDto } from '../../../common/dto/receptor.dto';
import { TributoDto } from '../../../common/dto/tributo.dto';

export class CuerpoDocumentoCcfDto {
  @ApiProperty({ description: 'Número correlativo del ítem', example: 1 })
  @IsNumber()
  @Min(1)
  numItem: number;

  @ApiProperty({ description: 'Tipo de ítem (1=Bien, 2=Servicio, 3=Ambos, 4=Otros)', example: 1 })
  @IsNumber()
  @Min(1)
  @Max(4)
  tipoItem: number;

  @ApiProperty({ description: 'Número de documento del ítem', required: false })
  @IsOptional()
  @IsString()
  numeroDocumento?: string;

  @ApiProperty({ description: 'Cantidad del ítem', example: 1.00 })
  @IsNumber({ maxDecimalPlaces: 8 })
  @Min(0)
  cantidad: number;

  @ApiProperty({ description: 'Código del ítem', required: false })
  @IsOptional()
  @IsString()
  codigo?: string;

  @ApiProperty({ description: 'Código de tributo', required: false })
  @IsOptional()
  codTributo?: any;

  @ApiProperty({ description: 'Unidad de medida (1=Unidad, 2=Docena, etc.)', example: 1 })
  @IsNumber()
  @Min(1)
  uniMedida: number;

  @ApiProperty({ description: 'Descripción del ítem', example: 'Producto de ejemplo' })
  @IsNotEmpty()
  @IsString()
  descripcion: string;

  @ApiProperty({ description: 'Precio unitario', example: 10.50 })
  @IsNumber({ maxDecimalPlaces: 4 })
  @Min(0)
  precioUni: number;

  @ApiProperty({ description: 'Monto de descuento', example: 0.00 })
  @IsNumber({ maxDecimalPlaces: 4 })
  @Min(0)
  montoDescu: number;

  @ApiProperty({ description: 'Venta no sujeta', example: 0.00 })
  @IsNumber({ maxDecimalPlaces: 4 })
  @Min(0)
  ventaNoSuj: number;

  @ApiProperty({ description: 'Venta exenta', example: 0.00 })
  @IsNumber({ maxDecimalPlaces: 4 })
  @Min(0)
  ventaExenta: number;

  @ApiProperty({ description: 'Venta gravada', example: 10.50 })
  @IsNumber({ maxDecimalPlaces: 4 })
  @Min(0)
  ventaGravada: number;

  @ApiProperty({ description: 'Lista de tributos aplicables', type: [String], required: false })
  @IsOptional()
  @IsArray()
  @IsString({ each: true })
  tributos?: string[];

  @ApiProperty({ description: 'Precio de venta sugerido', example: 0.00 })
  @IsNumber({ maxDecimalPlaces: 4 })
  @Min(0)
  psv: number;

  @ApiProperty({ description: 'Monto no gravado', example: 0.00 })
  @IsNumber({ maxDecimalPlaces: 4 })
  @Min(0)
  noGravado: number;
}

export class ResumenCcfDto {
  @ApiProperty({ description: 'Total de operaciones no sujetas', example: 0.00 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalNoSuj: number;

  @ApiProperty({ description: 'Total de operaciones exentas', example: 0.00 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalExenta: number;

  @ApiProperty({ description: 'Total de operaciones gravadas', example: 100.00 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalGravada: number;

  @ApiProperty({ description: 'Sub-total de ventas', example: 100.00 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  subTotalVentas: number;

  @ApiProperty({ description: 'Descuento en operaciones no sujetas', example: 0.00 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  descuNoSuj: number;

  @ApiProperty({ description: 'Descuento en operaciones exentas', example: 0.00 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  descuExenta: number;

  @ApiProperty({ description: 'Descuento en operaciones gravadas', example: 0.00 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  descuGravada: number;

  @ApiProperty({ description: 'Porcentaje de descuento', example: 0.00 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  @Max(100)
  porcentajeDescuento: number;

  @ApiProperty({ description: 'Total de descuentos', example: 0.00 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalDescu: number;

  @ApiProperty({ description: 'Lista de tributos', type: [TributoDto] })
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => TributoDto)
  tributos: TributoDto[];

  @ApiProperty({ description: 'Sub-total después de descuentos', example: 100.00 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  subTotal: number;

  @ApiProperty({ description: 'IVA percibido 1%', example: 0.00 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  ivaPerci1: number;

  @ApiProperty({ description: 'IVA retenido 1%', example: 0.00 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  ivaRete1: number;

  @ApiProperty({ description: 'Retención de renta', example: 0.00 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  reteRenta: number;

  @ApiProperty({ description: 'Monto total de la operación', example: 113.00 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  montoTotalOperacion: number;

  @ApiProperty({ description: 'Total no gravado', example: 0.00 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalNoGravado: number;

  @ApiProperty({ description: 'Total a pagar', example: 113.00 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalPagar: number;

  @ApiProperty({ description: 'Total en letras', example: 'CIENTO TRECE 00/100 DÓLARES' })
  @IsNotEmpty()
  @IsString()
  totalLetras: string;

  @ApiProperty({ description: 'Saldo a favor', example: 0.00 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  saldoFavor: number;

  @ApiProperty({ description: 'Condición de operación (1=Contado, 2=Crédito, 3=Otro)', example: 1 })
  @IsNumber()
  @Min(1)
  @Max(3)
  condicionOperacion: number;

  @ApiProperty({ description: 'Información de pagos', required: false })
  @IsOptional()
  pagos?: any;

  @ApiProperty({ description: 'Número de pago electrónico', required: false })
  @IsOptional()
  numPagoElectronico?: any;
}

export class CreateCcfDto {
  @ApiProperty({ description: 'Datos de identificación del CCF', type: IdentificacionDto })
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

  @ApiProperty({ description: 'Otros documentos', required: false })
  @IsOptional()
  otrosDocumentos?: any;

  @ApiProperty({ description: 'Venta a terceros', required: false })
  @IsOptional()
  ventaTercero?: any;

  @ApiProperty({ description: 'Cuerpo del documento', type: [CuerpoDocumentoCcfDto] })
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => CuerpoDocumentoCcfDto)
  cuerpoDocumento: CuerpoDocumentoCcfDto[];

  @ApiProperty({ description: 'Resumen del CCF', type: ResumenCcfDto })
  @ValidateNested()
  @Type(() => ResumenCcfDto)
  resumen: ResumenCcfDto;

  @ApiProperty({ description: 'Extensión del documento', required: false })
  @IsOptional()
  extension?: any;

  @ApiProperty({ description: 'Apéndice del documento', required: false })
  @IsOptional()
  apendice?: any;
}