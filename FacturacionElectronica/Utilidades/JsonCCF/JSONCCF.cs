using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class JSONCCF
    {
        public IdentificacionCCF identificacion { get; set; }
        public object documentoRelacionado { get; set; }
        public EmisorCCF emisor { get; set; }
        public ReceptorCCF receptor { get; set; }
        public object otrosDocumentos { get; set; }
        public object ventaTercero { get; set; }
        public List<CuerpoDocumentoCCF> cuerpoDocumento { get; set; }
        public ResumenCCF resumen { get; set; }
        public ExtensionCCF extension { get; set; }
        public object apendice { get; set; }
    }
}
