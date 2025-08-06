import { Entity, PrimaryColumn, Column } from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';

@Entity('cat_015_tributos')
export class Tributo {
  @ApiProperty({ description: 'Código del tributo', example: '20' })
  @PrimaryColumn({ type: 'varchar', length: 2 })
  codigo: string;

  @ApiProperty({ description: 'Descripción del tributo', example: 'Impuesto al Valor Agregado 13%' })
  @Column({ type: 'varchar', length: 150 })
  descripcion: string;

  @ApiProperty({ description: 'Porcentaje del tributo', example: 13.00 })
  @Column({ type: 'decimal', precision: 5, scale: 2, nullable: true })
  porcentaje: number;

  @ApiProperty({ description: 'Tipo de tributo', example: 'IVA' })
  @Column({ type: 'varchar', length: 50, nullable: true })
  tipo: string;
}