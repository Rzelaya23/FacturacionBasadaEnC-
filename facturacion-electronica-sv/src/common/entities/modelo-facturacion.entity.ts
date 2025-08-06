import { Entity, PrimaryColumn, Column } from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';

@Entity('cat_003_modelo_facturacion')
export class ModeloFacturacion {
  @ApiProperty({ description: 'Código del modelo (1=Previo, 2=Diferido)', example: '1' })
  @PrimaryColumn({ type: 'char', length: 1 })
  codigo: string;

  @ApiProperty({ description: 'Descripción del modelo', example: 'Modelo Facturación previo' })
  @Column({ type: 'varchar', length: 100 })
  descripcion: string;
}