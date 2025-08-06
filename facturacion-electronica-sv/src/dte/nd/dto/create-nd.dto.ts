import { ApiProperty } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  IsNotEmpty,
  IsString,
  IsNumber,
  IsOptional,
  IsArray,
  ValidateNested,
  IsDecimal,
  Min,
  Max,
  IsDateString,
  IsInt,
} from 'class-validator';
import { IdentificacionDto } from '../../../common/dto/identificacion.dto';
import { EmisorDto } from '../../../common/dto/emisor.dto';
import { ReceptorDto } from '../../../common/dto/receptor.dto';

export class DocumentoRelacionadoNdDto {
  @ApiProperty({
    description: 'Tipo de documento relacionado',
    example: '01',
  })
  @IsNotEmpty()
  @IsString()
  tipoDocumento: string;

  @ApiProperty({
    description: 'Tipo de generación del documento',
    example: 1,
  })
  @IsNotEmpty()
  @IsInt()
  @Min(1)
  @Max(2)
  tipoGeneracion: number;

  @ApiProperty({
    description: 'Número del documento relacionado',
    example: 'DTE-01-00000001-000000000000001',
  })
  @IsNotEmpty()
  @IsString()
  numeroDocumento: string;

  @ApiProperty({
    description: 'Fecha de emisión del documento relacionado',
    example: '2024-01-15',
  })
  @IsNotEmpty()
  @IsDateString()
  fechaEmision: string;
}

export class CuerpoDocumentoNdDto {
  @ApiProperty({
    description: 'Número de ítem',
    example: 1,
  })
  @IsNotEmpty()
  @IsInt()
  @Min(1)
  numItem: number;

  @ApiProperty({
    description: 'Tipo de ítem',
    example: 1,
  })
  @IsNotEmpty()
  @IsInt()
  @Min(1)
  @Max(4)
  tipoItem: number;

  @ApiProperty({
    description: 'Número de documento',
    example: 'DOC-001',
    required: false,
  })
  @IsOptional()
  @IsString()
  numeroDocumento?: string;

  @ApiProperty({
    description: 'Código del producto/servicio',
    example: 'PROD-001',
    required: false,
  })
  @IsOptional()
  @IsString()
  codigo?: string;

  @ApiProperty({
    description: 'Código de tributo',
    required: false,
  })
  @IsOptional()
  codTributo?: any;

  @ApiProperty({
    description: 'Descripción del ítem',
    example: 'Servicio de consultoría',
  })
  @IsNotEmpty()
  @IsString()
  descripcion: string;

  @ApiProperty({
    description: 'Cantidad',
    example: 1.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 8 })
  @Min(0)
  cantidad: number;

  @ApiProperty({
    description: 'Unidad de medida',
    example: 99,
  })
  @IsNotEmpty()
  @IsInt()
  @Min(1)
  uniMedida: number;

  @ApiProperty({
    description: 'Precio unitario',
    example: 100.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 4 })
  @Min(0)
  precioUni: number;

