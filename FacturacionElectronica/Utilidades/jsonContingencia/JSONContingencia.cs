using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades.jsonContingencia
{
    public class JSONContingencia
    {
        public IdentificacionContingencia identificacion { get; set; }
        public EmisorContingencia emisor { get; set; }
        public List<DocsContingencia> detalleDTE { get; set; }
        public MotivoContingencia motivo { get; set; }
    }
}
