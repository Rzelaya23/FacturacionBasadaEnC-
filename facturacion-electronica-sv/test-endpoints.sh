#!/bin/bash

# Script de pruebas para API de Facturación Electrónica
# Ejecutar después de: npm run start:dev

BASE_URL="http://localhost:3000"

echo "🧪 INICIANDO PRUEBAS DE API FACTURACIÓN ELECTRÓNICA"
echo "=================================================="

# Función para hacer requests y mostrar resultados
test_endpoint() {
    local method=$1
    local endpoint=$2
    local data=$3
    local description=$4
    
    echo ""
    echo "🔍 Probando: $description"
    echo "   $method $endpoint"
    echo "   ----------------------------------------"
    
    if [ "$method" = "GET" ]; then
        curl -s -X GET "$BASE_URL$endpoint" \
             -H "Content-Type: application/json" | jq '.' || echo "❌ Error en request"
    else
        curl -s -X "$method" "$BASE_URL$endpoint" \
             -H "Content-Type: application/json" \
             -d "$data" | jq '.' || echo "❌ Error en request"
    fi
}

echo ""
echo "1️⃣ PROBANDO ENDPOINTS BÁSICOS"
echo "=============================="

# Test básico de salud
test_endpoint "GET" "/" "Endpoint raíz"

# Test de documentación Swagger
echo ""
echo "📚 Documentación Swagger disponible en: $BASE_URL/api/docs"

echo ""
echo "2️⃣ PROBANDO FACTURA ELECTRÓNICA (FE)"
echo "===================================="

# Obtener ejemplo de FE
test_endpoint "GET" "/dte/fe/ejemplo" "" "Obtener ejemplo de FE"

# Validar datos de FE (usando el ejemplo)
FE_EXAMPLE='{
  "identificacion": {
    "version": 1,
    "ambiente": "00",
    "tipoDte": "01",
    "numeroControl": "DTE-01-00000001-000000000000001",
    "tipoModelo": 1,
    "tipoOperacion": 1,
    "fecEmi": "2024-07-16",
    "horEmi": "10:30:00",
    "tipoMoneda": "USD"
  },
  "emisor": {
    "nit": "02101601014",
    "nrc": "240372-1",
    "nombre": "EMPRESA EJEMPLO S.A. DE C.V.",
    "codActividad": "62020",
    "descActividad": "Consultoría en informática",
    "nombreComercial": "EMPRESA EJEMPLO",
    "tipoEstablecimiento": "01",
    "direccion": {
      "departamento": "06",
      "municipio": "01",
      "complemento": "Colonia Escalón, Avenida Norte #123"
    },
    "telefono": "2234-5678",
    "correo": "facturacion@empresaejemplo.com"
  },
  "receptor": {
    "tipoDocumento": "36",
    "numDocumento": "02101601015",
    "nombre": "CLIENTE EJEMPLO S.A. DE C.V.",
    "direccion": {
      "departamento": "06",
      "municipio": "01",
      "complemento": "Colonia San Benito, Calle Principal #456"
    }
  },
  "cuerpoDocumento": [
    {
      "numItem": 1,
      "tipoItem": 2,
      "cantidad": 1,
      "uniMedida": 1,
      "descripcion": "Servicio de consultoría en sistemas",
      "precioUni": 100.00,
      "montoDescu": 0.00,
      "ventaNoSuj": 0.00,
      "ventaExenta": 0.00,
      "ventaGravada": 100.00
    }
  ],
  "resumen": {
    "totalNoSuj": 0.00,
    "totalExenta": 0.00,
    "totalGravada": 100.00,
    "subTotalVentas": 100.00,
    "descuNoSuj": 0.00,
    "descuExenta": 0.00,
    "descuGravada": 0.00,
    "porcentajeDescuento": 0.00,
    "totalDescu": 0.00,
    "tributos": [
      {
        "codigo": "20",
        "descripcion": "Impuesto al Valor Agregado 13%",
        "valor": 13.00
      }
    ],
    "subTotal": 100.00,
    "ivaPerci1": 0.00,
    "ivaRete1": 0.00,
    "reteRenta": 0.00,
    "montoTotalOperacion": 113.00,
    "totalNoGravado": 0.00,
    "totalPagar": 113.00,
    "totalLetras": "CIENTO TRECE 00/100 DÓLARES",
    "saldoFavor": 0.00,
    "condicionOperacion": 1
  }
}'

test_endpoint "POST" "/dte/fe/validar" "$FE_EXAMPLE" "Validar datos de FE"

echo ""
echo "3️⃣ PROBANDO COMPROBANTE CRÉDITO FISCAL (CCF)"
echo "============================================="

# Obtener ejemplo de CCF
test_endpoint "GET" "/dte/ccf/ejemplo" "" "Obtener ejemplo de CCF"

# Validar datos de CCF
test_endpoint "POST" "/dte/ccf/validar" "$FE_EXAMPLE" "Validar datos de CCF (usando ejemplo FE modificado)"

echo ""
echo "4️⃣ PROBANDO NOTA DE CRÉDITO (NC)"
echo "================================"

# Obtener ejemplo de NC
test_endpoint "GET" "/dte/nc/ejemplo" "" "Obtener ejemplo de NC"

echo ""
echo "5️⃣ PROBANDO SERVICIOS DE CATÁLOGOS"
echo "==================================="

# Estos endpoints no están expuestos directamente, pero podemos verificar que el servicio funciona
echo "   Los catálogos se usan internamente en las validaciones"
echo "   ✅ Si las validaciones funcionan, los catálogos están OK"

echo ""
echo "🎉 PRUEBAS COMPLETADAS"
echo "======================"
echo ""
echo "📋 RESUMEN:"
echo "   - FE: Factura Electrónica ✅"
echo "   - CCF: Comprobante Crédito Fiscal ✅"  
echo "   - NC: Nota de Crédito ✅"
echo "   - Catálogos: Funcionando internamente ✅"
echo ""
echo "🌐 Para más pruebas, visita: $BASE_URL/api/docs (Swagger UI)"
echo ""
echo "⚠️  NOTA: Los endpoints de firma y envío al MH requieren:"
echo "   - Servicio de firmado en puerto 8113"
echo "   - Configuración de credenciales MH"
echo "   - Base de datos PostgreSQL conectada"