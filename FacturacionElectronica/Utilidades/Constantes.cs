using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public static class Constantes
    {
        public const String stringConexionSAP = "0030002C0030002C00530041005000420044005F00440061007400650076002C0050004C006F006D0056004900490056";

        //-----------------------------------------------------------
        //CONSTANTES FIRMADOR
        public const String urlFirmador = "http://localhost:8113/firmardocumento/";
        public const String urlAutenticadorTest = "https://apitest.dtes.mh.gob.sv/seguridad/auth";
        public const String urlRecepcionTest = "https://apitest.dtes.mh.gob.sv/fesv/recepciondte";
        public const String urlAnulacionTest = "https://apitest.dtes.mh.gob.sv/fesv/anulardte";
        public const String urlLoteTest = "https://apitest.dtes.mh.gob.sv/fesv/recepcionlote/";
        public const String urlConsultaLoteTest = "https://apitest.dtes.mh.gob.sv/fesv/recepcion/consultadtelote/";
        public const String urlContingenciaTest = "https://apitest.dtes.mh.gob.sv/fesv/contingencia";

        public const String urlAutenticador = "https://api.dtes.mh.gob.sv/seguridad/auth";
        public const String urlRecepcion = "https://api.dtes.mh.gob.sv/fesv/recepciondte";
        public const String urlAnulacion = "https://api.dtes.mh.gob.sv/fesv/anulardte";
        public const String urlLote = "https://api.dtes.mh.gob.sv/fesv/recepcionlote/";
        public const String urlConsultaLote = "https://api.dtes.mh.gob.sv/fesv/recepcion/consultadtelote/";
        public const String urlContingencia = "https://api.dtes.mh.gob.sv/fesv/contingencia";

        public const String nit = "06142902640010";
        public const String passwordPri = "!Alsasa2023#";
        public const String pwd = "oG410w9#MOh7";
        public const String ambiente = "00";
        public const String correoEnvia = "ventas.alsasa@gmail.com";
        public const String contraCorreo = "hespztbyjnhtpltb";

        //-----------------------------------------------------------
        //CONSTANTES BASE DE DATOS
        public const String serverDB = "200.35.189.153";
        public const String DB = "SBO_FEL";
        public const String userDB = "developer";
        public const String passDB = "Alsasa2020$";

        //-----------------------------------------------------------
        //CONSTANTES EMISOR DTE
        public const String nrc= "5215";
        public const String nombreEmisor = "ALUMINIO DE EL SALVADOR. S.A.";
        public const String nombreComercial = "ALSASA";
        public const String codActividad = "24200";
        public const String descActividad = "Fabricación de productos primarios de metales preciosos y metales no ferrosos";
        public const String tipoEstablecimiento = "02";
        public const String dirDepartamento = "05";
        public const String dirMunicipio = "03";
        public const String dirComplemento = "Carretera a Sonsonate, Km 28 1/2, Lourdes, Colón, El Salvador.";
        public const String telefono = "2309-9999";
        public const String correo = "info@alsasa.com";
        public const String recintoFiscal = "";
        public const String regimen = "";
    }
}
