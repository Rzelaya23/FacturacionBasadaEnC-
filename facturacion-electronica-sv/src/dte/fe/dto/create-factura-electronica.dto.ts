import { ApiProperty } from '@nestjs/swagger';
import { IsArray, IsOptional, ValidateNested, IsString, IsNumber, Length, Min, IsIn } from 'class-validator';
import { Type } from 'class-transformer';
import { IdentificacionDto } from '../../../common/dto/identificacion.dto';
import { EmisorDto } from '../../../common/dto/emisor.dto';
import { ReceptorDto } from '../../../common/dto/receptor.dto';
import { TributoDto } from '../../../common/dto/tributo.dto';

export class CuerpoDocumentoFeDto {
  @ApiProperty({ 
    description: 'Número de ítem', 
    example: 1,
    minimum: 1 
  })
  @IsNumber()
  @Min(1)
  numItem: number;

  @ApiProperty({ 
    description: 'Tipo de ítem', 
    example: 1,
    enum: [1, 2, 3, 4],
    enumName: 'TipoItem'
  })
  @IsNumber()
  @IsIn([1, 2, 3, 4], { message: 'Tipo ítem debe ser: 1(Bien), 2(Servicio), 3(Bien y Servicio), 4(Otro)' })
  tipoItem: number;

  @ApiProperty({ 
    description: 'Número de documento relacionado (opcional)', 
    example: 'DOC-001',
    required: false,
    maxLength: 36 
  })
  @IsOptional()
  @IsString()
  @Length(1, 36)
  numeroDocumento?: string;

  @ApiProperty({ 
    description: 'Cantidad', 
    example: 5.00,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 8 })
  @Min(0)
  cantidad: number;

  @ApiProperty({ 
    description: 'Código del producto/servicio', 
    example: 'PROD-001',
    maxLength: 25 
  })
  @IsString()
  @Length(1, 25)
  codigo: string;

  @ApiProperty({ 
    description: 'Código de tributo (opcional)', 
    required: false,
    maxLength: 2 
  })
  @IsOptional()
  @IsString()
  @Length(1, 2)
  codTributo?: string;

  @ApiProperty({ 
    description: 'Unidad de medida', 
    example: 1,
    minimum: 1 
  })
  @IsNumber()
  @Min(1)
  uniMedida: number;

  @ApiProperty({ 
    description: 'Descripción del producto/servicio', 
    example: 'Producto de aluminio',
    maxLength: 1000 
  })
  @IsString()
  @Length(1, 1000)
  descripcion: string;

  @ApiProperty({ 
    description: 'Precio unitario', 
    example: 25.50,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 4 })
  @Min(0)
  precioUni: number;

