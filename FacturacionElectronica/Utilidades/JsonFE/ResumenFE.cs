using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class ResumenFE
    {
        public Decimal totalNoSuj { get; set; }
        public Decimal totalExenta { get; set; }
        public Decimal totalGravada { get; set; }
        public Decimal subTotalVentas { get; set; }
        public Decimal descuNoSuj { get; set; }
        public Decimal descuExenta { get; set; }
        public Decimal descuGravada { get; set; }
        public Decimal porcentajeDescuento { get; set; }
        public Decimal totalDescu { get; set; }
        public List<TributoFE> tributos { get; set; }
        public Decimal subTotal { get; set; }
        public Decimal ivaRete1 { get; set; }
        public Decimal reteRenta { get; set; }
        public Decimal montoTotalOperacion { get; set; }
        public Decimal totalNoGravado { get; set; }
        public Decimal totalPagar { get; set; }
        public string totalLetras { get; set; }
        public Decimal totalIva { get; set; }
        public Decimal saldoFavor { get; set; }
        public int condicionOperacion { get; set; }
        public object pagos { get; set; }
        public object numPagoElectronico { get; set; }
    }
}
