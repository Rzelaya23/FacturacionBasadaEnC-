import { Entity, PrimaryColumn, Column } from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';

@Entity('cat_016_condicion_operacion')
export class CondicionOperacion {
  @ApiProperty({ description: 'Código de la condición', example: '1' })
  @PrimaryColumn({ type: 'char', length: 1 })
  codigo: string;

  @ApiProperty({ description: 'Descripción de la condición', example: 'Contado' })
  @Column({ type: 'varchar', length: 100 })
  descripcion: string;
}