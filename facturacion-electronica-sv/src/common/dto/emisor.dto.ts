import { ApiProperty } from '@nestjs/swagger';
import { IsString, IsOptional, Length, ValidateNested } from 'class-validator';
import { Type } from 'class-transformer';
import { DireccionDto } from './direccion.dto';

export class EmisorDto {
  @ApiProperty({ 
    description: 'NIT del emisor', 
    example: '06142902640010',
    maxLength: 14 
  })
  @IsString()
  @Length(14, 14)
  nit: string;

  @ApiProperty({ 
    description: 'NRC del emisor', 
    example: '5215',
    maxLength: 8 
  })
  @IsString()
  @Length(1, 8)
  nrc: string;

  @ApiProperty({ 
    description: 'Nombre o razón social del emisor', 
    example: 'ALUMINIO DE EL SALVADOR. S.A.',
    maxLength: 100 
  })
  @IsString()
  @Length(1, 100)
  nombre: string;

  @ApiProperty({ 
    description: 'Código de actividad económica', 
    example: '24200',
    maxLength: 5 
  })
  @IsString()
  @Length(5, 5)
  codActividad: string;

  @ApiProperty({ 
    description: 'Descripción de actividad económica', 
    example: 'Fabricación de productos primarios de metales preciosos y metales no ferrosos',
    maxLength: 150 
  })
  @IsString()
  @Length(1, 150)
  descActividad: string;

  @ApiProperty({ 
    description: 'Nombre comercial del emisor', 
    example: 'ALSASA',
    maxLength: 100 
  })
  @IsString()
  @Length(1, 100)
  nombreComercial: string;

  @ApiProperty({ 
    description: 'Tipo de establecimiento', 
    example: '02',
    enum: ['01', '02', '04', '07', '20'],
    enumName: 'TipoEstablecimiento'
  })
  @IsString()
  @Length(2, 2)
  tipoEstablecimiento: string;

  @ApiProperty({ 
    description: 'Dirección del emisor',
    type: DireccionDto
  })
  @ValidateNested()
  @Type(() => DireccionDto)
  direccion: DireccionDto;

  @ApiProperty({ 
    description: 'Teléfono del emisor', 
    example: '2309-9999',
    maxLength: 25 
  })
  @IsString()
  @Length(1, 25)
  telefono: string;

  @ApiProperty({ 
    description: 'Correo electrónico del emisor', 
    example: 'info@alsasa.com',
    maxLength: 100 
  })
  @IsString()
  @Length(1, 100)
  correo: string;

  @ApiProperty({ 
    description: 'Código de establecimiento MH', 
    required: false,
    maxLength: 4 
  })
  @IsOptional()
  @IsString()
  @Length(1, 4)
  codEstableMH?: string;

  @ApiProperty({ 
    description: 'Código de establecimiento', 
    required: false,
    maxLength: 4 
  })
  @IsOptional()
  @IsString()
  @Length(1, 4)
  codEstable?: string;

  @ApiProperty({ 
    description: 'Código de punto de venta MH', 
    required: false,
    maxLength: 4 
  })
  @IsOptional()
  @IsString()
  @Length(1, 4)
  codPuntoVentaMH?: string;

  @ApiProperty({ 
    description: 'Código de punto de venta', 
    required: false,
    maxLength: 4 
  })
  @IsOptional()
  @IsString()
  @Length(1, 4)
  codPuntoVenta?: string;
}