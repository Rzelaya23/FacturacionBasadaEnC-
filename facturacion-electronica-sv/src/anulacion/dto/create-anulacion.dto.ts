import { ApiProperty } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsString, IsNumber, IsOptional, ValidateNested, IsDateString, IsDecimal, Min } from 'class-validator';

export class IdentificacionAnulacionDto {
  @ApiProperty({ description: 'Versión del documento', example: 2 })
  @IsNumber()
  version: number;

  @ApiProperty({ description: 'Ambiente de destino', example: '00' })
  @IsString()
  ambiente: string;

  @ApiProperty({ description: 'Código de generación único', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' })
  @IsString()
  codigoGeneracion: string;

  @ApiProperty({ description: 'Fecha de emisión de la anulación', example: '2024-07-21' })
  @IsDateString()
  fecAnula: string;

  @ApiProperty({ description: 'Hora de emisión de la anulación', example: '19:30:00' })
  @IsString()
  horAnula: string;
}

export class EmisorAnulacionDto {
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

  @ApiProperty({ description: 'Código de establecimiento', required: false })
  @IsOptional()
  codEstable?: any;

  @ApiProperty({ description: 'Código de punto de venta', required: false })
  @IsOptional()
  codPuntoVenta?: any;

  @ApiProperty({ description: 'Nombre del establecimiento', example: 'SUCURSAL PRINCIPAL' })
  @IsString()
  nomEstablecimiento: string;
}

export class DocumentoAnulacionDto {
  @ApiProperty({ description: 'Tipo de DTE a anular', example: '01' })
  @IsString()
  tipoDte: string;

  @ApiProperty({ description: 'Código de generación del documento a anular', example: 'B2C3D4E5-F6G7-8901-BCDE-FG2345678901' })
  @IsString()
  codigoGeneracion: string;

  @ApiProperty({ description: 'Sello recibido del MH', example: 'SELLO_MH_12345' })
  @IsString()
  selloRecibido: string;

  @ApiProperty({ description: 'Número de control del documento', example: 'DTE-01-0001-0001-0000001' })
  @IsString()
  numeroControl: string;

  @ApiProperty({ description: 'Fecha de emisión del documento original', example: '2024-07-20' })
  @IsDateString()
  fecEmi: string;

  @ApiProperty({ description: 'Monto de IVA del documento', example: 20.90, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  montoIva?: number;

  @ApiProperty({ description: 'Código de generación del receptor', required: false })
  @IsOptional()
  @IsString()
  codigoGeneracionR?: string;

  @ApiProperty({ description: 'Tipo de documento del receptor', example: '13' })
  @IsString()
  tipoDocumento: string;

  @ApiProperty({ description: 'Número de documento del receptor', example: '12345678-9' })
  @IsString()
  numDocumento: string;

  @ApiProperty({ description: 'Nombre del receptor', example: 'Juan Pérez' })
  @IsString()
  nombre: string;

  @ApiProperty({ description: 'Teléfono del receptor', example: '7234-5678', required: false })
  @IsOptional()
  @IsString()
  telefono?: string;

  @ApiProperty({ description: 'Correo del receptor', example: 'juan@email.com', required: false })
  @IsOptional()
  @IsString()
  correo?: string;
}

export class MotivoAnulacionDto {
  @ApiProperty({ description: 'Tipo de anulación', example: 1 })
  @IsNumber()
  @Min(1)
  tipoAnulacion: number;

  @ApiProperty({ description: 'Motivo de la anulación', example: 'Error en datos del receptor' })
  @IsString()
  motivoAnulacion: string;

  @ApiProperty({ description: 'Nombre del responsable', example: 'María García' })
  @IsString()
  nombreResponsable: string;

  @ApiProperty({ description: 'Tipo de documento del responsable', example: '13' })
  @IsString()
  tipDocResponsable: string;

  @ApiProperty({ description: 'Número de documento del responsable', example: '98765432-1' })
  @IsString()
  numDocResponsable: string;

  @ApiProperty({ description: 'NIT del responsable', example: '06140506901012', required: false })
  @IsOptional()
  @IsString()
  nitResponsable?: string;

  @ApiProperty({ description: 'NRC del responsable', required: false })
  @IsOptional()
  @IsString()
  nrcResponsable?: string;
}

export class CreateAnulacionDto {
  @ApiProperty({ description: 'Datos de identificación de la anulación', type: IdentificacionAnulacionDto })
  @ValidateNested()
  @Type(() => IdentificacionAnulacionDto)
  identificacion: IdentificacionAnulacionDto;

  @ApiProperty({ description: 'Datos del emisor', type: EmisorAnulacionDto })
  @ValidateNested()
  @Type(() => EmisorAnulacionDto)
  emisor: EmisorAnulacionDto;

  @ApiProperty({ description: 'Datos del documento a anular', type: DocumentoAnulacionDto })
  @ValidateNested()
  @Type(() => DocumentoAnulacionDto)
  documento: DocumentoAnulacionDto;

  @ApiProperty({ description: 'Motivo de la anulación', type: MotivoAnulacionDto })
  @ValidateNested()
  @Type(() => MotivoAnulacionDto)
  motivo: MotivoAnulacionDto;
}