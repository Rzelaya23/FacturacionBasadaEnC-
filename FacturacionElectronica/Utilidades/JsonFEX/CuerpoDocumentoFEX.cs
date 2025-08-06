using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{ 
    public class CuerpoDocumentoFEX
    {
        public int numItem { get; set; }
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public Decimal cantidad { get; set; }
        public int uniMedida { get; set; }
        public Decimal precioUni { get; set; }
        public Decimal montoDescu { get; set; }
        public Decimal ventaGravada { get; set; }
        public List<string> tributos { get; set; }
        public Decimal noGravado { get; set; }
    }
}
