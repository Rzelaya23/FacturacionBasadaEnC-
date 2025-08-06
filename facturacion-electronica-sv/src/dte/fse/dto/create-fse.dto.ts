import { ApiProperty } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsString, IsNumber, IsOptional, IsArray, ValidateNested, IsDecimal, Min, Max } from 'class-validator';
import { DireccionDto } from '../../../common/dto/direccion.dto';
import { IdentificacionDto } from '../../../common/dto/identificacion.dto';
import { EmisorDto } from '../../../common/dto/emisor.dto';

export class SujetoExcluidoDto {
  @ApiProperty({ description: 'Tipo de documento del sujeto excluido', example: '36' })
  @IsString()
  tipoDocumento: string;

  @ApiProperty({ description: 'Número de documento del sujeto excluido', example: '12345678-9' })
  @IsString()
  numDocumento: string;

  @ApiProperty({ description: 'Nombre del sujeto excluido', example: 'Juan Pérez' })
  @IsString()
  nombre: string;

  @ApiProperty({ description: 'Código de actividad económica', example: '01111' })
  @IsString()
  codActividad: string;

  @ApiProperty({ description: 'Descripción de actividad económica', example: 'Cultivo de maíz' })
  @IsString()
  descActividad: string;

  @ApiProperty({ description: 'Dirección del sujeto excluido', type: DireccionDto })
  @ValidateNested()
  @Type(() => DireccionDto)
  direccion: DireccionDto;

  @ApiProperty({ description: 'Teléfono del sujeto excluido', example: '2234-5678', required: false })
  @IsOptional()
  @IsString()
  telefono?: string;

  @ApiProperty({ description: 'Correo electrónico del sujeto excluido', example: 'juan@email.com', required: false })
  @IsOptional()
  @IsString()
  correo?: string;
}

export class CuerpoDocumentoFseDto {
  @ApiProperty({ description: 'Número de ítem', example: 1 })
  @IsNumber()
  @Min(1)
  numItem: number;

  @ApiProperty({ description: 'Tipo de ítem', example: 1 })
  @IsNumber()
  @Min(1)
  @Max(4)
  tipoItem: number;

  @ApiProperty({ description: 'Cantidad', example: 10.5 })
  @IsNumber({ maxDecimalPlaces: 8 })
  @Min(0)
  cantidad: number;

  @ApiProperty({ description: 'Código del producto/servicio', example: 'PROD001', required: false })
  @IsOptional()
  @IsString()
  codigo?: string;

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

  @ApiProperty({ description: 'Total de compra del ítem', example: 163.25 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  compra: number;
}

export class ResumenFseDto {
  @ApiProperty({ description: 'Total de compra', example: 163.25 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalCompra: number;

  @ApiProperty({ description: 'Descuento aplicado', example: 2.50, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  descu?: number;

  @ApiProperty({ description: 'Total de descuento', example: 2.50, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalDescu?: number;

  @ApiProperty({ description: 'Subtotal', example: 160.75 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  subTotal: number;

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

  @ApiProperty({ description: 'Total a pagar', example: 160.75 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalPagar: number;

  @ApiProperty({ description: 'Total en letras', example: 'CIENTO SESENTA DÓLARES CON 75/100' })
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

  @ApiProperty({ description: 'Observaciones', required: false })
  @IsOptional()
  @IsString()
  observaciones?: string;
}

export class CreateFseDto {
  @ApiProperty({ description: 'Datos de identificación del DTE', type: IdentificacionDto })
  @ValidateNested()
  @Type(() => IdentificacionDto)
  identificacion: IdentificacionDto;

  @ApiProperty({ description: 'Datos del emisor', type: EmisorDto })
  @ValidateNested()
  @Type(() => EmisorDto)
  emisor: EmisorDto;

  @ApiProperty({ description: 'Datos del sujeto excluido', type: SujetoExcluidoDto })
  @ValidateNested()
  @Type(() => SujetoExcluidoDto)
  sujetoExcluido: SujetoExcluidoDto;

  @ApiProperty({ description: 'Cuerpo del documento - ítems', type: [CuerpoDocumentoFseDto] })
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => CuerpoDocumentoFseDto)
  cuerpoDocumento: CuerpoDocumentoFseDto[];

  @ApiProperty({ description: 'Resumen de totales', type: ResumenFseDto })
  @ValidateNested()
  @Type(() => ResumenFseDto)
  resumen: ResumenFseDto;

  @ApiProperty({ description: 'Información adicional', required: false })
  @IsOptional()
  apendice?: any;
}