#!/bin/bash

# Script para configurar la base de datos PostgreSQL
# Asegúrate de tener PostgreSQL instalado y corriendo

echo "🗄️ Configurando Base de Datos PostgreSQL para Facturación Electrónica SV"

# Variables de configuración
DB_NAME="facturacion_electronica"
DB_USER="postgres"
DB_HOST="localhost"
DB_PORT="5432"

echo "📊 Creando base de datos: $DB_NAME"

# Crear base de datos si no existe
psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d postgres -c "CREATE DATABASE $DB_NAME;" 2>/dev/null || echo "Base de datos ya existe"

echo "📋 Aplicando catálogos oficiales del MH..."
# Aplicar catálogos
psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f database/CATALOGOS\ POSTGRES.sql

echo "📑 Creando tabla de DTEs..."
# Crear tabla principal de DTEs
psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f database/CREATE_DTES_TABLE.sql

echo "✅ Base de datos configurada exitosamente!"
echo ""
echo "📝 Configuración para .env:"
echo "DB_HOST=localhost"
echo "DB_PORT=5432"
echo "DB_USERNAME=$DB_USER"
echo "DB_PASSWORD=tu_password"
echo "DB_DATABASE=$DB_NAME"
echo ""
echo "🔍 Para verificar:"
echo "psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c '\dt'"