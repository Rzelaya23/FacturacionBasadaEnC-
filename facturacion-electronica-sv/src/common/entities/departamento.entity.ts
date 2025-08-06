import { Entity, PrimaryGeneratedColumn, Column, OneToMany } from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';
import { Municipio } from './municipio.entity';

@Entity('cat_departamento')
export class Departamento {
  @ApiProperty({ description: 'ID único del departamento' })
  @PrimaryGeneratedColumn()
  id: number;

  @ApiProperty({ description: 'Código del departamento según MH' })
  @Column({ type: 'varchar', length: 2, unique: true })
  codigo: string;

  @ApiProperty({ description: 'Nombre del departamento' })
  @Column({ type: 'varchar', length: 100 })
  nombre: string;

  @ApiProperty({ description: 'Descripción del departamento' })
  @Column({ type: 'text', nullable: true })
  descripcion: string;

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
  @OneToMany(() => Municipio, municipio => municipio.departamento)
  municipios: Municipio[];
}