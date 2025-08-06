import { Module } from '@nestjs/common';
import { CodigoGeneracionService } from './services/codigo-generacion.service';
import { CatalogosService } from './services/catalogos.service';
import { TypeOrmModule } from '@nestjs/typeorm';
import {
  // Entidades originales
  Departamento,
  Municipio,
  TipoDocumento,
  // Nuevas entidades de catalogos
  AmbienteDestino,
  ModeloFacturacion,
  TipoTransmision,
  TipoContingencia,
  TipoItem,
  UnidadMedida,
  Tributo,
  CondicionOperacion,
  FormaPago,
  ActividadEconomica,
  Pais,
  TipoEstablecimiento
} from './entities';

@Module({
  imports: [
    TypeOrmModule.forFeature([
      // Entidades originales
      Departamento,
      Municipio,
      TipoDocumento,
      // Nuevas entidades de catalogos
      AmbienteDestino,
      ModeloFacturacion,
      TipoTransmision,
      TipoContingencia,
      TipoItem,
      UnidadMedida,
      Tributo,
      CondicionOperacion,
      FormaPago,
      ActividadEconomica,
      Pais,
      TipoEstablecimiento
    ]),
  ],
  providers: [
    CodigoGeneracionService,
    CatalogosService
  ],
  exports: [
    TypeOrmModule,
    CodigoGeneracionService,
    CatalogosService
  ],
})
export class CommonModule {}