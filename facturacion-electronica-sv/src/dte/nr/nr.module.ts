import { Module } from '@nestjs/common';
import { NrController } from './nr.controller';
import { NrService } from './nr.service';
import { CommonModule } from '../../common/common.module';
import { FirmadorModule } from '../../firmador/firmador.module';
import { MhIntegrationModule } from '../../mh-integration/mh-integration.module';

@Module({
  imports: [
    CommonModule,
    FirmadorModule,
    MhIntegrationModule,
  ],
  controllers: [NrController],
  providers: [NrService],
  exports: [NrService],
})
export class NrModule {}