import { Entity, PrimaryColumn, Column } from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';

@Entity('cat_019_actividades_economicas')
export class ActividadEconomica {
  @ApiProperty({ description: 'Código de la actividad económica', example: '62020' })
  @PrimaryColumn({ type: 'varchar', length: 6 })
  codigo: string;

  @ApiProperty({ description: 'Descripción de la actividad económica', example: 'Consultoría en informática' })
  @Column({ type: 'varchar', length: 500 })
  descripcion: string;
}