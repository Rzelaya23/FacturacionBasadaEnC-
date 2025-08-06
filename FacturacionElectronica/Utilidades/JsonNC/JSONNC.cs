using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class JSONNC
    {
        public IdentificacionNC identificacion { get; set; }
        public List<DocumentoRelacionadoNC> documentoRelacionado { get; set; }
        public EmisorNC emisor { get; set; }
        public ReceptorNC receptor { get; set; }
        public object ventaTercero { get; set; }
        public List<CuerpoDocumentoNC> cuerpoDocumento { get; set; }
        public ResumenNC resumen { get; set; }
        public ExtensionNC extension { get; set; }
        public object apendice { get; set; }
    }
}
