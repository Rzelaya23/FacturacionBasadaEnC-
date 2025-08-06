using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class ResumenFSE
    {
        public Decimal totalCompra { get; set; }
        public Decimal descu { get; set; }
        public Decimal totalDescu { get; set; }
        public Decimal subTotal { get; set; }
        public Decimal ivaRete1 { get; set; }
        public Decimal reteRenta { get; set; }
        public Decimal totalPagar { get; set; }
        public string totalLetras { get; set; }
        public int condicionOperacion { get; set; }
        public object pagos { get; set; }
        public object observaciones { get; set; }
    }
}
