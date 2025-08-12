import { DocumentBuilder, SwaggerModule } from '@nestjs/swagger';
import { INestApplication } from '@nestjs/common';

export function setupSwagger(app: INestApplication): void {
  const config = new DocumentBuilder()
    .setTitle('Facturación Electrónica El Salvador')
    .setDescription('API completa para facturación electrónica con 7 DTEs implementados')
    .setVersion('2.0.0')
    .addTag('Health & Monitoring', 'Endpoints de monitoreo y health checks')
    .addTag('DTEs', 'Documentos Tributarios Electrónicos')
    .addTag('MH Integration', 'Integración con Ministerio de Hacienda')
    .addBearerAuth()
    .build();

  const document = SwaggerModule.createDocument(app, config);
  SwaggerModule.setup('api/docs', app, document, {
    swaggerOptions: {
      persistAuthorization: true,
    },
  });
}
