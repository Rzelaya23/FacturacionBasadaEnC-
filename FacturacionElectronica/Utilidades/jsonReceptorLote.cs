using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsumoAPI
{
    public class jsonReceptorLote
    {
        public string ambiente { get; set; }
        public string idEnvio { get; set; }
        public int version { get; set; }
        public string nitEmisor { get; set; }
        public List<string> documentos { get; set; }
    }
}
