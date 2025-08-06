using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;
using Utilidades;

namespace ConexionSapApp
{
    public class Consultas
    {
        private string serverDB;
        private string DB;
        private string userDB;
        private string passDB;
        public Consultas(string serverDB, string DB, string userDB, string passDB)
        {
            this.serverDB = serverDB;
            this.DB = DB;
            this.userDB = userDB;
            this.passDB = passDB;
        }


        public Cliente obtenerInfoClienteFAC(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            Cliente cliente = new Cliente();
            try
            {
                sqlConnection.Open();


                String SQL = "SELECT  " +
                            "	ISNULL(CASE WHEN T0.U_FacSerie IS NULL THEN '' ELSE (SELECT K0.U_ID_HACIENDA FROM [@TIPOSDOCUMENTO] K0 WHERE K0.code=T0.U_FacSerie) END,'') tipoDoc, " +
                            "	ISNULL(T1.MailAddres,ISNULL((SELECT TOP 1 K0.Street FROM CRD1 K0 WHERE K0.cardCode=T1.cardcode),'')) MailAddress, " +
                            "   ISNULL(T1.U_Documento, '') tipoDocumento, " +
                            "   ISNULL(T1.U_NumDocumento,'') numDocumento,   " +
                            "	ISNULL(T1.U_NRC,'') nrc, " +
                            "	ISNULL(T0.U_Nombre_c,CASE WHEN T1.CardName='CONSUMIDOR FINAL' THEN T0.U_Nombre_c ELSE T1.CardName END) nombre, " +
                            "	ISNULL(T1.Phone1,ISNULL(T0.U_Telefono,'')) telefono, " +
                            "	ISNULL(T1.E_Mail,ISNULL(T0.U_Correo,'')) email, " +
                            "   ISNULL(CASE WHEN T0.U_FacSerie IN('FAC','FE','EXP','FEX','FEXE','NSJ','FSE') THEN 1 WHEN T0.U_FacSerie IN('CCFE','CCF','NC','NCL','NDI','NDLE') THEN 3 END,'') version, " +
                            "   ISNULL( T1.U_NOMBRE_COMERCIAL,'') nombreComercial, " +
                            "   ISNULL((SELECT CAST(K0.U_COD_AEC AS VARCHAR) FROM [@FE_ACT_ECON] K0 WHERE K0.Code = T1.U_FE_ACT_ECON),'') actEconomicaCode, " +
                            "   ISNULL((SELECT K0.U_DESC_ACTEC FROM [@FE_ACT_ECON] K0 WHERE K0.Code = T1.U_FE_ACT_ECON),'') actEconomicaName, " +
                            "   ISNULL(T1.U_FE_DEPARTAMENTO, '') departamento, " +
                            "   ISNULL((SELECT K0.U_COD_MUNI FROM [@FE_MUNICIPIO] K0 WHERE K0.Code = T1.U_FE_MUNICIPIO) , '') municipio,  " +
                            "   ISNULL(T1.U_FE_PAIS , '') codPais, " +
                            "   ISNULL((SELECT K0.Name FROM [@FE_PAIS] K0 WHERE K0.Code = T1.U_FE_PAIS), '') pais,  " +
                            "   CAST(ISNULL(T1.U_tipoPersona,0) AS INT) U_tipoPersona, " +
                            "   CASE WHEN T0.DocType = 'I' THEN 1 WHEN T0.DocType = 'S' THEN 2 END tipoItem, " +
                            "   CASE " +
                            "       WHEN T0.BPLid IS NULL THEN 'ALSASA' " +
                            "       ELSE('S' + right('00' + CONVERT([varchar], T0.BPLid), (3))) " +
                            "   END Sucursal, " +
                            "   CAST(CAST(T0.docDate AS DATE) AS VARCHAR) docDate " +
                            "FROM OINV T0 " +
                            "JOIN OCRD T1 ON T0.CardCode=T1.CardCode " +
                            "WHERE T0.docNum=@docNum ";

                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    cliente = new Cliente();
                    cliente.tipoDoc = dr.GetString(0).Equals("") ? null : dr.GetString(0);
                    cliente.direccion = dr.GetString(1).Equals("") ? null : dr.GetString(1);
                    cliente.tipoDocumento = dr.GetString(2).Equals("") ? null : dr.GetString(2);
                    cliente.numDocumento = dr.GetString(3).Equals("") ? null : dr.GetString(3);
                    cliente.nrc = dr.GetString(4).Equals("") ? null : dr.GetString(4);
                    cliente.nombre = dr.GetString(5).Equals("") ? null : dr.GetString(5);
                    cliente.telefono = dr.GetString(6).Equals("") ? null : dr.GetString(6);
                    cliente.correo = dr.GetString(7).Equals("") ? null : dr.GetString(7);
                    cliente.version = dr.GetInt32(8);
                    cliente.nombreComercial = dr.GetString(9).Equals("") ? null : dr.GetString(9);
                    cliente.actEconomicaCode = dr.GetString(10).Equals("") ? null : dr.GetString(10);
                    cliente.actEconomicaName = dr.GetString(11).Equals("") ? null : dr.GetString(11);
                    cliente.departamento = dr.GetString(12).Equals("") ? null : dr.GetString(12);
                    cliente.municipio = dr.GetString(13).Equals("") ? null : dr.GetString(13);
                    cliente.codPais = dr.GetString(14).Equals("") ? null : dr.GetString(14);
                    cliente.pais = dr.GetString(15).Equals("") ? null : dr.GetString(15);
                    cliente.tipoPersona = dr.GetInt32(16);
                    cliente.tipoitem = dr.GetInt32(17);
                    cliente.sucursal = dr.GetString(18);
                    cliente.docDate = dr.GetString(19);

                }
            }
            catch(Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en informacion del cliente. Por favor completar todos los datos del receptor. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerInfoCliente: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return cliente;
        }

