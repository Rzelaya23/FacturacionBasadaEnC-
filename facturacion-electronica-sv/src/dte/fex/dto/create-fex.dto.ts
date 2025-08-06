import { ApiProperty } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsString, IsNumber, IsOptional, IsArray, ValidateNested, IsDecimal, Min, Max } from 'class-validator';
import { DireccionDto } from '../../../common/dto/direccion.dto';
import { IdentificacionDto } from '../../../common/dto/identificacion.dto';
import { EmisorDto } from '../../../common/dto/emisor.dto';
import { ReceptorDto } from '../../../common/dto/receptor.dto';

export class EmisorFexDto extends EmisorDto {
  @ApiProperty({ description: 'Tipo de ítem de exportación', example: 1 })
  @IsNumber()
  @Min(1)
  @Max(3)
  tipoItemExpor: number;

  @ApiProperty({ description: 'Recinto fiscal', example: 'PUERTO ACAJUTLA', required: false })
  @IsOptional()
  @IsString()
  recintoFiscal?: string;

  @ApiProperty({ description: 'Régimen de exportación', example: 'DEFINITIVO', required: false })
  @IsOptional()
  @IsString()
  regimen?: string;
}

export class ReceptorFexDto {
  @ApiProperty({ description: 'Nombre del receptor', example: 'EMPRESA IMPORTADORA S.A.' })
  @IsString()
  nombre: string;

  @ApiProperty({ description: 'Código del país', example: 'US' })
  @IsString()
  codPais: string;

  @ApiProperty({ description: 'Nombre del país', example: 'Estados Unidos' })
  @IsString()
  nombrePais: string;

  @ApiProperty({ description: 'Dirección completa', example: '123 Main Street, New York, NY 10001' })
  @IsString()
  complemento: string;

  @ApiProperty({ description: 'Tipo de documento del receptor', example: 'PASAPORTE' })
  @IsString()
  tipoDocumento: string;

  @ApiProperty({ description: 'Número de documento del receptor', example: 'A12345678' })
  @IsString()
  numDocumento: string;

  @ApiProperty({ description: 'Nombre comercial', example: 'IMPORTADORA XYZ', required: false })
  @IsOptional()
  @IsString()
  nombreComercial?: string;

  @ApiProperty({ description: 'Tipo de persona (1=Natural, 2=Jurídica)', example: 2 })
  @IsNumber()
  @Min(1)
  @Max(2)
  tipoPersona: number;

  @ApiProperty({ description: 'Descripción de actividad', example: 'Comercio al por mayor', required: false })
  @IsOptional()
  @IsString()
  descActividad?: string;

  @ApiProperty({ description: 'Teléfono del receptor', example: '+1-555-123-4567', required: false })
  @IsOptional()
  @IsString()
  telefono?: string;

  @ApiProperty({ description: 'Correo electrónico del receptor', example: 'contacto@importadora.com', required: false })
  @IsOptional()
  @IsString()
  correo?: string;
}

export class CuerpoDocumentoFexDto {
  @ApiProperty({ description: 'Número de ítem', example: 1 })
  @IsNumber()
  @Min(1)
  numItem: number;

  @ApiProperty({ description: 'Código del producto/servicio', example: 'PROD001', required: false })
  @IsOptional()
  @IsString()
  codigo?: string;

  @ApiProperty({ description: 'Descripción del producto/servicio', example: 'Café gourmet de exportación' })
  @IsString()
  descripcion: string;

  @ApiProperty({ description: 'Cantidad', example: 100.5 })
  @IsNumber({ maxDecimalPlaces: 8 })
  @Min(0)
  cantidad: number;

  @ApiProperty({ description: 'Unidad de medida', example: 99 })
  @IsNumber()
  @Min(1)
  uniMedida: number;

  @ApiProperty({ description: 'Precio unitario', example: 25.75 })
  @IsNumber({ maxDecimalPlaces: 4 })
  @Min(0)
  precioUni: number;

  @ApiProperty({ description: 'Monto de descuento', example: 5.00, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  montoDescu?: number;

  @ApiProperty({ description: 'Venta gravada', example: 2583.75 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  ventaGravada: number;

  @ApiProperty({ description: 'Tributos aplicables', type: [String], required: false })
  @IsOptional()
  @IsArray()
  @IsString({ each: true })
  tributos?: string[];

  @ApiProperty({ description: 'Monto no gravado', example: 0.00, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  noGravado?: number;
}

export class ResumenFexDto {
  @ApiProperty({ description: 'Total de operaciones gravadas', example: 2583.75 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalGravada: number;

  @ApiProperty({ description: 'Descuento aplicado', example: 5.00, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  descuento?: number;

  @ApiProperty({ description: 'Porcentaje de descuento', example: 0.19, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 4 })
  @Min(0)
  @Max(100)
  porcentajeDescuento?: number;

  @ApiProperty({ description: 'Total de descuentos', example: 5.00, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalDescu?: number;

  @ApiProperty({ description: 'Monto total de la operación', example: 2578.75 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  montoTotalOperacion: number;

  @ApiProperty({ description: 'Total no gravado', example: 0.00, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalNoGravado?: number;

  @ApiProperty({ description: 'Total a pagar', example: 2578.75 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalPagar: number;

  @ApiProperty({ description: 'Total en letras', example: 'DOS MIL QUINIENTOS SETENTA Y OCHO DÓLARES CON 75/100' })
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

  @ApiProperty({ description: 'Código Incoterms', example: 'FOB', required: false })
  @IsOptional()
  @IsString()
  codIncoterms?: string;

  @ApiProperty({ description: 'Descripción Incoterms', example: 'Free On Board', required: false })
  @IsOptional()
  @IsString()
  descIncoterms?: string;

  @ApiProperty({ description: 'Observaciones', required: false })
  @IsOptional()
  @IsString()
  observaciones?: string;

  @ApiProperty({ description: 'Costo de flete', example: 150.00, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  flete?: number;

  @ApiProperty({ description: 'Número de pago electrónico', required: false })
  @IsOptional()
  @IsString()
  numPagoElectronico?: string;

  @ApiProperty({ description: 'Costo de seguro', example: 25.00, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  seguro?: number;
}

export class CreateFexDto {
  @ApiProperty({ description: 'Datos de identificación del DTE', type: IdentificacionDto })
  @ValidateNested()
  @Type(() => IdentificacionDto)
  identificacion: IdentificacionDto;

  @ApiProperty({ description: 'Datos del emisor exportador', type: EmisorFexDto })
  @ValidateNested()
  @Type(() => EmisorFexDto)
  emisor: EmisorFexDto;

  @ApiProperty({ description: 'Datos del receptor (importador)', type: ReceptorFexDto })
  @ValidateNested()
  @Type(() => ReceptorFexDto)
  receptor: ReceptorFexDto;

  @ApiProperty({ description: 'Otros documentos relacionados', required: false })
  @IsOptional()
  otrosDocumentos?: any;

  @ApiProperty({ description: 'Información de venta a terceros', required: false })
  @IsOptional()
  ventaTercero?: any;

  @ApiProperty({ description: 'Cuerpo del documento - ítems de exportación', type: [CuerpoDocumentoFexDto] })
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => CuerpoDocumentoFexDto)
  cuerpoDocumento: CuerpoDocumentoFexDto[];

  @ApiProperty({ description: 'Resumen de totales', type: ResumenFexDto })
  @ValidateNested()
  @Type(() => ResumenFexDto)
  resumen: ResumenFexDto;

  @ApiProperty({ description: 'Información adicional', required: false })
  @IsOptional()
  apendice?: any;
}