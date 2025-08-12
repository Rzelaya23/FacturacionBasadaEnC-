#!/bin/bash

echo "🔍 Probando conexión a la base de datos..."

# Verificar si hay un .env configurado
if [ ! -f ".env" ]; then
    echo "❌ No existe archivo .env"
    echo "💡 Ejecuta: cp .env.database .env"
    exit 1
fi

# Extraer configuración de .env
DB_HOST=$(grep DB_HOST .env | cut -d '=' -f2)
DB_PORT=$(grep DB_PORT .env | cut -d '=' -f2)
DB_USERNAME=$(grep DB_USERNAME .env | cut -d '=' -f2)
DB_DATABASE=$(grep DB_DATABASE .env | cut -d '=' -f2)

echo "📊 Configuración detectada:"
echo "   Host: $DB_HOST:$DB_PORT"
echo "   Database: $DB_DATABASE"
echo "   User: $DB_USERNAME"
echo ""

# Probar conexión
echo "🔌 Probando conexión..."
if command -v psql &> /dev/null; then
    psql -h $DB_HOST -p $DB_PORT -U $DB_USERNAME -d $DB_DATABASE -c "SELECT 'Conexión exitosa!' as status;" 2>/dev/null
    if [ $? -eq 0 ]; then
        echo "✅ Conexión exitosa!"
        echo ""
        echo "📋 Tablas disponibles:"
        psql -h $DB_HOST -p $DB_PORT -U $DB_USERNAME -d $DB_DATABASE -c "\dt" 2>/dev/null
    else
        echo "❌ Error de conexión"
    fi
else
    echo "⚠️ psql no está instalado, pero puedes probar desde la app"
fi

echo ""
echo "🌐 También puedes verificar en Adminer: http://localhost:8080"