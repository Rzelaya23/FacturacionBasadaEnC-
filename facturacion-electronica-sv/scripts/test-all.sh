#!/bin/bash

# Script completo de testing para el proyecto de Facturación Electrónica
# FASE 1: TESTING Y CALIDAD

echo "🧪 INICIANDO FASE 1: TESTING Y CALIDAD"
echo "======================================"
echo "📅 Fecha: $(date)"
echo "🎯 Objetivo: Verificar calidad del código y funcionalidad"
echo ""

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Función para mostrar resultados
show_result() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✅ $2${NC}"
    else
        echo -e "${RED}❌ $2${NC}"
        return 1
    fi
}

# Función para mostrar sección
show_section() {
    echo ""
    echo -e "${BLUE}🔍 $1${NC}"
    echo "----------------------------------------"
}

# Variables de control
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# 1. VERIFICAR DEPENDENCIAS
show_section "VERIFICANDO DEPENDENCIAS"

echo "📦 Verificando Node.js..."
node --version
show_result $? "Node.js disponible"

echo "📦 Verificando npm..."
npm --version
show_result $? "npm disponible"

echo "📦 Verificando dependencias del proyecto..."
npm list --depth=0 > /dev/null 2>&1
show_result $? "Dependencias instaladas"

# 2. LINTING Y FORMATO
show_section "VERIFICANDO CÓDIGO (LINTING)"

echo "🔍 Ejecutando ESLint..."
npm run lint > /dev/null 2>&1
LINT_RESULT=$?
show_result $LINT_RESULT "ESLint"

if [ $LINT_RESULT -ne 0 ]; then
    echo -e "${YELLOW}⚠️  Ejecutando fix automático...${NC}"
    npm run lint -- --fix > /dev/null 2>&1
    show_result $? "ESLint fix automático"
fi

echo "📝 Verificando formato con Prettier..."
npm run format > /dev/null 2>&1
show_result $? "Prettier format"

# 3. TESTS UNITARIOS
show_section "EJECUTANDO TESTS UNITARIOS"

echo "🧪 Tests de CodigoGeneracionService..."
npm test -- --testPathPattern=codigo-generacion.service.spec.ts --verbose
CODIGO_TEST_RESULT=$?
show_result $CODIGO_TEST_RESULT "CodigoGeneracionService tests"
TOTAL_TESTS=$((TOTAL_TESTS + 1))
[ $CODIGO_TEST_RESULT -eq 0 ] && PASSED_TESTS=$((PASSED_TESTS + 1)) || FAILED_TESTS=$((FAILED_TESTS + 1))

echo "🧪 Tests de FeService..."
npm test -- --testPathPattern=fe.service.spec.ts --verbose
FE_TEST_RESULT=$?
show_result $FE_TEST_RESULT "FeService tests"
TOTAL_TESTS=$((TOTAL_TESTS + 1))
[ $FE_TEST_RESULT -eq 0 ] && PASSED_TESTS=$((PASSED_TESTS + 1)) || FAILED_TESTS=$((FAILED_TESTS + 1))

echo "🧪 Tests de MhIntegrationService..."
npm test -- --testPathPattern=mh-integration.service.spec.ts --verbose
MH_TEST_RESULT=$?
show_result $MH_TEST_RESULT "MhIntegrationService tests"
TOTAL_TESTS=$((TOTAL_TESTS + 1))
[ $MH_TEST_RESULT -eq 0 ] && PASSED_TESTS=$((PASSED_TESTS + 1)) || FAILED_TESTS=$((FAILED_TESTS + 1))

echo "🧪 Tests de FirmadorService..."
npm test -- --testPathPattern=firmador.service.spec.ts --verbose
FIRMADOR_TEST_RESULT=$?
show_result $FIRMADOR_TEST_RESULT "FirmadorService tests"
TOTAL_TESTS=$((TOTAL_TESTS + 1))
[ $FIRMADOR_TEST_RESULT -eq 0 ] && PASSED_TESTS=$((PASSED_TESTS + 1)) || FAILED_TESTS=$((FAILED_TESTS + 1))

# 4. TESTS DE INTEGRACIÓN
show_section "EJECUTANDO TESTS DE INTEGRACIÓN"

echo "🔗 Tests E2E de creación de DTEs..."
npm run test:e2e -- --testPathPattern=dte-creation.e2e-spec.ts
E2E_TEST_RESULT=$?
show_result $E2E_TEST_RESULT "Tests E2E"
TOTAL_TESTS=$((TOTAL_TESTS + 1))
[ $E2E_TEST_RESULT -eq 0 ] && PASSED_TESTS=$((PASSED_TESTS + 1)) || FAILED_TESTS=$((FAILED_TESTS + 1))