        public Cliente obtenerInfoClienteND(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            Cliente cliente = new Cliente();
            try
            {
                sqlConnection.Open();

                String SQL = "SELECT  " +
                            "	'06' tipoDoc, " +
                            "	ISNULL(T1.MailAddres,ISNULL((SELECT TOP 1 K0.Street FROM CRD1 K0 WHERE K0.cardCode=T1.cardcode),'')) MailAddress, " +
                            "	T1.U_Documento tipoDocumento,  " +
                            "   ISNULL(T1.U_NumDocumento,'') numDocumento,   " +
                            "	ISNULL(T1.U_NRC,'') nrc, " +
                            "	T1.CardName nombre, " +
                            "	ISNULL(T1.Phone1,'') telefono, " +
                            "	ISNULL(T1.E_Mail,'') email, " +
                            "   3 version, " +
                            "   ISNULL( T1.U_NOMBRE_COMERCIAL,'') nombreComercial, " +
                            "   ISNULL((SELECT K0.U_COD_AEC FROM [@FE_ACT_ECON] K0 WHERE K0.Code = T1.U_FE_ACT_ECON),'') actEconomicaCode, " +
                            "   ISNULL((SELECT K0.U_DESC_ACTEC FROM [@FE_ACT_ECON] K0 WHERE K0.Code = T1.U_FE_ACT_ECON),'') actEconomicaName, " +
                            "   ISNULL(T1.U_FE_DEPARTAMENTO, '') departamento, " +
                            "   ISNULL((SELECT K0.U_COD_MUNI FROM [@FE_MUNICIPIO] K0 WHERE K0.Code = T1.U_FE_MUNICIPIO) , '') municipio,  " +
                            "   ISNULL(T1.U_FE_PAIS , '') codPais, " +
                            "   ISNULL((SELECT K0.Name FROM [@FE_PAIS] K0 WHERE K0.Code = T1.U_FE_PAIS), '') pais,  " +
                            "   CAST(T1.U_tipoPersona AS INT) U_tipoPersona, " +
                            "   CASE WHEN T0.DocType = 'I' THEN 1 WHEN T0.DocType = 'S' THEN 2 END tipoItem, " +
                            "   CASE " +
                            "       WHEN T0.BPLid IS NULL THEN 'ALSASA' " +
                            "       ELSE('S' + right('00' + CONVERT([varchar], T0.BPLid), (3))) " +
                            "   END Sucursal, " +
                            "   CAST(CAST(T0.docDate AS DATE) AS VARCHAR) docDate " +
                            "FROM OINV T0 " +
                            "JOIN OCRD T1 ON T0.CardCode=T1.CardCode " +
                            "WHERE T0.docNum=@docNum ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    cliente = new Cliente();
                    cliente.tipoDoc = dr.GetString(0);
                    cliente.direccion = dr.GetString(1);
                    cliente.tipoDocumento = dr.GetString(2);
                    cliente.numDocumento = dr.GetString(3);
                    cliente.nrc = dr.GetString(4).Equals("") ? null : dr.GetString(4);
                    cliente.nombre = dr.GetString(5);
                    cliente.telefono = dr.GetString(6);
                    cliente.correo = dr.GetString(7);
                    cliente.version = dr.GetInt32(8);
                    cliente.nombreComercial = dr.GetString(9).Equals("") ? null : dr.GetString(9);
                    cliente.actEconomicaCode = dr.GetString(10);
                    cliente.actEconomicaName = dr.GetString(11);
                    cliente.departamento = dr.GetString(12);
                    cliente.municipio = dr.GetString(13);
                    cliente.codPais = dr.GetString(14);
                    cliente.pais = dr.GetString(15);
                    cliente.tipoPersona = dr.GetInt32(16);
                    cliente.tipoitem = dr.GetInt32(17);
                    cliente.sucursal = dr.GetString(18);
                    cliente.docDate = dr.GetString(19);

                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en informacion del cliente. Por favor completar todos los datos del receptor. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerInfoCliente: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return cliente;
        }

        public Cliente obtenerInfoClienteNC(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            Cliente cliente = new Cliente();
            try
            {
                sqlConnection.Open();

                String SQL = "SELECT  " +
                            "	'05' tipoDoc, " +
                            "	ISNULL(T1.MailAddres,ISNULL((SELECT TOP 1 K0.Street FROM CRD1 K0 WHERE K0.cardCode=T1.cardcode),'')) MailAddress, " +
                            "	T1.U_Documento tipoDocumento,  " +
                            "   ISNULL(T1.U_NumDocumento,'') numDocumento,   " +
                            "	ISNULL(T1.U_NRC,'') nrc, " +
                            "	T1.CardName nombre, " +
                            "	ISNULL(T1.Phone1,'') telefono, " +
                            "	ISNULL(T1.E_Mail,'') email, " +
                            "   3 version, " +
                            "   ISNULL( T1.U_NOMBRE_COMERCIAL,'') nombreComercial, " +
                            "   ISNULL((SELECT K0.U_COD_AEC FROM [@FE_ACT_ECON] K0 WHERE K0.Code = T1.U_FE_ACT_ECON),'') actEconomicaCode, " +
                            "   ISNULL((SELECT K0.U_DESC_ACTEC FROM [@FE_ACT_ECON] K0 WHERE K0.Code = T1.U_FE_ACT_ECON),'') actEconomicaName, " +
                            "   ISNULL(T1.U_FE_DEPARTAMENTO, '') departamento, " +
                            "   ISNULL((SELECT K0.U_COD_MUNI FROM [@FE_MUNICIPIO] K0 WHERE K0.Code = T1.U_FE_MUNICIPIO) , '') municipio,  " +
                            "   ISNULL(T1.U_FE_PAIS , '') codPais, " +
                            "   ISNULL((SELECT K0.Name FROM [@FE_PAIS] K0 WHERE K0.Code = T1.U_FE_PAIS), '') pais,  " +
                            "   CAST(T1.U_tipoPersona AS INT) U_tipoPersona, " +
                            "   CASE WHEN T0.DocType = 'I' THEN 1 WHEN T0.DocType = 'S' THEN 2 END tipoItem, " +
                            "   CASE " +
                            "       WHEN T0.BPLid IS NULL THEN 'ALSASA' " +
                            "       ELSE('S' + right('00' + CONVERT([varchar], T0.BPLid), (3))) " +
                            "   END Sucursal, " +
                            "   CAST(CAST(T0.docDate AS DATE) AS VARCHAR) docDate " +
                            "FROM ORIN T0 " +
                            "JOIN OCRD T1 ON T0.CardCode=T1.CardCode " +
                            "WHERE T0.docNum=@docNum ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    cliente = new Cliente();
                    cliente.tipoDoc = dr.GetString(0);
                    cliente.direccion = dr.GetString(1);
                    cliente.tipoDocumento = dr.GetString(2);
                    cliente.numDocumento = dr.GetString(3);
                    cliente.nrc = dr.GetString(4).Equals("") ? null : dr.GetString(4);
                    cliente.nombre = dr.GetString(5);
                    cliente.telefono = dr.GetString(6);
                    cliente.correo = dr.GetString(7);
                    cliente.version = dr.GetInt32(8);
                    cliente.nombreComercial = dr.GetString(9).Equals("") ? null : dr.GetString(9);
                    cliente.actEconomicaCode = dr.GetString(10);
                    cliente.actEconomicaName = dr.GetString(11);
                    cliente.departamento = dr.GetString(12);
                    cliente.municipio = dr.GetString(13);
                    cliente.codPais = dr.GetString(14);
                    cliente.pais = dr.GetString(15);
                    cliente.tipoPersona = dr.GetInt32(16);
                    cliente.tipoitem = dr.GetInt32(17);
                    cliente.sucursal = dr.GetString(18);
                    cliente.docDate = dr.GetString(19);

                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en informacion del cliente. Por favor completar todos los datos del receptor. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerInfoCliente: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return cliente;
        }

        public Cliente obtenerInfoClienteNR(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            Cliente cliente = new Cliente();
            try
            {
                sqlConnection.Open();

                String SQL = "SELECT  " +
                            "	'04' tipoDoc, " +
                            "	ISNULL(T1.MailAddres,ISNULL((SELECT TOP 1 K0.Street FROM CRD1 K0 WHERE K0.cardCode=T1.cardcode),'')) MailAddress, " +
                            "	T1.U_Documento tipoDocumento,  " +
                            "   ISNULL(T1.U_NumDocumento,'') numDocumento,   " +
                            "	ISNULL(T1.U_NRC,'') nrc, " +
                            "	T1.CardName nombre, " +
                            "	ISNULL(T1.Phone1,'') telefono, " +
                            "	ISNULL(T1.E_Mail,'') email, " +
                            "   3 version, " +
                            "   ISNULL( T1.U_NOMBRE_COMERCIAL,'') nombreComercial, " +
                            "   ISNULL((SELECT K0.U_COD_AEC FROM [@FE_ACT_ECON] K0 WHERE K0.Code = T1.U_FE_ACT_ECON),'') actEconomicaCode, " +
                            "   ISNULL((SELECT K0.U_DESC_ACTEC FROM [@FE_ACT_ECON] K0 WHERE K0.Code = T1.U_FE_ACT_ECON),'') actEconomicaName, " +
                            "   ISNULL(T1.U_FE_DEPARTAMENTO, '') departamento, " +
                            "   ISNULL((SELECT K0.U_COD_MUNI FROM [@FE_MUNICIPIO] K0 WHERE K0.Code = T1.U_FE_MUNICIPIO) , '') municipio,  " +
                            "   ISNULL(T1.U_FE_PAIS , '') codPais, " +
                            "   ISNULL((SELECT K0.Name FROM [@FE_PAIS] K0 WHERE K0.Code = T1.U_FE_PAIS), '') pais,  " +
                            "   CAST(T1.U_tipoPersona AS INT) U_tipoPersona, " +
                            "   CASE WHEN T0.DocType = 'I' THEN 1 WHEN T0.DocType = 'S' THEN 2 END tipoItem, " +
                            "   CASE " +
                            "       WHEN T0.BPLid IS NULL THEN 'ALSASA' " +
                            "       ELSE('S' + right('00' + CONVERT([varchar], T0.BPLid), (3))) " +
                            "   END Sucursal, " +
                            "   T0.U_E_bienTitulo, " +
                            "   CAST(CAST(T0.docDate AS DATE) AS VARCHAR) docDate " +
                            "FROM OWTR T0 " +
                            "JOIN OCRD T1 ON T0.CardCode=T1.CardCode " +
                            "WHERE T0.docNum=@docNum ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    cliente = new Cliente();
                    cliente.tipoDoc = dr.GetString(0);
                    cliente.direccion = dr.GetString(1);
                    cliente.tipoDocumento = dr.GetString(2);
                    cliente.numDocumento = dr.GetString(3);
                    cliente.nrc = dr.GetString(4).Equals("") ? null : dr.GetString(4);
                    cliente.nombre = dr.GetString(5);
                    cliente.telefono = dr.GetString(6);
                    cliente.correo = dr.GetString(7);
                    cliente.version = dr.GetInt32(8);
                    cliente.nombreComercial = dr.GetString(9).Equals("") ? null : dr.GetString(9);
                    cliente.actEconomicaCode = dr.GetString(10);
                    cliente.actEconomicaName = dr.GetString(11);
                    cliente.departamento = dr.GetString(12);
                    cliente.municipio = dr.GetString(13);
                    cliente.codPais = dr.GetString(14);
                    cliente.pais = dr.GetString(15);
                    cliente.tipoPersona = dr.GetInt32(16);
                    cliente.tipoitem = dr.GetInt32(17);
                    cliente.sucursal = dr.GetString(18);
                    cliente.bienTitulo = dr.GetString(19);
                    cliente.docDate = dr.GetString(20);

                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en informacion del cliente. Por favor completar todos los datos del receptor. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerInfoCliente: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return cliente;
        }

        public Cliente obtenerInfoClienteNRT(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            Cliente cliente = new Cliente();
            try
            {
                sqlConnection.Open();

                String SQL = "SELECT  " +
                            "	'04' tipoDoc, " +
                            "	ISNULL(T1.MailAddres,ISNULL((SELECT TOP 1 K0.Street FROM CRD1 K0 WHERE K0.cardCode=T1.cardcode),'')) MailAddress, " +
                            "	T1.U_Documento tipoDocumento,  " +
                            "   ISNULL(T1.U_NumDocumento,'') numDocumento,   " +
                            "	ISNULL(T1.U_NRC,'') nrc, " +
                            "	T1.CardName nombre, " +
                            "	ISNULL(T1.Phone1,'') telefono, " +
                            "	ISNULL(T1.E_Mail,'') email, " +
                            "   3 version, " +
                            "   ISNULL( T1.U_NOMBRE_COMERCIAL,'') nombreComercial, " +
                            "   ISNULL((SELECT K0.U_COD_AEC FROM [@FE_ACT_ECON] K0 WHERE K0.Code = T1.U_FE_ACT_ECON),'') actEconomicaCode, " +
                            "   ISNULL((SELECT K0.U_DESC_ACTEC FROM [@FE_ACT_ECON] K0 WHERE K0.Code = T1.U_FE_ACT_ECON),'') actEconomicaName, " +
                            "   ISNULL(T1.U_FE_DEPARTAMENTO, '') departamento, " +
                            "   ISNULL((SELECT K0.U_COD_MUNI FROM [@FE_MUNICIPIO] K0 WHERE K0.Code = T1.U_FE_MUNICIPIO) , '') municipio,  " +
                            "   ISNULL(T1.U_FE_PAIS , '') codPais, " +
                            "   ISNULL((SELECT K0.Name FROM [@FE_PAIS] K0 WHERE K0.Code = T1.U_FE_PAIS), '') pais,  " +
                            "   CAST(T1.U_tipoPersona AS INT) U_tipoPersona, " +
                            "   CASE WHEN T0.DocType = 'I' THEN 1 WHEN T0.DocType = 'S' THEN 2 END tipoItem, " +
                            "   CASE " +
                            "       WHEN T0.BPLid IS NULL THEN 'ALSASA' " +
                            "       ELSE('S' + right('00' + CONVERT([varchar], T0.BPLid), (3))) " +
                            "   END Sucursal, " +
                            "   T0.U_E_bienTitulo, " +
                            "   CAST(CAST(T0.docDate AS DATE) AS VARCHAR) docDate " +
                            "FROM OWTR T0 " +
                            "JOIN OCRD T1 ON T0.CardCode=T1.CardCode " +
                            "WHERE T0.docNum=@docNum ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    cliente = new Cliente();
                    cliente.tipoDoc = dr.GetString(0);
                    cliente.direccion = dr.GetString(1);
                    cliente.tipoDocumento = dr.GetString(2);
                    cliente.numDocumento = dr.GetString(3);
                    cliente.nrc = dr.GetString(4).Equals("") ? null : dr.GetString(4);
                    cliente.nombre = dr.GetString(5);
                    cliente.telefono = dr.GetString(6);
                    cliente.correo = dr.GetString(7);
                    cliente.version = dr.GetInt32(8);
                    cliente.nombreComercial = dr.GetString(9).Equals("") ? null : dr.GetString(9);
                    cliente.actEconomicaCode = dr.GetString(10);
                    cliente.actEconomicaName = dr.GetString(11);
                    cliente.departamento = dr.GetString(12);
                    cliente.municipio = dr.GetString(13);
                    cliente.codPais = dr.GetString(14);
                    cliente.pais = dr.GetString(15);
                    cliente.tipoPersona = dr.GetInt32(16);
                    cliente.tipoitem = dr.GetInt32(17);
                    cliente.sucursal = dr.GetString(18);
                    cliente.bienTitulo = dr.GetString(19);
                    cliente.docDate = dr.GetString(20);

                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en informacion del cliente. Por favor completar todos los datos del receptor. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerInfoCliente: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return cliente;
        }

        public Cliente obtenerInfoClienteFSE(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            Cliente cliente = new Cliente();
            try
            {
                sqlConnection.Open();

                String SQL = "SELECT  " +
                            "	'14' tipoDoc, " +
                            "	ISNULL(T1.MailAddres,ISNULL((SELECT TOP 1 K0.Street FROM CRD1 K0 WHERE K0.cardCode=T1.cardcode),'')) MailAddress, " +
                            "	T1.U_Documento tipoDocumento,  " +
                            "   ISNULL(T1.U_NumDocumento,'') numDocumento,   " +
                            "	ISNULL(T1.U_NRC,'') nrc, " +
                            "	T1.CardName nombre, " +
                            "	ISNULL(T1.Phone1,'') telefono, " +
                            "	ISNULL(T1.E_Mail,'') email, " +
                            "   1 version, " +
                            "   ISNULL( T1.U_NOMBRE_COMERCIAL,'') nombreComercial, " +
                            "   ISNULL((SELECT K0.U_COD_AEC FROM [@FE_ACT_ECON] K0 WHERE K0.Code = T1.U_FE_ACT_ECON),'') actEconomicaCode, " +
                            "   ISNULL((SELECT K0.U_DESC_ACTEC FROM [@FE_ACT_ECON] K0 WHERE K0.Code = T1.U_FE_ACT_ECON),'') actEconomicaName, " +
                            "   ISNULL(T1.U_FE_DEPARTAMENTO, '') departamento, " +
                            "   ISNULL((SELECT K0.U_COD_MUNI FROM [@FE_MUNICIPIO] K0 WHERE K0.Code = T1.U_FE_MUNICIPIO) , '') municipio,  " +
                            "   ISNULL(T1.U_FE_PAIS , '') codPais, " +
                            "   ISNULL((SELECT K0.Name FROM [@FE_PAIS] K0 WHERE K0.Code = T1.U_FE_PAIS), '') pais,  " +
                            "   CAST(T1.U_tipoPersona AS INT) U_tipoPersona, " +
                            "   CASE WHEN T0.DocType = 'I' THEN 1 WHEN T0.DocType = 'S' THEN 2 END tipoItem, " +
                            "   CASE " +
                            "       WHEN T0.BPLid IS NULL THEN 'ALSASA' " +
                            "       ELSE('S' + right('00' + CONVERT([varchar], T0.BPLid), (3))) " +
                            "   END Sucursal, " +
                            "   T0.U_E_bienTitulo, " +
                            "   CAST(CAST(T0.docDate AS DATE) AS VARCHAR) docDate " +
                            "FROM OPCH T0 " +
                            "JOIN OCRD T1 ON T0.CardCode=T1.CardCode " +
                            "WHERE T0.docNum=@docNum ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    cliente = new Cliente();
                    cliente.tipoDoc = dr.GetString(0);
                    cliente.direccion = dr.GetString(1);
                    cliente.tipoDocumento = dr.GetString(2);
                    cliente.numDocumento = dr.GetString(3).Replace("-","");
                    cliente.nrc = dr.GetString(4).Equals("") ? null : dr.GetString(4);
                    cliente.nombre = dr.GetString(5);
                    cliente.telefono = dr.GetString(6);
                    cliente.correo = dr.GetString(7);
                    cliente.version = dr.GetInt32(8);
                    cliente.nombreComercial = dr.GetString(9).Equals("") ? null : dr.GetString(9);
                    cliente.actEconomicaCode = dr.GetString(10);
                    cliente.actEconomicaName = dr.GetString(11);
                    cliente.departamento = dr.GetString(12);
                    cliente.municipio = dr.GetString(13);
                    cliente.codPais = dr.GetString(14);
                    cliente.pais = dr.GetString(15);
                    cliente.tipoPersona = dr.GetInt32(16);
                    cliente.tipoitem = dr.GetInt32(17);
                    cliente.sucursal = dr.GetString(18);
                    cliente.docDate = dr.GetString(20);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en informacion del cliente. Por favor completar todos los datos del receptor. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerInfoCliente: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return cliente;
        }

        public Cliente obtenerInfoClienteAnul(String cardCode)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            Cliente cliente = new Cliente();
            try
            {
                sqlConnection.Open();

                String SQL = "SELECT " +
                            "   ISNULL(CAST(T0.U_Documento AS VARCHAR ),'') tipoDoc, " +
                            "	ISNULL(T0.U_NumDocumento,'') U_NumDocumento, " +
                            "	ISNULL(T0.CardName,'') CardName, " +
                            "	ISNULL(T0.Phone1,'') Phone1, " +
                            "	ISNULL(T0.E_Mail,'') E_Mail " +
                            "FROM OCRD T0 WHERE T0.CardCode = @cardCode ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@cardCode", cardCode);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    cliente = new Cliente();
                    cliente.tipoDocumento = dr.GetString(0).Equals("") ? null : dr.GetString(0);
                    cliente.numDocumento = dr.GetString(1).Equals("") ? null : dr.GetString(1);
                    cliente.nombre = dr.GetString(2).Equals("") ? null : dr.GetString(2);
                    cliente.telefono = dr.GetString(3).Equals("") ? null : dr.GetString(3);
                    cliente.correo = dr.GetString(4).Equals("") ? null : dr.GetString(4);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en informacion del cliente. Por favor completar todos los datos del receptor. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerInfoCliente: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return cliente;
        }

        public DocumentoAnulacion obtenerInfoDoc(int docNum,String tipodoc)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            DocumentoAnulacion documento = new DocumentoAnulacion();
            try
            {
                sqlConnection.Open();
                String SQL = "";
                if (tipodoc == "05")
                {

                    SQL = "SELECT  " +
                        "   CAST(T0.DocNum AS VARCHAR) DocNum, " +
                        "   T0.U_E_CODGENE, " +
                        "   T0.U_E_NUMCONT, " +
                        "   T0.U_E_SELLRECEP, " +
                        "   CAST(CAST(CAST(T0.U_E_FECHORA AS VARCHAR) AS DATE) AS VARCHAR) U_E_FECHORA, " +
                        "   CAST(T0.VatSum AS NUMERIC(19, 2)) montoIva " +
                        "FROM ORIN T0 " +
                        "WHERE T0.docNum=@docNum ";
                }
                else if (tipodoc == "04")
                {
                    SQL = "SELECT  " +
                        "   CAST(T0.DocNum AS VARCHAR) DocNum, " +
                        "   T0.U_E_CODGENE, " +
                        "   T0.U_E_NUMCONT, " +
                        "   T0.U_E_SELLRECEP, " +
                        "   CAST(CAST(CAST(T0.U_E_FECHORA AS VARCHAR) AS DATE) AS VARCHAR) U_E_FECHORA, " +
                        "   CAST(T0.VatSum AS NUMERIC(19, 2)) montoIva " +
                        "FROM OWTR T0 " +
                        "JOIN OCRD T1 ON T0.CardCode=T1.CardCode " +
                        "WHERE T0.docNum=@docNum ";
                }else if (tipodoc == "14")
                {
                    SQL = "SELECT  " +
                        "   CAST(T0.DocNum AS VARCHAR) DocNum, " +
                        "   T0.U_E_CODGENE, " +
                        "   T0.U_E_NUMCONT, " +
                        "   T0.U_E_SELLRECEP, " +
                        "   CAST(CAST(CAST(T0.U_E_FECHORA AS VARCHAR) AS DATE) AS VARCHAR) U_E_FECHORA, " +
                        "   CAST(T0.VatSum AS NUMERIC(19, 2)) montoIva " +
                        "FROM OPCH T0 " +
                        "JOIN OCRD T1 ON T0.CardCode=T1.CardCode " +
                        "WHERE T0.docNum=@docNum ";
                }
                else
                {
                    SQL="SELECT  " +
                        "   CAST(T0.DocNum AS VARCHAR) DocNum, " +
                        "   T0.U_E_CODGENE, " +
                        "   T0.U_E_NUMCONT, " +
                        "   T0.U_E_SELLRECEP, " +
                        "   CAST(CAST(CAST(T0.U_E_FECHORA AS VARCHAR) AS DATE) AS VARCHAR) U_E_FECHORA, " +
                        "   CAST(T0.VatSum AS NUMERIC(19, 2)) montoIva " +
                        "FROM OINV T0 " +
                        "JOIN OCRD T1 ON T0.CardCode=T1.CardCode " +
                        "WHERE T0.docNum=@docNum ";
                }
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    documento = new DocumentoAnulacion();
                    documento.tipoDte = dr.GetString(0);
                    documento.codigoGeneracion = dr.GetString(1);
                    documento.numeroControl = dr.GetString(2);
                    documento.selloRecibido = dr.GetString(3);
                    documento.fecEmi = dr.GetString(4);
                    documento.montoIva = dr.GetDecimal(5);

                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en información del documento a anular. Por favor revisar la información del que desea anular. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerInfoDoc: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return documento;
        }

        public EmisorDTE obtenerInfoEmisor()
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            EmisorDTE emisorDTE = new EmisorDTE();
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT  " +
                            "    T0.U_Nombre, " +
                            "    T0.U_NombreComercial, " +
                            "    T0.U_nit, " +
                            "    T0.U_nrc, " +
                            "    T0.U_ActEcon, " +
                            "    (SELECT K0.U_DESC_ACTEC FROM [@FE_ACT_ECON] K0 WHERE K0.U_COD_AEC=T0.U_ActEcon) descActEcon, " +
                            "    T0.U_tipoEstablecimiento, " +
                            "    T0.U_departamento, " +
                            "    (SELECT K0.U_COD_MUNI FROM [@FE_MUNICIPIO] K0 WHERE K0.Code=T0.U_municipio) municipio, " +
                            "    T0.U_direccion, " +
                            "    T0.U_telefono, " +
                            "    T0.U_correo, " +
                            "    T0.U_userDTE, " +
                            "    T0.U_passDTE, " +
                            "    T0.U_passPrivDTE, " +
                            "    T0.U_AMBIENTE " +
                            "FROM [@FE_EMISOR] T0 ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    emisorDTE.nombre = dr.GetString(0);
                    emisorDTE.nombreComercial = dr.GetString(1);
                    emisorDTE.nit = dr.GetString(2);
                    emisorDTE.nrc = dr.GetString(3);
                    emisorDTE.codActividad = dr.GetString(4);
                    emisorDTE.descActividad = dr.GetString(5);
                    emisorDTE.tipoEstablecimiento = dr.GetString(6);
                    emisorDTE.departamento = dr.GetString(7);
                    emisorDTE.municipio = dr.GetString(8);
                    emisorDTE.direccion = dr.GetString(9);
                    emisorDTE.telefono = dr.GetString(10);
                    emisorDTE.correo = dr.GetString(11);
                    emisorDTE.userDTE = dr.GetString(12);
                    emisorDTE.passDTE = dr.GetString(13);
                    emisorDTE.passPrivDTE = dr.GetString(14);
                    emisorDTE.ambiente = dr.GetString(15);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en informacion del emisor. Por favor completar todos los datos del emisor. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerEmisor: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return emisorDTE;
        }

        public List<CuerpoDocumentoFE> obtenerCuerpoDocFE(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            CuerpoDocumentoFE cuerpoDocumento;
            List<CuerpoDocumentoFE> CuerpoDoc = new List<CuerpoDocumentoFE>();
            List<string> tributos = new List<string>();
            tributos.Add("20");
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT " +
                            "   CAST(ROW_NUMBER() OVER(ORDER BY T0.lineNum ASC) AS INT) numItem, " +
                            "   CASE WHEN T1.DocType = 'I' THEN 1 WHEN T1.DocType = 'S' THEN 2 END tipoItem, " +
                            "   T0.Quantity cantidad, " +
                            "   T0.ItemCode codigo, " +
                            "   ISNULL((SELECT CAST(K0.U_FE_UMEDIDA AS INT) FROM OITM K0 WHERE K0.ItemCode=T0.ItemCode),59) unidadMedida, " +
                            "   T0.Dscription description, " +
                            "   CASE WHEN T0.taxCode LIKE '%EXE%' THEN CAST(T0.Price AS NUMERIC(19, 6)) ELSE CAST(T0.PriceAfVAT AS NUMERIC(19, 6)) END precioUni, " +
                            "   0.00 montoDescu, " +
                            "   0.00 ventaNoSuj, " +
                            "   CASE WHEN T0.taxCode LIKE '%EXE%' THEN CAST(T0.GTotal AS NUMERIC(19, 6)) ELSE 0 END ventaExenta, " +
                            "   CAST(T0.VatSum AS NUMERIC(19, 6)) iva, " +
                            "   CASE WHEN T0.taxCode LIKE '%EXE%' THEN 0 ELSE CAST(T0.GTotal AS NUMERIC(19, 6)) END ventaGravada,  " +
                            "   T1.docNum numDocumento  " +
                            "FROM INV1 T0 " +
                            "JOIN OINV T1 ON T0.docEntry = T1.docEntry " +
                            "WHERE docNum = @docNum " +
                            "ORDER BY T0.LineNum ASC ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    cuerpoDocumento = new CuerpoDocumentoFE();

                    cuerpoDocumento.numItem = dr.GetInt32(0);
                    cuerpoDocumento.tipoItem = dr.GetInt32(1);
                    cuerpoDocumento.cantidad = dr.GetDecimal(2);
                    if (!dr.GetString(3).Equals(""))
                    {
                        cuerpoDocumento.codigo = dr.GetString(3);
                    }
                    cuerpoDocumento.codTributo = null;
                    cuerpoDocumento.uniMedida = dr.GetInt32(4);
                    cuerpoDocumento.descripcion = dr.GetString(5);
                    cuerpoDocumento.precioUni = dr.GetDecimal(6);
                    cuerpoDocumento.montoDescu = dr.GetDecimal(7);
                    cuerpoDocumento.ventaNoSuj = dr.GetDecimal(8);
                    cuerpoDocumento.ventaExenta = dr.GetDecimal(9);
                    cuerpoDocumento.ivaItem = dr.GetDecimal(10);
                    cuerpoDocumento.ventaGravada = dr.GetDecimal(11);
                    cuerpoDocumento.tributos = null;
                    cuerpoDocumento.psv = 0;
                    cuerpoDocumento.noGravado = 0;
                    //cuerpoDocumento.numeroDocumento = dr.GetInt32(12);
                    CuerpoDoc.Add(cuerpoDocumento);
                }

                
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en información del cuerpo de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerCuerpoDoc: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return CuerpoDoc;
        }

        public ResumenFE obtenerResumenDocFE(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            ResumenFE resumen = new ResumenFE() ;
            TributoFE tributo;
            List<TributoFE> tributos = new List<TributoFE>();
            String result = ""; 
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT DISTINCT " +  
                            "    0.00 totalNoSuj, " +
                            "    CAST(ISNULL((SELECT SUM(K0.GTotal) FROM INV1 K0 WHERE K0.DOCENTRY=T0.Docentry AND K0.taxCode LIKE '%EXE%'),0.0) AS NUMERIC(19,2)) totalExenta, " +
                            "    CAST(ISNULL((SELECT SUM(K0.GTotal) FROM INV1 K0 WHERE K0.DOCENTRY=T0.Docentry AND K0.taxCode NOT LIKE '%EXE%'),0.0) AS NUMERIC(19,2)) totalGravada, " +
                            "    CAST(T0.docTotal+ISNULL((SELECT TOP 1 K0.WTAmnt FROM INV5 K0 WHERE K0.absEntry=T0.docEntry AND K0.WTCode LIKE 'R1%'),0) AS NUMERIC(19,2)) subTotalVentas,  " +
                            "    0.00 descuNoSuj, " +
                            "    0.00 descuExenta, " +
                            "    CAST(T0.DiscSum AS NUMERIC(19,2)) descuGravada, " +
                            "    CAST(T0.DiscPrcnt AS NUMERIC(19,2)) porcentajeDescuento, " +
                            "    CAST(T0.DiscSum AS NUMERIC(19,2)) totalDescu, " +
                            "    CASE WHEN T0.U_FacSerie='EXP' OR (SELECT COUNT(*) FROM INV1 K0 WHERE K0.DOCENTRY=T0.Docentry AND K0.taxCode LIKE '%IVA%')=0 THEN '' ELSE '20' END tributo, " +
                            "    CAST(T0.docTotal+ISNULL((SELECT TOP 1 K0.WTAmnt FROM INV5 K0 WHERE K0.absEntry=T0.docEntry AND K0.WTCode LIKE 'R1%'),0) AS NUMERIC(19,2)) subTotal," +
                            "    CAST(T0.wtSum AS NUMERIC(19,2)) ivaRete1, " +
                            "    0.00 reteRenta, " +
                            "    CAST(T0.docTotal+ISNULL((SELECT TOP 1 K0.WTAmnt FROM INV5 K0 WHERE K0.absEntry=T0.docEntry AND K0.WTCode LIKE 'R1%'),0) AS NUMERIC(19,2)) montoTotalOperacion, " +
                            "    0.00 totalNoGravado, " +
                            "    CAST(T0.docTotal AS NUMERIC(19,2)) totalPagar, " +
                            "    dbo.[FN_CANTIDADALETRAS](T0.docTotal) totalLetras, " +
                            "    CAST(T0.VatSum AS NUMERIC(19,2)) totalIva, " +
                            "    0.00 saldoFavor, " +
                            "    CASE WHEN (SELECT K0.PymntGroup FROM OCTG K0 WHERE K0.groupNum=T0.groupNum) LIKE '%CONTADO%' THEN 1 ELSE 2 END condicionOperacion " +
                            "FROM OINV T0 " +
                            "WHERE T0.docNum= @docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    resumen = new ResumenFE();
                    resumen.totalNoSuj = dr.GetDecimal(0);
                    resumen.totalExenta = dr.GetDecimal(1);
                    resumen.totalGravada = dr.GetDecimal(2);
                    resumen.subTotalVentas = dr.GetDecimal(3);
                    resumen.descuNoSuj = dr.GetDecimal(4);
                    resumen.descuExenta = dr.GetDecimal(5);
                    resumen.descuGravada = dr.GetDecimal(6);
                    resumen.porcentajeDescuento = dr.GetDecimal(7);
                    resumen.totalDescu = dr.GetDecimal(8);
                    tributo = null;
                    tributos.Add(tributo);
                    resumen.tributos = null;
                    resumen.subTotal = dr.GetDecimal(10);
                    resumen.ivaRete1 = dr.GetDecimal(11);
                    resumen.reteRenta = dr.GetDecimal(12);
                    resumen.montoTotalOperacion = dr.GetDecimal(13);
                    resumen.totalNoGravado = dr.GetDecimal(14);
                    resumen.totalPagar = dr.GetDecimal(15);
                    resumen.totalLetras = dr.GetString(16);
                    resumen.totalIva = dr.GetDecimal(17);
                    resumen.saldoFavor = dr.GetDecimal(18);
                    resumen.condicionOperacion = dr.GetInt32(19);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en información del resumen de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerResumenDoc: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return resumen;
        }

        public List<CuerpoDocumentoCCF> obtenerCuerpoDocCCF(int docNum, String codGeneracion)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            CuerpoDocumentoCCF cuerpoDocumento;
            List<CuerpoDocumentoCCF> CuerpoDoc = new List<CuerpoDocumentoCCF>();
            List<string> tributos = new List<string>();
            tributos.Add("20");
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT " +
                            "   CAST(ROW_NUMBER() OVER(ORDER BY T0.lineNum ASC) AS INT) numItem, " +
                            "   CASE WHEN T1.DocType = 'I' THEN 1 WHEN T1.DocType = 'S' THEN 2 END tipoItem, " +
                            "     CASE WHEN T0.Quantity=0 THEN 1 ELSE T0.Quantity END cantidad, " +
                            "     ISNULL(T0.ItemCode,'') codigo, " +
                            "   ISNULL((SELECT CAST(K0.U_FE_UMEDIDA AS INT) FROM OITM K0 WHERE K0.ItemCode=T0.ItemCode),59) unidadMedida, " +
                            "   T0.Dscription description, " +
                            "   CAST(T0.Price AS NUMERIC(19, 6)) precioUni, " +
                            "   0.00 montoDescu, " +
                            "   0.00 ventaNoSuj, " + 
                            "   CASE WHEN T0.taxCode LIKE '%EXE%' THEN CAST(T0.LineTotal AS NUMERIC(19, 6)) ELSE 0 END ventaExenta, " +
                            "   CASE WHEN T0.taxCode LIKE '%EXE%' THEN 0 ELSE CAST(T0.LineTotal AS NUMERIC(19, 6)) END ventaGravada,  " +
                            "   T1.docNum numDocumento  " +
                            "FROM INV1 T0 " +
                            "JOIN OINV T1 ON T0.docEntry = T1.docEntry " +
                            "WHERE docNum = @docNum " +
                            "ORDER BY T0.LineNum ASC ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    cuerpoDocumento = new CuerpoDocumentoCCF();

                    cuerpoDocumento.numItem = dr.GetInt32(0);
                    cuerpoDocumento.tipoItem = dr.GetInt32(1);
                    cuerpoDocumento.cantidad = dr.GetDecimal(2);
                    if (!dr.GetString(3).Equals(""))
                    {
                        cuerpoDocumento.codigo = dr.GetString(3);
                    }
                    cuerpoDocumento.codTributo = null;
                    cuerpoDocumento.uniMedida = dr.GetInt32(4);
                    cuerpoDocumento.descripcion = dr.GetString(5);
                    cuerpoDocumento.precioUni = dr.GetDecimal(6);
                    cuerpoDocumento.montoDescu = dr.GetDecimal(7);
                    cuerpoDocumento.ventaNoSuj = dr.GetDecimal(8);
                    cuerpoDocumento.ventaExenta = dr.GetDecimal(9);
                    cuerpoDocumento.ventaGravada = dr.GetDecimal(10);
                    cuerpoDocumento.tributos = tributos;
                    cuerpoDocumento.psv = 0;
                    cuerpoDocumento.noGravado = 0;

                    CuerpoDoc.Add(cuerpoDocumento);
                }


            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en información del cuerpo de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerCuerpoDoc: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return CuerpoDoc;
        }

