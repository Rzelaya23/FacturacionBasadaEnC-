acli -- PostgreSQL Database Schema
-- Converted from MySQL phpMyAdmin export
-- Database: db_signer

BEGIN;

-- Drop existing tables if they exist (in reverse dependency order)
DROP TABLE IF EXISTS cat_013_municipio CASCADE;
DROP TABLE IF EXISTS cat_032_domicilio_fiscal CASCADE;
DROP TABLE IF EXISTS cat_031_incoterms CASCADE;
DROP TABLE IF EXISTS cat_030_transporte CASCADE;
DROP TABLE IF EXISTS cat_029_tipo_persona CASCADE;
DROP TABLE IF EXISTS cat_028_regimen CASCADE;
DROP TABLE IF EXISTS cat_027_recinto_fiscal CASCADE;
DROP TABLE IF EXISTS cat_026_tipo_donacion CASCADE;
DROP TABLE IF EXISTS cat_025_titulo_remiten_bienes CASCADE;
DROP TABLE IF EXISTS cat_024_tipo_invalidacion CASCADE;
DROP TABLE IF EXISTS cat_023_tipo_doc_contingencia CASCADE;
DROP TABLE IF EXISTS cat_022_tipo_doc_ident_receptor CASCADE;
DROP TABLE IF EXISTS cat_021_otros_documentos_asociados CASCADE;
DROP TABLE IF EXISTS cat_020_pais CASCADE;
DROP TABLE IF EXISTS cat_019_actividades_economicas CASCADE;
DROP TABLE IF EXISTS cat_018_plazo CASCADE;
DROP TABLE IF EXISTS cat_017_forma_pago CASCADE;
DROP TABLE IF EXISTS cat_016_condicion_operacion CASCADE;
DROP TABLE IF EXISTS cat_015_tributos CASCADE;
DROP TABLE IF EXISTS cat_014_unidad_medida CASCADE;
DROP TABLE IF EXISTS cat_012_departamento CASCADE;
DROP TABLE IF EXISTS cat_011_tipo_item CASCADE;
DROP TABLE IF EXISTS cat_010_codigo_tipo_servicio_medico CASCADE;
DROP TABLE IF EXISTS cat_009_tipo_establecimiento CASCADE;
DROP TABLE IF EXISTS cat_008_eliminado CASCADE;
DROP TABLE IF EXISTS cat_007_tipo_generacion_documento CASCADE;
DROP TABLE IF EXISTS cat_006_retencion_iva_mh CASCADE;
DROP TABLE IF EXISTS cat_005_tipo_contingencia CASCADE;
DROP TABLE IF EXISTS cat_004_tipo_transmision CASCADE;
DROP TABLE IF EXISTS cat_003_modelo_facturacion CASCADE;
DROP TABLE IF EXISTS cat_002_tipo_documento CASCADE;
DROP TABLE IF EXISTS cat_001_ambiente_destino CASCADE;

--
-- Table: cat_001_ambiente_destino
--

CREATE TABLE cat_001_ambiente_destino (
  codigo CHAR(2) NOT NULL,
  descripcion VARCHAR(50) NOT NULL,
  PRIMARY KEY (codigo)
);

--
-- Data for table cat_001_ambiente_destino
--

INSERT INTO cat_001_ambiente_destino (codigo, descripcion) VALUES
('00', 'Modo prueba'),
('01', 'Modo producción');

--
-- Table: cat_002_tipo_documento
--

CREATE TABLE cat_002_tipo_documento (
  codigo CHAR(2) NOT NULL,
  descripcion VARCHAR(100) NOT NULL,
  PRIMARY KEY (codigo)
);

--
-- Data for table cat_002_tipo_documento
--

INSERT INTO cat_002_tipo_documento (codigo, descripcion) VALUES
('01', 'Factura'),
('03', 'Comprobante de crédito fiscal'),
('04', 'Nota de remisión'),
('05', 'Nota de crédito'),
('06', 'Nota de débito'),
('07', 'Comprobante de retención'),
('08', 'Comprobante de liquidación'),
('09', 'Documento contable de liquidación'),vaya
('11', 'Facturas de exportación'),
('14', 'Factura de sujeto excluido'),
('15', 'Comprobante de donación');

--
-- Table: cat_003_modelo_facturacion
--

CREATE TABLE cat_003_modelo_facturacion (
  codigo CHAR(1) NOT NULL,
  descripcion VARCHAR(100) NOT NULL,
  PRIMARY KEY (codigo)
);

--
-- Data for table cat_003_modelo_facturacion
--

INSERT INTO cat_003_modelo_facturacion (codigo, descripcion) VALUES
('1', 'Modelo Facturación previo'),
('2', 'Modelo Facturación diferido');

--
-- Table: cat_004_tipo_transmision
--

CREATE TABLE cat_004_tipo_transmision (
  codigo CHAR(1) NOT NULL,
  descripcion VARCHAR(100) NOT NULL,
  PRIMARY KEY (codigo)
);

--
-- Data for table cat_004_tipo_transmision
--

INSERT INTO cat_004_tipo_transmision (codigo, descripcion) VALUES
('1', 'Transmisión normal'),
('2', 'Transmisión por contingencia');

--
-- Table: cat_005_tipo_contingencia
--

CREATE TABLE cat_005_tipo_contingencia (
  codigo CHAR(1) NOT NULL,
  descripcion TEXT NOT NULL,
  PRIMARY KEY (codigo)
);

--
-- Data for table cat_005_tipo_contingencia
--

INSERT INTO cat_005_tipo_contingencia (codigo, descripcion) VALUES
('1', 'No disponibilidad de sistema del MH'),
('2', 'No disponibilidad de sistema del emisor'),
('3', 'Falla en el suministro de servicio de Internet del Emisor'),
('4', 'Falla en el suministro de servicio de energía eléctrica del emisor que impida la transmisión de los DTE'),
('5', 'Otro (deberá digitar un máximo de 500 caracteres explicando el motivo)');

--
-- Table: cat_006_retencion_iva_mh
--

