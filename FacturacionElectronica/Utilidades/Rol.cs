using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class Rol
    {
        public string nombre { get; set; }
        public string codigo { get; set; }
        public Object descripcion { get; set; }
        public Object rolSuperior { get; set; }
        public Object nivel { get; set; }
        public Object activo { get; set; }
        public Object permisos { get; set; }
    }
}
