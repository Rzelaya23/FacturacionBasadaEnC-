-- Crear tabla para almacenar DTEs
CREATE TABLE IF NOT EXISTS dtes (
    id SERIAL PRIMARY KEY,
    codigo_generacion VARCHAR(36) UNIQUE NOT NULL,
    numero_control VARCHAR(31) UNIQUE NOT NULL,
    tipo_dte CHAR(2) NOT NULL,
    fecha_emision DATE NOT NULL,
    hora_emision TIME NOT NULL,
    emisor_nit VARCHAR(17) NOT NULL,
    emisor_nombre VARCHAR(250) NOT NULL,
    receptor_documento VARCHAR(20),
    receptor_nombre VARCHAR(250),
    total_pagar DECIMAL(18,2) NOT NULL,
    moneda VARCHAR(3) DEFAULT 'USD',
    estado_mh VARCHAR(20) DEFAULT 'PENDIENTE',
    sello_recibido VARCHAR(50),
    documento_json TEXT NOT NULL,
    documento_firmado TEXT,
    respuesta_mh TEXT,
    observaciones TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Crear índices para mejorar performance
CREATE INDEX IF NOT EXISTS idx_dtes_codigo_generacion ON dtes(codigo_generacion);
CREATE INDEX IF NOT EXISTS idx_dtes_numero_control ON dtes(numero_control);
CREATE INDEX IF NOT EXISTS idx_dtes_tipo_fecha ON dtes(tipo_dte, fecha_emision);
CREATE INDEX IF NOT EXISTS idx_dtes_emisor_nit ON dtes(emisor_nit);
CREATE INDEX IF NOT EXISTS idx_dtes_estado_mh ON dtes(estado_mh);

-- Trigger para actualizar updated_at automáticamente
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_dtes_updated_at 
    BEFORE UPDATE ON dtes 
    FOR EACH ROW 
    EXECUTE FUNCTION update_updated_at_column();