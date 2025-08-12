# 🇸🇻 Sistema de Facturación Electrónica El Salvador

[![NestJS](https://img.shields.io/badge/NestJS-E0234E?style=for-the-badge&logo=nestjs&logoColor=white)](https://nestjs.com/)
[![TypeScript](https://img.shields.io/badge/TypeScript-007ACC?style=for-the-badge&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)

## 📁 **ESTRUCTURA DEL REPOSITORIO**

```
📦 FacturacionBasadaEnC-
├── 🚀 facturacion-electronica-sv/     # Sistema NestJS (PRINCIPAL)
│   ├── src/                           # Código fuente
│   ├── test/                          # Tests E2E
│   ├── database/                      # Scripts SQL
│   └── README.md                      # Documentación completa
├── 📚 FacturacionElectronica/         # Sistema C# (REFERENCIA)
│   ├── ConsumoFirmador/               # Lógica de firmado
│   ├── Utilidades/                    # Estructuras JSON MH
│   └── FacturacionElectronica/        # App principal C#
└── 📖 docs/                           # Documentación adicional
```

## 🚀 **INICIO RÁPIDO**

### **1. Clonar Repositorio**
```bash
git clone https://github.com/Rzelaya23/FacturacionBasadaEnC-.git
cd FacturacionBasadaEnC-/facturacion-electronica-sv
```

### **2. Instalar Dependencias**
```bash
npm install
```

### **3. Configurar Variables de Entorno**
```bash
cp .env.example .env
# Editar .env con tus configuraciones
```

### **4. Ejecutar en Desarrollo**
```bash
npm run start:dev
```

### **5. Acceder a la API**
- **API**: http://localhost:3000
- **Swagger**: http://localhost:3000/api/docs

## 📊 **ESTADO DEL PROYECTO**

| Componente | Estado | Descripción |
|------------|--------|-------------|
| **API REST** | ✅ 100% | 7 DTEs implementados |
| **Validaciones MH** | ✅ 100% | Cumple normativa salvadoreña |
| **Base de Datos** | ✅ 100% | PostgreSQL + TypeORM |
| **Testing** | ✅ 85% | Tests unitarios principales |
| **Documentación** | ✅ 100% | Swagger + README completo |
| **Firmado Digital** | ⚙️ Config | Listo para certificados |
| **Integración MH** | ⚙️ Config | Listo para credenciales |

## 🛠️ **CONFIGURACIÓN PARA PRODUCCIÓN**

### **Requisitos:**
1. **Certificados digitales** del MH
2. **Credenciales productivas** del MH
3. **Base de datos PostgreSQL**
4. **Servidor de firmado** (opcional)

### **Variables de Entorno Necesarias:**
```bash
# Base de datos (PostgreSQL)
DB_HOST=localhost
DB_PORT=5432
DB_USERNAME=postgres
DB_PASSWORD=password
# Se aceptan DB_DATABASE o DB_NAME (DB_DATABASE tiene prioridad)
DB_DATABASE=facturacion_electronica
DB_NAME=facturacion_electronica

# Ministerio de Hacienda
MH_BASE_URL=https://apitest.dtes.mh.gob.sv/fesv/
MH_NIT_EMISOR=tu-nit
MH_PASSWORD_PRIVATE_KEY=tu-password

# Firmador Digital
FIRMADOR_URL=https://tu-firmador.com
FIRMADOR_TOKEN=tu-token
```

## 📚 **DOCUMENTACIÓN**

- **[Documento Final](facturacion-electronica-sv/README_FINAL.md)** - Documentación completa y consolidada

## 🤝 **CONTRIBUCIÓN**

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📄 **LICENCIA**

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para más detalles.

## 📞 **CONTACTO**

- **Desarrollador**: Tu Nombre
- **Email**: tu-email@ejemplo.com
- **Proyecto**: [https://github.com/Rzelaya23/FacturacionBasadaEnC-](https://github.com/Rzelaya23/FacturacionBasadaEnC-)

---

⭐ **¡Dale una estrella si este proyecto te ayudó!**