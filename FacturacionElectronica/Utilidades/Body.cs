using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class Body
    {
        public string user { get; set; }
        public string token { get; set; }
        public Rol rol { get; set; }
        public IList<string> roles { get; set; }
        public string tokenType { get; set; }
    }
}
