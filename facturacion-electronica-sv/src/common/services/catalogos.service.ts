import { Injectable, Logger } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import {
  AmbienteDestino,
  TipoDocumento,
  ModeloFacturacion,
  TipoTransmision,
  TipoContingencia,
  TipoItem,
  UnidadMedida,
  Tributo,
  CondicionOperacion,
  FormaPago,
  ActividadEconomica,
  Pais,
  TipoEstablecimiento
} from '../entities';

@Injectable()
export class CatalogosService {
  private readonly logger = new Logger(CatalogosService.name);

  constructor(
    @InjectRepository(AmbienteDestino)
    private ambienteDestinoRepository: Repository<AmbienteDestino>,
    
    @InjectRepository(TipoDocumento)
    private tipoDocumentoRepository: Repository<TipoDocumento>,
    
    @InjectRepository(ModeloFacturacion)
    private modeloFacturacionRepository: Repository<ModeloFacturacion>,
    
    @InjectRepository(TipoTransmision)
    private tipoTransmisionRepository: Repository<TipoTransmision>,
    
    @InjectRepository(TipoContingencia)
    private tipoContingenciaRepository: Repository<TipoContingencia>,
    
    @InjectRepository(TipoItem)
    private tipoItemRepository: Repository<TipoItem>,
    
    @InjectRepository(UnidadMedida)
    private unidadMedidaRepository: Repository<UnidadMedida>,
    
    @InjectRepository(Tributo)
    private tributoRepository: Repository<Tributo>,
    
    @InjectRepository(CondicionOperacion)
    private condicionOperacionRepository: Repository<CondicionOperacion>,
    
    @InjectRepository(FormaPago)
    private formaPagoRepository: Repository<FormaPago>,
    
    @InjectRepository(ActividadEconomica)
    private actividadEconomicaRepository: Repository<ActividadEconomica>,
    
    @InjectRepository(Pais)
    private paisRepository: Repository<Pais>,
    
    @InjectRepository(TipoEstablecimiento)
    private tipoEstablecimientoRepository: Repository<TipoEstablecimiento>,
  ) {}

  // Ambientes de destino
  async getAmbientesDestino(): Promise<AmbienteDestino[]> {
    return this.ambienteDestinoRepository.find();
  }

  async getAmbienteDestino(codigo: string): Promise<AmbienteDestino> {
    return this.ambienteDestinoRepository.findOne({ where: { codigo } });
  }

  // Tipos de documento
  async getTiposDocumento(): Promise<TipoDocumento[]> {
    return this.tipoDocumentoRepository.find();
  }

  async getTipoDocumento(codigo: string): Promise<TipoDocumento> {
    return this.tipoDocumentoRepository.findOne({ where: { codigo } });
  }

  // Modelos de facturación
  async getModelosFacturacion(): Promise<ModeloFacturacion[]> {
    return this.modeloFacturacionRepository.find();
  }

  async getModeloFacturacion(codigo: string): Promise<ModeloFacturacion> {
    return this.modeloFacturacionRepository.findOne({ where: { codigo } });
  }

  // Tipos de transmisión
  async getTiposTransmision(): Promise<TipoTransmision[]> {
    return this.tipoTransmisionRepository.find();
  }

  async getTipoTransmision(codigo: string): Promise<TipoTransmision> {
    return this.tipoTransmisionRepository.findOne({ where: { codigo } });
  }

  // Tipos de contingencia
  async getTiposContingencia(): Promise<TipoContingencia[]> {
    return this.tipoContingenciaRepository.find();
  }

  async getTipoContingencia(codigo: string): Promise<TipoContingencia> {
    return this.tipoContingenciaRepository.findOne({ where: { codigo } });
  }

  // Tipos de ítem
  async getTiposItem(): Promise<TipoItem[]> {
    return this.tipoItemRepository.find();
  }

  async getTipoItem(codigo: string): Promise<TipoItem> {
    return this.tipoItemRepository.findOne({ where: { codigo } });
  }

