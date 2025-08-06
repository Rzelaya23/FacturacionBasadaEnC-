import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { AnulacionController } from './anulacion.controller';
import { AnulacionService } from './anulacion.service';
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
  controllers: [AnulacionController],
  providers: [AnulacionService],
  exports: [AnulacionService],
})
export class AnulacionModule {}