# 🇸🇻 Facturación Electrónica SV (NestJS) — Documento Final

Este documento consolida toda la información del proyecto en un único lugar: origen (proyecto C#), arquitectura actual (NestJS), estado de implementación, consideraciones especiales y plan de trabajo para fases futuras.

---

## 1) Origen: Proyecto base en C# y relación con este sistema

El sistema actual en NestJS es una re-implementación modernizada del proyecto C# (Windows Forms + librerías) incluido como referencia en el repositorio:

- FacturacionElectronica/FacturacionElectronica (WinForms)
  - Aplicación de escritorio que orquestaba la generación, firma y envío de DTEs.
- FacturacionElectronica/ConsumoFirmador
  - Librería para consumo de un servicio de firmado digital (uso de RestSharp/Json).
- FacturacionElectronica/Utilidades
  - Modelos y estructuras JSON específicas por tipo de DTE (FE, CCF, NC, ND, NR, FSE, FEX) y utilitarios (roles, catálogos, dirección, etc.).

Migración y equivalencias principales:
- UI de escritorio → API REST (NestJS)
- Clases .NET de estructuras JSON → DTOs TypeScript y validaciones con class-validator.
- Consumo de firmador (RestSharp) → Módulo `firmador` en NestJS con Axios.
- Consumo a MH (autenticación/recepción/consulta) → Módulo `mh-integration` en NestJS (servicio base y servicio optimizado).
- Persistencia y catálogos → TypeORM con PostgreSQL + scripts SQL incluidos.

Resultado: misma cobertura funcional (7 DTEs) con una arquitectura moderna, modular y desplegable en contenedores.

---

## 2) Estado actual (resumen ejecutivo)

- DTEs implementados (7/7): FE, CCF, NC, ND, NR, FSE, FEX.
- API y documentación: Swagger disponible en /api/docs.
- Salud y monitoreo: endpoints /health, /health/detailed, /health/metrics.
- Base de datos: PostgreSQL + TypeORM + scripts de catálogos/tabla.
- Integraciones: Ministerio de Hacienda (MH) y servicio de firmado (externo).
- Caché: CacheService en memoria (Redis disponible en infraestructura; ver Fase 3).
- Observabilidad: Prometheus/Grafana en docker-compose (app aún no expone /metrics Prometheus; ver Fase 3).
- Producción: Dockerfile multi-stage, docker-compose.production.yml, scripts de deploy/health.

---

## 3) Arquitectura y módulos

- common/
  - dto/: DTOs compartidos (identificación, emisor, receptor, tributos, dirección, etc.)
  - entities/: Entidades TypeORM (catálogos y DTE)
  - services/: CacheService, PerformanceService, LoggerService, HealthService
  - controllers/: HealthController (health, metrics)
  - filters/interceptors/: GlobalExceptionFilter, LoggingInterceptor
  - modules/: OptimizationModule (global)
- dte/
  - fe, ccf, nc, nd, nr, fse, fex/: Módulos por tipo de DTE (controller, service, dto)
- anulacion/, contingencia/, lotes/, reports/: Funcionalidades adicionales
- mh-integration/: Integración con MH (servicio base y optimizado)
- firmador/: Integración con servicio de firmado
- main.ts: bootstrap, CORS, validación global, Swagger (/api/docs)

---

## 4) DTEs implementados

- 01-FE — Factura Electrónica
- 03-CCF — Comprobante de Crédito Fiscal
- 04-NR — Nota de Remisión
- 05-NC — Nota de Crédito
- 06-ND — Nota de Débito
- 11-FEX — Factura de Exportación
- 14-FSE — Factura Sujeto Excluido

Cada módulo expone endpoints para:
- ejemplo/plantilla (cuando aplica)
- validar (validaciones de negocio MH)
- generar/firmar/enviar (según configuración y credenciales)

---

## 5) Integraciones externas

- Servicio de Firmado (SIGNER_URL):
  - Servicio HTTP externo que firma el documento DTE.
  - Requiere estar operativo antes de pruebas end-to-end.
- Ministerio de Hacienda (MH):
  - Autenticación, recepción de DTEs, anulación, lotes, contingencia y consultas.
  - Requiere credenciales y URLs correctas por ambiente (test/producción).

---

## 6) Requisitos previos

- Node.js >= 18, npm >= 8
- PostgreSQL >= 13
- Servicio de firmado activo (para pruebas de firma y envío)
- Credenciales del MH (test o producción)

---

## 7) Variables de entorno (clave)

Se acepta DB_DATABASE o DB_NAME (DB_DATABASE tiene prioridad). Ejemplo base:

```env
# Base de datos (PostgreSQL)
DB_HOST=localhost
DB_PORT=5432
DB_USERNAME=postgres
DB_PASSWORD=postgres
DB_DATABASE=facturacion_electronica
DB_NAME=facturacion_electronica

# Ministerio de Hacienda
MH_ENVIRONMENT=test  # test o production
MH_USER=tu_usuario_mh
MH_PASSWORD=tu_password_mh
# URLs por ambiente
MH_BASE_URL_TEST=https://apitest.dtes.mh.gob.sv
MH_AUTH_URL_TEST=https://apitest.dtes.mh.gob.sv/seguridad/auth
MH_RECEPTION_URL_TEST=https://apitest.dtes.mh.gob.sv/fesv/recepciondte
MH_CANCELLATION_URL_TEST=https://apitest.dtes.mh.gob.sv/fesv/anulardte
MH_BATCH_URL_TEST=https://apitest.dtes.mh.gob.sv/fesv/recepcionlote/
MH_BATCH_QUERY_URL_TEST=https://apitest.dtes.mh.gob.sv/fesv/recepcion/consultadtelote/
MH_CONTINGENCY_URL_TEST=https://apitest.dtes.mh.gob.sv/fesv/contingencia
MH_BASE_URL_PROD=https://api.dtes.mh.gob.sv
MH_AUTH_URL_PROD=https://api.dtes.mh.gob.sv/seguridad/auth
MH_RECEPTION_URL_PROD=https://api.dtes.mh.gob.sv/fesv/recepciondte
MH_CANCELLATION_URL_PROD=https://api.dtes.mh.gob.sv/fesv/anulardte
MH_BATCH_URL_PROD=https://api.dtes.mh.gob.sv/fesv/recepcionlote/
MH_BATCH_QUERY_URL_PROD=https://api.dtes.mh.gob.sv/fesv/recepcion/consultadtelote/
MH_CONTINGENCY_URL_PROD=https://api.dtes.mh.gob.sv/fesv/contingencia

# Firmador
SIGNER_URL=http://localhost:8113/firmardocumento/

# JWT
JWT_SECRET=tu_jwt_secret
JWT_EXPIRES_IN=24h

# App
PORT=3000
NODE_ENV=development

# Optimización (opcional)
CACHE_TTL=300000
LOG_LEVEL=info
REQUEST_TIMEOUT=30000
MAX_RETRIES=3
```

---

## 8) Puesta en marcha local (desarrollo)

```bash
npm install
cp .env.example .env  # ajustar valores

# Crear BD y cargar catálogos/tabla DTE
createdb facturacion_electronica
psql -d facturacion_electronica -f database/CATALOGOS POSTGRES.sql
psql -d facturacion_electronica -f database/CREATE_DTES_TABLE.sql

# Iniciar en desarrollo
npm run start:dev
```

Verificación rápida:
- API: http://localhost:3000
- Swagger: http://localhost:3000/api/docs
- Health: http://localhost:3000/health

---

## 9) Testing

- Unit tests y coverage:
  - `npm test` (coverage mínimo global configurado al 70%)
- E2E:
  - `npm run test:e2e`
- Script auxiliar de endpoints (opcional):
  - `./test-endpoints.sh` (muestra ejemplos de llamadas a algunos endpoints)

---

## 10) Despliegue en producción (Docker)

- Archivos clave:
  - `Dockerfile.production` (multi-stage, no-root user, healthcheck)
  - `docker-compose.production.yml` (API + Postgres + Redis + Nginx + Prometheus + Grafana)
  - `scripts/deploy-production.sh` (build/up, health, logs)

Pasos típicos:
```bash
export DB_PASSWORD=... MH_BASE_URL=... MH_USER=... MH_PASSWORD=... \
FIRMADOR_URL=... FIRMADOR_TOKEN=...

bash scripts/deploy-production.sh
```

Notas:
- Nginx: el compose referencia `./nginx/nginx.conf`. Debe proveerse un archivo de configuración acorde a tu entorno (puedo añadir una plantilla si lo requieres).
- Redis: se levanta como servicio, pero la app actualmente usa cache en memoria (ver Fase 3 para integración real con Redis).

---

## 11) Observabilidad y monitoreo

- Health y métricas básicas en JSON:
  - `/health`, `/health/detailed`, `/health/metrics`
- Prometheus/Grafana (opcional):
  - Infraestructura incluida en docker-compose; la app aún no expone `/metrics` en formato Prometheus (ver Fase 3).

---

## 12) Consideraciones especiales y limitaciones actuales

- Firmado y MH:
  - Endpoints de firma y envío requieren `SIGNER_URL` activo y credenciales MH válidas.
- Base de datos:
  - Cargar catálogos y crear tabla DTE con los scripts de `database/` antes de pruebas end-to-end.
- Cache:
  - CacheService en memoria (apto para desarrollo o instancias únicas). Para alta disponibilidad/escala, integrar Redis (Fase 3).
- Swagger:
  - Unificado en `/api/docs` en código, scripts y documentación.
- Variables de BD:
  - Se soporta `DB_DATABASE` o `DB_NAME` (prioridad a `DB_DATABASE`).
- Nginx:
  - Se debe proporcionar `nginx.conf` (reverse proxy/SSL/headers) cuando se use el servicio Nginx del compose.

---

## 13) Plan de trabajo sugerido — Fase 3 (próximos pasos)

1) Cache distribuido con Redis
- Integrar `ioredis` o `@nestjs/cache-manager` con store Redis.
- Namespacing, TTLs y reconexión; conmutación por entorno (dev: memoria, prod: Redis).

