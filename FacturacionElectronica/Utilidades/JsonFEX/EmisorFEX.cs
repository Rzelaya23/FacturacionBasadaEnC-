using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class EmisorFEX
    {
        public string nit { get; set; }
        public string nrc { get; set; }
        public string nombre { get; set; }
        public string codActividad { get; set; }
        public string descActividad { get; set; }
        public string nombreComercial { get; set; }
        public string tipoEstablecimiento { get; set; }
        public Direccion direccion { get; set; }
        public string telefono { get; set; }
        public string correo { get; set; }
        public object codEstableMH { get; set; }
        public object codEstable { get; set; }
        public object codPuntoVentaMH { get; set; }
        public object codPuntoVenta { get; set; }
        public int tipoItemExpor { get; set; }
        public string recintoFiscal { get; set; }
        public string regimen { get; set; }
    }
}
