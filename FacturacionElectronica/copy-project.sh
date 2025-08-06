#!/bin/bash

# Script para copiar el proyecto de facturación electrónica
echo "🚀 Copiando proyecto de Facturación Electrónica..."

# Crear directorio destino
DEST_DIR="$HOME/facturacion-electronica-sv"
mkdir -p "$DEST_DIR"

# Copiar todos los archivos
cp -r facturacion-electronica-sv/* "$DEST_DIR/"

echo "✅ Proyecto copiado a: $DEST_DIR"
echo ""
echo "📋 Próximos pasos:"
echo "1. cd $DEST_DIR"
echo "2. npm install"
echo "3. cp .env.example .env"
echo "4. nano .env  # Configurar variables"
echo "5. npm run start:dev"
echo ""
echo "📚 Documentación: http://localhost:3000/api"