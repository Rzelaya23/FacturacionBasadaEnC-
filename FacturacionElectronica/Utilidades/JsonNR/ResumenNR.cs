using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class ResumenNR
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
        public List<TributoNR> tributos { get; set; }
        public Decimal subTotal { get; set; }
        public Decimal montoTotalOperacion { get; set; }
        public string totalLetras { get; set; }
    }
}