        public ResumenCCF obtenerResumenDocCCF(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            ResumenCCF resumen = new ResumenCCF();
            TributoCCF tributo=new TributoCCF();
            List<TributoCCF> tributos = new List<TributoCCF>();
            String result = "";
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT " +
                            "    0.00 totalNoSuj, " +
                            "    CAST(ISNULL((SELECT SUM(K0.LineTotal) FROM INV1 K0 WHERE K0.DOCENTRY=T0.Docentry AND K0.taxCode LIKE '%EXE%'),0.0) AS NUMERIC(19,2)) totalExenta, " +
                            "    CAST(ISNULL((SELECT SUM(K0.LineTotal) FROM INV1 K0 WHERE K0.DOCENTRY=T0.Docentry AND K0.taxCode NOT LIKE '%EXE%'),0.0) AS NUMERIC(19,2)) totalGravada, " +
                            "    CAST((T0.docTotal-T0.VatSum)+T0.wtSum AS NUMERIC(19,2)) subTotalVentas, " +
                            "    0.00 descuNoSuj, " +
                            "    0.00 descuExenta, " +
                            "    CAST(T0.DiscSum AS NUMERIC(19,2)) descuGravada, " +
                            "    CAST(T0.DiscPrcnt AS NUMERIC(19,2)) porcentajeDescuento, " +
                            "    CAST(T0.DiscSum AS NUMERIC(19,2)) totalDescu, " +
                            "    CASE WHEN T0.U_FacSerie='EXP' AND (SELECT COUNT(*) FROM INV1 K0 WHERE K0.DOCENTRY=T0.Docentry AND K0.taxCode NOT LIKE '%EXE%')=0 THEN '' ELSE '20' END tributo, " +
                            "    CAST((T0.docTotal-T0.VatSum)+T0.wtSum AS NUMERIC(19,2)) subTotal," +
                            "    CAST(T0.wtSum AS NUMERIC(19,2)) ivaRete1, " +
                            "    0.00 reteRenta, " +
                            "    CAST(T0.docTotal+T0.wtSum AS NUMERIC(19,2)) montoTotalOperacion, " +
                            "    0.00 totalNoGravado, " +
                            "    CAST(T0.docTotal AS NUMERIC(19,2)) totalPagar, " +
                            "    dbo.[FN_CANTIDADALETRAS](T0.docTotal) totalLetras, " +
                            "    0.00 saldoFavor, " +
                            "    CASE WHEN (SELECT K0.PymntGroup FROM OCTG K0 WHERE K0.groupNum=T0.groupNum) LIKE '%CRED%' THEN 2 ELSE 1 END condicionOperacion, " +
                            "    CAST(ISNULL(T0.VatSum,0.00) AS NUMERIC(19,2)) iva " +
                            "FROM OINV T0 " +
                            "WHERE T0.docNum= @docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection); 
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    resumen = new ResumenCCF();
                    resumen.totalNoSuj = dr.GetDecimal(0);
                    resumen.totalExenta = dr.GetDecimal(1);
                    resumen.totalGravada = dr.GetDecimal(2);
                    resumen.subTotalVentas = dr.GetDecimal(3);
                    resumen.descuNoSuj = dr.GetDecimal(4);
                    resumen.descuExenta = dr.GetDecimal(5);
                    resumen.descuGravada = dr.GetDecimal(6);
                    resumen.porcentajeDescuento = dr.GetDecimal(7);
                    resumen.totalDescu = dr.GetDecimal(8); 
                    //if (dr.GetDecimal(19) > 0)
                    //{
                        tributo.codigo = "20";
                        tributo.descripcion = "Impuesto al Valor Agregado 13 %";
                        tributo.valor = dr.GetDecimal(19);
                        tributos.Add(tributo);
                        resumen.tributos = tributos;
                    //}
                    resumen.subTotal = dr.GetDecimal(10);
                    resumen.ivaPerci1 = 0;
                    resumen.ivaRete1 = dr.GetDecimal(11);
                    resumen.reteRenta = dr.GetDecimal(12);
                    resumen.montoTotalOperacion = dr.GetDecimal(13);
                    resumen.totalNoGravado = dr.GetDecimal(14);
                    resumen.totalPagar = dr.GetDecimal(15);
                    resumen.totalLetras = dr.GetString(16);
                    resumen.saldoFavor = dr.GetDecimal(17);
                    resumen.condicionOperacion = dr.GetInt32(18);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en información del resumen de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerResumenDoc: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return resumen;
        }

