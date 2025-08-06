import { ApiProperty } from '@nestjs/swagger';
import { IsOptional, IsString, IsDateString, IsArray, IsNumber, Min, Max } from 'class-validator';

export class ReportFiltersDto {
  @ApiProperty({ description: 'Fecha de inicio', example: '2024-07-01', required: false })
  @IsOptional()
  @IsDateString()
  fechaInicio?: string;

  @ApiProperty({ description: 'Fecha de fin', example: '2024-07-31', required: false })
  @IsOptional()
  @IsDateString()
  fechaFin?: string;

  @ApiProperty({ description: 'Tipos de DTE', example: ['01', '03', '05'], required: false })
  @IsOptional()
  @IsArray()
  @IsString({ each: true })
  tiposDte?: string[];

  @ApiProperty({ description: 'Estados de DTE', example: ['PROCESADO', 'RECHAZADO'], required: false })
  @IsOptional()
  @IsArray()
  @IsString({ each: true })
  estados?: string[];

  @ApiProperty({ description: 'NIT del emisor', example: '06140506901012', required: false })
  @IsOptional()
  @IsString()
  emisorNit?: string;

  @ApiProperty({ description: 'Monto mínimo', example: 100.00, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  montoMinimo?: number;

  @ApiProperty({ description: 'Monto máximo', example: 10000.00, required: false })
  @IsOptional()
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  montoMaximo?: number;

  @ApiProperty({ description: 'Página', example: 1, required: false })
  @IsOptional()
  @IsNumber()
  @Min(1)
  page?: number;

  @ApiProperty({ description: 'Límite por página', example: 50, required: false })
  @IsOptional()
  @IsNumber()
  @Min(1)
  @Max(1000)
  limit?: number;
}

export class DashboardFiltersDto {
  @ApiProperty({ description: 'Período de análisis', example: 'mes', required: false })
  @IsOptional()
  @IsString()
  periodo?: 'dia' | 'semana' | 'mes' | 'trimestre' | 'año';

  @ApiProperty({ description: 'Fecha de referencia', example: '2024-07-21', required: false })
  @IsOptional()
  @IsDateString()
  fechaReferencia?: string;

  @ApiProperty({ description: 'Incluir comparación con período anterior', example: true, required: false })
  @IsOptional()
  incluirComparacion?: boolean;
}