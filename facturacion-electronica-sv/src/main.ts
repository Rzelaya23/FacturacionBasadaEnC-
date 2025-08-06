import { NestFactory } from '@nestjs/core';
import { ValidationPipe } from '@nestjs/common';
import { SwaggerModule, DocumentBuilder } from '@nestjs/swagger';
import { AppModule } from './app.module';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  // Configurar CORS
  app.enableCors({
    origin: true,
    methods: 'GET,HEAD,PUT,PATCH,POST,DELETE,OPTIONS',
    credentials: true,
  });

  // Configurar validación global
  app.useGlobalPipes(
    new ValidationPipe({
      whitelist: true,
      forbidNonWhitelisted: true,
      transform: true,
      transformOptions: {
        enableImplicitConversion: true,
      },
    }),
  );

  // Configurar Swagger
  const config = new DocumentBuilder()
    .setTitle('Sistema de Facturación Electrónica El Salvador')
    .setDescription('API para el manejo de Documentos Tributarios Electrónicos (DTEs) según normativa del Ministerio de Hacienda de El Salvador')
    .setVersion('1.0')
    .addBearerAuth(
      {
        type: 'http',
        scheme: 'bearer',
        bearerFormat: 'JWT',
        name: 'JWT',
        description: 'Enter JWT token',
        in: 'header',
      },
      'JWT-auth',
    )
    .addTag('Autenticación', 'Endpoints para autenticación de usuarios')
    .addTag('Emisores', 'Gestión de emisores de DTEs')
    .addTag('Receptores', 'Gestión de receptores de DTEs')
    .addTag('Factura Electrónica (FE)', 'Gestión de Facturas Electrónicas')
    .addTag('Comprobante Crédito Fiscal (CCF)', 'Gestión de Comprobantes de Crédito Fiscal')
    .addTag('Nota de Crédito (NC)', 'Gestión de Notas de Crédito')
    .addTag('Nota de Débito (ND)', 'Gestión de Notas de Débito')
    .addTag('Nota de Remisión (NR)', 'Gestión de Notas de Remisión')
    .addTag('Factura Sujeto Excluido (FSE)', 'Gestión de Facturas de Sujeto Excluido')
    .addTag('Factura Exportación (FEX)', 'Gestión de Facturas de Exportación')
    .addTag('Anulación', 'Anulación de DTEs')
    .addTag('Contingencia', 'Manejo de contingencias')
    .addTag('Lotes', 'Procesamiento por lotes')
    .addTag('Reportes', 'Generación de reportes')
    .addTag('Ministerio Hacienda', 'Integración con APIs del MH')
    .build();

  const document = SwaggerModule.createDocument(app, config);
  SwaggerModule.setup('api', app, document, {
    swaggerOptions: {
      persistAuthorization: true,
    },
  });

  const port = process.env.PORT || 3000;
  await app.listen(port);
  
  console.log(`🚀 Aplicación ejecutándose en: http://localhost:${port}`);
  console.log(`📚 Documentación Swagger: http://localhost:${port}/api`);
}

bootstrap();