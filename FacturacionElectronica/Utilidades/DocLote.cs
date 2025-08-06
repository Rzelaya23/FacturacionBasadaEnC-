using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class DocLote
    {
        public string tipoDoc { get; set; }
        public int docNum { get; set; }
        public string DocDate { get; set; }
        public string cardCode { get; set; }
        public string cardName { get; set; }
        public Decimal docTotal { get; set; }
        public Decimal iva { get; set; }
    }
}
