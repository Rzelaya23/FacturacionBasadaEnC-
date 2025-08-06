import { ApiProperty } from '@nestjs/swagger';
import { IsString, IsNumber, IsOptional, Length, IsIn, IsDateString } from 'class-validator';

export class IdentificacionDto {
  @ApiProperty({ 
    description: 'Versión del DTE', 
    example: 1,
    minimum: 1,
    maximum: 99 
  })
  @IsNumber()
  version: number;

  @ApiProperty({ 
    description: 'Ambiente de destino', 
    example: '00',
    enum: ['00', '01'],
    enumName: 'Ambiente'
  })
  @IsString()
  @Length(2, 2)
  @IsIn(['00', '01'], { message: 'Ambiente debe ser 00 (Pruebas) o 01 (Producción)' })
  ambiente: string;

  @ApiProperty({ 
    description: 'Tipo de DTE', 
    example: '01',
    enum: ['01', '03', '04', '05', '06', '11', '14'],
    enumName: 'TipoDTE'
  })
  @IsString()
  @Length(2, 2)
  @IsIn(['01', '03', '04', '05', '06', '11', '14'], { 
    message: 'Tipo DTE debe ser: 01(FE), 03(CCF), 04(NR), 05(NC), 06(ND), 11(FSE), 14(FEX)' 
  })
  tipoDte: string;

  @ApiProperty({ 
    description: 'Número de control del DTE', 
    example: 'DTE-01-00000001-000000000000001',
    maxLength: 31 
  })
  @IsString()
  @Length(1, 31)
  numeroControl: string;

  @ApiProperty({ 
    description: 'Código de generación único', 
    example: 'A1B2C3D4',
    maxLength: 36 
  })
  @IsString()
  @Length(1, 36)
  codigoGeneracion: string;

  @ApiProperty({ 
    description: 'Tipo de modelo de facturación', 
    example: 1,
    enum: [1, 2],
    enumName: 'TipoModelo'
  })
  @IsNumber()
  @IsIn([1, 2], { message: 'Tipo modelo debe ser 1 (Normal) o 2 (Previo)' })
  tipoModelo: number;

  @ApiProperty({ 
    description: 'Tipo de operación', 
    example: 1,
    enum: [1, 2],
    enumName: 'TipoOperacion'
  })
  @IsNumber()
  @IsIn([1, 2], { message: 'Tipo operación debe ser 1 (Normal) o 2 (Contingencia)' })
  tipoOperacion: number;

  @ApiProperty({ 
    description: 'Tipo de contingencia (solo si tipoOperacion = 2)', 
    example: '1',
    required: false,
    enum: ['1', '2', '3', '4', '5']
  })
  @IsOptional()
  @IsString()
  @IsIn(['1', '2', '3', '4', '5'], { 
    message: 'Tipo contingencia debe ser: 1-5' 
  })
  tipoContingencia?: string;

  @ApiProperty({ 
    description: 'Motivo de contingencia (solo si tipoOperacion = 2)', 
    example: 'Falla en el sistema del MH',
    required: false,
    maxLength: 500
  })
  @IsOptional()
  @IsString()
  @Length(1, 500)
  motivoContin?: string;

  @ApiProperty({ 
    description: 'Fecha de emisión', 
    example: '2024-01-15',
    format: 'date'
  })
  @IsDateString({}, { message: 'Fecha de emisión debe tener formato YYYY-MM-DD' })
  fecEmi: string;

  @ApiProperty({ 
    description: 'Hora de emisión', 
    example: '14:30:00',
    pattern: '^([01]?[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$'
  })
  @IsString()
  @Length(8, 8)
  horEmi: string;

  @ApiProperty({ 
    description: 'Tipo de moneda', 
    example: 'USD',
    enum: ['USD'],
    enumName: 'TipoMoneda'
  })
  @IsString()
  @Length(3, 3)
  @IsIn(['USD'], { message: 'Tipo moneda debe ser USD' })
  tipoMoneda: string;
}