using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class JSONFEX
    {
        public IdentificacionFEX identificacion { get; set; }
        public EmisorFEX emisor { get; set; }
        public ReceptorFEX receptor { get; set; }
        public object otrosDocumentos { get; set; }
        public object ventaTercero { get; set; }
        public List<CuerpoDocumentoFEX> cuerpoDocumento { get; set; }
        public ResumenFEX resumen { get; set; }
        public object apendice { get; set; }
    }
}
