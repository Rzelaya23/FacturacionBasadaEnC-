using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class ResumenFEX
    {
        public Decimal totalGravada { get; set; }
        public Decimal descuento { get; set; }
        public Decimal porcentajeDescuento { get; set; }
        public Decimal totalDescu { get; set; }
        public Decimal montoTotalOperacion { get; set; }
        public Decimal totalNoGravado { get; set; }
        public Decimal totalPagar { get; set; }
        public string totalLetras { get; set; }
        public int condicionOperacion { get; set; }
        public object pagos { get; set; }
        public string codIncoterms { get; set; }
        public string descIncoterms { get; set; }
        public string observaciones { get; set; }
        public Decimal flete { get; set; }
        public object numPagoElectronico { get; set; }
        public Decimal seguro { get; set; }
    }
}
