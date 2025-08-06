using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{ 
    public class CuerpoDocumentoNC
    {
        public int numItem { get; set; }
        public int tipoItem { get; set; }
        public string numeroDocumento { get; set; }
        public string codigo { get; set; }
        public object codTributo { get; set; }
        public string descripcion { get; set; }
        public Decimal cantidad { get; set; }
        public int uniMedida { get; set; }
        public Decimal precioUni { get; set; }
        public Decimal montoDescu { get; set; }
        public Decimal ventaNoSuj { get; set; }
        public Decimal ventaExenta { get; set; }
        public Decimal ventaGravada { get; set; }
        public List<string> tributos { get; set; }
    }
}