        public ResumenNC obtenerResumenDocNC(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            ResumenNC resumen = new ResumenNC();
            TributoNC tributo = new TributoNC();
            List<TributoNC> tributos = new List<TributoNC>();
            String result = "";
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT " +
                            "    0.00 totalNoSuj, " +
                            "    CAST(ISNULL((SELECT SUM(K0.LineTotal) FROM RIN1 K0 WHERE K0.DOCENTRY=T0.Docentry AND K0.taxCode LIKE '%EXE%'),0.0) AS NUMERIC(19,2)) totalExenta, " +
                            "    CAST(ISNULL((SELECT SUM(K0.LineTotal) FROM RIN1 K0 WHERE K0.DOCENTRY=T0.Docentry AND K0.taxCode NOT LIKE '%EXE%'),0.0) AS NUMERIC(19,2)) totalGravada, " +
                            "    CAST((T0.docTotal-T0.VatSum)+T0.wtSum AS NUMERIC(19,2)) subTotalVentas, " +
                            "    0.00 descuNoSuj, " +
                            "    0.00 descuExenta, " +
                            "    CAST(T0.DiscSum AS NUMERIC(19,2)) descuGravada, " +
                            "    CAST(T0.DiscSum AS NUMERIC(19,2)) totalDescu, " +
                            "    CASE WHEN T0.U_FacSerie='EXP' AND (SELECT COUNT(*) FROM RIN1 K0 WHERE K0.DOCENTRY=T0.Docentry AND K0.taxCode LIKE '%EXE%')=0 THEN '' ELSE '20' END tributo, " +
                            "    CAST((T0.docTotal-T0.VatSum)+T0.wtSum AS NUMERIC(19,2)) subTotal," +
                            "    CAST(T0.wtSum AS NUMERIC(19,2)) ivaRete1, " +
                            "    0.00 reteRenta, " +
                            "    CAST(T0.docTotal AS NUMERIC(19,2)) montoTotalOperacion, " +
                            "    dbo.[FN_CANTIDADALETRAS](T0.docTotal) totalLetras, " +
                            "    CASE WHEN (SELECT K0.PymntGroup FROM OCTG K0 WHERE K0.groupNum=T0.groupNum) LIKE '%CRED%' THEN 2 ELSE 1 END condicionOperacion, " +
                            "    CAST(ISNULL(T0.VatSum,0.00) AS NUMERIC(19,2)) iva " +
                            "FROM ORIN T0 " +
                            "WHERE T0.docNum= @docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    resumen = new ResumenNC();
                    resumen.totalNoSuj = dr.GetDecimal(0);
                    resumen.totalExenta = dr.GetDecimal(1);
                    resumen.totalGravada = dr.GetDecimal(2);
                    resumen.subTotalVentas = dr.GetDecimal(3);
                    resumen.descuNoSuj = dr.GetDecimal(4);
                    resumen.descuExenta = dr.GetDecimal(5);
                    resumen.descuGravada = dr.GetDecimal(6);
                    resumen.totalDescu = dr.GetDecimal(7);
                    tributo.codigo = "20";
                    tributo.descripcion = "Impuesto al Valor Agregado 13 %";
                    tributo.valor = dr.GetDecimal(15);
                    tributos.Add(tributo);
                    resumen.tributos = tributos;
                    resumen.subTotal = dr.GetDecimal(9);
                    resumen.ivaPerci1 = 0;
                    resumen.ivaRete1 = dr.GetDecimal(10);
                    resumen.reteRenta = dr.GetDecimal(11);
                    resumen.montoTotalOperacion = dr.GetDecimal(12);
                    resumen.totalLetras = dr.GetString(13);
                    resumen.condicionOperacion = dr.GetInt32(14);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en información del resumen de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerResumenDoc: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return resumen;
        }

        public List<CuerpoDocumentoNC> obtenerCuerpoDocNC(int docNum, String codGeneracion)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            CuerpoDocumentoNC cuerpoDocumento;
            List<CuerpoDocumentoNC> CuerpoDoc = new List<CuerpoDocumentoNC>();
            List<string> tributos = new List<string>();
            tributos.Add("20");
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT " +
                            "   CAST(ROW_NUMBER() OVER(ORDER BY T0.lineNum ASC) AS INT) numItem, " +
                            "   CASE WHEN T1.DocType = 'I' THEN 1 WHEN T1.DocType = 'S' THEN 2 END tipoItem, " + 
                            "     CASE WHEN T0.Quantity=0 THEN 1 ELSE T0.Quantity END cantidad, " +
                            "     ISNULL(T0.ItemCode,'') codigo, " +
                            "   ISNULL((SELECT CAST(K0.U_FE_UMEDIDA AS INT) FROM OITM K0 WHERE K0.ItemCode=T0.ItemCode),59) unidadMedida, " +
                            "   T0.Dscription description, " +
                            "   CAST(T0.Price AS NUMERIC(19, 6)) precioUni, " +
                            "   0.00 montoDescu, " +
                            "   0.00 ventaNoSuj, " +
                            "   CASE WHEN T0.taxCode LIKE '%EXE%' THEN CAST(T0.LineTotal AS NUMERIC(19, 2)) ELSE 0 END ventaExenta, " +
                            "   CASE WHEN T0.taxCode LIKE '%EXE%' THEN 0 ELSE CAST(T0.LineTotal AS NUMERIC(19, 2)) END ventaGravada,  " +
                            "   ISNULL((SELECT U_E_CODGENE FROM OINV K0 WHERE K0.DocNum = T0.BaseRef), ISNULL((SELECT K0.U_FacNum FROM OINV K0 WHERE K0.DocNum = T0.BaseRef),(SELECT U_E_CODGENE FROM OINV K0 WHERE K0.DocNum = T1.U_NumUnic))) numDocumento  " +
                            "FROM RIN1 T0 " +
                            "JOIN ORIN T1 ON T0.docEntry = T1.docEntry " +
                            "WHERE docNum = @docNum " +
                            "ORDER BY T0.LineNum ASC ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    cuerpoDocumento = new CuerpoDocumentoNC();

                    cuerpoDocumento.numItem = dr.GetInt32(0);
                    cuerpoDocumento.tipoItem = dr.GetInt32(1);
                    cuerpoDocumento.cantidad = dr.GetDecimal(2);
                    if (!dr.GetString(3).Equals(""))
                    {
                        cuerpoDocumento.codigo = dr.GetString(3);
                    }
                    cuerpoDocumento.codTributo = null;
                    cuerpoDocumento.uniMedida = dr.GetInt32(4);
                    cuerpoDocumento.descripcion = dr.GetString(5);
                    cuerpoDocumento.precioUni = dr.GetDecimal(6);
                    cuerpoDocumento.montoDescu = dr.GetDecimal(7);
                    cuerpoDocumento.ventaNoSuj = dr.GetDecimal(8);
                    cuerpoDocumento.ventaExenta = dr.GetDecimal(9);
                    cuerpoDocumento.ventaGravada = dr.GetDecimal(10);
                    if (!dr.GetString(11).Equals(""))
                    {
                        cuerpoDocumento.numeroDocumento = dr.GetString(11);
                    }

