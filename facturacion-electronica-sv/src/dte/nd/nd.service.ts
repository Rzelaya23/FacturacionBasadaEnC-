import { Injectable, Logger, BadRequestException, InternalServerErrorException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { CreateNdDto } from './dto/create-nd.dto';
import { Dte } from '../../common/entities/dte.entity';
import { CodigoGeneracionService } from '../../common/services/codigo-generacion.service';
import { FirmadorService } from '../../firmador/firmador.service';
import { MhIntegrationService } from '../../mh-integration/mh-integration.service';

@Injectable()
export class NdService {
  private readonly logger = new Logger(NdService.name);

  constructor(
    @InjectRepository(Dte)
    private readonly dteRepository: Repository<Dte>,
    private readonly codigoGeneracionService: CodigoGeneracionService,
    private readonly firmadorService: FirmadorService,
    private readonly mhIntegrationService: MhIntegrationService,
  ) {}

  async create(createNdDto: CreateNdDto): Promise<any> {
    this.logger.log('Iniciando creación de Nota de Débito');
    
    try {
      // 1. Validar datos básicos
      this.validateNdData(createNdDto);

      // 2. Generar código de generación
      const codigoGeneracion = this.codigoGeneracionService.generateCodigoGeneracion();
      this.logger.log(`Código de generación: ${codigoGeneracion}`);

      // 3. Preparar datos para firmado
      const ndData = this.prepareNdData(createNdDto, codigoGeneracion);

      // 4. Firmar documento
      this.logger.log('Enviando documento al firmador...');
      const documentoFirmado = await this.firmadorService.signND(ndData);
      
      if (!documentoFirmado || !documentoFirmado.body) {
        throw new InternalServerErrorException('Error al firmar el documento ND');
      }

      // 5. Guardar en base de datos
      const dteEntity = await this.saveDteToDatabase(createNdDto, codigoGeneracion, documentoFirmado);

      // 6. Enviar al Ministerio de Hacienda
      this.logger.log('Enviando ND al Ministerio de Hacienda...');
      const mhResponse = await this.mhIntegrationService.sendDTE(documentoFirmado.body);

      // 7. Actualizar estado en base de datos
      await this.updateDteStatus(dteEntity.id, mhResponse);

      return {
        success: true,
        message: 'Nota de Débito creada exitosamente',
        data: {
          codigoGeneracion,
          numeroControl: createNdDto.identificacion.numeroControl,
          selloRecibido: mhResponse?.selloRecibido,
          estado: mhResponse?.estado || 'PROCESADO',
          fechaHoraProcesamiento: new Date().toISOString(),
          documentoFirmado: documentoFirmado.body,
          respuestaMH: mhResponse
        }
      };

    } catch (error) {
      this.logger.error('Error al crear Nota de Débito:', error);
      
      if (error instanceof BadRequestException) {
        throw error;
      }
      
      throw new InternalServerErrorException(
        `Error interno al procesar Nota de Débito: ${error.message}`
      );
    }
  }

  async findOne(codigoGeneracion: string): Promise<Dte> {
    this.logger.log(`Buscando ND con código: ${codigoGeneracion}`);
    
    const dte = await this.dteRepository.findOne({
      where: { 
        codigoGeneracion,
        tipoDte: '04' // Código para Nota de Débito
      }
    });

    if (!dte) {
      throw new BadRequestException(`Nota de Débito no encontrada: ${codigoGeneracion}`);
    }

    return dte;
  }

  async findAll(): Promise<Dte[]> {
    this.logger.log('Obteniendo todas las Notas de Débito');
    
    return await this.dteRepository.find({
      where: { tipoDte: '04' },
      order: { createdAt: 'DESC' }
    });
  }

  private validateNdData(createNdDto: CreateNdDto): void {
    // Validar que tenga documentos relacionados
    if (!createNdDto.documentoRelacionado || createNdDto.documentoRelacionado.length === 0) {
      throw new BadRequestException('La Nota de Débito debe tener al menos un documento relacionado');
    }

    // Validar que tenga ítems
    if (!createNdDto.cuerpoDocumento || createNdDto.cuerpoDocumento.length === 0) {
      throw new BadRequestException('La Nota de Débito debe tener al menos un ítem');
    }

    // Validar totales
    const totalCalculado = createNdDto.cuerpoDocumento.reduce((sum, item) => {
      return sum + (item.ventaGravada + item.ventaExenta + item.ventaNoSuj - item.montoDescu);
    }, 0);

    const diferencia = Math.abs(totalCalculado - createNdDto.resumen.subTotal);
    if (diferencia > 0.01) {
      throw new BadRequestException(
        `Los totales no coinciden. Calculado: ${totalCalculado}, Declarado: ${createNdDto.resumen.subTotal}`
      );
    }

    // Validar que el tipo de documento sea ND
    if (createNdDto.identificacion.tipoDte !== '04') {
      throw new BadRequestException('El tipo de DTE debe ser 04 para Nota de Débito');
    }
  }

  private prepareNdData(createNdDto: CreateNdDto, codigoGeneracion: string): any {
    // Actualizar identificación con código generado
    const identificacionActualizada = {
      ...createNdDto.identificacion,
      codigoGeneracion,
      fecEmi: new Date().toISOString().split('T')[0],
      horEmi: new Date().toTimeString().split(' ')[0],
    };

    return {
      identificacion: identificacionActualizada,
      documentoRelacionado: createNdDto.documentoRelacionado,
      emisor: createNdDto.emisor,
      receptor: createNdDto.receptor,
      ventaTercero: createNdDto.ventaTercero || null,
      cuerpoDocumento: createNdDto.cuerpoDocumento,
      resumen: createNdDto.resumen,
      extension: createNdDto.extension || null,
      apendice: createNdDto.apendice || null,
    };
  }

  private async saveDteToDatabase(
    createNdDto: CreateNdDto, 
    codigoGeneracion: string, 
    documentoFirmado: any
  ): Promise<Dte> {
    const dteEntity = this.dteRepository.create({
      codigoGeneracion,
      numeroControl: createNdDto.identificacion.numeroControl,
      tipoDte: '04', // Nota de Débito
      fechaEmision: new Date(createNdDto.identificacion.fecEmi),
      horaEmision: createNdDto.identificacion.horEmi,
      emisorNit: createNdDto.emisor.nit,
      emisorNombre: createNdDto.emisor.nombre,
      receptorDocumento: createNdDto.receptor.numDocumento || null,
      receptorNombre: createNdDto.receptor.nombre || null,
      totalPagar: createNdDto.resumen.totalPagar,
      estadoMh: 'FIRMADO',
      documentoJson: JSON.stringify(createNdDto),
      documentoFirmado: JSON.stringify(documentoFirmado),
    });

    return await this.dteRepository.save(dteEntity);
  }

  private async updateDteStatus(dteId: number, mhResponse: any): Promise<void> {
    const updateData: any = {
      updatedAt: new Date(),
    };

    if (mhResponse) {
      updateData.estadoMh = mhResponse.estado || 'PROCESADO';
      updateData.selloRecibido = mhResponse.selloRecibido;
      updateData.respuestaMh = JSON.stringify(mhResponse);
    } else {
      updateData.estadoMh = 'ERROR_MH';
    }

    await this.dteRepository.update(dteId, updateData);
  }

  // Método para regenerar y reenviar una ND
  async regenerateAndResend(codigoGeneracion: string): Promise<any> {
    this.logger.log(`Regenerando y reenviando ND: ${codigoGeneracion}`);
    
    const dte = await this.findOne(codigoGeneracion);
    const originalData = JSON.parse(dte.documentoJson);
    
    // Generar nuevo código
    const nuevoCodigoGeneracion = this.codigoGeneracionService.generateCodigoGeneracion();
    
    // Actualizar datos
    originalData.identificacion.codigoGeneracion = nuevoCodigoGeneracion;
    originalData.identificacion.fecEmi = new Date().toISOString().split('T')[0];
    originalData.identificacion.horEmi = new Date().toTimeString().split(' ')[0];
    
    // Procesar como nueva ND
    return await this.create(originalData);
  }

  // Método para obtener estadísticas de NDs
  async getEstadisticas(): Promise<any> {
    const total = await this.dteRepository.count({ 
      where: { tipoDte: '04' } 
    });
    
    const procesados = await this.dteRepository.count({ 
      where: { tipoDte: '04', estadoMh: 'PROCESADO' } 
    });
    
    const errores = await this.dteRepository.count({ 
      where: { tipoDte: '04', estadoMh: 'ERROR_MH' } 
    });

    return {
      total,
      procesados,
      errores,
      porcentajeExito: total > 0 ? ((procesados / total) * 100).toFixed(2) : 0
    };
  }
}