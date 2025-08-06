using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class ResponseAnular
    {
        public int version { get; set; }
        public string ambiente { get; set; }
        public int versionApp { get; set; }
        public string estado { get; set; }
        public string codigoGeneracion { get; set; }
        public string selloRecibido { get; set; }
        public string fhProcesamiento { get; set; }
        public string codigoMsg { get; set; }
        public Object descripcionMsg { get; set; }
        public List<Object> observaciones { get; set; }
    }
}
