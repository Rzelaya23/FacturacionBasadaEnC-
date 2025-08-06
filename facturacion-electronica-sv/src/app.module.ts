import { Module } from '@nestjs/common';
import { ConfigModule, ConfigService } from '@nestjs/config';
import { TypeOrmModule } from '@nestjs/typeorm';
import { AppController } from './app.controller';
import { AppService } from './app.service';

// Módulos de la aplicación
import { AuthModule } from './auth/auth.module';
import { EmisorModule } from './emisor/emisor.module';
import { ReceptorModule } from './receptor/receptor.module';
import { DteModule } from './dte/dte.module';
import { MhIntegrationModule } from './mh-integration/mh-integration.module';
import { FirmadorModule } from './firmador/firmador.module';
import { AnulacionModule } from './anulacion/anulacion.module';
import { ContingenciaModule } from './contingencia/contingencia.module';
import { LotesModule } from './lotes/lotes.module';
import { ReportsModule } from './reports/reports.module';
import { CommonModule } from './common/common.module';

@Module({
  imports: [
    // Configuración global
    ConfigModule.forRoot({
      isGlobal: true,
      envFilePath: '.env',
    }),

    // Base de datos
    TypeOrmModule.forRootAsync({
      imports: [ConfigModule],
      useFactory: (configService: ConfigService) => ({
        type: 'postgres',
        host: configService.get<string>('DB_HOST', 'localhost'),
        port: configService.get<number>('DB_PORT', 5432),
        username: configService.get<string>('DB_USERNAME', 'root'),
        password: configService.get<string>('DB_PASSWORD', ''),
        database: configService.get<string>('DB_NAME', 'facturacion_electronica'),
        entities: [__dirname + '/**/*.entity{.ts,.js}'],
        synchronize: false, // Deshabilitado temporalmente
        logging: configService.get('NODE_ENV') === 'development',
        timezone: 'Z',
      }),
      inject: [ConfigService],
    }),

    // Módulos de la aplicación
    CommonModule,
    AuthModule,
    EmisorModule,
    ReceptorModule,
    DteModule,
    MhIntegrationModule,
    FirmadorModule,
    AnulacionModule,
    ContingenciaModule,
    LotesModule,
    ReportsModule,
  ],
  controllers: [AppController],
  providers: [AppService],
})
export class AppModule {}