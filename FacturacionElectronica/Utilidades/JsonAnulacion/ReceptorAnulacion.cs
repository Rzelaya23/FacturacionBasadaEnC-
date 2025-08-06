using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class ReceptorAnulacion
    {
        public int tipoDocumento { get; set; }
        public string numDocumento { get; set; }
        public object numFacturador { get; set; }
        public string nombre { get; set; }
        public string telefono { get; set; }
        public string correo { get; set; }
    }
}