  @ApiProperty({
    description: 'Monto de descuento',
    example: 0.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  montoDescu: number;

  @ApiProperty({
    description: 'Venta no sujeta',
    example: 0.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  ventaNoSuj: number;

  @ApiProperty({
    description: 'Venta exenta',
    example: 0.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  ventaExenta: number;

  @ApiProperty({
    description: 'Venta gravada',
    example: 100.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  ventaGravada: number;

  @ApiProperty({
    description: 'Lista de tributos aplicables',
    type: [String],
    required: false,
  })
  @IsOptional()
  @IsArray()
  @IsString({ each: true })
  tributos?: string[];
}

export class ResumenNdDto {
  @ApiProperty({
    description: 'Total de operaciones no sujetas',
    example: 0.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalNoSuj: number;

  @ApiProperty({
    description: 'Total de operaciones exentas',
    example: 0.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalExenta: number;

  @ApiProperty({
    description: 'Total de operaciones gravadas',
    example: 100.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalGravada: number;

  @ApiProperty({
    description: 'Sub total (antes de impuestos)',
    example: 100.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  subTotalVentas: number;

  @ApiProperty({
    description: 'Total de descuentos',
    example: 0.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  descuNoSuj: number;

  @ApiProperty({
    description: 'Descuento en operaciones exentas',
    example: 0.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  descuExenta: number;

  @ApiProperty({
    description: 'Descuento en operaciones gravadas',
    example: 0.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  descuGravada: number;

  @ApiProperty({
    description: 'Total de descuentos',
    example: 0.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalDescu: number;

  @ApiProperty({
    description: 'Sub total después de descuentos',
    example: 100.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  subTotal: number;

  @ApiProperty({
    description: 'IVA retenido',
    example: 0.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  ivaRete1: number;

  @ApiProperty({
    description: 'Retención de renta',
    example: 0.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  reteRenta: number;

  @ApiProperty({
    description: 'Monto total de la operación',
    example: 113.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  montoTotalOperacion: number;

  @ApiProperty({
    description: 'Total a pagar',
    example: 113.00,
  })
  @IsNotEmpty()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalPagar: number;

  @ApiProperty({
    description: 'Total en letras',
    example: 'CIENTO TRECE 00/100 DÓLARES',
  })
  @IsNotEmpty()
  @IsString()
  totalLetras: string;

  @ApiProperty({
    description: 'Condición de la operación',
    example: 1,
  })
  @IsNotEmpty()
  @IsInt()
  @Min(1)
  @Max(3)
  condicionOperacion: number;

  @ApiProperty({
    description: 'Observaciones',
    required: false,
  })
  @IsOptional()
  @IsString()
  observaciones?: string;
}

export class ExtensionNdDto {
  @ApiProperty({
    description: 'Nombre de la extensión',
    required: false,
  })
  @IsOptional()
  @IsString()
  nombEntrega?: string;

  @ApiProperty({
    description: 'Documento de entrega',
    required: false,
  })
  @IsOptional()
  @IsString()
  docuEntrega?: string;

  @ApiProperty({
    description: 'Nombre de quien recibe',
    required: false,
  })
  @IsOptional()
  @IsString()
  nombRecibe?: string;

  @ApiProperty({
    description: 'Documento de quien recibe',
    required: false,
  })
  @IsOptional()
  @IsString()
  docuRecibe?: string;

  @ApiProperty({
    description: 'Observaciones adicionales',
    required: false,
  })
  @IsOptional()
  @IsString()
  observaciones?: string;
}

export class CreateNdDto {
  @ApiProperty({
    description: 'Información de identificación del DTE',
    type: IdentificacionDto,
  })
  @IsNotEmpty()
  @ValidateNested()
  @Type(() => IdentificacionDto)
  identificacion: IdentificacionDto;

  @ApiProperty({
    description: 'Documentos relacionados',
    type: [DocumentoRelacionadoNdDto],
  })
  @IsNotEmpty()
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => DocumentoRelacionadoNdDto)
  documentoRelacionado: DocumentoRelacionadoNdDto[];

  @ApiProperty({
    description: 'Información del emisor',
    type: EmisorDto,
  })
  @IsNotEmpty()
  @ValidateNested()
  @Type(() => EmisorDto)
  emisor: EmisorDto;

  @ApiProperty({
    description: 'Información del receptor',
    type: ReceptorDto,
  })
  @IsNotEmpty()
  @ValidateNested()
  @Type(() => ReceptorDto)
  receptor: ReceptorDto;

  @ApiProperty({
    description: 'Venta a tercero',
    required: false,
  })
  @IsOptional()
  ventaTercero?: any;

  @ApiProperty({
    description: 'Cuerpo del documento',
    type: [CuerpoDocumentoNdDto],
  })
  @IsNotEmpty()
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => CuerpoDocumentoNdDto)
  cuerpoDocumento: CuerpoDocumentoNdDto[];

  @ApiProperty({
    description: 'Resumen de la nota de débito',
    type: ResumenNdDto,
  })
  @IsNotEmpty()
  @ValidateNested()
  @Type(() => ResumenNdDto)
  resumen: ResumenNdDto;

  @ApiProperty({
    description: 'Extensión del documento',
    type: ExtensionNdDto,
    required: false,
  })
  @IsOptional()
  @ValidateNested()
  @Type(() => ExtensionNdDto)
  extension?: ExtensionNdDto;

  @ApiProperty({
    description: 'Apéndice del documento',
    required: false,
  })
  @IsOptional()
  apendice?: any;
}