import { Entity, PrimaryGeneratedColumn, Column } from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';

@Entity('cat_tipo_documento')
export class TipoDocumento {
  @ApiProperty({ description: 'ID único del tipo de documento' })
  @PrimaryGeneratedColumn()
  id: number;

  @ApiProperty({ description: 'Código del tipo de documento según MH (01=FE, 03=CCF, 05=NC, etc.)' })
  @Column({ type: 'varchar', length: 2, unique: true })
  codigo: string;

  @ApiProperty({ description: 'Nombre del tipo de documento' })
  @Column({ type: 'varchar', length: 100 })
  nombre: string;

  @ApiProperty({ description: 'Descripción del tipo de documento' })
  @Column({ type: 'text', nullable: true })
  descripcion: string;

  @ApiProperty({ description: 'Siglas del documento (FE, CCF, NC, ND, NR, FSE, FEX)' })
  @Column({ type: 'varchar', length: 10 })
  siglas: string;

  @ApiProperty({ description: 'Estado activo/inactivo' })
  @Column({ type: 'boolean', default: true })
  activo: boolean;

  @ApiProperty({ description: 'Fecha de creación' })
  @Column({ type: 'timestamp', default: () => 'CURRENT_TIMESTAMP' })
  createdAt: Date;

  @ApiProperty({ description: 'Fecha de actualización' })
  @Column({ type: 'timestamp', default: () => 'CURRENT_TIMESTAMP', onUpdate: 'CURRENT_TIMESTAMP' })
  updatedAt: Date;
}