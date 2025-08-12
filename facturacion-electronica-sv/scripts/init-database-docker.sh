#!/bin/bash

echo "🐳 Configurando PostgreSQL con Docker"

# Verificar si Docker está instalado
if ! command -v docker &> /dev/null; then
    echo "❌ Docker no está instalado. Instálalo desde: https://docker.com"
    exit 1
fi

# Verificar si Docker Compose está disponible
if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
    echo "❌ Docker Compose no está disponible"
    exit 1
fi

echo "📦 Iniciando contenedor PostgreSQL..."

# Usar docker compose o docker-compose según disponibilidad
if docker compose version &> /dev/null; then
    DOCKER_COMPOSE="docker compose"
else
    DOCKER_COMPOSE="docker-compose"
fi

# Levantar contenedores
$DOCKER_COMPOSE -f docker-compose.db.yml up -d

echo "⏳ Esperando que PostgreSQL esté listo..."
sleep 10

# Verificar que la BD esté corriendo
if docker exec facturacion_electronica_db pg_isready -U postgres -d facturacion_electronica; then
    echo "✅ PostgreSQL configurado exitosamente!"
    echo ""
    echo "📊 Base de datos disponible en:"
    echo "   Host: localhost:5432"
    echo "   Database: facturacion_electronica"
    echo "   User: postgres"
    echo "   Password: postgres123"
    echo ""
    echo "🌐 Interfaz web Adminer disponible en:"
    echo "   http://localhost:8080"
    echo "   Sistema: PostgreSQL"
    echo "   Servidor: postgres"
    echo "   Usuario: postgres"
    echo "   Contraseña: postgres123"
    echo "   Base de datos: facturacion_electronica"
    echo ""
    echo "📝 Para usar en tu app, copia .env.database a .env"
    echo "cp .env.database .env"
else
    echo "❌ Error al configurar PostgreSQL"
    docker logs facturacion_electronica_db
fi