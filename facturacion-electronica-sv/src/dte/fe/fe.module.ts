import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { FeController } from './fe.controller';
import { FeService } from './fe.service';
import { FirmadorModule } from '../../firmador/firmador.module';
import { MhIntegrationModule } from '../../mh-integration/mh-integration.module';
import { CommonModule } from '../../common/common.module';
import { Dte } from '../../common/entities/dte.entity';

@Module({
  imports: [
    TypeOrmModule.forFeature([Dte]),
    CommonModule,
    FirmadorModule,
    MhIntegrationModule,
  ],
  controllers: [FeController],
  providers: [FeService],
  exports: [FeService],
})
export class FeModule {}