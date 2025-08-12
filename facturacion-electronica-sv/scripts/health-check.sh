#!/bin/bash

# =================================
# SCRIPT DE HEALTH CHECK COMPLETO
# =================================

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuración
API_URL="http://localhost:3000"
TIMEOUT=10

# Función para logging
log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')] $1${NC}"
}

warn() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] WARNING: $1${NC}"
}

error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR: $1${NC}"
}

success() {
    echo -e "${GREEN}✅ $1${NC}"
}

fail() {
    echo -e "${RED}❌ $1${NC}"
}

# Función para hacer requests con timeout
make_request() {
    local url=$1
    local timeout=${2:-$TIMEOUT}
    
    curl -s --max-time $timeout "$url" 2>/dev/null
}

# Función para verificar JSON response
check_json_status() {
    local response=$1
    local expected_status=${2:-"healthy"}
    
    if echo "$response" | jq -e ".status == \"$expected_status\"" > /dev/null 2>&1; then
        return 0
    else
        return 1
    fi
}

echo "🏥 Ejecutando Health Check Completo..."
echo "========================================"

# 1. Health Check Básico
log "1. Verificando health check básico..."
basic_health=$(make_request "$API_URL/health")

if [ $? -eq 0 ] && [ -n "$basic_health" ]; then
    if echo "$basic_health" | jq -e '.status == "ok"' > /dev/null 2>&1; then
        success "Health check básico: OK"
        uptime=$(echo "$basic_health" | jq -r '.uptime // "N/A"')
        version=$(echo "$basic_health" | jq -r '.version // "N/A"')
        echo "  - Uptime: ${uptime}s"
        echo "  - Version: $version"
    else
        fail "Health check básico: FAIL"
        echo "$basic_health"
    fi
else
    fail "Health check básico: NO RESPONSE"
fi

echo ""

# 2. Health Check Detallado
log "2. Verificando health check detallado..."
detailed_health=$(make_request "$API_URL/health/detailed" 15)

if [ $? -eq 0 ] && [ -n "$detailed_health" ]; then
    overall_status=$(echo "$detailed_health" | jq -r '.status // "unknown"')
    
    if [ "$overall_status" = "healthy" ]; then
        success "Health check detallado: HEALTHY"
    elif [ "$overall_status" = "unhealthy" ]; then
        fail "Health check detallado: UNHEALTHY"
    else
        warn "Health check detallado: $overall_status"
    fi
    
    # Verificar componentes individuales
    echo "  Componentes:"
    
    # Base de datos
    db_status=$(echo "$detailed_health" | jq -r '.checks.database.status // "unknown"')
    if [ "$db_status" = "healthy" ]; then
        success "  - Base de datos: $db_status"
        db_time=$(echo "$detailed_health" | jq -r '.checks.database.responseTime // "N/A"')
        echo "    Response time: ${db_time}ms"
    else
        fail "  - Base de datos: $db_status"
    fi
    
    # MH
    mh_status=$(echo "$detailed_health" | jq -r '.checks.mh.status // "unknown"')
    if [ "$mh_status" = "healthy" ]; then
        success "  - Ministerio de Hacienda: $mh_status"
    elif [ "$mh_status" = "warning" ]; then
        warn "  - Ministerio de Hacienda: $mh_status"
    else
        fail "  - Ministerio de Hacienda: $mh_status"
    fi
    
    # Firmador
    firmador_status=$(echo "$detailed_health" | jq -r '.checks.firmador.status // "unknown"')
    if [ "$firmador_status" = "healthy" ]; then
        success "  - Firmador: $firmador_status"
    elif [ "$firmador_status" = "warning" ]; then
        warn "  - Firmador: $firmador_status"
    else
        fail "  - Firmador: $firmador_status"
    fi
    
    # Memoria
    memory_status=$(echo "$detailed_health" | jq -r '.checks.memory.status // "unknown"')
    if [ "$memory_status" = "healthy" ]; then
        success "  - Memoria: $memory_status"
    else
        warn "  - Memoria: $memory_status"
    fi
    
else
    fail "Health check detallado: NO RESPONSE"
fi

echo ""

# 3. Métricas de Performance
log "3. Verificando métricas de performance..."
metrics=$(make_request "$API_URL/health/metrics" 10)

if [ $? -eq 0 ] && [ -n "$metrics" ]; then
    success "Métricas: DISPONIBLES"
    
    # Cache stats
    cache_size=$(echo "$metrics" | jq -r '.cache.size // "N/A"')
    echo "  - Cache entries: $cache_size"
    
    # Memory usage
    memory_used=$(echo "$metrics" | jq -r '.memory.heapUsed // "N/A"')
    echo "  - Memory used: ${memory_used}MB"
    
    # Uptime
    uptime=$(echo "$metrics" | jq -r '.uptime // "N/A"')
    echo "  - Uptime: ${uptime}s"
    
else
    fail "Métricas: NO DISPONIBLES"
fi

echo ""

# 4. Test de Endpoints Críticos
log "4. Verificando endpoints críticos..."

# Swagger docs
swagger_response=$(make_request "$API_URL/api/docs" 5)
if [ $? -eq 0 ]; then
    success "Swagger docs: DISPONIBLE"
else
    fail "Swagger docs: NO DISPONIBLE"
fi

# Health endpoints específicos
endpoints=("database" "mh" "firmador" "system")

for endpoint in "${endpoints[@]}"; do
    response=$(make_request "$API_URL/health/$endpoint" 8)
    if [ $? -eq 0 ] && [ -n "$response" ]; then
        status=$(echo "$response" | jq -r '.status // "unknown"')
        if [ "$status" = "healthy" ]; then
            success "Health /$endpoint: $status"
        else
            warn "Health /$endpoint: $status"
        fi
    else
        fail "Health /$endpoint: NO RESPONSE"
    fi
done

echo ""

# 5. Verificar Docker Containers (si aplica)
log "5. Verificando containers Docker..."
if command -v docker-compose &> /dev/null; then
    if [ -f "docker-compose.production.yml" ]; then
        container_status=$(docker-compose -f docker-compose.production.yml ps --services --filter "status=running" 2>/dev/null)
        if [ -n "$container_status" ]; then
            success "Containers Docker: RUNNING"
            echo "$container_status" | while read service; do
                echo "  - $service: running"
            done
        else
            warn "Containers Docker: NO RUNNING"
        fi
    else
        warn "docker-compose.production.yml no encontrado"
    fi
else
    warn "Docker Compose no disponible"
fi

echo ""

# Resumen final
echo "========================================"
log "Health Check Completado"

# Determinar estado general
if curl -s --max-time 5 "$API_URL/health" | jq -e '.status == "ok"' > /dev/null 2>&1; then
    echo -e "${GREEN}🎉 SISTEMA SALUDABLE${NC}"
    exit 0
else
    echo -e "${RED}⚠️  SISTEMA CON PROBLEMAS${NC}"
    exit 1
fi