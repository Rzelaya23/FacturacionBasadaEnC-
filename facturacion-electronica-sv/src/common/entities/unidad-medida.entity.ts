import { Entity, PrimaryColumn, Column } from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';

@Entity('cat_014_unidad_medida')
export class UnidadMedida {
  @ApiProperty({ description: 'Código de la unidad de medida', example: '1' })
  @PrimaryColumn({ type: 'char', length: 2 })
  codigo: string;

  @ApiProperty({ description: 'Descripción de la unidad de medida', example: 'Unidad' })
  @Column({ type: 'varchar', length: 100 })
  descripcion: string;
}