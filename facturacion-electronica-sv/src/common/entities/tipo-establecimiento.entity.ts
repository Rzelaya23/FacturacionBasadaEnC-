import { Entity, PrimaryColumn, Column } from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';

@Entity('cat_009_tipo_establecimiento')
export class TipoEstablecimiento {
  @ApiProperty({ description: 'Código del tipo de establecimiento', example: '01' })
  @PrimaryColumn({ type: 'char', length: 2 })
  codigo: string;

  @ApiProperty({ description: 'Descripción del tipo de establecimiento', example: 'Casa matriz' })
  @Column({ type: 'varchar', length: 100 })
  descripcion: string;
}