using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsumoAPI
{
    public class jsonReceptor
    {
        public string ambiente { get; set; }
        public int idEnvio { get; set; }
        public int version { get; set; }
        public string tipoDte { get; set; }
        public string documento { get; set; }
        public string codigoGeneracion { get; set; }
    }
}
