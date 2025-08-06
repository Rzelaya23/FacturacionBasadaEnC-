using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class JSONFSE
    {
        public IdentificacionFSE identificacion { get; set; }
        public EmisorFSE emisor { get; set; }
        public sujetoExcluidoFSE sujetoExcluido { get; set; }
        public List<CuerpoDocumentoFSE> cuerpoDocumento { get; set; }
        public ResumenFSE resumen { get; set; }
        public object apendice { get; set; }
    }
}
