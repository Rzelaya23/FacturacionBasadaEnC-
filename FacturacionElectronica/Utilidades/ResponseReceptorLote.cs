using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class ResponseReceptorLote
    {
        public int version { get; set; }
        public string ambiente { get; set; }
        public int versionApp { get; set; }
        public string idEnvio { get; set; }
        public string fhProcesamiento { get; set; }
        public string codigoLote { get; set; }
        public string codigoMsg { get; set; }
        public Object descripcionMsg { get; set; }  
    }
}