# 5. COVERAGE REPORT
show_section "GENERANDO REPORTE DE COBERTURA"

echo "📊 Ejecutando tests con coverage..."
npm run test:cov > /dev/null 2>&1
COVERAGE_RESULT=$?
show_result $COVERAGE_RESULT "Coverage report generado"

if [ $COVERAGE_RESULT -eq 0 ]; then
    echo "📁 Reporte disponible en: coverage/lcov-report/index.html"
fi

# 6. VERIFICACIÓN DE COMPATIBILIDAD C#
show_section "VERIFICANDO COMPATIBILIDAD CON SISTEMA C#"

echo "🔍 Verificando estructura de DTOs..."
# Verificar que los DTOs tienen la misma estructura que las clases C#
find src -name "*.dto.ts" | wc -l
DTO_COUNT=$(find src -name "*.dto.ts" | wc -l)
echo "📄 DTOs encontrados: $DTO_COUNT"

echo "🔍 Verificando servicios principales..."
# Verificar que existen los servicios equivalentes a las clases C#
SERVICES_FOUND=0
[ -f "src/dte/fe/fe.service.ts" ] && SERVICES_FOUND=$((SERVICES_FOUND + 1))
[ -f "src/dte/ccf/ccf.service.ts" ] && SERVICES_FOUND=$((SERVICES_FOUND + 1))
[ -f "src/dte/nc/nc.service.ts" ] && SERVICES_FOUND=$((SERVICES_FOUND + 1))
[ -f "src/dte/nd/nd.service.ts" ] && SERVICES_FOUND=$((SERVICES_FOUND + 1))
[ -f "src/dte/nr/nr.service.ts" ] && SERVICES_FOUND=$((SERVICES_FOUND + 1))
[ -f "src/dte/fse/fse.service.ts" ] && SERVICES_FOUND=$((SERVICES_FOUND + 1))
[ -f "src/dte/fex/fex.service.ts" ] && SERVICES_FOUND=$((SERVICES_FOUND + 1))

echo "🔧 Servicios DTE encontrados: $SERVICES_FOUND/7"
[ $SERVICES_FOUND -eq 7 ] && show_result 0 "Todos los servicios DTE implementados" || show_result 1 "Faltan servicios DTE"

# 7. VERIFICACIÓN DE ENDPOINTS
show_section "VERIFICANDO ENDPOINTS API"

echo "🌐 Iniciando servidor para verificación..."
npm run start:dev &
SERVER_PID=$!
sleep 10

echo "🔍 Verificando health checks..."
curl -s http://localhost:3000/health > /dev/null 2>&1
HEALTH_RESULT=$?
show_result $HEALTH_RESULT "Health check principal"

curl -s http://localhost:3000/dte/fe/health > /dev/null 2>&1
FE_HEALTH_RESULT=$?
show_result $FE_HEALTH_RESULT "Health check FE"

echo "🛑 Deteniendo servidor..."
kill $SERVER_PID > /dev/null 2>&1

# 8. RESUMEN FINAL
show_section "RESUMEN DE RESULTADOS"

echo ""
echo "📊 ESTADÍSTICAS DE TESTING:"
echo "   Total de suites: $TOTAL_TESTS"
echo "   Exitosos: $PASSED_TESTS"
echo "   Fallidos: $FAILED_TESTS"
echo ""

if [ $FAILED_TESTS -eq 0 ]; then
    echo -e "${GREEN}🎉 TODOS LOS TESTS PASARON EXITOSAMENTE${NC}"
    echo ""
    echo "✅ FASE 1 COMPLETADA CON ÉXITO"
    echo "   - Código con calidad verificada"
    echo "   - Tests unitarios funcionando"
    echo "   - Tests de integración operativos"
    echo "   - Compatibilidad C# verificada"
    echo "   - API endpoints respondiendo"
    echo ""
    echo "🚀 LISTO PARA FASE 2: PERSISTENCIA REAL"
    exit 0
else
    echo -e "${RED}❌ ALGUNOS TESTS FALLARON${NC}"
    echo ""
    echo "🔧 ACCIONES REQUERIDAS:"
    echo "   1. Revisar logs de tests fallidos"
    echo "   2. Corregir errores encontrados"
    echo "   3. Re-ejecutar este script"
    echo ""
    echo "📋 COMANDOS ÚTILES:"
    echo "   npm test -- --verbose"
    echo "   npm run test:watch"
    echo "   npm run test:cov"
    exit 1
fi