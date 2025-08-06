import { Entity, PrimaryColumn, Column } from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';

@Entity('cat_005_tipo_contingencia')
export class TipoContingencia {
  @ApiProperty({ description: 'Código del tipo de contingencia', example: '1' })
  @PrimaryColumn({ type: 'char', length: 1 })
  codigo: string;

  @ApiProperty({ description: 'Descripción del tipo de contingencia', example: 'No disponibilidad de sistema del MH' })
  @Column({ type: 'text' })
  descripcion: string;
}