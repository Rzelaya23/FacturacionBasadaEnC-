using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConexionSapApp;

namespace FacturacionElectronica
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ConexionUI conexionUI = new ConexionUI();
            conexionUI.connectUI();

            conexionUI.crearMenu();

            conexionUI.inicializacionEventos();

            Application.Run();
        }
    }
}
