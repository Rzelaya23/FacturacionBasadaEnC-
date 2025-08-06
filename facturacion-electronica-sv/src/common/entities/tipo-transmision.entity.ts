import { Entity, PrimaryColumn, Column } from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';

@Entity('cat_004_tipo_transmision')
export class TipoTransmision {
  @ApiProperty({ description: 'Código del tipo (1=Normal, 2=Contingencia)', example: '1' })
  @PrimaryColumn({ type: 'char', length: 1 })
  codigo: string;

  @ApiProperty({ description: 'Descripción del tipo', example: 'Transmisión normal' })
  @Column({ type: 'varchar', length: 100 })
  descripcion: string;
}