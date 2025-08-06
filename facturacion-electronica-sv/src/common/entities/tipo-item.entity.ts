import { Entity, PrimaryColumn, Column } from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';

@Entity('cat_011_tipo_item')
export class TipoItem {
  @ApiProperty({ description: 'Código del tipo de ítem', example: '1' })
  @PrimaryColumn({ type: 'char', length: 1 })
  codigo: string;

  @ApiProperty({ description: 'Descripción del tipo de ítem', example: 'Bienes' })
  @Column({ type: 'varchar', length: 100 })
  descripcion: string;
}