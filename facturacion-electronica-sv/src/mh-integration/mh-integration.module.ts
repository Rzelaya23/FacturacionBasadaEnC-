import { Module } from '@nestjs/common';
import { MhIntegrationService } from './mh-integration.service';

@Module({
  providers: [MhIntegrationService],
  exports: [MhIntegrationService],
})
export class MhIntegrationModule {}