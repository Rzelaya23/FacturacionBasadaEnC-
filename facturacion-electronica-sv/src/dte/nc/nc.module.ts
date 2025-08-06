import { Module } from '@nestjs/common';
import { NcController } from './nc.controller';
import { NcService } from './nc.service';
import { FirmadorModule } from '../../firmador/firmador.module';
import { MhIntegrationModule } from '../../mh-integration/mh-integration.module';
import { CommonModule } from '../../common/common.module';

@Module({
  imports: [
    CommonModule,
    FirmadorModule,
    MhIntegrationModule,
  ],
  controllers: [NcController],
  providers: [NcService],
  exports: [NcService],
})
export class NcModule {}