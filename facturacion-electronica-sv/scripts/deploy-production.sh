#!/bin/bash

# =================================
# SCRIPT DE DEPLOYMENT PRODUCCIÓN
# =================================

set -e  # Salir en caso de error

echo "🚀 Iniciando deployment de producción..."

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Función para logging
log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')] $1${NC}"
}

warn() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] WARNING: $1${NC}"
}

error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR: $1${NC}"
    exit 1
}

# Verificar que estamos en el directorio correcto
if [ ! -f "package.json" ]; then
    error "No se encontró package.json. Ejecutar desde el directorio raíz del proyecto."
fi

# Verificar variables de entorno requeridas
log "Verificando variables de entorno..."
required_vars=(
    "DB_PASSWORD"
    "MH_BASE_URL"
    "MH_USER"
    "MH_PASSWORD"
    "FIRMADOR_URL"
    "FIRMADOR_TOKEN"
)

for var in "${required_vars[@]}"; do
    if [ -z "${!var}" ]; then
        error "Variable de entorno requerida no encontrada: $var"
    fi
done

log "✅ Variables de entorno verificadas"

# Crear directorios necesarios
log "Creando directorios necesarios..."
mkdir -p logs
mkdir -p certificates
mkdir -p monitoring/grafana/dashboards
mkdir -p monitoring/grafana/datasources
mkdir -p nginx

log "✅ Directorios creados"

# Verificar Docker
log "Verificando Docker..."
if ! command -v docker &> /dev/null; then
    error "Docker no está instalado"
fi

if ! command -v docker-compose &> /dev/null; then
    error "Docker Compose no está instalado"
fi

log "✅ Docker verificado"

# Construir imágenes
log "Construyendo imágenes Docker..."
docker-compose -f docker-compose.production.yml build --no-cache

log "✅ Imágenes construidas"

# Detener servicios existentes
log "Deteniendo servicios existentes..."
docker-compose -f docker-compose.production.yml down

log "✅ Servicios detenidos"

# Ejecutar migraciones de base de datos
log "Ejecutando migraciones de base de datos..."
docker-compose -f docker-compose.production.yml up -d postgres
sleep 10

# Esperar a que PostgreSQL esté listo
log "Esperando a que PostgreSQL esté listo..."
timeout=60
while ! docker-compose -f docker-compose.production.yml exec postgres pg_isready -U postgres; do
    sleep 1
    timeout=$((timeout - 1))
    if [ $timeout -eq 0 ]; then
        error "PostgreSQL no está respondiendo"
    fi
done

log "✅ PostgreSQL listo"

# Iniciar todos los servicios
log "Iniciando todos los servicios..."
docker-compose -f docker-compose.production.yml up -d

log "✅ Servicios iniciados"

# Esperar a que la aplicación esté lista
log "Esperando a que la aplicación esté lista..."
timeout=120
while ! curl -f http://localhost:3000/health &> /dev/null; do
    sleep 2
    timeout=$((timeout - 2))
    if [ $timeout -eq 0 ]; then
        error "La aplicación no está respondiendo en /health"
    fi
done

log "✅ Aplicación lista"

# Verificar health checks
log "Verificando health checks..."
health_response=$(curl -s http://localhost:3000/health/detailed)
if echo "$health_response" | grep -q '"status":"healthy"'; then
    log "✅ Health checks pasando"
else
    warn "Health checks no están completamente saludables"
    echo "$health_response"
fi

# Mostrar estado de los servicios
log "Estado de los servicios:"
docker-compose -f docker-compose.production.yml ps

# Mostrar logs recientes
log "Logs recientes de la aplicación:"
docker-compose -f docker-compose.production.yml logs --tail=20 facturacion-api

# Información de endpoints
log "🎉 Deployment completado exitosamente!"
echo ""
echo "📋 Endpoints disponibles:"
echo "  - API Principal: http://localhost:3000"
echo "  - Health Check: http://localhost:3000/health"
echo "  - Swagger Docs: http://localhost:3000/api/docs"
echo "  - Métricas: http://localhost:3000/health/metrics"
echo ""
echo "📊 Monitoreo (opcional):"
echo "  - Prometheus: http://localhost:9090"
echo "  - Grafana: http://localhost:3001"
echo ""
echo "🗄️ Base de datos:"
echo "  - PostgreSQL: localhost:5432"
echo "  - Redis: localhost:6379"
echo ""
echo "📁 Logs disponibles en: ./logs/"
echo ""
log "Sistema listo para producción! 🚀"