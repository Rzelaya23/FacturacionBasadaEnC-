using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilidades;

namespace ConsumoAPI
{
    public class jsonAnular
    {
        public string contentType { get; set; }
        public string ambiente { get; set; }
        public int idEnvio { get; set; }
        public int version { get; set; }
        public string documento { get; set; }
    }
}
