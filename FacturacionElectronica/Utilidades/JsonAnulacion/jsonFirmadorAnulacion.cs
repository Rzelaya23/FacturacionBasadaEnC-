using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class jsonFirmadorAnulacion
    {
        public string contentType { get; set; }
        public string nit { get; set; }
        public string activo { get; set; }
        public string passwordPri { get; set; }
        public JSONAnulacion dteJson { get; set; }
    }
}
