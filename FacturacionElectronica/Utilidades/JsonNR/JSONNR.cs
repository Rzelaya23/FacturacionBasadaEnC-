using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class JSONNR
    {
        public IdentificacionNR identificacion { get; set; }
        public object documentoRelacionado { get; set; }
        public EmisorNR emisor { get; set; }
        public ReceptorNR receptor { get; set; }
        public object ventaTercero { get; set; }
        public List<CuerpoDocumentoNR> cuerpoDocumento { get; set; }
        public ResumenNR resumen { get; set; }
        public ExtensionNR extension { get; set; }
        public object apendice { get; set; }
    }
}
