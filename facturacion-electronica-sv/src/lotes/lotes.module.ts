import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { LotesController } from './lotes.controller';
import { LotesService } from './lotes.service';
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
  controllers: [LotesController],
  providers: [LotesService],
  exports: [LotesService],
})
export class LotesModule {}