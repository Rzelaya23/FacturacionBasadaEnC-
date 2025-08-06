import { Module } from '@nestjs/common';
import { FirmadorService } from './firmador.service';

@Module({
  providers: [FirmadorService],
  exports: [FirmadorService],
})
export class FirmadorModule {}