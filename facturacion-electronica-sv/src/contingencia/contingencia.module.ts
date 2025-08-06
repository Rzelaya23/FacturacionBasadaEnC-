import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { ContingenciaController } from './contingencia.controller';
import { ContingenciaService } from './contingencia.service';
import { CommonModule } from '../common/common.module';
import { FirmadorModule } from '../firmador/firmador.module';
import { MhIntegrationModule } from '../mh-integration/mh-integration.module';
import { Dte } from '../common/entities/dte.entity';

@Module({
  imports: [
    TypeOrmModule.forFeature([Dte]),
    CommonModule,
    FirmadorModule,
    MhIntegrationModule,
  ],
  controllers: [ContingenciaController],
  providers: [ContingenciaService],
  exports: [ContingenciaService],
})
export class ContingenciaModule {}