  @ApiProperty({ 
    description: 'Monto de descuento', 
    example: 2.50,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  montoDescu: number;

  @ApiProperty({ 
    description: 'Venta no sujeta', 
    example: 0.00,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  ventaNoSuj: number;

  @ApiProperty({ 
    description: 'Venta exenta', 
    example: 0.00,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  ventaExenta: number;

  @ApiProperty({ 
    description: 'Venta gravada', 
    example: 125.00,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  ventaGravada: number;

  @ApiProperty({ 
    description: 'Lista de tributos aplicables', 
    type: [String],
    example: ['20'],
    required: false 
  })
  @IsOptional()
  @IsArray()
  @IsString({ each: true })
  tributos?: string[];

  @ApiProperty({ 
    description: 'Precio de venta unitario', 
    example: 0.00,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  psv: number;

  @ApiProperty({ 
    description: 'Monto no gravado', 
    example: 0.00,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  noGravado: number;

  @ApiProperty({ 
    description: 'IVA del ítem', 
    example: 16.25,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  ivaItem: number;
}

export class ResumenFeDto {
  @ApiProperty({ 
    description: 'Total no sujeto', 
    example: 0.00,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalNoSuj: number;

  @ApiProperty({ 
    description: 'Total exento', 
    example: 0.00,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalExenta: number;

  @ApiProperty({ 
    description: 'Total gravado', 
    example: 125.00,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalGravada: number;

  @ApiProperty({ 
    description: 'Subtotal de ventas', 
    example: 125.00,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  subTotalVentas: number;

  @ApiProperty({ 
    description: 'Descuento no sujeto', 
    example: 0.00,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  descuNoSuj: number;

  @ApiProperty({ 
    description: 'Descuento exento', 
    example: 0.00,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  descuExenta: number;

  @ApiProperty({ 
    description: 'Descuento gravado', 
    example: 2.50,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  descuGravada: number;

  @ApiProperty({ 
    description: 'Porcentaje de descuento', 
    example: 2.00,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  porcentajeDescuento: number;

  @ApiProperty({ 
    description: 'Total descuento', 
    example: 2.50,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalDescu: number;

  @ApiProperty({ 
    description: 'Lista de tributos del resumen',
    type: [TributoDto]
  })
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => TributoDto)
  tributos: TributoDto[];

  @ApiProperty({ 
    description: 'Subtotal', 
    example: 122.50,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  subTotal: number;

  @ApiProperty({ 
    description: 'IVA retenido 1%', 
    example: 0.00,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  ivaRete1: number;

  @ApiProperty({ 
    description: 'Retención de renta', 
    example: 0.00,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  reteRenta: number;

  @ApiProperty({ 
    description: 'Monto total de la operación', 
    example: 138.75,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  montoTotalOperacion: number;

  @ApiProperty({ 
    description: 'Total no gravado', 
    example: 0.00,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalNoGravado: number;

  @ApiProperty({ 
    description: 'Total a pagar', 
    example: 138.75,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalPagar: number;

  @ApiProperty({ 
    description: 'Total en letras', 
    example: 'CIENTO TREINTA Y OCHO 75/100 DÓLARES',
    maxLength: 200 
  })
  @IsString()
  @Length(1, 200)
  totalLetras: string;

  @ApiProperty({ 
    description: 'Total IVA', 
    example: 16.25,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  totalIva: number;

  @ApiProperty({ 
    description: 'Saldo a favor', 
    example: 0.00,
    minimum: 0 
  })
  @IsNumber({ maxDecimalPlaces: 2 })
  @Min(0)
  saldoFavor: number;

  @ApiProperty({ 
    description: 'Condición de operación', 
    example: 1,
    enum: [1, 2, 3],
    enumName: 'CondicionOperacion'
  })
  @IsNumber()
  @IsIn([1, 2, 3], { message: 'Condición operación debe ser: 1(Contado), 2(Crédito), 3(Otro)' })
  condicionOperacion: number;

  @ApiProperty({ 
    description: 'Información de pagos (opcional)', 
    required: false 
  })
  @IsOptional()
  pagos?: any;

  @ApiProperty({ 
    description: 'Número de pago electrónico (opcional)', 
    required: false,
    maxLength: 100 
  })
  @IsOptional()
  @IsString()
  @Length(1, 100)
  numPagoElectronico?: string;
}

export class ExtensionFeDto {
  @ApiProperty({ 
    description: 'Nombre de quien entrega (opcional)', 
    example: 'Juan Pérez',
    required: false,
    maxLength: 100 
  })
  @IsOptional()
  @IsString()
  @Length(1, 100)
  nombEntrega?: string;

  @ApiProperty({ 
    description: 'Documento de quien entrega (opcional)', 
    example: '12345678-9',
    required: false,
    maxLength: 25 
  })
  @IsOptional()
  @IsString()
  @Length(1, 25)
  docuEntrega?: string;

  @ApiProperty({ 
    description: 'Nombre de quien recibe (opcional)', 
    example: 'María García',
    required: false,
    maxLength: 100 
  })
  @IsOptional()
  @IsString()
  @Length(1, 100)
  nombRecibe?: string;

  @ApiProperty({ 
    description: 'Documento de quien recibe (opcional)', 
    example: '98765432-1',
    required: false,
    maxLength: 25 
  })
  @IsOptional()
  @IsString()
  @Length(1, 25)
  docuRecibe?: string;

  @ApiProperty({ 
    description: 'Observaciones (opcional)', 
    example: 'Entrega en horario de oficina',
    required: false,
    maxLength: 3000 
  })
  @IsOptional()
  @IsString()
  @Length(1, 3000)
  observaciones?: string;

  @ApiProperty({ 
    description: 'Placa del vehículo (opcional)', 
    example: 'P123-456',
    required: false,
    maxLength: 10 
  })
  @IsOptional()
  @IsString()
  @Length(1, 10)
  placaVehiculo?: string;
}

export class CreateFacturaElectronicaDto {
  @ApiProperty({ 
    description: 'Información de identificación del DTE',
    type: IdentificacionDto
  })
  @ValidateNested()
  @Type(() => IdentificacionDto)
  identificacion: IdentificacionDto;

  @ApiProperty({ 
    description: 'Documento relacionado (opcional)', 
    required: false 
  })
  @IsOptional()
  documentoRelacionado?: any;

  @ApiProperty({ 
    description: 'Información del emisor',
    type: EmisorDto
  })
  @ValidateNested()
  @Type(() => EmisorDto)
  emisor: EmisorDto;

  @ApiProperty({ 
    description: 'Información del receptor',
    type: ReceptorDto
  })
  @ValidateNested()
  @Type(() => ReceptorDto)
  receptor: ReceptorDto;

  @ApiProperty({ 
    description: 'Otros documentos (opcional)', 
    required: false 
  })
  @IsOptional()
  otrosDocumentos?: any;

  @ApiProperty({ 
    description: 'Venta a tercero (opcional)', 
    required: false 
  })
  @IsOptional()
  ventaTercero?: any;

  @ApiProperty({ 
    description: 'Cuerpo del documento - lista de ítems',
    type: [CuerpoDocumentoFeDto]
  })
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => CuerpoDocumentoFeDto)
  cuerpoDocumento: CuerpoDocumentoFeDto[];

  @ApiProperty({ 
    description: 'Resumen de la factura',
    type: ResumenFeDto
  })
  @ValidateNested()
  @Type(() => ResumenFeDto)
  resumen: ResumenFeDto;

  @ApiProperty({ 
    description: 'Información de extensión',
    type: ExtensionFeDto
  })
  @ValidateNested()
  @Type(() => ExtensionFeDto)
  extension: ExtensionFeDto;

  @ApiProperty({ 
    description: 'Apéndice (opcional)', 
    required: false 
  })
  @IsOptional()
  apendice?: any;
}