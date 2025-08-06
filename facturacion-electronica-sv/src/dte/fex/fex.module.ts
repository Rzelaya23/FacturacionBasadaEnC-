import { Module } from '@nestjs/common';
import { FexController } from './fex.controller';
import { FexService } from './fex.service';
import { CommonModule } from '../../common/common.module';
import { FirmadorModule } from '../../firmador/firmador.module';
import { MhIntegrationModule } from '../../mh-integration/mh-integration.module';

@Module({
  imports: [
    CommonModule,
    FirmadorModule,
    MhIntegrationModule,
  ],
  controllers: [FexController],
  providers: [FexService],
  exports: [FexService],
})
export class FexModule {}