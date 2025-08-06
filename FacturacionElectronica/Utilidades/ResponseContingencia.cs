using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class ResponseContingencia
    {
        public string estado { get; set; }
        public string fechaHora { get; set; }
        public string mensaje { get; set; }
        public string selloRecibido { get; set; }
        public List<string> observaciones { get; set; }
    }
}
