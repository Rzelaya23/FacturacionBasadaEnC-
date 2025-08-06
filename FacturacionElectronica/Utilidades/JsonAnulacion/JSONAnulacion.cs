using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class JSONAnulacion
    {
        public IdentificacionAnulacion identificacion { get; set; }
        public EmisorAnulacion emisor { get; set; }
        public DocumentoAnulacion documento { get; set; }
        //public ReceptorAnulacion receptor { get; set; }
        public Motivo motivo { get; set; }
    }
}
