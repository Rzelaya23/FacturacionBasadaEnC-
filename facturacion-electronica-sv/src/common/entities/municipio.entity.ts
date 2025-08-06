import { Entity, PrimaryGeneratedColumn, Column, ManyToOne, JoinColumn } from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';
import { Departamento } from './departamento.entity';

@Entity('cat_municipio')
export class Municipio {
  @ApiProperty({ description: 'ID único del municipio' })
  @PrimaryGeneratedColumn()
  id: number;

  @ApiProperty({ description: 'Código del municipio según MH' })
  @Column({ type: 'varchar', length: 4, unique: true })
  codigo: string;

  @ApiProperty({ description: 'Nombre del municipio' })
  @Column({ type: 'varchar', length: 100 })
  nombre: string;

  @ApiProperty({ description: 'Descripción del municipio' })
  @Column({ type: 'text', nullable: true })
  descripcion: string;

  @ApiProperty({ description: 'ID del departamento' })
  @Column({ name: 'departamento_id' })
  departamentoId: number;

  @ApiProperty({ description: 'Estado activo/inactivo' })
  @Column({ type: 'boolean', default: true })
  activo: boolean;

  @ApiProperty({ description: 'Fecha de creación' })
  @Column({ type: 'timestamp', default: () => 'CURRENT_TIMESTAMP' })
  createdAt: Date;

  @ApiProperty({ description: 'Fecha de actualización' })
  @Column({ type: 'timestamp', default: () => 'CURRENT_TIMESTAMP', onUpdate: 'CURRENT_TIMESTAMP' })
  updatedAt: Date;

  // Relaciones
  @ManyToOne(() => Departamento, departamento => departamento.municipios)
  @JoinColumn({ name: 'departamento_id' })
  departamento: Departamento;
}