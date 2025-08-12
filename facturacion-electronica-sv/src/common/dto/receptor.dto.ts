import { ApiProperty } from '@nestjs/swagger';
import { IsString, IsOptional, Length, ValidateNested, IsIn } from 'class-validator';
import { Type } from 'class-transformer';
import { DireccionDto } from './direccion.dto';

export class ReceptorDto {
  @ApiProperty({ 
    description: 'Tipo de documento del receptor', 
    example: '36',
    enum: ['13', '02', '03', '36', '37'],
    enumName: 'TipoDocumento'
  })
  @IsString()
  @Length(2, 2)
  @IsIn(['13', '02', '03', '36', '37'], { 
    message: 'Tipo documento debe ser: 13(DUI), 02(Carnet Residente), 03(Pasaporte), 36(NIT), 37(Otro)' 
  })
  tipoDocumento: string;

  @ApiProperty({ 
    description: 'Número de documento del receptor', 
    example: '06142902640010',
    maxLength: 20 
  })
  @IsString()
  @Length(1, 20)
  numDocumento: string;

  @ApiProperty({ 
    description: 'NIT del receptor (opcional, para ciertos DTEs como CCF)', 
    example: '06142902640010',
    required: false,
    maxLength: 14 
  })
  @IsOptional()
  @IsString()
  @Length(1, 14)
  nit?: string;

  @ApiProperty({ 
    description: 'NRC del receptor (opcional)', 
    example: '5215',
    required: false,
    maxLength: 8 
  })
  @IsOptional()
  @IsString()
  @Length(1, 8)
  nrc?: string;

  @ApiProperty({ 
    description: 'Nombre o razón social del receptor', 
    example: 'EMPRESA RECEPTORA S.A. DE C.V.',
    maxLength: 100 
  })
  @IsString()
  @Length(1, 100)
  nombre: string;

  @ApiProperty({ 
    description: 'Código de actividad económica del receptor (opcional)', 
    example: '46900',
    required: false,
    maxLength: 5 
  })
  @IsOptional()
  @IsString()
  @Length(5, 5)
  codActividad?: string;

  @ApiProperty({ 
    description: 'Descripción de actividad económica del receptor (opcional)', 
    example: 'Comercio al por mayor de otros productos',
    required: false,
    maxLength: 150 
  })
  @IsOptional()
  @IsString()
  @Length(1, 150)
  descActividad?: string;

  @ApiProperty({ 
    description: 'Nombre comercial del receptor (opcional)', 
    example: 'EMPRESA RECEPTORA',
    required: false,
    maxLength: 100 
  })
  @IsOptional()
  @IsString()
  @Length(1, 100)
  nombreComercial?: string;

  @ApiProperty({ 
    description: 'Dirección del receptor',
    type: DireccionDto
  })
  @ValidateNested()
  @Type(() => DireccionDto)
  direccion: DireccionDto;

  @ApiProperty({ 
    description: 'Teléfono del receptor (opcional)', 
    example: '2222-3333',
    required: false,
    maxLength: 25 
  })
  @IsOptional()
  @IsString()
  @Length(1, 25)
  telefono?: string;

  @ApiProperty({ 
    description: 'Correo electrónico del receptor (opcional)', 
    example: 'contacto@empresa.com',
    required: false,
    maxLength: 100 
  })
  @IsOptional()
  @IsString()
  @Length(1, 100)
  correo?: string;
}