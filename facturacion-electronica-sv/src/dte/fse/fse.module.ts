import { Module } from '@nestjs/common';
import { FseController } from './fse.controller';
import { FseService } from './fse.service';
import { CommonModule } from '../../common/common.module';
import { FirmadorModule } from '../../firmador/firmador.module';
import { MhIntegrationModule } from '../../mh-integration/mh-integration.module';

@Module({
  imports: [
    CommonModule,
    FirmadorModule,
    MhIntegrationModule,
  ],
  controllers: [FseController],
  providers: [FseService],
  exports: [FseService],
})
export class FseModule {}