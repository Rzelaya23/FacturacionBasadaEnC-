using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class IdentificacionAnulacion
    {
        public int version { get; set; }
        public string ambiente { get; set; }
        public object codigoGeneracion { get; set; }
        public string fecAnula { get; set; }
        public string horAnula { get; set; }
    }
}
