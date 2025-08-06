import { Module } from '@nestjs/common';
import { FeModule } from './fe/fe.module';
import { CcfModule } from './ccf/ccf.module';
import { NcModule } from './nc/nc.module';
import { NdModule } from './nd/nd.module';
import { FseModule } from './fse/fse.module';
import { FexModule } from './fex/fex.module';
import { NrModule } from './nr/nr.module';

@Module({
  imports: [
    FeModule,
    CcfModule,
    NcModule,
    NdModule,
    FseModule,
    FexModule,
    NrModule,
  ],
  exports: [
    FeModule,
    CcfModule,
    NcModule,
    NdModule,
    FseModule,
    FexModule,
    NrModule,
  ],
})
export class DteModule {}