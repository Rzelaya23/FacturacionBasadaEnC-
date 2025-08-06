using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class JSONND
    {
        public IdentificacionND identificacion { get; set; }
        public List<DocumentoRelacionadoND> documentoRelacionado { get; set; }
        public EmisorND emisor { get; set; }
        public ReceptorND receptor { get; set; }
        public object ventaTercero { get; set; }
        public List<CuerpoDocumentoND> cuerpoDocumento { get; set; }
        public ResumenND resumen { get; set; }
        public ExtensionND extension { get; set; }
        public object apendice { get; set; }
    }
}
