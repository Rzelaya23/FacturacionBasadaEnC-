import { ApiProperty } from '@nestjs/swagger';
import { IsString, Length } from 'class-validator';

export class DireccionDto {
  @ApiProperty({ 
    description: 'Código del departamento', 
    example: '05',
    maxLength: 2 
  })
  @IsString()
  @Length(2, 2)
  departamento: string;

  @ApiProperty({ 
    description: 'Código del municipio', 
    example: '03',
    maxLength: 2 
  })
  @IsString()
  @Length(2, 2)
  municipio: string;

  @ApiProperty({ 
    description: 'Dirección complementaria', 
    example: 'Carretera a Sonsonate, Km 28 1/2, Lourdes, Colón',
    maxLength: 200 
  })
  @IsString()
  @Length(1, 200)
  complemento: string;
}