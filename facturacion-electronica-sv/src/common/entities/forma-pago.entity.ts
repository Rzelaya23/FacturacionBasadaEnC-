import { Entity, PrimaryColumn, Column } from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';

@Entity('cat_017_forma_pago')
export class FormaPago {
  @ApiProperty({ description: 'Código de la forma de pago', example: '01' })
  @PrimaryColumn({ type: 'char', length: 2 })
  codigo: string;

  @ApiProperty({ description: 'Descripción de la forma de pago', example: 'Efectivo' })
  @Column({ type: 'varchar', length: 100 })
  descripcion: string;
}