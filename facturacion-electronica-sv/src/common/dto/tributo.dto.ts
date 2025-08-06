import { ApiProperty } from '@nestjs/swagger';
import { IsString, IsNumber, Length, Min } from 'class-validator';

export class TributoDto {
  @ApiProperty({ 
    description: 'Código del tributo', 
    example: '20',
    maxLength: 2 
  })
  @IsString()
  @Length(1, 2)
  codigo: string;

  @ApiProperty({ 
    description: 'Descripción del tributo', 
    example: 'Impuesto al Valor Agregado 13%',
    maxLength: 100 
  })
  @IsString()
  @Length(1, 100)
  descripcion: string;

  @ApiProperty({ 
    description: 'Valor del tributo', 
    example: 13.50,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  valor: number;
}