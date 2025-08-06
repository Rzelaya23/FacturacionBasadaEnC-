using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class IdentificacionFE
    {
        public int version { get; set; }
        public string ambiente { get; set; }
        public string tipoDte { get; set; }
        public object numeroControl { get; set; }
        public object codigoGeneracion { get; set; }
        public int tipoModelo { get; set; }
        public int tipoOperacion { get; set; }
        public object tipoContingencia { get; set; }
        public object motivoContin { get; set; }
        public string fecEmi { get; set; }
        public string horEmi { get; set; }
        public string tipoMoneda { get; set; }
    }
}
