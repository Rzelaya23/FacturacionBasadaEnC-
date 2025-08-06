import { ApiProperty } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsString, IsNumber, IsOptional, IsArray, ValidateNested, IsDateString, Min, Max } from 'class-validator';

export class IdentificacionLoteDto {
  @ApiProperty({ description: 'Versión del lote', example: 1 })
  @IsNumber()
  version: number;

  @ApiProperty({ description: 'Ambiente de destino', example: '00' })
  @IsString()
  ambiente: string;

  @ApiProperty({ description: 'Código de generación del lote', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' })
  @IsString()
  codigoLote: string;

  @ApiProperty({ description: 'Fecha de procesamiento del lote', example: '2024-07-21' })
  @IsDateString()
  fecRecepcion: string;

  @ApiProperty({ description: 'Hora de procesamiento del lote', example: '19:30:00' })
  @IsString()
  horRecepcion: string;

  @ApiProperty({ description: 'Cantidad de documentos en el lote', example: 25 })
  @IsNumber()
  @Min(1)
  @Max(500)
  cantidadDoc: number;
}

export class EmisorLoteDto {
  @ApiProperty({ description: 'NIT del emisor', example: '06140506901012' })
  @IsString()
  nit: string;

  @ApiProperty({ description: 'Nombre del emisor', example: 'EMPRESA DE PRUEBA S.A. DE C.V.' })
  @IsString()
  nombre: string;

  @ApiProperty({ description: 'Tipo de establecimiento', example: '01' })
  @IsString()
  tipoEstablecimiento: string;

  @ApiProperty({ description: 'Teléfono del emisor', example: '2234-5678' })
  @IsString()
  telefono: string;

  @ApiProperty({ description: 'Correo del emisor', example: 'empresa@prueba.com' })
  @IsString()
  correo: string;
}

export class DocumentoLoteDto {
  @ApiProperty({ description: 'Número de ítem en el lote', example: 1 })
  @IsNumber()
  @Min(1)
  noItem: number;

  @ApiProperty({ description: 'Tipo de DTE', example: '01' })
  @IsString()
  tipoDte: string;

  @ApiProperty({ description: 'Código de generación del DTE', example: 'B2C3D4E5-F6G7-8901-BCDE-FG2345678901' })
  @IsString()
  codigoGeneracion: string;

  @ApiProperty({ description: 'Número de control del DTE', example: 'DTE-01-0001-0001-0000001' })
  @IsString()
  numeroControl: string;

  @ApiProperty({ description: 'Fecha de emisión del DTE', example: '2024-07-21' })
  @IsDateString()
  fecEmi: string;

  @ApiProperty({ description: 'Monto total del DTE', example: 181.65 })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  montoTotal: number;

  @ApiProperty({ description: 'Documento JSON del DTE (base64 o JSON string)' })
  @IsString()
  documentoJson: string;
}

export class CreateLoteDto {
  @ApiProperty({ description: 'Datos de identificación del lote', type: IdentificacionLoteDto })
  @ValidateNested()
  @Type(() => IdentificacionLoteDto)
  identificacion: IdentificacionLoteDto;

  @ApiProperty({ description: 'Datos del emisor', type: EmisorLoteDto })
  @ValidateNested()
  @Type(() => EmisorLoteDto)
  emisor: EmisorLoteDto;

  @ApiProperty({ description: 'Lista de documentos en el lote', type: [DocumentoLoteDto] })
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => DocumentoLoteDto)
  documentos: DocumentoLoteDto[];
}

export class ConsultaLoteDto {
  @ApiProperty({ description: 'Código del lote a consultar', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' })
  @IsString()
  codigoLote: string;

  @ApiProperty({ description: 'NIT del emisor', example: '06140506901012' })
  @IsString()
  nit: string;

  @ApiProperty({ description: 'Ambiente', example: '00' })
  @IsString()
  ambiente: string;
}