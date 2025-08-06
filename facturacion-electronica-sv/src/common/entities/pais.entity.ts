import { Entity, PrimaryColumn, Column } from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';

@Entity('cat_020_pais')
export class Pais {
  @ApiProperty({ description: 'Código del país (ISO)', example: 'SV' })
  @PrimaryColumn({ type: 'varchar', length: 4 })
  codigo: string;

  @ApiProperty({ description: 'Nombre del país', example: 'El Salvador' })
  @Column({ type: 'varchar', length: 100 })
  nombre: string;
}