2) Observabilidad avanzada
- Añadir `prom-client` y exponer `/metrics` (Prometheus).
- Dashboards base en Grafana (HTTP latencias, error rate, GC, event loop).

3) Seguridad y performance
- `helmet`, `@nestjs/throttler` (rate limiting), compresión.
- CORS configurable por entorno.

4) CI/CD y despliegue
- Pipeline (lint/tests/coverage, build Docker y push a registry, deploy).
- Plantilla `nginx.conf` y hardening de imágenes.

5) Migraciones de BD y documentación
- Flujo de migraciones con TypeORM.
- Guías de operación y runbooks (incidencias comunes, rotación de certificados, etc.).

---

## 14) Comandos útiles

```bash
# Desarrollo
npm run start:dev

# Build y producción local
npm run build && npm run start:prod

# Tests
npm test
npm run test:e2e

# Salud
curl http://localhost:3000/health
curl http://localhost:3000/health/detailed

# Documentación
open http://localhost:3000/api/docs
```

---

## 15) Conclusión

El sistema NestJS replica y mejora el alcance funcional del sistema C# original, entregando:
- Cobertura completa de DTEs (7/7)
- Arquitectura modular, escalable y lista para contenedores
- Documentación, validaciones, salud y herramientas de despliegue

Con la Fase 3 (Redis, métricas Prometheus, seguridad reforzada y CI/CD), el sistema quedará endurecido para operación productiva a gran escala.