CREATE TABLE cat_006_retencion_iva_mh (
  codigo VARCHAR(2) NOT NULL,
  descripcion VARCHAR(100) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_006_retencion_iva_mh (codigo, descripcion) VALUES
('22', 'Retención IVA 1%'),
('C4', 'Retención IVA 13%'),
('C9', 'Otras retenciones IVA casos especiales');

--
-- Table: cat_007_tipo_generacion_documento
--

CREATE TABLE cat_007_tipo_generacion_documento (
  codigo CHAR(1) NOT NULL,
  descripcion VARCHAR(50) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_007_tipo_generacion_documento (codigo, descripcion) VALUES
('1', 'Físico'),
('2', 'Electrónico');

--
-- Table: cat_008_eliminado
--

CREATE TABLE cat_008_eliminado (
  codigo VARCHAR(10) NOT NULL,
  descripcion VARCHAR(100) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_008_eliminado (codigo, descripcion) VALUES
('N/A', 'Catálogo eliminado');

--
-- Table: cat_009_tipo_establecimiento
--

CREATE TABLE cat_009_tipo_establecimiento (
  codigo CHAR(2) NOT NULL,
  descripcion VARCHAR(50) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_009_tipo_establecimiento (codigo, descripcion) VALUES
('01', 'Sucursal'),
('02', 'Casa Matriz'),
('04', 'Bodega'),
('07', 'Patio');

--
-- Table: cat_010_codigo_tipo_servicio_medico
--

CREATE TABLE cat_010_codigo_tipo_servicio_medico (
  codigo CHAR(1) NOT NULL,
  descripcion VARCHAR(150) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_010_codigo_tipo_servicio_medico (codigo, descripcion) VALUES
('1', 'Cirugía'),
('2', 'Operación'),
('3', 'Tratamiento médico'),
('4', 'Cirugía Instituto Salvadoreño de Bienestar Magisterial'),
('5', 'Operación Instituto Salvadoreño de Bienestar Magisterial'),
('6', 'Tratamiento médico Instituto Salvadoreño de Bienestar Magisterial');

--
-- Table: cat_011_tipo_item
--

CREATE TABLE cat_011_tipo_item (
  codigo CHAR(1) NOT NULL,
  descripcion VARCHAR(150) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_011_tipo_item (codigo, descripcion) VALUES
('1', 'Bienes'),
('2', 'Servicios'),
('3', 'Ambos (Bienes y Servicios, incluye los dos inherente a los productos o servicios)'),
('4', 'Otros tributos por ítem');

--
-- Table: cat_012_departamento
--

CREATE TABLE cat_012_departamento (
  codigo VARCHAR(2) NOT NULL,
  nombre VARCHAR(100) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_012_departamento (codigo, nombre) VALUES
('00', 'Otro (Para extranjeros)'),
('01', 'Ahuachapán'),
('02', 'Santa Ana'),
('03', 'Sonsonate'),
('04', 'Chalatenango'),
('05', 'La Libertad'),
('06', 'San Salvador'),
('07', 'Cuscatlán'),
('08', 'La Paz'),
('09', 'Cabañas'),
('10', 'San Vicente'),
('11', 'Usulután'),
('12', 'San Miguel'),
('13', 'Morazán'),
('14', 'La Unión');

--
-- Table: cat_013_municipio
--

CREATE TABLE cat_013_municipio (
  codigo VARCHAR(2) NOT NULL,
  departamento_codigo VARCHAR(2) NOT NULL,
  nombre VARCHAR(100) NOT NULL,
  PRIMARY KEY (codigo, departamento_codigo)
);

-- Data for municipios - INSERT STATEMENTS SKIPPED (large dataset)
INSERT INTO cat_013_municipio (codigo, departamento_codigo, nombre) VALUES
('10', '09', 'CABAÑAS OESTE'),
('11', '09', 'CABAÑAS ESTE'),
('13', '01', 'AHUACHAPAN NORTE'),
('14', '01', 'AHUACHAPAN CENTRO'),
('14', '02', 'SANTA ANA NORTE'),
('14', '10', 'SAN VICENTE NORTE'),
('15', '01', 'AHUACHAPAN SUR'),
('15', '02', 'SANTA ANA CENTRO'),
('15', '10', 'SAN VICENTE SUR'),
('16', '02', 'SANTA ANA ESTE'),
('17', '02', 'SANTA ANA OESTE'),
('17', '03', 'SONSONATE NORTE'),
('17', '07', 'CUSCATLAN NORTE'),
('18', '03', 'SONSONATE CENTRO'),
('18', '07', 'CUSCATLAN SUR'),
('19', '03', 'SONSONATE ESTE'),
('20', '03', 'SONSONATE OESTE'),
('20', '06', 'SAN SALVADOR NORTE'),
('21', '06', 'SAN SALVADOR OESTE'),
('21', '12', 'SAN MIGUEL NORTE'),
('22', '06', 'SAN SALVADOR ESTE'),
('22', '12', 'SAN MIGUEL CENTRO'),
('23', '05', 'LA LIBERTAD NORTE'),
('23', '06', 'SAN SALVADOR CENTRO'),
('23', '08', 'LA PAZ OESTE'),
('23', '12', 'SAN MIGUEL OESTE'),
('24', '04', 'CHALATENANGO NORTE'),
('24', '05', 'LA LIBERTAD CENTRO'),
('24', '06', 'SAN SALVADOR SUR'),
('24', '08', 'LA PAZ CENTRO'),
('24', '11', 'USULUTAN NORTE'),
('25', '04', 'CHALATENANGO CENTRO'),
('25', '05', 'LA LIBERTAD OESTE'),
('25', '08', 'LA PAZ ESTE'),
('25', '11', 'USULUTAN ESTE'),
('26', '04', 'CHALATENANGO SUR'),
('26', '05', 'LA LIBERTAD ESTE'),
('26', '11', 'USULUTAN OESTE'),
('27', '05', 'LA LIBERTAD COSTA'),
('27', '13', 'MORAZAN NORTE'),
('27', '14', 'LA UNION NORTE'),
('28', '05', 'LA LIBERTAD SUR'),
('28', '13', 'MORAZAN SUR'),
('28', '14', 'LA UNION SUR');


--
-- Table: cat_014_unidad_medida
--

CREATE TABLE cat_014_unidad_medida (
  codigo CHAR(2) NOT NULL,
  descripcion VARCHAR(50) NOT NULL,
  PRIMARY KEY (codigo)
);

-- Data for unidad_medida - INSERT STATEMENTS SKIPPED (large dataset)
INSERT INTO cat_014_unidad_medida (codigo, descripcion) VALUES
('1', 'metro'),
('10', 'Hectárea'),
('13', 'metro cuadrado'),
('15', 'Vara cuadrada'),
('18', 'metro cúbico'),
('2', 'Yarda'),
('20', 'Barril'),
('22', 'Galón'),
('23', 'Litro'),
('24', 'Botella'),
('26', 'Mililitro'),
('30', 'Tonelada'),
('32', 'Quintal'),
('33', 'Arroba'),
('34', 'Kilogramo'),
('36', 'Libra'),
('37', 'Onza troy'),
('38', 'Onza'),
('39', 'Gramo'),
('40', 'Miligramo'),
('42', 'Megawatt'),
('43', 'Kilowatt'),
('44', 'Watt'),
('45', 'Megavoltio-amperio'),
('46', 'Kilovoltio-amperio'),
('47', 'Voltio-amperio'),
('49', 'Gigawatt-hora'),
('50', 'Megawatt-hora'),
('51', 'Kilowatt-hora'),
('52', 'Watt-hora'),
('53', 'Kilovoltio'),
('54', 'Voltio'),
('55', 'Millar'),
('56', 'Medio millar'),
('57', 'Ciento'),
('58', 'Docena'),
('59', 'Unidad'),
('6', 'milímetro'),
('9', 'kilómetro cuadrado'),
('99', 'Otra');

--
-- Table: cat_015_tributos
--

CREATE TABLE cat_015_tributos (
  codigo VARCHAR(3) NOT NULL,
  descripcion VARCHAR(255) NOT NULL,
  PRIMARY KEY (codigo)
);

-- Data for tributos - INSERT STATEMENTS SKIPPED (large dataset)
INSERT INTO cat_015_tributos (codigo, descripcion) VALUES
('19', 'Fabricante de bebidas gaseosas, isotónicas, deportivas, fortificantes, energizante o estimulante'),
('20', 'Impuesto al Valor Agregado 13%'),
('28', 'Importador de bebidas gaseosas, isotónicas, deportivas, fortificantes, energizante o estimulante'),
('31', 'Detallistas o expendedores de bebidas alcohólicas'),
('32', 'Fabricante de cerveza'),
('33', 'Importador de cerveza'),
('34', 'Fabricante de productos de tabaco'),
('35', 'Importador de productos de tabaco'),
('36', 'Fabricante de armas de fuego, municiones y artículos similares'),
('37', 'Importador de armas de fuego, munición y artículos similares'),
('38', 'Fabricante de explosivos'),
('39', 'Importador de explosivos'),
('42', 'Fabricante de productos pirotécnicos'),
('43', 'Importador de productos pirotécnicos'),
('44', 'Productor de tabaco'),
('50', 'Distribuidor de bebidas gaseosas, isotónicas, deportivas, fortificantes, energizante o estimulante'),
('51', 'Bebidas alcohólicas'),
('52', 'Cerveza'),
('53', 'Productos del tabaco'),
('54', 'Bebidas carbonatadas o gaseosas simples o endulzadas'),
('55', 'Otros específicos'),
('57', 'Impuesto industria de Cemento'),
('58', 'Alcohol'),
('59', 'Turismo: por alojamiento (5%)'),
('71', 'Turismo: salida del país por vía aérea $7.00'),
('77', 'Importador de jugos, néctares, bebidas con jugo y refrescos'),
('78', 'Distribuidor de jugos, néctares, bebidas con jugo y refrescos'),
('79', 'Sobre llamadas telefónicas provenientes del ext.'),
('85', 'Detallista de jugos, néctares, bebidas con jugo y refrescos'),
('86', 'Fabricante de preparaciones concentradas o en polvo para la elaboración de bebidas'),
('90', 'Impuesto especial a la primera matrícula'),
('91', 'Fabricante de jugos, néctares, bebidas con jugo y refrescos'),
('92', 'Importador de preparaciones concentradas o en polvo para la elaboración de bebidas'),
('A1', 'Específicos y Ad-Valorem'),
('A5', 'Bebidas gaseosas, isotónicas, deportivas, fortificantes, energizantes o estimulantes'),
('A6', 'Impuesto ad-valorem, armas de fuego, municiones explosivas y artículos similares'),
('A7', 'Alcohol etílico'),
('A8', 'Impuesto Especial al Combustible (0%, 0.5%, 1%)'),
('A9', 'Sacos sintéticos'),
('C3', 'Impuesto al Valor Agregado (exportaciones) 0%'),
('C5', 'Impuesto ad-valorem diferencial bebidas alcohólicas (8%)'),
('C6', 'Impuesto ad-valorem por diferencial tabaco cigarrillos (39%)'),
('C7', 'Impuesto ad-valorem por diferencial tabaco cigarros (100%)'),
('C8', 'COTRANS ($0.10 Ctv. por galón)'),
('D1', 'FOVIAL ($0.20 Ctv. por galón)'),
('D4', 'Otros impuestos casos especiales'),
('D5', 'Otras tasas casos especiales');



--
-- Table: cat_016_condicion_operacion
--

CREATE TABLE cat_016_condicion_operacion (
  codigo CHAR(1) NOT NULL,
  descripcion VARCHAR(50) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_016_condicion_operacion (codigo, descripcion) VALUES
('1', 'Contado'),
('2', 'A crédito'),
('3', 'Otro');

--
-- Table: cat_017_forma_pago
--

CREATE TABLE cat_017_forma_pago (
  codigo CHAR(2) NOT NULL,
  descripcion VARCHAR(100) NOT NULL,
  PRIMARY KEY (codigo)
);

-- Data for forma_pago - INSERT STATEMENTS SKIPPED (large dataset)
INSERT INTO cat_017_forma_pago (codigo, descripcion) VALUES
('01', 'Billetes y monedas'),
('02', 'Tarjeta Débito'),
('03', 'Tarjeta Crédito'),
('04', 'Cheque'),
('05', 'Transferencia-Depósito Bancario'),
('08', 'Dinero electrónico'),
('09', 'Monedero electrónico'),
('11', 'Bitcoin'),
('12', 'Otras Criptomonedas'),
('13', 'Cuentas por pagar del receptor'),
('14', 'Giro bancario'),
('99', 'Otros (se debe indicar el medio de pago)');
--
-- Table: cat_018_plazo
--

CREATE TABLE cat_018_plazo (
  codigo CHAR(2) NOT NULL,
  descripcion VARCHAR(20) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_018_plazo (codigo, descripcion) VALUES
('01', 'Días'),
('02', 'Meses'),
('03', 'Años');

--
-- Table: cat_019_actividades_economicas
--

CREATE TABLE cat_019_actividades_economicas (
  cod_actividad CHAR(5) NOT NULL,
  desc_actividad TEXT NOT NULL,
  PRIMARY KEY (cod_actividad)
);

-- Data for actividades_economicas - INSERT STATEMENTS SKIPPED (massive dataset)
INSERT INTO cat_019_actividades_economicas (cod_actividad, desc_actividad) VALUES
('01111', 'Cultivo de cereales excepto arroz y para forrajes'),
('01112', 'Cultivo de legumbres'),
('01113', 'Cultivo de semillas oleaginosas'),
('01114', 'Cultivo de plantas para la preparación de semillas'),
('01119', 'Cultivo de otros cereales excepto arroz y forrajeros n.c.p.'),
('01120', 'Cultivo de arroz'),
('01131', 'Cultivo de raíces y tubérculos'),
('01132', 'Cultivo de brotes, bulbos, vegetales tubérculos y cultivos similares'),
('01133', 'Cultivo hortícola de fruto'),
('01134', 'Cultivo de hortalizas de hoja y otras hortalizas n.c.p.'),
('01140', 'Cultivo de caña de azúcar'),
('01150', 'Cultivo de tabaco'),
('01161', 'Cultivo de algodón'),
('01162', 'Cultivo de fibras vegetales excepto algodón'),
('01191', 'Cultivo de plantas no perennes para la producción de semillas y flores'),
('01192', 'Cultivo de cereales y pastos para la alimentación animal'),
('01199', 'Producción de cultivos no estacionales n.c.p.'),
('01220', 'Cultivo de frutas tropicales'),
('01230', 'Cultivo de cítricos'),
('01240', 'Cultivo de frutas de pepita y hueso'),
('01251', 'Cultivo de frutas n.c.p.'),
('01252', 'Cultivo de otros frutos y nueces de árboles y arbustos'),
('01260', 'Cultivo de frutos oleaginosos'),
('01271', 'Cultivo de café'),
('01272', 'Cultivo de plantas para la elaboración de bebidas excepto café'),
('01281', 'Cultivo de especias y aromáticas'),
('01282', 'Cultivo de plantas para la obtención de productos medicinales y farmacéuticos'),
('01291', 'Cultivo de árboles de hule (caucho) para la obtención de látex'),
('01299', 'Cultivo de plantas para la obtención de productos químicos y colorantes'),
('01300', 'Producción de cultivos perennes n.c.p.'),
('01301', 'Propagación de plantas'),
('01302', 'Cultivo de plantas y flores ornamentales'),
('01410', 'Cría y engorde de ganado bovino'),
('01420', 'Cría de caballos y otros equinos'),
('01440', 'Cría de ovejas y cabras'),
('01450', 'Cría de cerdos'),
('01460', 'Cría de aves de corral y producción de huevos'),
('01491', 'Cría de abejas (apicultura) para obtención de miel y otros productos apícolas'),
('01492', 'Cría de conejos'),
('01493', 'Cría de iguanas y garrobos'),
('01494', 'Cría de mariposas y otros insectos'),
('01499', 'Cría y obtención de productos animales n.c.p.'),
('01500', 'Cultivo de productos agrícolas en combinación con la cría de animales'),
('01611', 'Servicios de maquinaria agrícola'),
('01612', 'Control de plagas'),
('01613', 'Servicios de riego'),
('01614', 'Servicios de contratación de mano de obra para la agricultura'),
('01619', 'Servicios agrícolas n.c.p.'),
('01621', 'Actividades para mejorar la reproducción, el crecimiento y el rendimiento de los animales y sus productos'),
('01622', 'Servicios de mano de obra pecuaria'),
('01629', 'Servicios pecuarios n.c.p.'),
('01631', 'Labores postcosecha de preparación de productos agrícolas para comercialización o industria'),
('01632', 'Servicio de beneficio de café'),
('01633', 'Servicio de beneficiado de plantas textiles (incluye cuando se realiza en la misma explotación)'),
('01640', 'Tratamiento de semillas para la propagación'),
('01700', 'Caza ordinaria y mediante trampas, repoblación de animales de caza y servicios conexos'),
('02100', 'Silvicultura y otras actividades forestales'),
('02200', 'Extracción de madera'),
('02300', 'Recolección de productos diferentes a la madera'),
('02400', 'Servicios de apoyo a la silvicultura'),
('03110', 'Pesca marítima de altura y costera'),
('03120', 'Pesca de agua dulce'),
('03210', 'Acuicultura marítima'),
('03220', 'Acuicultura de agua dulce'),
('03300', 'Servicios de apoyo a la pesca y acuicultura'),
('05100', 'Extracción de hulla'),
('05200', 'Extracción y aglomeración de lignito'),
('06100', 'Extracción de petróleo crudo'),
('06200', 'Extracción de gas natural'),
('07100', 'Extracción de minerales de hierro'),
('07210', 'Extracción de minerales de uranio y torio'),
('07290', 'Extracción de minerales metalíferos no ferrosos'),
('08100', 'Extracción de piedra, arena y arcilla'),
('08910', 'Extracción de minerales para fabricación de abonos y productos químicos'),
('08920', 'Extracción y aglomeración de turba'),
('08930', 'Extracción de sal'),
('08990', 'Explotación de otras minas y canteras n.c.p.'),
('09100', 'Actividades de apoyo a la extracción de petróleo y gas natural'),
('09900', 'Actividades de apoyo a la explotación de minas y canteras'),
('10001', 'Empleados'),
('10002', 'Pensionado'),
('10003', 'Estudiante'),
('10004', 'Desempleado'),
('10005', 'Otros'),
('10006', 'Comerciante'),
('10101', 'Servicio de rastros y mataderos de bovinos y porcinos'),
('10102', 'Matanza y procesamiento de bovinos y porcinos'),
('10103', 'Matanza y procesamiento de aves de corral'),
('10104', 'Elaboración y conservación de embutidos y tripas naturales'),
('10105', 'Servicios de conservación y empaque de carnes'),
('10106', 'Elaboración y conservación de grasas y aceites animales'),
('10107', 'Servicios de molienda de carne'),
('10108', 'Elaboración de productos de carne n.c.p.'),
('10201', 'Procesamiento y conservación de pescado, crustáceos y moluscos'),
('10209', 'Fabricación de productos de pescado n.c.p.'),
('10301', 'Elaboración de jugos de frutas y hortalizas'),
('10302', 'Elaboración y envase de jaleas, mermeladas y frutas deshidratadas'),
('10309', 'Elaboración de productos de frutas y hortalizas n.c.p.'),
('10401', 'Fabricación de aceites y grasas vegetales y animales comestibles'),
('10402', 'Fabricación de aceites y grasas vegetales y animales no comestibles'),
('10409', 'Servicio de maquilado de aceites'),
('10501', 'Fabricación de productos lácteos excepto sorbetes y quesos sustitutos'),
('10502', 'Fabricación de sorbetes y helados'),
('10503', 'Fabricación de quesos'),
('10611', 'Molienda de cereales'),
('10612', 'Elaboración de cereales para el desayuno y similares'),
('10613', 'Servicios de beneficiado de productos agrícolas n.c.p. (excluye beneficio de azúcar rama 1072 y beneficio de café rama 0163)'),
('10621', 'Fabricación de almidón'),
('10628', 'Servicio de molienda de maíz húmedo (molino para nixtamal)'),
('10711', 'Elaboración de tortillas'),
('10712', 'Fabricación de pan, galletas y barquillos'),
('10713', 'Fabricación de repostería'),
('10721', 'Ingenios azucareros'),
('10722', 'Molienda de caña de azúcar para la elaboración de dulces'),
('10723', 'Elaboración de jarabes de azúcar y otros similares'),
('10724', 'Maquilado de azúcar de caña'),
('10730', 'Fabricación de cacao, chocolates y productos de confitería'),
('10740', 'Elaboración de macarrones, fideos y productos farináceos similares'),
('10750', 'Elaboración de comidas y platos preparados para la reventa en'),
('10791', 'Elaboración de productos de café'),
('10792', 'Elaboración de especias, sazonadores y condimentos'),
('10793', 'Elaboración de sopas, cremas y consomé'),
('10794', 'Fabricación de bocadillos tostados y/o fritos'),
('10799', 'Elaboración de productos alimenticios n.c.p.'),
('10800', 'Elaboración de alimentos preparados para animales'),
('11012', 'Fabricación de aguardiente y licores'),
('11020', 'Elaboración de vinos'),
('11030', 'Fabricación de cerveza'),
('11041', 'Fabricación de aguas gaseosas'),
('11042', 'Fabricación y envasado de agua'),
('11043', 'Elaboración de refrescos'),
('11048', 'Maquilado de aguas gaseosas'),
('11049', 'Elaboración de bebidas no alcohólicas'),
('12000', 'Elaboración de productos de tabaco'),
('13111', 'Preparación de fibras textiles'),
('13112', 'Fabricación de hilados'),
('13120', 'Fabricación de telas'),
('13130', 'Acabado de productos textiles'),
('13910', 'Fabricación de tejidos de punto y ganchillo'),
('13921', 'Fabricación de productos textiles para el hogar'),
('13922', 'Sacos, bolsas y otros artículos textiles'),
('13929', 'Fabricación de artículos confeccionados con materiales textiles, excepto prendas de vestir n.c.p.'),
('13930', 'Fabricación de tapices y alfombras'),
('13941', 'Fabricación de cuerdas de henequén y otras fibras naturales (lazos, pitas)'),
('13942', 'Fabricación de redes de diversos materiales'),
('13948', 'Maquilado de productos trenzables de cualquier material (petates, sillas, etc.)'),
('13991', 'Fabricación de adornos, etiquetas y otros artículos para prendas de vestir'),
('13992', 'Servicio de bordados en artículos y prendas de tela'),
('13999', 'Fabricación de productos textiles n.c.p.'),
('14101', 'Fabricación de ropa interior, para dormir y similares'),
('14102', 'Fabricación de ropa para niños'),
('14103', 'Fabricación de prendas de vestir para ambos sexos'),
('14104', 'Confección de prendas a medida'),
('14105', 'Fabricación de prendas de vestir para deportes'),
('14106', 'Elaboración de artesanías de uso personal confeccionadas especialmente de materiales textiles'),
('14108', 'Maquilado de prendas de vestir, accesorios y otros'),
('14109', 'Fabricación de prendas y accesorios de vestir n.c.p.'),
('14200', 'Fabricación de artículos de piel'),
('14301', 'Fabricación de calcetines, calcetas, medias (panty house) y otros similares'),
('14302', 'Fabricación de ropa interior de tejido de punto'),
('14309', 'Fabricación de prendas de vestir de tejido de punto n.c.p.'),
('15110', 'Curtido y adobo de cueros; adobo y teñido de pieles'),
('15121', 'Fabricación de maletas, bolsos de mano y otros artículos de marroquinería'),
('15122', 'Fabricación de monturas, accesorios y vainas talabartería'),
('15123', 'Fabricación de artesanías principalmente de cuero natural y sintético'),
('15128', 'Maquilado de artículos de cuero natural, sintético y de otros materiales'),
('15201', 'Fabricación de calzado'),
('15202', 'Fabricación de partes y accesorios de calzado'),
('15208', 'Maquilado de partes y accesorios de calzado'),
('16100', 'Aserradero y acépilladura de madera'),
('16210', 'Fabricación de madera laminada, terciada, enchapada y contrachapada, paneles para la construcción'),
('16220', 'Fabricación de partes y piezas de carpintería para edificios y construcciones'),
('16230', 'Fabricación de envases y recipientes de madera'),
('16292', 'Fabricación de artesanías de madera, semillas, materiales trenzables'),
('16299', 'Fabricación de productos de madera, corcho, paja y materiales trenzables n.c.p.'),
('17010', 'Fabricación de pasta de madera, papel y cartón'),
('17020', 'Fabricación de papel y cartón ondulado y envases de papel y cartón'),
('17091', 'Fabricación de artículos de papel y cartón de uso personal y doméstico'),
('17092', 'Fabricación de productos de papel n.c.p.'),
('18110', 'Impresión'),
('18120', 'Servicios relacionados con la impresión'),
('18200', 'Reproducción de grabaciones'),
('19100', 'Fabricación de productos de hornos de coque'),
('19201', 'Fabricación de combustible'),
('19202', 'Fabricación de aceites y lubricantes'),
('20111', 'Fabricación de materias primas para la fabricación de colorantes'),
('20112', 'Fabricación de materiales curtientes'),
('20113', 'Fabricación de gases industriales'),
('20114', 'Fabricación de alcohol etílico'),
('20119', 'Fabricación de sustancias químicas básicas'),
('20120', 'Fabricación de abonos y fertilizantes'),
('20130', 'Fabricación de plástico y caucho en formas primarias'),
('20210', 'Fabricación de plaguicidas y otros productos químicos de uso agropecuario'),
('20220', 'Fabricación de pinturas, barnices y productos de revestimiento similares; tintas de imprenta y masillas'),
('20231', 'Fabricación de jabones, detergentes y similares para limpieza'),
('20232', 'Fabricación de perfumes, cosméticos y productos de higiene y cuidado personal, incluyendo tintes, champú, etc.'),
('20291', 'Fabricación de tintas y colores para escribir y pintar; fabricación de cintas para impresoras'),
('20292', 'Fabricación de productos pirotécnicos, explosivos y municiones'),
('20299', 'Fabricación de productos químicos n.c.p.'),
('21001', 'Manufactura de productos farmacéuticos, sustancias químicas y productos botánicos'),
('21008', 'Maquilado de medicamentos'),
('22110', 'Fabricación de cubiertas y cámaras; renovación y recauchutado de cubiertas'),
('22190', 'Fabricación de otros productos de caucho'),
('22201', 'Fabricación de envases plásticos'),
('22202', 'Fabricación de productos plásticos para uso personal o doméstico'),
('22208', 'Maquila de plásticos'),
('22209', 'Fabricación de productos plásticos n.c.p.'),
('23101', 'Fabricación de vidrio'),
('23102', 'Fabricación de recipientes y envases de vidrio'),
('23108', 'Servicio de maquilado'),
('23109', 'Fabricación de productos de vidrio n.c.p.'),
('23910', 'Fabricación de productos refractarios'),
('23920', 'Fabricación de productos de arcilla para la construcción'),
('23931', 'Fabricación de productos de cerámica y porcelana no refractaria'),
('23932', 'Fabricación de productos de cerámica y porcelana n.c.p.'),
('23940', 'Fabricación de cemento, cal y yeso'),
('23950', 'Fabricación de artículos de hormigón, cemento y yeso'),
('23960', 'Corte, tallado y acabado de la piedra'),
('23990', 'Fabricación de productos minerales no metálicos n.c.p.'),
('24100', 'Industrias básicas de hierro y acero'),
('24200', 'Fabricación de productos primarios de metales preciosos y metales no ferrosos'),
('24310', 'Fundición de hierro y acero'),
('24320', 'Fundición de metales no ferrosos'),
('25111', 'Fabricación de productos metálicos para uso estructural'),
('25118', 'Servicio de maquila para la fabricación de estructuras metálicas'),
('25120', 'Fabricación de tanques, depósitos y recipientes de metal'),
('25130', 'Fabricación de generadores de vapor, excepto calderas de agua caliente para calefacción central'),
('25200', 'Fabricación de armas y municiones'),
('25910', 'Forjado, prensado, estampado y laminado de metales; pulvimetalurgia'),
('25920', 'Tratamiento y revestimiento de metales'),
('25930', 'Fabricación de artículos de cuchillería, herramientas de mano y artículos de ferretería'),
('25991', 'Fabricación de envases y artículos conexos de metal'),
('25992', 'Fabricación de artículos metálicos de uso personal y/o doméstico'),
('25999', 'Fabricación de productos elaborados de metal n.c.p.'),
('26100', 'Fabricación de componentes electrónicos'),
('26200', 'Fabricación de computadoras y equipo conexo'),
('26300', 'Fabricación de equipo de comunicaciones'),
('26400', 'Fabricación de aparatos electrónicos de consumo para audio, video, radio y televisión'),
('26510', 'Fabricación de instrumentos y aparatos para medir, verificar, ensayar, navegar y de control de procesos industriales'),
('26520', 'Fabricación de relojes y piezas de relojes'),
('26600', 'Fabricación de equipo médico de irradiación y equipo electrónico de uso médico y terapéutico'),
('26700', 'Fabricación de instrumentos de óptica y equipo fotográfico'),
('26800', 'Fabricación de medios magnéticos y ópticos'),
('27100', 'Fabricación de motores, generadores, transformadores eléctricos, aparatos de distribución y de control de electricidad'),
('27200', 'Fabricación de pilas, baterías y acumuladores'),
('27310', 'Fabricación de cables de fibra óptica'),
('27320', 'Fabricación de otros hilos y cables eléctricos'),
('27330', 'Fabricación de dispositivos de cableados'),
('27400', 'Fabricación de equipo eléctrico de iluminación'),
('27500', 'Fabricación de aparatos de uso doméstico'),
('27900', 'Fabricación de otros tipos de equipo eléctrico'),
('28110', 'Fabricación de motores y turbinas, excepto motores para aeronaves, vehículos automotores y motocicletas'),
('28120', 'Fabricación de equipo hidráulico'),
('28130', 'Fabricación de otras bombas, compresores, grifos y válvulas'),
('28140', 'Fabricación de cojinetes, engranajes, trenes de engranajes y piezas de transmisión'),
('28150', 'Fabricación de hornos y quemadores'),
('28160', 'Fabricación de equipo de elevación y manipulación'),
('28170', 'Fabricación de maquinaria y equipo de oficina'),
('28180', 'Fabricación de herramientas manuales'),
('28190', 'Fabricación de otros tipos de maquinaria de uso general'),
('28210', 'Fabricación de maquinaria agropecuaria y forestal'),
('28220', 'Fabricación de máquinas para conformar metales y maquinaria herramienta'),
('28230', 'Fabricación de maquinaria metalúrgica'),
('28240', 'Fabricación de maquinaria para la explotación de minas y canteras y para obras de construcción'),
('28250', 'Fabricación de maquinaria para la elaboración de alimentos, bebidas y tabaco'),
('28260', 'Fabricación de maquinaria para la elaboración de productos textiles, prendas de vestir y cueros'),
('28291', 'Fabricación de máquinas para imprenta'),
('28299', 'Fabricación de maquinaria de uso especial n.c.p.'),
('29100', 'Fabricación de vehículos automotores'),
('29200', 'Fabricación de carrocerías para vehículos automotores; fabricación de remolques y semirremolques'),
('29300', 'Fabricación de partes, piezas y accesorios para vehículos automotores'),
('30110', 'Fabricación de buques'),
('30120', 'Construcción y reparación de embarcaciones de recreo'),
('30200', 'Fabricación de locomotoras y de material rodante'),
('30300', 'Fabricación de aeronaves y naves espaciales'),
('30400', 'Fabricación de vehículos militares de combate'),
('30910', 'Fabricación de motocicletas'),
('30920', 'Fabricación de bicicletas y sillones de ruedas para inválidos'),
('30990', 'Fabricación de equipo de transporte n.c.p.'),
('31001', 'Fabricación de colchones y somier'),
('31002', 'Fabricación de muebles y otros productos de madera a medida'),
('31008', 'Servicios de maquilado de muebles'),
('31009', 'Fabricación de muebles n.c.p.'),
('32110', 'Fabricación de joyas platerías y joyerías'),
('32120', 'Fabricación de joyas de imitación (fantasía) y artículos conexos'),
('32200', 'Fabricación de instrumentos musicales'),
('32301', 'Fabricación de artículos de deporte'),
('32308', 'Servicio de maquila de productos deportivos'),
('32401', 'Fabricación de juegos de mesa y de salón'),
('32402', 'Servicio de maquila de juguetes y juegos'),
('32409', 'Fabricación de juegos y juguetes n.c.p.'),
('32500', 'Fabricación de instrumentos y materiales médicos y odontológicos'),
('32901', 'Fabricación de lápices, bolígrafos, sellos y artículos de librería en general'),
('32902', 'Fabricación de escobas, cepillos, pinceles y similares'),
('32903', 'Fabricación de artesanías de materiales diversos'),
('32904', 'Fabricación de artículos de uso personal y domésticos n.c.p.'),
('32905', 'Fabricación de accesorios para las confecciones y la marroquinería n.c.p.'),
('32908', 'Servicios de maquila n.c.p.'),
('32909', 'Fabricación de productos manufacturados n.c.p.'),
('33110', 'Reparación y mantenimiento de productos elaborados de metal'),
('33120', 'Reparación y mantenimiento de maquinaria'),
('33130', 'Reparación y mantenimiento de equipo electrónico y óptico'),
('33140', 'Reparación y mantenimiento de equipo eléctrico'),
('33150', 'Reparación y mantenimiento de equipo de transporte, excepto vehículos automotores'),
('33190', 'Reparación y mantenimiento de equipos n.c.p.'),
('33200', 'Instalación de maquinaria y equipo industrial'),
('36000', 'Captación, tratamiento y suministro de agua'),
('37000', 'Evacuación de aguas residuales (alcantarillado)'),
('38110', 'Recolección y transporte de desechos sólidos proveniente de hogares y sector urbano'),
('38120', 'Recolección de desechos peligrosos'),
('38210', 'Tratamiento y eliminación de desechos peligrosos'),
('38301', 'Reciclaje de desperdicios y desechos textiles'),
('38302', 'Reciclaje de desperdicios y desechos de plástico y caucho'),
('38303', 'Reciclaje de desperdicios y desechos de vidrio'),
('38304', 'Reciclaje de desperdicios y desechos de papel y cartón'),
('38305', 'Reciclaje de desperdicios y desechos metálicos'),
('38309', 'Reciclaje de desperdicios y desechos no metálicos n.c.p.'),
('39000', 'Actividades de saneamiento y otros servicios de gestión de desechos'),
('41001', 'Construcción de edificios residenciales'),
('41002', 'Construcción de edificios no residenciales'),
('42100', 'Construcción de carreteras, calles y caminos'),
('42200', 'Construcción de proyectos de servicio público'),
('42900', 'Construcción de obras de ingeniería civil n.c.p.'),
('43110', 'Demolición'),
('43120', 'Preparación de terreno'),
('43210', 'Instalaciones eléctricas'),
('43220', 'Instalación de fontanería, calefacción y aire acondicionado'),
('43290', 'Otras instalaciones para obras de construcción'),
('43300', 'Terminación y acabado de edificios'),
('43900', 'Otras actividades especializadas de construcción'),
('43901', 'Fabricación de techos y materiales diversos'),
('45100', 'Venta de vehículos automotores'),
('45201', 'Reparación mecánica de vehículos automotores'),
('45202', 'Reparaciones eléctricas del automotor y recarga de baterías'),
('45203', 'Enderezado y pintura de vehículos automotores'),
('45204', 'Reparaciones de radiadores, escapes y silenciadores'),
('45205', 'Reparación y reconstrucción de vías, stop y otros artículos de fibra de vidrio'),
('45206', 'Reparación de llantas de vehículos automotores'),
('45207', 'Polarizado de vehículos (adhesión de papel especial a los vidrios)'),
('45208', 'Lavado y pasteado de vehículos (carwash)'),
('45209', 'Reparaciones de vehículos n.c.p.'),
('45211', 'Remolque de vehículos automotores'),
('45301', 'Venta de partes, piezas y accesorios nuevos para vehículos automotores'),
('45302', 'Venta de partes, piezas y accesorios usados para vehículos automotores'),
('45401', 'Venta de motocicletas'),
('45402', 'Venta de repuestos, piezas y accesorios de motocicletas'),
('45403', 'Mantenimiento y reparación de motocicletas'),
('46100', 'Venta al por mayor a cambio de retribución o por contrata'),
('46201', 'Venta al por mayor de materias primas agrícolas'),
('46202', 'Venta al por mayor de productos de la silvicultura'),
('46203', 'Venta al por mayor de productos pecuarios y de granja'),
('46211', 'Venta de productos para uso agropecuario'),
('46291', 'Venta al por mayor de granos básicos (cereales, leguminosas)'),
('46292', 'Venta al por mayor de semillas mejoradas para cultivo'),
('46293', 'Venta al por mayor de café oro y uva'),
('46294', 'Venta al por mayor de caña de azúcar'),
('46295', 'Venta al por mayor de flores, plantas y otros productos naturales'),
('46296', 'Venta al por mayor de productos agrícolas'),
('46297', 'Venta al por mayor de ganado bovino (vivo)'),
('46298', 'Venta al por mayor de animales porcinos, ovinos, caprino, canículas, apícolas, avícolas vivos'),
('46299', 'Venta de otras especies vivas del reino animal'),
('46301', 'Venta al por mayor de alimentos'),
('46302', 'Venta al por mayor de bebidas'),
('46303', 'Venta al por mayor de tabaco'),
('46371', 'Venta al por mayor de frutas, hortalizas, legumbres y tubérculos'),
('46372', 'Venta al por mayor de pollos, gallinas destazadas, pavos y otras aves'),
('46373', 'Venta al por mayor de carne bovina y porcina, productos de carne y embutidos'),
('46374', 'Venta al por mayor de huevos'),
('46375', 'Venta al por mayor de productos lácteos'),
('46376', 'Venta al por mayor de productos farináceos de panadería'),
('46377', 'Venta al por mayor de pastas alimenticias, aceites y grasas comestibles'),
('46378', 'Venta al por mayor de sal comestible'),
('46379', 'Venta al por mayor de azúcar'),
('46391', 'Venta al por mayor de abarrotes (vinos, licores, productos alimenticios envasados)'),
('46392', 'Venta al por mayor de aguas gaseosas'),
('46393', 'Venta al por mayor de agua purificada'),
('46394', 'Venta al por mayor de refrescos y otras bebidas'),
('46395', 'Venta al por mayor de cerveza y licores'),
('46396', 'Venta al por mayor de hielo'),
('46411', 'Venta al por mayor de hilados, tejidos y productos textiles de mercería'),
('46412', 'Venta al por mayor de artículos textiles excepto confecciones para el hogar'),
('46413', 'Venta al por mayor de confecciones textiles para el hogar'),
('46414', 'Venta al por mayor de prendas de vestir y accesorios de vestir'),
('46415', 'Venta al por mayor de ropa usada'),
('46416', 'Venta al por mayor de calzado'),
('46417', 'Venta al por mayor de artículos de marroquinería y talabartería'),
('46418', 'Venta al por mayor de artículos de peletería'),
('46419', 'Venta al por mayor de otros artículos textiles n.c.p.'),
('46471', 'Venta al por mayor de instrumentos musicales'),
('46472', 'Venta al por mayor de colchones, almohadas y cojines'),
('46473', 'Venta al por mayor de artículos de aluminio para el hogar'),
('46474', 'Venta al por mayor de depósitos y artículos plásticos para el hogar'),
('46475', 'Venta al por mayor de cámaras fotográficas, accesorios y materiales'),
('46482', 'Venta al por mayor de medicamentos y productos de uso veterinario'),
('46483', 'Venta al por mayor de productos de belleza y de uso personal'),
('46484', 'Venta al por mayor de productos farmacéuticos y medicinales'),
('46491', 'Venta al por mayor de productos medicinales, cosméticos y de limpieza'),
('46492', 'Venta al por mayor de relojes y artículos de joyería'),
('46493', 'Venta al por mayor de electrodomésticos y artículos del hogar'),
('46494', 'Venta al por mayor de artículos de bazar y similares'),
('46495', 'Venta al por mayor de artículos de óptica'),
('46496', 'Venta al por mayor de libros, periódicos, papelería y cartón'),
('46497', 'Venta al por mayor de artículos deportivos, juguetes y rodados'),
('46498', 'Venta al por mayor de productos usados para el hogar'),
('46499', 'Venta al por mayor de enseres domésticos y de uso personal n.c.p.'),
('46500', 'Venta al por mayor de bicicletas, partes y accesorios'),
('46510', 'Venta al por mayor de computadoras, equipo periférico y software'),
('46520', 'Venta al por mayor de equipos de comunicación'),
('46530', 'Venta al por mayor de maquinaria y equipo agropecuario'),
('46590', 'Venta al por mayor de equipos e instrumentos de uso profesional y científico'),
('46591', 'Venta al por mayor de maquinaria, accesorios y materiales para la madera'),
('46592', 'Venta al por mayor de maquinaria para la industria gráfica y del papel'),
('46593', 'Venta al por mayor de maquinaria para la industria química, plástico y caucho'),
('46594', 'Venta al por mayor de maquinaria para la industria metálica'),
('46595', 'Venta al por mayor de equipamiento médico, odontológico y veterinario'),
('46596', 'Venta al por mayor de maquinaria para la industria de la alimentación'),
('46597', 'Venta al por mayor de maquinaria para la industria textil y cuero'),
('46598', 'Venta al por mayor de maquinaria para construcción y minas'),
('46599', 'Venta al por mayor de otro tipo de maquinaria y equipo'),
('46610', 'Venta al por mayor de otros combustibles sólidos, líquidos, gaseosos'),
('46612', 'Venta al por mayor de combustibles para automotores, aviones y barcos'),
('46613', 'Venta al por mayor de lubricantes, grasas y aceites'),
('46614', 'Venta al por mayor de gas propano'),
('46615', 'Venta al por mayor de leña y carbón'),
('46620', 'Venta al por mayor de metales y minerales metalíferos'),
('46631', 'Venta al por mayor de puertas, ventanas y vitrinas'),
('46632', 'Venta al por mayor de artículos de ferretería y pinturerías'),
('46633', 'Vidrierías'),
('46639', 'Venta al por mayor de materiales para la construcción n.c.p.'),
('46691', 'Venta al por mayor de sal industrial sin yodar'),
('46692', 'Venta al por mayor de productos intermedios y desechos textiles'),
('46693', 'Venta al por mayor de productos intermedios y desechos metálicos'),
('46694', 'Venta al por mayor de productos intermedios y desechos de papel y cartón'),
('46695', 'Venta al por mayor de fertilizantes, abonos y agroquímicos'),
('46696', 'Venta al por mayor de productos intermedios y desechos de plástico'),
('46697', 'Venta al por mayor de tintas, curtientes y colorantes'),
('46698', 'Venta al por mayor de productos intermedios y desechos químicos y de caucho'),
('46699', 'Venta al por mayor de productos intermedios y desechos n.c.p.'),
('46701', 'Venta al por mayor de algodón en oro'),
('46900', 'Venta al por mayor de otros productos'),
('46901', 'Venta al por mayor de cohetes y productos pirotécnicos'),
('46902', 'Venta al por mayor de artículos diversos para consumo humano'),
('46903', 'Venta al por mayor de armas de fuego, municiones y accesorios'),
('46904', 'Venta al por mayor de toldos y tiendas de campaña'),
('46905', 'Venta al por mayor de exhibidores publicitarios y rótulos'),
('46906', 'Venta al por mayor de artículos promocionales diversos'),
('47111', 'Venta en supermercados'),
('47112', 'Venta en tiendas de artículos de primera necesidad'),
('47119', 'Almacenes (venta de diversos artículos)'),
('47190', 'Venta al por menor de otros productos en comercios no especializados'),
('47199', 'Venta en establecimientos no especializados (alimentos, bebidas y tabaco)'),
('47211', 'Venta al por menor de frutas y hortalizas'),
('47212', 'Venta al por menor de carnes, embutidos y productos de granja'),
('47213', 'Venta al por menor de pescado y mariscos'),
('47214', 'Venta al por menor de productos lácteos'),
('47215', 'Venta al por menor de productos de panadería, repostería y galletas'),
('47216', 'Venta al por menor de huevos'),
('47217', 'Venta al por menor de carnes y productos cárnicos'),
('47218', 'Venta al por menor de granos básicos y otros'),
('47219', 'Venta al por menor de alimentos n.c.p.'),
('47221', 'Venta al por menor de hielo'),
('47223', 'Venta de bebidas no alcohólicas para consumo fuera del establecimiento'),
('47224', 'Venta de bebidas alcohólicas para consumo fuera del establecimiento'),
('47225', 'Venta de bebidas alcohólicas para consumo dentro del establecimiento'),
('47230', 'Venta al por menor de tabaco'),
('47300', 'Venta de combustibles, lubricantes y otros (gasolineras)'),
('47411', 'Venta al por menor de computadoras y equipo periférico'),
('47412', 'Venta al por menor de equipo y accesorios de telecomunicación'),
('47420', 'Venta al por menor de equipo de audio y video'),
('47510', 'Venta al por menor de hilados, tejidos y productos textiles de mercería'),
('47521', 'Venta al por menor de productos de madera'),
('47522', 'Venta al por menor de artículos de ferretería'),
('47523', 'Venta al por menor de productos de pinturerías'),
('47524', 'Venta al por menor en vidrierías'),
('47529', 'Venta al por menor de materiales de construcción y artículos conexos'),
('47530', 'Venta al por menor de tapices, alfombras y revestimientos'),
('47591', 'Venta al por menor de muebles'),
('47592', 'Venta al por menor de artículos de bazar'),
('47593', 'Venta al por menor de aparatos electrodomésticos y accesorios'),
('47594', 'Venta al por menor de artículos eléctricos y de iluminación'),
('47598', 'Venta al por menor de instrumentos musicales'),
('47610', 'Venta al por menor de libros, periódicos y papelería'),
('47620', 'Venta al por menor de discos láser, cassettes y cintas de video'),
('47630', 'Venta al por menor de productos y equipos deportivos'),
('47631', 'Venta al por menor de bicicletas, accesorios y repuestos'),
('47640', 'Venta al por menor de juegos y juguetes en comercios especializados'),
('47711', 'Venta al por menor de prendas de vestir y accesorios'),
('47712', 'Venta al por menor de calzado'),
('47713', 'Venta al por menor de artículos de peletería y talabartería'),
('47721', 'Venta al por menor de medicamentos y artículos de uso médico'),
('47722', 'Venta al por menor de productos cosméticos y de tocador'),
('47731', 'Venta al por menor de joyería, bisutería, óptica y relojería'),
('47732', 'Venta al por menor de plantas, semillas y artículos conexos'),
('47733', 'Venta al por menor de combustibles de uso doméstico'),
('47734', 'Venta al por menor de artesanías, cerámica y recuerdos'),
('47735', 'Venta al por menor de ataúdes, lápidas y artículos religiosos'),
('47736', 'Venta al por menor de armas de fuego, municiones y accesorios'),
('47737', 'Venta al por menor de cohetería y pirotécnicos'),
('47738', 'Venta al por menor de artículos desechables de uso personal y doméstico'),
('47739', 'Venta al por menor de otros productos n.c.p.'),
('47741', 'Venta al por menor de artículos usados'),
('47742', 'Venta al por menor de textiles y confecciones usados'),
('47743', 'Venta al por menor de libros, revistas y cartón usados'),
('47749', 'Venta al por menor de productos usados n.c.p.'),
('47811', 'Venta al por menor de frutas, verduras y hortalizas'),
('47814', 'Venta al por menor de productos lácteos'),
('47815', 'Venta al por menor de productos de panadería y galletas'),
('47816', 'Venta al por menor de bebidas'),
('47818', 'Venta al por menor en tiendas de mercado y puestos'),
('47821', 'Venta al por menor de mercería en puestos de mercados y ferias'),
('47822', 'Venta al por menor de artículos textiles en puestos de mercados y ferias'),
('47823', 'Venta al por menor de confecciones textiles en puestos de mercados y ferias'),
('47824', 'Venta al por menor de prendas de vestir en puestos de mercados y ferias'),
('47825', 'Venta al por menor de ropa usada en puestos de mercados y ferias'),
('47826', 'Venta al por menor de calzado y talabartería en puestos de mercados y ferias'),
('47829', 'Venta al por menor de textiles n.c.p. en puestos de mercados y ferias'),
('47891', 'Venta al por menor de animales, flores y productos conexos en puestos de ferias'),
('47892', 'Venta al por menor de productos medicinales y de limpieza en puestos de ferias'),
('47893', 'Venta al por menor de artículos de bazar en puestos de ferias'),
('47894', 'Venta al por menor de papelería y cartón en puestos de ferias'),
('47895', 'Venta al por menor de materiales de construcción y electrodomésticos en ferias'),
('47896', 'Venta al por menor de equipos de comunicaciones en puestos de ferias'),
('47899', 'Venta al por menor en puestos de ferias y mercados n.c.p.'),
('47901', 'Venta al por menor por correo o Internet'),
('49110', 'Transporte interurbano de pasajeros por ferrocarril'),
('49120', 'Transporte de carga por ferrocarril'),
('49211', 'Transporte de pasajeros urbanos e interurbanos en buses'),
('49212', 'Transporte de pasajeros interdepartamental en microbuses'),
('49213', 'Transporte de pasajeros urbanos e interurbanos en microbuses'),
('49214', 'Transporte de pasajeros interdepartamental en buses'),
('49221', 'Transporte internacional de pasajeros'),
('49222', 'Transporte de pasajeros en taxis y autos con chofer'),
('49223', 'Transporte escolar'),
('49225', 'Transporte de pasajeros para excursiones'),
('49226', 'Servicios de transporte de personal'),
('49229', 'Transporte de pasajeros por vía terrestre n.c.p.'),
('49231', 'Transporte de carga urbano'),
('49232', 'Transporte nacional de carga'),
('49233', 'Transporte de carga internacional'),
('49234', 'Servicios de mudanza'),
('49235', 'Alquiler de vehículos de carga con conductor'),
('49300', 'Transporte por oleoducto o gasoducto'),
('50110', 'Transporte de pasajeros marítimo y de cabotaje'),
('50120', 'Transporte de carga marítimo y de cabotaje'),
('50211', 'Transporte de pasajeros por navegación interior'),
('50212', 'Alquiler de transporte de pasajeros por navegación interior con conductor'),
('50220', 'Transporte de carga por navegación interior'),
('51100', 'Transporte aéreo de pasajeros'),
('51201', 'Transporte de carga por vía aérea'),
('51202', 'Alquiler de aerotransporte con operador para carga'),
('52101', 'Alquiler de instalaciones de almacenamiento en zonas francas'),
('52102', 'Alquiler de silos para conservación de granos'),
('52103', 'Alquiler de instalaciones refrigeradas para alimentos'),
('52109', 'Alquiler de bodegas para almacenamiento n.c.p.'),
('52211', 'Servicios de garaje y estacionamiento'),
('52212', 'Servicios de terminales terrestres'),
('52219', 'Servicios de transporte terrestre n.c.p.'),
('52220', 'Manipulación de carga'),
('52290', 'Servicios para el transporte n.c.p.'),
('52291', 'Agencias de tramitaciones aduanales'),
('53100', 'Servicios de correo nacional'),
('53200', 'Actividades de correo distintas a las postales nacionales'),
('53201', 'Agencia privada de correo y encomiendas'),
('55101', 'Actividades de alojamiento para estancias cortas'),
('55102', 'Hoteles'),
('55200', 'Actividades de campamentos y parques de caravanas'),
('55900', 'Alojamiento n.c.p.'),
('56101', 'Restaurantes'),
('56106', 'Pupusería'),
('56107', 'Actividades varias de restaurantes'),
('56108', 'Comedores'),
('56109', 'Merenderos ambulantes'),
('56210', 'Preparación de comida para eventos especiales'),
('56291', 'Servicios de provisión de comidas por contrato'),
('56292', 'Servicios de concesión de cafeterías y chalet en empresas'),
('56299', 'Servicios de preparación de comidas n.c.p.'),
('56301', 'Expendio de bebidas en salones y bares'),
('56302', 'Expendio de bebidas en puestos callejeros, mercados y ferias'),
('58110', 'Edición de libros, folletos y partituras'),
('58120', 'Edición de directorios y listas de correos'),
('58130', 'Edición de periódicos, revistas y publicaciones periódicas'),
('58190', 'Otras actividades de edición'),
('58200', 'Edición de programas informáticos'),
('59110', 'Actividades de producción cinematográfica'),
('59120', 'Postproducción de películas, videos y programas de TV'),
('59130', 'Distribución de películas cinematográficas y programas de TV'),
('59140', 'Exhibición de películas y cintas de video'),
('59200', 'Edición y grabación de música'),
('60100', 'Servicios de difusión de radio'),
('60201', 'Programación y difusión de TV abierta'),
('60202', 'Suscripción y difusión de TV por cable/suscripción'),
('60299', 'Servicios de televisión (incluye cable)'),
('60900', 'Programación y transmisión de radio y televisión'),
('61101', 'Servicio de telefonía'),
('61102', 'Servicio de Internet'),
('61103', 'Servicio de telefonía fija'),
('61109', 'Servicio de Internet n.c.p.'),
('61201', 'Servicios de telefonía celular'),
('61202', 'Servicios de Internet inalámbrico'),
('61209', 'Telecomunicaciones inalámbricas n.c.p.'),
('61301', 'Telecomunicaciones satelitales'),
('61309', 'Comunicación vía satélite n.c.p.'),
('61900', 'Actividades de telecomunicación n.c.p.'),
('62010', 'Programación informática'),
('62020', 'Consultorías y gestión de servicios informáticos'),
('62090', 'Otras actividades de TI y servicios de computadora'),
('63110', 'Procesamiento de datos y actividades relacionadas'),
('63120', 'Portales web'),
('63910', 'Servicios de agencias de noticias'),
('63990', 'Otros servicios de información n.c.p.'),
('64110', 'Servicios provistos por el Banco Central'),
('64190', 'Bancos'),
('64192', 'Entidades dedicadas al envío de remesas'),
('64199', 'Otras entidades financieras'),
('64200', 'Actividades de sociedades de cartera'),
('64300', 'Fideicomisos, fondos y otras fuentes de financiamiento'),
('64910', 'Arrendamientos financieros'),
('64920', 'Asociaciones cooperativas de ahorro y crédito dedicadas a la intermediación financiera'),
('64921', 'Instituciones emisoras de tarjetas de crédito y otros'),
('64922', 'Tipos de crédito n.c.p.'),
('64928', 'Prestamistas y casas de empeño'),
('64990', 'Actividades de servicios financieros, excepto la financiación de planes de seguros y de pensiones n.c.p.'),
('65110', 'Planes de seguros de vida'),
('65120', 'Planes de seguro excepto de vida'),
('65199', 'Seguros generales de todo tipo'),
('65200', 'Planes de seguro'),
('65300', 'Planes de pensiones'),
('66110', 'Administración de mercados financieros (Bolsa de Valores)'),
('66120', 'Actividades bursátiles (Corredores de Bolsa)'),
('66190', 'Actividades auxiliares de la intermediación financiera n.c.p.'),
('66210', 'Evaluación de riesgos y daños'),
('66220', 'Actividades de agentes y corredores de seguros'),
('66290', 'Otras actividades auxiliares de seguros y fondos de pensiones'),
('66300', 'Actividades de administración de fondos'),
('68101', 'Servicio de alquiler y venta de lotes en cementerios'),
('68109', 'Actividades inmobiliarias realizadas con bienes propios o arrendados n.c.p.'),
('68200', 'Actividades inmobiliarias realizadas a cambio de una retribución o por contrata'),
('69100', 'Actividades jurídicas'),
('69200', 'Actividades de contabilidad, teneduría de libros y auditoría; asesoramiento en materia de impuestos'),
('70100', 'Actividades de oficinas centrales de sociedades de cartera'),
('70200', 'Actividades de consultoría en gestión empresarial'),
('71101', 'Servicios de arquitectura y planificación urbana y servicios conexos'),
('71102', 'Servicios de ingeniería'),
('71103', 'Servicios de agrimensura, topografía, cartografía, prospección y geofísica y servicios conexos'),
('71200', 'Ensayos y análisis técnicos'),
('72100', 'Investigaciones y desarrollo experimental en el campo de las ciencias naturales e ingeniería'),
('72199', 'Investigaciones científicas'),
('72200', 'Investigaciones y desarrollo experimental en el campo de las ciencias sociales y las humanidades; investigación y desarrollo'),
('73100', 'Publicidad'),
('73200', 'Investigación de mercados y realización de encuestas de opinión pública'),
('74100', 'Actividades de diseño especializado'),
('74200', 'Actividades de fotografía'),
('74900', 'Servicios profesionales y científicos n.c.p.'),
('75000', 'Actividades veterinarias'),
('77101', 'Alquiler de equipo de transporte terrestre'),
('77102', 'Alquiler de equipo de transporte acuático'),
('77103', 'Alquiler de equipo de transporte por vía aérea'),
('77210', 'Alquiler y arrendamiento de equipo de recreo y deportivo'),
('77220', 'Alquiler de cintas de video y discos'),
('77290', 'Alquiler de otros efectos personales y enseres domésticos'),
('77300', 'Alquiler de maquinaria y equipo'),
('77400', 'Arrendamiento de productos de propiedad intelectual'),
('78100', 'Obtención y dotación de personal'),
('78200', 'Actividades de las agencias de trabajo temporal'),
('78300', 'Dotación de recursos humanos y gestión; gestión de las funciones de recursos humanos'),
('79110', 'Actividades de agencias de viajes y organizadores de viajes; actividades de asistencia a turistas'),
('79120', 'Actividades de los operadores turísticos'),
('79900', 'Otros servicios de reservas y actividades relacionadas'),
('80100', 'Servicios de seguridad privados'),
('80201', 'Actividades de servicios de sistemas de seguridad'),
('80202', 'Actividades para la prestación de sistemas de seguridad'),
('80300', 'Actividades de investigación'),
('81100', 'Actividades combinadas de mantenimiento de edificios e instalaciones'),
('81210', 'Limpieza general de edificios'),
('81290', 'Otras actividades combinadas de mantenimiento de edificios e instalaciones n.c.p.'),
('81300', 'Servicio de jardinería'),
('82110', 'Servicios administrativos de oficinas'),
('82190', 'Servicios de fotocopiado y similares, excepto en imprentas'),
('82200', 'Actividades de las centrales de llamadas (call center)'),
('82300', 'Organización de convenciones y ferias de negocios'),
('82910', 'Actividades de agencias de cobro y oficinas de crédito'),
('82921', 'Servicios de envase y empaque de productos alimenticios'),
('82922', 'Servicios de envase y empaque de productos medicinales'),
('82929', 'Servicio de envase y empaque n.c.p.'),
('82990', 'Actividades de apoyo empresariales n.c.p.'),
('84110', 'Actividades de la Administración Pública en general'),
('84111', 'Alcaldías Municipales'),
('84120', 'Regulación de actividades sanitarias, educativas, culturales y otros servicios sociales; administración y funcionamiento del Ministerio de Relaciones Exteriores'),
('84220', 'Actividades de defensa'),
('84230', 'Actividades de mantenimiento del orden público y de seguridad'),
('84300', 'Actividades de planes de seguridad social de afiliación obligatoria'),
('85101', 'Guardería educativa'),
('85102', 'Enseñanza preescolar o parvularia'),
('85103', 'Enseñanza primaria'),
('85104', 'Servicio de educación preescolar y primaria integrada'),
('85211', 'Enseñanza secundaria tercer ciclo (7°, 8° y 9°)'),
('85212', 'Enseñanza secundaria de formación general bachillerato'),
('85221', 'Enseñanza secundaria de formación técnica y profesional'),
('85222', 'Enseñanza secundaria técnica y profesional integrada con primaria'),
('85301', 'Enseñanza superior universitaria'),
('85302', 'Enseñanza superior no universitaria'),
('85303', 'Enseñanza superior integrada a secundaria y/o primaria'),
('85410', 'Educación deportiva y recreativa'),
('85420', 'Educación cultural'),
('85490', 'Otros tipos de enseñanza n.c.p.'),
('85499', 'Enseñanza formal'),
('85500', 'Servicios de apoyo a la enseñanza'),
('86100', 'Actividades de hospitales'),
('86201', 'Clínicas médicas'),
('86202', 'Servicios de Odontología'),
('86203', 'Servicios médicos'),
('86901', 'Servicios de análisis y estudios de diagnóstico'),
('86902', 'Actividades de atención de la salud humana'),
('86909', 'Otros servicios relacionados con la salud n.c.p.'),
('87100', 'Residencias de ancianos con atención de enfermería'),
('87200', 'Instituciones para tratamiento de retraso mental, salud mental y uso indebido de sustancias nocivas'),
('87300', 'Instituciones para cuidado de ancianos y discapacitados'),
('87900', 'Actividades de asistencia a niños y jóvenes'),
('87901', 'Otras actividades de atención en instituciones'),
('88100', 'Actividades de asistencia social sin alojamiento para ancianos y discapacitados'),
('88900', 'Servicios sociales sin alojamiento n.c.p.'),
('90000', 'Actividades creativas artísticas y de esparcimiento'),
('91010', 'Actividades de bibliotecas y archivos'),
('91020', 'Actividades de museos y preservación de lugares y edificios históricos'),
('91030', 'Actividades de jardines botánicos, zoológicos y reservas naturales'),
('92000', 'Actividades de juegos y apuestas'),
('93110', 'Gestión de instalaciones deportivas'),
('93120', 'Actividades de clubes deportivos'),
('93190', 'Otras actividades deportivas'),
('93210', 'Actividades de parques de atracciones y parques temáticos'),
('93291', 'Discotecas y salas de baile'),
('93298', 'Centros vacacionales'),
('93299', 'Actividades de esparcimiento n.c.p.'),
('94110', 'Actividades de organizaciones empresariales y de empleadores'),
('94120', 'Actividades de organizaciones profesionales'),
('94200', 'Actividades de sindicatos'),
('94910', 'Actividades de organizaciones religiosas'),
('94920', 'Actividades de organizaciones políticas'),
('94990', 'Actividades de asociaciones n.c.p.'),
('95110', 'Reparación de computadoras y equipo periférico'),
('95120', 'Reparación de equipo de comunicación'),
('95210', 'Reparación de aparatos electrónicos de consumo'),
('95220', 'Reparación de aparatos domésticos y equipo de hogar y jardín'),
('95230', 'Reparación de calzado y artículos de cuero'),
('95240', 'Reparación de muebles y accesorios para el hogar'),
('95291', 'Reparación de instrumentos musicales'),
('95292', 'Servicios de cerrajería y copiado de llaves'),
('95293', 'Reparación de joyas y relojes'),
('95294', 'Reparación de bicicletas, sillas de ruedas y rodados n.c.p.'),
('95299', 'Reparación de enseres personales n.c.p.'),
('96010', 'Lavado y limpieza de prendas de tela y de piel, incluso limpieza en seco'),
('96020', 'Peluquería y otros tratamientos de belleza'),
('96030', 'Pompas fúnebres y actividades conexas'),
('96091', 'Servicios de sauna y otros servicios para la estética corporal n.c.p.'),
('96092', 'Servicios n.c.p.'),
('97000', 'Actividades de los hogares como empleadores de personal doméstico'),
('98100', 'Actividades indiferenciadas de producción de bienes de los hogares privados para uso propio'),
('98200', 'Actividades indiferenciadas de producción de servicios de los hogares privados para uso propio'),
('99000', 'Actividades de organizaciones y órganos extraterritoriales');

--
-- Table: cat_020_pais
--

CREATE TABLE cat_020_pais (
  codigo CHAR(2) NOT NULL,
  descripcion VARCHAR(100) NOT NULL,
  PRIMARY KEY (codigo)
);

-- Data for pais - INSERT STATEMENTS SKIPPED (large dataset)
INSERT INTO cat_020_pais (codigo, descripcion) VALUES
('AD', 'Andorra'),
('AE', 'Emiratos Árabes Unidos'),
('AF', 'Afganistán'),
('AG', 'Antigua y Barbuda'),
('AI', 'Anguilla'),
('AL', 'Albania'),
('AM', 'Armenia'),
('AO', 'Angola'),
('AQ', 'Antártica'),
('AR', 'Argentina'),
('AS', 'Samoa Americana'),
('AT', 'Austria'),
('AU', 'Australia'),
('AW', 'Aruba'),
('AX', 'Aland'),
('AZ', 'Azerbaiyán'),
('BA', 'Bosnia-Herzegovina'),
('BB', 'Barbados'),
('BD', 'Bangladesh'),
('BE', 'Bélgica'),
('BF', 'Burkina Faso'),
('BG', 'Bulgaria'),
('BH', 'Bahrein'),
('BI', 'Burundi'),
('BJ', 'Benin'),
('BL', 'Saint Barthélemy'),
('BM', 'Bermudas'),
('BN', 'Brunei'),
('BO', 'Bolivia'),
('BQ', 'Bonaire, Sint Eustatius and Saba'),
('BR', 'Brasil'),
('BS', 'Bahamas'),
('BT', 'Bután'),
('BV', 'Isla Bouvet'),
('BW', 'Botswana'),
('BY', 'Bielorrusia'),
('BZ', 'Belice'),
('CA', 'Canadá'),
('CC', 'Islas Cocos'),
('CD', 'República Democrática del Congo'),
('CF', 'Centroafricana, República'),
('CG', 'Congo'),
('CH', 'Suiza'),
('CI', 'Costa de Marfil'),
('CK', 'Islas Cook'),
('CL', 'Chile'),
('CM', 'Camerún'),
('CN', 'China'),
('CO', 'Colombia'),
('CR', 'Costa Rica'),
('CU', 'Cuba'),
('CV', 'Cabo Verde'),
('CW', 'Curazao'),
('CX', 'Islas Navidad'),
('CY', 'Chipre'),
('CZ', 'República Checa'),
('DE', 'Alemania'),
('DJ', 'Djiboutí'),
('DK', 'Dinamarca'),
('DM', 'Dominica'),
('DO', 'República Dominicana'),
('DZ', 'Argelia'),
('EC', 'Ecuador'),
('EE', 'Estonia'),
('EG', 'Egipto'),
('EH', 'Sahara Occidental'),
('ER', 'Eritrea'),
('ES', 'España'),
('ET', 'Etiopía'),
('FI', 'Finlandia'),
('FJ', 'Fiji'),
('FK', 'Islas Malvinas (Falkland)'),
('FM', 'Micronesia'),
('FO', 'Islas Faroe'),
('FR', 'Francia'),
('GA', 'Gabón'),
('GB', 'Reino Unido'),
('GD', 'Granada'),
('GE', 'Georgia'),
('GF', 'Guayana Francesa'),
('GG', 'Guernsey'),
('GH', 'Ghana'),
('GI', 'Gibraltar'),
('GL', 'Groenlandia'),
('GM', 'Gambia'),
('GN', 'Guinea'),
('GP', 'Guadalupe'),
('GQ', 'Guinea Ecuatorial'),
('GR', 'Grecia'),
('GS', 'Islas Georgias d. S.-Sandwich d. S.'),
('GT', 'Guatemala'),
('GU', 'Guam'),
('GW', 'Guinea-Bissau'),
('GY', 'Guyana'),
('HK', 'Hong Kong'),
('HM', 'Islas Heard y McDonald'),
('HN', 'Honduras'),
('HR', 'Croacia'),
('HT', 'Haití'),
('HU', 'Hungría'),
('ID', 'Indonesia'),
('IE', 'Irlanda'),
('IL', 'Israel'),
('IM', 'Isla de Man'),
('IN', 'India'),
('IO', 'Territorio Británico Océano Índico'),
('IQ', 'Irak'),
('IR', 'República Islámica de Irán'),
('IS', 'Islandia'),
('IT', 'Italia'),
('JE', 'Jersey'),
('JM', 'Jamaica'),
('JO', 'Jordania'),
('JP', 'Japón'),
('KE', 'Kenia'),
('KG', 'Kirguistán'),
('KH', 'Camboya'),
('KI', 'Kiribati'),
('KM', 'Comoras'),
('KN', 'San Cristóbal y Nieves'),
('KP', 'Rep. Democrática popular de Corea'),
('KR', 'República de Corea'),
('KW', 'Kuwait'),
('KY', 'Caimán, Islas'),
('KZ', 'Kazajistán'),
('LA', 'Laos, República Democrática'),
('LB', 'Líbano'),
('LC', 'Santa Lucía'),
('LI', 'Liechtenstein'),
('LK', 'Sri Lanka'),
('LR', 'Liberia'),
('LS', 'Lesotho'),
('LT', 'Lituania'),
('LU', 'Luxemburgo'),
('LV', 'Letonia'),
('LY', 'Libia'),
('MA', 'Marruecos'),
('MC', 'Mónaco'),
('MD', 'Moldavia, República de'),
('ME', 'Montenegro'),
('MF', 'Saint Martin (French part)'),
('MG', 'Madagascar'),
('MH', 'Islas Marshall'),
('MK', 'Macedonia'),
('ML', 'Malí'),
('MM', 'Myanmar'),
('MN', 'Mongolia'),
('MO', 'Macao'),
('MP', 'Islas Marianas del Norte'),
('MQ', 'Martinica e.a.'),
('MR', 'Mauritania'),
('MS', 'Montserrat'),
('MT', 'Malta'),
('MU', 'Mauricio'),
('MV', 'Maldivas'),
('MW', 'Malawi'),
('MX', 'México'),
('MY', 'Malasia'),
('MZ', 'Mozambique'),
('NA', 'Namibia'),
('NC', 'Nueva Caledonia'),
('NE', 'Níger'),
('NF', 'Isla Norfolk'),
('NG', 'Nigeria'),
('NI', 'Nicaragua'),
('NL', 'Países Bajos'),
('NO', 'Noruega'),
('NP', 'Nepal'),
('NR', 'Nauru'),
('NU', 'Niue'),
('NZ', 'Nueva Zelanda'),
('OM', 'Omán'),
('PA', 'Panamá'),
('PE', 'Perú'),
('PF', 'Polinesia Francesa'),
('PG', 'Papúa, Nueva Guinea'),
('PH', 'Filipinas'),
('PK', 'Pakistán'),
('PL', 'Polonia'),
('PM', 'San Pedro y Miquelón'),
('PN', 'Islas Pitcairn'),
('PR', 'Puerto Rico'),
('PS', 'Palestina'),
('PT', 'Portugal'),
('PW', 'Palaos'),
('PY', 'Paraguay'),
('QA', 'Qatar'),
('RE', 'Reunión'),
('RO', 'Rumania'),
('RS', 'Serbia'),
('RU', 'Rusia'),
('RW', 'Ruanda'),
('SA', 'Arabia Saudita'),
('SB', 'Salomón, Islas'),
('SC', 'Seychelles'),
('SD', 'Sudán'),
('SE', 'Suecia'),
('SG', 'Singapur'),
('SH', 'Santa Elena'),
('SI', 'Eslovenia'),
('SJ', 'Svalbard y Jan Mayen'),
('SK', 'Eslovaquia'),
('SL', 'Sierra Leona'),
('SM', 'San Marino'),
('SN', 'Senegal'),
('SO', 'Somalia'),
('SR', 'Surinam'),
('SS', 'South Sudan'),
('ST', 'Santo Tomé y Príncipe'),
('SV', 'El Salvador'),
('SX', 'Sint Maarten (Dutch part)'),
('SY', 'Siria'),
('SZ', 'Suazilandia'),
('TC', 'Islas Turcas y Caicos'),
('TD', 'Chad'),
('TF', 'Territorios Australes Franceses'),
('TG', 'Togo'),
('TH', 'Tailandia'),
('TJ', 'Tayikistán'),
('TK', 'Tokelau'),
('TL', 'Timor Oriental'),
('TM', 'Turkmenistán'),
('TN', 'Túnez'),
('TO', 'Tonga'),
('TR', 'Turquía'),
('TT', 'Trinidad y Tobago'),
('TV', 'Tuvalu'),
('TW', 'Taiwan, Provincia de China'),
('TZ', 'Tanzania, República Unida de'),
('UA', 'Ucrania'),
('UG', 'Uganda'),
('UM', 'Islas Ultramarinas de E.E.U.U'),
('US', 'Estados Unidos'),
('UY', 'Uruguay'),
('UZ', 'Uzbekistán'),
('VA', 'Ciudad del Vaticano'),
('VC', 'San Vicente y las Granadinas'),
('VE', 'Venezuela'),
('VG', 'Islas Vírgenes Británicas'),
('VI', 'Islas Vírgenes'),
('VN', 'Vietnam'),
('VU', 'Vanuatu'),
('WF', 'Wallis y Fortuna, Islas'),
('WS', 'Samoa'),
('YE', 'Yemen'),
('YT', 'Mayotte'),
('ZA', 'Sudáfrica'),
('ZM', 'Zambia'),
('ZW', 'Zimbabue');


--
-- Table: cat_021_otros_documentos_asociados
--

CREATE TABLE cat_021_otros_documentos_asociados (
  codigo CHAR(1) NOT NULL,
  descripcion VARCHAR(255) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_021_otros_documentos_asociados (codigo, descripcion) VALUES
('1', 'Emisor'),
('2', 'Receptor'),
('3', 'Médico (solo aplica para contribuyentes obligados a la presentación de F-958)'),
('4', 'Transporte (solo aplica para Factura de exportación)');

--
-- Table: cat_022_tipo_doc_ident_receptor
--

CREATE TABLE cat_022_tipo_doc_ident_receptor (
  codigo CHAR(2) NOT NULL,
  descripcion VARCHAR(100) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_022_tipo_doc_ident_receptor (codigo, descripcion) VALUES
('02', 'Carnet de Residente'),
('03', 'Pasaporte'),
('13', 'DUI'),
('36', 'NIT'),
('37', 'Otro');

--
-- Table: cat_023_tipo_doc_contingencia
--

CREATE TABLE cat_023_tipo_doc_contingencia (
  codigo CHAR(2) NOT NULL,
  descripcion VARCHAR(255) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_023_tipo_doc_contingencia (codigo, descripcion) VALUES
('01', 'Factura Electrónico'),
('03', 'Comprobante de Crédito Fiscal Electrónico'),
('04', 'Nota de Remisión Electrónica'),
('05', 'Nota de Crédito Electrónica'),
('06', 'Nota de Débito Electrónica'),
('11', 'Factura de Exportación Electrónica'),
('14', 'Factura de Sujeto Excluido Electrónica');

--
-- Table: cat_024_tipo_invalidacion
--

CREATE TABLE cat_024_tipo_invalidacion (
  codigo CHAR(1) NOT NULL,
  descripcion VARCHAR(255) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_024_tipo_invalidacion (codigo, descripcion) VALUES
('1', 'Error en la Información del Documento Tributario Electrónico a invalidar'),
('2', 'Rescindir de la operación realizada'),
('3', 'Otro');

--
-- Table: cat_025_titulo_remiten_bienes
--

CREATE TABLE cat_025_titulo_remiten_bienes (
  codigo CHAR(2) NOT NULL,
  descripcion VARCHAR(50) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_025_titulo_remiten_bienes (codigo, descripcion) VALUES
('01', 'Depósito'),
('02', 'Propiedad'),
('03', 'Consignación'),
('04', 'Traslado'),
('05', 'Otros');

--
-- Table: cat_026_tipo_donacion
--

CREATE TABLE cat_026_tipo_donacion (
  codigo CHAR(1) NOT NULL,
  descripcion VARCHAR(50) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_026_tipo_donacion (codigo, descripcion) VALUES
('1', 'Efectivo'),
('2', 'Bien'),
('3', 'Servicio');

--
-- Table: cat_027_recinto_fiscal
--

CREATE TABLE cat_027_recinto_fiscal (
  codigo VARCHAR(3) NOT NULL,
  descripcion VARCHAR(100) NOT NULL,
  PRIMARY KEY (codigo)
);

-- Data for recinto_fiscal - INSERT STATEMENTS SKIPPED (large dataset)
INSERT INTO cat_027_recinto_fiscal (codigo, descripcion) VALUES
('01', 'Terrestre San Bartolo'),
('02', 'Marítima de Acajutla'),
('03', 'Aérea de Comalapa'),
('04', 'Terrestre Las Chinamas'),
('05', 'Terrestre La Hachadura'),
('06', 'Terrestre Santa Ana'),
('07', 'Terrestre San Cristóbal'),
('08', 'Terrestre Anguiatú'),
('09', 'Terrestre El Amatillo'),
('10', 'Marítima La Unión'),
('11', 'Terrestre El Poy'),
('12', 'Terrestre Metalío'),
('15', 'Fardos Postales'),
('16', 'Z.F. San Marcos'),
('17', 'Z.F. El Pedregal'),
('18', 'Z.F. San Bartolo'),
('20', 'Z.F. Exportsalva'),
('21', 'Z.F. American Park'),
('23', 'Z.F. Internacional'),
('24', 'Z.F. Diez'),
('26', 'Z.F. Miramar'),
('27', 'Z.F. Santo Tomás'),
('28', 'Z.F. Santa Tecla'),
('29', 'Z.F. Santa Ana'),
('30', 'Z.F. La Concordia'),
('31', 'Aérea Ilopango'),
('32', 'Z.F. Pipil'),
('33', 'Puerto Barillas'),
('34', 'Z.F. Calvo Conservas'),
('35', 'Feria Internacional'),
('36', 'Aduana El Papalón'),
('37', 'Z.F. Sam-Li'),
('38', 'Z.F. San José'),
('39', 'Z.F. Las Mercedes'),
('71', 'Aldesa'),
('72', 'Agdosa Merliot'),
('73', 'Bodesa'),
('76', 'Delegación DHL'),
('77', 'Transauto'),
('80', 'Nejapa'),
('81', 'Almaconsa'),
('83', 'Agdosa Apopa'),
('85', 'Gutiérrez Courier y Cargo'),
('99', 'San Bartolo Envío Hn/Gt');

--
-- Table: cat_028_regimen
--

CREATE TABLE cat_028_regimen (
  codigo VARCHAR(15) NOT NULL,
  descripcion TEXT NOT NULL,
  PRIMARY KEY (codigo)
);

-- Data for regimen - INSERT STATEMENTS SKIPPED (massive dataset)

--
-- Table: cat_029_tipo_persona
--

CREATE TABLE cat_029_tipo_persona (
  codigo CHAR(1) NOT NULL,
  descripcion VARCHAR(50) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_029_tipo_persona (codigo, descripcion) VALUES
('1', 'Persona Natural'),
('2', 'Persona Jurídica');

--
-- Table: cat_030_transporte
--

CREATE TABLE cat_030_transporte (
  codigo CHAR(1) NOT NULL,
  descripcion VARCHAR(50) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_030_transporte (codigo, descripcion) VALUES
('1', 'TERRESTRE'),
('2', 'AÉREO'),
('3', 'MARÍTIMO'),
('4', 'FERRO'),
('5', 'MULTIMODAL'),
('6', 'CORREO');

--
-- Table: cat_031_incoterms
--

CREATE TABLE cat_031_incoterms (
  codigo CHAR(2) NOT NULL,
  descripcion VARCHAR(100) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_031_incoterms (codigo, descripcion) VALUES
('01', 'EXW – En fábrica'),
('02', 'FCA – Libre transportista'),
('03', 'CPT – Transporte pagado hasta'),
('04', 'CIP – Transporte y seguro pagado hasta'),
('05', 'DAP – Entrega en el lugar'),
('06', 'DPU – Entregado en el lugar descargado'),
('07', 'DDP – Entrega con impuestos pagados'),
('08', 'FAS – Libre al costado del buque'),
('09', 'FOB – Libre a bordo'),
('10', 'CFR – Costo y flete'),
('11', 'CIF – Costo, seguro y flete');

--
-- Table: cat_032_domicilio_fiscal
--

CREATE TABLE cat_032_domicilio_fiscal (
  codigo CHAR(1) NOT NULL,
  descripcion VARCHAR(50) NOT NULL,
  PRIMARY KEY (codigo)
);

INSERT INTO cat_032_domicilio_fiscal (codigo, descripcion) VALUES
('1', 'Domiciliado'),
('2', 'No Domiciliado');

--
-- Foreign Key Constraints
--

ALTER TABLE cat_013_municipio 
  ADD CONSTRAINT cat_013_municipio_departamento_fk 
  FOREIGN KEY (departamento_codigo) 
  REFERENCES cat_012_departamento (codigo);

-- Create indexes for better performance
CREATE INDEX idx_cat_municipio_depto ON cat_013_municipio (departamento_codigo);

COMMIT;