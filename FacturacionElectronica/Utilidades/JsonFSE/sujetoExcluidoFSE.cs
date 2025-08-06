using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class sujetoExcluidoFSE
    {
        public string tipoDocumento { get; set; }
        public string numDocumento { get; set; }
        public string nombre { get; set; }
        public string codActividad { get; set; }
        public string descActividad { get; set; }
        public Direccion direccion { get; set; }
        public string telefono { get; set; }
        public string correo { get; set; }
    }
}
