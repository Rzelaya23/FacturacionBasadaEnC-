using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Utilidades;

namespace ConexionSapApp
{
    public class ConexionSQL
    {
        private String StringConection= "Data Source="+Constantes.serverDB+"; Initial Catalog="+Constantes.DB+";User ID="+Constantes.userDB+";Password="+Constantes.passDB;
        
        public ConexionSQL()
        {

        }

        public SqlConnection connection(string serverDB, string DB, string userDB, string passDB)
        {
            SqlConnection connection = new SqlConnection("Data Source=" + serverDB + "; Initial Catalog=" + DB + ";User ID=" + userDB + ";Password=" + passDB);

            return connection;
        }
    }
}