                    cuerpoDocumento.tributos = tributos;

                    CuerpoDoc.Add(cuerpoDocumento);
                }


            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en información del cuerpo de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerCuerpoDoc: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return CuerpoDoc;
        }

        public ResumenND obtenerResumenDocND(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            ResumenND resumen = new ResumenND();
            TributoND tributo = new TributoND();
            List<TributoND> tributos = new List<TributoND>();
            String result = "";
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT " +
                            "    0.00 totalNoSuj, " +
                            "    0.00 totalExenta, " +
                            "    CAST((T0.docTotal-T0.VatSum)+T0.wtSum AS NUMERIC(19,2)) totalGravada, " +
                            "    CAST((T0.docTotal-T0.VatSum)+T0.wtSum AS NUMERIC(19,2)) subTotalVentas, " +
                            "    0.00 descuNoSuj, " +
                            "    0.00 descuExenta, " +
                            "    CAST(T0.DiscSum AS NUMERIC(19,2)) descuGravada, " +
                            "    CAST(T0.DiscSum AS NUMERIC(19,2)) totalDescu, " +
                            "    CASE WHEN T0.U_FacSerie='EXP' THEN '' ELSE '20' END tributo, " +
                            "    CAST((T0.docTotal-T0.VatSum)+T0.wtSum AS NUMERIC(19,2)) subTotal," +
                            "    CAST(T0.wtSum AS NUMERIC(19,2)) ivaRete1, " +
                            "    0.00 reteRenta, " +
                            "    CAST(T0.docTotal+T0.wtSum  AS NUMERIC(19,2)) montoTotalOperacion, " +
                            "    dbo.[FN_CANTIDADALETRAS](T0.docTotal) totalLetras, " +
                            "    CASE WHEN (SELECT K0.PymntGroup FROM OCTG K0 WHERE K0.groupNum=T0.groupNum) LIKE '%CRED%' THEN 2 ELSE 1 END condicionOperacion, " +
                            "    CAST(ISNULL(T0.VatSum,0.00) AS NUMERIC(19,2)) iva " +
                            "FROM OINV T0 " +
                            "WHERE T0.docNum= @docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    resumen = new ResumenND();
                    resumen.totalNoSuj = dr.GetDecimal(0);
                    resumen.totalExenta = dr.GetDecimal(1);
                    resumen.totalGravada = dr.GetDecimal(2);
                    resumen.subTotalVentas = dr.GetDecimal(3);
                    resumen.descuNoSuj = dr.GetDecimal(4);
                    resumen.descuExenta = dr.GetDecimal(5);
                    resumen.descuGravada = dr.GetDecimal(6);
                    resumen.totalDescu = dr.GetDecimal(7);
                    tributo.codigo = "20";
                    tributo.descripcion = "Impuesto al Valor Agregado 13 %";
                    tributo.valor = dr.GetDecimal(15);
                    tributos.Add(tributo);
                    resumen.tributos = tributos;
                    resumen.subTotal = dr.GetDecimal(9);
                    resumen.ivaPerci1 = 0;
                    resumen.ivaRete1 = dr.GetDecimal(10);
                    resumen.reteRenta = dr.GetDecimal(11);
                    resumen.montoTotalOperacion = dr.GetDecimal(12);
                    resumen.totalLetras = dr.GetString(13);
                    resumen.condicionOperacion = dr.GetInt32(14);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en información del resumen de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerResumenDoc: " + e.Message);
                } 
            }
            finally
            {
                sqlConnection.Close();
            }

            return resumen;
        }

        public List<CuerpoDocumentoND> obtenerCuerpoDocND(int docNum, String codGeneracion)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            CuerpoDocumentoND cuerpoDocumento;
            List<CuerpoDocumentoND> CuerpoDoc = new List<CuerpoDocumentoND>();
            List<string> tributos = new List<string>();
            tributos.Add("20");
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT " +
                            "   CAST(ROW_NUMBER() OVER(ORDER BY T0.lineNum ASC) AS INT) numItem, " +
                            "   CASE WHEN T1.DocType = 'I' THEN 1 WHEN T1.DocType = 'S' THEN 2 END tipoItem, " +
                            "     CASE WHEN T0.Quantity=0 THEN 1 ELSE T0.Quantity END cantidad, " +
                            "     ISNULL(T0.ItemCode,'') codigo, " +
                            "   ISNULL((SELECT CAST(K0.U_FE_UMEDIDA AS INT) FROM OITM K0 WHERE K0.ItemCode=T0.ItemCode),59) unidadMedida, " +
                            "   T0.Dscription description, " +
                            "   CAST(T0.Price AS NUMERIC(19, 6)) precioUni, " +
                            "   0.00 montoDescu, " +
                            "   0.00 ventaNoSuj, " +
                            "   0.00 ventaExenta, " +
                            "   CAST(T0.lineTotal AS NUMERIC(19, 2)) ventaGravada,  " +
                            "   ISNULL((SELECT K0.U_E_CODGENE FROM OINV K0 WHERE K0.DocNum = T0.BaseRef),(SELECT K0.U_FacNum FROM OINV K0 WHERE K0.DocNum = T0.BaseRef)) numDocumento  " +
                            "FROM INV1 T0 " +
                            "JOIN OINV T1 ON T0.docEntry = T1.docEntry " +
                            "WHERE docNum = @docNum " +
                            "ORDER BY T0.LineNum ASC ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    cuerpoDocumento = new CuerpoDocumentoND();

                    cuerpoDocumento.numItem = dr.GetInt32(0);
                    cuerpoDocumento.tipoItem = dr.GetInt32(1);
                    cuerpoDocumento.cantidad = dr.GetDecimal(2);
                    if (!dr.GetString(3).Equals(""))
                    {
                        cuerpoDocumento.codigo = dr.GetString(3);
                    }
                    cuerpoDocumento.codTributo = null;
                    cuerpoDocumento.uniMedida = dr.GetInt32(4);
                    cuerpoDocumento.descripcion = dr.GetString(5);
                    cuerpoDocumento.precioUni = dr.GetDecimal(6);
                    cuerpoDocumento.montoDescu = dr.GetDecimal(7);
                    cuerpoDocumento.ventaNoSuj = dr.GetDecimal(8);
                    cuerpoDocumento.ventaExenta = dr.GetDecimal(9);
                    cuerpoDocumento.ventaGravada = dr.GetDecimal(10);
                    if (!dr.GetString(11).Equals(""))
                    {
                        cuerpoDocumento.numeroDocumento = dr.GetString(11);
                    }
                    
                    cuerpoDocumento.tributos = tributos;

                    CuerpoDoc.Add(cuerpoDocumento);
                }


            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en información del cuerpo de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerCuerpoDoc: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return CuerpoDoc;
        }

        public ResumenNR obtenerResumenDocNR(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            ResumenNR resumen = new ResumenNR();
            TributoNR tributo = new TributoNR();
            List<TributoNR> tributos = new List<TributoNR>();
            String result = "";
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT " +
                            "    0.00 totalNoSuj, " +
                            "    0.00 totalExenta, " +
                            "    CAST((T0.docTotal-T0.VatSum)+T0.wtSum AS NUMERIC(19,2)) totalGravada, " +
                            "    CAST((T0.docTotal-T0.VatSum)+T0.wtSum AS NUMERIC(19,2)) subTotalVentas, " +
                            "    0.00 descuNoSuj, " +
                            "    0.00 descuExenta, " +
                            "    CAST(T0.DiscSum AS NUMERIC(19,2)) descuGravada, " +
                            "    CAST(T0.DiscSum AS NUMERIC(19,2)) totalDescu, " +
                            "    CASE WHEN T0.U_FacSerie='EXP' THEN '' ELSE '20' END tributo, " +
                            "    CAST((T0.docTotal-T0.VatSum)+T0.wtSum AS NUMERIC(19,2)) subTotal," +
                            "    CAST(T0.wtSum AS NUMERIC(19,2)) ivaRete1, " +
                            "    0.00 reteRenta, " +
                            "    CAST((T0.docTotal+T0.wtSum)*1.13  AS NUMERIC(19,2)) montoTotalOperacion, " +
                            "    dbo.[FN_CANTIDADALETRAS](T0.docTotal) totalLetras, " +
                            "    CASE WHEN (SELECT K0.PymntGroup FROM OCTG K0 WHERE K0.groupNum=T0.groupNum) LIKE '%CRED%' THEN 2 ELSE 1 END condicionOperacion, " +
                            "    CAST(((T0.docTotal-T0.VatSum)+T0.wtSum)*0.13 AS NUMERIC(19,2)) iva " +
                            "FROM OWTR T0 " +
                            "WHERE T0.docNum= @docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    resumen = new ResumenNR();
                    resumen.totalNoSuj = dr.GetDecimal(0);
                    resumen.totalExenta = dr.GetDecimal(1);
                    resumen.totalGravada = dr.GetDecimal(2);
                    resumen.subTotalVentas = dr.GetDecimal(3);
                    resumen.descuNoSuj = dr.GetDecimal(4);
                    resumen.descuExenta = dr.GetDecimal(5);
                    resumen.descuGravada = dr.GetDecimal(6);
                    resumen.totalDescu = dr.GetDecimal(7);
                    tributo.codigo = "20";
                    tributo.descripcion = "Impuesto al Valor Agregado 13 %";
                    tributo.valor = dr.GetDecimal(15);
                    tributos.Add(tributo);
                    resumen.tributos = tributos;
                    resumen.subTotal = dr.GetDecimal(9);
                    resumen.montoTotalOperacion = dr.GetDecimal(12);
                    resumen.totalLetras = dr.GetString(13);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en información del cuerpo de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerResumenDoc: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return resumen;
        }

        public ResumenNR obtenerResumenDocNRT(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            ResumenNR resumen = new ResumenNR();
            TributoNR tributo = new TributoNR();
            List<TributoNR> tributos = new List<TributoNR>();
            String result = "";
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT " +
                            "    0.00 totalNoSuj, " +
                            "    0.00 totalExenta, " +
                            "    CAST((T0.docTotal-T0.VatSum)+T0.wtSum AS NUMERIC(19,2)) totalGravada, " +
                            "    CAST((T0.docTotal-T0.VatSum)+T0.wtSum AS NUMERIC(19,2)) subTotalVentas, " +
                            "    0.00 descuNoSuj, " +
                            "    0.00 descuExenta, " +
                            "    CAST(T0.DiscSum AS NUMERIC(19,2)) descuGravada, " +
                            "    CAST(T0.DiscSum AS NUMERIC(19,2)) totalDescu, " +
                            "    CASE WHEN T0.U_FacSerie='EXP' THEN '' ELSE '20' END tributo, " +
                            "    CAST((T0.docTotal-T0.VatSum)+T0.wtSum AS NUMERIC(19,2)) subTotal," +
                            "    CAST(T0.wtSum AS NUMERIC(19,2)) ivaRete1, " +
                            "    0.00 reteRenta, " +
                            "    CAST(T0.docTotal+T0.wtSum  AS NUMERIC(19,2)) montoTotalOperacion, " +
                            "    dbo.[FN_CANTIDADALETRAS](T0.docTotal) totalLetras, " +
                            "    CASE WHEN (SELECT K0.PymntGroup FROM OCTG K0 WHERE K0.groupNum=T0.groupNum) LIKE '%CRED%' THEN 2 ELSE 1 END condicionOperacion, " +
                            "    CAST(ISNULL(T0.VatSum,0.00) AS NUMERIC(19,2)) iva " +
                            "FROM OWTR T0 " +
                            "WHERE T0.docNum= @docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    resumen = new ResumenNR();
                    resumen.totalNoSuj = dr.GetDecimal(0);
                    resumen.totalExenta = dr.GetDecimal(1);
                    resumen.totalGravada = dr.GetDecimal(2);
                    resumen.subTotalVentas = dr.GetDecimal(3);
                    resumen.descuNoSuj = dr.GetDecimal(4);
                    resumen.descuExenta = dr.GetDecimal(5);
                    resumen.descuGravada = dr.GetDecimal(6);
                    resumen.totalDescu = dr.GetDecimal(7);
                    tributo.codigo = "20";
                    tributo.descripcion = "Impuesto al Valor Agregado 13 %";
                    tributo.valor = dr.GetDecimal(15);
                    tributos.Add(tributo);
                    resumen.tributos = tributos;
                    resumen.subTotal = dr.GetDecimal(9);
                    resumen.montoTotalOperacion = dr.GetDecimal(12);
                    resumen.totalLetras = dr.GetString(13);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en información del resumen de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerResumenDoc: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return resumen;
        }

        public List<CuerpoDocumentoNR> obtenerCuerpoDocNR(int docNum, String codGeneracion)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            CuerpoDocumentoNR cuerpoDocumento;
            List<CuerpoDocumentoNR> CuerpoDoc = new List<CuerpoDocumentoNR>();
            List<string> tributos = new List<string>();
            tributos.Add("20");
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT " +
                            "   CAST(ROW_NUMBER() OVER(ORDER BY T0.lineNum ASC) AS INT) numItem, " +
                            "   CASE WHEN T1.DocType = 'I' THEN 1 WHEN T1.DocType = 'S' THEN 2 END tipoItem, " +
                            "     CASE WHEN T0.Quantity=0 THEN 1 ELSE T0.Quantity END cantidad, " +
                            "     ISNULL(T0.ItemCode,'') codigo, " +
                            "   ISNULL((SELECT CAST(K0.U_FE_UMEDIDA AS INT) FROM OITM K0 WHERE K0.ItemCode=T0.ItemCode),59) unidadMedida, " +
                            "   T0.Dscription description, " +
                            "   CAST(T0.Price AS NUMERIC(19, 6)) precioUni, " +
                            "   0.00 montoDescu, " +
                            "   0.00 ventaNoSuj, " +
                            "   0.00 ventaExenta, " +
                            "   CAST(T0.lineTotal AS NUMERIC(19, 2)) ventaGravada,  " +
                            "   ISNULL((SELECT U_E_CODGENE FROM OINV K0 WHERE K0.DocNum = T0.BaseRef),'') numDocumento  " +
                            "FROM WTR1 T0 " +
                            "JOIN OWTR T1 ON T0.docEntry = T1.docEntry " +
                            "WHERE docNum = @docNum " +
                            "ORDER BY T0.LineNum ASC ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    cuerpoDocumento = new CuerpoDocumentoNR();

                    cuerpoDocumento.numItem = dr.GetInt32(0);
                    cuerpoDocumento.tipoItem = dr.GetInt32(1);
                    cuerpoDocumento.cantidad = dr.GetDecimal(2);
                    if (!dr.GetString(3).Equals(""))
                    {
                        cuerpoDocumento.codigo = dr.GetString(3);
                    }
                    cuerpoDocumento.codTributo = null;
                    cuerpoDocumento.uniMedida = dr.GetInt32(4);
                    cuerpoDocumento.descripcion = dr.GetString(5);
                    cuerpoDocumento.precioUni = dr.GetDecimal(6);
                    cuerpoDocumento.montoDescu = dr.GetDecimal(7);
                    cuerpoDocumento.ventaNoSuj = dr.GetDecimal(8);
                    cuerpoDocumento.ventaExenta = dr.GetDecimal(9);
                    cuerpoDocumento.ventaGravada = dr.GetDecimal(10);
                    if (!dr.GetString(11).Equals(""))
                    {
                        cuerpoDocumento.numeroDocumento = dr.GetString(11);
                    }

                    cuerpoDocumento.tributos = tributos;

                    CuerpoDoc.Add(cuerpoDocumento);
                }


            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en información del cuerpo de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerCuerpoDoc: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return CuerpoDoc;
        }

        public List<CuerpoDocumentoNR> obtenerCuerpoDocNRT(int docNum, String codGeneracion)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            CuerpoDocumentoNR cuerpoDocumento;
            List<CuerpoDocumentoNR> CuerpoDoc = new List<CuerpoDocumentoNR>();
            List<string> tributos = new List<string>();
            tributos.Add("20");
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT " +
                            "   CAST(ROW_NUMBER() OVER(ORDER BY T0.lineNum ASC) AS INT) numItem, " +
                            "   CASE WHEN T1.DocType = 'I' THEN 1 WHEN T1.DocType = 'S' THEN 2 END tipoItem, " +
                            "     CASE WHEN T0.Quantity=0 THEN 1 ELSE T0.Quantity END cantidad, " +
                            "     ISNULL(T0.ItemCode,'') codigo, " +
                            "   ISNULL((SELECT CAST(K0.U_FE_UMEDIDA AS INT) FROM OITM K0 WHERE K0.ItemCode=T0.ItemCode),59) unidadMedida, " +
                            "   T0.Dscription description, " +
                            "   CAST(T0.Price AS NUMERIC(19, 6)) precioUni, " +
                            "   0.00 montoDescu, " +
                            "   0.00 ventaNoSuj, " +
                            "   0.00 ventaExenta, " +
                            "   CAST(T0.lineTotal AS NUMERIC(19, 2)) ventaGravada,  " +
                            "   ISNULL((SELECT U_E_CODGENE FROM OINV K0 WHERE K0.DocNum = T0.BaseRef),'') numDocumento  " +
                            "FROM WTR1 T0 " +
                            "JOIN OWTR T1 ON T0.docEntry = T1.docEntry " +
                            "WHERE docNum = @docNum " +
                            "ORDER BY T0.LineNum ASC ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    cuerpoDocumento = new CuerpoDocumentoNR();

                    cuerpoDocumento.numItem = dr.GetInt32(0);
                    cuerpoDocumento.tipoItem = dr.GetInt32(1);
                    cuerpoDocumento.cantidad = dr.GetDecimal(2);
                    if (!dr.GetString(3).Equals(""))
                    {
                        cuerpoDocumento.codigo = dr.GetString(3);
                    }
                    cuerpoDocumento.codTributo = null;
                    cuerpoDocumento.uniMedida = dr.GetInt32(4);
                    cuerpoDocumento.descripcion = dr.GetString(5);
                    cuerpoDocumento.precioUni = dr.GetDecimal(6);
                    cuerpoDocumento.montoDescu = dr.GetDecimal(7);
                    cuerpoDocumento.ventaNoSuj = dr.GetDecimal(8);
                    cuerpoDocumento.ventaExenta = dr.GetDecimal(9);
                    cuerpoDocumento.ventaGravada = dr.GetDecimal(10);
                    if (!dr.GetString(11).Equals(""))
                    {
                        cuerpoDocumento.numeroDocumento = dr.GetString(11);
                    }

                    cuerpoDocumento.tributos = tributos;

                    CuerpoDoc.Add(cuerpoDocumento);
                }


            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en información del cuerpo de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerCuerpoDoc: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return CuerpoDoc;
        }

        public ResumenFSE obtenerResumenDocFSE(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            ResumenFSE resumen = new ResumenFSE();
            String result = "";
            try
            {
                sqlConnection.Open();
                String SQL = " SELECT " +
                            "     CAST((T0.docTotal-T0.VatSum)+T0.wtSum AS NUMERIC(19,2)) totalCompra, " +
                            "     CAST(T0.DiscSum AS NUMERIC(19,2)) descu, " +
                            "     CAST(T0.DiscSum AS NUMERIC(19,2)) totalDescu,	" +
                            "     CAST((T0.docTotal-T0.VatSum)+T0.wtSum AS NUMERIC(19,2)) subTotal," +
                            "     0.00 ivaRete1, " +
                            "     CAST(T0.wtSum AS NUMERIC(19,2)) reteRenta, " +
                            "     CAST(T0.docTotal AS NUMERIC(19,2)) totalPagar, " +
                            "     dbo.[FN_CANTIDADALETRAS](T0.docTotal) totalLetras, " +
                            "     CASE WHEN (SELECT K0.PymntGroup FROM OCTG K0 WHERE K0.groupNum=T0.groupNum) LIKE '%CRED%' THEN 2 ELSE 1 END condicionOperacion " +
                            " FROM OPCH T0 " +
                            " WHERE T0.docNum= @docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    resumen = new ResumenFSE();
                    resumen.totalCompra = dr.GetDecimal(0);
                    resumen.descu = dr.GetDecimal(1);
                    resumen.totalDescu = dr.GetDecimal(2);
                    resumen.subTotal = dr.GetDecimal(3);
                    resumen.ivaRete1 = dr.GetDecimal(4);
                    resumen.reteRenta = dr.GetDecimal(5);
                    resumen.totalPagar = dr.GetDecimal(6);
                    resumen.totalLetras = dr.GetString(7);
                    resumen.condicionOperacion = dr.GetInt32(8);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en información del resumen de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerResumenDoc: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return resumen;
        }

        public List<CuerpoDocumentoFSE> obtenerCuerpoDocFSE(int docNum, String codGeneracion)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            CuerpoDocumentoFSE cuerpoDocumento;
            List<CuerpoDocumentoFSE> CuerpoDoc = new List<CuerpoDocumentoFSE>();
            try
            {
                sqlConnection.Open();
                String SQL = " SELECT " +
                            "     CAST(ROW_NUMBER() OVER(ORDER BY T0.lineNum ASC) AS INT) numItem, " +
                            "     CASE WHEN T1.DocType = 'I' THEN 1 WHEN T1.DocType = 'S' THEN 2 END tipoItem, " +
                            "     CASE WHEN T0.Quantity=0 THEN 1 ELSE T0.Quantity END cantidad, " +
                            "     ISNULL(T0.ItemCode,'') codigo, " +
                            "     ISNULL((SELECT CAST(K0.U_FE_UMEDIDA AS INT) FROM OITM K0 WHERE K0.ItemCode=T0.ItemCode),59) unidadMedida, " +
                            "     T0.Dscription description, " +
                            "     CAST(T0.Price AS NUMERIC(19, 6)) precioUni, " +
                            "     0.00 montoDescu, " +
                            "     CAST(T0.lineTotal AS NUMERIC(19, 6)) compra  " +
                            " FROM PCH1 T0 " +
                            " JOIN OPCH T1 ON T0.docEntry = T1.docEntry " +
                            "WHERE docNum = @docNum " +
                            "ORDER BY T0.LineNum ASC ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    cuerpoDocumento = new CuerpoDocumentoFSE();

                    cuerpoDocumento.numItem = dr.GetInt32(0);
                    cuerpoDocumento.tipoItem = dr.GetInt32(1);
                    cuerpoDocumento.cantidad = dr.GetDecimal(2);
                    if (!dr.GetString(3).Equals("")) {
                        cuerpoDocumento.codigo = dr.GetString(3);
                    }
                    cuerpoDocumento.uniMedida = dr.GetInt32(4);
                    cuerpoDocumento.descripcion = dr.GetString(5);
                    cuerpoDocumento.precioUni = dr.GetDecimal(6);
                    cuerpoDocumento.montoDescu = dr.GetDecimal(7);
                    cuerpoDocumento.compra= dr.GetDecimal(8);

                    CuerpoDoc.Add(cuerpoDocumento);
                }


            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en información del cuerpo de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerCuerpoDoc: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return CuerpoDoc;
        }

        public List<CuerpoDocumentoFEX> obtenerCuerpoDocFEX(int docNum, String codGeneracion)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            CuerpoDocumentoFEX cuerpoDocumento;
            List<CuerpoDocumentoFEX> CuerpoDoc = new List<CuerpoDocumentoFEX>();
            List<string> tributos = new List<string>();
            //tributos.Add("20");
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT " +
                            "   CAST(ROW_NUMBER() OVER(ORDER BY T0.lineNum ASC) AS INT) numItem, " +
                            "   T0.ItemCode codigo, " +
                            "   T0.Dscription description, " +
                            "   T0.Quantity cantidad, " +
                            "   ISNULL((SELECT CAST(K0.U_FE_UMEDIDA AS INT) FROM OITM K0 WHERE K0.ItemCode=T0.ItemCode),59) unidadMedida, " +
                            "   CAST(T0.Price AS NUMERIC(19, 6)) precioUni, " +
                            "   0.00 montoDescu, " +
                            "   CAST(T0.lineTotal AS NUMERIC(19, 6)) ventaGravada,  " +
                            "   T1.docNum numDocumento  " +
                            "FROM INV1 T0 " +
                            "JOIN OINV T1 ON T0.docEntry = T1.docEntry " +
                            "WHERE docNum = @docNum " +
                            "ORDER BY T0.LineNum ASC ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    cuerpoDocumento = new CuerpoDocumentoFEX();

                    cuerpoDocumento.numItem = dr.GetInt32(0);
                    cuerpoDocumento.codigo = dr.GetString(1);
                    cuerpoDocumento.descripcion = dr.GetString(2);
                    cuerpoDocumento.cantidad = dr.GetDecimal(3);
                    cuerpoDocumento.uniMedida = dr.GetInt32(4);
                    cuerpoDocumento.precioUni = dr.GetDecimal(5);
                    cuerpoDocumento.montoDescu = dr.GetDecimal(6);
                    cuerpoDocumento.ventaGravada = dr.GetDecimal(7);
                    cuerpoDocumento.tributos = null;
                    cuerpoDocumento.noGravado = 0;

                    CuerpoDoc.Add(cuerpoDocumento);
                }


            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en información del cuerpo de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerCuerpoDoc: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return CuerpoDoc;
        }

        public ResumenFEX obtenerResumenDocFEX(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            ResumenFEX resumen = new ResumenFEX();
            String result = "";
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT " +
                            "    CAST(T0.docTotal+ISNULL(T0.DiscSum,0)-T0.VatSum-T0.U_flete-T0.U_Seguro AS NUMERIC(19,2)) totalGravada,  " +
                            "    CAST(ISNULL(T0.DiscSum,0) AS NUMERIC(19,2)) descuento, " +
                            "    CAST(ISNULL(T0.DiscPrcnt,0) AS NUMERIC(19,2)) porcentajeDescuento, " +
                            "    CAST(ISNULL(T0.DiscSum,0) AS NUMERIC(19,2)) totalDescu, " +
                            "    CAST(T0.docTotal AS NUMERIC(19,2)) montoTotalOperacion, " +
                            "    0.00 totalNoGravado, " +
                            "    CAST(T0.docTotal AS NUMERIC(19,2)) totalPagar, " +
                            "    dbo.[FN_CANTIDADALETRAS](T0.docTotal) totalLetras, " +
                            "    CASE WHEN (SELECT K0.PymntGroup FROM OCTG K0 WHERE K0.groupNum=T0.groupNum) LIKE '%CRED%' THEN 2 ELSE 1 END condicionOperacion, " +
                            "    T0.U_flete, " +
                            "    T0.U_Seguro, " +
                            "    ISNULL(RIGHT(T0.U_E_INCOTERMS,2),'') codINCOTERMS, " +
                            "    ISNULL((SELECT K0.Name FROM [@FE_INCOTERMS] K0 WHERE K0.Code=T0.U_E_INCOTERMS),'') descINCOTERMS " +
                            "FROM OINV T0 " +
                            "WHERE T0.docNum= @docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    resumen = new ResumenFEX();
                    resumen.totalGravada = dr.GetDecimal(0);
                    resumen.descuento = dr.GetDecimal(1);
                    resumen.porcentajeDescuento = dr.GetDecimal(2);
                    resumen.totalDescu = dr.GetDecimal(3);
                    resumen.montoTotalOperacion = dr.GetDecimal(4);
                    resumen.totalNoGravado = dr.GetDecimal(5);
                    resumen.totalPagar = dr.GetDecimal(6);
                    resumen.totalLetras = dr.GetString(7);
                    resumen.condicionOperacion = dr.GetInt32(8);
                    resumen.flete= dr.GetDecimal(9);
                    resumen.seguro = dr.GetDecimal(10);
                    if(dr.GetString(11)!="")
                        resumen.codIncoterms = dr.GetString(11);

                    if (dr.GetString(12) != "")
                        resumen.descIncoterms = dr.GetString(12);
                }
                    
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error en información del resumen de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerResumenDoc: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return resumen;
        }

        public int obtenerDocNumFAC()
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT TOP 1 CASE WHEN T0.CANCELED!='N' THEN -1 ELSE T0.DocNum END DocNum FROM OINV T0 ORDER BY T0.DocEntry DESC";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error al obtener informacion de numero de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerDocNum: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public int obtenerDocNumND()
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT TOP 1 CASE WHEN T0.CANCELED!='N' THEN -1 ELSE T0.DocNum END DocNum FROM OINV T0 WHERE T0.DocSubType='DN' ORDER BY T0.DocEntry DESC";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error al obtener informacion de numero de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerDocNum: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public int obtenerDocNumNC()
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT TOP 1 CASE WHEN T0.CANCELED!='N' THEN -1 ELSE T0.DocNum END DocNum FROM ORIN T0 ORDER BY T0.DocEntry DESC";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error al obtener informacion de numero de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerDocNum: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public int obtenerDocNumNR()
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT TOP 1 CASE WHEN T0.CANCELED!='N' THEN -1 ELSE T0.DocNum END DocNum FROM OWTR T0 ORDER BY T0.DocEntry DESC";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error al obtener informacion de numero de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerDocNum: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public int obtenerDocNumNRT()
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT TOP 1 CASE WHEN T0.CANCELED!='N' THEN -1 ELSE T0.DocNum END DocNum FROM OWTR T0 ORDER BY T0.DocEntry DESC";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error al obtener informacion de numero de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerDocNum: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public int obtenerDocNumFSE()
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT TOP 1 CASE WHEN T0.CANCELED!='N' OR T0.U_FacSerie NOT IN ('FSE','fse') THEN -1 ELSE T0.DocNum END DocNum FROM OPCH T0 ORDER BY T0.DocEntry DESC";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error al obtener informacion de numero de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerDocNum: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public int obtenerDocEntryFAC(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT T0.docEntry FROM OINV T0 WHERE T0.docNum=@docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en obtenerDocEntry: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public int obtenerDocEntryND(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT T0.docEntry FROM OINV T0 WHERE T0.docNum=@docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en obtenerDocEntry: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public int obtenerDocEntryNC(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT T0.docEntry FROM ORIN T0 WHERE T0.docNum=@docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en obtenerDocEntry: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public int obtenerDocEntryNR(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT T0.docEntry FROM OWTR T0 WHERE T0.docNum=@docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en obtenerDocEntry: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public int obtenerDocEntryNRT(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT T0.docEntry FROM OWTR T0 WHERE T0.docNum=@docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en obtenerDocEntry: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public int obtenerDocEntryFSE(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT T0.docEntry FROM OPCH T0 WHERE T0.docNum=@docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en obtenerDocEntry: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public int contarEmisor()
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT COUNT(*) FROM [@FE_EMISOR] T0";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en ContarEmisor: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public List<ActEconomica> obtenerActEcon()
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            ActEconomica actEconomica;
            List<ActEconomica> actEconomicas = new List<ActEconomica>();
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT T0.U_COD_AEC,T0.U_DESC_ACTEC FROM [@FE_ACT_ECON] T0";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    actEconomica = new ActEconomica();
                    actEconomica.codActividad = dr.GetString(0);
                    actEconomica.descActividad = dr.GetString(1);
                    actEconomicas.Add(actEconomica);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error al obtener informacion de actividades economicas. Por favor Revisar la tabla de actividades economicas [FE_ACT_ECON]. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerActEcon: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return actEconomicas;
        }

        public List<Departamento> obtenerDepto()
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            Departamento departamento;
            List<Departamento> departamentos = new List<Departamento>();
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT T0.* FROM [@FE_DEPARTAMENTO] T0";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    departamento = new Departamento();
                    departamento.codDepto = dr.GetString(0);
                    departamento.Depto = dr.GetString(1);
                    departamentos.Add(departamento);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error al obtener informacion de de departamentos. Por favor Revisar la tabla de departamentos [FE_DEPARTAMENTO]. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerDepto: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return departamentos;
        }

        public List<Municipio> obtenerMunicipio()
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            Municipio municipio;
            List<Municipio> municipios = new List<Municipio>();
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT T0.Code,T0.U_DESC_MUNI FROM [@FE_MUNICIPIO] T0";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    municipio = new Municipio();
                    municipio.codMunicipio = dr.GetString(0);
                    municipio.municipio = dr.GetString(1);
                    municipios.Add(municipio);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error al obtener informacion de de Municipios. Por favor Revisar la tabla de municipios [FE_MUNICIPIO]. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerMunicipio: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return municipios;
        }

        public List<ObjFE> obtenerRegimen()
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            ObjFE objFE;
            List<ObjFE> regimen = new List<ObjFE>();
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT T0.U_COD_REGIMEN,LEFT(T0.U_DESC_REGIMEN,50) FROM [@FE_REGIMEN] T0";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    objFE = new ObjFE();
                    objFE.cod = dr.GetString(0);
                    objFE.descrip = dr.GetString(1);
                    regimen.Add(objFE);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {
                    MessageBox.Show("Error al obtener informacion de de regimen. Por favor Revisar la tabla de regimen [FE_REGIMEN]. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerRegimen: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return regimen;
        }

        public List<ObjFE> obtenerRecintoFis()
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            ObjFE objFE;
            List<ObjFE> recintoFiscal = new List<ObjFE>();
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT T0.U_COD_RECINTO,LEFT(T0.U_DESC_RECINTO,50) FROM [@FE_RE_FISCAL] T0";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    objFE = new ObjFE();
                    objFE.cod = dr.GetString(0);
                    objFE.descrip = dr.GetString(1);
                    recintoFiscal.Add(objFE);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error al obtener informacion de de recintos fiscales. Por favor Revisar la tabla de recintos fiscales [FE_RE_FISCAL]. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerRegimenFis: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return recintoFiscal;
        }

        public List<ObjFE> obtenerTipoAnulacion()
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            ObjFE tipoAnulacion;
            List<ObjFE> tiposAnulacion = new List<ObjFE>();
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT T0.Code,T0.Name FROM [@FE_TIPOANULACION] T0 ORDER BY T0.Code ASC";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    tipoAnulacion = new ObjFE();
                    tipoAnulacion.cod = dr.GetString(0);
                    tipoAnulacion.descrip = dr.GetString(1);
                    tiposAnulacion.Add(tipoAnulacion);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error al obtener informacion de tipos de anulacion. Por favor Revisar la tabla de tipos de anulacion [FE_TIPOANULACION]. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerTipoAnulacion: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return tiposAnulacion;
        }

        public List<ObjFE> obtenerTipoDoc()
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            ObjFE tipodoc;
            List<ObjFE> tipodocs = new List<ObjFE>();
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT T0.Code,T0.Name FROM [@FE_TIPODOC] T0 ORDER BY T0.Code ASC";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    tipodoc = new ObjFE();
                    tipodoc.cod = dr.GetString(0);
                    tipodoc.descrip = dr.GetString(1);
                    tipodocs.Add(tipodoc);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {
                    MessageBox.Show("Error al obtener informacion de tipos de documentos. Por favor Revisar la tabla de tipos de documentos [FE_TIPODOC]. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerTipoDoc: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return tipodocs;
        }

        public EmisorDTE obtenerEmisor()
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            EmisorDTE emisorDTE= new EmisorDTE();
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT " +
                             "    T0.U_Nombre, " +
                             "    T0.U_NombreComercial, " +
                             "    T0.U_nit, " +
                             "    T0.U_nrc, " +
                             "    T0.U_ActEcon, " +
                             "    T0.U_tipoEstablecimiento, " +
                             "    T0.U_departamento, " +
                             "    T0.U_municipio, " +
                             "    T0.U_direccion, " +
                             "    T0.U_telefono, " +
                             "    T0.U_correo, " +
                             "    T0.U_userDTE, " +
                             "    T0.U_passDTE, " +
                             "    T0.U_passPrivDTE, " +
                             "    T0.U_AMBIENTE, " +
                             "    T0.U_correoEnvio, " +
                             "    T0.U_contraCorreo, " +
                             "    T0.U_smtp, " +
                             "    T0.U_puertoCorreo " +
                             "FROM [@FE_EMISOR] T0 ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    emisorDTE.nombre = dr.GetString(0);
                    emisorDTE.nombreComercial = dr.GetString(1);
                    emisorDTE.nit = dr.GetString(2);
                    emisorDTE.nrc = dr.GetString(3);
                    emisorDTE.codActividad = dr.GetString(4);
                    emisorDTE.tipoEstablecimiento = dr.GetString(5);
                    emisorDTE.departamento = dr.GetString(6);
                    emisorDTE.municipio = dr.GetString(7);
                    emisorDTE.direccion = dr.GetString(8);
                    emisorDTE.telefono = dr.GetString(9);
                    emisorDTE.correo = dr.GetString(10);
                    emisorDTE.userDTE = dr.GetString(11);
                    emisorDTE.passDTE = dr.GetString(12);
                    emisorDTE.passPrivDTE = dr.GetString(13);
                    emisorDTE.ambiente = dr.GetString(14);
                    emisorDTE.correoEnvio = dr.GetString(15);
                    emisorDTE.contraCorreo = dr.GetString(16);
                    emisorDTE.smtp = dr.GetString(17);
                    emisorDTE.puertoCorreo = dr.GetString(18);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {
                    MessageBox.Show("Error al obtener informacion del emisor. Por favor Revisar la tabla del emisor [FE_EMISOR]. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerEmisor: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return emisorDTE;
        }

        public List<DocumentoRelacionadoNC> obtenerDocumentoRelacionadoNC(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            DocumentoRelacionadoNC documentoRelacionado;
            List<DocumentoRelacionadoNC> documentoRelacionados = new List<DocumentoRelacionadoNC>();
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT  TOP 1" +
                             "   CASE WHEN T0.U_FacSerie IS NULL THEN '' ELSE (SELECT K0.U_ID_HACIENDA FROM [@TIPOSDOCUMENTO] K0 WHERE K0.code = ISNULL((SELECT K0.U_FacSerie FROM OINV K0 WHERE K0.DocNum = T1.BaseRef AND DocSubType!='DN'),(SELECT K0.U_FacSerie FROM OINV K0 WHERE K0.DocNum = T0.U_NumUnic AND DocSubType!='DN'))) END tipoDoc, " +
                             "   ISNULL((SELECT U_E_CODGENE FROM OINV K0 WHERE K0.DocNum = T1.BaseRef), ISNULL((SELECT K0.U_FacNum FROM OINV K0 WHERE K0.DocNum = T1.BaseRef),(SELECT U_E_CODGENE FROM OINV K0 WHERE K0.DocNum = T0.U_NumUnic))) docNum, " +
                             "   ISNULL(CAST((SELECT CAST(CAST(K0.U_E_FECHORA AS VARCHAR) AS DATE) FROM OINV K0 WHERE K0.DocNum = T1.BaseRef) AS VARCHAR),CAST(ISNULL((SELECT CAST(CAST(K0.docDate AS VARCHAR) AS DATE) FROM OINV K0 WHERE K0.DocNum = T1.BaseRef),(SELECT CAST(CAST(K0.docDate AS VARCHAR) AS DATE)  FROM OINV K0 WHERE K0.DocNum = T0.U_NumUnic)) AS VARCHAR)) fechaEmision, " +
                             "   CASE WHEN ISNULL((SELECT U_E_CODGENE FROM OINV K0 WHERE K0.DocNum = T1.BaseRef),(SELECT U_E_CODGENE FROM OINV K0 WHERE K0.DocNum = T0.U_NumUnic)) IS NULL THEN 1 ELSE 2 END tipoGeneracion " +
                             "FROM ORIN T0 " + 
                             "JOIN RIN1 T1 ON T0.DocEntry = T1.DocEntry " +
                             "WHERE T0.docnum = @docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    documentoRelacionado = new DocumentoRelacionadoNC();
                    if (!dr.GetString(1).Equals("")) {
                        documentoRelacionado.tipoDocumento = dr.GetString(0);
                        documentoRelacionado.tipoGeneracion = dr.GetInt32(3);
                        documentoRelacionado.numeroDocumento = dr.GetString(1);
                        documentoRelacionado.fechaEmision = dr.GetString(2);
                    }

                    documentoRelacionados.Add(documentoRelacionado);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error al obtener informacion del documento relacionado. Por favor Revisar informacion del documento al que se esta haciendo referencia. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerDocumentoRelacionado: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return documentoRelacionados;
        }

        public List<DocumentoRelacionadoND> obtenerDocumentoRelacionadoND(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            DocumentoRelacionadoND documentoRelacionado;
            List<DocumentoRelacionadoND> documentoRelacionados = new List<DocumentoRelacionadoND>();
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT  TOP 1" +
                             "   CASE WHEN T0.U_FacSerie IS NULL THEN '' ELSE(SELECT K0.U_ID_HACIENDA FROM[@TIPOSDOCUMENTO] K0 WHERE K0.code = T0.U_FacSerie) END tipoDoc, " +
                             "   ISNULL((SELECT U_E_CODGENE FROM OINV K0 WHERE K0.DocNum = T0.U_NumUnic),T0.U_NumUnic) docNum, " +
                             "   ISNULL(CAST((SELECT CAST(CAST(K0.U_E_FECHORA AS VARCHAR) AS DATE) FROM OINV K0 WHERE K0.DocNum = T0.U_NumUnic) AS VARCHAR),'') fechaEmision, " +
                             "   CASE WHEN (SELECT U_E_CODGENE FROM OINV K0 WHERE K0.DocNum = T0.U_NumUnic) IS NULL THEN 1 ELSE 2 END tipoGeneracion " +
                             "FROM OINV T0 " +
                             "WHERE T0.docnum = @docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    documentoRelacionado = new DocumentoRelacionadoND();
                    if (!dr.GetString(1).Equals(""))
                    {
                        documentoRelacionado.tipoDocumento = dr.GetString(0);
                        documentoRelacionado.tipoGeneracion = dr.GetInt32(3);
                        documentoRelacionado.numeroDocumento = dr.GetString(1);
                        documentoRelacionado.fechaEmision = dr.GetString(2);
                    }

                    documentoRelacionados.Add(documentoRelacionado);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error al obtener informacion del documento relacionado. Por favor Revisar informacion del documento al que se esta haciendo referencia. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerDocumentoRelacionado: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return documentoRelacionados;
        }

        public String nombreNR_PDF(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            String result = "";
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT   " +
                            "    CONCAT(CAST(T0.U_E_CODGENE AS text),CAST(' - ' AS text),CAST(T0.U_E_NUMCONT AS text))  " +
                            "FROM OWTR T0 " +
                            "JOIN OCRD T1 ON T0.CardCode = T1.CardCode " +
                            "WHERE T0.docNum=@docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetString(0);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en nombreDocumentoPDF: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public String nombreFAC_PDF(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            String result = "";
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT   " +
                            "    CONCAT(CAST(T0.U_E_CODGENE AS text),CAST(' - ' AS text),CAST(T0.U_E_NUMCONT AS text))  " + 
                            "FROM OINV T0 " + 
                            "JOIN OCRD T1 ON T0.CardCode = T1.CardCode " +
                            "WHERE T0.docNum=@docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetString(0);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en nombreDocumentoPDF: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public String nombreNC_PDF(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            String result = "";
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT   " +
                            "    CONCAT(CAST(T0.U_E_CODGENE AS text),CAST(' - ' AS text),CAST(T0.U_E_NUMCONT AS text))  " +
                            "FROM ORIN T0 " +
                            "JOIN OCRD T1 ON T0.CardCode = T1.CardCode " +
                            "WHERE T0.docNum=@docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetString(0);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en nombreDocumentoPDF: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public String nombreFSE_PDF(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection= conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            String result = "";
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT   " +
                            "    CONCAT(CAST(T0.U_E_CODGENE AS text),CAST(' - ' AS text),CAST(T0.U_E_NUMCONT AS text))  " +
                            "FROM OPCH T0 " +
                            "JOIN OCRD T1 ON T0.CardCode = T1.CardCode " +
                            "WHERE T0.docNum=@docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetString(0);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en nombreDocumentoPDF: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public List<DocLote> obtenerDocsLote(int tipoDoc, string serie, string fecha1, string fecha2)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            String result = "";
            DocLote docLote = new DocLote();
            List<DocLote> DocsLote = new List<DocLote>();
            try
            {
                sqlConnection.Open();
                String SQL = "EXEC SP_LISTAR_DOCS_LOTE_FEL @TIPODOC=@tipoDoc, @SERIE=@serie, @FECHA1=@fecha1, @FECHA2=@fecha2 ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@tipoDoc", tipoDoc);
                command.Parameters.AddWithValue("@serie", serie);
                command.Parameters.AddWithValue("@fecha1", fecha1.Replace("-", ""));
                command.Parameters.AddWithValue("@fecha2", fecha2.Replace("-", ""));

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    docLote = new DocLote();
                    docLote.tipoDoc = dr.GetString(0);
                    docLote.docNum = dr.GetInt32(1);
                    docLote.DocDate = dr.GetString(2);
                    docLote.cardCode = dr.GetString(3);
                    docLote.cardName = dr.GetString(4);
                    docLote.docTotal = dr.GetDecimal(5);
                    docLote.iva = dr.GetDecimal(6);
                    DocsLote.Add(docLote);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en obtenerDocsLote: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return DocsLote;
        }

        public List<DocLote> obtenerDocsLoteContin(int tipoDoc, string serie, string contingencia)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            String result = "";
            DocLote docLote = new DocLote();
            List<DocLote> DocsLote = new List<DocLote>();
            try
            {
                sqlConnection.Open();
                String SQL = "DECLARE @FE1 DATETIME=(SELECT K0.U_fechaIni FROM [@FE_CONTINGENCIA] K0 WHERE K0.Code=@conti); " +
                            "DECLARE @FE2 DATETIME=(SELECT K0.U_fechaFin FROM [@FE_CONTINGENCIA] K0 WHERE K0.Code=@conti); " +
                            "EXEC SP_LISTAR_DOCS_LOTE_FEL  " +
                            "@TIPODOC=@tipoDoc,  " +
                            "@SERIE=@serie,  " +
                            "@FECHA1=@FE1,  " +
                            "@FECHA2=@FE2 ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@conti", contingencia);
                command.Parameters.AddWithValue("@tipoDoc", tipoDoc);
                command.Parameters.AddWithValue("@serie", serie);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    docLote = new DocLote();
                    docLote.tipoDoc = dr.GetString(0);
                    docLote.docNum = dr.GetInt32(1);
                    docLote.DocDate = dr.GetString(2);
                    docLote.cardCode = dr.GetString(3);
                    docLote.cardName = dr.GetString(4);
                    docLote.docTotal = dr.GetDecimal(5);
                    docLote.iva = dr.GetDecimal(6);
                    DocsLote.Add(docLote);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en obtenerDocsLote: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return DocsLote;
        }

        public List<Contingencia> obtenerContingencias()
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            String result = "";
            Contingencia contingencia = new Contingencia();
            List<Contingencia> contingencias = new List<Contingencia>();
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT TOP 100 " +
                            "    T0.U_tipoContingencia, " +
                            "    T0.U_descContingencia, " +
                            "    ISNULL(T0.U_motivo,'') U_motivo, " +
                            "    CAST(CAST(T0.U_fechaIni AS DATE) AS VARCHAR) fechaIni, " +
                            "    CAST(CAST(T0.U_fechaFin AS DATE) AS VARCHAR) fechaFin, " +
                            "    T0.U_horainI, " +
                            "    T0.U_horaFin, " +
                            "    ISNULL(T0.U_E_SELLRECEP,'') U_E_SELLRECEP " +
                            "FROM [@FE_CONTINGENCIA] T0 " +
                            "ORDER BY Code DESC ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    contingencia = new Contingencia();
                    contingencia.tipoContingencia = dr.GetString(0);
                    contingencia.descContingencia = dr.GetString(1);
                    contingencia.motivo = dr.GetString(2);
                    contingencia.fechaIni = dr.GetString(3);
                    contingencia.fechaFin = dr.GetString(4);
                    contingencia.horaIni = dr.GetString(5);
                    contingencia.horaFin = dr.GetString(6);
                    contingencia.selloRecepcion = dr.GetString(7);
                    contingencias.Add(contingencia);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en obtenerContingencias: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return contingencias;
        }

        public Contingencia obtenerContingencia(string code)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            String result = "";
            Contingencia contingencia = new Contingencia();
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT TOP 100 " +
                            "    T0.U_tipoContingencia, " +
                            "    T0.U_descContingencia, " +
                            "    ISNULL(T0.U_motivo,'') U_motivo, " +
                            "    CAST(CAST(T0.U_fechaIni AS DATE) AS VARCHAR) fechaIni, " +
                            "    CAST(CAST(T0.U_fechaFin AS DATE) AS VARCHAR) fechaFin, " +
                            "    T0.U_horainI, " +
                            "    T0.U_horaFin, " +
                            "    ISNULL(T0.U_E_SELLRECEP,'') U_E_SELLRECEP " +
                            "FROM [@FE_CONTINGENCIA] T0 " +
                            "WHERE T0.code=@code " +
                            "ORDER BY Code DESC ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@code", code);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    contingencia = new Contingencia();
                    contingencia.tipoContingencia = dr.GetString(0);
                    contingencia.descContingencia = dr.GetString(1);
                    contingencia.motivo = dr.GetString(2);
                    contingencia.fechaIni = dr.GetString(3);
                    contingencia.fechaFin = dr.GetString(4);
                    contingencia.horaIni = dr.GetString(5);
                    contingencia.horaFin = dr.GetString(6);
                    contingencia.selloRecepcion = dr.GetString(7);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en obtenerContingencias: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return contingencia;
        }

        public List<Contingencia> obtenerListaContingencia()
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            String result = "";
            Contingencia contingencia = new Contingencia();
            List<Contingencia> contingencias= new List<Contingencia>();
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT TOP 20 " +
                             "  T0.code, " +
	                         "  T0.U_descContingencia " +
                            "FROM [@FE_CONTINGENCIA] T0 " +
                            "ORDER BY Code DESC ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    contingencia = new Contingencia();
                    contingencia.tipoContingencia = dr.GetString(0);
                    contingencia.descContingencia = dr.GetString(1);
                    contingencias.Add(contingencia);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en obtenerListaContingencias: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return contingencias;
        }

        public String obtenerDocNumOINV(string codGen)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            String result = "";
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT T0.DocNum FROM OINV T0 WHERE CAST(T0.U_E_CODGENE AS VARCHAR)=@codGen";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@codGen", codGen);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetString(0);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en obtenerDocNumOINV: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public String obtenerDocNumOPCH(string codGen)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            String result = "";
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT T0.DocNum FROM OPCH T0 WHERE CAST(T0.U_E_CODGENE AS VARCHAR)=@codGen";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@codGen", codGen);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetString(0);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en obtenerDocNumOPCH: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public String obtenerDocNumOWTR(string codGen)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            String result = "";
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT T0.DocNum FROM OWTR T0 WHERE CAST(T0.U_E_CODGENE AS VARCHAR)=@codGen";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@codGen", codGen);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetString(0);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en obtenerDocNumOWTR: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public String paisNR(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            String result = "";
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT   " +
                            "    T1.U_FE_PAIS   " +
                            "FROM OWTR T0 " +
                            "JOIN OCRD T1 ON T0.CardCode = T1.CardCode " +
                            "WHERE T0.docNum=@docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetString(0);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en paisNR: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public String paisFAC(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            String result = "";
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT   " +
                            "    T1.U_FE_PAIS   " +
                            "FROM OINV T0 " +
                            "JOIN OCRD T1 ON T0.CardCode = T1.CardCode " +
                            "WHERE T0.docNum=@docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetString(0);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en paisFAC: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public String paisNC(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            String result = "";
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT   " +
                            "    T1.U_FE_PAIS   " +
                            "FROM ORIN T0 " +
                            "JOIN OCRD T1 ON T0.CardCode = T1.CardCode " +
                            "WHERE T0.docNum=@docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetString(0);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en paisNC: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public String paisFSE(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            String result = "";
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT   " +
                            "    T1.U_FE_PAIS   " +
                            "FROM OPCH T0 " +
                            "JOIN OCRD T1 ON T0.CardCode = T1.CardCode " +
                            "WHERE T0.docNum=@docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);

                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetString(0);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en paisFSE: " + e.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public int obtenerCancelDocFAC(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT TOP 1 CASE WHEN T0.CANCELED!='N' THEN -1 ELSE T0.DocNum END DocNum FROM OINV T0 WHERE T0.docNum=@docNum AND T0.DocSubType!='DN' ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);
                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error al obtener informacion de cancelacion de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerCancelDocFAC: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public int obtenerCancelDocNC(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT TOP 1 CASE WHEN T0.CANCELED!='N' THEN -1 ELSE T0.DocNum END DocNum FROM ORIN T0 WHERE T0.docNum=@docNum";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);
                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error al obtener informacion de cancelacion de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerCancelDocNC: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public int obtenerCancelDocND(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT TOP 1 CASE WHEN T0.CANCELED!='N' THEN -1 ELSE T0.DocNum END DocNum FROM OINV T0 WHERE T0.docNum=@docNum AND T0.DocSubType='DN'";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);
                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error al obtener informacion de cancelacion de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerCancelDocND: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public int obtenerCancelDocNR(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT TOP 1 CASE WHEN T0.CANCELED!='N' THEN -1 ELSE T0.DocNum END DocNum FROM OWTR T0 WHERE T0.docNum=@docNum ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);
                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error al obtener informacion de cancelacion de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerCancelDocNR: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public int obtenerCancelDocFSE(int docNum)
        {
            ConexionSQL conexionSQL = new ConexionSQL();
            SqlConnection sqlConnection = conexionSQL.connection(this.serverDB, this.DB, this.userDB, this.passDB);
            int result = 0;
            try
            {
                sqlConnection.Open();
                String SQL = "SELECT TOP 1 CASE WHEN T0.CANCELED!='N' THEN -1 ELSE T0.DocNum END DocNum FROM OPCH T0 WHERE T0.docNum=@docNum ";
                SqlCommand command = new SqlCommand(SQL, sqlConnection);
                command.Parameters.AddWithValue("@docNum", docNum);
                SqlDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    result = dr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Null"))
                {

                    MessageBox.Show("Error al obtener informacion de cancelacion de documento. Por favor Revisar la información del documento. ");
                }
                else
                {
                    MessageBox.Show("Error en obtenerCancelDocFSE: " + e.Message);
                }
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }
    }
}