  // Unidades de medida
  async getUnidadesMedida(): Promise<UnidadMedida[]> {
    return this.unidadMedidaRepository.find();
  }

  async getUnidadMedida(codigo: string): Promise<UnidadMedida> {
    return this.unidadMedidaRepository.findOne({ where: { codigo } });
  }

  // Tributos
  async getTributos(): Promise<Tributo[]> {
    return this.tributoRepository.find();
  }

  async getTributo(codigo: string): Promise<Tributo> {
    return this.tributoRepository.findOne({ where: { codigo } });
  }

  // Condiciones de operación
  async getCondicionesOperacion(): Promise<CondicionOperacion[]> {
    return this.condicionOperacionRepository.find();
  }

  async getCondicionOperacion(codigo: string): Promise<CondicionOperacion> {
    return this.condicionOperacionRepository.findOne({ where: { codigo } });
  }

  // Formas de pago
  async getFormasPago(): Promise<FormaPago[]> {
    return this.formaPagoRepository.find();
  }

  async getFormaPago(codigo: string): Promise<FormaPago> {
    return this.formaPagoRepository.findOne({ where: { codigo } });
  }

  // Actividades económicas
  async getActividadesEconomicas(): Promise<ActividadEconomica[]> {
    return this.actividadEconomicaRepository.find();
  }

  async getActividadEconomica(codigo: string): Promise<ActividadEconomica> {
    return this.actividadEconomicaRepository.findOne({ where: { codigo } });
  }

  async buscarActividadesEconomicas(termino: string): Promise<ActividadEconomica[]> {
    return this.actividadEconomicaRepository
      .createQueryBuilder('actividad')
      .where('actividad.descripcion ILIKE :termino', { termino: `%${termino}%` })
      .orWhere('actividad.codigo LIKE :codigo', { codigo: `${termino}%` })
      .limit(20)
      .getMany();
  }

  // Países
  async getPaises(): Promise<Pais[]> {
    return this.paisRepository.find();
  }

  async getPais(codigo: string): Promise<Pais> {
    return this.paisRepository.findOne({ where: { codigo } });
  }

  // Tipos de establecimiento
  async getTiposEstablecimiento(): Promise<TipoEstablecimiento[]> {
    return this.tipoEstablecimientoRepository.find();
  }

  async getTipoEstablecimiento(codigo: string): Promise<TipoEstablecimiento> {
    return this.tipoEstablecimientoRepository.findOne({ where: { codigo } });
  }

  // Método para validar códigos
  async validarCodigo(tabla: string, codigo: string): Promise<boolean> {
    try {
      let resultado = null;
      
      switch (tabla) {
        case 'ambiente_destino':
          resultado = await this.getAmbienteDestino(codigo);
          break;
        case 'tipo_documento':
          resultado = await this.getTipoDocumento(codigo);
          break;
        case 'modelo_facturacion':
          resultado = await this.getModeloFacturacion(codigo);
          break;
        case 'tipo_transmision':
          resultado = await this.getTipoTransmision(codigo);
          break;
        case 'tipo_contingencia':
          resultado = await this.getTipoContingencia(codigo);
          break;
        case 'tipo_item':
          resultado = await this.getTipoItem(codigo);
          break;
        case 'unidad_medida':
          resultado = await this.getUnidadMedida(codigo);
          break;
        case 'tributo':
          resultado = await this.getTributo(codigo);
          break;
        case 'condicion_operacion':
          resultado = await this.getCondicionOperacion(codigo);
          break;
        case 'forma_pago':
          resultado = await this.getFormaPago(codigo);
          break;
        case 'actividad_economica':
          resultado = await this.getActividadEconomica(codigo);
          break;
        case 'pais':
          resultado = await this.getPais(codigo);
          break;
        case 'tipo_establecimiento':
          resultado = await this.getTipoEstablecimiento(codigo);
          break;
        default:
          return false;
      }
      
      return resultado !== null;
    } catch (error) {
      this.logger.error(`Error validando código ${codigo} en tabla ${tabla}:`, error);
      return false;
    }
  }
}