import { ApiProperty } from '@nestjs/swagger';
import { IsNotEmpty, IsString, IsNumber, IsArray, ValidateNested, IsOptional, Min, Max } from 'class-validator';
import { Type } from 'class-transformer';
import { IdentificacionDto } from '../../../common/dto/identificacion.dto';
import { EmisorDto } from '../../../common/dto/emisor.dto';
import { ReceptorDto } from '../../../common/dto/receptor.dto';
import { TributoDto } from '../../../common/dto/tributo.dto';

export class DocumentoRelacionadoNcDto {
  @ApiProperty({ description: 'Tipo de documento relacionado', example: '01' })
  @IsNotEmpty()
  @IsString()
  tipoDocumento: string;

  @ApiProperty({ description: 'Tipo de generación del documento relacionado', example: '1' })
  @IsNumber()
  @Min(1)
  @Max(2)
  tipoGeneracion: number;

  @ApiProperty({ description: 'Número de documento relacionado', example: 'DTE-01-0001-0001-000000000000001' })
  @IsNotEmpty()
  @IsString()
  numeroDocumento: string;

  @ApiProperty({ description: 'Fecha de emisión del documento relacionado', example: '2024-07-15' })
  @IsNotEmpty()
  @IsString()
  fechaEmision: string;
}

export class CuerpoDocumentoNcDto {
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

  @ApiProperty({ description: 'Código del ítem', required: false })
  @IsOptional()
  @IsString()
  codigo?: string;

  @ApiProperty({ description: 'Código de tributo', required: false })
  @IsOptional()
  codTributo?: any;

  @ApiProperty({ description: 'Descripción del ítem', example: 'Producto de ejemplo' })
  @IsNotEmpty()
  @IsString()
  descripcion: string;

  @ApiProperty({ description: 'Cantidad del ítem', example: 1.00 })
  @IsNumber({ maxDecimalPlaces: 8 })
  @Min(0)
  cantidad: number;

  @ApiProperty({ description: 'Unidad de medida (1=Unidad, 2=Docena, etc.)', example: 1 })
  @IsNumber()
  @Min(1)
  uniMedida: number;

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
}

export class ResumenNcDto {
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

  @ApiProperty({ description: 'Total en letras', example: 'CIENTO TRECE 00/100 DÓLARES' })
  @IsNotEmpty()
  @IsString()
  totalLetras: string;

  @ApiProperty({ description: 'Condición de operación (1=Contado, 2=Crédito, 3=Otro)', example: 1 })
  @IsNumber()
  @Min(1)
  @Max(3)
  condicionOperacion: number;
}

export class CreateNcDto {
  @ApiProperty({ description: 'Datos de identificación de la NC', type: IdentificacionDto })
  @ValidateNested()
  @Type(() => IdentificacionDto)
  identificacion: IdentificacionDto;

  @ApiProperty({ description: 'Documentos relacionados', type: [DocumentoRelacionadoNcDto] })
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => DocumentoRelacionadoNcDto)
  documentoRelacionado: DocumentoRelacionadoNcDto[];

  @ApiProperty({ description: 'Datos del emisor', type: EmisorDto })
  @ValidateNested()
  @Type(() => EmisorDto)
  emisor: EmisorDto;

  @ApiProperty({ description: 'Datos del receptor', type: ReceptorDto })
  @ValidateNested()
  @Type(() => ReceptorDto)
  receptor: ReceptorDto;

  @ApiProperty({ description: 'Venta a terceros', required: false })
  @IsOptional()
  ventaTercero?: any;

  @ApiProperty({ description: 'Cuerpo del documento', type: [CuerpoDocumentoNcDto] })
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => CuerpoDocumentoNcDto)
  cuerpoDocumento: CuerpoDocumentoNcDto[];

  @ApiProperty({ description: 'Resumen de la NC', type: ResumenNcDto })
  @ValidateNested()
  @Type(() => ResumenNcDto)
  resumen: ResumenNcDto;

  @ApiProperty({ description: 'Extensión del documento', required: false })
  @IsOptional()
  extension?: any;

  @ApiProperty({ description: 'Apéndice del documento', required: false })
  @IsOptional()
  apendice?: any;
}