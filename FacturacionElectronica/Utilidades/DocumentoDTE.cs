using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class DocumentoDTE
    {
        public int docNum { set; get; }
        public string jsonFirmado { set; get; }
        public jsonFirmadorFE jsonFirmadorFE { set; get; }
        public jsonFirmadorCCF jsonFirmadorCCF { set; get; }
        public jsonFirmadorFEX jsonFirmadorFEX { set; get; }
        public jsonFirmadorFSE jsonFirmadorFSE { set; get; }
        public jsonFirmadorNC jsonFirmadorNC { set; get; }
        public jsonFirmadorND jsonFirmadorND { set; get; }
        public jsonFirmadorNR jsonFirmadorNR { set; get; }
    }
}
