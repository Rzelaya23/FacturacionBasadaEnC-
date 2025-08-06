import { Entity, PrimaryColumn, Column } from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';

@Entity('cat_001_ambiente_destino')
export class AmbienteDestino {
  @ApiProperty({ description: 'Código del ambiente (00=Prueba, 01=Producción)', example: '00' })
  @PrimaryColumn({ type: 'char', length: 2 })
  codigo: string;

  @ApiProperty({ description: 'Descripción del ambiente', example: 'Modo prueba' })
  @Column({ type: 'varchar', length: 50 })
  descripcion: string;
}