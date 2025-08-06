import { ApiProperty } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsString, IsNumber, IsOptional, IsArray, ValidateNested, IsDateString, Min, Max } from 'class-validator';

export class IdentificacionContingenciaDto {
  @ApiProperty({ description: 'Versión del documento', example: 3 })
  @IsNumber()
  version: number;

  @ApiProperty({ description: 'Ambiente de destino', example: '00' })
  @IsString()
  ambiente: string;

  @ApiProperty({ description: 'Código de generación único', example: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890' })
  @IsString()
  codigoGeneracion: string;

  @ApiProperty({ description: 'Fecha de inicio de contingencia', example: '2024-07-21' })
  @IsDateString()
  fInicio: string;

  @ApiProperty({ description: 'Hora de inicio de contingencia', example: '08:00:00' })
  @IsString()
  hInicio: string;

  @ApiProperty({ description: 'Fecha de fin de contingencia', example: '2024-07-21' })
  @IsDateString()
  fFin: string;

  @ApiProperty({ description: 'Hora de fin de contingencia', example: '18:00:00' })
  @IsString()
  hFin: string;
}

export class EmisorContingenciaDto {
  @ApiProperty({ description: 'NIT del emisor', example: '06140506901012' })
  @IsString()
  nit: string;

  @ApiProperty({ description: 'Nombre del emisor', example: 'EMPRESA DE PRUEBA S.A. DE C.V.' })
  @IsString()
  nombre: string;

  @ApiProperty({ description: 'Nombre del responsable', example: 'María Elena García Rodríguez' })
  @IsString()
  nombreResponsable: string;

  @ApiProperty({ description: 'Tipo de documento del responsable', example: '13' })
  @IsString()
  tipoDocResponsable: string;

  @ApiProperty({ description: 'Número de documento del responsable', example: '98765432-1' })
  @IsString()
  numeroDocResponsable: string;

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

export class DocsContingenciaDto {
  @ApiProperty({ description: 'Número de ítem', example: 1 })
  @IsNumber()
  @Min(1)
  noItem: number;

  @ApiProperty({ description: 'Código de generación del DTE', example: 'B2C3D4E5-F6G7-8901-BCDE-FG2345678901' })
  @IsString()
  codigoGeneracion: string;

  @ApiProperty({ description: 'Tipo de documento', example: '01' })
  @IsString()
  tipoDoc: string;
}

export class MotivoContingenciaDto {
  @ApiProperty({ description: 'Tipo de contingencia', example: 1 })
  @IsNumber()
  @Min(1)
  @Max(5)
  tipoContingencia: number;

  @ApiProperty({ description: 'Motivo de la contingencia', example: 'Falla en conectividad con el Ministerio de Hacienda' })
  @IsString()
  motivoContingencia: string;
}

export class CreateContingenciaDto {
  @ApiProperty({ description: 'Datos de identificación de la contingencia', type: IdentificacionContingenciaDto })
  @ValidateNested()
  @Type(() => IdentificacionContingenciaDto)
  identificacion: IdentificacionContingenciaDto;

  @ApiProperty({ description: 'Datos del emisor', type: EmisorContingenciaDto })
  @ValidateNested()
  @Type(() => EmisorContingenciaDto)
  emisor: EmisorContingenciaDto;

  @ApiProperty({ description: 'Detalle de DTEs en contingencia', type: [DocsContingenciaDto] })
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => DocsContingenciaDto)
  detalleDTE: DocsContingenciaDto[];

  @ApiProperty({ description: 'Motivo de la contingencia', type: MotivoContingenciaDto })
  @ValidateNested()
  @Type(() => MotivoContingenciaDto)
  motivo: MotivoContingenciaDto;
}