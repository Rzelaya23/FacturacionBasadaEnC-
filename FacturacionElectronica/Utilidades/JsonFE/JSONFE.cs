using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class JSONFE
    {
        public IdentificacionFE identificacion { get; set; }
        public object documentoRelacionado { get; set; }
        public EmisorFE emisor { get; set; }
        public ReceptorFE receptor { get; set; }
        public object otrosDocumentos { get; set; }
        public object ventaTercero { get; set; }
        public List<CuerpoDocumentoFE> cuerpoDocumento { get; set; }
        public ResumenFE resumen { get; set; }
        public ExtensionFE extension { get; set; }
        public object apendice { get; set; }
    }
}
