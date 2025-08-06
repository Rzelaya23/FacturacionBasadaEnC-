import { Entity, PrimaryGeneratedColumn, Column, CreateDateColumn, UpdateDateColumn, Index } from 'typeorm';

@Entity('dtes')
@Index(['codigoGeneracion'], { unique: true })
@Index(['numeroControl'], { unique: true })
@Index(['tipoDte', 'fechaEmision'])
export class Dte {
  @PrimaryGeneratedColumn()
  id: number;

  @Column({ name: 'codigo_generacion', type: 'varchar', length: 36, unique: true })
  codigoGeneracion: string;

  @Column({ name: 'numero_control', type: 'varchar', length: 31, unique: true })
  numeroControl: string;

  @Column({ name: 'tipo_dte', type: 'char', length: 2 })
  tipoDte: string;

  @Column({ name: 'fecha_emision', type: 'date' })
  fechaEmision: Date;

  @Column({ name: 'hora_emision', type: 'time' })
  horaEmision: string;

  @Column({ name: 'emisor_nit', type: 'varchar', length: 17 })
  emisorNit: string;

  @Column({ name: 'emisor_nombre', type: 'varchar', length: 250 })
  emisorNombre: string;

  @Column({ name: 'receptor_documento', type: 'varchar', length: 20, nullable: true })
  receptorDocumento: string;

  @Column({ name: 'receptor_nombre', type: 'varchar', length: 250, nullable: true })
  receptorNombre: string;

  @Column({ name: 'total_pagar', type: 'decimal', precision: 18, scale: 2 })
  totalPagar: number;

  @Column({ name: 'moneda', type: 'varchar', length: 3, default: 'USD' })
  moneda: string;

  @Column({ name: 'estado_mh', type: 'varchar', length: 20, default: 'PENDIENTE' })
  estadoMh: string;

  @Column({ name: 'sello_recibido', type: 'varchar', length: 50, nullable: true })
  selloRecibido: string;

  @Column({ name: 'documento_json', type: 'text' })
  documentoJson: string;

  @Column({ name: 'documento_firmado', type: 'text', nullable: true })
  documentoFirmado: string;

  @Column({ name: 'respuesta_mh', type: 'text', nullable: true })
  respuestaMh: string;

  @Column({ name: 'observaciones', type: 'text', nullable: true })
  observaciones: string;

  @CreateDateColumn({ name: 'created_at' })
  createdAt: Date;

  @UpdateDateColumn({ name: 'updated_at' })
  updatedAt: Date;
}