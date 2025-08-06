import { Module } from '@nestjs/common';
import { CcfController } from './ccf.controller';
import { CcfService } from './ccf.service';
import { FirmadorModule } from '../../firmador/firmador.module';
import { MhIntegrationModule } from '../../mh-integration/mh-integration.module';
import { CommonModule } from '../../common/common.module';

@Module({
  imports: [
    CommonModule,
    FirmadorModule,
    MhIntegrationModule,
  ],
  controllers: [CcfController],
  providers: [CcfService],
  exports: [CcfService],
})
export class CcfModule {}