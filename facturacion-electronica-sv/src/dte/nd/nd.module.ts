import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { NdController } from './nd.controller';
import { NdService } from './nd.service';
import { Dte } from '../../common/entities/dte.entity';
import { CommonModule } from '../../common/common.module';
import { FirmadorModule } from '../../firmador/firmador.module';
import { MhIntegrationModule } from '../../mh-integration/mh-integration.module';

@Module({
  imports: [
    TypeOrmModule.forFeature([Dte]),
    CommonModule,
    FirmadorModule,
    MhIntegrationModule,
  ],
  controllers: [NdController],
  providers: [NdService],
  exports: [NdService],
})
export class NdModule {}