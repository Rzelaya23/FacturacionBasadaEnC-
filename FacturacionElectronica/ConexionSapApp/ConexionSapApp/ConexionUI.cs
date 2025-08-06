using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Utilidades;
using Utilidades.jsonContingencia;
using Newtonsoft.Json;
using System.Dynamic;
using System.Text.Json;
using System.Web;
using System.IO;
using System.Net.Mail;
using CrystalDecisions.Shared;
using CrystalDecisions.CrystalReports.Engine;
using ConsumoAPI;
using System.Globalization;

namespace ConexionSapApp
{
    public class ConexionUI
    {
        private SAPbouiCOM.Application mySBO_Application;
        private SAPbobsCOM.Company mySBO_Company;
        SAPbouiCOM.Form myForm;
        ReportDocument crystalReport;
        Consultas consultas;

        private string serverDB;
        private string DB;
        private string userDB;
        private string passDB;

        private string UserLog;

        public ConexionUI()
        {
        }

        //CREAR CONEXION CON APLICACION DE SAP
        public void connectUI()
        {
            try
            {

                SAPbouiCOM.SboGuiApi SboGuiApi = null;
                SboGuiApi = new SAPbouiCOM.SboGuiApi();

                SboGuiApi.Connect(Constantes.stringConexionSAP);

                mySBO_Application = SboGuiApi.GetApplication(-1);

                mySBO_Application.StatusBar.SetText("ADD ON DTE INICIADO CORRECTAMENTE", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                conectarDI();
                if (mySBO_Company.Connect() != 0)
                {
                    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                }
                else
                {
                    UserLog = mySBO_Company.UserName;
                    SAPbobsCOM.UserTable oUserTable = mySBO_Company.UserTables.Item("FE_DB");
                    if (oUserTable.GetByKey("1") == true)
                    {
                        serverDB = oUserTable.UserFields.Fields.Item("U_serverDB").Value.ToString();
                        DB = oUserTable.UserFields.Fields.Item("U_DB").Value.ToString();
                        userDB = oUserTable.UserFields.Fields.Item("U_userDB").Value.ToString();
                        passDB = oUserTable.UserFields.Fields.Item("U_passDB").Value.ToString();
                        consultas = new Consultas(oUserTable.UserFields.Fields.Item("U_serverDB").Value.ToString(),
                                                  oUserTable.UserFields.Fields.Item("U_DB").Value.ToString(),
                                                  oUserTable.UserFields.Fields.Item("U_userDB").Value.ToString(),
                                                  oUserTable.UserFields.Fields.Item("U_passDB").Value.ToString());
                        
                    }
                    else
                    {
                        crearFormularioDB();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error general de add-on al conectarse a SAP B1: " + e.Message);
                Console.WriteLine(e.Message);
                System.Environment.Exit(0);
            }
            finally
            {
                mySBO_Company.Disconnect();
            }
        }

        //CONTROLADOR DE EVENTOS DE MENU
        public void inicializacionEventos()
        {
            mySBO_Application.ItemEvent += new SAPbouiCOM._IApplicationEvents_ItemEventEventHandler(this.manejadorEventosControles);
            mySBO_Application.AppEvent += new SAPbouiCOM._IApplicationEvents_AppEventEventHandler(this.manejadorEventosAppSAP);
            mySBO_Application.MenuEvent += new SAPbouiCOM._IApplicationEvents_MenuEventEventHandler(this.manejoEventosMenu);
        }

        private void manejadorEventosControles(string formUID, ref SAPbouiCOM.ItemEvent pval, out bool bubleEvent)
        {
            bubleEvent = true;
            int option = 0;

            Cliente cliente = new Cliente();
            
            try
            {
                if (pval.FormType == 133 || pval.FormType == 60091 || pval.FormType == 60090)//codigo de formulario de facturas
                {
                    SAPbouiCOM.IItems oItems = mySBO_Application.Forms.Item(formUID).Items;
                    SAPbouiCOM.Item oItem;
                    SAPbouiCOM.Button oButton;
                    SAPbouiCOM.Item oItemTransmitir=null;
                    SAPbouiCOM.Button oButtonTransmitir = null;
                    SAPbouiCOM.Item oItemEnviar = null;
                    SAPbouiCOM.Button oButtonEnviar = null;
                    SAPbouiCOM.Item oItemAnular = null;
                    SAPbouiCOM.Button oButtonAnular = null;

                    //SAPbouiCOM.EditText txt = (SAPbouiCOM.EditText)oItems.Item("U_E_SELLRECEP").Specific;
                    //Console.WriteLine("SELLO: " + txt.Value);

                    //CREAR BOTON TRANSMITIR DTE
                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_FORM_LOAD && pval.BeforeAction == false)
                    {
                        oItemTransmitir = oItems.Add("MV_btnDTE", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                        oItemTransmitir.Top = oItems.Item("2").Top;
                        oItemTransmitir.Left = oItems.Item("2").Left + oItems.Item("2").Width + 4;
                        oItemTransmitir.Width = oItems.Item("2").Width + 10;

                        oButtonTransmitir = (SAPbouiCOM.Button)oItemTransmitir.Specific;
                        oButtonTransmitir.Caption = "Transmitir DTE";

                        oItemEnviar = oItems.Add("MV_btnMail", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                        oItemEnviar.Top = oItems.Item("MV_btnDTE").Top;
                        oItemEnviar.Left = oItems.Item("MV_btnDTE").Left + oItems.Item("MV_btnDTE").Width + 4;
                        oItemEnviar.Width = oItems.Item("MV_btnDTE").Width - 11;

                        oButtonEnviar = (SAPbouiCOM.Button)oItemEnviar.Specific;
                        oButtonEnviar.Caption = "Enviar DTE";

                        oItemAnular = oItems.Add("MV_btnAnul", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                        oItemAnular.Top = oItems.Item("MV_btnMail").Top;
                        oItemAnular.Left = oItems.Item("MV_btnMail").Left - oItems.Item("MV_btnMail").Width + 130;
                        oItemAnular.Width = oItems.Item("MV_btnMail").Width;

                        oButtonAnular = (SAPbouiCOM.Button)oItemAnular.Specific;
                        oButtonAnular.Caption = "Anular DTE";

                    } 

                    //EVENTO BOTON GUARDAR
                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && pval.FormMode == ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.BeforeAction == false && pval.ItemUID == "1")
                    {

                        if (pval.ActionSuccess == true)
                        //if (true == true)
                        {
                            int docNum = consultas.obtenerDocNumFAC();
                            //bubleEvent = false;
                            if (docNum!=-1)
                            {
                                option = mySBO_Application.MessageBox("¿Enviar los datos de la factura para su firmado y transmisión al sistema de facturacion electronica?", 1, "Si", "No");

                                if (option == 1)
                                {
                                    cliente = consultas.obtenerInfoClienteFAC(docNum);

                                    switch (cliente.tipoDoc)
                                    {
                                        case "01":
                                            enviarFE(cliente, docNum);
                                            break;
                                        case "03":
                                            enviarCCF(cliente, docNum);
                                            break;
                                        case "11":
                                            enviarFEX(cliente, docNum);
                                            break;
                                    }


                                }
                                else
                                {
                                    mySBO_Application.MessageBox("Factura no enviada al sistema de facturacion electronica");
                                }
                            }
                            
                        }
                    }

                    //EVENTO BOTON Trasmitir DTE
                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && (pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_FIND_MODE)) && pval.BeforeAction == false && pval.ItemUID == "MV_btnDTE")
                    {

                        if (pval.ActionSuccess == true)
                        //if (true == true)
                        {

                            //stringJSONContingencia();
                            //bubleEvent = false;
                            SAPbouiCOM.EditText txtDocNum = (SAPbouiCOM.EditText)oItems.Item("8").Specific;
                            int docNum = Int32.Parse(txtDocNum.Value);
                            int res = consultas.obtenerCancelDocFAC(docNum);
                            if (res > 0)
                            {
                                option = mySBO_Application.MessageBox("¿Enviar los datos de la factura para su firmado y transmisión al sistema de facturacion electronica?", 1, "Si", "No");

                                if (option == 1)
                                {

                                    cliente = consultas.obtenerInfoClienteFAC(docNum);

                                    switch (cliente.tipoDoc)
                                    {
                                        case "01":
                                            enviarFE(cliente, docNum);
                                            break;
                                        case "03":
                                            enviarCCF(cliente, docNum);
                                            break;
                                        case "11":
                                            enviarFEX(cliente, docNum);
                                            break;
                                    }


                                }
                                else
                                {
                                    mySBO_Application.MessageBox("Factura no enviada al sistema de facturacion electronica");
                                }

                            }
                        }
                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && (pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_FIND_MODE)) && pval.BeforeAction == false && pval.ItemUID == "MV_btnMail")
                    {

                        if (pval.ActionSuccess == true)
                        //if (true == true)
                        {

                            //bubleEvent = false;
                            option = mySBO_Application.MessageBox("¿Enviar Correo al cliente?", 1, "Si", "No");

                            if (option == 1)
                            {
                                SAPbouiCOM.EditText txtDocNum = (SAPbouiCOM.EditText)oItems.Item("8").Specific;


                                int docNum = Int32.Parse(txtDocNum.Value);
                                cliente = consultas.obtenerInfoClienteFAC(docNum);
                                    switch (cliente.tipoDoc)
                                    {
                                        case "01":
                                            EnviarCorreoDTE_FAC(docNum);
                                            break;
                                        case "03":
                                            EnviarCorreoDTE_CCF(docNum);
                                            break;
                                        case "11":
                                            EnviarCorreoDTE_FEX(docNum);
                                            break;
                                }
                            }
                            else
                            {
                                mySBO_Application.MessageBox("Factura no enviada al correo del cliente");
                            }
                        }
                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && (pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_FIND_MODE)) && pval.BeforeAction == false && pval.ItemUID == "MV_btnAnul")
                    {

                        if (pval.ActionSuccess == true)
                        //if (true == true)
                        {

                            //bubleEvent = false;
                            option = mySBO_Application.MessageBox("¿Anular DTE?", 1, "Si", "No");

                            if (option == 1)
                            {
                                SAPbouiCOM.EditText txtDocNum = (SAPbouiCOM.EditText)oItems.Item("8").Specific;
                                SAPbouiCOM.EditText txtCardCode = (SAPbouiCOM.EditText)oItems.Item("4").Specific;


                                int docNum = Int32.Parse(txtDocNum.Value);
                                cliente = consultas.obtenerInfoClienteFAC(docNum);
                                switch (cliente.tipoDoc)
                                {
                                    case "01":
                                        crearFormularioAnulacion(docNum, cliente.tipoDoc, txtCardCode.Value);
                                        break;
                                    case "03":
                                        crearFormularioAnulacion(docNum, cliente.tipoDoc, txtCardCode.Value);
                                        break;
                                    case "11":
                                        crearFormularioAnulacion(docNum, cliente.tipoDoc, txtCardCode.Value);
                                        break;
                                }
                            }
                        }
                    }
                }


                //-----------------------------------------------------------------------------------------------

            if (pval.FormType == 179)//codigo de formulario de nota de credito de clientes
                {
                    SAPbouiCOM.IItems oItems = mySBO_Application.Forms.Item(formUID).Items;
                    SAPbouiCOM.Item oItem;
                    SAPbouiCOM.Button oButton;
                    SAPbouiCOM.Item oItemTransmitir;
                    SAPbouiCOM.Button oButtonTransmitir;
                    SAPbouiCOM.Item oItemEnviar;
                    SAPbouiCOM.Button oButtonEnviar;
                    SAPbouiCOM.Item oItemAnular;
                    SAPbouiCOM.Button oButtonAnular;

                    //CREAR BOTON TRANSMITIR DTE
                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_FORM_LOAD && pval.BeforeAction == false)
                    {
                        oItemTransmitir = oItems.Add("MV_btnDTE", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                        oItemTransmitir.Top = oItems.Item("2").Top;
                        oItemTransmitir.Left = oItems.Item("2").Left + oItems.Item("2").Width + 4;
                        oItemTransmitir.Width = oItems.Item("2").Width + 10;

                        oButtonTransmitir = (SAPbouiCOM.Button)oItemTransmitir.Specific;
                        oButtonTransmitir.Caption = "Transmitir DTE";

                        oItemEnviar = oItems.Add("MV_btnMail", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                        oItemEnviar.Top = oItems.Item("MV_btnDTE").Top;
                        oItemEnviar.Left = oItems.Item("MV_btnDTE").Left + oItems.Item("MV_btnDTE").Width + 4;
                        oItemEnviar.Width = oItems.Item("MV_btnDTE").Width-11;

                        oButtonEnviar = (SAPbouiCOM.Button)oItemEnviar.Specific;
                        oButtonEnviar.Caption = "Enviar DTE";

                        oItemAnular = oItems.Add("MV_btnAnul", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                        oItemAnular.Top = oItems.Item("MV_btnMail").Top;
                        oItemAnular.Left = oItems.Item("MV_btnMail").Left - oItems.Item("MV_btnMail").Width + 130;
                        oItemAnular.Width = oItems.Item("MV_btnMail").Width;

                        oButtonAnular = (SAPbouiCOM.Button)oItemAnular.Specific;
                        oButtonAnular.Caption = "Anular DTE";

                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && pval.FormMode == ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.BeforeAction == false && pval.ItemUID == "1")
                    {
                        if (pval.ActionSuccess == true)
                        // if (true == true)
                        {
                            int docNum = consultas.obtenerDocNumNC();
                            //bubleEvent = false;
                            if (docNum!=-1)
                            {
                                option = mySBO_Application.MessageBox("¿Enviar los datos de la Nota de credito para su firmado y transmisión al sistema de facturacion electronica?", 1, "Si", "No");

                                if (option == 1)
                                {
                                    cliente = consultas.obtenerInfoClienteNC(docNum);
                                    enviarNC(cliente, docNum);
                                }
                            }
                        }
                    }

                    //EVENTO BOTON Trasmitir DTE
                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && (pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_FIND_MODE)) && pval.BeforeAction == false && pval.ItemUID == "MV_btnDTE")
                    {
                        if (pval.ActionSuccess == true)
                        //if (true == true)
                        {
                            //bubleEvent = false;

                            SAPbouiCOM.EditText txtDocNum = (SAPbouiCOM.EditText)oItems.Item("8").Specific;
                            int docNum = Int32.Parse(txtDocNum.Value);
                            int res = consultas.obtenerCancelDocNC(docNum);
                            if (res > 0)
                            {
                                option = mySBO_Application.MessageBox("¿Enviar los datos de la Nota de credito para su firmado y transmisión al sistema de facturacion electronica?", 1, "Si", "No");

                                if (option == 1)
                                {
                                    cliente = consultas.obtenerInfoClienteNC(docNum);

                                    enviarNC(cliente, docNum);
                                }
                            }
                        }
                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && (pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_FIND_MODE)) && pval.BeforeAction == false && pval.ItemUID == "MV_btnMail")
                    {

                        if (pval.ActionSuccess == true)
                        //if (true == true)
                        {

                            //bubleEvent = false;
                            option = mySBO_Application.MessageBox("¿Enviar Correo al cliente?", 1, "Si", "No");

                            if (option == 1)
                            {
                                SAPbouiCOM.EditText txtDocNum = (SAPbouiCOM.EditText)oItems.Item("8").Specific;


                                int docNum = Int32.Parse(txtDocNum.Value);
                                
                                EnviarCorreoDTE_NC(docNum);
                            }
                            else
                            {
                                mySBO_Application.MessageBox("Factura no enviada al correo del cliente");
                            }
                        }
                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && (pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_FIND_MODE)) && pval.BeforeAction == false && pval.ItemUID == "MV_btnAnul")
                    {

                        if (pval.ActionSuccess == true)
                        //if (true == true)
                        {

                            //bubleEvent = false;
                            option = mySBO_Application.MessageBox("¿Anular DTE?", 1, "Si", "No");

                            if (option == 1)
                            {
                                SAPbouiCOM.EditText txtDocNum = (SAPbouiCOM.EditText)oItems.Item("8").Specific;
                                SAPbouiCOM.EditText txtCardCode = (SAPbouiCOM.EditText)oItems.Item("4").Specific;


                                int docNum = Int32.Parse(txtDocNum.Value);

                                crearFormularioAnulacion(docNum, "05", txtCardCode.Value);
                            }
                        }
                    }
                }


                //-----------------------------------------------------------------------------------------------

                if (pval.FormType == 940)//codigo de formulario de nota de remision de clientes
                {
                    SAPbouiCOM.IItems oItems = mySBO_Application.Forms.Item(formUID).Items;
                    SAPbouiCOM.Item oItem;
                    SAPbouiCOM.Button oButton;
                    SAPbouiCOM.Item oItemTransmitir;
                    SAPbouiCOM.Button oButtonTransmitir;
                    SAPbouiCOM.Item oItemEnviar;
                    SAPbouiCOM.Button oButtonEnviar;
                    SAPbouiCOM.Item oItemAnular;
                    SAPbouiCOM.Button oButtonAnular;

                    //CREAR BOTON TRANSMITIR DTE
                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_FORM_LOAD && pval.BeforeAction == false)
                    {
                        oItemTransmitir = oItems.Add("MV_btnDTE", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                        oItemTransmitir.Top = oItems.Item("2").Top;
                        oItemTransmitir.Left = oItems.Item("2").Left + oItems.Item("2").Width + 4;
                        oItemTransmitir.Width = oItems.Item("2").Width + 10;

                        oButtonTransmitir = (SAPbouiCOM.Button)oItemTransmitir.Specific;
                        oButtonTransmitir.Caption = "Transmitir DTE";

                        oItemEnviar = oItems.Add("MV_btnMail", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                        oItemEnviar.Top = oItems.Item("MV_btnDTE").Top;
                        oItemEnviar.Left = oItems.Item("MV_btnDTE").Left + oItems.Item("MV_btnDTE").Width + 4;
                        oItemEnviar.Width = oItems.Item("MV_btnDTE").Width - 11;

                        oButtonEnviar = (SAPbouiCOM.Button)oItemEnviar.Specific;
                        oButtonEnviar.Caption = "Enviar DTE";

                        oItemAnular = oItems.Add("MV_btnAnul", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                        oItemAnular.Top = oItems.Item("MV_btnMail").Top;
                        oItemAnular.Left = oItems.Item("MV_btnMail").Left - oItems.Item("MV_btnMail").Width + 130;
                        oItemAnular.Width = oItems.Item("MV_btnMail").Width;

                        oButtonAnular = (SAPbouiCOM.Button)oItemAnular.Specific;
                        oButtonAnular.Caption = "Anular DTE";

                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && pval.FormMode == ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.BeforeAction == false && pval.ItemUID == "1")
                    {
                        if (pval.ActionSuccess == true)
                        // if (true == true)
                        {
                            //bubleEvent = false;
                            int docNum = consultas.obtenerDocNumNR();
                            if (docNum!=-1)
                            {
                                option = mySBO_Application.MessageBox("¿Enviar los datos de la Nota de remisión para su firmado y transmisión al sistema de facturacion electronica?", 1, "Si", "No");

                                if (option == 1)
                                {
                                    cliente = consultas.obtenerInfoClienteNR(docNum);
                                    enviarNR(cliente, docNum);
                                }
                            }
                        }
                    }

                    //EVENTO BOTON Trasmitir DTE
                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && (pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_FIND_MODE)) && pval.BeforeAction == false && pval.ItemUID == "MV_btnDTE")
                    {
                        if (pval.ActionSuccess == true)
                        //if (true == true)
                        {
                            //bubleEvent = false;

                            SAPbouiCOM.EditText txtDocNum = (SAPbouiCOM.EditText)oItems.Item("8").Specific;
                            int docNum = Int32.Parse(txtDocNum.Value);
                            int res = consultas.obtenerCancelDocNR(docNum);
                            if (res > 0)
                            {
                                option = mySBO_Application.MessageBox("¿Enviar los datos de la Nota de remisión para su firmado y transmisión al sistema de facturacion electronica?", 1, "Si", "No");

                                if (option == 1)
                                {
                                    cliente = consultas.obtenerInfoClienteNR(docNum);

                                    enviarNR(cliente, docNum);
                                }
                            }
                        }
                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && (pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_FIND_MODE)) && pval.BeforeAction == false && pval.ItemUID == "MV_btnMail")
                    {

                        if (pval.ActionSuccess == true)
                        //if (true == true)
                        {

                            //bubleEvent = false;
                            option = mySBO_Application.MessageBox("¿Enviar Correo al cliente?", 1, "Si", "No");

                            if (option == 1)
                            {
                                SAPbouiCOM.EditText txtDocNum = (SAPbouiCOM.EditText)oItems.Item("11").Specific;


                                int docNum = Int32.Parse(txtDocNum.Value);

                                EnviarCorreoDTE_NR(docNum);
                            }
                            else
                            {
                                mySBO_Application.MessageBox("Factura no enviada al correo del cliente");
                            }
                        }
                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && (pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_FIND_MODE)) && pval.BeforeAction == false && pval.ItemUID == "MV_btnAnul")
                    {

                        if (pval.ActionSuccess == true)
                        //if (true == true)
                        {

                            //bubleEvent = false;
                            option = mySBO_Application.MessageBox("¿Anular DTE?", 1, "Si", "No");

                            if (option == 1)
                            {
                                SAPbouiCOM.EditText txtDocNum = (SAPbouiCOM.EditText)oItems.Item("11").Specific;
                                SAPbouiCOM.EditText txtCardCode = (SAPbouiCOM.EditText)oItems.Item("3").Specific;


                                int docNum = Int32.Parse(txtDocNum.Value);

                                crearFormularioAnulacion(docNum, "04", txtCardCode.Value);
                            }
                        }
                    }
                }


                //-----------------------------------------------------------------------------------------------

                if (pval.FormType == 141)//codigo de formulario de FACTURA SE de clientes
                {
                    SAPbouiCOM.IItems oItems = mySBO_Application.Forms.Item(formUID).Items;
                    SAPbouiCOM.Item oItem;
                    SAPbouiCOM.Button oButton;
                    SAPbouiCOM.Item oItemTransmitir;
                    SAPbouiCOM.Button oButtonTransmitir;
                    SAPbouiCOM.Item oItemEnviar;
                    SAPbouiCOM.Button oButtonEnviar;
                    SAPbouiCOM.Item oItemAnular;
                    SAPbouiCOM.Button oButtonAnular;

                    //CREAR BOTON TRANSMITIR DTE
                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_FORM_LOAD && pval.BeforeAction == false)
                    {
                        oItemTransmitir = oItems.Add("MV_btnDTE", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                        oItemTransmitir.Top = oItems.Item("2").Top;
                        oItemTransmitir.Left = oItems.Item("2").Left + oItems.Item("2").Width + 4;
                        oItemTransmitir.Width = oItems.Item("2").Width + 10;

                        oButtonTransmitir = (SAPbouiCOM.Button)oItemTransmitir.Specific;
                        oButtonTransmitir.Caption = "Transmitir DTE";

                        oItemEnviar = oItems.Add("MV_btnMail", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                        oItemEnviar.Top = oItems.Item("MV_btnDTE").Top;
                        oItemEnviar.Left = oItems.Item("MV_btnDTE").Left + oItems.Item("MV_btnDTE").Width + 4;
                        oItemEnviar.Width = oItems.Item("MV_btnDTE").Width - 11;

                        oButtonEnviar = (SAPbouiCOM.Button)oItemEnviar.Specific;
                        oButtonEnviar.Caption = "Enviar DTE";

                        oItemAnular = oItems.Add("MV_btnAnul", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                        oItemAnular.Top = oItems.Item("MV_btnMail").Top;
                        oItemAnular.Left = oItems.Item("MV_btnMail").Left - oItems.Item("MV_btnMail").Width + 130;
                        oItemAnular.Width = oItems.Item("MV_btnMail").Width;

                        oButtonAnular = (SAPbouiCOM.Button)oItemAnular.Specific;
                        oButtonAnular.Caption = "Anular DTE";

                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && pval.FormMode == ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.BeforeAction == false && pval.ItemUID == "1")
                    {
                        if (pval.ActionSuccess == true)
                        // if (true == true)
                        {
                            //bubleEvent = false;
                            int docNum = consultas.obtenerDocNumFSE();
                            if (docNum!=-1)
                            {
                                option = mySBO_Application.MessageBox("¿Enviar los datos de la factura de sujeto excluido para su firmado y transmisión al sistema de facturacion electronica?", 1, "Si", "No");

                                if (option == 1)
                                {
                                    cliente = consultas.obtenerInfoClienteFSE(docNum);
                                    enviarFSE(cliente, docNum);
                                }
                            }
                        }
                    }

                    //EVENTO BOTON Trasmitir DTE
                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && (pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_FIND_MODE)) && pval.BeforeAction == false && pval.ItemUID == "MV_btnDTE")
                    {
                        if (pval.ActionSuccess == true)
                        //if (true == true)
                        {
                            //bubleEvent = false;

                            SAPbouiCOM.EditText txtDocNum = (SAPbouiCOM.EditText)oItems.Item("8").Specific;
                            int docNum = Int32.Parse(txtDocNum.Value);
                            int res = consultas.obtenerCancelDocFSE(docNum);
                            if (res > 0)
                            {
                                option = mySBO_Application.MessageBox("¿Enviar los datos de la Factura de sujeto excluido para su firmado y transmisión al sistema de facturacion electronica?", 1, "Si", "No");

                                if (option == 1)
                                {
                                    cliente = consultas.obtenerInfoClienteFSE(docNum);

                                    enviarFSE(cliente, docNum);
                                    //EnviarCorreoDTE_FSE(docNum);
                                }
                            }
                        }
                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && (pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_FIND_MODE)) && pval.BeforeAction == false && pval.ItemUID == "MV_btnMail")
                    {

                        if (pval.ActionSuccess == true)
                        //if (true == true)
                        {

                            //bubleEvent = false;
                            option = mySBO_Application.MessageBox("¿Enviar Correo al cliente?", 1, "Si", "No");

                            if (option == 1)
                            {
                                SAPbouiCOM.EditText txtDocNum = (SAPbouiCOM.EditText)oItems.Item("8").Specific;


                                int docNum = Int32.Parse(txtDocNum.Value);

                                EnviarCorreoDTE_FSE(docNum);
                            }
                            else
                            {
                                mySBO_Application.MessageBox("Factura no enviada al correo del cliente");
                            }
                        }
                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && (pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_FIND_MODE)) && pval.BeforeAction == false && pval.ItemUID == "MV_btnAnul")
                    {

                        if (pval.ActionSuccess == true)
                        //if (true == true)
                        {
                            //bubleEvent = false;
                            option = mySBO_Application.MessageBox("¿Anular DTE?", 1, "Si", "No");

                            if (option == 1)
                            {
                                SAPbouiCOM.EditText txtDocNum = (SAPbouiCOM.EditText)oItems.Item("8").Specific;
                                SAPbouiCOM.EditText txtCardCode = (SAPbouiCOM.EditText)oItems.Item("4").Specific;


                                int docNum = Int32.Parse(txtDocNum.Value);

                                crearFormularioAnulacion(docNum, "14", txtCardCode.Value);
                            }
                        }
                    }
                }


                //-----------------------------------------------------------------------------------------------

                if (pval.FormType == 65303)//codigo de formulario de nota de debito de clientes
                {
                    SAPbouiCOM.IItems oItems = mySBO_Application.Forms.Item(formUID).Items;
                    SAPbouiCOM.Item oItem;
                    SAPbouiCOM.Button oButton;
                    SAPbouiCOM.Item oItemTransmitir;
                    SAPbouiCOM.Button oButtonTransmitir;
                    SAPbouiCOM.Item oItemEnviar;
                    SAPbouiCOM.Button oButtonEnviar;
                    SAPbouiCOM.Item oItemAnular;
                    SAPbouiCOM.Button oButtonAnular;

                    //CREAR BOTON TRANSMITIR DTE
                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_FORM_LOAD && pval.BeforeAction == false)
                    {
                        oItemTransmitir = oItems.Add("MV_btnDTE", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                        oItemTransmitir.Top = oItems.Item("2").Top;
                        oItemTransmitir.Left = oItems.Item("2").Left + oItems.Item("2").Width + 4;
                        oItemTransmitir.Width = oItems.Item("2").Width + 10;

                        oButtonTransmitir = (SAPbouiCOM.Button)oItemTransmitir.Specific;
                        oButtonTransmitir.Caption = "Transmitir DTE";

                        oItemEnviar = oItems.Add("MV_btnMail", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                        oItemEnviar.Top = oItems.Item("MV_btnDTE").Top;
                        oItemEnviar.Left = oItems.Item("MV_btnDTE").Left + oItems.Item("MV_btnDTE").Width + 4;
                        oItemEnviar.Width = oItems.Item("MV_btnDTE").Width - 11;

                        oButtonEnviar = (SAPbouiCOM.Button)oItemEnviar.Specific;
                        oButtonEnviar.Caption = "Enviar DTE";

                        oItemAnular = oItems.Add("MV_btnAnul", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                        oItemAnular.Top = oItems.Item("MV_btnMail").Top;
                        oItemAnular.Left = oItems.Item("MV_btnMail").Left - oItems.Item("MV_btnMail").Width + 130;
                        oItemAnular.Width = oItems.Item("MV_btnMail").Width;

                        oButtonAnular = (SAPbouiCOM.Button)oItemAnular.Specific;
                        oButtonAnular.Caption = "Anular DTE";

                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && pval.FormMode == ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.BeforeAction == false && pval.ItemUID == "1")
                    { 
                        if (pval.ActionSuccess == true)
                        // if (true == true)
                        {
                            //bubleEvent = false;
                            int docNum = consultas.obtenerDocNumND();
                            if (docNum!=-1)
                            {
                                option = mySBO_Application.MessageBox("¿Enviar los datos de la nota de debito para su firmado y transmisión al sistema de facturacion electronica?", 1, "Si", "No");

                                if (option == 1)
                                {
                                    cliente = consultas.obtenerInfoClienteND(docNum);
                                    enviarND(cliente, docNum);
                                }
                            }
                        }
                    }

                    //EVENTO BOTON Trasmitir DTE
                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && (pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_FIND_MODE)) && pval.BeforeAction == false && pval.ItemUID == "MV_btnDTE")
                    {
                        if (pval.ActionSuccess == true)
                        //if (true == true)
                        {
                            //bubleEvent = false;

                            SAPbouiCOM.EditText txtDocNum = (SAPbouiCOM.EditText)oItems.Item("8").Specific;
                            int docNum = Int32.Parse(txtDocNum.Value);
                            int res = consultas.obtenerCancelDocND(docNum);
                            if (res > 0)
                            {
                                option = mySBO_Application.MessageBox("¿Enviar los datos de la nota de debito para su firmado y transmisión al sistema de facturacion electronica?", 1, "Si", "No");

                                if (option == 1)
                                {
                                    cliente = consultas.obtenerInfoClienteND(docNum);

                                    enviarND(cliente, docNum);
                                    //EnviarCorreoDTE_ND(docNum);
                                }
                            }
                        }
                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && (pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_FIND_MODE)) && pval.BeforeAction == false && pval.ItemUID == "MV_btnMail")
                    {

                        if (pval.ActionSuccess == true)
                        //if (true == true)
                        {

                            //bubleEvent = false;
                            option = mySBO_Application.MessageBox("¿Enviar Correo al cliente?", 1, "Si", "No");

                            if (option == 1)
                            {
                                SAPbouiCOM.EditText txtDocNum = (SAPbouiCOM.EditText)oItems.Item("8").Specific;


                                int docNum = Int32.Parse(txtDocNum.Value);

                                //EnviarCorreoDTE_ND(docNum);
                            }
                            else
                            {
                                mySBO_Application.MessageBox("Factura no enviada al correo del cliente");
                            }
                        }
                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && (pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_ADD_MODE) && pval.FormMode != ((int)SAPbouiCOM.BoFormMode.fm_FIND_MODE)) && pval.BeforeAction == false && pval.ItemUID == "MV_btnAnul")
                    {

                        if (pval.ActionSuccess == true)
                        //if (true == true)
                        {
                            //bubleEvent = false;
                            option = mySBO_Application.MessageBox("¿Anular DTE?", 1, "Si", "No");

                            if (option == 1)
                            {
                                SAPbouiCOM.EditText txtDocNum = (SAPbouiCOM.EditText)oItems.Item("8").Specific;
                                SAPbouiCOM.EditText txtCardCode = (SAPbouiCOM.EditText)oItems.Item("4").Specific;


                                int docNum = Int32.Parse(txtDocNum.Value);

                                crearFormularioAnulacion(docNum, "06", txtCardCode.Value);
                            }
                        }
                    }
                }

                //-----------------------------------------------------------------------------------------------

                if (pval.FormUID == "MV_GS_DTE")
                {
                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && pval.BeforeAction == false && pval.ItemUID == "MV_btnAdd")
                    {
                        option = mySBO_Application.MessageBox("¿Guardar datos del emisor?", 1, "Si", "No");

                        if (option == 1)
                        {
                            guardarEmisor();
                        }
                    }
                }

                if (pval.FormUID == "EA_frmGDB")
                {
                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && pval.BeforeAction == false && pval.ItemUID == "EA_btnSave")
                    {
                        option = mySBO_Application.MessageBox("¿Guardar datos de DB?", 1, "Si", "No");

                        if (option == 1)
                        {
                            guardaDB();
                        }
                    }
                }

                if (pval.FormUID == "EA_frmMail")
                {
                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && pval.BeforeAction == false && pval.ItemUID == "EA_btnSave")
                    {
                        option = mySBO_Application.MessageBox("¿Guardar datos de Correo?", 1, "Si", "No");

                        if (option == 1)
                        {
                            guardaCorreo();
                        }
                    }
                }

                if (pval.FormUID == "EA_frmLote")
                {
                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && pval.BeforeAction == false && pval.ItemUID == "EA_btnODoc")
                    {
                        option = mySBO_Application.MessageBox("¿Obtener documentos?", 1, "Si", "No");

                        if (option == 1)
                        {
                            CargarInfoDocs();
                        }
                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && pval.BeforeAction == false && pval.ItemUID == "EA_btnCanc")
                    {
                        myForm.Close();
                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && pval.BeforeAction == false && pval.ItemUID == "EA_btnTran")
                    {
                        option = mySBO_Application.MessageBox("¿Enviar documentos?", 1, "Si", "No");

                        if (option == 1)
                        {
                            enviarDTE_Lote();
                        }
                    }
                }

                if (pval.FormUID == "EA_FRM_ANU")
                {
                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && pval.BeforeAction == false && pval.ItemUID == "EA_btnAnul")
                    {
                        option = mySBO_Application.MessageBox("¿Seguro de anular DTE?", 1, "Si", "No");

                        if (option == 1)
                        {
                            SAPbouiCOM.EditText myEditText_docNum = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtDNum").Specific;
                            SAPbouiCOM.EditText myEditText_cl= (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtCL").Specific;
                            SAPbouiCOM.EditText myEditText_TipoDoc= (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtTDoc").Specific;
                            cliente = consultas.obtenerInfoClienteAnul(myEditText_cl.Value);
                            enviarAnulacion(consultas.obtenerInfoDoc(Int32.Parse(myEditText_docNum.Value), myEditText_TipoDoc.Value),cliente, Int32.Parse(myEditText_docNum.Value), myEditText_TipoDoc.Value);
                        }
                    }
                }

                if (pval.FormUID == "EA_frmCont")
                {
                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && pval.BeforeAction == false && pval.ItemUID == "EA_btnSave")
                    {
                        option = mySBO_Application.MessageBox("¿Guardar Contingencia?", 1, "Si", "No");

                        if (option == 1)
                        {
                            guardaContingencia();
                        }
                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && pval.BeforeAction == false && pval.ItemUID == "EA_btnCanc")
                    {
                        myForm.Close();
                    }
                }

                if (pval.FormUID == "EA_frmECon")
                {
                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && pval.BeforeAction == false && pval.ItemUID == "EA_btnODoc")
                    {
                        option = mySBO_Application.MessageBox("¿Obtener documentos?", 1, "Si", "No");

                        if (option == 1)
                        {
                            CargarInfoDocsContingencia();
                        }
                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && pval.BeforeAction == false && pval.ItemUID == "EA_btnTran")
                    {
                        option = mySBO_Application.MessageBox("¿enviar documentos?", 1, "Si", "No");

                        if (option == 1)
                        {
                            enviarDTE_Contingencia();
                        }
                    }

                    if (pval.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && pval.BeforeAction == false && pval.ItemUID == "EA_btnCanc")
                    {
                        myForm.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                if(!ex.Message.Equals("Form - Invalid Form"))
                    mySBO_Application.MessageBox("Error en manejadorEventosControladores: "+ ex.Message);
                    Console.WriteLine("Error en manejadorEventosControladores: " + ex.Message);
            }
        }

        private async Task enviarFE(Cliente cliente, int docNum)
        {
            try
            {
                conectarDI();
                if (mySBO_Company.Connect() != 0)
                {
                    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                }
                else
                {
                    jsonFirmadorFE jsonFirmador = new jsonFirmadorFE();//json a transmitir
                    JSONFE dtejson = new JSONFE();
                    IdentificacionFE identificacion = new IdentificacionFE();
                    Direccion direccion = new Direccion();
                    EmisorFE emisor = new EmisorFE();
                    ReceptorFE receptor = new ReceptorFE();
                    ExtensionFE extension = new ExtensionFE();

                    EmisorDTE emisorDTE = new EmisorDTE();

                    Guid g = Guid.NewGuid(); //LIBRERIA PARA CODIGO DE GENERACION
                    String codigoGeneracion = "";


                    ConsumoFirmador consumoFirmador = new ConsumoFirmador();

                    jsonReceptor jsonReceptor = new jsonReceptor();

                    dynamic jsonRespuesta;

                    emisorDTE = consultas.obtenerInfoEmisor();

                    //-------------------------------------------
                    jsonFirmador.contentType = "application/JSON";
                    jsonFirmador.nit = emisorDTE.userDTE;
                    jsonFirmador.activo = "True";
                    jsonFirmador.passwordPri = emisorDTE.passDTE;

                    //-------------------------------------------
                    identificacion.version = cliente.version;
                    identificacion.ambiente = emisorDTE.ambiente;
                    identificacion.tipoDte = cliente.tipoDoc;


                    string numeracion = "000000000000000";

                    string docNumber = docNum.ToString();

                    string numControl1 = (numeracion + docNumber.Substring(docNumber.Length - 4));

                    string numControl = "DTE-" + cliente.tipoDoc + "-" + cliente.sucursal + "01-" + numControl1.Substring(numControl1.Length - 15);

                    identificacion.numeroControl = numControl;
                    codigoGeneracion = g.ToString().ToUpper();
                    identificacion.codigoGeneracion = codigoGeneracion;
                    identificacion.tipoModelo = 1;
                    identificacion.tipoOperacion = 1;
                    identificacion.tipoContingencia = null;
                    identificacion.motivoContin = null;
                    if (DateTime.Now.ToString("dd") == "01" || DateTime.Now.ToString("dd") == "02" || DateTime.Now.ToString("dd") == "03" || DateTime.Now.ToString("dd") == "04" || DateTime.Now.ToString("dd") == "05")
                        identificacion.fecEmi = cliente.docDate;
                    else
                        identificacion.fecEmi = DateTime.Now.ToString("yyyy-MM-dd");
                    identificacion.horEmi = DateTime.Now.ToString("HH:mm:ss");
                    identificacion.tipoMoneda = "USD";


                    dtejson.identificacion = identificacion;

                    //-------------------------------------------
                    dtejson.documentoRelacionado = null;

                    //-------------------------------------------
                    direccion = new Direccion();
                    direccion.departamento = emisorDTE.departamento;
                    direccion.municipio = emisorDTE.municipio;
                    direccion.complemento = emisorDTE.direccion;

                    emisor.nit = emisorDTE.nit;
                    emisor.nrc = emisorDTE.nrc;
                    emisor.nombre = emisorDTE.nombre;
                    emisor.codActividad = emisorDTE.codActividad;
                    emisor.descActividad = emisorDTE.descActividad;
                    emisor.nombreComercial = emisorDTE.nombreComercial;
                    emisor.tipoEstablecimiento = emisorDTE.tipoEstablecimiento;
                    emisor.telefono = emisorDTE.telefono;
                    emisor.correo = emisorDTE.correo;
                    emisor.codEstableMH = null;
                    emisor.codEstable = null;
                    emisor.codPuntoVentaMH = null;
                    emisor.codPuntoVenta = null;
                    emisor.direccion = direccion;

                    dtejson.emisor = emisor;

                    //--------------------------------------------
                    direccion = new Direccion();
                    if (cliente.direccion != null && cliente.municipio != null && cliente.departamento != null)
                    {
                        direccion.departamento = cliente.departamento;
                        direccion.municipio = cliente.municipio;
                        direccion.complemento = cliente.direccion;
                    }

                    if (cliente.tipoDocumento != null)
                        receptor.tipoDocumento = cliente.tipoDocumento;

                    if (cliente.tipoDocumento != null)
                        receptor.numDocumento = cliente.tipoDocumento == "36" ? cliente.numDocumento.Replace("-", "") : cliente.numDocumento;

                    if (cliente.nrc != null)
                    {
                        receptor.nrc = cliente.nrc.ToString().Replace("-", "");
                    }

                    receptor.nombre = cliente.nombre;

                    if (cliente.actEconomicaCode != null)
                        receptor.codActividad = cliente.actEconomicaCode;

                    if (cliente.actEconomicaName != null)
                        receptor.descActividad = cliente.actEconomicaName;

                    if (cliente.telefono != null)
                        receptor.telefono = cliente.telefono;

                    if (cliente.correo != null)
                        receptor.correo = cliente.correo;

                    if (cliente.direccion != null && cliente.municipio != null && cliente.departamento != null)
                        receptor.direccion = direccion;

                    dtejson.receptor = receptor;

                    //----------------------------------------------------
                    dtejson.otrosDocumentos = null;
                    //----------------------------------------------------
                    dtejson.ventaTercero = null;
                    //----------------------------------------------------
                    dtejson.cuerpoDocumento = new List<CuerpoDocumentoFE>();
                    dtejson.cuerpoDocumento.AddRange(consultas.obtenerCuerpoDocFE(docNum));

                    //----------------------------------------------------

                    dtejson.resumen = consultas.obtenerResumenDocFE(docNum);

                    //----------------------------------------------------

                    extension = null;

                    dtejson.extension = extension;

                    //----------------------------------------------------

                    dtejson.apendice = null;


                    jsonFirmador.dteJson = dtejson;

                    string jsonDTE = JsonConvert.SerializeObject(dtejson);//CONVERTIR OBJETO A JSON

                    string json = JsonConvert.SerializeObject(jsonFirmador);//CONVERTIR OBJETO A JSON

                    Console.WriteLine(json);

                    WriteToFile("JSON PLANO FE: " + json);

                    jsonRespuesta = await consumoFirmador.postFirmador("http://" + serverDB + ":8113/firmardocumento/", json); //ENVIAR A SERVICIO DE FIRMADO

                    Response response = JsonConvert.DeserializeObject<Response>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO

                    if (response.status == "OK")
                    {

                        jsonRespuesta = await consumoFirmador.postAutenticador(emisorDTE.ambiente == "00" ? Constantes.urlAutenticadorTest : Constantes.urlAutenticador, emisorDTE.nit, emisorDTE.passPrivDTE);
                        ResponseAutenticador responseAutenticador = JsonConvert.DeserializeObject<ResponseAutenticador>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO
                        if (responseAutenticador.status == "OK")
                        {
                            jsonReceptor.ambiente = emisorDTE.ambiente;
                            jsonReceptor.idEnvio = docNum;
                            jsonReceptor.version = cliente.version;
                            jsonReceptor.tipoDte = cliente.tipoDoc;
                            jsonReceptor.documento = response.body.ToString();
                            jsonReceptor.codigoGeneracion = codigoGeneracion;
                            string jsonreceptor = JsonConvert.SerializeObject(jsonReceptor);//CONVERTIR OBJETO A JSON
                            Console.WriteLine(jsonreceptor);

                            WriteToFile("JSON FIRMADO FE: " + jsonreceptor);

                            jsonRespuesta = await consumoFirmador.postReceptor(emisorDTE.ambiente == "00" ? Constantes.urlRecepcionTest : Constantes.urlRecepcion, responseAutenticador.body.token, jsonreceptor); //ENVIAR A SERVICIO DE FIRMADO
                            ResponseReceptor responseReceptor = JsonConvert.DeserializeObject<ResponseReceptor>(jsonRespuesta.ToString());

                            Console.WriteLine(jsonRespuesta);
                            WriteToFile("JSON RESPUESTA FE: " + jsonRespuesta);
                            if (responseReceptor.estado.Equals("PROCESADO"))
                            {
                                Console.WriteLine("Sello recibido: " + responseReceptor.selloRecibido);
                                guardarDatosDTE(responseReceptor, docNum, dtejson.identificacion.numeroControl.ToString(), dtejson.identificacion.tipoModelo, dtejson.identificacion.tipoOperacion, dtejson.identificacion.tipoDte, jsonDTE, jsonReceptor.documento,"N");
                            }
                            else
                            {
                                mySBO_Application.MessageBox("La factura no pudo ser procesada: \n\n" + jsonRespuesta);
                            }
                        }
                        else
                        {
                            mySBO_Application.MessageBox("Error Autenticador: " + JsonConvert.SerializeObject(responseAutenticador));
                        }
                    }
                    else
                    {
                        mySBO_Application.MessageBox("Error Firmador: " + JsonConvert.SerializeObject(response));
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error enviarFE: " + e.Message);
            }
            finally
            {
                mySBO_Company.Disconnect();
            }
        }

        private jsonFirmadorFE obtenerJSONFE(Cliente cliente, int docNum)
        {
            jsonFirmadorFE jsonFirmador = new jsonFirmadorFE();//json a transmitir
            JSONFE dtejson = new JSONFE();
            IdentificacionFE identificacion = new IdentificacionFE();
            Direccion direccion = new Direccion();
            EmisorFE emisor = new EmisorFE();
            ReceptorFE receptor = new ReceptorFE();
            ExtensionFE extension = new ExtensionFE();

            EmisorDTE emisorDTE = new EmisorDTE();

            Guid g = Guid.NewGuid(); //LIBRERIA PARA CODIGO DE GENERACION
            String codigoGeneracion = "";

            ConsumoFirmador consumoFirmador = new ConsumoFirmador();

            jsonReceptor jsonReceptor = new jsonReceptor();

            dynamic jsonRespuesta;

            Response response = new Response();

            try
            {
                emisorDTE = consultas.obtenerInfoEmisor();

                //-------------------------------------------
                jsonFirmador.contentType = "application/JSON";
                jsonFirmador.nit = emisorDTE.userDTE;
                jsonFirmador.activo = "True";
                jsonFirmador.passwordPri = emisorDTE.passDTE;

                //-------------------------------------------
                identificacion.version = cliente.version;
                identificacion.ambiente = emisorDTE.ambiente;
                identificacion.tipoDte = cliente.tipoDoc;


                string numeracion = "000000000000000";

                string docNumber = docNum.ToString();

                string numControl1 = (numeracion + docNumber.Substring(docNumber.Length - 4));

                string numControl = "DTE-" + cliente.tipoDoc + "-" + cliente.sucursal + "01-" + numControl1.Substring(numControl1.Length - 15);

                identificacion.numeroControl = numControl;
                codigoGeneracion = g.ToString().ToUpper();
                identificacion.codigoGeneracion = codigoGeneracion;
                identificacion.tipoModelo = 1;
                identificacion.tipoOperacion = 1;
                identificacion.tipoContingencia = null;
                identificacion.motivoContin = null;
                if (DateTime.Now.ToString("dd") == "01" || DateTime.Now.ToString("dd") == "02" || DateTime.Now.ToString("dd") == "03" || DateTime.Now.ToString("dd") == "04" || DateTime.Now.ToString("dd") == "05")
                    identificacion.fecEmi = cliente.docDate;
                else
                    identificacion.fecEmi = DateTime.Now.ToString("yyyy-MM-dd");
                identificacion.horEmi = DateTime.Now.ToString("HH:mm:ss");
                identificacion.tipoMoneda = "USD";


                dtejson.identificacion = identificacion;

                //-------------------------------------------
                dtejson.documentoRelacionado = null;

                //-------------------------------------------
                direccion.departamento = emisorDTE.departamento;
                direccion.municipio = emisorDTE.municipio;
                direccion.complemento = emisorDTE.direccion;

                emisor.nit = emisorDTE.nit;
                emisor.nrc = emisorDTE.nrc;
                emisor.nombre = emisorDTE.nombre;
                emisor.codActividad = emisorDTE.codActividad;
                emisor.descActividad = emisorDTE.descActividad;
                emisor.nombreComercial = emisorDTE.nombreComercial;
                emisor.tipoEstablecimiento = emisorDTE.tipoEstablecimiento;
                emisor.telefono = emisorDTE.telefono;
                emisor.correo = emisorDTE.correo;
                emisor.codEstableMH = null;
                emisor.codEstable = null;
                emisor.codPuntoVentaMH = null;
                emisor.codPuntoVenta = null;
                emisor.direccion = direccion;

                dtejson.emisor = emisor;

                //--------------------------------------------
                direccion = new Direccion();
                if (cliente.direccion != null && cliente.municipio != null && cliente.departamento != null)
                {
                    direccion.departamento = cliente.departamento;
                    direccion.municipio = cliente.municipio;
                    direccion.complemento = cliente.direccion;
                }

                if (cliente.tipoDocumento != null)
                    receptor.tipoDocumento = cliente.tipoDocumento;

                if (cliente.tipoDocumento != null)
                    receptor.numDocumento = cliente.tipoDocumento == "36" ? cliente.numDocumento.Replace("-", "") : cliente.numDocumento;

                if (cliente.nrc != null)
                {
                    receptor.nrc = cliente.nrc.ToString().Replace("-", "");
                }

                receptor.nombre = cliente.nombre;

                if (cliente.actEconomicaCode != null)
                    receptor.codActividad = cliente.actEconomicaCode;

                if (cliente.actEconomicaName != null)
                    receptor.descActividad = cliente.actEconomicaName;

                if (cliente.telefono != null)
                    receptor.telefono = cliente.telefono;

                if (cliente.correo != null)
                    receptor.correo = cliente.correo;

                if (cliente.direccion != null && cliente.municipio != null && cliente.departamento != null)
                    receptor.direccion = direccion;

                dtejson.receptor = receptor;

                //----------------------------------------------------
                dtejson.otrosDocumentos = null;
                //----------------------------------------------------
                dtejson.ventaTercero = null;
                //----------------------------------------------------
                dtejson.cuerpoDocumento = new List<CuerpoDocumentoFE>();
                dtejson.cuerpoDocumento.AddRange(consultas.obtenerCuerpoDocFE(docNum));

                //----------------------------------------------------

                dtejson.resumen = consultas.obtenerResumenDocFE(docNum);

                //----------------------------------------------------

                extension = null;

                dtejson.extension = extension;

                //----------------------------------------------------

                dtejson.apendice = null;


                jsonFirmador.dteJson = dtejson;

            }
            catch (Exception e)
            {
                MessageBox.Show("Error obtenerJSONFE: " + e.Message);
            } 
            return jsonFirmador;
        }

        private async Task enviarCCF(Cliente cliente,int docNum)
        {
            try
            {
                conectarDI();
                if (mySBO_Company.Connect() != 0)
                {
                    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                }
                else
                {
                    jsonFirmadorCCF jsonFirmador = new jsonFirmadorCCF();//json a transmitir
                    JSONCCF dtejson = new JSONCCF();
                    IdentificacionCCF identificacion = new IdentificacionCCF();
                    Direccion direccionEmisor = new Direccion();
                    Direccion direccionReceptor = new Direccion();
                    EmisorCCF emisor = new EmisorCCF();
                    ReceptorCCF receptor = new ReceptorCCF();
                    ExtensionCCF extension = new ExtensionCCF();

                    List<CuerpoDocumentoCCF> CuerpoDoc = new List<CuerpoDocumentoCCF>();

                    EmisorDTE emisorDTE = new EmisorDTE();

                    Guid g = Guid.NewGuid(); //LIBRERIA PARA CODIGO DE GENERACION
                    String codigoGeneracion = "";


                    ConsumoFirmador consumoFirmador = new ConsumoFirmador();

                    jsonReceptor jsonReceptor = new jsonReceptor();

                    dynamic jsonRespuesta;

                    emisorDTE = consultas.obtenerInfoEmisor();

                    //-------------------------------------------
                    jsonFirmador.contentType = "application/JSON";
                    jsonFirmador.nit = emisorDTE.userDTE;
                    jsonFirmador.activo = "True";
                    jsonFirmador.passwordPri = emisorDTE.passDTE;

                    //-------------------------------------------
                    identificacion.version = cliente.version;
                    identificacion.ambiente = emisorDTE.ambiente;
                    identificacion.tipoDte = cliente.tipoDoc;

                    string numeracion = "000000000000000";

                    string docNumber = docNum.ToString();

                    string numControl1 = (numeracion + docNumber.Substring(docNumber.Length - 4));

                    string numControl = "DTE-" + cliente.tipoDoc + "-" + cliente.sucursal + "01-" + numControl1.Substring(numControl1.Length - 15);

                    identificacion.numeroControl = numControl;
                    codigoGeneracion = g.ToString().ToUpper();
                    identificacion.codigoGeneracion = codigoGeneracion;
                    identificacion.tipoModelo = 1;
                    identificacion.tipoOperacion = 1;
                    identificacion.tipoContingencia = null;
                    identificacion.motivoContin = null;
                    if (DateTime.Now.ToString("dd") == "01" || DateTime.Now.ToString("dd") == "02" || DateTime.Now.ToString("dd") == "03" || DateTime.Now.ToString("dd") == "04" || DateTime.Now.ToString("dd") == "05")
                        identificacion.fecEmi = cliente.docDate;
                    else
                        identificacion.fecEmi = DateTime.Now.ToString("yyyy-MM-dd");
                    identificacion.horEmi = DateTime.Now.ToString("HH:mm:ss");
                    identificacion.tipoMoneda = "USD";


                    dtejson.identificacion = identificacion;

                    //-------------------------------------------
                    dtejson.documentoRelacionado = null;

                    //-------------------------------------------
                    direccionEmisor.departamento = emisorDTE.departamento;
                    direccionEmisor.municipio = emisorDTE.municipio;
                    direccionEmisor.complemento = emisorDTE.direccion;

                    emisor.nit = emisorDTE.nit;
                    emisor.nrc = emisorDTE.nrc;
                    emisor.nombre = emisorDTE.nombre;
                    emisor.codActividad = emisorDTE.codActividad;
                    emisor.descActividad = emisorDTE.descActividad;
                    emisor.nombreComercial = emisorDTE.nombreComercial;
                    emisor.tipoEstablecimiento = emisorDTE.tipoEstablecimiento;
                    emisor.telefono = emisorDTE.telefono;
                    emisor.correo = emisorDTE.correo;
                    emisor.codEstableMH = null;
                    emisor.codEstable = null;
                    emisor.codPuntoVentaMH = null;
                    emisor.codPuntoVenta = null;
                    emisor.direccion = direccionEmisor;

                    dtejson.emisor = emisor;

                    //--------------------------------------------
                    direccionReceptor.departamento = cliente.departamento;
                    direccionReceptor.municipio = cliente.municipio;
                    direccionReceptor.complemento = cliente.direccion;

                    receptor.nit = cliente.numDocumento.Replace("-", "");

                    if (cliente.nrc != null)
                    {
                        receptor.nrc = cliente.nrc.ToString().Replace("-", "");
                    }

                    if (cliente.nombreComercial != null)
                    {
                        receptor.nombreComercial = cliente.nombreComercial;
                    }

                    receptor.nombre = cliente.nombre;

                    receptor.codActividad = cliente.actEconomicaCode;

                    receptor.descActividad = cliente.actEconomicaName;

                    receptor.telefono = cliente.telefono;

                    receptor.correo = cliente.correo;

                    receptor.direccion = direccionReceptor;

                    dtejson.receptor = receptor;

                    //----------------------------------------------------
                    dtejson.otrosDocumentos = null;
                    //----------------------------------------------------
                    dtejson.ventaTercero = null;
                    //----------------------------------------------------
                    CuerpoDoc.AddRange(consultas.obtenerCuerpoDocCCF(docNum, codigoGeneracion));

                    dtejson.cuerpoDocumento = new List<CuerpoDocumentoCCF>();
                    dtejson.cuerpoDocumento.AddRange(CuerpoDoc);
                    //----------------------------------------------------

                    dtejson.resumen = consultas.obtenerResumenDocCCF(docNum);

                    //----------------------------------------------------

                    extension = null;

                    dtejson.extension = extension;

                    //----------------------------------------------------

                    dtejson.apendice = null;


                    jsonFirmador.dteJson = dtejson;

                    string jsonDTE = JsonConvert.SerializeObject(dtejson);//CONVERTIR OBJETO A JSON

                    string json = JsonConvert.SerializeObject(jsonFirmador);//CONVERTIR OBJETO A JSON

                    Console.WriteLine(json);

                    WriteToFile("JSON PLANO CCF: " + json);

                    jsonRespuesta = await consumoFirmador.postFirmador("http://" + serverDB + ":8113/firmardocumento/", json); //ENVIAR A SERVICIO DE FIRMADO

                    Response response = JsonConvert.DeserializeObject<Response>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO

                    if (response.status == "OK")
                    {

                        jsonRespuesta = await consumoFirmador.postAutenticador(emisorDTE.ambiente == "00" ? Constantes.urlAutenticadorTest : Constantes.urlAutenticador, emisorDTE.nit, emisorDTE.passPrivDTE);
                        ResponseAutenticador responseAutenticador = JsonConvert.DeserializeObject<ResponseAutenticador>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO
                        if (responseAutenticador.status == "OK")
                        {
                            jsonReceptor.ambiente = emisorDTE.ambiente;
                            jsonReceptor.idEnvio = docNum;
                            jsonReceptor.version = cliente.version;
                            jsonReceptor.tipoDte = cliente.tipoDoc;
                            jsonReceptor.documento = response.body.ToString();
                            jsonReceptor.codigoGeneracion = codigoGeneracion;
                            string jsonreceptor = JsonConvert.SerializeObject(jsonReceptor);//CONVERTIR OBJETO A JSON
                            Console.WriteLine(jsonreceptor);
                            WriteToFile("JSON FIRMADO CCF: " + jsonreceptor);
                            jsonRespuesta = await consumoFirmador.postReceptor(emisorDTE.ambiente == "00" ? Constantes.urlRecepcionTest : Constantes.urlRecepcion, responseAutenticador.body.token, jsonreceptor); //ENVIAR A SERVICIO DE FIRMADO
                            ResponseReceptor responseReceptor = JsonConvert.DeserializeObject<ResponseReceptor>(jsonRespuesta.ToString());

                            Console.WriteLine(jsonRespuesta);

                            if (responseReceptor.estado.Equals("PROCESADO"))
                            {
                                Console.WriteLine("Sello recibido: " + responseReceptor.selloRecibido);
                                guardarDatosDTE(responseReceptor, docNum, dtejson.identificacion.numeroControl.ToString(), dtejson.identificacion.tipoModelo, dtejson.identificacion.tipoOperacion, dtejson.identificacion.tipoDte, jsonDTE, jsonReceptor.documento, "N");
                            }
                            else
                            {
                                mySBO_Application.MessageBox("La factura no pudo ser procesada: \n\n" + jsonRespuesta);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error enviarCCF: " + e.Message);
            }
            finally
            {
                mySBO_Company.Disconnect();
            }
        }

        private jsonFirmadorCCF obtenerJSONCCF(Cliente cliente, int docNum)
        {
            jsonFirmadorCCF jsonFirmador = new jsonFirmadorCCF();//json a transmitir
            JSONCCF dtejson = new JSONCCF();
            IdentificacionCCF identificacion = new IdentificacionCCF();
            Direccion direccionEmisor = new Direccion();
            Direccion direccionReceptor = new Direccion();
            EmisorCCF emisor = new EmisorCCF();
            ReceptorCCF receptor = new ReceptorCCF();
            ExtensionCCF extension = new ExtensionCCF();

            List<CuerpoDocumentoCCF> CuerpoDoc = new List<CuerpoDocumentoCCF>();

            EmisorDTE emisorDTE = new EmisorDTE();

            Guid g = Guid.NewGuid(); //LIBRERIA PARA CODIGO DE GENERACION
            String codigoGeneracion = "";


            ConsumoFirmador consumoFirmador = new ConsumoFirmador();

            jsonReceptor jsonReceptor = new jsonReceptor();

            dynamic jsonRespuesta;

            Response response = new Response();
            try
            {

                emisorDTE = consultas.obtenerInfoEmisor();

                //-------------------------------------------
                jsonFirmador.contentType = "application/JSON";
                jsonFirmador.nit = emisorDTE.userDTE;
                jsonFirmador.activo = "True";
                jsonFirmador.passwordPri = emisorDTE.passDTE;

                //-------------------------------------------
                identificacion.version = cliente.version;
                identificacion.ambiente = emisorDTE.ambiente;
                identificacion.tipoDte = cliente.tipoDoc;

                string numeracion = "000000000000000";

                string docNumber = docNum.ToString();

                string numControl1 = (numeracion + docNumber.Substring(docNumber.Length - 4));

                string numControl = "DTE-" + cliente.tipoDoc + "-" + cliente.sucursal + "01-" + numControl1.Substring(numControl1.Length - 15);

                identificacion.numeroControl = numControl;
                codigoGeneracion = g.ToString().ToUpper();
                identificacion.codigoGeneracion = codigoGeneracion;
                identificacion.tipoModelo = 1;
                identificacion.tipoOperacion = 1;
                identificacion.tipoContingencia = null;
                identificacion.motivoContin = null;
                if (DateTime.Now.ToString("dd") == "01" || DateTime.Now.ToString("dd") == "02" || DateTime.Now.ToString("dd") == "03" || DateTime.Now.ToString("dd") == "04" || DateTime.Now.ToString("dd") == "05")
                    identificacion.fecEmi = cliente.docDate;
                else
                    identificacion.fecEmi = DateTime.Now.ToString("yyyy-MM-dd");
                identificacion.horEmi = DateTime.Now.ToString("HH:mm:ss");
                identificacion.tipoMoneda = "USD";


                dtejson.identificacion = identificacion;

                //-------------------------------------------
                dtejson.documentoRelacionado = null;

                //-------------------------------------------
                direccionEmisor.departamento = emisorDTE.departamento;
                direccionEmisor.municipio = emisorDTE.municipio;
                direccionEmisor.complemento = emisorDTE.direccion;

                emisor.nit = emisorDTE.nit;
                emisor.nrc = emisorDTE.nrc;
                emisor.nombre = emisorDTE.nombre;
                emisor.codActividad = emisorDTE.codActividad;
                emisor.descActividad = emisorDTE.descActividad;
                emisor.nombreComercial = emisorDTE.nombreComercial;
                emisor.tipoEstablecimiento = emisorDTE.tipoEstablecimiento;
                emisor.telefono = emisorDTE.telefono;
                emisor.correo = emisorDTE.correo;
                emisor.codEstableMH = null;
                emisor.codEstable = null;
                emisor.codPuntoVentaMH = null;
                emisor.codPuntoVenta = null;
                emisor.direccion = direccionEmisor;

                dtejson.emisor = emisor;

                //--------------------------------------------
                direccionReceptor.departamento = cliente.departamento;
                direccionReceptor.municipio = cliente.municipio;
                direccionReceptor.complemento = cliente.direccion;

                receptor.nit = cliente.numDocumento.Replace("-", "");

                if (cliente.nrc != null)
                {
                    receptor.nrc = cliente.nrc.ToString().Replace("-", "");
                }

                if (cliente.nombreComercial != null)
                {
                    receptor.nombreComercial = cliente.nombreComercial;
                }

                receptor.nombre = cliente.nombre;

                receptor.codActividad = cliente.actEconomicaCode;

                receptor.descActividad = cliente.actEconomicaName;

                receptor.telefono = cliente.telefono;

                receptor.correo = cliente.correo;

                receptor.direccion = direccionReceptor;

                dtejson.receptor = receptor;

                //----------------------------------------------------
                dtejson.otrosDocumentos = null;
                //----------------------------------------------------
                dtejson.ventaTercero = null;
                //----------------------------------------------------
                CuerpoDoc.AddRange(consultas.obtenerCuerpoDocCCF(docNum, codigoGeneracion));

                dtejson.cuerpoDocumento = new List<CuerpoDocumentoCCF>();
                dtejson.cuerpoDocumento.AddRange(CuerpoDoc);
                //----------------------------------------------------

                dtejson.resumen = consultas.obtenerResumenDocCCF(docNum);

                //----------------------------------------------------

                extension = null;

                dtejson.extension = extension;

                //----------------------------------------------------

                dtejson.apendice = null;


                jsonFirmador.dteJson = dtejson;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error obtenerJSONCCF: " + e.Message);
            }
            return jsonFirmador;
        }

        private async Task enviarND(Cliente cliente, int docNum)
        {
            try
            {
                conectarDI();
                if (mySBO_Company.Connect() != 0)
                {
                    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                }
                else
                {
                    jsonFirmadorND jsonFirmador = new jsonFirmadorND();//json a transmitir
                    JSONND dtejson = new JSONND();
                    IdentificacionND identificacion = new IdentificacionND();
                    Direccion direccionEmisor = new Direccion();
                    Direccion direccionReceptor = new Direccion();
                    EmisorND emisor = new EmisorND();
                    ReceptorND receptor = new ReceptorND();
                    ExtensionND extension = new ExtensionND();

                    List<CuerpoDocumentoND> CuerpoDoc = new List<CuerpoDocumentoND>();

                    EmisorDTE emisorDTE = new EmisorDTE();

                    Guid g = Guid.NewGuid(); //LIBRERIA PARA CODIGO DE GENERACION
                    String codigoGeneracion = "";


                    ConsumoFirmador consumoFirmador = new ConsumoFirmador();

                    jsonReceptor jsonReceptor = new jsonReceptor();

                    dynamic jsonRespuesta;

                    emisorDTE = consultas.obtenerInfoEmisor();

                    //-------------------------------------------
                    jsonFirmador.contentType = "application/JSON";
                    jsonFirmador.nit = emisorDTE.userDTE;
                    jsonFirmador.activo = "True";
                    jsonFirmador.passwordPri = emisorDTE.passDTE;

                    //-------------------------------------------
                    identificacion.version = cliente.version;
                    identificacion.ambiente = emisorDTE.ambiente;
                    identificacion.tipoDte = cliente.tipoDoc;

                    string numeracion = "000000000000000";

                    string docNumber = docNum.ToString();

                    string numControl1 = (numeracion + docNumber.Substring(docNumber.Length - 4));

                    string numControl = "DTE-" + cliente.tipoDoc + "-" + cliente.sucursal + "01-" + numControl1.Substring(numControl1.Length - 15);

                    identificacion.numeroControl = numControl;
                    codigoGeneracion = g.ToString().ToUpper();
                    identificacion.codigoGeneracion = codigoGeneracion;
                    identificacion.tipoModelo = 1;
                    identificacion.tipoOperacion = 1;
                    identificacion.tipoContingencia = null;
                    identificacion.motivoContin = null;
                    if (DateTime.Now.ToString("dd") == "01" || DateTime.Now.ToString("dd") == "02" || DateTime.Now.ToString("dd") == "03" || DateTime.Now.ToString("dd") == "04" || DateTime.Now.ToString("dd") == "05")
                        identificacion.fecEmi = cliente.docDate;
                    else
                        identificacion.fecEmi = DateTime.Now.ToString("yyyy-MM-dd");
                    identificacion.horEmi = DateTime.Now.ToString("HH:mm:ss");
                    identificacion.tipoMoneda = "USD";


                    dtejson.identificacion = identificacion;

                    //-------------------------------------------
                    dtejson.documentoRelacionado = new List<DocumentoRelacionadoND>();
                    dtejson.documentoRelacionado.AddRange(consultas.obtenerDocumentoRelacionadoND(docNum));
                    if (dtejson.documentoRelacionado[0].numeroDocumento == null)
                    {
                        dtejson.documentoRelacionado = null;
                    }

                    //-------------------------------------------
                    direccionEmisor.departamento = emisorDTE.departamento;
                    direccionEmisor.municipio = emisorDTE.municipio;
                    direccionEmisor.complemento = emisorDTE.direccion;

                    emisor.nit = emisorDTE.nit;
                    emisor.nrc = emisorDTE.nrc;
                    emisor.nombre = emisorDTE.nombre;
                    emisor.codActividad = emisorDTE.codActividad;
                    emisor.descActividad = emisorDTE.descActividad;
                    emisor.nombreComercial = emisorDTE.nombreComercial;
                    emisor.tipoEstablecimiento = emisorDTE.tipoEstablecimiento;
                    emisor.telefono = emisorDTE.telefono;
                    emisor.correo = emisorDTE.correo;
                    emisor.direccion = direccionEmisor;

                    dtejson.emisor = emisor;

                    //--------------------------------------------
                    direccionReceptor.departamento = cliente.departamento;
                    direccionReceptor.municipio = cliente.municipio;
                    direccionReceptor.complemento = cliente.direccion;

                    receptor.nit = cliente.numDocumento.Replace("-", "");

                    if (cliente.nrc != null)
                    {
                        receptor.nrc = cliente.nrc.ToString().Replace("-", "");
                    }

                    if (cliente.nombreComercial != null)
                    {
                        receptor.nombreComercial = cliente.nombreComercial;
                    }

                    receptor.nombre = cliente.nombre;

                    receptor.codActividad = cliente.actEconomicaCode;

                    receptor.descActividad = cliente.actEconomicaName;

                    receptor.telefono = cliente.telefono;

                    receptor.correo = cliente.correo;

                    receptor.direccion = direccionReceptor;

                    dtejson.receptor = receptor;

                    //----------------------------------------------------
                    dtejson.ventaTercero = null;
                    //----------------------------------------------------
                    CuerpoDoc.AddRange(consultas.obtenerCuerpoDocND(docNum, codigoGeneracion));

                    dtejson.cuerpoDocumento = new List<CuerpoDocumentoND>();
                    dtejson.cuerpoDocumento.AddRange(CuerpoDoc);
                    //----------------------------------------------------

                    dtejson.resumen = consultas.obtenerResumenDocND(docNum);

                    //----------------------------------------------------

                    extension = null;

                    dtejson.extension = extension;

                    //----------------------------------------------------

                    dtejson.apendice = null;


                    jsonFirmador.dteJson = dtejson;

                    string jsonDTE = JsonConvert.SerializeObject(dtejson);//CONVERTIR OBJETO A JSON

                    string json = JsonConvert.SerializeObject(jsonFirmador);//CONVERTIR OBJETO A JSON

                    Console.WriteLine(json);
                    WriteToFile("JSON PLANO ND: " + json);

                    jsonRespuesta = await consumoFirmador.postFirmador("http://" + serverDB + ":8113/firmardocumento/", json); //ENVIAR A SERVICIO DE FIRMADO

                    Response response = JsonConvert.DeserializeObject<Response>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO

                    if (response.status == "OK")
                    {

                        jsonRespuesta = await consumoFirmador.postAutenticador(emisorDTE.ambiente == "00" ? Constantes.urlAutenticadorTest : Constantes.urlAutenticador, emisorDTE.nit, emisorDTE.passPrivDTE);
                        ResponseAutenticador responseAutenticador = JsonConvert.DeserializeObject<ResponseAutenticador>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO
                        if (responseAutenticador.status == "OK")
                        {
                            jsonReceptor.ambiente = emisorDTE.ambiente;
                            jsonReceptor.idEnvio = docNum;
                            jsonReceptor.version = cliente.version;
                            jsonReceptor.tipoDte = cliente.tipoDoc;
                            jsonReceptor.documento = response.body.ToString();
                            jsonReceptor.codigoGeneracion = codigoGeneracion;
                            string jsonreceptor = JsonConvert.SerializeObject(jsonReceptor);//CONVERTIR OBJETO A JSON
                            Console.WriteLine(jsonreceptor);
                            WriteToFile("JSON FIRMADO ND: " + jsonreceptor);
                            jsonRespuesta = await consumoFirmador.postReceptor(emisorDTE.ambiente == "00" ? Constantes.urlRecepcionTest : Constantes.urlRecepcion, responseAutenticador.body.token, jsonreceptor); //ENVIAR A SERVICIO DE FIRMADO
                            ResponseReceptor responseReceptor = JsonConvert.DeserializeObject<ResponseReceptor>(jsonRespuesta.ToString());

                            Console.WriteLine(jsonRespuesta);
                            WriteToFile("JSON RESPUESTA ND: " + jsonRespuesta);

                            if (responseReceptor.estado.Equals("PROCESADO"))
                            {
                                Console.WriteLine("Sello recibido: " + responseReceptor.selloRecibido);
                                guardarDatosDTE(responseReceptor, docNum, dtejson.identificacion.numeroControl.ToString(), dtejson.identificacion.tipoModelo, dtejson.identificacion.tipoOperacion, dtejson.identificacion.tipoDte, jsonDTE, jsonReceptor.documento, "N");
                            }
                            else
                            {
                                mySBO_Application.MessageBox("La factura no pudo ser procesada: \n\n" + jsonRespuesta);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error enviarND: " + e.Message);
            }
            finally
            {
                mySBO_Company.Disconnect();
            }
        }

        private jsonFirmadorND obtenerJSONND(Cliente cliente, int docNum)
        {
            jsonFirmadorND jsonFirmador = new jsonFirmadorND();//json a transmitir
            JSONND dtejson = new JSONND();
            IdentificacionND identificacion = new IdentificacionND();
            Direccion direccionEmisor = new Direccion();
            Direccion direccionReceptor = new Direccion();
            EmisorND emisor = new EmisorND();
            ReceptorND receptor = new ReceptorND();
            ExtensionND extension = new ExtensionND();

            List<CuerpoDocumentoND> CuerpoDoc = new List<CuerpoDocumentoND>();

            EmisorDTE emisorDTE = new EmisorDTE();

            Guid g = Guid.NewGuid(); //LIBRERIA PARA CODIGO DE GENERACION
            String codigoGeneracion = "";


            ConsumoFirmador consumoFirmador = new ConsumoFirmador();

            jsonReceptor jsonReceptor = new jsonReceptor();

            dynamic jsonRespuesta;

            Response response = new Response();

            try
            {
                emisorDTE = consultas.obtenerInfoEmisor();

                //-------------------------------------------
                jsonFirmador.contentType = "application/JSON";
                jsonFirmador.nit = emisorDTE.userDTE;
                jsonFirmador.activo = "True";
                jsonFirmador.passwordPri = emisorDTE.passDTE;

                //-------------------------------------------
                identificacion.version = cliente.version;
                identificacion.ambiente = emisorDTE.ambiente;
                identificacion.tipoDte = cliente.tipoDoc;

                string numeracion = "000000000000000";

                string docNumber = docNum.ToString();

                string numControl1 = (numeracion + docNumber.Substring(docNumber.Length - 4));

                string numControl = "DTE-" + cliente.tipoDoc + "-" + cliente.sucursal + "01-" + numControl1.Substring(numControl1.Length - 15);

                identificacion.numeroControl = numControl;
                codigoGeneracion = g.ToString().ToUpper();
                identificacion.codigoGeneracion = codigoGeneracion;
                identificacion.tipoModelo = 1;
                identificacion.tipoOperacion = 1;
                identificacion.tipoContingencia = null;
                identificacion.motivoContin = null;
                if (DateTime.Now.ToString("dd") == "01" || DateTime.Now.ToString("dd") == "02" || DateTime.Now.ToString("dd") == "03" || DateTime.Now.ToString("dd") == "04" || DateTime.Now.ToString("dd") == "05")
                    identificacion.fecEmi = cliente.docDate;
                else
                    identificacion.fecEmi = DateTime.Now.ToString("yyyy-MM-dd");
                identificacion.horEmi = DateTime.Now.ToString("HH:mm:ss");
                identificacion.tipoMoneda = "USD";


                dtejson.identificacion = identificacion;

                //-------------------------------------------
                dtejson.documentoRelacionado = new List<DocumentoRelacionadoND>();
                dtejson.documentoRelacionado.AddRange(consultas.obtenerDocumentoRelacionadoND(docNum));
                if (dtejson.documentoRelacionado[0].numeroDocumento == null)
                {
                    dtejson.documentoRelacionado = null;
                }

                //-------------------------------------------
                direccionEmisor.departamento = emisorDTE.departamento;
                direccionEmisor.municipio = emisorDTE.municipio;
                direccionEmisor.complemento = emisorDTE.direccion;

                emisor.nit = emisorDTE.nit;
                emisor.nrc = emisorDTE.nrc;
                emisor.nombre = emisorDTE.nombre;
                emisor.codActividad = emisorDTE.codActividad;
                emisor.descActividad = emisorDTE.descActividad;
                emisor.nombreComercial = emisorDTE.nombreComercial;
                emisor.tipoEstablecimiento = emisorDTE.tipoEstablecimiento;
                emisor.telefono = emisorDTE.telefono;
                emisor.correo = emisorDTE.correo;
                emisor.direccion = direccionEmisor;

                dtejson.emisor = emisor;

                //--------------------------------------------
                direccionReceptor.departamento = cliente.departamento;
                direccionReceptor.municipio = cliente.municipio;
                direccionReceptor.complemento = cliente.direccion;

                receptor.nit = cliente.numDocumento.Replace("-", "");

                if (cliente.nrc != null)
                {
                    receptor.nrc = cliente.nrc.ToString().Replace("-", "");
                }

                if (cliente.nombreComercial != null)
                {
                    receptor.nombreComercial = cliente.nombreComercial;
                }

                receptor.nombre = cliente.nombre;

                receptor.codActividad = cliente.actEconomicaCode;

                receptor.descActividad = cliente.actEconomicaName;

                receptor.telefono = cliente.telefono;

                receptor.correo = cliente.correo;

                receptor.direccion = direccionReceptor;

                dtejson.receptor = receptor;

                //----------------------------------------------------
                dtejson.ventaTercero = null;
                //----------------------------------------------------
                CuerpoDoc.AddRange(consultas.obtenerCuerpoDocND(docNum, codigoGeneracion));

                dtejson.cuerpoDocumento = new List<CuerpoDocumentoND>();
                dtejson.cuerpoDocumento.AddRange(CuerpoDoc);
                //----------------------------------------------------

                dtejson.resumen = consultas.obtenerResumenDocND(docNum);

                //----------------------------------------------------

                extension = null;

                dtejson.extension = extension;

                //----------------------------------------------------

                dtejson.apendice = null;


                jsonFirmador.dteJson = dtejson;

            }
            catch (Exception e)
            {
                MessageBox.Show("Error obtenerJSONND: " + e.Message);
            }

            return jsonFirmador;
        }

        private async Task enviarNC(Cliente cliente, int docNum)
        {
            try
            {
                conectarDI();
                if (mySBO_Company.Connect() != 0)
                {
                    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                }
                else
                {
                    jsonFirmadorNC jsonFirmador = new jsonFirmadorNC();//json a transmitir
                    JSONNC dtejson = new JSONNC();
                    IdentificacionNC identificacion = new IdentificacionNC();
                    Direccion direccionEmisor = new Direccion();
                    Direccion direccionReceptor = new Direccion();
                    EmisorNC emisor = new EmisorNC();
                    ReceptorNC receptor = new ReceptorNC();
                    ExtensionNC extension = new ExtensionNC();

                    List<CuerpoDocumentoNC> CuerpoDoc = new List<CuerpoDocumentoNC>();

                    EmisorDTE emisorDTE = new EmisorDTE();

                    Guid g = Guid.NewGuid(); //LIBRERIA PARA CODIGO DE GENERACION
                    String codigoGeneracion = "";


                    ConsumoFirmador consumoFirmador = new ConsumoFirmador();

                    jsonReceptor jsonReceptor = new jsonReceptor();

                    dynamic jsonRespuesta;

                    emisorDTE = consultas.obtenerInfoEmisor();

                    //-------------------------------------------
                    jsonFirmador.contentType = "application/JSON";
                    jsonFirmador.nit = emisorDTE.userDTE;
                    jsonFirmador.activo = "True";
                    jsonFirmador.passwordPri = emisorDTE.passDTE;

                    //-------------------------------------------
                    identificacion.version = cliente.version;
                    identificacion.ambiente = emisorDTE.ambiente;
                    identificacion.tipoDte = cliente.tipoDoc;

                    string numeracion = "000000000000000";

                    string docNumber = docNum.ToString();

                    string numControl1 = (numeracion + docNumber.Substring(docNumber.Length - 4));

                    string numControl = "DTE-" + cliente.tipoDoc + "-" + cliente.sucursal + "01-" + numControl1.Substring(numControl1.Length - 15);

                    identificacion.numeroControl = numControl;
                    codigoGeneracion = g.ToString().ToUpper();
                    identificacion.codigoGeneracion = codigoGeneracion;
                    identificacion.tipoModelo = 1;
                    identificacion.tipoOperacion = 1;
                    identificacion.tipoContingencia = null;
                    identificacion.motivoContin = null;
                    if (DateTime.Now.ToString("dd") == "01" || DateTime.Now.ToString("dd") == "02" || DateTime.Now.ToString("dd") == "03" || DateTime.Now.ToString("dd") == "04" || DateTime.Now.ToString("dd") == "05")
                        identificacion.fecEmi = cliente.docDate;
                    else
                        identificacion.fecEmi = DateTime.Now.ToString("yyyy-MM-dd");
                    identificacion.horEmi = DateTime.Now.ToString("HH:mm:ss");
                    identificacion.tipoMoneda = "USD";


                    dtejson.identificacion = identificacion;

                    //-------------------------------------------
                    dtejson.documentoRelacionado = new List<DocumentoRelacionadoNC>();
                    dtejson.documentoRelacionado.AddRange(consultas.obtenerDocumentoRelacionadoNC(docNum));
                    if (dtejson.documentoRelacionado[0].numeroDocumento == null)
                    {
                        dtejson.documentoRelacionado = null;
                    }

                    //-------------------------------------------
                    direccionEmisor.departamento = emisorDTE.departamento;
                    direccionEmisor.municipio = emisorDTE.municipio;
                    direccionEmisor.complemento = emisorDTE.direccion;

                    emisor.nit = emisorDTE.nit;
                    emisor.nrc = emisorDTE.nrc;
                    emisor.nombre = emisorDTE.nombre;
                    emisor.codActividad = emisorDTE.codActividad;
                    emisor.descActividad = emisorDTE.descActividad;
                    emisor.nombreComercial = emisorDTE.nombreComercial;
                    emisor.tipoEstablecimiento = emisorDTE.tipoEstablecimiento;
                    emisor.telefono = emisorDTE.telefono;
                    emisor.correo = emisorDTE.correo;
                    emisor.direccion = direccionEmisor;

                    dtejson.emisor = emisor;

                    //--------------------------------------------
                    direccionReceptor.departamento = cliente.departamento;
                    direccionReceptor.municipio = cliente.municipio;
                    direccionReceptor.complemento = cliente.direccion;

                    receptor.nit = cliente.numDocumento.Replace("-", "");

                    if (cliente.nrc != null)
                    {
                        receptor.nrc = cliente.nrc.ToString().Replace("-", "");
                    }

                    if (cliente.nombreComercial != null)
                    {
                        receptor.nombreComercial = cliente.nombreComercial;
                    }

                    receptor.nombre = cliente.nombre;

                    receptor.codActividad = cliente.actEconomicaCode;

                    receptor.descActividad = cliente.actEconomicaName;

                    receptor.telefono = cliente.telefono;

                    receptor.correo = cliente.correo;

                    receptor.direccion = direccionReceptor;

                    dtejson.receptor = receptor;

                    //----------------------------------------------------
                    dtejson.ventaTercero = null;
                    //----------------------------------------------------
                    CuerpoDoc.AddRange(consultas.obtenerCuerpoDocNC(docNum, codigoGeneracion));

                    dtejson.cuerpoDocumento = new List<CuerpoDocumentoNC>();
                    dtejson.cuerpoDocumento.AddRange(CuerpoDoc);
                    //----------------------------------------------------

                    dtejson.resumen = consultas.obtenerResumenDocNC(docNum);

                    //----------------------------------------------------

                    extension = null;

                    dtejson.extension = extension;

                    //----------------------------------------------------

                    dtejson.apendice = null;


                    jsonFirmador.dteJson = dtejson;

                    string jsonDTE = JsonConvert.SerializeObject(dtejson);//CONVERTIR OBJETO A JSON

                    string json = JsonConvert.SerializeObject(jsonFirmador);//CONVERTIR OBJETO A JSON

                    Console.WriteLine(json);
                    WriteToFile("JSON PLANO NC: " + json);

                    jsonRespuesta = await consumoFirmador.postFirmador("http://" + serverDB + ":8113/firmardocumento/", json); //ENVIAR A SERVICIO DE FIRMADO

                    Response response = JsonConvert.DeserializeObject<Response>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO

                    if (response.status == "OK")
                    {

                        jsonRespuesta = await consumoFirmador.postAutenticador(emisorDTE.ambiente == "00" ? Constantes.urlAutenticadorTest : Constantes.urlAutenticador, emisorDTE.nit, emisorDTE.passPrivDTE);
                        ResponseAutenticador responseAutenticador = JsonConvert.DeserializeObject<ResponseAutenticador>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO
                        if (responseAutenticador.status == "OK")
                        {
                            jsonReceptor.ambiente = emisorDTE.ambiente;
                            jsonReceptor.idEnvio = docNum;
                            jsonReceptor.version = cliente.version;
                            jsonReceptor.tipoDte = cliente.tipoDoc;
                            jsonReceptor.documento = response.body.ToString();
                            jsonReceptor.codigoGeneracion = codigoGeneracion;
                            string jsonreceptor = JsonConvert.SerializeObject(jsonReceptor);//CONVERTIR OBJETO A JSON
                            Console.WriteLine(jsonreceptor);
                            WriteToFile("JSON FIRMADO NC: " + jsonreceptor);
                            jsonRespuesta = await consumoFirmador.postReceptor(emisorDTE.ambiente == "00" ? Constantes.urlRecepcionTest : Constantes.urlRecepcion, responseAutenticador.body.token, jsonreceptor); //ENVIAR A SERVICIO DE FIRMADO
                            ResponseReceptor responseReceptor = JsonConvert.DeserializeObject<ResponseReceptor>(jsonRespuesta.ToString());

                            Console.WriteLine(jsonRespuesta);

                            WriteToFile("JSON RESPUESTA NC: " + jsonRespuesta);
                            if (responseReceptor.estado.Equals("PROCESADO"))
                            {
                                Console.WriteLine("Sello recibido: " + responseReceptor.selloRecibido);

                                guardarDatosDTE(responseReceptor, docNum, dtejson.identificacion.numeroControl.ToString(), dtejson.identificacion.tipoModelo, dtejson.identificacion.tipoOperacion, dtejson.identificacion.tipoDte, jsonDTE, jsonReceptor.documento, "N");
                            }
                            else
                            {
                                mySBO_Application.MessageBox("La factura no pudo ser procesada: \n\n" + jsonRespuesta);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error enviarNC: " + e.Message);
            }
            finally
            {
                mySBO_Company.Disconnect();
            }
        }

        private jsonFirmadorNC obtenerJSONNC(Cliente cliente, int docNum)
        {
            jsonFirmadorNC jsonFirmador = new jsonFirmadorNC();//json a transmitir
            JSONNC dtejson = new JSONNC();
            IdentificacionNC identificacion = new IdentificacionNC();
            Direccion direccionEmisor = new Direccion();
            Direccion direccionReceptor = new Direccion();
            EmisorNC emisor = new EmisorNC();
            ReceptorNC receptor = new ReceptorNC();
            ExtensionNC extension = new ExtensionNC();

            List<CuerpoDocumentoNC> CuerpoDoc = new List<CuerpoDocumentoNC>();

            EmisorDTE emisorDTE = new EmisorDTE();

            Guid g = Guid.NewGuid(); //LIBRERIA PARA CODIGO DE GENERACION
            String codigoGeneracion = "";


            ConsumoFirmador consumoFirmador = new ConsumoFirmador();

            jsonReceptor jsonReceptor = new jsonReceptor();

            dynamic jsonRespuesta;

            Response response = new Response();

            try
            {
                emisorDTE = consultas.obtenerInfoEmisor();

                //-------------------------------------------
                jsonFirmador.contentType = "application/JSON";
                jsonFirmador.nit = emisorDTE.userDTE;
                jsonFirmador.activo = "True";
                jsonFirmador.passwordPri = emisorDTE.passDTE;

                //-------------------------------------------
                identificacion.version = cliente.version;
                identificacion.ambiente = emisorDTE.ambiente;
                identificacion.tipoDte = cliente.tipoDoc;

                string numeracion = "000000000000000";

                string docNumber = docNum.ToString();

                string numControl1 = (numeracion + docNumber.Substring(docNumber.Length - 4));

                string numControl = "DTE-" + cliente.tipoDoc + "-" + cliente.sucursal + "01-" + numControl1.Substring(numControl1.Length - 15);

                identificacion.numeroControl = numControl;
                codigoGeneracion = g.ToString().ToUpper();
                identificacion.codigoGeneracion = codigoGeneracion;
                identificacion.tipoModelo = 1;
                identificacion.tipoOperacion = 1;
                identificacion.tipoContingencia = null;
                identificacion.motivoContin = null;
                if (DateTime.Now.ToString("dd") == "01" || DateTime.Now.ToString("dd") == "02" || DateTime.Now.ToString("dd") == "03" || DateTime.Now.ToString("dd") == "04" || DateTime.Now.ToString("dd") == "05")
                    identificacion.fecEmi = cliente.docDate;
                else
                    identificacion.fecEmi = DateTime.Now.ToString("yyyy-MM-dd");
                identificacion.horEmi = DateTime.Now.ToString("HH:mm:ss");
                identificacion.tipoMoneda = "USD";


                dtejson.identificacion = identificacion;

                //-------------------------------------------
                dtejson.documentoRelacionado = new List<DocumentoRelacionadoNC>();
                dtejson.documentoRelacionado.AddRange(consultas.obtenerDocumentoRelacionadoNC(docNum));
                if (dtejson.documentoRelacionado[0].numeroDocumento == null)
                {
                    dtejson.documentoRelacionado = null;
                }

                //-------------------------------------------
                direccionEmisor.departamento = emisorDTE.departamento;
                direccionEmisor.municipio = emisorDTE.municipio;
                direccionEmisor.complemento = emisorDTE.direccion;

                emisor.nit = emisorDTE.nit;
                emisor.nrc = emisorDTE.nrc;
                emisor.nombre = emisorDTE.nombre;
                emisor.codActividad = emisorDTE.codActividad;
                emisor.descActividad = emisorDTE.descActividad;
                emisor.nombreComercial = emisorDTE.nombreComercial;
                emisor.tipoEstablecimiento = emisorDTE.tipoEstablecimiento;
                emisor.telefono = emisorDTE.telefono;
                emisor.correo = emisorDTE.correo;
                emisor.direccion = direccionEmisor;

                dtejson.emisor = emisor;

                //--------------------------------------------
                direccionReceptor.departamento = cliente.departamento;
                direccionReceptor.municipio = cliente.municipio;
                direccionReceptor.complemento = cliente.direccion;

                receptor.nit = cliente.numDocumento.Replace("-", "");

                if (cliente.nrc != null)
                {
                    receptor.nrc = cliente.nrc.ToString().Replace("-", "");
                }

                if (cliente.nombreComercial != null)
                {
                    receptor.nombreComercial = cliente.nombreComercial;
                }

                receptor.nombre = cliente.nombre;

                receptor.codActividad = cliente.actEconomicaCode;

                receptor.descActividad = cliente.actEconomicaName;

                receptor.telefono = cliente.telefono;

                receptor.correo = cliente.correo;

                receptor.direccion = direccionReceptor;

                dtejson.receptor = receptor;

                //----------------------------------------------------
                dtejson.ventaTercero = null;
                //----------------------------------------------------
                CuerpoDoc.AddRange(consultas.obtenerCuerpoDocNC(docNum, codigoGeneracion));

                dtejson.cuerpoDocumento = new List<CuerpoDocumentoNC>();
                dtejson.cuerpoDocumento.AddRange(CuerpoDoc);
                //----------------------------------------------------

                dtejson.resumen = consultas.obtenerResumenDocNC(docNum);

                //----------------------------------------------------

                extension = null;

                dtejson.extension = extension;

                //----------------------------------------------------

                dtejson.apendice = null;


                jsonFirmador.dteJson = dtejson;

                
            }
            catch (Exception e)
            {
                MessageBox.Show("Error obtenerJSONNC: " + e.Message);
            }

            return jsonFirmador;
        }

        private async Task enviarNR(Cliente cliente, int docNum)
        {
            try
            {
                conectarDI();
                if (mySBO_Company.Connect() != 0)
                {
                    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                }
                else
                {
                    jsonFirmadorNR jsonFirmador = new jsonFirmadorNR();//json a transmitir
                    JSONNR dtejson = new JSONNR();
                    IdentificacionNR identificacion = new IdentificacionNR();
                    Direccion direccionEmisor = new Direccion();
                    Direccion direccionReceptor = new Direccion();
                    EmisorNR emisor = new EmisorNR();
                    ReceptorNR receptor = new ReceptorNR();
                    ExtensionNR extension = new ExtensionNR();

                    List<CuerpoDocumentoNR> CuerpoDoc = new List<CuerpoDocumentoNR>();

                    EmisorDTE emisorDTE = new EmisorDTE();

                    Guid g = Guid.NewGuid(); //LIBRERIA PARA CODIGO DE GENERACION
                    String codigoGeneracion = "";


                    ConsumoFirmador consumoFirmador = new ConsumoFirmador();

                    jsonReceptor jsonReceptor = new jsonReceptor();

                    dynamic jsonRespuesta;

                    emisorDTE = consultas.obtenerInfoEmisor();

                    //-------------------------------------------
                    jsonFirmador.contentType = "application/JSON";
                    jsonFirmador.nit = emisorDTE.userDTE;
                    jsonFirmador.activo = "True";
                    jsonFirmador.passwordPri = emisorDTE.passDTE;

                    //-------------------------------------------
                    identificacion.version = cliente.version;
                    identificacion.ambiente = emisorDTE.ambiente;
                    identificacion.tipoDte = cliente.tipoDoc;

                    string numeracion = "000000000000000";

                    string docNumber = docNum.ToString();

                    string numControl1 = (numeracion + docNumber.Substring(docNumber.Length - 4));

                    string numControl = "DTE-" + cliente.tipoDoc + "-" + cliente.sucursal + "01-" + numControl1.Substring(numControl1.Length - 15);

                    identificacion.numeroControl = numControl;
                    codigoGeneracion = g.ToString().ToUpper();
                    identificacion.codigoGeneracion = codigoGeneracion;
                    identificacion.tipoModelo = 1;
                    identificacion.tipoOperacion = 1;
                    identificacion.tipoContingencia = null;
                    identificacion.motivoContin = null;
                    if (DateTime.Now.ToString("dd") == "01" || DateTime.Now.ToString("dd") == "02" || DateTime.Now.ToString("dd") == "03" || DateTime.Now.ToString("dd") == "04" || DateTime.Now.ToString("dd") == "05")
                        identificacion.fecEmi = cliente.docDate;
                    else
                        identificacion.fecEmi = DateTime.Now.ToString("yyyy-MM-dd");
                    identificacion.horEmi = DateTime.Now.ToString("HH:mm:ss");
                    identificacion.tipoMoneda = "USD";


                    dtejson.identificacion = identificacion;

                    //-------------------------------------------
                    /*dtejson.documentoRelacionado = new List<DocumentoRelacionadoNC>();
                    dtejson.documentoRelacionado.AddRange(consultas.obtenerDocumentoRelacionadoNC(docNum));
                    if (dtejson.documentoRelacionado[0].numeroDocumento == null)
                    {
                        dtejson.documentoRelacionado = null;
                    }*/

                    //-------------------------------------------
                    direccionEmisor.departamento = emisorDTE.departamento;
                    direccionEmisor.municipio = emisorDTE.municipio;
                    direccionEmisor.complemento = emisorDTE.direccion;

                    emisor.nit = emisorDTE.nit;
                    emisor.nrc = emisorDTE.nrc;
                    emisor.nombre = emisorDTE.nombre;
                    emisor.codActividad = emisorDTE.codActividad;
                    emisor.descActividad = emisorDTE.descActividad;
                    emisor.nombreComercial = emisorDTE.nombreComercial;
                    emisor.tipoEstablecimiento = emisorDTE.tipoEstablecimiento;
                    emisor.telefono = emisorDTE.telefono;
                    emisor.correo = emisorDTE.correo;
                    emisor.direccion = direccionEmisor;

                    dtejson.emisor = emisor;

                    //--------------------------------------------
                    direccionReceptor.departamento = cliente.departamento;
                    direccionReceptor.municipio = cliente.municipio;
                    direccionReceptor.complemento = cliente.direccion;


                    receptor.tipoDocumento = cliente.tipoDocumento;

                    receptor.numDocumento = cliente.tipoDocumento == "13" ? cliente.numDocumento : cliente.numDocumento.Replace("-", "");

                    if (cliente.nrc != null)
                    {
                        receptor.nrc = cliente.nrc.ToString().Replace("-", "");
                    }

                    if (cliente.nombreComercial != null)
                    {
                        receptor.nombreComercial = cliente.nombreComercial;
                    }

                    receptor.nombre = cliente.nombre;

                    receptor.codActividad = cliente.actEconomicaCode;

                    receptor.descActividad = cliente.actEconomicaName;

                    receptor.telefono = cliente.telefono;

                    receptor.correo = cliente.correo;

                    receptor.direccion = direccionReceptor;

                    receptor.bienTitulo = cliente.bienTitulo;

                    dtejson.receptor = receptor;

                    //----------------------------------------------------
                    dtejson.ventaTercero = null;
                    //----------------------------------------------------
                    CuerpoDoc.AddRange(consultas.obtenerCuerpoDocNR(docNum, codigoGeneracion));

                    dtejson.cuerpoDocumento = new List<CuerpoDocumentoNR>();
                    dtejson.cuerpoDocumento.AddRange(CuerpoDoc);
                    //----------------------------------------------------

                    dtejson.resumen = consultas.obtenerResumenDocNR(docNum);

                    //----------------------------------------------------

                    extension = null;

                    dtejson.extension = extension;

                    //----------------------------------------------------

                    dtejson.apendice = null;


                    jsonFirmador.dteJson = dtejson;

                    string jsonDTE = JsonConvert.SerializeObject(dtejson);//CONVERTIR OBJETO A JSON

                    string json = JsonConvert.SerializeObject(jsonFirmador);//CONVERTIR OBJETO A JSON

                    Console.WriteLine(json);
                    WriteToFile("JSON PLANO NR " + json);

                    jsonRespuesta = await consumoFirmador.postFirmador("http://" + serverDB + ":8113/firmardocumento/", json); //ENVIAR A SERVICIO DE FIRMADO

                    Response response = JsonConvert.DeserializeObject<Response>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO

                    if (response.status == "OK")
                    {

                        jsonRespuesta = await consumoFirmador.postAutenticador(emisorDTE.ambiente == "00" ? Constantes.urlAutenticadorTest : Constantes.urlAutenticador, emisorDTE.nit, emisorDTE.passPrivDTE);
                        ResponseAutenticador responseAutenticador = JsonConvert.DeserializeObject<ResponseAutenticador>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO
                        if (responseAutenticador.status == "OK")
                        {
                            jsonReceptor.ambiente = emisorDTE.ambiente;
                            jsonReceptor.idEnvio = docNum;
                            jsonReceptor.version = cliente.version;
                            jsonReceptor.tipoDte = cliente.tipoDoc;
                            jsonReceptor.documento = response.body.ToString();
                            jsonReceptor.codigoGeneracion = codigoGeneracion;
                            string jsonreceptor = JsonConvert.SerializeObject(jsonReceptor);//CONVERTIR OBJETO A JSON
                            Console.WriteLine(jsonreceptor);
                            WriteToFile("JSON FIRMADO NR: " + jsonreceptor);
                            jsonRespuesta = await consumoFirmador.postReceptor(emisorDTE.ambiente == "00" ? Constantes.urlRecepcionTest : Constantes.urlRecepcion, responseAutenticador.body.token, jsonreceptor); //ENVIAR A SERVICIO DE FIRMADO
                            ResponseReceptor responseReceptor = JsonConvert.DeserializeObject<ResponseReceptor>(jsonRespuesta.ToString());

                            Console.WriteLine(jsonRespuesta);
                            WriteToFile("JSON RESPUESTA NR: " + jsonRespuesta);

                            if (responseReceptor.estado.Equals("PROCESADO"))
                            {
                                Console.WriteLine("Sello recibido: " + responseReceptor.selloRecibido);
                                guardarDatosDTE(responseReceptor, docNum, dtejson.identificacion.numeroControl.ToString(), dtejson.identificacion.tipoModelo, dtejson.identificacion.tipoOperacion, dtejson.identificacion.tipoDte, jsonDTE, jsonReceptor.documento, "N");
                            }
                            else
                            {
                                mySBO_Application.MessageBox("La factura no pudo ser procesada: \n\n" + jsonRespuesta);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error enviarNR: " + e.Message);
            }
            finally
            {
                mySBO_Company.Disconnect();
            }
        }

        private async Task enviarNRT(Cliente cliente, int docNum)
        {
            try
            {

                conectarDI();
                if (mySBO_Company.Connect() != 0)
                {
                    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                }
                else
                {
                    jsonFirmadorNR jsonFirmador = new jsonFirmadorNR();//json a transmitir
                    JSONNR dtejson = new JSONNR();
                    IdentificacionNR identificacion = new IdentificacionNR();
                    Direccion direccionEmisor = new Direccion();
                    Direccion direccionReceptor = new Direccion();
                    EmisorNR emisor = new EmisorNR();
                    ReceptorNR receptor = new ReceptorNR();
                    ExtensionNR extension = new ExtensionNR();

                    List<CuerpoDocumentoNR> CuerpoDoc = new List<CuerpoDocumentoNR>();

                    EmisorDTE emisorDTE = new EmisorDTE();

                    Guid g = Guid.NewGuid(); //LIBRERIA PARA CODIGO DE GENERACION
                    String codigoGeneracion = "";


                    ConsumoFirmador consumoFirmador = new ConsumoFirmador();

                    jsonReceptor jsonReceptor = new jsonReceptor();

                    dynamic jsonRespuesta;

                    emisorDTE = consultas.obtenerInfoEmisor();

                    //-------------------------------------------
                    jsonFirmador.contentType = "application/JSON";
                    jsonFirmador.nit = emisorDTE.userDTE;
                    jsonFirmador.activo = "True";
                    jsonFirmador.passwordPri = emisorDTE.passDTE;

                    //-------------------------------------------
                    identificacion.version = cliente.version;
                    identificacion.ambiente = emisorDTE.ambiente;
                    identificacion.tipoDte = cliente.tipoDoc;

                    string numeracion = "000000000000000";

                    string docNumber = docNum.ToString();

                    string numControl1 = (numeracion + docNumber.Substring(docNumber.Length - 4));

                    string numControl = "DTE-" + cliente.tipoDoc + "-" + cliente.sucursal + "01-" + numControl1.Substring(numControl1.Length - 15);

                    identificacion.numeroControl = numControl;
                    codigoGeneracion = g.ToString().ToUpper();
                    identificacion.codigoGeneracion = codigoGeneracion;
                    identificacion.tipoModelo = 1;
                    identificacion.tipoOperacion = 1;
                    identificacion.tipoContingencia = null;
                    identificacion.motivoContin = null;
                    if (DateTime.Now.ToString("dd") == "01" || DateTime.Now.ToString("dd") == "02" || DateTime.Now.ToString("dd") == "03" || DateTime.Now.ToString("dd") == "04" || DateTime.Now.ToString("dd") == "05")
                        identificacion.fecEmi = cliente.docDate;
                    else
                        identificacion.fecEmi = DateTime.Now.ToString("yyyy-MM-dd");
                    identificacion.horEmi = DateTime.Now.ToString("HH:mm:ss");
                    identificacion.tipoMoneda = "USD";


                    dtejson.identificacion = identificacion;

                    //-------------------------------------------
                    /*dtejson.documentoRelacionado = new List<DocumentoRelacionadoNC>();
                    dtejson.documentoRelacionado.AddRange(consultas.obtenerDocumentoRelacionadoNC(docNum));
                    if (dtejson.documentoRelacionado[0].numeroDocumento == null)
                    {
                        dtejson.documentoRelacionado = null;
                    }*/

                    //-------------------------------------------
                    direccionEmisor.departamento = emisorDTE.departamento;
                    direccionEmisor.municipio = emisorDTE.municipio;
                    direccionEmisor.complemento = emisorDTE.direccion;

                    emisor.nit = emisorDTE.nit;
                    emisor.nrc = emisorDTE.nrc;
                    emisor.nombre = emisorDTE.nombre;
                    emisor.codActividad = emisorDTE.codActividad;
                    emisor.descActividad = emisorDTE.descActividad;
                    emisor.nombreComercial = emisorDTE.nombreComercial;
                    emisor.tipoEstablecimiento = emisorDTE.tipoEstablecimiento;
                    emisor.telefono = emisorDTE.telefono;
                    emisor.correo = emisorDTE.correo;
                    emisor.direccion = direccionEmisor;

                    dtejson.emisor = emisor;

                    //--------------------------------------------
                    direccionReceptor.departamento = cliente.departamento;
                    direccionReceptor.municipio = cliente.municipio;
                    direccionReceptor.complemento = cliente.direccion;


                    receptor.tipoDocumento = cliente.tipoDocumento;

                    receptor.numDocumento = cliente.numDocumento.Replace("-", ""); ;

                    if (cliente.nrc != null)
                    {
                        receptor.nrc = cliente.nrc.ToString().Replace("-", "");
                    }

                    if (cliente.nombreComercial != null)
                    {
                        receptor.nombreComercial = cliente.nombreComercial;
                    }

                    receptor.nombre = cliente.nombre;

                    receptor.codActividad = cliente.actEconomicaCode;

                    receptor.descActividad = cliente.actEconomicaName;

                    receptor.telefono = cliente.telefono;

                    receptor.correo = cliente.correo;

                    receptor.direccion = direccionReceptor;

                    receptor.bienTitulo = cliente.bienTitulo;

                    dtejson.receptor = receptor;

                    //----------------------------------------------------
                    dtejson.ventaTercero = null;
                    //----------------------------------------------------
                    CuerpoDoc.AddRange(consultas.obtenerCuerpoDocNRT(docNum, codigoGeneracion));

                    dtejson.cuerpoDocumento = new List<CuerpoDocumentoNR>();
                    dtejson.cuerpoDocumento.AddRange(CuerpoDoc);
                    //----------------------------------------------------

                    dtejson.resumen = consultas.obtenerResumenDocNRT(docNum);

                    //----------------------------------------------------

                    extension = null;

                    dtejson.extension = extension;

                    //----------------------------------------------------

                    dtejson.apendice = null;


                    jsonFirmador.dteJson = dtejson;

                    string jsonDTE = JsonConvert.SerializeObject(dtejson);//CONVERTIR OBJETO A JSON

                    string json = JsonConvert.SerializeObject(jsonFirmador);//CONVERTIR OBJETO A JSON

                    Console.WriteLine(json);
                    WriteToFile("JSON PLANO NR: " + json);

                    jsonRespuesta = await consumoFirmador.postFirmador("http://" + serverDB + ":8113/firmardocumento/", json); //ENVIAR A SERVICIO DE FIRMADO

                    Response response = JsonConvert.DeserializeObject<Response>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO

                    if (response.status == "OK")
                    {

                        jsonRespuesta = await consumoFirmador.postAutenticador(emisorDTE.ambiente == "00" ? Constantes.urlAutenticadorTest : Constantes.urlAutenticador, emisorDTE.nit, emisorDTE.passPrivDTE);
                        ResponseAutenticador responseAutenticador = JsonConvert.DeserializeObject<ResponseAutenticador>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO
                        if (responseAutenticador.status == "OK")
                        {
                            jsonReceptor.ambiente = emisorDTE.ambiente;
                            jsonReceptor.idEnvio = docNum;
                            jsonReceptor.version = cliente.version;
                            jsonReceptor.tipoDte = cliente.tipoDoc;
                            jsonReceptor.documento = response.body.ToString();
                            jsonReceptor.codigoGeneracion = codigoGeneracion;
                            string jsonreceptor = JsonConvert.SerializeObject(jsonReceptor);//CONVERTIR OBJETO A JSON
                            Console.WriteLine(jsonreceptor);
                            WriteToFile("JSON FIRMADO NR: " + jsonreceptor);
                            jsonRespuesta = await consumoFirmador.postReceptor(emisorDTE.ambiente == "00" ? Constantes.urlRecepcionTest : Constantes.urlRecepcion, responseAutenticador.body.token, jsonreceptor); //ENVIAR A SERVICIO DE FIRMADO
                            ResponseReceptor responseReceptor = JsonConvert.DeserializeObject<ResponseReceptor>(jsonRespuesta.ToString());

                            Console.WriteLine(jsonRespuesta);
                            WriteToFile("JSON RESPUESTA NR: " + jsonRespuesta);

                            if (responseReceptor.estado.Equals("PROCESADO"))
                            {
                                Console.WriteLine("Sello recibido: " + responseReceptor.selloRecibido);
                                guardarDatosDTE(responseReceptor, docNum, dtejson.identificacion.numeroControl.ToString(), dtejson.identificacion.tipoModelo, dtejson.identificacion.tipoOperacion, dtejson.identificacion.tipoDte, jsonDTE, jsonReceptor.documento, "N");
                            }
                            else
                            {
                                mySBO_Application.MessageBox("La factura no pudo ser procesada: \n\n" + jsonRespuesta);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error enviarNR: " + e.Message);
            }
            finally
            {
                mySBO_Company.Disconnect();
            }
        }

        private jsonFirmadorNR obtenerJSONNR(Cliente cliente, int docNum)
        {
            jsonFirmadorNR jsonFirmador = new jsonFirmadorNR();//json a transmitir
            JSONNR dtejson = new JSONNR();
            IdentificacionNR identificacion = new IdentificacionNR();
            Direccion direccionEmisor = new Direccion();
            Direccion direccionReceptor = new Direccion();
            EmisorNR emisor = new EmisorNR();
            ReceptorNR receptor = new ReceptorNR();
            ExtensionNR extension = new ExtensionNR();

            List<CuerpoDocumentoNR> CuerpoDoc = new List<CuerpoDocumentoNR>();

            EmisorDTE emisorDTE = new EmisorDTE();

            Guid g = Guid.NewGuid(); //LIBRERIA PARA CODIGO DE GENERACION
            String codigoGeneracion = "";

            ConsumoFirmador consumoFirmador = new ConsumoFirmador();

            jsonReceptor jsonReceptor = new jsonReceptor();

            dynamic jsonRespuesta;

            Response response = new Response();

            try
            {
                emisorDTE = consultas.obtenerInfoEmisor();

                //-------------------------------------------
                jsonFirmador.contentType = "application/JSON";
                jsonFirmador.nit = emisorDTE.userDTE;
                jsonFirmador.activo = "True";
                jsonFirmador.passwordPri = emisorDTE.passDTE;

                //-------------------------------------------
                identificacion.version = cliente.version;
                identificacion.ambiente = emisorDTE.ambiente;
                identificacion.tipoDte = cliente.tipoDoc;

                string numeracion = "000000000000000";

                string docNumber = docNum.ToString();

                string numControl1 = (numeracion + docNumber.Substring(docNumber.Length - 4));

                string numControl = "DTE-" + cliente.tipoDoc + "-" + cliente.sucursal + "01-" + numControl1.Substring(numControl1.Length - 15);

                identificacion.numeroControl = numControl;
                codigoGeneracion = g.ToString().ToUpper();
                identificacion.codigoGeneracion = codigoGeneracion;
                identificacion.tipoModelo = 1;
                identificacion.tipoOperacion = 1;
                identificacion.tipoContingencia = null;
                identificacion.motivoContin = null;
                if (DateTime.Now.ToString("dd") == "01" || DateTime.Now.ToString("dd") == "02" || DateTime.Now.ToString("dd") == "03" || DateTime.Now.ToString("dd") == "04" || DateTime.Now.ToString("dd") == "05")
                    identificacion.fecEmi = cliente.docDate;
                else
                    identificacion.fecEmi = DateTime.Now.ToString("yyyy-MM-dd");
                identificacion.horEmi = DateTime.Now.ToString("HH:mm:ss");
                identificacion.tipoMoneda = "USD";


                dtejson.identificacion = identificacion;

                //-------------------------------------------
                /*dtejson.documentoRelacionado = new List<DocumentoRelacionadoNC>();
                dtejson.documentoRelacionado.AddRange(consultas.obtenerDocumentoRelacionadoNC(docNum));
                if (dtejson.documentoRelacionado[0].numeroDocumento == null)
                {
                    dtejson.documentoRelacionado = null;
                }*/

                //-------------------------------------------
                direccionEmisor.departamento = emisorDTE.departamento;
                direccionEmisor.municipio = emisorDTE.municipio;
                direccionEmisor.complemento = emisorDTE.direccion;

                emisor.nit = emisorDTE.nit;
                emisor.nrc = emisorDTE.nrc;
                emisor.nombre = emisorDTE.nombre;
                emisor.codActividad = emisorDTE.codActividad;
                emisor.descActividad = emisorDTE.descActividad;
                emisor.nombreComercial = emisorDTE.nombreComercial;
                emisor.tipoEstablecimiento = emisorDTE.tipoEstablecimiento;
                emisor.telefono = emisorDTE.telefono;
                emisor.correo = emisorDTE.correo;
                emisor.direccion = direccionEmisor;

                dtejson.emisor = emisor;

                //--------------------------------------------
                direccionReceptor.departamento = cliente.departamento;
                direccionReceptor.municipio = cliente.municipio;
                direccionReceptor.complemento = cliente.direccion;


                receptor.tipoDocumento = cliente.tipoDocumento;

                receptor.numDocumento = cliente.numDocumento;

                if (cliente.nrc != null)
                {
                    receptor.nrc = cliente.nrc.ToString().Replace("-", "");
                }

                if (cliente.nombreComercial != null)
                {
                    receptor.nombreComercial = cliente.nombreComercial;
                }

                receptor.nombre = cliente.nombre;

                receptor.codActividad = cliente.actEconomicaCode;

                receptor.descActividad = cliente.actEconomicaName;

                receptor.telefono = cliente.telefono;

                receptor.correo = cliente.correo;

                receptor.direccion = direccionReceptor;

                receptor.bienTitulo = cliente.bienTitulo;

                dtejson.receptor = receptor;

                //----------------------------------------------------
                dtejson.ventaTercero = null;
                //----------------------------------------------------
                CuerpoDoc.AddRange(consultas.obtenerCuerpoDocNR(docNum, codigoGeneracion));

                dtejson.cuerpoDocumento = new List<CuerpoDocumentoNR>();
                dtejson.cuerpoDocumento.AddRange(CuerpoDoc);
                //----------------------------------------------------

                dtejson.resumen = consultas.obtenerResumenDocNR(docNum);

                //----------------------------------------------------

                extension = null;

                dtejson.extension = extension;

                //----------------------------------------------------

                dtejson.apendice = null;


                jsonFirmador.dteJson = dtejson;

            }
            catch (Exception e)
            {
                MessageBox.Show("Error obtenerJSONNR: " + e.Message);
            }

            return jsonFirmador;
        }

        private async Task enviarFSE(Cliente cliente, int docNum)
        {
            try
            {
                conectarDI();
                if (mySBO_Company.Connect() != 0)
                {
                    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                }
                else
                {
                    jsonFirmadorFSE jsonFirmador = new jsonFirmadorFSE();//json a transmitir
                    JSONFSE dtejson = new JSONFSE();
                    IdentificacionFSE identificacion = new IdentificacionFSE();
                    Direccion direccionEmisor = new Direccion();
                    Direccion direccionReceptor = new Direccion();
                    EmisorFSE emisor = new EmisorFSE();
                    sujetoExcluidoFSE receptor = new sujetoExcluidoFSE();

                    List<CuerpoDocumentoFSE> CuerpoDoc = new List<CuerpoDocumentoFSE>();

                    EmisorDTE emisorDTE = new EmisorDTE();

                    Guid g = Guid.NewGuid(); //LIBRERIA PARA CODIGO DE GENERACION
                    String codigoGeneracion = "";


                    ConsumoFirmador consumoFirmador = new ConsumoFirmador();

                    jsonReceptor jsonReceptor = new jsonReceptor();

                    dynamic jsonRespuesta;

                    emisorDTE = consultas.obtenerInfoEmisor();

                    //-------------------------------------------
                    jsonFirmador.contentType = "application/JSON";
                    jsonFirmador.nit = emisorDTE.userDTE;
                    jsonFirmador.activo = "True";
                    jsonFirmador.passwordPri = emisorDTE.passDTE;

                    //-------------------------------------------
                    identificacion.version = cliente.version;
                    identificacion.ambiente = emisorDTE.ambiente;
                    identificacion.tipoDte = cliente.tipoDoc;

                    string numeracion = "000000000000000";

                    string docNumber = docNum.ToString();

                    string numControl1 = (numeracion + docNumber.Substring(docNumber.Length - 4));

                    string numControl = "DTE-" + cliente.tipoDoc + "-" + cliente.sucursal + "01-" + numControl1.Substring(numControl1.Length - 15);

                    identificacion.numeroControl = numControl;
                    codigoGeneracion = g.ToString().ToUpper();
                    identificacion.codigoGeneracion = codigoGeneracion;
                    identificacion.tipoModelo = 1;
                    identificacion.tipoOperacion = 1;
                    identificacion.tipoContingencia = null;
                    identificacion.motivoContin = null;
                    if (DateTime.Now.ToString("dd") == "01" || DateTime.Now.ToString("dd") == "02" || DateTime.Now.ToString("dd") == "03" || DateTime.Now.ToString("dd") == "04" || DateTime.Now.ToString("dd") == "05")
                        identificacion.fecEmi = cliente.docDate;
                    else
                        identificacion.fecEmi = DateTime.Now.ToString("yyyy-MM-dd");
                    identificacion.horEmi = DateTime.Now.ToString("HH:mm:ss");
                    identificacion.tipoMoneda = "USD";


                    dtejson.identificacion = identificacion;

                    //-------------------------------------------
                    /*dtejson.documentoRelacionado = new List<DocumentoRelacionadoNC>();
                    dtejson.documentoRelacionado.AddRange(consultas.obtenerDocumentoRelacionadoNC(docNum));
                    if (dtejson.documentoRelacionado[0].numeroDocumento == null)
                    {
                        dtejson.documentoRelacionado = null;
                    }*/

                    //-------------------------------------------
                    direccionEmisor.departamento = emisorDTE.departamento;
                    direccionEmisor.municipio = emisorDTE.municipio;
                    direccionEmisor.complemento = emisorDTE.direccion;

                    emisor.nit = emisorDTE.nit;
                    emisor.nrc = emisorDTE.nrc;
                    emisor.nombre = emisorDTE.nombre;
                    emisor.codActividad = emisorDTE.codActividad;
                    emisor.descActividad = emisorDTE.descActividad;
                    emisor.telefono = emisorDTE.telefono;
                    emisor.correo = emisorDTE.correo;
                    emisor.direccion = direccionEmisor;

                    dtejson.emisor = emisor;

                    //--------------------------------------------
                    direccionReceptor.departamento = cliente.departamento;
                    direccionReceptor.municipio = cliente.municipio;
                    direccionReceptor.complemento = cliente.direccion;


                    receptor.tipoDocumento = cliente.tipoDocumento;

                    receptor.numDocumento = cliente.numDocumento;

                    receptor.nombre = cliente.nombre;

                    receptor.codActividad = cliente.actEconomicaCode;

                    receptor.descActividad = cliente.actEconomicaName;

                    receptor.telefono = cliente.telefono;

                    receptor.correo = cliente.correo;

                    receptor.direccion = direccionReceptor;

                    dtejson.sujetoExcluido = receptor;

                    //----------------------------------------------------
                    CuerpoDoc.AddRange(consultas.obtenerCuerpoDocFSE(docNum, codigoGeneracion));

                    dtejson.cuerpoDocumento = new List<CuerpoDocumentoFSE>();
                    dtejson.cuerpoDocumento.AddRange(CuerpoDoc);
                    //----------------------------------------------------

                    dtejson.resumen = consultas.obtenerResumenDocFSE(docNum);

                    //----------------------------------------------------

                    dtejson.apendice = null;


                    jsonFirmador.dteJson = dtejson;

                    string jsonDTE = JsonConvert.SerializeObject(dtejson);//CONVERTIR OBJETO A JSON

                    string json = JsonConvert.SerializeObject(jsonFirmador);//CONVERTIR OBJETO A JSON

                    Console.WriteLine(json);
                    WriteToFile("JSON PLANO FSE: " + json);

                    jsonRespuesta = await consumoFirmador.postFirmador("http://" + serverDB + ":8113/firmardocumento/", json); //ENVIAR A SERVICIO DE FIRMADO

                    Response response = JsonConvert.DeserializeObject<Response>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO

                    if (response.status == "OK")
                    {

                        jsonRespuesta = await consumoFirmador.postAutenticador(emisorDTE.ambiente == "00" ? Constantes.urlAutenticadorTest : Constantes.urlAutenticador, emisorDTE.nit, emisorDTE.passPrivDTE);
                        ResponseAutenticador responseAutenticador = JsonConvert.DeserializeObject<ResponseAutenticador>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO
                        if (responseAutenticador.status == "OK")
                        {
                            jsonReceptor.ambiente = emisorDTE.ambiente;
                            jsonReceptor.idEnvio = docNum;
                            jsonReceptor.version = cliente.version;
                            jsonReceptor.tipoDte = cliente.tipoDoc;
                            jsonReceptor.documento = response.body.ToString();
                            jsonReceptor.codigoGeneracion = codigoGeneracion;
                            string jsonreceptor = JsonConvert.SerializeObject(jsonReceptor);//CONVERTIR OBJETO A JSON
                            Console.WriteLine(jsonreceptor);
                            WriteToFile("JSON FIRMADO FSE: " + jsonreceptor);
                            jsonRespuesta = await consumoFirmador.postReceptor(emisorDTE.ambiente == "00" ? Constantes.urlRecepcionTest : Constantes.urlRecepcion, responseAutenticador.body.token, jsonreceptor); //ENVIAR A SERVICIO DE FIRMADO
                            ResponseReceptor responseReceptor = JsonConvert.DeserializeObject<ResponseReceptor>(jsonRespuesta.ToString());

                            Console.WriteLine(jsonRespuesta);

                            WriteToFile("JSON RESPUESTA FSE: " + jsonRespuesta);
                            if (responseReceptor.estado.Equals("PROCESADO"))
                            {
                                Console.WriteLine("Sello recibido: " + responseReceptor.selloRecibido);
                                guardarDatosDTE(responseReceptor, docNum, dtejson.identificacion.numeroControl.ToString(), dtejson.identificacion.tipoModelo, dtejson.identificacion.tipoOperacion, dtejson.identificacion.tipoDte, jsonDTE, jsonReceptor.documento, "N");
                            }
                            else
                            {
                                mySBO_Application.MessageBox("La factura no pudo ser procesada: \n\n" + jsonRespuesta);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error enviarFSE: " + e.Message);
            }
            finally
            {
                mySBO_Company.Disconnect();
            }
        }

        private jsonFirmadorFSE obtenerJSONFSE(Cliente cliente, int docNum)
        {
            jsonFirmadorFSE jsonFirmador = new jsonFirmadorFSE();//json a transmitir
            JSONFSE dtejson = new JSONFSE();
            IdentificacionFSE identificacion = new IdentificacionFSE();
            Direccion direccionEmisor = new Direccion();
            Direccion direccionReceptor = new Direccion();
            EmisorFSE emisor = new EmisorFSE();
            sujetoExcluidoFSE receptor = new sujetoExcluidoFSE();

            List<CuerpoDocumentoFSE> CuerpoDoc = new List<CuerpoDocumentoFSE>();

            EmisorDTE emisorDTE = new EmisorDTE();

            Guid g = Guid.NewGuid(); //LIBRERIA PARA CODIGO DE GENERACION
            String codigoGeneracion = "";


            ConsumoFirmador consumoFirmador = new ConsumoFirmador();

            jsonReceptor jsonReceptor = new jsonReceptor();

            dynamic jsonRespuesta;

            Response response = new Response();

            try
            {
                emisorDTE = consultas.obtenerInfoEmisor();

                //-------------------------------------------
                jsonFirmador.contentType = "application/JSON";
                jsonFirmador.nit = emisorDTE.userDTE;
                jsonFirmador.activo = "True";
                jsonFirmador.passwordPri = emisorDTE.passDTE;

                //-------------------------------------------
                identificacion.version = cliente.version;
                identificacion.ambiente = emisorDTE.ambiente;
                identificacion.tipoDte = cliente.tipoDoc;

                string numeracion = "000000000000000";

                string docNumber = docNum.ToString();

                string numControl1 = (numeracion + docNumber.Substring(docNumber.Length - 4));

                string numControl = "DTE-" + cliente.tipoDoc + "-" + cliente.sucursal + "01-" + numControl1.Substring(numControl1.Length - 15);

                identificacion.numeroControl = numControl;
                codigoGeneracion = g.ToString().ToUpper();
                identificacion.codigoGeneracion = codigoGeneracion;
                identificacion.tipoModelo = 1;
                identificacion.tipoOperacion = 1;
                identificacion.tipoContingencia = null;
                identificacion.motivoContin = null;
                if (DateTime.Now.ToString("dd") == "01" || DateTime.Now.ToString("dd") == "02" || DateTime.Now.ToString("dd") == "03" || DateTime.Now.ToString("dd") == "04" || DateTime.Now.ToString("dd") == "05")
                    identificacion.fecEmi = cliente.docDate;
                else
                    identificacion.fecEmi = DateTime.Now.ToString("yyyy-MM-dd");
                identificacion.horEmi = DateTime.Now.ToString("HH:mm:ss");
                identificacion.tipoMoneda = "USD";


                dtejson.identificacion = identificacion;

                //-------------------------------------------
                /*dtejson.documentoRelacionado = new List<DocumentoRelacionadoNC>();
                dtejson.documentoRelacionado.AddRange(consultas.obtenerDocumentoRelacionadoNC(docNum));
                if (dtejson.documentoRelacionado[0].numeroDocumento == null)
                {
                    dtejson.documentoRelacionado = null;
                }*/

                //-------------------------------------------
                direccionEmisor.departamento = emisorDTE.departamento;
                direccionEmisor.municipio = emisorDTE.municipio;
                direccionEmisor.complemento = emisorDTE.direccion;

                emisor.nit = emisorDTE.nit;
                emisor.nrc = emisorDTE.nrc;
                emisor.nombre = emisorDTE.nombre;
                emisor.codActividad = emisorDTE.codActividad;
                emisor.descActividad = emisorDTE.descActividad;
                emisor.telefono = emisorDTE.telefono;
                emisor.correo = emisorDTE.correo;
                emisor.direccion = direccionEmisor;

                dtejson.emisor = emisor;

                //--------------------------------------------
                direccionReceptor.departamento = cliente.departamento;
                direccionReceptor.municipio = cliente.municipio;
                direccionReceptor.complemento = cliente.direccion;


                receptor.tipoDocumento = cliente.tipoDocumento;

                receptor.numDocumento = cliente.numDocumento;

                receptor.nombre = cliente.nombre;

                receptor.codActividad = cliente.actEconomicaCode;

                receptor.descActividad = cliente.actEconomicaName;

                receptor.telefono = cliente.telefono;

                receptor.correo = cliente.correo;

                receptor.direccion = direccionReceptor;

                dtejson.sujetoExcluido = receptor;

                //----------------------------------------------------
                CuerpoDoc.AddRange(consultas.obtenerCuerpoDocFSE(docNum, codigoGeneracion));

                dtejson.cuerpoDocumento = new List<CuerpoDocumentoFSE>();
                dtejson.cuerpoDocumento.AddRange(CuerpoDoc);
                //----------------------------------------------------

                dtejson.resumen = consultas.obtenerResumenDocFSE(docNum);

                //----------------------------------------------------

                dtejson.apendice = null;


                jsonFirmador.dteJson = dtejson;

            }
            catch (Exception e)
            {
                MessageBox.Show("Error obtenerJSONFSE: " + e.Message);
            }

            return jsonFirmador;
        }

        private async Task enviarFEX(Cliente cliente, int docNum)
        {
            try
            {
                conectarDI();
                if (mySBO_Company.Connect() != 0)
                {
                    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                }
                else
                {
                    jsonFirmadorFEX jsonFirmador = new jsonFirmadorFEX();//json a transmitir
                    JSONFEX dtejson = new JSONFEX();
                    IdentificacionFEX identificacion = new IdentificacionFEX();
                    Direccion direccionEmisor = new Direccion();
                    Direccion direccionReceptor = new Direccion();
                    EmisorFEX emisor = new EmisorFEX();
                    ReceptorFEX receptor = new ReceptorFEX();
                    ExtensionFEX extension = new ExtensionFEX();

                    List<CuerpoDocumentoFEX> CuerpoDoc = new List<CuerpoDocumentoFEX>();

                    EmisorDTE emisorDTE = new EmisorDTE();

                    Guid g = Guid.NewGuid(); //LIBRERIA PARA CODIGO DE GENERACION
                    String codigoGeneracion = "";


                    ConsumoFirmador consumoFirmador = new ConsumoFirmador();

                    jsonReceptor jsonReceptor = new jsonReceptor();

                    dynamic jsonRespuesta;

                    emisorDTE = consultas.obtenerInfoEmisor();

                    //-------------------------------------------
                    jsonFirmador.contentType = "application/JSON";
                    jsonFirmador.nit = emisorDTE.nit;
                    jsonFirmador.activo = "True";
                    jsonFirmador.passwordPri = emisorDTE.passDTE;

                    //-------------------------------------------
                    identificacion.version = cliente.version;
                    identificacion.ambiente = emisorDTE.ambiente;
                    identificacion.tipoDte = cliente.tipoDoc;

                    string numeracion = "000000000000000";

                    string docNumber = docNum.ToString();

                    string numControl1 = (numeracion + docNumber.Substring(docNumber.Length - 4));

                    string numControl = "DTE-" + cliente.tipoDoc + "-" + cliente.sucursal + "01-" + numControl1.Substring(numControl1.Length - 15);

                    identificacion.numeroControl = numControl;
                    codigoGeneracion = g.ToString().ToUpper();
                    identificacion.codigoGeneracion = codigoGeneracion;
                    identificacion.tipoModelo = 1;
                    identificacion.tipoOperacion = 1;
                    identificacion.tipoContingencia = null;
                    identificacion.motivoContigencia = null;
                    if (DateTime.Now.ToString("dd") == "01" || DateTime.Now.ToString("dd") == "02" || DateTime.Now.ToString("dd") == "03" || DateTime.Now.ToString("dd") == "04" || DateTime.Now.ToString("dd") == "05")
                        identificacion.fecEmi = cliente.docDate;
                    else
                        identificacion.fecEmi = DateTime.Now.ToString("yyyy-MM-dd");
                    identificacion.horEmi = DateTime.Now.ToString("HH:mm:ss");
                    identificacion.tipoMoneda = "USD";


                    dtejson.identificacion = identificacion;


                    //-------------------------------------------
                    direccionEmisor.departamento = emisorDTE.departamento;
                    direccionEmisor.municipio = emisorDTE.municipio;
                    direccionEmisor.complemento = emisorDTE.direccion;

                    emisor.nit = emisorDTE.nit;
                    emisor.nrc = emisorDTE.nrc;
                    emisor.nombre = emisorDTE.nombre;
                    emisor.codActividad = emisorDTE.codActividad;
                    emisor.descActividad = emisorDTE.descActividad;
                    emisor.nombreComercial = emisorDTE.nombreComercial;
                    emisor.tipoEstablecimiento = emisorDTE.tipoEstablecimiento;
                    emisor.telefono = emisorDTE.telefono;
                    emisor.correo = emisorDTE.correo;
                    emisor.codEstableMH = null;
                    emisor.codEstable = null;
                    emisor.codPuntoVentaMH = null;
                    emisor.codPuntoVenta = null;
                    emisor.direccion = direccionEmisor;
                    emisor.tipoItemExpor = cliente.tipoitem;

                    if (Constantes.recintoFiscal != "")
                    {
                        emisor.recintoFiscal = Constantes.recintoFiscal;
                    }
                    if (Constantes.regimen != "")
                    {
                        emisor.regimen = Constantes.regimen;
                    }

                    dtejson.emisor = emisor;

                    //--------------------------------------------

                    receptor.tipoDocumento = cliente.tipoDocumento;

                    receptor.numDocumento = cliente.numDocumento;

                    if (cliente.nombreComercial != null)
                    {
                        receptor.nombreComercial = cliente.nombreComercial;
                    }

                    receptor.nombre = cliente.nombre;

                    receptor.codPais = cliente.codPais;

                    receptor.nombrePais = cliente.pais;

                    receptor.complemento = cliente.direccion;

                    receptor.descActividad = cliente.actEconomicaName;

                    receptor.telefono = cliente.telefono;

                    receptor.correo = cliente.correo;

                    receptor.tipoPersona = cliente.tipoPersona;


                    dtejson.receptor = receptor;

                    //----------------------------------------------------
                    dtejson.otrosDocumentos = null;
                    //----------------------------------------------------
                    dtejson.ventaTercero = null;
                    //----------------------------------------------------
                    CuerpoDoc.AddRange(consultas.obtenerCuerpoDocFEX(docNum, codigoGeneracion));

                    dtejson.cuerpoDocumento = new List<CuerpoDocumentoFEX>();
                    dtejson.cuerpoDocumento.AddRange(CuerpoDoc);
                    //----------------------------------------------------

                    dtejson.resumen = consultas.obtenerResumenDocFEX(docNum);

                    //----------------------------------------------------


                    //----------------------------------------------------

                    dtejson.apendice = null;


                    jsonFirmador.dteJson = dtejson;

                    string jsonDTE = JsonConvert.SerializeObject(dtejson);//CONVERTIR OBJETO A JSON

                    string json = JsonConvert.SerializeObject(jsonFirmador);//CONVERTIR OBJETO A JSON

                    Console.WriteLine(json);
                    WriteToFile("JSON PLANO FEX: " + json);

                    jsonRespuesta = await consumoFirmador.postFirmador("http://" + serverDB + ":8113/firmardocumento/", json); //ENVIAR A SERVICIO DE FIRMADO

                    Response response = JsonConvert.DeserializeObject<Response>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO

                    if (response.status == "OK")
                    {

                        jsonRespuesta = await consumoFirmador.postAutenticador(emisorDTE.ambiente == "00" ? Constantes.urlAutenticadorTest : Constantes.urlAutenticador, emisorDTE.nit, emisorDTE.passPrivDTE);
                        ResponseAutenticador responseAutenticador = JsonConvert.DeserializeObject<ResponseAutenticador>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO
                        if (responseAutenticador.status == "OK")
                        {
                            jsonReceptor.ambiente = emisorDTE.ambiente;
                            jsonReceptor.idEnvio = docNum;
                            jsonReceptor.version = cliente.version;
                            jsonReceptor.tipoDte = cliente.tipoDoc;
                            jsonReceptor.documento = response.body.ToString();
                            jsonReceptor.codigoGeneracion = codigoGeneracion;
                            string jsonreceptor = JsonConvert.SerializeObject(jsonReceptor);//CONVERTIR OBJETO A JSON
                            Console.WriteLine(jsonreceptor);
                            WriteToFile("JSON FIRMADO FEX: " + jsonreceptor);
                            jsonRespuesta = await consumoFirmador.postReceptor(emisorDTE.ambiente == "00" ? Constantes.urlRecepcionTest : Constantes.urlRecepcion, responseAutenticador.body.token, jsonreceptor); //ENVIAR A SERVICIO DE FIRMADO
                            ResponseReceptor responseReceptor = JsonConvert.DeserializeObject<ResponseReceptor>(jsonRespuesta.ToString());

                            Console.WriteLine(jsonRespuesta);

                            WriteToFile("JSON RESPUESTA FSE: " + jsonRespuesta);
                            if (responseReceptor.estado.Equals("PROCESADO"))
                            {
                                Console.WriteLine("Sello recibido: " + responseReceptor.selloRecibido);
                                guardarDatosDTE(responseReceptor, docNum, dtejson.identificacion.numeroControl.ToString(), dtejson.identificacion.tipoModelo, dtejson.identificacion.tipoOperacion, dtejson.identificacion.tipoDte, jsonDTE, jsonReceptor.documento, "N");
                            }
                            else
                            {
                                mySBO_Application.MessageBox("La factura no pudo ser procesada: \n\n" + jsonRespuesta);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error enviarFEX: " + e.Message);
            }
            finally
            {
                mySBO_Company.Disconnect();
            }
        }

        private jsonFirmadorFEX obtenerJSONFEX(Cliente cliente, int docNum)
        {
            
            jsonFirmadorFEX jsonFirmador = new jsonFirmadorFEX();//json a transmitir
            JSONFEX dtejson = new JSONFEX();
            IdentificacionFEX identificacion = new IdentificacionFEX();
            Direccion direccionEmisor = new Direccion();
            Direccion direccionReceptor = new Direccion();
            EmisorFEX emisor = new EmisorFEX();
            ReceptorFEX receptor = new ReceptorFEX();
            ExtensionFEX extension = new ExtensionFEX();

            List<CuerpoDocumentoFEX> CuerpoDoc = new List<CuerpoDocumentoFEX>();

            EmisorDTE emisorDTE = new EmisorDTE();

            Guid g = Guid.NewGuid(); //LIBRERIA PARA CODIGO DE GENERACION
            String codigoGeneracion = "";


            ConsumoFirmador consumoFirmador = new ConsumoFirmador();

            jsonReceptor jsonReceptor = new jsonReceptor();

            dynamic jsonRespuesta;

            Response response = new Response();
            try
            {
                emisorDTE = consultas.obtenerInfoEmisor();

                //-------------------------------------------
                jsonFirmador.contentType = "application/JSON";
                jsonFirmador.nit = emisorDTE.nit;
                jsonFirmador.activo = "True";
                jsonFirmador.passwordPri = emisorDTE.passDTE;

                //-------------------------------------------
                identificacion.version = cliente.version;
                identificacion.ambiente = emisorDTE.ambiente;
                identificacion.tipoDte = cliente.tipoDoc;

                string numeracion = "000000000000000";

                string docNumber = docNum.ToString();

                string numControl1 = (numeracion + docNumber.Substring(docNumber.Length - 4));

                string numControl = "DTE-" + cliente.tipoDoc + "-" + cliente.sucursal + "01-" + numControl1.Substring(numControl1.Length - 15);

                identificacion.numeroControl = numControl;
                codigoGeneracion = g.ToString().ToUpper();
                identificacion.codigoGeneracion = codigoGeneracion;
                identificacion.tipoModelo = 1;
                identificacion.tipoOperacion = 1;
                identificacion.tipoContingencia = null;
                identificacion.motivoContigencia = null;
                if (DateTime.Now.ToString("dd") == "01" || DateTime.Now.ToString("dd") == "02" || DateTime.Now.ToString("dd") == "03" || DateTime.Now.ToString("dd") == "04" || DateTime.Now.ToString("dd") == "05")
                    identificacion.fecEmi = cliente.docDate;
                else
                    identificacion.fecEmi = DateTime.Now.ToString("yyyy-MM-dd");
                identificacion.horEmi = DateTime.Now.ToString("HH:mm:ss");
                identificacion.tipoMoneda = "USD";


                dtejson.identificacion = identificacion;


                //-------------------------------------------
                direccionEmisor.departamento = emisorDTE.departamento;
                direccionEmisor.municipio = emisorDTE.municipio;
                direccionEmisor.complemento = emisorDTE.direccion;

                emisor.nit = emisorDTE.nit;
                emisor.nrc = emisorDTE.nrc;
                emisor.nombre = emisorDTE.nombre;
                emisor.codActividad = emisorDTE.codActividad;
                emisor.descActividad = emisorDTE.descActividad;
                emisor.nombreComercial = emisorDTE.nombreComercial;
                emisor.tipoEstablecimiento = emisorDTE.tipoEstablecimiento;
                emisor.telefono = emisorDTE.telefono;
                emisor.correo = emisorDTE.correo;
                emisor.codEstableMH = null;
                emisor.codEstable = null;
                emisor.codPuntoVentaMH = null;
                emisor.codPuntoVenta = null;
                emisor.direccion = direccionEmisor;
                emisor.tipoItemExpor = cliente.tipoitem;

                if (Constantes.recintoFiscal != "")
                {
                    emisor.recintoFiscal = Constantes.recintoFiscal;
                }
                if (Constantes.regimen != "")
                {
                    emisor.regimen = Constantes.regimen;
                }

                dtejson.emisor = emisor;

                //--------------------------------------------

                receptor.tipoDocumento = cliente.tipoDocumento;

                receptor.numDocumento = cliente.numDocumento;

                if (cliente.nombreComercial != null)
                {
                    receptor.nombreComercial = cliente.nombreComercial;
                }

                receptor.nombre = cliente.nombre;

                receptor.codPais = cliente.codPais;

                receptor.nombrePais = cliente.pais;

                receptor.complemento = cliente.direccion;

                receptor.descActividad = cliente.actEconomicaName;

                receptor.telefono = cliente.telefono;

                receptor.correo = cliente.correo;

                receptor.tipoPersona = cliente.tipoPersona;


                dtejson.receptor = receptor;

                //----------------------------------------------------
                dtejson.otrosDocumentos = null;
                //----------------------------------------------------
                dtejson.ventaTercero = null;
                //----------------------------------------------------
                CuerpoDoc.AddRange(consultas.obtenerCuerpoDocFEX(docNum, codigoGeneracion));

                dtejson.cuerpoDocumento = new List<CuerpoDocumentoFEX>();
                dtejson.cuerpoDocumento.AddRange(CuerpoDoc);
                //----------------------------------------------------

                dtejson.resumen = consultas.obtenerResumenDocFEX(docNum);

                //----------------------------------------------------


                //----------------------------------------------------

                dtejson.apendice = null;


                jsonFirmador.dteJson = dtejson;

            }
            catch (Exception e)
            {
                MessageBox.Show("Error obtenerJSONFEX: " + e.Message);
            }

            return jsonFirmador;
        }

        private async Task enviarAnulacion(DocumentoAnulacion documentoAnul, Cliente cliente, int docNum, String tipoDoc)
        {
            try
            {
                JSONAnulacion dtejson = new JSONAnulacion();//json a transmitir
                jsonAnular jsonAnular = new jsonAnular();
                jsonFirmadorAnulacion jsonFirmador = new jsonFirmadorAnulacion();
                IdentificacionAnulacion identificacion = new IdentificacionAnulacion();
                EmisorAnulacion emisor = new EmisorAnulacion();
                ReceptorAnulacion receptor = new ReceptorAnulacion();
                DocumentoAnulacion documento = new DocumentoAnulacion();
                Motivo motivo = new Motivo();

                EmisorDTE emisorDTE = new EmisorDTE();

                Guid g = Guid.NewGuid(); //LIBRERIA PARA CODIGO DE GENERACION
                String codigoGeneracion = "";

                
                ConsumoFirmador consumoFirmador = new ConsumoFirmador();


                dynamic jsonRespuesta;

                emisorDTE = consultas.obtenerInfoEmisor();


                //-------------------------------------------
                jsonFirmador.contentType = "application/JSON";
                jsonFirmador.nit = emisorDTE.nit;
                jsonFirmador.activo = "True";
                jsonFirmador.passwordPri = emisorDTE.passDTE;


                //-------------------------------------------
                identificacion.version = 02;
                identificacion.ambiente = emisorDTE.ambiente;
                codigoGeneracion = g.ToString().ToUpper();
                identificacion.codigoGeneracion = codigoGeneracion;
                identificacion.fecAnula = DateTime.Now.ToString("yyyy-MM-dd");
                identificacion.horAnula = DateTime.Now.ToString("HH:mm:ss");



                dtejson.identificacion = identificacion;


                //-------------------------------------------

                emisor.nit = emisorDTE.nit;
                emisor.nombre = emisorDTE.nombre;
                emisor.tipoEstablecimiento = emisorDTE.tipoEstablecimiento;
                emisor.telefono = emisorDTE.telefono.Replace("-","");
                emisor.correo = emisorDTE.correo;
                emisor.codEstable = null;
                emisor.codPuntoVenta = null;
                emisor.nomEstablecimiento = emisorDTE.nombre;

                dtejson.emisor = emisor;

                //--------------------------------------------

               /* receptor.tipoDocumento = tipoDocumentoRecep;

                receptor.numDocumento = cliente.numDocumento;

                receptor.nombre = cliente.nombre;

                receptor.telefono = cliente.telefono.Replace("-", ""); ;

                receptor.correo = cliente.correo;*/

                //dtejson.receptor = receptor;

                //--------------------------------------------
                documentoAnul.tipoDte = tipoDoc;

                documentoAnul.tipoDocumento = cliente.tipoDocumento;

                documentoAnul.numDocumento = cliente.numDocumento;

                documentoAnul.nombre = cliente.nombre;

                documentoAnul.telefono = cliente.telefono.Replace("-", "");

                documentoAnul.correo = cliente.correo;


                SAPbouiCOM.EditText myEditText_CodigoGeneracionR = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtCGeR").Specific;
                if (myEditText_CodigoGeneracionR.Value!="") {
                    documentoAnul.codigoGeneracionR = myEditText_CodigoGeneracionR.Value;
                }

                dtejson.documento = documentoAnul;

                //--------------------------------------------

                SAPbouiCOM.ComboBox myEditText_TipoAnul = (SAPbouiCOM.ComboBox)myForm.Items.Item("EA_cbTAnu").Specific;
                SAPbouiCOM.EditText myEditText_Motivo = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtMoti").Specific;
                SAPbouiCOM.EditText myEditText_NomResp = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtNRes").Specific;
                SAPbouiCOM.ComboBox myEditText_TipoDocResp = (SAPbouiCOM.ComboBox)myForm.Items.Item("EA_cbTDRes").Specific;
                SAPbouiCOM.EditText myEditText_NumResp = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtNDRe").Specific;
                SAPbouiCOM.EditText myEditText_NomSolic = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtNSol").Specific;
                SAPbouiCOM.ComboBox myEditText_TipoDocSolic = (SAPbouiCOM.ComboBox)myForm.Items.Item("EA_cbTDSol").Specific;
                SAPbouiCOM.EditText myEditText_NumSolic = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtNDSo").Specific;

                motivo.tipoAnulacion = Int32.Parse(myEditText_TipoAnul.Selected.Value);
                motivo.motivoAnulacion = myEditText_Motivo.Value;
                motivo.nombreResponsable = myEditText_NomResp.Value;
                motivo.tipDocResponsable = myEditText_TipoDocResp.Selected.Value;
                motivo.numDocResponsable = myEditText_NumResp.Value;
                motivo.nombreSolicita = myEditText_NomSolic.Value;
                motivo.tipDocSolicita = myEditText_TipoDocSolic.Selected.Value;
                motivo.numDocSolicita = myEditText_NumSolic.Value;

                dtejson.motivo = motivo;

                //--------------------------------------------

                jsonFirmador.dteJson = dtejson;

                string jsonDTE = JsonConvert.SerializeObject(dtejson);//CONVERTIR OBJETO A JSON

                string json = JsonConvert.SerializeObject(jsonFirmador);//CONVERTIR OBJETO A JSON

                
                Console.WriteLine(json);
                WriteToFile("JSON PLANO ANULACION: " + json);

                jsonRespuesta = await consumoFirmador.postFirmador("http://" + serverDB + ":8113/firmardocumento/", json); //ENVIAR A SERVICIO DE FIRMADO

                Response response = JsonConvert.DeserializeObject<Response>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO

                if (response.status == "OK")
                {
                    Console.WriteLine("FIRMA: " + response.body.ToString());
                    
                    jsonRespuesta = await consumoFirmador.postAutenticador(emisorDTE.ambiente == "00" ? Constantes.urlAutenticadorTest : Constantes.urlAutenticador, emisorDTE.nit, emisorDTE.passPrivDTE);
                    ResponseAutenticador responseAutenticador = JsonConvert.DeserializeObject<ResponseAutenticador>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO
                    Console.WriteLine("JSON AUTENTICADOR: " + jsonRespuesta.ToString());
                    if (responseAutenticador.status == "OK")
                    {
                        Console.WriteLine("TOCKEN DE ACCESO: " + responseAutenticador.body.token);
                        //-------------------------------------------
                        jsonAnular.contentType = "application/JSON";
                        jsonAnular.ambiente = emisorDTE.ambiente;
                        jsonAnular.idEnvio = docNum;
                        jsonAnular.version = 02;
                        jsonAnular.documento= response.body.ToString();
                        string jsonanular = JsonConvert.SerializeObject(jsonAnular);//CONVERTIR OBJETO A JSON
                        Console.WriteLine("JSON ANULACION: " + jsonanular);
                        WriteToFile("JSON FIRMADO ANULACION: " + jsonanular);

                        jsonRespuesta = await consumoFirmador.postAnulacion(emisorDTE.ambiente == "00" ? Constantes.urlAnulacionTest : Constantes.urlAnulacion, responseAutenticador.body.token, jsonanular); //ENVIAR A SERVICIO DE ANULACION
                        ResponseAnular responseAnular = JsonConvert.DeserializeObject<ResponseAnular>(jsonRespuesta.ToString());
                        Console.WriteLine("JSON RESPUESTA ANULACION: " + jsonRespuesta);

                        WriteToFile("JSON RESPUESTA ANULACION: " + jsonRespuesta);

                        if (responseAnular.estado.Equals("PROCESADO"))
                         {
                             Console.WriteLine("Sello recibido: " + responseAnular.selloRecibido);
                             guardarAnulacion(responseAnular, docNum,dtejson.documento.tipoDte, jsonDTE, motivo);
                         }
                         else
                         {
                             mySBO_Application.MessageBox("La Anulacion no pudo ser procesada: \n\n" + jsonRespuesta);
                         }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error enviarAnulacion: " + e.Message);
            }
        }

        private void conectarDI() {
            int setConnectionContextReturn = 0;

            string sCookie = null;

            string sConnectionContext = null;

            //Inicializar Objeto Company
            mySBO_Company = new SAPbobsCOM.Company();

            //Obtener la cadena de contexto desde company
            sCookie = mySBO_Company.GetContextCookie();

            //Asignando el cookie de conexion desde Application
            sConnectionContext = mySBO_Application.Company.GetConnectionContext(sCookie);

            //Validar que no este conectado previamente
            if (mySBO_Company.Connected == true)
            {
                mySBO_Company.Disconnect();
            }

            //Establecer conexion a DI desde UI
            setConnectionContextReturn = mySBO_Company.SetSboLoginContext(sConnectionContext);

            if (setConnectionContextReturn != 0)
            {
                Console.WriteLine("Error al conectar DI API: " + mySBO_Company.GetLastErrorCode());
                mySBO_Application.StatusBar.SetText("Error al conectar DI API" + mySBO_Company.GetLastErrorCode(), SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                System.Environment.Exit(0);
            }
            else
            {
                mySBO_Application.StatusBar.SetText("Conexion DI API Exitosa", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
        }

        private void guardarDatosDTE(ResponseReceptor responseReceptor, int docNum, string numControl,int modeloFacturacion, int tipotransmision, string tipoDoc, string jsonDTE, string firma, string tipotrans) {
            try
            {
                
                int cod = -1;
                int option = 0;
                if (tipoDoc.Equals("05"))
                {
                    SAPbobsCOM.Documents oCreditMemo = null;

                    oCreditMemo = (SAPbobsCOM.Documents)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes);

                    oCreditMemo.GetByKey(consultas.obtenerDocEntryNC(docNum));

                    oCreditMemo.UserFields.Fields.Item("U_E_CODGENE").Value = responseReceptor.codigoGeneracion;
                    oCreditMemo.UserFields.Fields.Item("U_E_NUMCONT").Value = numControl;
                    oCreditMemo.UserFields.Fields.Item("U_E_SELLRECEP").Value = responseReceptor.selloRecibido;
                    oCreditMemo.UserFields.Fields.Item("U_E_MODFACT").Value = modeloFacturacion == 1 ? "Previo" : "Diferido";
                    oCreditMemo.UserFields.Fields.Item("U_E_TIPOTRANS").Value = tipotransmision == 1 ? "Normal" : "Contingencia";
                    oCreditMemo.UserFields.Fields.Item("U_E_FECHORA").Value = responseReceptor.fhProcesamiento;
                    oCreditMemo.UserFields.Fields.Item("U_JSON_DTE").Value = jsonDTE;
                    oCreditMemo.UserFields.Fields.Item("U_FIRMA_DTE").Value = firma;

                    cod = oCreditMemo.Update();

                    if (cod != 0)
                    {
                        Console.WriteLine("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode());
                        mySBO_Application.StatusBar.SetText("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    else
                    {
                        if (tipotrans == "N")
                        {
                            option = mySBO_Application.MessageBox("¿Enviar Correo al cliente?", 1, "Si", "No");

                            if (option == 1)
                            {
                                mySBO_Application.StatusBar.SetText("Datos de DTE Guardados Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                EnviarCorreoDTE_NC(docNum);
                            }
                        }
                        else
                        {
                            mySBO_Application.StatusBar.SetText("Datos de DTE Guardados Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                            EnviarCorreoDTE_NC(docNum);
                        }
                    }
                }
                else if (tipoDoc.Equals("04"))
                {
                    SAPbobsCOM.Documents oDeliveryNotes = null;
                    SAPbobsCOM.StockTransfer oStockTransfer = null;
                    //if (consultas.obtenerDocEntryNR(docNum) != 0)
                    //{

                    //    oDeliveryNotes = (SAPbobsCOM.Documents)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);

                    //    oDeliveryNotes.GetByKey(consultas.obtenerDocEntryNR(docNum));

                    //    oDeliveryNotes.UserFields.Fields.Item("U_E_CODGENE").Value = responseReceptor.codigoGeneracion;
                    //    oDeliveryNotes.UserFields.Fields.Item("U_E_NUMCONT").Value = numControl;
                    //    oDeliveryNotes.UserFields.Fields.Item("U_E_SELLRECEP").Value = responseReceptor.selloRecibido;
                    //    oDeliveryNotes.UserFields.Fields.Item("U_E_MODFACT").Value = modeloFacturacion == 1 ? "Previo" : "Diferido";
                    //    oDeliveryNotes.UserFields.Fields.Item("U_E_TIPOTRANS").Value = tipotransmision == 1 ? "Normal" : "Contingencia";
                    //    oDeliveryNotes.UserFields.Fields.Item("U_E_FECHORA").Value = responseReceptor.fhProcesamiento;
                    //    oDeliveryNotes.UserFields.Fields.Item("U_JSON_DTE").Value = jsonDTE;
                    //    oDeliveryNotes.UserFields.Fields.Item("U_FIRMA_DTE").Value = firma;

                    //    cod = oDeliveryNotes.Update();
                    //}
                    //else
                    //{

                    oStockTransfer = (SAPbobsCOM.StockTransfer)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);

                    oStockTransfer.GetByKey(consultas.obtenerDocEntryNR(docNum));

                    oStockTransfer.UserFields.Fields.Item("U_E_CODGENE").Value = responseReceptor.codigoGeneracion;
                    oStockTransfer.UserFields.Fields.Item("U_E_NUMCONT").Value = numControl;
                    oStockTransfer.UserFields.Fields.Item("U_E_SELLRECEP").Value = responseReceptor.selloRecibido;
                    oStockTransfer.UserFields.Fields.Item("U_E_MODFACT").Value = modeloFacturacion == 1 ? "Previo" : "Diferido";
                    oStockTransfer.UserFields.Fields.Item("U_E_TIPOTRANS").Value = tipotransmision == 1 ? "Normal" : "Contingencia";
                    oStockTransfer.UserFields.Fields.Item("U_E_FECHORA").Value = responseReceptor.fhProcesamiento;
                    oStockTransfer.UserFields.Fields.Item("U_JSON_DTE").Value = jsonDTE;
                    oStockTransfer.UserFields.Fields.Item("U_FIRMA_DTE").Value = firma;
                    cod = oStockTransfer.Update();
                    //}

                    if (cod != 0)
                    {
                        Console.WriteLine("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode());
                        mySBO_Application.StatusBar.SetText("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    else
                    {
                        if (tipotrans == "N")
                        {
                            option = mySBO_Application.MessageBox("¿Enviar Correo al cliente?", 1, "Si", "No");

                            if (option == 1)
                            {
                                mySBO_Application.StatusBar.SetText("Datos de DTE Guardados Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                EnviarCorreoDTE_NR(docNum);
                            }
                        }
                        else
                        {
                            mySBO_Application.StatusBar.SetText("Datos de DTE Guardados Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                            EnviarCorreoDTE_NR(docNum);
                        }
                    }
                }
                else if (tipoDoc.Equals("14"))
                {
                    SAPbobsCOM.Documents oPurchaseInvoices = null;

                    oPurchaseInvoices = (SAPbobsCOM.Documents)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseInvoices);

                    oPurchaseInvoices.GetByKey(consultas.obtenerDocEntryFSE(docNum));

                    oPurchaseInvoices.UserFields.Fields.Item("U_E_CODGENE").Value = responseReceptor.codigoGeneracion;
                    oPurchaseInvoices.UserFields.Fields.Item("U_E_NUMCONT").Value = numControl;
                    oPurchaseInvoices.UserFields.Fields.Item("U_E_SELLRECEP").Value = responseReceptor.selloRecibido;
                    oPurchaseInvoices.UserFields.Fields.Item("U_E_MODFACT").Value = modeloFacturacion == 1 ? "Previo" : "Diferido";
                    oPurchaseInvoices.UserFields.Fields.Item("U_E_TIPOTRANS").Value = tipotransmision == 1 ? "Normal" : "Contingencia";
                    oPurchaseInvoices.UserFields.Fields.Item("U_E_FECHORA").Value = responseReceptor.fhProcesamiento;
                    oPurchaseInvoices.UserFields.Fields.Item("U_JSON_DTE").Value = jsonDTE;
                    oPurchaseInvoices.UserFields.Fields.Item("U_FIRMA_DTE").Value = firma;

                    cod = oPurchaseInvoices.Update();

                    if (cod != 0)
                    {
                        Console.WriteLine("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode());
                        mySBO_Application.StatusBar.SetText("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    else
                    {
                        if (tipotrans == "N")
                        {
                            option = mySBO_Application.MessageBox("¿Enviar Correo al cliente?", 1, "Si", "No");

                            if (option == 1)
                            {
                                mySBO_Application.StatusBar.SetText("Datos de DTE Guardados Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                EnviarCorreoDTE_FSE(docNum);
                            }
                        }
                        else
                        {
                            mySBO_Application.StatusBar.SetText("Datos de DTE Guardados Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                            EnviarCorreoDTE_FSE(docNum);
                        }
                    }
                }
                else
                {
                    SAPbobsCOM.Documents oInvoice = null;

                    oInvoice = (SAPbobsCOM.Documents)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

                    oInvoice.GetByKey(consultas.obtenerDocEntryFAC(docNum));

                    oInvoice.UserFields.Fields.Item("U_E_CODGENE").Value = responseReceptor.codigoGeneracion;
                    oInvoice.UserFields.Fields.Item("U_E_NUMCONT").Value = numControl;
                    oInvoice.UserFields.Fields.Item("U_E_SELLRECEP").Value = responseReceptor.selloRecibido;
                    oInvoice.UserFields.Fields.Item("U_E_MODFACT").Value = modeloFacturacion == 1 ? "Previo" : "Diferido";
                    oInvoice.UserFields.Fields.Item("U_E_TIPOTRANS").Value = tipotransmision == 1 ? "Normal" : "Contingencia";
                    oInvoice.UserFields.Fields.Item("U_E_FECHORA").Value = responseReceptor.fhProcesamiento;
                    oInvoice.UserFields.Fields.Item("U_JSON_DTE").Value = jsonDTE;
                    oInvoice.UserFields.Fields.Item("U_FIRMA_DTE").Value = firma;

                    cod = oInvoice.Update();

                    if (cod != 0)
                    {
                        Console.WriteLine("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode());
                        mySBO_Application.StatusBar.SetText("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    else
                    {
                        if (tipotrans == "N")
                        {
                            mySBO_Application.StatusBar.SetText("Datos de DTE Guardados Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);


                            option = mySBO_Application.MessageBox("¿Enviar Correo al cliente?", 1, "Si", "No");

                            if (option == 1)
                            {
                                switch (tipoDoc)
                                {
                                    case "01":
                                        EnviarCorreoDTE_FAC(docNum);
                                        break;
                                    case "03":
                                        EnviarCorreoDTE_CCF(docNum);
                                        break;
                                    case "11":
                                        EnviarCorreoDTE_FEX(docNum);
                                        break;
                                        /*case "06":
                                            EnviarCorreoDTE_FEX(docNum);
                                            break;*/
                                }
                            }
                        }
                        else
                        {
                            mySBO_Application.StatusBar.SetText("Datos de DTE Guardados Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                            switch (tipoDoc)
                            {
                                case "01":
                                    EnviarCorreoDTE_FAC(docNum);
                                    break;
                                case "03":
                                    EnviarCorreoDTE_CCF(docNum);
                                    break;
                                case "11":
                                    EnviarCorreoDTE_FEX(docNum);
                                    break;
                                    /*case "06":
                                        EnviarCorreoDTE_FEX(docNum);
                                        break;*/
                            }
                        }
                    }
                }
                mySBO_Application.MessageBox("Facturas transmitidas correctamente");

            }
            catch (Exception e)
            {
                mySBO_Application.StatusBar.SetText("Error al guardar DTE de factura: " + e.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void guardarCodigoLoteDTE(int docNum,string codigoLote, string tipoDoc)
        {
            try
            {

                int cod = -1;
                int option = 0;
                //conectarDI();
                //if (mySBO_Company.Connect() != 0)
                //{
                //    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                //}
                //else
                //{
                if (tipoDoc.Equals("05"))
                {
                    SAPbobsCOM.Documents oCreditMemo = null;

                    oCreditMemo = (SAPbobsCOM.Documents)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes);

                    oCreditMemo.GetByKey(consultas.obtenerDocEntryNC(docNum));

                    oCreditMemo.UserFields.Fields.Item("U_E_codigoLote").Value = codigoLote;

                    cod = oCreditMemo.Update();

                    if (cod != 0)
                    {
                        Console.WriteLine("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode());
                        mySBO_Application.StatusBar.SetText("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    else
                    {
                        option = mySBO_Application.MessageBox("¿Enviar Correo al cliente?", 1, "Si", "No");

                        if (option == 1)
                        {
                            mySBO_Application.StatusBar.SetText("Datos de DTE Guardados Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        }
                    }
                }
                else if (tipoDoc.Equals("04"))
                {
                    SAPbobsCOM.StockTransfer stockTransfer = null;

                    stockTransfer = (SAPbobsCOM.StockTransfer)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);

                    stockTransfer.GetByKey(consultas.obtenerDocEntryNR(docNum));

                    stockTransfer.UserFields.Fields.Item("U_E_codigoLote").Value = codigoLote;

                    cod = stockTransfer.Update();

                    if (cod != 0)
                    {
                        Console.WriteLine("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode());
                        mySBO_Application.StatusBar.SetText("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    else
                    {
                        option = mySBO_Application.MessageBox("¿Enviar Correo al cliente?", 1, "Si", "No");

                        if (option == 1)
                        {
                            mySBO_Application.StatusBar.SetText("Datos de DTE Guardados Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                            EnviarCorreoDTE_NR(docNum);
                        }
                    }
                }
                else if (tipoDoc.Equals("14"))
                {
                    SAPbobsCOM.Documents oPurchaseInvoices = null;

                    oPurchaseInvoices = (SAPbobsCOM.Documents)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseInvoices);

                    oPurchaseInvoices.GetByKey(consultas.obtenerDocEntryFSE(docNum));

                    oPurchaseInvoices.UserFields.Fields.Item("U_E_codigoLote").Value = codigoLote;

                    cod = oPurchaseInvoices.Update();

                    if (cod != 0)
                    {
                        Console.WriteLine("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode());
                        mySBO_Application.StatusBar.SetText("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    else
                    {
                        option = mySBO_Application.MessageBox("¿Enviar Correo al cliente?", 1, "Si", "No");

                        if (option == 1)
                        {
                            mySBO_Application.StatusBar.SetText("Datos de DTE Guardados Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        }
                    }
                }
                else
                {
                    SAPbobsCOM.Documents oInvoice = null;

                    oInvoice = (SAPbobsCOM.Documents)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

                    oInvoice.GetByKey(consultas.obtenerDocEntryFAC(docNum));

                    oInvoice.UserFields.Fields.Item("U_E_codigoLote").Value = codigoLote;

                    cod = oInvoice.Update();

                    if (cod != 0)
                    {
                        Console.WriteLine("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode());
                        mySBO_Application.StatusBar.SetText("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    else
                    {
                        mySBO_Application.StatusBar.SetText("Datos de DTE Guardados Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                    }
                }
                //}
            }
            catch (Exception e)
            {
                mySBO_Application.StatusBar.SetText("Error al guardar DTE de factura: " + e.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                //mySBO_Company.Disconnect();
            }
        }

        private void guardarDatosContingenciaDTE(ResponseContingencia responseContingencia, int docNum, string codeContinencia, string tipoDoc, string json)
        {
            try
            {

                int cod = -1;
                int option = 0;
                //conectarDI();
                //if (mySBO_Company.Connect() != 0)
                //{
                //    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                //}
                //else
                //{
                if (tipoDoc.Equals("05"))
                {
                    SAPbobsCOM.Documents oCreditMemo = null;

                    oCreditMemo = (SAPbobsCOM.Documents)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes);

                    oCreditMemo.GetByKey(consultas.obtenerDocEntryNC(docNum));

                    oCreditMemo.UserFields.Fields.Item("U_E_SELLCONTI").Value = responseContingencia.selloRecibido;

                    cod = oCreditMemo.Update();
                    if (cod != 0)
                    {
                        Console.WriteLine("Error al guardar datos de coningencia: " + mySBO_Company.GetLastErrorCode());
                        mySBO_Application.StatusBar.SetText("Error al guardar datos de coningencia : " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                }
                else if (tipoDoc.Equals("04"))
                {
                    SAPbobsCOM.StockTransfer oStockTransfer = null;

                    oStockTransfer = (SAPbobsCOM.StockTransfer)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);

                    oStockTransfer.GetByKey(consultas.obtenerDocEntryNR(docNum));

                    oStockTransfer.UserFields.Fields.Item("U_E_SELLCONTI").Value = responseContingencia.selloRecibido;
                    cod = oStockTransfer.Update();
                    if (cod != 0)
                    {
                        Console.WriteLine("Error al guardar datos de coningencia: " + mySBO_Company.GetLastErrorCode());
                        mySBO_Application.StatusBar.SetText("Error al guardar datos de coningencia : " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }

                }
                else if (tipoDoc.Equals("14"))
                {
                    SAPbobsCOM.Documents oPurchaseInvoices = null;

                    oPurchaseInvoices = (SAPbobsCOM.Documents)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseInvoices);

                    oPurchaseInvoices.GetByKey(consultas.obtenerDocEntryFSE(docNum));

                    oPurchaseInvoices.UserFields.Fields.Item("U_E_SELLCONTI").Value = responseContingencia.selloRecibido;

                    cod = oPurchaseInvoices.Update();
                    if (cod != 0)
                    {
                        Console.WriteLine("Error al guardar datos de coningencia: " + mySBO_Company.GetLastErrorCode());
                        mySBO_Application.StatusBar.SetText("Error al guardar datos de coningencia : " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                }
                else
                {
                    SAPbobsCOM.Documents oInvoice = null;

                    oInvoice = (SAPbobsCOM.Documents)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

                    oInvoice.GetByKey(consultas.obtenerDocEntryFAC(docNum));

                    oInvoice.UserFields.Fields.Item("U_E_SELLCONTI").Value = responseContingencia.selloRecibido;

                    cod = oInvoice.Update();
                    if (cod != 0)
                    {
                        Console.WriteLine("Error al guardar datos de coningencia: " + mySBO_Company.GetLastErrorCode());
                        mySBO_Application.StatusBar.SetText("Error al guardar datos de coningencia : " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                }

                SAPbobsCOM.UserTable oUserTable = mySBO_Company.UserTables.Item("FE_CONTINGENCIA");
                oUserTable.GetByKey(codeContinencia);
                oUserTable.UserFields.Fields.Item("U_E_SELLRECEP").Value = responseContingencia.selloRecibido;
                oUserTable.UserFields.Fields.Item("U_JSONcontingencia").Value = json;
                cod=oUserTable.Update();
                if (cod != 0)
                {
                    Console.WriteLine("Error al guardar datos de coningencia: " + mySBO_Company.GetLastErrorCode());
                    mySBO_Application.StatusBar.SetText("Error al guardar datos de coningencia : " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                }
                //}
            }
            catch (Exception e)
            {
                mySBO_Application.StatusBar.SetText("Error al guardar coningencia de factura: " + e.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                //mySBO_Company.Disconnect();
            }
        }

        private void guardarDatosLoteDTE(ResponseReceptor responseReceptor, int docNum, string numControl, int modeloFacturacion, int tipotransmision, string tipoDoc, string jsonDTE, string firma, string codigoLote)
        {
            try
            {

                int cod = -1;
                int option = 0;
                //conectarDI();
                //if (mySBO_Company.Connect() != 0)
                //{
                //    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                //}
                //else
                //{
                    if (tipoDoc.Equals("05"))
                    {
                        SAPbobsCOM.Documents oCreditMemo = null;

                        oCreditMemo = (SAPbobsCOM.Documents)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes);

                        oCreditMemo.GetByKey(consultas.obtenerDocEntryNC(docNum));

                        oCreditMemo.UserFields.Fields.Item("U_E_CODGENE").Value = responseReceptor.codigoGeneracion;
                        oCreditMemo.UserFields.Fields.Item("U_E_NUMCONT").Value = numControl;
                        oCreditMemo.UserFields.Fields.Item("U_E_SELLRECEP").Value = responseReceptor.selloRecibido;
                        oCreditMemo.UserFields.Fields.Item("U_E_MODFACT").Value = modeloFacturacion == 1 ? "Previo" : "Diferido";
                        oCreditMemo.UserFields.Fields.Item("U_E_TIPOTRANS").Value = tipotransmision == 1 ? "Normal" : "Contingencia";
                        oCreditMemo.UserFields.Fields.Item("U_E_FECHORA").Value = responseReceptor.fhProcesamiento;
                        oCreditMemo.UserFields.Fields.Item("U_JSON_DTE").Value = jsonDTE;
                        oCreditMemo.UserFields.Fields.Item("U_FIRMA_DTE").Value = firma;
                        oCreditMemo.UserFields.Fields.Item("U_E_codigoLote").Value = codigoLote;

                        cod = oCreditMemo.Update();

                        if (cod != 0)
                        {
                            Console.WriteLine("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode());
                            mySBO_Application.StatusBar.SetText("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        }
                        else
                        {
                            option = mySBO_Application.MessageBox("¿Enviar Correo al cliente?", 1, "Si", "No");

                            if (option == 1)
                            {
                                mySBO_Application.StatusBar.SetText("Datos de DTE Guardados Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                EnviarCorreoDTE_NC(docNum);
                            }
                        }
                    }
                    else if (tipoDoc.Equals("04"))
                    {
                        SAPbobsCOM.StockTransfer oStockTransfer = null;

                        oStockTransfer = (SAPbobsCOM.StockTransfer)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);

                        oStockTransfer.GetByKey(consultas.obtenerDocEntryNR(docNum));

                        oStockTransfer.UserFields.Fields.Item("U_E_CODGENE").Value = responseReceptor.codigoGeneracion;
                        oStockTransfer.UserFields.Fields.Item("U_E_NUMCONT").Value = numControl;
                        oStockTransfer.UserFields.Fields.Item("U_E_SELLRECEP").Value = responseReceptor.selloRecibido;
                        oStockTransfer.UserFields.Fields.Item("U_E_MODFACT").Value = modeloFacturacion == 1 ? "Previo" : "Diferido";
                        oStockTransfer.UserFields.Fields.Item("U_E_TIPOTRANS").Value = tipotransmision == 1 ? "Normal" : "Contingencia";
                        oStockTransfer.UserFields.Fields.Item("U_E_FECHORA").Value = responseReceptor.fhProcesamiento;
                        oStockTransfer.UserFields.Fields.Item("U_JSON_DTE").Value = jsonDTE;
                        oStockTransfer.UserFields.Fields.Item("U_FIRMA_DTE").Value = firma;
                        cod = oStockTransfer.Update();

                    if (cod != 0)
                        {
                            Console.WriteLine("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode());
                            mySBO_Application.StatusBar.SetText("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        }
                        else
                        {
                            option = mySBO_Application.MessageBox("¿Enviar Correo al cliente?", 1, "Si", "No");

                            if (option == 1)
                            {
                                mySBO_Application.StatusBar.SetText("Datos de DTE Guardados Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                EnviarCorreoDTE_NR(docNum);
                            }
                        }
                    }
                    else if (tipoDoc.Equals("14"))
                    {
                        SAPbobsCOM.Documents oPurchaseInvoices = null;

                        oPurchaseInvoices = (SAPbobsCOM.Documents)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseInvoices);

                        oPurchaseInvoices.GetByKey(consultas.obtenerDocEntryFSE(docNum));

                        oPurchaseInvoices.UserFields.Fields.Item("U_E_CODGENE").Value = responseReceptor.codigoGeneracion;
                        oPurchaseInvoices.UserFields.Fields.Item("U_E_NUMCONT").Value = numControl;
                        oPurchaseInvoices.UserFields.Fields.Item("U_E_SELLRECEP").Value = responseReceptor.selloRecibido;
                        oPurchaseInvoices.UserFields.Fields.Item("U_E_MODFACT").Value = modeloFacturacion == 1 ? "Previo" : "Diferido";
                        oPurchaseInvoices.UserFields.Fields.Item("U_E_TIPOTRANS").Value = tipotransmision == 1 ? "Normal" : "Contingencia";
                        oPurchaseInvoices.UserFields.Fields.Item("U_E_FECHORA").Value = responseReceptor.fhProcesamiento;
                        oPurchaseInvoices.UserFields.Fields.Item("U_JSON_DTE").Value = jsonDTE;
                        oPurchaseInvoices.UserFields.Fields.Item("U_FIRMA_DTE").Value = firma;
                        oPurchaseInvoices.UserFields.Fields.Item("U_E_codigoLote").Value = codigoLote;

                        cod = oPurchaseInvoices.Update();

                        if (cod != 0)
                        {
                            Console.WriteLine("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode());
                            mySBO_Application.StatusBar.SetText("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        }
                        else
                        {
                            option = mySBO_Application.MessageBox("¿Enviar Correo al cliente?", 1, "Si", "No");

                            if (option == 1)
                            {
                                mySBO_Application.StatusBar.SetText("Datos de DTE Guardados Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                EnviarCorreoDTE_FSE(docNum);
                            }
                        }
                    }
                    else
                    {
                        SAPbobsCOM.Documents oInvoice = null;

                        oInvoice = (SAPbobsCOM.Documents)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

                        oInvoice.GetByKey(consultas.obtenerDocEntryFAC(docNum));

                        oInvoice.UserFields.Fields.Item("U_E_CODGENE").Value = responseReceptor.codigoGeneracion;
                        oInvoice.UserFields.Fields.Item("U_E_NUMCONT").Value = numControl;
                        oInvoice.UserFields.Fields.Item("U_E_SELLRECEP").Value = responseReceptor.selloRecibido;
                        oInvoice.UserFields.Fields.Item("U_E_MODFACT").Value = modeloFacturacion == 1 ? "Previo" : "Diferido";
                        oInvoice.UserFields.Fields.Item("U_E_TIPOTRANS").Value = tipotransmision == 1 ? "Normal" : "Contingencia";
                        oInvoice.UserFields.Fields.Item("U_E_FECHORA").Value = responseReceptor.fhProcesamiento;
                        oInvoice.UserFields.Fields.Item("U_JSON_DTE").Value = jsonDTE;
                        oInvoice.UserFields.Fields.Item("U_FIRMA_DTE").Value = firma;
                        oInvoice.UserFields.Fields.Item("U_E_codigoLote").Value = codigoLote;

                        cod = oInvoice.Update();

                        if (cod != 0)
                        {
                            Console.WriteLine("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode());
                            mySBO_Application.StatusBar.SetText("Error al guardar datos DTE en factura: " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        }
                        else
                        {
                            mySBO_Application.StatusBar.SetText("Datos de DTE Guardados Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);


                            //option = mySBO_Application.MessageBox("¿Enviar Correo al cliente?", 1, "Si", "No");

                            //if (option == 1)
                            //{
                                switch (tipoDoc)
                                {
                                    case "01":
                                        EnviarCorreoDTE_FAC(docNum);
                                        break;
                                    case "03":
                                        EnviarCorreoDTE_CCF(docNum);
                                        break;
                                    case "11":
                                        EnviarCorreoDTE_FEX(docNum);
                                        break;
                                        /*case "06":
                                            EnviarCorreoDTE_FEX(docNum);
                                            break;*/
                                }
                            //}
                        }
                    }
                //}
            }
            catch (Exception e)
            {
                mySBO_Application.StatusBar.SetText("Error al guardar DTE de factura: " + e.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                //mySBO_Company.Disconnect();
            }
        }

        private void guardarAnulacion(ResponseAnular responseAnular, int docNum, string tipoDoc, string jsonDTE, Motivo motivo)
        {
            try
            {
                
                int cod = -1;
                conectarDI();
                if (mySBO_Company.Connect() != 0)
                {
                    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                }
                else
                {
                    SAPbobsCOM.UserTable oUserTable = mySBO_Company.UserTables.Item("FE_ANULACION");

                    if (tipoDoc.Equals("05"))
                    {
                        SAPbobsCOM.Documents oCreditMemo = null;

                        oCreditMemo = (SAPbobsCOM.Documents)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes);

                        oCreditMemo.GetByKey(consultas.obtenerDocEntryNC(docNum));

                        oCreditMemo.UserFields.Fields.Item("U_E_SELLCANCELED").Value = responseAnular.selloRecibido;

                        cod = oCreditMemo.Update();

                        if (cod != 0)
                        {
                            Console.WriteLine("Error al guardar Sello anulacion: " + mySBO_Company.GetLastErrorCode());
                            mySBO_Application.StatusBar.SetText("Error al guardar Sello anulacion: " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        }
                        else
                        {
                            mySBO_Application.StatusBar.SetText("Datos de Sello anulacion Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        }
                    }
                    else if (tipoDoc.Equals("04"))
                    {
                        SAPbobsCOM.StockTransfer stockTransfer = null;

                        stockTransfer = (SAPbobsCOM.StockTransfer)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);

                        stockTransfer.GetByKey(consultas.obtenerDocEntryNR(docNum));

                        stockTransfer.UserFields.Fields.Item("U_E_SELLCANCELED").Value = responseAnular.selloRecibido;

                        cod = stockTransfer.Update();

                        if (cod != 0)
                        {
                            Console.WriteLine("Error al guardar Sello anulacion: " + mySBO_Company.GetLastErrorCode());
                            mySBO_Application.StatusBar.SetText("Error al guardar Sello anulacion: " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        }
                        else
                        {
                            mySBO_Application.StatusBar.SetText("Datos de Sello anulacion Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        }
                    }
                    else if (tipoDoc.Equals("14"))
                    {
                        SAPbobsCOM.Documents oPurchaseInvoices = null;

                        oPurchaseInvoices = (SAPbobsCOM.Documents)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseInvoices);

                        oPurchaseInvoices.GetByKey(consultas.obtenerDocEntryFSE(docNum));

                        oPurchaseInvoices.UserFields.Fields.Item("U_E_SELLCANCELED").Value = responseAnular.selloRecibido;

                        cod = oPurchaseInvoices.Update();

                        if (cod != 0)
                        {
                            Console.WriteLine("Error al guardar Sello anulacion: " + mySBO_Company.GetLastErrorCode());
                            mySBO_Application.StatusBar.SetText("Error al guardar Sello anulacion: " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        }
                        else
                        {
                            mySBO_Application.StatusBar.SetText("Datos de Sello anulacion Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        }
                    }
                    else
                    {
                        SAPbobsCOM.Documents oInvoice = null;

                        oInvoice = (SAPbobsCOM.Documents)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

                        oInvoice.GetByKey(consultas.obtenerDocEntryFAC(docNum));
                        oInvoice.UserFields.Fields.Item("U_E_SELLCANCELED").Value = responseAnular.selloRecibido;
                        cod = oInvoice.Update();

                        if (cod != 0)
                        {
                            Console.WriteLine("Error al guardar Sello anulacion: " + mySBO_Company.GetLastErrorCode());
                            mySBO_Application.StatusBar.SetText("Error al guardar Sello anulacion: " + mySBO_Company.GetLastErrorCode() + " " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        }
                        else
                        {
                            mySBO_Application.StatusBar.SetText("Datos de Sello anulacion Correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                        }
                    }

                    //------------------------------------------------------------------------------------

                    oUserTable.Code = responseAnular.codigoGeneracion.ToString();
                    oUserTable.Name = responseAnular.codigoGeneracion.ToString();
                    oUserTable.UserFields.Fields.Item("U_E_SELLRECEP").Value = responseAnular.selloRecibido.ToString();
                    oUserTable.UserFields.Fields.Item("U_E_CODGENE").Value = responseAnular.codigoGeneracion.ToString();
                    oUserTable.UserFields.Fields.Item("U_tipoAnulacion").Value = motivo.tipoAnulacion.ToString();
                    oUserTable.UserFields.Fields.Item("U_motivoAnulacion").Value = motivo.motivoAnulacion.ToString();
                    oUserTable.UserFields.Fields.Item("U_nombreResp").Value = motivo.nombreResponsable.ToString();
                    oUserTable.UserFields.Fields.Item("U_tipDocResp").Value = motivo.tipDocResponsable.ToString();
                    oUserTable.UserFields.Fields.Item("U_numDocResp").Value = motivo.numDocResponsable.ToString();
                    oUserTable.UserFields.Fields.Item("U_nombreSolic").Value = motivo.nombreSolicita.ToString();
                    oUserTable.UserFields.Fields.Item("U_tipDocSolic").Value = motivo.tipDocSolicita.ToString();
                    oUserTable.UserFields.Fields.Item("U_numDocSolic").Value = motivo.numDocSolicita.ToString();
                    oUserTable.UserFields.Fields.Item("U_JSONanulacion").Value = jsonDTE.ToString();
                    oUserTable.UserFields.Fields.Item("U_tipoDoc").Value = tipoDoc.ToString();
                    oUserTable.UserFields.Fields.Item("U_docNum").Value = docNum.ToString();
                    
                    if (oUserTable.Add() != 0)
                    {
                        mySBO_Application.StatusBar.SetText("Error al guardar informacion de anulacion: " + mySBO_Company.GetLastErrorCode() + " - " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    else
                    {
                        mySBO_Application.StatusBar.SetText("Informacion de anulacion Guardada", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    }
                }
            }
            catch (Exception e)
            {
                mySBO_Application.StatusBar.SetText("Error al guardar anulacion: " + e.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                mySBO_Company.Disconnect();
            }
        }

        private void EnviarCorreoDTE_FAC(int docNum)
        {
            try
            {
                Cliente cliente = new Cliente();
                EmisorDTE emisorDTE = new EmisorDTE();
                jsonFirmadorFE jsonFirmadorFE = new jsonFirmadorFE();

                emisorDTE = consultas.obtenerEmisor();
                cliente = consultas.obtenerInfoClienteFAC(docNum);
                String nombreDoc = consultas.nombreFAC_PDF(docNum);

                jsonFirmadorFE = obtenerJSONFE(cliente,docNum);
                jsonFirmadorFE.dteJson.identificacion.fecEmi = cliente.docDate;
                jsonFirmadorFE.dteJson.identificacion.codigoGeneracion = consultas.nombreFAC_PDF(docNum).Substring(0, 36);

                string fileName = Path.GetTempPath() + nombreDoc + ".json";
                string jsonString = JsonConvert.SerializeObject(jsonFirmadorFE.dteJson);

                if(!File.Exists(fileName))
                    File.WriteAllText(fileName, jsonString);

                crystalReport = new ReportDocument();
                crystalReport.Load(Application.StartupPath + @"\Reportes\DTE_FE.rpt");
                crystalReport.SetDatabaseLogon(userDB, passDB);
                crystalReport.SetParameterValue("DocKey@", consultas.obtenerDocEntryFAC(docNum));
                MailMessage mailMessage = new MailMessage(emisorDTE.correoEnvio,
                                                            cliente.correo,
                                                            "DOCUMENTO DTE: " + nombreDoc,
                                                            "<h1>ESTIMADO(A) CLIENTE: </h1>" +
                                                            "<h1><b>" + cliente.nombre + "</b></h1>" +
                                                            "<h3>POR ESTE MEDIO COMPARTIMOS SU FACTURA.</h3>");
                mailMessage.IsBodyHtml = true;
                mailMessage.Attachments.Add(new Attachment(crystalReport.ExportToStream(ExportFormatType.PortableDocFormat), nombreDoc + ".pdf"));
                mailMessage.Attachments.Add(new Attachment(fileName));
                SmtpClient smtpClient = new SmtpClient(emisorDTE.smtp);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Port = int.Parse(emisorDTE.puertoCorreo);
                smtpClient.Credentials = new System.Net.NetworkCredential(emisorDTE.correoEnvio, emisorDTE.contraCorreo);
                smtpClient.Send(mailMessage);
                smtpClient.Dispose();
                mySBO_Application.StatusBar.SetText("Correo Enviado correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error al Enviar DTE al receptor: " + e.Message);
                mySBO_Application.StatusBar.SetText("Error al Enviar DTE al receptor: " + e.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);

            }

        }

        private void EnviarCorreoDTE_CCF(int docNum)
        {
            try
            {
                Cliente cliente = new Cliente();
                EmisorDTE emisorDTE = new EmisorDTE();
                jsonFirmadorCCF jsonFirmadorCCF = new jsonFirmadorCCF();

                emisorDTE = consultas.obtenerEmisor();

                cliente = consultas.obtenerInfoClienteFAC(docNum);
                String nombreDoc = consultas.nombreFAC_PDF(docNum);

                jsonFirmadorCCF = obtenerJSONCCF(cliente, docNum);
                jsonFirmadorCCF.dteJson.identificacion.fecEmi = cliente.docDate;
                jsonFirmadorCCF.dteJson.identificacion.codigoGeneracion = consultas.nombreFAC_PDF(docNum).Substring(0, 36);

                string fileName = Path.GetTempPath() + nombreDoc + ".json";
                string jsonString = JsonConvert.SerializeObject(jsonFirmadorCCF.dteJson);

                if (!File.Exists(fileName))
                    File.WriteAllText(fileName, jsonString);

                crystalReport = new ReportDocument();
                crystalReport.Load(Application.StartupPath + @"\Reportes\DTE_CCF.rpt");
                crystalReport.SetDatabaseLogon(userDB, passDB);
                crystalReport.SetParameterValue("DocKey@", consultas.obtenerDocEntryFAC(docNum));
                MailMessage mailMessage = new MailMessage(emisorDTE.correoEnvio,
                                                          cliente.correo,
                                                          "DOCUMENTO DTE: " + nombreDoc,
                                                          "<h1>ESTIMADO(A) CLIENTE: </h1>" +
                                                         "<h1><b>"+cliente.nombre+"</b></h1>" +
                                                         "<h3>POR ESTE MEDIO COMPARTIMOS SU FACTURA.</h3>");
                mailMessage.IsBodyHtml = true;
                mailMessage.Attachments.Add(new Attachment(crystalReport.ExportToStream(ExportFormatType.PortableDocFormat), nombreDoc + ".pdf"));
                mailMessage.Attachments.Add(new Attachment(fileName));
                SmtpClient smtpClient = new SmtpClient(emisorDTE.smtp);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Port = int.Parse(emisorDTE.puertoCorreo);
                smtpClient.Credentials = new System.Net.NetworkCredential(emisorDTE.correoEnvio, emisorDTE.contraCorreo);
                smtpClient.Send(mailMessage);
                smtpClient.Dispose();
                mySBO_Application.StatusBar.SetText("Correo Enviado correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

            }
            catch (Exception e)
            {
                Console.WriteLine("Error al Enviar DTE al receptor: " + e.Message);
                mySBO_Application.StatusBar.SetText("Error al Enviar DTE al receptor: " + e.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);

            }
        }

        private void EnviarCorreoDTE_NC(int docNum)
        {
            try
            {
                Cliente cliente = new Cliente();
                EmisorDTE emisorDTE = new EmisorDTE();
                jsonFirmadorNC jsonFirmadorNC = new jsonFirmadorNC();

                emisorDTE = consultas.obtenerEmisor();
                cliente = consultas.obtenerInfoClienteNC(docNum);
                String nombreDoc = consultas.nombreNC_PDF(docNum);

                jsonFirmadorNC = obtenerJSONNC(cliente, docNum);
                jsonFirmadorNC.dteJson.identificacion.fecEmi = cliente.docDate;
                jsonFirmadorNC.dteJson.identificacion.codigoGeneracion = consultas.nombreNC_PDF(docNum).Substring(0, 36);

                string fileName = Path.GetTempPath() + nombreDoc + ".json";
                string jsonString = JsonConvert.SerializeObject(jsonFirmadorNC.dteJson);

                if (!File.Exists(fileName))
                    File.WriteAllText(fileName, jsonString);

                crystalReport = new ReportDocument();
                crystalReport.Load(Application.StartupPath + @"\Reportes\DTE_NC.rpt");
                crystalReport.SetDatabaseLogon(userDB, passDB);
                crystalReport.SetParameterValue("DocKey@", consultas.obtenerDocEntryNC(docNum));
                MailMessage mailMessage = new MailMessage(emisorDTE.correoEnvio,
                                                          cliente.correo,
                                                          "DOCUMENTO DTE: " + nombreDoc,
                                                          "<h1>ESTIMADO(A) CLIENTE: </h1>" +
                                                         "<h1><b>" + cliente.nombre + "</b></h1>" +
                                                         "<h3>POR ESTE MEDIO COMPARTIMOS SU NOTA DE CREDITO.</h3>");
                mailMessage.IsBodyHtml = true;
                mailMessage.Attachments.Add(new Attachment(crystalReport.ExportToStream(ExportFormatType.PortableDocFormat), nombreDoc + ".pdf"));
                mailMessage.Attachments.Add(new Attachment(fileName));
                SmtpClient smtpClient = new SmtpClient(emisorDTE.smtp);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Port = int.Parse(emisorDTE.puertoCorreo);
                smtpClient.Credentials = new System.Net.NetworkCredential(emisorDTE.correoEnvio, emisorDTE.contraCorreo);
                smtpClient.Send(mailMessage);
                smtpClient.Dispose();
                mySBO_Application.StatusBar.SetText("Correo Enviado correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error al Enviar DTE al receptor: " + e.Message);
                mySBO_Application.StatusBar.SetText("Error al Enviar DTE al receptor: " + e.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);

            }

        }

        private void EnviarCorreoDTE_NR(int docNum)
        {
            try
            {
                Cliente cliente = new Cliente();
                EmisorDTE emisorDTE = new EmisorDTE();
                jsonFirmadorNR jsonFirmadorNR = new jsonFirmadorNR();

                emisorDTE = consultas.obtenerEmisor();
                cliente = consultas.obtenerInfoClienteNR(docNum);
                String nombreDoc = consultas.nombreNR_PDF(docNum);

                jsonFirmadorNR = obtenerJSONNR(cliente, docNum);
                jsonFirmadorNR.dteJson.identificacion.fecEmi = cliente.docDate;
                jsonFirmadorNR.dteJson.identificacion.codigoGeneracion = consultas.nombreNR_PDF(docNum).Substring(0, 36);

                string fileName = Path.GetTempPath() + nombreDoc + ".json";
                string jsonString = JsonConvert.SerializeObject(jsonFirmadorNR.dteJson);

                if (!File.Exists(fileName))
                    File.WriteAllText(fileName, jsonString);

                crystalReport = new ReportDocument();
                crystalReport.Load(Application.StartupPath + @"\Reportes\DTE_NR.rpt");
                crystalReport.SetDatabaseLogon(userDB, passDB);
                crystalReport.SetParameterValue("DocKey@", consultas.obtenerDocEntryNR(docNum));
                MailMessage mailMessage = new MailMessage(emisorDTE.correoEnvio,
                                                          cliente.correo,
                                                          "DOCUMENTO DTE: " + nombreDoc,
                                                          "<h1>ESTIMADO(A) CLIENTE: </h1>" +
                                                         "<h1><b>" + cliente.nombre + "</b></h1>" +
                                                         "<h3>POR ESTE MEDIO COMPARTIMOS SU NOTA DE REMISION.</h3>");
                mailMessage.IsBodyHtml = true;
                mailMessage.Attachments.Add(new Attachment(crystalReport.ExportToStream(ExportFormatType.PortableDocFormat), nombreDoc + ".pdf"));
                mailMessage.Attachments.Add(new Attachment(fileName));
                SmtpClient smtpClient = new SmtpClient(emisorDTE.smtp);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Port = int.Parse(emisorDTE.puertoCorreo);
                smtpClient.Credentials = new System.Net.NetworkCredential(emisorDTE.correoEnvio, emisorDTE.contraCorreo);
                smtpClient.Send(mailMessage);
                smtpClient.Dispose();
                mySBO_Application.StatusBar.SetText("Correo Enviado correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error al Enviar DTE al receptor: " + e.Message);
                mySBO_Application.StatusBar.SetText("Error al Enviar DTE al receptor: " + e.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);

            }

        }

        private void EnviarCorreoDTE_FEX(int docNum)
        {
            try
            {
                Cliente cliente = new Cliente();
                EmisorDTE emisorDTE = new EmisorDTE();
                jsonFirmadorFEX jsonFirmadorFEX = new jsonFirmadorFEX();

                emisorDTE = consultas.obtenerEmisor();
                cliente = consultas.obtenerInfoClienteFAC(docNum);
                String nombreDoc = consultas.nombreFAC_PDF(docNum);
                String pais = consultas.paisFAC(docNum);


                jsonFirmadorFEX = obtenerJSONFEX(cliente, docNum);
                jsonFirmadorFEX.dteJson.identificacion.fecEmi = cliente.docDate;
                jsonFirmadorFEX.dteJson.identificacion.codigoGeneracion = consultas.nombreFAC_PDF(docNum).Substring(0, 36);

                string fileName = Path.GetTempPath() + nombreDoc + ".json";
                string jsonString = JsonConvert.SerializeObject(jsonFirmadorFEX.dteJson);

                if (!File.Exists(fileName))
                    File.WriteAllText(fileName, jsonString);

                crystalReport = new ReportDocument();
                if(pais== "9450")
                    crystalReport.Load(Application.StartupPath + @"\Reportes\DTE_FEX_EN.rpt");
                else
                    crystalReport.Load(Application.StartupPath + @"\Reportes\DTE_FEX_ES.rpt");
                crystalReport.SetDatabaseLogon(userDB, passDB);
                crystalReport.SetParameterValue("DocKey@", consultas.obtenerDocEntryFAC(docNum));
                MailMessage mailMessage = new MailMessage(emisorDTE.correoEnvio,
                                                          cliente.correo,
                                                          "DOCUMENTO DTE: " + nombreDoc,
                                                          "<h1>ESTIMADO(A) CLIENTE: </h1>" +
                                                         "<h1><b>" + cliente.nombre + "</b></h1>" +
                                                         "<h3>POR ESTE MEDIO COMPARTIMOS SU FACTURA DE EXPORTACION.</h3>");
                mailMessage.IsBodyHtml = true;
                mailMessage.Attachments.Add(new Attachment(crystalReport.ExportToStream(ExportFormatType.PortableDocFormat), nombreDoc + ".pdf"));
                mailMessage.Attachments.Add(new Attachment(fileName));
                SmtpClient smtpClient = new SmtpClient(emisorDTE.smtp);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Port = int.Parse(emisorDTE.puertoCorreo);
                smtpClient.Credentials = new System.Net.NetworkCredential(emisorDTE.correoEnvio, emisorDTE.contraCorreo);
                smtpClient.Send(mailMessage);
                smtpClient.Dispose();
                mySBO_Application.StatusBar.SetText("Correo Enviado correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error al Enviar DTE al receptor: " + e.Message);
                mySBO_Application.StatusBar.SetText("Error al Enviar DTE al receptor: " + e.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);

            }

        }

        private void EnviarCorreoDTE_FSE(int docNum)
        {
            try
            {
                Cliente cliente = new Cliente();
                EmisorDTE emisorDTE = new EmisorDTE();
                jsonFirmadorFSE jsonFirmadorFSE = new jsonFirmadorFSE();
                emisorDTE = consultas.obtenerEmisor();
                cliente = consultas.obtenerInfoClienteFSE(docNum);
                String nombreDoc = consultas.nombreFSE_PDF(docNum);

                jsonFirmadorFSE = obtenerJSONFSE(cliente, docNum);
                jsonFirmadorFSE.dteJson.identificacion.fecEmi = cliente.docDate;
                jsonFirmadorFSE.dteJson.identificacion.codigoGeneracion = consultas.nombreFSE_PDF(docNum).Substring(0, 36);

                string fileName = Path.GetTempPath() + nombreDoc + ".json";
                string jsonString = JsonConvert.SerializeObject(jsonFirmadorFSE.dteJson);

                if (!File.Exists(fileName))
                    File.WriteAllText(fileName, jsonString);

                crystalReport = new ReportDocument();
                crystalReport.Load(Application.StartupPath + @"\Reportes\DTE_FSE.rpt");
                crystalReport.SetDatabaseLogon(userDB, passDB);
                crystalReport.SetParameterValue("DocKey@", consultas.obtenerDocEntryFSE(docNum));
                MailMessage mailMessage = new MailMessage(emisorDTE.correoEnvio,
                                                          cliente.correo,
                                                          "DOCUMENTO DTE: " + nombreDoc,
                                                          "<h1>ESTIMADO(A) CLIENTE: </h1>" +
                                                         "<h1><b>" + cliente.nombre + "</b></h1>" +
                                                         "<h3>POR ESTE MEDIO COMPARTIMOS SU FACTURA DE SUJETO EXCLUIDO.</h3>");
                mailMessage.IsBodyHtml = true;
                mailMessage.Attachments.Add(new Attachment(crystalReport.ExportToStream(ExportFormatType.PortableDocFormat), nombreDoc + ".pdf"));
                mailMessage.Attachments.Add(new Attachment(fileName));
                SmtpClient smtpClient = new SmtpClient(emisorDTE.smtp);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Port = int.Parse(emisorDTE.puertoCorreo);
                smtpClient.Credentials = new System.Net.NetworkCredential(emisorDTE.correoEnvio, emisorDTE.contraCorreo);
                smtpClient.Send(mailMessage);
                smtpClient.Dispose();
                mySBO_Application.StatusBar.SetText("Correo Enviado correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error al Enviar DTE al receptor: " + e.Message);
                mySBO_Application.StatusBar.SetText("Error al Enviar DTE al receptor: " + e.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);

            }

        }

        private void manejadorEventosAppSAP(SAPbouiCOM.BoAppEventTypes eventTypes)
        {
            switch (eventTypes)
            {
                case SAPbouiCOM.BoAppEventTypes.aet_ShutDown:
                case SAPbouiCOM.BoAppEventTypes.aet_ServerTerminition:
                case SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged:
                    mySBO_Application.StatusBar.SetText("Deteniendo add-On DTE", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    System.Environment.Exit(0);
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_LanguageChanged:
                    mySBO_Application.StatusBar.SetText("Lenguaje cambiado", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    break;
            }
        }

        private void manejoEventosMenu(ref SAPbouiCOM.MenuEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                if ((pVal.MenuUID == "MV101") & (pVal.BeforeAction == false))
                {
                    if (UserLog == "manager")
                        crearFormularioEmisor();

                }

                if ((pVal.MenuUID == "MV102") & (pVal.BeforeAction == false))
                {
                    if (UserLog == "manager")
                        crearFormularioDB();

                }

                if ((pVal.MenuUID == "MV103") & (pVal.BeforeAction == false))
                {
                    if (UserLog == "manager")
                    crearFormularioEnvLote();

                }

                if ((pVal.MenuUID == "MV104") & (pVal.BeforeAction == false))
                {
                    if (UserLog == "manager")
                        crearFormularioCorreo();

                }

                if ((pVal.MenuUID == "MV105") & (pVal.BeforeAction == false))
                {
                    crearFormularioContingencia();

                }

                if ((pVal.MenuUID == "MV106") & (pVal.BeforeAction == false))
                {
                    crearFormularioEnvioContingencia();

                }
            }
            catch (Exception ex)
            {
                mySBO_Application.StatusBar.SetText("Error: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);

            }
        }

        //CREAR OPCIONES DE MENU
        public void crearMenu()
        {
            SAPbouiCOM.Menus oMenus = null;
            SAPbouiCOM.MenuItem oMenuItem = null;

            //O  BTENER MENUS DE SAP B1
            oMenus = mySBO_Application.Menus;
            SAPbouiCOM.MenuCreationParams oCreationPackage = null;
            oCreationPackage = (SAPbouiCOM.MenuCreationParams)mySBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams);
            oMenuItem = mySBO_Application.Menus.Item("43520");// modulo de menus o menu principal

            //Obtener el nuero de hijos (Sub-menus) del menu principal
            int menuHijos = oMenuItem.SubMenus.Count;

            oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_POPUP;
            oCreationPackage.UniqueID = "MV001";
            oCreationPackage.String = "Gestion DTE";
            oCreationPackage.Enabled = true;
            string spath = null;
            spath = Application.StartupPath;
            oCreationPackage.Position = menuHijos + 1;

            oMenus = oMenuItem.SubMenus;

            try
            {
                oMenus.AddEx(oCreationPackage);

                oMenuItem = mySBO_Application.Menus.Item("MV001");//MENU AGREGADOR
                oMenus = oMenuItem.SubMenus;

                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                oCreationPackage.UniqueID = "MV101";
                oCreationPackage.String = "Gestion emisor DTE";
                oMenus.AddEx(oCreationPackage);

                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                oCreationPackage.UniqueID = "MV104";
                oCreationPackage.String = "Gestion Correo DTE";
                oMenus.AddEx(oCreationPackage);

                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                oCreationPackage.UniqueID = "MV102";
                oCreationPackage.String = "Gestion Base de datos";
                oMenus.AddEx(oCreationPackage);

                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                oCreationPackage.UniqueID = "MV103";
                oCreationPackage.String = "Envio DTE Masivo";
                oMenus.AddEx(oCreationPackage);

                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                oCreationPackage.UniqueID = "MV105";
                oCreationPackage.String = "Gestion de contingencias";
                oMenus.AddEx(oCreationPackage);

                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                oCreationPackage.UniqueID = "MV106";
                oCreationPackage.String = "Envio de contingencia";
                oMenus.AddEx(oCreationPackage);


            }
            catch (Exception ex)
            {
                mySBO_Application.StatusBar.SetText("El menu 'Gestion DTE' tiene un error: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }


        //CREAR UN FORMULARIO
        private void crearFormulario()
        {
            try
            {
                SAPbouiCOM.Form myForm;
                SAPbouiCOM.FormCreationParams myFormCreationParams;
                SAPbouiCOM.Item myItem;

                myFormCreationParams = (SAPbouiCOM.FormCreationParams)mySBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_FormCreationParams);

                myFormCreationParams.BorderStyle = SAPbouiCOM.BoFormBorderStyle.fbs_Fixed;

                myForm = mySBO_Application.Forms.AddEx(myFormCreationParams);
                myForm.Title = "FORMULARIO DE PRUEBA";
                myForm.Left = 100;

                //AGREGAR UN EDIT TEXT
                myItem = myForm.Items.Add("MV_ET1", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                myItem.Left = 50;
                myItem.Top = 50;
                SAPbouiCOM.EditText editText = (SAPbouiCOM.EditText)myItem.Specific;

                //AGREGAR BOTON 1
                myItem = myForm.Items.Add("MV_BT1", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                myItem.Left = 50;
                myItem.Top = 100;
                SAPbouiCOM.Button myButton1 = (SAPbouiCOM.Button)myItem.Specific;
                myButton1.Caption = "Aceptar";

                //AGREGAR BOTON 2
                myItem = myForm.Items.Add("MV_BT2", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                myItem.Left = 125;
                myItem.Top = 100;
                SAPbouiCOM.Button myButton2 = (SAPbouiCOM.Button)myItem.Specific;
                myButton2.Caption = "Cancelar";


                myForm.Visible = true;


            }
            catch (Exception ex)
            {
                mySBO_Application.StatusBar.SetText("El Formulario 'FORMULARIO DE PRUEBA' tiene un error: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void crearFormularioEmisor()
        {
            try
            {

                List<ActEconomica> actEconomicas = new List<ActEconomica>();
                List<Departamento> departamentos = new List<Departamento>();
                List<Municipio> municipios = new List<Municipio>();
                List<ObjFE> regimen = new List<ObjFE>();
                List<ObjFE> recintoFis = new List<ObjFE>();
                EmisorDTE emisorDTE = new EmisorDTE();

                

                SAPbouiCOM.FormCreationParams myFormCreationParams;
                XmlDocument oXML = new XmlDocument();
                String sPath = null;
                sPath = Application.StartupPath;
                oXML.Load(sPath + "\\formXML\\MVformularioEmisorDTE.xml");

                myFormCreationParams = (SAPbouiCOM.FormCreationParams)mySBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_FormCreationParams);
                myFormCreationParams.XmlData = oXML.InnerXml;
                myForm = mySBO_Application.Forms.AddEx(myFormCreationParams);

                //----------------------------------------------------------------------------

                SAPbouiCOM.Item myItemActCon;
                SAPbouiCOM.ComboBox myComboBoxActCon;
                myItemActCon = myForm.Items.Item("MV_cbACom");
                myComboBoxActCon = (SAPbouiCOM.ComboBox)myItemActCon.Specific;
                //myComboBox.DataBind.SetBound(true, "@FE_DEPARTAMENTO", "codDEPTO");
                actEconomicas.AddRange(consultas.obtenerActEcon());
                foreach (ActEconomica obj in actEconomicas)
                {
                    myComboBoxActCon.ValidValues.Add(obj.codActividad, obj.descActividad);
                }

                //----------------------------------------------------------------------------

                SAPbouiCOM.Item myItemDepto;
                SAPbouiCOM.ComboBox myComboBoxDepto;
                myItemDepto = myForm.Items.Item("MV_cbDep");
                myComboBoxDepto = (SAPbouiCOM.ComboBox)myItemDepto.Specific;
                //myComboBox.DataBind.SetBound(true, "@FE_DEPARTAMENTO", "codDEPTO");
                departamentos.AddRange(consultas.obtenerDepto());
                foreach (Departamento obj in departamentos)
                {
                    myComboBoxDepto.ValidValues.Add(obj.codDepto, obj.Depto);
                }

                //----------------------------------------------------------------------------

                SAPbouiCOM.Item myItemMuni;
                SAPbouiCOM.ComboBox myComboBoxMuni;
                myItemMuni = myForm.Items.Item("MV_cbMun");
                myComboBoxMuni = (SAPbouiCOM.ComboBox)myItemMuni.Specific;
                //myComboBox.DataBind.SetBound(true, "@FE_DEPARTAMENTO", "codDEPTO");
                municipios.AddRange(consultas.obtenerMunicipio());
                foreach (Municipio obj in municipios)
                {
                    myComboBoxMuni.ValidValues.Add(obj.codMunicipio, obj.municipio);
                }



                SAPbouiCOM.ComboBox myComboBox_recintoFis = (SAPbouiCOM.ComboBox)myForm.Items.Item("MV_cbRFis").Specific;
                recintoFis.AddRange(consultas.obtenerRecintoFis());
                foreach (ObjFE obj in recintoFis)
                {
                    myComboBox_recintoFis.ValidValues.Add(obj.cod, obj.descrip);
                }

                SAPbouiCOM.ComboBox myComboBox_regimen = (SAPbouiCOM.ComboBox)myForm.Items.Item("MV_cbReg").Specific;
                regimen.AddRange(consultas.obtenerRegimen());
                foreach (ObjFE obj in regimen)
                {
                    myComboBox_regimen.ValidValues.Add(obj.cod, obj.descrip);
                }


                SAPbouiCOM.EditText myEditText_nombre = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtName").Specific;
                SAPbouiCOM.EditText myEditText_nombreComer = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtNCom").Specific;
                SAPbouiCOM.EditText myEditText_nit = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtNIT").Specific;
                SAPbouiCOM.EditText myEditText_nrc = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtNRC").Specific;
                SAPbouiCOM.ComboBox myComboBox_actEcon = (SAPbouiCOM.ComboBox)myForm.Items.Item("MV_cbACom").Specific;
                SAPbouiCOM.ComboBox myComboBox_tipoEst = (SAPbouiCOM.ComboBox)myForm.Items.Item("MV_cbTEst").Specific;
                SAPbouiCOM.ComboBox myComboBox_depto = (SAPbouiCOM.ComboBox)myForm.Items.Item("MV_cbDep").Specific;
                SAPbouiCOM.ComboBox myComboBox_muni = (SAPbouiCOM.ComboBox)myForm.Items.Item("MV_cbMun").Specific;
                SAPbouiCOM.EditText myEditText_direccion = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtDire").Specific;
                SAPbouiCOM.EditText myEditText_telefono = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtTel").Specific;
                SAPbouiCOM.EditText myEditText_correo = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtCor").Specific;
                SAPbouiCOM.EditText myEditText_user = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtUsr").Specific;
                SAPbouiCOM.EditText myEditText_pass = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtPss").Specific;
                SAPbouiCOM.EditText myEditText_passPriv = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtPs1").Specific;
                SAPbouiCOM.ComboBox myComboBox_Ambiente = (SAPbouiCOM.ComboBox)myForm.Items.Item("MV_txtAmb1").Specific;
                SAPbouiCOM.EditText myEditText_correoEnvio = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtCE").Specific;
                SAPbouiCOM.EditText myEditText_contraCorreo = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtCCR").Specific;

                emisorDTE = consultas.obtenerEmisor();

                myEditText_nombre.Value = emisorDTE.nombre;
                myEditText_nombreComer.Value = emisorDTE.nombreComercial;
                myEditText_nit.Value = emisorDTE.nit;
                myEditText_nrc.Value = emisorDTE.nrc;
                myComboBox_actEcon.Select(emisorDTE.codActividad);
                myComboBox_tipoEst.Select(emisorDTE.tipoEstablecimiento);
                myComboBox_depto.Select(emisorDTE.departamento);
                myComboBox_muni.Select(emisorDTE.municipio);
                myEditText_direccion.Value = emisorDTE.direccion;
                myEditText_telefono.Value = emisorDTE.telefono;
                myEditText_correo.Value = emisorDTE.correo;
                myEditText_user.Value = emisorDTE.userDTE;
                myEditText_pass.Value = emisorDTE.passDTE;
                myEditText_passPriv.Value = emisorDTE.passPrivDTE;
                myComboBox_Ambiente.Select(emisorDTE.ambiente);
                myEditText_correoEnvio.Value = emisorDTE.correoEnvio;
                myEditText_contraCorreo.Value = emisorDTE.contraCorreo;

                myForm.Visible = true;

            }
            catch (Exception ex)
            {
                mySBO_Application.StatusBar.SetText("Error: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void crearFormularioDB()
        {
            try
            {

                SAPbouiCOM.FormCreationParams myFormCreationParams;
                XmlDocument oXML = new XmlDocument();
                String sPath = null;
                sPath = Application.StartupPath;
                oXML.Load(sPath + "\\formXML\\EAformularioDB_DTE.xml");

                myFormCreationParams = (SAPbouiCOM.FormCreationParams)mySBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_FormCreationParams);
                myFormCreationParams.XmlData = oXML.InnerXml;
                myForm = mySBO_Application.Forms.AddEx(myFormCreationParams);

                SAPbouiCOM.EditText myEditText_Server = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtSrv").Specific;
                SAPbouiCOM.EditText myEditText_Db = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtDB").Specific;
                SAPbouiCOM.EditText myEditText_User = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtUser").Specific;
                SAPbouiCOM.EditText myEditText_Pass = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtPass").Specific;


                conectarDI();
                if (mySBO_Company.Connect() != 0)
                {
                    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                }
                else
                {
                    //SAPbobsCOM.UserObjectsMD userObjectsMD = (SAPbobsCOM.UserObjectsMD)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserObjectsMD);
                    SAPbobsCOM.UserTable oUserTable = mySBO_Company.UserTables.Item("FE_DB");
                    oUserTable.GetByKey("1");
                    myEditText_Server.Value = oUserTable.UserFields.Fields.Item("U_serverDB").Value.ToString();
                    myEditText_Db.Value = oUserTable.UserFields.Fields.Item("U_DB").Value.ToString();
                    myEditText_User.Value = oUserTable.UserFields.Fields.Item("U_userDB").Value.ToString();
                    myEditText_Pass.Value = oUserTable.UserFields.Fields.Item("U_passDB").Value.ToString();

                }

                myForm.Visible = true;

            }
            catch (Exception ex)
            {
                mySBO_Application.StatusBar.SetText("Error: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                mySBO_Company.Disconnect();
            }
        }

        private void crearFormularioCorreo()
        {
            try
            {

                SAPbouiCOM.FormCreationParams myFormCreationParams;
                XmlDocument oXML = new XmlDocument();
                String sPath = null;
                sPath = Application.StartupPath;
                oXML.Load(sPath + "\\formXML\\EAformularioCorreo_DTE.xml");

                myFormCreationParams = (SAPbouiCOM.FormCreationParams)mySBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_FormCreationParams);
                myFormCreationParams.XmlData = oXML.InnerXml;
                myForm = mySBO_Application.Forms.AddEx(myFormCreationParams);

                SAPbouiCOM.EditText myEditText_Smtp = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtsmtp").Specific;
                SAPbouiCOM.EditText myEditText_Port = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtPort").Specific;
                SAPbouiCOM.EditText myEditText_User = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtUser").Specific;
                SAPbouiCOM.EditText myEditText_Pass = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtPass").Specific;


                conectarDI();
                if (mySBO_Company.Connect() != 0)
                {
                    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                }
                else
                {
                    //SAPbobsCOM.UserObjectsMD userObjectsMD = (SAPbobsCOM.UserObjectsMD)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserObjectsMD);
                    SAPbobsCOM.UserTable oUserTable = mySBO_Company.UserTables.Item("FE_EMISOR");
                    oUserTable.GetByKey("1");
                    myEditText_Smtp.Value = oUserTable.UserFields.Fields.Item("U_smtp").Value.ToString();
                    myEditText_Port.Value = oUserTable.UserFields.Fields.Item("U_puertoCorreo").Value.ToString();
                    myEditText_User.Value = oUserTable.UserFields.Fields.Item("U_correoEnvio").Value.ToString();
                    myEditText_Pass.Value = oUserTable.UserFields.Fields.Item("U_contraCorreo").Value.ToString();

                }

                myForm.Visible = true;

            }
            catch (Exception ex)
            {
                mySBO_Application.StatusBar.SetText("Error: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                mySBO_Company.Disconnect();
            }
        }

        private void crearFormularioEnvLote()
        {
            try
            {

                SAPbouiCOM.FormCreationParams myFormCreationParams;
                XmlDocument oXML = new XmlDocument();
                String sPath = null;
                sPath = Application.StartupPath;
                oXML.Load(sPath + "\\formXML\\EAformularioLoteDTE.xml");

                myFormCreationParams = (SAPbouiCOM.FormCreationParams)mySBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_FormCreationParams);
                myFormCreationParams.XmlData = oXML.InnerXml;
                myForm = mySBO_Application.Forms.AddEx(myFormCreationParams);

                //oEdit.String = Format(Today.Date, "dd/MM/yyyy");

                SAPbouiCOM.EditText myEditText_FechaDe = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtFeDe").Specific;
                SAPbouiCOM.EditText myEditText_FechaAl = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtFeAl").Specific;


                myForm.Visible = true;
                myForm.DataSources.DataTables.Add("EA_tabDoc1");
            }
            catch (Exception ex)
            {
                mySBO_Application.StatusBar.SetText("Error: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
               
            }
        }

        private void crearFormularioAnulacion(int docNum, String docTipo, String cardCode)
        {
            try
            {

                List<ObjFE> TiposAnulacion = new List<ObjFE>();
                List<ObjFE> tipoDocs = new List<ObjFE>();
                DocumentoAnulacion documento = new DocumentoAnulacion();
                EmisorDTE emisorDTE = new EmisorDTE();

                
                SAPbouiCOM.FormCreationParams myFormCreationParams;
                XmlDocument oXML = new XmlDocument();
                String sPath = null;
                sPath = Application.StartupPath;
                oXML.Load(sPath + "\\formXML\\EAformularioAnulacionDTE.xml");

                myFormCreationParams = (SAPbouiCOM.FormCreationParams)mySBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_FormCreationParams);
                myFormCreationParams.XmlData = oXML.InnerXml;
                myForm = mySBO_Application.Forms.AddEx(myFormCreationParams);

                //----------------------------------------------------------------------------

                SAPbouiCOM.Item myItemTipoAnulacion;
                SAPbouiCOM.ComboBox myComboBoxTipoAnulacion;
                myItemTipoAnulacion = myForm.Items.Item("EA_cbTAnu");
                myComboBoxTipoAnulacion = (SAPbouiCOM.ComboBox)myItemTipoAnulacion.Specific;
                //myComboBox.DataBind.SetBound(true, "@FE_DEPARTAMENTO", "codDEPTO");
                TiposAnulacion.AddRange(consultas.obtenerTipoAnulacion());
                foreach (ObjFE obj in TiposAnulacion)
                {
                    myComboBoxTipoAnulacion.ValidValues.Add(obj.cod, obj.descrip);
                }

                emisorDTE = consultas.obtenerEmisor();

                //----------------------------------------------------------------------------

                SAPbouiCOM.Item myItemTipoDocRes;
                SAPbouiCOM.Item myItemTipoDocSoli;
                SAPbouiCOM.ComboBox myComboBoxTipoDocRes;
                SAPbouiCOM.ComboBox myComboBoxTipoDocSoli;
                myItemTipoDocRes = myForm.Items.Item("EA_cbTDRes");
                myItemTipoDocSoli = myForm.Items.Item("EA_cbTDSol");
                myComboBoxTipoDocRes = (SAPbouiCOM.ComboBox)myItemTipoDocRes.Specific;
                myComboBoxTipoDocSoli = (SAPbouiCOM.ComboBox)myItemTipoDocSoli.Specific;
                //myComboBox.DataBind.SetBound(true, "@FE_DEPARTAMENTO", "codDEPTO");
                tipoDocs.AddRange(consultas.obtenerTipoDoc());
                foreach (ObjFE obj in tipoDocs)
                {
                    myComboBoxTipoDocRes.ValidValues.Add(obj.cod, obj.descrip);
                    myComboBoxTipoDocSoli.ValidValues.Add(obj.cod, obj.descrip);
                }

                //----------------------------------------------------------------------------



                SAPbouiCOM.EditText myEditText_docNum = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtDNum").Specific;
                SAPbouiCOM.EditText myEditText_CodGen = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtCGen").Specific;
                SAPbouiCOM.EditText myEditText_numControl = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtNumC").Specific;
                SAPbouiCOM.EditText myEditText_fechaEmi = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtFEmi").Specific;
                SAPbouiCOM.EditText myEditText_montoIva = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtMIVA").Specific;
                SAPbouiCOM.EditText myEditText_cl = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtCL").Specific;
                SAPbouiCOM.EditText myEditText_TipoDoc = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtTDoc").Specific;

                SAPbouiCOM.EditText myEditText_NomResp = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtNRes").Specific;
                SAPbouiCOM.ComboBox myEditText_TipoDocResp = (SAPbouiCOM.ComboBox)myForm.Items.Item("EA_cbTDRes").Specific;
                SAPbouiCOM.EditText myEditText_NumResp = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtNDRe").Specific;

                documento = consultas.obtenerInfoDoc(docNum, docTipo); 

                myEditText_cl.Value = cardCode;
                myEditText_TipoDoc.Value = docTipo;
                myEditText_docNum.Value = documento.tipoDte;
                myEditText_CodGen.Value = documento.codigoGeneracion;
                myEditText_numControl.Value = documento.numeroControl;
                myEditText_fechaEmi.Value = documento.fecEmi;
                myEditText_montoIva.Value = documento.montoIva.ToString();
                myEditText_NomResp.Value = emisorDTE.nombre;
                myEditText_NumResp.Value = emisorDTE.nit;
                myEditText_TipoDocResp.Select("36");
                myForm.Visible = true;

            }
            catch (Exception ex)
            {
                myForm.Visible = false;
                Console.WriteLine("Error: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                mySBO_Application.StatusBar.SetText("Error: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void crearFormularioContingencia()
        {
            try
            {

                SAPbouiCOM.FormCreationParams myFormCreationParams;
                XmlDocument oXML = new XmlDocument();
                String sPath = null;
                sPath = Application.StartupPath;
                oXML.Load(sPath + "\\formXML\\EAformularioContiDTE.xml");

                myFormCreationParams = (SAPbouiCOM.FormCreationParams)mySBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_FormCreationParams);
                myFormCreationParams.XmlData = oXML.InnerXml;
                myForm = mySBO_Application.Forms.AddEx(myFormCreationParams);

                SAPbouiCOM.EditText myEditText_FechaDe = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtFeDe").Specific;
                SAPbouiCOM.EditText myEditText_FechaAl = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtFeAl").Specific;


                myForm.Visible = true;
                myForm.DataSources.DataTables.Add("EA_tabCon1");
                cargarContingencias();
            }
            catch (Exception ex)
            {
                mySBO_Application.StatusBar.SetText("Error: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {

            }
        }

        private void crearFormularioEnvioContingencia()
        {
            try
            {
                List<Contingencia> contingencias = new List<Contingencia>();
                SAPbouiCOM.FormCreationParams myFormCreationParams;
                XmlDocument oXML = new XmlDocument();
                String sPath = null;
                sPath = Application.StartupPath;
                oXML.Load(sPath + "\\formXML\\EAformularioEnvioContiDTE.xml");

                myFormCreationParams = (SAPbouiCOM.FormCreationParams)mySBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_FormCreationParams);
                myFormCreationParams.XmlData = oXML.InnerXml;
                myForm = mySBO_Application.Forms.AddEx(myFormCreationParams);

                SAPbouiCOM.Item myItemListContin;
                SAPbouiCOM.ComboBox myComboBoxListContin;
                myItemListContin = myForm.Items.Item("EA_cmbTCon");
                myComboBoxListContin = (SAPbouiCOM.ComboBox)myItemListContin.Specific;
                //myComboBox.DataBind.SetBound(true, "@FE_DEPARTAMENTO", "codDEPTO");
                contingencias.AddRange(consultas.obtenerListaContingencia());
                foreach (Contingencia obj in contingencias)
                {
                    myComboBoxListContin.ValidValues.Add(obj.tipoContingencia, obj.descContingencia);
                }

                myForm.Visible = true;
                myForm.DataSources.DataTables.Add("EA_tabEnCo");
            }
            catch (Exception ex)
            {
                mySBO_Application.StatusBar.SetText("Error: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {

            }
        }

        private void guardarEmisor()
        {
            try
            {
                SAPbouiCOM.IItems oItems;
                SAPbouiCOM.Item oItem;
                
                conectarDI();
                if (mySBO_Company.Connect() != 0)
                {
                    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                }
                else
                {
                    //SAPbobsCOM.UserObjectsMD userObjectsMD = (SAPbobsCOM.UserObjectsMD)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserObjectsMD);
                    SAPbobsCOM.UserTable oUserTable = mySBO_Company.UserTables.Item("FE_EMISOR");

                    SAPbouiCOM.EditText myEditText_nombre = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtName").Specific;
                    SAPbouiCOM.EditText myEditText_nombreComer = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtNCom").Specific;
                    SAPbouiCOM.EditText myEditText_nit = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtNIT").Specific;
                    SAPbouiCOM.EditText myEditText_nrc = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtNRC").Specific;
                    SAPbouiCOM.ComboBox myComboBox_actEcon = (SAPbouiCOM.ComboBox)myForm.Items.Item("MV_cbACom").Specific;
                    SAPbouiCOM.ComboBox myComboBox_tipoEst = (SAPbouiCOM.ComboBox)myForm.Items.Item("MV_cbTEst").Specific;
                    SAPbouiCOM.ComboBox myComboBox_depto = (SAPbouiCOM.ComboBox)myForm.Items.Item("MV_cbDep").Specific;
                    SAPbouiCOM.ComboBox myComboBox_muni = (SAPbouiCOM.ComboBox)myForm.Items.Item("MV_cbMun").Specific;
                    SAPbouiCOM.EditText myEditText_direccion = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtDire").Specific;
                    SAPbouiCOM.EditText myEditText_telefono = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtTel").Specific;
                    SAPbouiCOM.EditText myEditText_correo = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtCor").Specific;
                    SAPbouiCOM.ComboBox myComboBox_resintoFis = (SAPbouiCOM.ComboBox)myForm.Items.Item("MV_cbRFis").Specific;
                    SAPbouiCOM.ComboBox myComboBox_regimen = (SAPbouiCOM.ComboBox)myForm.Items.Item("MV_cbReg").Specific;
                    SAPbouiCOM.EditText myEditText_user = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtUsr").Specific;
                    SAPbouiCOM.EditText myEditText_pass = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtPss").Specific;
                    SAPbouiCOM.EditText myEditText_passPriv = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtPs1").Specific;
                    SAPbouiCOM.ComboBox myComboBox_Ambiente = (SAPbouiCOM.ComboBox)myForm.Items.Item("MV_txtAmb1").Specific;
                    SAPbouiCOM.EditText myEditText_correoEnvio = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtCE").Specific;
                    SAPbouiCOM.EditText myEditText_contraCorreo = (SAPbouiCOM.EditText)myForm.Items.Item("MV_txtCCR").Specific;

                    if (consultas.contarEmisor() == 0)
                    {
                         oUserTable.Code = "1";
                         oUserTable.Name = "1";
                         oUserTable.UserFields.Fields.Item("U_Nombre").Value=myEditText_nombre.Value;
                         oUserTable.UserFields.Fields.Item("U_NombreComercial").Value = myEditText_nombreComer.Value;
                         oUserTable.UserFields.Fields.Item("U_nit").Value = myEditText_nit.Value;
                         oUserTable.UserFields.Fields.Item("U_nrc").Value = myEditText_nrc.Value;
                         oUserTable.UserFields.Fields.Item("U_ActEcon").Value = myComboBox_actEcon.Selected.Value;
                         oUserTable.UserFields.Fields.Item("U_tipoEstablecimiento").Value = myComboBox_tipoEst.Selected.Value;
                         oUserTable.UserFields.Fields.Item("U_departamento").Value = myComboBox_depto.Selected.Value;
                         oUserTable.UserFields.Fields.Item("U_municipio").Value = myComboBox_muni.Selected.Value;
                         oUserTable.UserFields.Fields.Item("U_direccion").Value = myEditText_direccion.Value;
                         oUserTable.UserFields.Fields.Item("U_telefono").Value = myEditText_telefono.Value;
                         oUserTable.UserFields.Fields.Item("U_correo").Value = myEditText_correo.Value;
                         oUserTable.UserFields.Fields.Item("U_userDTE").Value = myEditText_user.Value;
                         oUserTable.UserFields.Fields.Item("U_passDTE").Value = myEditText_pass.Value;
                         oUserTable.UserFields.Fields.Item("U_passPrivDTE").Value = myEditText_passPriv.Value;
                         oUserTable.UserFields.Fields.Item("U_AMBIENTE").Value = myComboBox_Ambiente.Selected.Value;
                         oUserTable.UserFields.Fields.Item("U_correoEnvio").Value = myEditText_correoEnvio.Value;
                         oUserTable.UserFields.Fields.Item("U_contraCorreo").Value = myEditText_contraCorreo.Value;
                        if (oUserTable.Add()!=0)
                         {
                             mySBO_Application.StatusBar.SetText("Error al guardar informacion de Emisor: " + mySBO_Company.GetLastErrorCode() + " - "+ mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                         }
                         else
                         {
                             mySBO_Application.StatusBar.SetText("Informacion de emisor Guardada", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                         }
                    }
                    else
                    {
                        oUserTable.GetByKey("1");
                        oUserTable.UserFields.Fields.Item("U_Nombre").Value = myEditText_nombre.Value;
                        oUserTable.UserFields.Fields.Item("U_NombreComercial").Value = myEditText_nombreComer.Value;
                        oUserTable.UserFields.Fields.Item("U_nit").Value = myEditText_nit.Value;
                        oUserTable.UserFields.Fields.Item("U_nrc").Value = myEditText_nrc.Value;
                        oUserTable.UserFields.Fields.Item("U_ActEcon").Value = myComboBox_actEcon.Value;
                        oUserTable.UserFields.Fields.Item("U_tipoEstablecimiento").Value = myComboBox_tipoEst.Value;
                        oUserTable.UserFields.Fields.Item("U_departamento").Value = myComboBox_depto.Value;
                        oUserTable.UserFields.Fields.Item("U_municipio").Value = myComboBox_muni.Value;
                        oUserTable.UserFields.Fields.Item("U_direccion").Value = myEditText_direccion.Value;
                        oUserTable.UserFields.Fields.Item("U_telefono").Value = myEditText_telefono.Value;
                        oUserTable.UserFields.Fields.Item("U_correo").Value = myEditText_correo.Value;
                        oUserTable.UserFields.Fields.Item("U_userDTE").Value = myEditText_user.Value;
                        oUserTable.UserFields.Fields.Item("U_passDTE").Value = myEditText_pass.Value;
                        oUserTable.UserFields.Fields.Item("U_passPrivDTE").Value = myEditText_passPriv.Value;
                        oUserTable.UserFields.Fields.Item("U_AMBIENTE").Value = myComboBox_Ambiente.Selected.Value;
                        oUserTable.UserFields.Fields.Item("U_correoEnvio").Value = myEditText_correoEnvio.Value;
                        oUserTable.UserFields.Fields.Item("U_contraCorreo").Value = myEditText_contraCorreo.Value;

                        if (oUserTable.Update() != 0)
                        {
                            mySBO_Application.StatusBar.SetText("Error al guardar informacion de Emisor: " + mySBO_Company.GetLastErrorCode() + " - " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        }
                        else
                        {
                            mySBO_Application.StatusBar.SetText("Informacion de emisor Guardada", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mySBO_Application.StatusBar.SetText("Error guardarEmisor: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                mySBO_Company.Disconnect();
            }
        }

        private void guardaDB()
        {
            try
            {
                SAPbouiCOM.IItems oItems;
                SAPbouiCOM.Item oItem;

                conectarDI();
                if (mySBO_Company.Connect() != 0)
                {
                    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                }
                else
                {
                    //SAPbobsCOM.UserObjectsMD userObjectsMD = (SAPbobsCOM.UserObjectsMD)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserObjectsMD);
                    SAPbobsCOM.UserTable oUserTable = mySBO_Company.UserTables.Item("FE_DB");


                    SAPbouiCOM.EditText myEditText_Server = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtSrv").Specific;
                    SAPbouiCOM.EditText myEditText_Db = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtDB").Specific;
                    SAPbouiCOM.EditText myEditText_User = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtUser").Specific;
                    SAPbouiCOM.EditText myEditText_Pass = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtPass").Specific;


                    conectarDI();
                    if (mySBO_Company.Connect() != 0)
                    {
                        mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                    }
                    else
                    {

                        if (oUserTable.GetByKey("1") == true)
                        {
                            oUserTable.UserFields.Fields.Item("U_serverDB").Value = myEditText_Server.Value;
                            oUserTable.UserFields.Fields.Item("U_DB").Value = myEditText_Db.Value;
                            oUserTable.UserFields.Fields.Item("U_userDB").Value = myEditText_User.Value;
                            oUserTable.UserFields.Fields.Item("U_passDB").Value = myEditText_Pass.Value;
                            if (oUserTable.Update() != 0)
                            {
                                mySBO_Application.StatusBar.SetText("Error al guardar informacion de DB: " + mySBO_Company.GetLastErrorCode() + " - " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            }
                            else
                            {
                                mySBO_Application.StatusBar.SetText("Informacion de DB Guardada, por favor reinicie SAP B1", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                            }
                        }
                        else
                        {
                            oUserTable.Code = "1";
                            oUserTable.Name = "1";
                            oUserTable.UserFields.Fields.Item("U_serverDB").Value = myEditText_Server.Value;
                            oUserTable.UserFields.Fields.Item("U_DB").Value = myEditText_Db.Value;
                            oUserTable.UserFields.Fields.Item("U_userDB").Value = myEditText_User.Value;
                            oUserTable.UserFields.Fields.Item("U_passDB").Value = myEditText_Pass.Value;
                            if (oUserTable.Add() != 0)
                            {
                                mySBO_Application.StatusBar.SetText("Error al guardar informacion de DB: " + mySBO_Company.GetLastErrorCode() + " - " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            }
                            else
                            {
                                mySBO_Application.StatusBar.SetText("Informacion de DB Guardada, por favor reinicie SAP B1", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mySBO_Application.StatusBar.SetText("Error guardaDB: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                mySBO_Company.Disconnect();
            }
        }

        private void guardaCorreo()
        {
            try
            {
                SAPbouiCOM.IItems oItems;
                SAPbouiCOM.Item oItem;

                conectarDI();
                if (mySBO_Company.Connect() != 0)
                {
                    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                }
                else
                {
                    //SAPbobsCOM.UserObjectsMD userObjectsMD = (SAPbobsCOM.UserObjectsMD)mySBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserObjectsMD);
                    SAPbobsCOM.UserTable oUserTable = mySBO_Company.UserTables.Item("FE_EMISOR");
                    oUserTable.GetByKey("1");

                    SAPbouiCOM.EditText myEditText_Smtp = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtsmtp").Specific;
                    SAPbouiCOM.EditText myEditText_Port = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtPort").Specific;
                    SAPbouiCOM.EditText myEditText_User = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtUser").Specific;
                    SAPbouiCOM.EditText myEditText_Pass = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtPass").Specific;


                    //conectarDI();
                    //if (mySBO_Company.Connect() != 0)
                    //{
                    //    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                    //}
                    //else
                    //{
                        oUserTable.UserFields.Fields.Item("U_smtp").Value = myEditText_Smtp.Value;
                        oUserTable.UserFields.Fields.Item("U_puertoCorreo").Value = myEditText_Port.Value;
                        oUserTable.UserFields.Fields.Item("U_correoEnvio").Value = myEditText_User.Value;
                        oUserTable.UserFields.Fields.Item("U_contraCorreo").Value = myEditText_Pass.Value;
                        if (oUserTable.Update() != 0)
                        {
                            mySBO_Application.StatusBar.SetText("Error al guardar informacion de Correo: " + mySBO_Company.GetLastErrorCode() + " - " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        }
                    //}
                }
            }
            catch (Exception ex)
            {
                mySBO_Application.StatusBar.SetText("Error guardaDB: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                mySBO_Company.Disconnect();
            }
        }

        private void CargarInfoDocs()
        {
            try
            {
                SAPbouiCOM.IItems oItems;
                SAPbouiCOM.Item oItem;
                List<DocLote> docsLote = new List<DocLote>();
                int count = 0;
                //conectarDI();
                //if (mySBO_Company.Connect() != 0)
                //{
                //    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                //}
                //else
                //{
                SAPbouiCOM.ComboBox myComboBox_tipoDoc = (SAPbouiCOM.ComboBox)myForm.Items.Item("EA_cmbTDoc").Specific;
                SAPbouiCOM.EditText myEditText_fechaDe = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtFeDe").Specific;
                SAPbouiCOM.EditText myEditText_fechaAl = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtFeAl").Specific;
                SAPbouiCOM.EditText myEditText_serie = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtSeri").Specific;

                myForm.DataSources.DataTables.Item("EA_tabDoc1").Clear();

                myForm.DataSources.DataTables.Item("EA_tabDoc1").Columns.Add("Tipo documento", SAPbouiCOM.BoFieldsType.ft_Text, 5);
                myForm.DataSources.DataTables.Item("EA_tabDoc1").Columns.Add("Número documento", SAPbouiCOM.BoFieldsType.ft_Integer, 15);
                myForm.DataSources.DataTables.Item("EA_tabDoc1").Columns.Add("Fecha documento", SAPbouiCOM.BoFieldsType.ft_Text, 10);
                myForm.DataSources.DataTables.Item("EA_tabDoc1").Columns.Add("Codigo cliente", SAPbouiCOM.BoFieldsType.ft_Text, 10);
                myForm.DataSources.DataTables.Item("EA_tabDoc1").Columns.Add("Nombre cliente", SAPbouiCOM.BoFieldsType.ft_Text, 150);
                myForm.DataSources.DataTables.Item("EA_tabDoc1").Columns.Add("Total", SAPbouiCOM.BoFieldsType.ft_Float, 10);
                myForm.DataSources.DataTables.Item("EA_tabDoc1").Columns.Add("Monto IVA", SAPbouiCOM.BoFieldsType.ft_Float, 10);

                docsLote = consultas.obtenerDocsLote(Int32.Parse(myComboBox_tipoDoc.Value), myEditText_serie.Value, myEditText_fechaDe.Value, myEditText_fechaAl.Value);

                foreach (DocLote docs in docsLote)
                {
                    myForm.DataSources.DataTables.Item("EA_tabDoc1").Rows.Add();
                    myForm.DataSources.DataTables.Item("EA_tabDoc1").SetValue("Tipo documento", count, docs.tipoDoc);
                    myForm.DataSources.DataTables.Item("EA_tabDoc1").SetValue("Número documento", count, docs.docNum);
                    myForm.DataSources.DataTables.Item("EA_tabDoc1").SetValue("Fecha documento", count, docs.DocDate);
                    myForm.DataSources.DataTables.Item("EA_tabDoc1").SetValue("Codigo cliente", count, docs.cardCode);
                    myForm.DataSources.DataTables.Item("EA_tabDoc1").SetValue("Nombre cliente", count, docs.cardName);
                    myForm.DataSources.DataTables.Item("EA_tabDoc1").SetValue("Total", count, ((double)docs.docTotal));
                    myForm.DataSources.DataTables.Item("EA_tabDoc1").SetValue("Monto IVA", count, ((double)docs.iva));
                    count++;
                }

                //myForm.DataSources.DataTables.Add("EA_tabDoc1");
                //string query = "Select 0, CardName, CardType, GroupNum from OCRD";
                //string query = "EXEC SP_LISTAR_DOCS_LOTE_FEL @TIPODOC='1', @SERIE='FACFE001', @FECHA1='20231228', @FECHA2='20231231' ";
                //myForm.DataSources.DataTables.Item("EA_tabDoc1").ExecuteQuery(query);

                SAPbouiCOM.Grid myGrid = (SAPbouiCOM.Grid)myForm.Items.Item("EA_tabDocs").Specific;
                myGrid.DataTable = myForm.DataSources.DataTables.Item("EA_tabDoc1");
                myGrid.Item.Enabled = false;
                myForm.Update();

                //Console.WriteLine();
                //}
            }
            catch (Exception ex)
            {
                mySBO_Application.StatusBar.SetText("Error CargarInfoDocs: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                //mySBO_Company.Disconnect();
            }
        }

        private void CargarInfoDocsContingencia()
        {
            try
            {
                SAPbouiCOM.IItems oItems;
                SAPbouiCOM.Item oItem;
                List<DocLote> docsLote = new List<DocLote>();
                int count = 0;
                //conectarDI();
                //if (mySBO_Company.Connect() != 0)
                //{
                //    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                //}
                //else
                //{
                    SAPbouiCOM.ComboBox myComboBox_tipoDoc = (SAPbouiCOM.ComboBox)myForm.Items.Item("EA_cmbTDoc").Specific;
                SAPbouiCOM.ComboBox myComboBoxListContin = (SAPbouiCOM.ComboBox)myForm.Items.Item("EA_cmbTCon").Specific;
                SAPbouiCOM.EditText myEditText_serie = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtSeri").Specific;

                    myForm.DataSources.DataTables.Item("EA_tabEnCo").Clear();

                    myForm.DataSources.DataTables.Item("EA_tabEnCo").Columns.Add("Tipo documento", SAPbouiCOM.BoFieldsType.ft_Text, 5);
                    myForm.DataSources.DataTables.Item("EA_tabEnCo").Columns.Add("Número documento", SAPbouiCOM.BoFieldsType.ft_Integer, 15);
                    myForm.DataSources.DataTables.Item("EA_tabEnCo").Columns.Add("Fecha documento", SAPbouiCOM.BoFieldsType.ft_Text, 10);
                    myForm.DataSources.DataTables.Item("EA_tabEnCo").Columns.Add("Codigo cliente", SAPbouiCOM.BoFieldsType.ft_Text, 10);
                    myForm.DataSources.DataTables.Item("EA_tabEnCo").Columns.Add("Nombre cliente", SAPbouiCOM.BoFieldsType.ft_Text, 150);
                    myForm.DataSources.DataTables.Item("EA_tabEnCo").Columns.Add("Total", SAPbouiCOM.BoFieldsType.ft_Float, 10);
                    myForm.DataSources.DataTables.Item("EA_tabEnCo").Columns.Add("Monto IVA", SAPbouiCOM.BoFieldsType.ft_Float, 10);

                    docsLote = consultas.obtenerDocsLoteContin(Int32.Parse(myComboBox_tipoDoc.Selected.Value), myEditText_serie.Value, myComboBoxListContin.Selected.Value);

                    foreach (DocLote docs in docsLote)
                    {
                        myForm.DataSources.DataTables.Item("EA_tabEnCo").Rows.Add();
                        myForm.DataSources.DataTables.Item("EA_tabEnCo").SetValue("Tipo documento", count, docs.tipoDoc);
                        myForm.DataSources.DataTables.Item("EA_tabEnCo").SetValue("Número documento", count, docs.docNum);
                        myForm.DataSources.DataTables.Item("EA_tabEnCo").SetValue("Fecha documento", count, docs.DocDate);
                        myForm.DataSources.DataTables.Item("EA_tabEnCo").SetValue("Codigo cliente", count, docs.cardCode);
                        myForm.DataSources.DataTables.Item("EA_tabEnCo").SetValue("Nombre cliente", count, docs.cardName);
                        myForm.DataSources.DataTables.Item("EA_tabEnCo").SetValue("Total", count, ((double)docs.docTotal));
                        myForm.DataSources.DataTables.Item("EA_tabEnCo").SetValue("Monto IVA", count, ((double)docs.iva));
                        count++;
                    }

                    //myForm.DataSources.DataTables.Add("EA_tabDoc1");
                    //string query = "Select 0, CardName, CardType, GroupNum from OCRD";
                    //string query = "EXEC SP_LISTAR_DOCS_LOTE_FEL @TIPODOC='1', @SERIE='FACFE001', @FECHA1='20231228', @FECHA2='20231231' ";
                    //myForm.DataSources.DataTables.Item("EA_tabDoc1").ExecuteQuery(query);

                    SAPbouiCOM.Grid myGrid = (SAPbouiCOM.Grid)myForm.Items.Item("EA_tabDocs").Specific;
                    myGrid.DataTable = myForm.DataSources.DataTables.Item("EA_tabEnCo");
                    myGrid.Item.Enabled = false;
                    myForm.Update();
                    
                //Console.WriteLine();
                //}
            }
            catch (Exception ex)
            {
                mySBO_Application.StatusBar.SetText("Error CargarInfoDocs: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                //mySBO_Company.Disconnect();
            }
        }

        private async Task enviarDTE_Lote()
        {
            try
            {
                SAPbouiCOM.IItems oItems;
                SAPbouiCOM.Item oItem;

                int count = 0;
                string jsonFirmador = "";
                string json = "";
                string jsonDTE = "";
                dynamic jsonRespuesta;

                List<DocLote> docsLote = new List<DocLote>();
                List<string> listJsonFirmador = new List<string>();
                List<int> listDocNum = new List<int>();
                Cliente cliente;

                jsonReceptor jsonReceptor = new jsonReceptor();

                jsonFirmadorFE jsonFirmadorFE = new jsonFirmadorFE();
                jsonFirmadorCCF jsonFirmadorCCF = new jsonFirmadorCCF();
                jsonFirmadorFEX jsonFirmadorFEX = new jsonFirmadorFEX();
                jsonFirmadorFSE jsonFirmadorFSE = new jsonFirmadorFSE();
                jsonFirmadorNC jsonFirmadorNC = new jsonFirmadorNC();
                jsonFirmadorND jsonFirmadorND = new jsonFirmadorND();
                jsonFirmadorNR jsonFirmadorNR = new jsonFirmadorNR();

                DocumentoDTE documentoDTE = new DocumentoDTE();
                List<DocumentoDTE> documentosDTE = new List<DocumentoDTE>();

                ConsumoFirmador consumoFirmador = new ConsumoFirmador();
                EmisorDTE emisorDTE = new EmisorDTE();
                emisorDTE = consultas.obtenerInfoEmisor();

                SAPbouiCOM.ComboBox myComboBox_tipoDoc = (SAPbouiCOM.ComboBox)myForm.Items.Item("EA_cmbTDoc").Specific;
                SAPbouiCOM.EditText myEditText_fechaDe = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtFeDe").Specific;
                SAPbouiCOM.EditText myEditText_fechaAl = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtFeAl").Specific;
                SAPbouiCOM.EditText myEditText_serie = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtSeri").Specific;

                docsLote = consultas.obtenerDocsLote(Int32.Parse(myComboBox_tipoDoc.Value), myEditText_serie.Value, myEditText_fechaDe.Value, myEditText_fechaAl.Value);
                conectarDI();
                if (mySBO_Company.Connect() != 0)
                {
                    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                }
                else
                {
                    foreach (DocLote docs in docsLote)
                    {
                        documentoDTE = new DocumentoDTE();
                        cliente = new Cliente();

                        jsonFirmadorFE = new jsonFirmadorFE();
                        jsonFirmadorCCF = new jsonFirmadorCCF();
                        jsonFirmadorFEX = new jsonFirmadorFEX();
                        jsonFirmadorFSE = new jsonFirmadorFSE();
                        jsonFirmadorNC = new jsonFirmadorNC();
                        jsonFirmadorND = new jsonFirmadorND();
                        jsonFirmadorNR = new jsonFirmadorNR();

                        switch (myComboBox_tipoDoc.Value)
                        {
                            case "1":
                                cliente = consultas.obtenerInfoClienteFAC(docs.docNum);
                                jsonFirmadorFE = obtenerJSONFE(cliente, docs.docNum);
                                //jsonFirmador = firmarFE(cliente, docs.docNum);
                                jsonDTE = JsonConvert.SerializeObject(jsonFirmadorFE.dteJson);//CONVERTIR OBJETO A JSON
                                json = JsonConvert.SerializeObject(jsonFirmadorFE);//CONVERTIR OBJETO A JSON

                                break;
                            case "3":
                                cliente = consultas.obtenerInfoClienteFAC(docs.docNum);
                                jsonFirmadorCCF = obtenerJSONCCF(cliente, docs.docNum);
                                //jsonFirmador = firmarCCF(cliente, docs.docNum);
                                jsonDTE = JsonConvert.SerializeObject(jsonFirmadorCCF.dteJson);//CONVERTIR OBJETO A JSON
                                json = JsonConvert.SerializeObject(jsonFirmadorCCF);//CONVERTIR OBJETO A JSON
                                break;
                            case "11":
                                cliente = consultas.obtenerInfoClienteFAC(docs.docNum);
                                jsonFirmadorFEX = obtenerJSONFEX(cliente, docs.docNum);
                                //jsonFirmador = firmarFEX(cliente, docs.docNum);
                                jsonDTE = JsonConvert.SerializeObject(jsonFirmadorFEX.dteJson);//CONVERTIR OBJETO A JSON
                                json = JsonConvert.SerializeObject(jsonFirmadorFEX);//CONVERTIR OBJETO A JSON
                                break;
                            case "14":
                                cliente = consultas.obtenerInfoClienteFSE(docs.docNum);
                                jsonFirmadorFSE = obtenerJSONFSE(cliente, docs.docNum);
                                //jsonFirmador = firmarFSE(cliente, docs.docNum);
                                jsonDTE = JsonConvert.SerializeObject(jsonFirmadorFSE.dteJson);//CONVERTIR OBJETO A JSON
                                json = JsonConvert.SerializeObject(jsonFirmadorFSE);//CONVERTIR OBJETO A JSON
                                break;
                            case "5":
                                cliente = consultas.obtenerInfoClienteNC(docs.docNum);
                                jsonFirmadorNC = obtenerJSONNC(cliente, docs.docNum);
                                //jsonFirmador = firmarNC(cliente, docs.docNum);
                                jsonDTE = JsonConvert.SerializeObject(jsonFirmadorNC.dteJson);//CONVERTIR OBJETO A JSON
                                json = JsonConvert.SerializeObject(jsonFirmadorNC);//CONVERTIR OBJETO A JSON
                                break;
                            case "6":
                                cliente = consultas.obtenerInfoClienteND(docs.docNum);
                                jsonFirmadorND = obtenerJSONND(cliente, docs.docNum);
                                //jsonFirmador = firmarND(cliente, docs.docNum);
                                jsonDTE = JsonConvert.SerializeObject(jsonFirmadorND.dteJson);//CONVERTIR OBJETO A JSON
                                json = JsonConvert.SerializeObject(jsonFirmadorND);//CONVERTIR OBJETO A JSON
                                break;
                            case "4":
                                cliente = consultas.obtenerInfoClienteNR(docs.docNum);
                                jsonFirmadorNR = obtenerJSONNR(cliente, docs.docNum);
                                //jsonFirmador = firmarNR(cliente, docs.docNum);
                                jsonDTE = JsonConvert.SerializeObject(jsonFirmadorNR.dteJson);//CONVERTIR OBJETO A JSON
                                json = JsonConvert.SerializeObject(jsonFirmadorNR);//CONVERTIR OBJETO A JSON
                                break;
                        }

                        Console.WriteLine(json);

                        WriteToFile("JSON PLANO DOCUMENTOS: " + json);

                        jsonRespuesta = await consumoFirmador.postFirmador("http://" + serverDB + ":8113/firmardocumento/", json); //ENVIAR A SERVICIO DE FIRMADO

                        Response response = JsonConvert.DeserializeObject<Response>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO

                        if (response.status == "OK")
                            jsonFirmador = response.body.ToString();
                        else
                            jsonFirmador = "";

                        documentoDTE.docNum = docs.docNum;
                        documentoDTE.jsonFirmado = jsonFirmador;
                        documentoDTE.jsonFirmadorFE = jsonFirmadorFE;
                        documentoDTE.jsonFirmadorCCF = jsonFirmadorCCF;
                        documentoDTE.jsonFirmadorFEX = jsonFirmadorFEX;
                        documentoDTE.jsonFirmadorFSE = jsonFirmadorFSE;
                        documentoDTE.jsonFirmadorNC = jsonFirmadorNC;
                        documentoDTE.jsonFirmadorND = jsonFirmadorND;
                        documentoDTE.jsonFirmadorNR = jsonFirmadorNR;

                        listJsonFirmador.Add(jsonFirmador);
                        documentosDTE.Add(documentoDTE);
                    }

                    jsonRespuesta = await consumoFirmador.postAutenticador(emisorDTE.ambiente == "00" ? Constantes.urlAutenticadorTest : Constantes.urlAutenticador, emisorDTE.nit, emisorDTE.passPrivDTE);
                    ResponseAutenticador responseAutenticador = JsonConvert.DeserializeObject<ResponseAutenticador>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO
                    if (responseAutenticador.status == "OK")
                    {
                        switch (myComboBox_tipoDoc.Value)
                        {
                            case "1":
                            foreach (DocumentoDTE docDTE in documentosDTE)
                            {
                                jsonReceptor.ambiente = emisorDTE.ambiente;
                                jsonReceptor.idEnvio = docDTE.docNum;
                                jsonReceptor.version = docDTE.jsonFirmadorFE.dteJson.identificacion.version;
                                jsonReceptor.tipoDte = docDTE.jsonFirmadorFE.dteJson.identificacion.tipoDte;
                                jsonReceptor.documento = docDTE.jsonFirmado;
                                jsonReceptor.codigoGeneracion = docDTE.jsonFirmadorFE.dteJson.identificacion.codigoGeneracion.ToString();
                                string jsonreceptor = JsonConvert.SerializeObject(jsonReceptor);//CONVERTIR OBJETO A JSON

                                WriteToFile("JSON FIRMADO FE: " + jsonreceptor);
                                jsonRespuesta = await consumoFirmador.postReceptor(emisorDTE.ambiente == "00" ? Constantes.urlRecepcionTest : Constantes.urlRecepcion, responseAutenticador.body.token, jsonreceptor); //ENVIAR A SERVICIO DE FIRMADO
                                ResponseReceptor responseReceptor = JsonConvert.DeserializeObject<ResponseReceptor>(jsonRespuesta.ToString());

                                WriteToFile("JSON RESPUESTA FE: " + jsonRespuesta);
                                    if (responseReceptor.estado.Equals("PROCESADO"))
                                {
                                    Console.WriteLine("Sello recibido: " + responseReceptor.selloRecibido);
                                    guardarDatosDTE(responseReceptor,
                                                    docDTE.docNum,
                                                    docDTE.jsonFirmadorFE.dteJson.identificacion.numeroControl.ToString(),
                                                    docDTE.jsonFirmadorFE.dteJson.identificacion.tipoModelo,
                                                    docDTE.jsonFirmadorFE.dteJson.identificacion.tipoOperacion,
                                                    docDTE.jsonFirmadorFE.dteJson.identificacion.tipoDte,
                                                    JsonConvert.SerializeObject(docDTE.jsonFirmadorFE.dteJson),
                                                    docDTE.jsonFirmado,
                                                    "D");
                                }
                                else
                                {
                                    mySBO_Application.MessageBox("La factura no pudo ser procesada: \n\n" + jsonRespuesta);
                                }
                            }
                                break;
                            case "3":
                                foreach (DocumentoDTE docDTE in documentosDTE)
                                {
                                    jsonReceptor.ambiente = emisorDTE.ambiente;
                                    jsonReceptor.idEnvio = docDTE.docNum;
                                    jsonReceptor.version = docDTE.jsonFirmadorCCF.dteJson.identificacion.version;
                                    jsonReceptor.tipoDte = docDTE.jsonFirmadorCCF.dteJson.identificacion.tipoDte;
                                    jsonReceptor.documento = docDTE.jsonFirmado;
                                    jsonReceptor.codigoGeneracion = docDTE.jsonFirmadorCCF.dteJson.identificacion.codigoGeneracion.ToString();
                                    string jsonreceptor = JsonConvert.SerializeObject(jsonReceptor);//CONVERTIR OBJETO A JSON
                                    WriteToFile("JSON FIRMADO CCF: " + jsonreceptor);
                                    jsonRespuesta = await consumoFirmador.postReceptor(emisorDTE.ambiente == "00" ? Constantes.urlRecepcionTest : Constantes.urlRecepcion, responseAutenticador.body.token, jsonreceptor); //ENVIAR A SERVICIO DE FIRMADO
                                    ResponseReceptor responseReceptor = JsonConvert.DeserializeObject<ResponseReceptor>(jsonRespuesta.ToString());

                                    WriteToFile("JSON RESPUESTA CCF: " + jsonRespuesta);
                                    if (responseReceptor.estado.Equals("PROCESADO"))
                                    {
                                        Console.WriteLine("Sello recibido: " + responseReceptor.selloRecibido);
                                        guardarDatosDTE(responseReceptor,
                                                        docDTE.docNum,
                                                        docDTE.jsonFirmadorCCF.dteJson.identificacion.numeroControl.ToString(),
                                                        docDTE.jsonFirmadorCCF.dteJson.identificacion.tipoModelo,
                                                        docDTE.jsonFirmadorCCF.dteJson.identificacion.tipoOperacion,
                                                        docDTE.jsonFirmadorCCF.dteJson.identificacion.tipoDte,
                                                        JsonConvert.SerializeObject(docDTE.jsonFirmadorCCF.dteJson),
                                                        docDTE.jsonFirmado,
                                                        "D");
                                    }
                                    else
                                    {
                                        mySBO_Application.MessageBox("La factura no pudo ser procesada: \n\n" + jsonRespuesta);
                                    }
                                }
                                break;
                            case "11":
                                foreach (DocumentoDTE docDTE in documentosDTE)
                                {
                                    jsonReceptor.ambiente = emisorDTE.ambiente;
                                    jsonReceptor.idEnvio = docDTE.docNum;
                                    jsonReceptor.version = docDTE.jsonFirmadorFEX.dteJson.identificacion.version;
                                    jsonReceptor.tipoDte = docDTE.jsonFirmadorFEX.dteJson.identificacion.tipoDte;
                                    jsonReceptor.documento = docDTE.jsonFirmado;
                                    jsonReceptor.codigoGeneracion = docDTE.jsonFirmadorFEX.dteJson.identificacion.codigoGeneracion.ToString();
                                    string jsonreceptor = JsonConvert.SerializeObject(jsonReceptor);//CONVERTIR OBJETO A JSON
                                    WriteToFile("JSON FIRMADO FEX: " + jsonreceptor);

                                    jsonRespuesta = await consumoFirmador.postReceptor(emisorDTE.ambiente == "00" ? Constantes.urlRecepcionTest : Constantes.urlRecepcion, responseAutenticador.body.token, jsonreceptor); //ENVIAR A SERVICIO DE FIRMADO
                                    ResponseReceptor responseReceptor = JsonConvert.DeserializeObject<ResponseReceptor>(jsonRespuesta.ToString());

                                    WriteToFile("JSON RESPUESTA FEX: " + jsonRespuesta);
                                    if (responseReceptor.estado.Equals("PROCESADO"))
                                    {
                                        Console.WriteLine("Sello recibido: " + responseReceptor.selloRecibido);
                                        guardarDatosDTE(responseReceptor,
                                                        docDTE.docNum,
                                                        docDTE.jsonFirmadorFEX.dteJson.identificacion.numeroControl.ToString(),
                                                        docDTE.jsonFirmadorFEX.dteJson.identificacion.tipoModelo,
                                                        docDTE.jsonFirmadorFEX.dteJson.identificacion.tipoOperacion,
                                                        docDTE.jsonFirmadorFEX.dteJson.identificacion.tipoDte,
                                                        JsonConvert.SerializeObject(docDTE.jsonFirmadorFEX.dteJson),
                                                        docDTE.jsonFirmado,
                                                        "D");
                                    }
                                    else
                                    {
                                        mySBO_Application.MessageBox("La factura no pudo ser procesada: \n\n" + jsonRespuesta);
                                    }
                                }
                                break;
                            case "14":
                                foreach (DocumentoDTE docDTE in documentosDTE)
                                {
                                    jsonReceptor.ambiente = emisorDTE.ambiente;
                                    jsonReceptor.idEnvio = docDTE.docNum;
                                    jsonReceptor.version = docDTE.jsonFirmadorFSE.dteJson.identificacion.version;
                                    jsonReceptor.tipoDte = docDTE.jsonFirmadorFSE.dteJson.identificacion.tipoDte;
                                    jsonReceptor.documento = docDTE.jsonFirmado;
                                    jsonReceptor.codigoGeneracion = docDTE.jsonFirmadorFSE.dteJson.identificacion.codigoGeneracion.ToString();
                                    string jsonreceptor = JsonConvert.SerializeObject(jsonReceptor);//CONVERTIR OBJETO A JSON
                                    WriteToFile("JSON FIRMADO FSE " + jsonreceptor);
                                    jsonRespuesta = await consumoFirmador.postReceptor(emisorDTE.ambiente == "00" ? Constantes.urlRecepcionTest : Constantes.urlRecepcion, responseAutenticador.body.token, jsonreceptor); //ENVIAR A SERVICIO DE FIRMADO
                                    ResponseReceptor responseReceptor = JsonConvert.DeserializeObject<ResponseReceptor>(jsonRespuesta.ToString());

                                    WriteToFile("JSON RESPUESTA FSE: " + jsonRespuesta);
                                    if (responseReceptor.estado.Equals("PROCESADO"))
                                    {
                                        Console.WriteLine("Sello recibido: " + responseReceptor.selloRecibido);
                                        guardarDatosDTE(responseReceptor,
                                                        docDTE.docNum,
                                                        docDTE.jsonFirmadorFSE.dteJson.identificacion.numeroControl.ToString(),
                                                        docDTE.jsonFirmadorFSE.dteJson.identificacion.tipoModelo,
                                                        docDTE.jsonFirmadorFSE.dteJson.identificacion.tipoOperacion,
                                                        docDTE.jsonFirmadorFSE.dteJson.identificacion.tipoDte,
                                                        JsonConvert.SerializeObject(docDTE.jsonFirmadorFSE.dteJson),
                                                        docDTE.jsonFirmado,
                                                        "D");
                                    }
                                    else
                                    {
                                        mySBO_Application.MessageBox("La factura no pudo ser procesada: \n\n" + jsonRespuesta);
                                    }
                                } 
                            break;
                            case "5":
                                foreach (DocumentoDTE docDTE in documentosDTE)
                                {
                                    jsonReceptor.ambiente = emisorDTE.ambiente;
                                    jsonReceptor.idEnvio = docDTE.docNum;
                                    jsonReceptor.version = docDTE.jsonFirmadorNC.dteJson.identificacion.version;
                                    jsonReceptor.tipoDte = docDTE.jsonFirmadorNC.dteJson.identificacion.tipoDte;
                                    jsonReceptor.documento = docDTE.jsonFirmado;
                                    jsonReceptor.codigoGeneracion = docDTE.jsonFirmadorNC.dteJson.identificacion.codigoGeneracion.ToString();
                                    string jsonreceptor = JsonConvert.SerializeObject(jsonReceptor);//CONVERTIR OBJETO A JSON

                                    WriteToFile("JSON FIRMADO NC: " + jsonreceptor);
                                    jsonRespuesta = await consumoFirmador.postReceptor(emisorDTE.ambiente == "00" ? Constantes.urlRecepcionTest : Constantes.urlRecepcion, responseAutenticador.body.token, jsonreceptor); //ENVIAR A SERVICIO DE FIRMADO
                                    ResponseReceptor responseReceptor = JsonConvert.DeserializeObject<ResponseReceptor>(jsonRespuesta.ToString());

                                    WriteToFile("JSON RESPUESTA NC: " + jsonRespuesta);

                                    if (responseReceptor.estado.Equals("PROCESADO"))
                                    {
                                        Console.WriteLine("Sello recibido: " + responseReceptor.selloRecibido);
                                        guardarDatosDTE(responseReceptor,
                                                        docDTE.docNum,
                                                        docDTE.jsonFirmadorNC.dteJson.identificacion.numeroControl.ToString(),
                                                        docDTE.jsonFirmadorNC.dteJson.identificacion.tipoModelo,
                                                        docDTE.jsonFirmadorNC.dteJson.identificacion.tipoOperacion,
                                                        docDTE.jsonFirmadorNC.dteJson.identificacion.tipoDte,
                                                        JsonConvert.SerializeObject(docDTE.jsonFirmadorNC.dteJson),
                                                        docDTE.jsonFirmado,
                                                        "D");
                                    }
                                    else
                                    {
                                        mySBO_Application.MessageBox("La factura no pudo ser procesada: \n\n" + jsonRespuesta);
                                    }
                                }
                                break;
                            case "6":
                                foreach (DocumentoDTE docDTE in documentosDTE)
                                {
                                    jsonReceptor.ambiente = emisorDTE.ambiente;
                                    jsonReceptor.idEnvio = docDTE.docNum;
                                    jsonReceptor.version = docDTE.jsonFirmadorND.dteJson.identificacion.version;
                                    jsonReceptor.tipoDte = docDTE.jsonFirmadorND.dteJson.identificacion.tipoDte;
                                    jsonReceptor.documento = docDTE.jsonFirmado;
                                    jsonReceptor.codigoGeneracion = docDTE.jsonFirmadorND.dteJson.identificacion.codigoGeneracion.ToString();
                                    string jsonreceptor = JsonConvert.SerializeObject(jsonReceptor);//CONVERTIR OBJETO A JSON

                                    WriteToFile("JSON FIRMADO ND: " + jsonreceptor);
                                    jsonRespuesta = await consumoFirmador.postReceptor(emisorDTE.ambiente == "00" ? Constantes.urlRecepcionTest : Constantes.urlRecepcion, responseAutenticador.body.token, jsonreceptor); //ENVIAR A SERVICIO DE FIRMADO
                                    ResponseReceptor responseReceptor = JsonConvert.DeserializeObject<ResponseReceptor>(jsonRespuesta.ToString());

                                    WriteToFile("JSON RESPUESTA ND: " + jsonRespuesta);
                                    if (responseReceptor.estado.Equals("PROCESADO"))
                                    {
                                        Console.WriteLine("Sello recibido: " + responseReceptor.selloRecibido);
                                        guardarDatosDTE(responseReceptor,
                                                        docDTE.docNum,
                                                        docDTE.jsonFirmadorND.dteJson.identificacion.numeroControl.ToString(),
                                                        docDTE.jsonFirmadorND.dteJson.identificacion.tipoModelo,
                                                        docDTE.jsonFirmadorND.dteJson.identificacion.tipoOperacion,
                                                        docDTE.jsonFirmadorND.dteJson.identificacion.tipoDte,
                                                        JsonConvert.SerializeObject(docDTE.jsonFirmadorND.dteJson),
                                                        docDTE.jsonFirmado,
                                                        "D");
                                    }
                                    else
                                    {
                                        mySBO_Application.MessageBox("La factura no pudo ser procesada: \n\n" + jsonRespuesta);
                                    }
                                }
                                break;
                            case "4":
                                foreach (DocumentoDTE docDTE in documentosDTE)
                                {
                                    jsonReceptor.ambiente = emisorDTE.ambiente;
                                    jsonReceptor.idEnvio = docDTE.docNum;
                                    jsonReceptor.version = docDTE.jsonFirmadorNR.dteJson.identificacion.version;
                                    jsonReceptor.tipoDte = docDTE.jsonFirmadorNR.dteJson.identificacion.tipoDte;
                                    jsonReceptor.documento = docDTE.jsonFirmado;
                                    jsonReceptor.codigoGeneracion = docDTE.jsonFirmadorNR.dteJson.identificacion.codigoGeneracion.ToString();
                                    string jsonreceptor = JsonConvert.SerializeObject(jsonReceptor);//CONVERTIR OBJETO A JSON

                                    WriteToFile("JSON FIRMADO NR: " + jsonreceptor);
                                    jsonRespuesta = await consumoFirmador.postReceptor(emisorDTE.ambiente == "00" ? Constantes.urlRecepcionTest : Constantes.urlRecepcion, responseAutenticador.body.token, jsonreceptor); //ENVIAR A SERVICIO DE FIRMADO
                                    ResponseReceptor responseReceptor = JsonConvert.DeserializeObject<ResponseReceptor>(jsonRespuesta.ToString());

                                    WriteToFile("JSON RESPUESTA NR: " + jsonRespuesta);
                                    if (responseReceptor.estado.Equals("PROCESADO"))
                                    {
                                        Console.WriteLine("Sello recibido: " + responseReceptor.selloRecibido);
                                        guardarDatosDTE(responseReceptor,
                                                        docDTE.docNum,
                                                        docDTE.jsonFirmadorNR.dteJson.identificacion.numeroControl.ToString(),
                                                        docDTE.jsonFirmadorNR.dteJson.identificacion.tipoModelo,
                                                        docDTE.jsonFirmadorNR.dteJson.identificacion.tipoOperacion,
                                                        docDTE.jsonFirmadorNR.dteJson.identificacion.tipoDte,
                                                        JsonConvert.SerializeObject(docDTE.jsonFirmadorNR.dteJson),
                                                        docDTE.jsonFirmado,
                                                        "D");
                                    }
                                    else
                                    {
                                        mySBO_Application.MessageBox("La factura no pudo ser procesada: \n\n" + jsonRespuesta);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mySBO_Application.StatusBar.SetText("Error CargarInfoDocs: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                Console.WriteLine("Error CargarInfoDocs: " + ex.Message);                
            }
            finally
            {
                mySBO_Company.Disconnect();
            }
        }

        private async Task enviarDTE_Contingencia()
        {
            try
            {
                SAPbouiCOM.IItems oItems;
                SAPbouiCOM.Item oItem;

                int count = 1;
                string jsonFirmador = "";
                string json = "";
                string jsonDTE = "";
                dynamic jsonRespuesta;

                List<DocLote> docsLote = new List<DocLote>();
                List<DocsContingencia> docsContingencias = new List<DocsContingencia>();

                DocsContingencia docsContingencia = new DocsContingencia();

                List<string> listJsonFirmador = new List<string>();
                List<int> listDocNum = new List<int>();
                Cliente cliente;

                jsonReceptorLote jsonReceptorLote = new jsonReceptorLote();

                jsonFirmadorFE jsonFirmadorFE = new jsonFirmadorFE();
                jsonFirmadorCCF jsonFirmadorCCF = new jsonFirmadorCCF();
                jsonFirmadorFEX jsonFirmadorFEX = new jsonFirmadorFEX();
                jsonFirmadorFSE jsonFirmadorFSE = new jsonFirmadorFSE();
                jsonFirmadorNC jsonFirmadorNC = new jsonFirmadorNC();
                jsonFirmadorND jsonFirmadorND = new jsonFirmadorND();
                jsonFirmadorNR jsonFirmadorNR = new jsonFirmadorNR();

                DocumentoDTE documentoDTE = new DocumentoDTE();
                List<DocumentoDTE> documentosDTE = new List<DocumentoDTE>();

                Contingencia Contingencia = new Contingencia();
                ConsumoFirmador consumoFirmador = new ConsumoFirmador();
                EmisorDTE emisorDTE = new EmisorDTE();

                emisorDTE = consultas.obtenerInfoEmisor();

                SAPbouiCOM.ComboBox myComboBox_tipoDoc = (SAPbouiCOM.ComboBox)myForm.Items.Item("EA_cmbTDoc").Specific;
                SAPbouiCOM.ComboBox myComboBoxListContin = (SAPbouiCOM.ComboBox)myForm.Items.Item("EA_cmbTCon").Specific;
                SAPbouiCOM.EditText myEditText_serie = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtSeri").Specific;
                Contingencia = consultas.obtenerContingencia(myComboBoxListContin.Selected.Value);
                docsLote = consultas.obtenerDocsLoteContin(Int32.Parse(myComboBox_tipoDoc.Selected.Value), myEditText_serie.Value, myComboBoxListContin.Selected.Value);

                conectarDI();
                if (mySBO_Company.Connect() != 0)
                {
                    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                }
                else
                {
                    foreach (DocLote docs in docsLote)
                    {
                        documentoDTE = new DocumentoDTE();
                        cliente = new Cliente();
                        docsContingencia = new DocsContingencia();

                        jsonFirmadorFE = new jsonFirmadorFE();
                        jsonFirmadorCCF = new jsonFirmadorCCF();
                        jsonFirmadorFEX = new jsonFirmadorFEX();
                        jsonFirmadorFSE = new jsonFirmadorFSE();
                        jsonFirmadorNC = new jsonFirmadorNC();
                        jsonFirmadorND = new jsonFirmadorND();
                        jsonFirmadorNR = new jsonFirmadorNR();

                        switch (myComboBox_tipoDoc.Value)
                        {
                            case "1":
                                cliente = consultas.obtenerInfoClienteFAC(docs.docNum);
                                jsonFirmadorFE = obtenerJSONFE(cliente, docs.docNum);
                                jsonFirmadorFE.dteJson.identificacion.tipoModelo = 2;
                                jsonFirmadorFE.dteJson.identificacion.tipoOperacion = 2;
                                jsonFirmadorFE.dteJson.identificacion.tipoContingencia = Int32.Parse(Contingencia.tipoContingencia);
                                jsonFirmadorFE.dteJson.identificacion.fecEmi = Contingencia.fechaIni;
                                jsonFirmadorFE.dteJson.identificacion.horEmi = Contingencia.horaIni + ":00:00";
                                if (Contingencia.motivo!="" && Contingencia.motivo !=null)
                                    jsonFirmadorFE.dteJson.identificacion.motivoContin = Contingencia.motivo;

                                docsContingencia.noItem = count;
                                docsContingencia.codigoGeneracion = jsonFirmadorFE.dteJson.identificacion.codigoGeneracion.ToString();
                                docsContingencia.tipoDoc = "01";

                                jsonDTE = JsonConvert.SerializeObject(jsonFirmadorFE.dteJson);//CONVERTIR OBJETO A JSON
                                WriteToFile("JSON PLANO FE: " + jsonDTE);
                                json = JsonConvert.SerializeObject(jsonFirmadorFE);//CONVERTIR OBJETO A JSON

                                break;
                            case "3":
                                cliente = consultas.obtenerInfoClienteFAC(docs.docNum);
                                jsonFirmadorCCF = obtenerJSONCCF(cliente, docs.docNum);
                                jsonFirmadorCCF.dteJson.identificacion.tipoModelo = 2;
                                jsonFirmadorCCF.dteJson.identificacion.tipoOperacion = 2;
                                jsonFirmadorCCF.dteJson.identificacion.tipoContingencia = Int32.Parse(Contingencia.tipoContingencia);
                                jsonFirmadorCCF.dteJson.identificacion.fecEmi = Contingencia.fechaIni;
                                jsonFirmadorCCF.dteJson.identificacion.horEmi = Contingencia.horaIni + ":00:00";
                                if (Contingencia.motivo != "" && Contingencia.motivo != null)
                                    jsonFirmadorCCF.dteJson.identificacion.motivoContin = Contingencia.motivo;

                                docsContingencia.noItem = count;
                                docsContingencia.codigoGeneracion = jsonFirmadorCCF.dteJson.identificacion.codigoGeneracion.ToString();
                                docsContingencia.tipoDoc = "03";

                                jsonDTE = JsonConvert.SerializeObject(jsonFirmadorCCF.dteJson);//CONVERTIR OBJETO A JSON
                                WriteToFile("JSON PLANO CCF: " + jsonDTE);
                                json = JsonConvert.SerializeObject(jsonFirmadorCCF);//CONVERTIR OBJETO A JSON
                                break;
                            case "11":
                                cliente = consultas.obtenerInfoClienteFAC(docs.docNum);
                                jsonFirmadorFEX = obtenerJSONFEX(cliente, docs.docNum);
                                jsonFirmadorFEX.dteJson.identificacion.tipoModelo = 2;
                                jsonFirmadorFEX.dteJson.identificacion.tipoOperacion = 2;
                                jsonFirmadorFEX.dteJson.identificacion.tipoContingencia = Int32.Parse(Contingencia.tipoContingencia);
                                jsonFirmadorFEX.dteJson.identificacion.fecEmi = Contingencia.fechaIni;
                                jsonFirmadorFEX.dteJson.identificacion.horEmi = Contingencia.horaIni + ":00:00";
                                if (Contingencia.motivo != "" && Contingencia.motivo != null)
                                    jsonFirmadorFEX.dteJson.identificacion.motivoContigencia = Contingencia.motivo;

                                docsContingencia.noItem = count;
                                docsContingencia.codigoGeneracion = jsonFirmadorFEX.dteJson.identificacion.codigoGeneracion.ToString();
                                docsContingencia.tipoDoc = "11";

                                jsonDTE = JsonConvert.SerializeObject(jsonFirmadorFEX.dteJson);//CONVERTIR OBJETO A JSON
                                WriteToFile("JSON PLANO FEX: " + jsonDTE);
                                json = JsonConvert.SerializeObject(jsonFirmadorFEX);//CONVERTIR OBJETO A JSON
                                break;
                            case "14":
                                cliente = consultas.obtenerInfoClienteFSE(docs.docNum);
                                jsonFirmadorFSE = obtenerJSONFSE(cliente, docs.docNum);
                                jsonFirmadorFSE.dteJson.identificacion.tipoModelo = 2;
                                jsonFirmadorFSE.dteJson.identificacion.tipoOperacion = 2;
                                jsonFirmadorFSE.dteJson.identificacion.tipoContingencia = Int32.Parse(Contingencia.tipoContingencia);
                                jsonFirmadorFSE.dteJson.identificacion.fecEmi = Contingencia.fechaIni;
                                jsonFirmadorFSE.dteJson.identificacion.horEmi = Contingencia.horaIni + ":00:00";
                                if (Contingencia.motivo != "" && Contingencia.motivo != null)
                                    jsonFirmadorFSE.dteJson.identificacion.motivoContin = Contingencia.motivo;

                                docsContingencia.noItem = count;
                                docsContingencia.codigoGeneracion = jsonFirmadorFSE.dteJson.identificacion.codigoGeneracion.ToString();
                                docsContingencia.tipoDoc = "14";

                                jsonDTE = JsonConvert.SerializeObject(jsonFirmadorFSE.dteJson);//CONVERTIR OBJETO A JSON
                                WriteToFile("JSON PLANO FSE: " + jsonDTE);
                                json = JsonConvert.SerializeObject(jsonFirmadorFSE);//CONVERTIR OBJETO A JSON
                                break;
                            case "5":
                                cliente = consultas.obtenerInfoClienteNC(docs.docNum);
                                jsonFirmadorNC = obtenerJSONNC(cliente, docs.docNum);
                                jsonFirmadorNC.dteJson.identificacion.tipoModelo = 2;
                                jsonFirmadorNC.dteJson.identificacion.tipoOperacion = 2;
                                jsonFirmadorNC.dteJson.identificacion.tipoContingencia = Int32.Parse(Contingencia.tipoContingencia);
                                jsonFirmadorNC.dteJson.identificacion.fecEmi = Contingencia.fechaIni;
                                jsonFirmadorNC.dteJson.identificacion.horEmi = Contingencia.horaIni + ":00:00";
                                if (Contingencia.motivo != "" && Contingencia.motivo != null)
                                    jsonFirmadorNC.dteJson.identificacion.motivoContin = Contingencia.motivo;

                                docsContingencia.noItem = count;
                                docsContingencia.codigoGeneracion = jsonFirmadorNC.dteJson.identificacion.codigoGeneracion.ToString();
                                docsContingencia.tipoDoc = "05";

                                jsonDTE = JsonConvert.SerializeObject(jsonFirmadorNC.dteJson);//CONVERTIR OBJETO A JSON
                                WriteToFile("JSON PLANO NC: " + jsonDTE);
                                json = JsonConvert.SerializeObject(jsonFirmadorNC);//CONVERTIR OBJETO A JSON
                                break;
                            case "6":
                                cliente = consultas.obtenerInfoClienteND(docs.docNum);
                                jsonFirmadorND = obtenerJSONND(cliente, docs.docNum);
                                jsonFirmadorND.dteJson.identificacion.tipoModelo = 2;
                                jsonFirmadorND.dteJson.identificacion.tipoOperacion = 2;
                                jsonFirmadorND.dteJson.identificacion.tipoContingencia = Int32.Parse(Contingencia.tipoContingencia);
                                jsonFirmadorND.dteJson.identificacion.fecEmi = Contingencia.fechaIni;
                                jsonFirmadorND.dteJson.identificacion.horEmi = Contingencia.horaIni + ":00:00";
                                if (Contingencia.motivo != "" && Contingencia.motivo != null)
                                    jsonFirmadorND.dteJson.identificacion.motivoContin = Contingencia.motivo;

                                docsContingencia.noItem = count;
                                docsContingencia.codigoGeneracion = jsonFirmadorND.dteJson.identificacion.codigoGeneracion.ToString();
                                docsContingencia.tipoDoc = "06";

                                jsonDTE = JsonConvert.SerializeObject(jsonFirmadorND.dteJson);//CONVERTIR OBJETO A JSON
                                WriteToFile("JSON PLANO NC: " + jsonDTE);
                                json = JsonConvert.SerializeObject(jsonFirmadorND);//CONVERTIR OBJETO A JSON
                                break;
                            case "4":
                                cliente = consultas.obtenerInfoClienteNR(docs.docNum);
                                jsonFirmadorNR = obtenerJSONNR(cliente, docs.docNum);
                                jsonFirmadorNR.dteJson.identificacion.tipoModelo = 2;
                                jsonFirmadorNR.dteJson.identificacion.tipoOperacion = 2;
                                jsonFirmadorNR.dteJson.identificacion.tipoContingencia = Int32.Parse(Contingencia.tipoContingencia);
                                jsonFirmadorNR.dteJson.identificacion.fecEmi = Contingencia.fechaIni;
                                jsonFirmadorNR.dteJson.identificacion.horEmi = Contingencia.horaIni + ":00:00";
                                if (Contingencia.motivo != "" && Contingencia.motivo != null)
                                    jsonFirmadorNR.dteJson.identificacion.motivoContin = Contingencia.motivo;

                                docsContingencia.noItem = count;
                                docsContingencia.codigoGeneracion = jsonFirmadorNR.dteJson.identificacion.codigoGeneracion.ToString();
                                docsContingencia.tipoDoc = "04";

                                jsonDTE = JsonConvert.SerializeObject(jsonFirmadorNR.dteJson);//CONVERTIR OBJETO A JSON
                                WriteToFile("JSON PLANO NR: " + jsonDTE);
                                json = JsonConvert.SerializeObject(jsonFirmadorNR);//CONVERTIR OBJETO A JSON
                                break;
                        }

                        Console.WriteLine(json);

                        jsonRespuesta = await consumoFirmador.postFirmador("http://" + serverDB + ":8113/firmardocumento/", json); //ENVIAR A SERVICIO DE FIRMADO

                        Response response = JsonConvert.DeserializeObject<Response>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO

                        if (response.status == "OK")
                            jsonFirmador = response.body.ToString();
                        else
                            jsonFirmador = "";

                        documentoDTE.docNum = docs.docNum;
                        documentoDTE.jsonFirmado = jsonFirmador;
                        documentoDTE.jsonFirmadorFE = jsonFirmadorFE;
                        documentoDTE.jsonFirmadorCCF = jsonFirmadorCCF;
                        documentoDTE.jsonFirmadorFEX = jsonFirmadorFEX;
                        documentoDTE.jsonFirmadorFSE = jsonFirmadorFSE;
                        documentoDTE.jsonFirmadorNC = jsonFirmadorNC;
                        documentoDTE.jsonFirmadorND = jsonFirmadorND;
                        documentoDTE.jsonFirmadorNR = jsonFirmadorNR;

                        listJsonFirmador.Add(jsonFirmador);
                        documentosDTE.Add(documentoDTE);
                        docsContingencias.Add(docsContingencia);
                        count++;
                    }


                    jsonFirmadorContingencia jsonFirmadorContingencia = new jsonFirmadorContingencia();//json a transmitir
                    jsonFirmadorContingencia.contentType = "application/JSON";
                    jsonFirmadorContingencia.nit = emisorDTE.userDTE;
                    jsonFirmadorContingencia.activo = "True";
                    jsonFirmadorContingencia.passwordPri = emisorDTE.passDTE;


                    EmisorContingencia emisorContingencia = new EmisorContingencia();
                    emisorContingencia.nit = emisorDTE.nit;
                    emisorContingencia.nombre = emisorDTE.nombre;
                    emisorContingencia.nombreResponsable = emisorDTE.nombre;
                    emisorContingencia.tipoDocResponsable = "36";
                    emisorContingencia.numeroDocResponsable = emisorDTE.nit;
                    emisorContingencia.tipoEstablecimiento = emisorDTE.tipoEstablecimiento;
                    emisorContingencia.telefono = emisorDTE.telefono;
                    emisorContingencia.correo = emisorDTE.correo;

                    IdentificacionContingencia identificacionContingencia = new IdentificacionContingencia();
                    identificacionContingencia.version = 3;
                    identificacionContingencia.ambiente = emisorDTE.ambiente;
                    identificacionContingencia.codigoGeneracion = Guid.NewGuid().ToString().ToUpper();
                    identificacionContingencia.fTransmision = DateTime.Now.ToString("yyyy-MM-dd");
                    identificacionContingencia.hTransmision = DateTime.Now.ToString("HH:mm:ss");

                    MotivoContingencia motivoContingencia = new MotivoContingencia();
                    motivoContingencia.tipoContingencia = Int32.Parse(Contingencia.tipoContingencia) ;
                    motivoContingencia.fInicio = Contingencia.fechaIni;
                    motivoContingencia.fFin = Contingencia.fechaFin;
                    motivoContingencia.hInicio = Contingencia.horaIni + ":00:00"; 
                    motivoContingencia.hFin = Contingencia.horaFin + ":00:00";
                    if (Contingencia.motivo != "" && Contingencia.motivo != null)
                        motivoContingencia.motivoContingencia = Contingencia.motivo;

                    JSONContingencia jSONContingencia = new JSONContingencia();
                    jSONContingencia.detalleDTE = new List<DocsContingencia>();
                    jSONContingencia.detalleDTE.AddRange(docsContingencias);
                    jSONContingencia.emisor = emisorContingencia;
                    jSONContingencia.identificacion = identificacionContingencia;
                    jSONContingencia.motivo = motivoContingencia;

                    jsonFirmadorContingencia.dteJson = jSONContingencia;

                    json = JsonConvert.SerializeObject(jsonFirmadorContingencia);

                    WriteToFile("JSON PLANO CONTINGENCIA: " + json);

                    jsonRespuesta = await consumoFirmador.postFirmador("http://" + serverDB + ":8113/firmardocumento/", json); //ENVIAR A SERVICIO DE FIRMADO
                    Response responseFirma = JsonConvert.DeserializeObject<Response>(jsonRespuesta.ToString());
                    jsonContingencia jsonContingencia = new jsonContingencia();
                    jsonContingencia.nit = emisorDTE.nit;
                    jsonContingencia.documento = responseFirma.body.ToString();

                    string jsonConti = JsonConvert.SerializeObject(jsonContingencia);

                    Console.WriteLine(jsonConti);
                    WriteToFile("JSON FIRMADO CONTINGENCIA: " + jsonConti);

                    jsonRespuesta = await consumoFirmador.postAutenticador(emisorDTE.ambiente == "00" ? Constantes.urlAutenticadorTest : Constantes.urlAutenticador, emisorDTE.nit, emisorDTE.passPrivDTE);
                    ResponseAutenticador responseAutenticador = JsonConvert.DeserializeObject<ResponseAutenticador>(jsonRespuesta.ToString());  // CONVERTIR RESPUESTA EN JSON A OBJETO
                    if (responseAutenticador.status == "OK")
                    {

                        jsonRespuesta = await consumoFirmador.postContingencia(emisorDTE.ambiente == "00" ? Constantes.urlContingenciaTest : Constantes.urlContingencia, responseAutenticador.body.token, jsonConti); //ENVIAR A SERVICIO DE FIRMADO
                        ResponseContingencia responseContingencia = JsonConvert.DeserializeObject<ResponseContingencia>(jsonRespuesta.ToString());

                            WriteToFile("JSON RESPUESTA COTINGENCIA: " + jsonRespuesta);
                        if (responseContingencia.estado == "RECIBIDO")
                        {

                            jsonReceptorLote.ambiente = emisorDTE.ambiente;
                            jsonReceptorLote.idEnvio = Guid.NewGuid().ToString().ToUpper();
                            jsonReceptorLote.version = 3;
                            jsonReceptorLote.nitEmisor = emisorDTE.nit;
                            jsonReceptorLote.documentos = listJsonFirmador;

                            string jsonreceptor = JsonConvert.SerializeObject(jsonReceptorLote);//CONVERTIR OBJETO A JSON
                            Console.WriteLine(jsonreceptor);
                            WriteToFile("JSON FIRMADO LOTE: " + jsonreceptor);

                            jsonRespuesta = await consumoFirmador.postLote(emisorDTE.ambiente == "00" ? Constantes.urlLoteTest : Constantes.urlLote, responseAutenticador.body.token, jsonreceptor); //ENVIAR A SERVICIO DE FIRMADO
                            ResponseReceptorLote responseReceptorLote = JsonConvert.DeserializeObject<ResponseReceptorLote>(jsonRespuesta.ToString());

                            Console.WriteLine(jsonRespuesta);
                            WriteToFile("JSON RESPUESTA LOTE: " + jsonRespuesta);

                            if (responseReceptorLote.codigoMsg.Equals("001") || responseReceptorLote.codigoMsg.Equals("002"))
                            {
                                jsonRespuesta = await consumoFirmador.getConsultaLote(emisorDTE.ambiente == "00" ? Constantes.urlConsultaLoteTest : Constantes.urlConsultaLote, responseAutenticador.body.token, responseReceptorLote.codigoLote);
                                ConsultaLote consultaLote = JsonConvert.DeserializeObject<ConsultaLote>(jsonRespuesta.ToString());

                                WriteToFile("JSON RESPUESTA CONSULTA LOTE: " + jsonRespuesta);
                                switch (myComboBox_tipoDoc.Value)
                                {
                                    case "1":
                                        foreach (DocumentoDTE docDTE in documentosDTE)
                                        {
                                            foreach (ResponseReceptor responseReceptor in consultaLote.procesados)
                                            {
                                                if (responseReceptor.codigoGeneracion == docDTE.jsonFirmadorFE.dteJson.identificacion.codigoGeneracion.ToString())
                                                {
                                                    guardarDatosLoteDTE(responseReceptor,
                                                                        docDTE.docNum,
                                                                        docDTE.jsonFirmadorFE.dteJson.identificacion.numeroControl.ToString(),
                                                                        docDTE.jsonFirmadorFE.dteJson.identificacion.tipoModelo,
                                                                        docDTE.jsonFirmadorFE.dteJson.identificacion.tipoOperacion,
                                                                        "01",
                                                                        JsonConvert.SerializeObject(docDTE.jsonFirmadorFE.dteJson),
                                                                        docDTE.jsonFirmado,
                                                                        responseReceptorLote.codigoLote);
                                                    guardarDatosContingenciaDTE(responseContingencia,
                                                                                docDTE.docNum, 
                                                                                myComboBoxListContin.Selected.Value,
                                                                                "01",
                                                                                JsonConvert.SerializeObject(jsonFirmadorContingencia));
                                                }
                                            }

                                        }
                                        break;
                                    case "3":
                                        foreach (DocumentoDTE docDTE in documentosDTE)
                                        {
                                            foreach (ResponseReceptor responseReceptor in consultaLote.procesados)
                                            {
                                                if (responseReceptor.codigoGeneracion == docDTE.jsonFirmadorCCF.dteJson.identificacion.codigoGeneracion.ToString())
                                                {
                                                    guardarDatosLoteDTE(responseReceptor,
                                                                        docDTE.docNum,
                                                                        docDTE.jsonFirmadorCCF.dteJson.identificacion.numeroControl.ToString(),
                                                                        docDTE.jsonFirmadorCCF.dteJson.identificacion.tipoModelo,
                                                                        docDTE.jsonFirmadorCCF.dteJson.identificacion.tipoOperacion,
                                                                        "03",
                                                                        JsonConvert.SerializeObject(docDTE.jsonFirmadorCCF.dteJson),
                                                                        docDTE.jsonFirmado,
                                                                        responseReceptorLote.codigoLote);
                                                    guardarDatosContingenciaDTE(responseContingencia,
                                                                                docDTE.docNum,
                                                                                myComboBoxListContin.Selected.Value,
                                                                                "03",
                                                                                JsonConvert.SerializeObject(jsonFirmadorContingencia));
                                                }
                                            }
                                        }
                                        break;
                                    case "11":
                                        foreach (DocumentoDTE docDTE in documentosDTE)
                                        {
                                            foreach (ResponseReceptor responseReceptor in consultaLote.procesados)
                                            {
                                                if (responseReceptor.codigoGeneracion == docDTE.jsonFirmadorFEX.dteJson.identificacion.codigoGeneracion.ToString())
                                                {

                                                    guardarDatosLoteDTE(responseReceptor,
                                                                        docDTE.docNum,
                                                                        docDTE.jsonFirmadorFEX.dteJson.identificacion.numeroControl.ToString(),
                                                                        docDTE.jsonFirmadorFEX.dteJson.identificacion.tipoModelo,
                                                                        docDTE.jsonFirmadorFEX.dteJson.identificacion.tipoOperacion,
                                                                        "11",
                                                                        JsonConvert.SerializeObject(docDTE.jsonFirmadorFEX.dteJson),
                                                                        docDTE.jsonFirmado,
                                                                        responseReceptorLote.codigoLote);
                                                    guardarDatosContingenciaDTE(responseContingencia,
                                                                                docDTE.docNum,
                                                                                myComboBoxListContin.Selected.Value,
                                                                                "11",
                                                                                JsonConvert.SerializeObject(jsonFirmadorContingencia));
                                                }
                                            }
                                        }
                                        break;
                                    case "14":
                                        foreach (DocumentoDTE docDTE in documentosDTE)
                                        {
                                            foreach (ResponseReceptor responseReceptor in consultaLote.procesados)
                                            {
                                                if (responseReceptor.codigoGeneracion == docDTE.jsonFirmadorFSE.dteJson.identificacion.codigoGeneracion.ToString())
                                                {
                                                    guardarDatosLoteDTE(responseReceptor,
                                                                        docDTE.docNum,
                                                                        docDTE.jsonFirmadorFSE.dteJson.identificacion.numeroControl.ToString(),
                                                                        docDTE.jsonFirmadorFSE.dteJson.identificacion.tipoModelo,
                                                                        docDTE.jsonFirmadorFSE.dteJson.identificacion.tipoOperacion,
                                                                        "14",
                                                                        JsonConvert.SerializeObject(docDTE.jsonFirmadorFSE.dteJson),
                                                                        docDTE.jsonFirmado,
                                                                        responseReceptorLote.codigoLote);
                                                    guardarDatosContingenciaDTE(responseContingencia,
                                                                                docDTE.docNum,
                                                                                myComboBoxListContin.Selected.Value,
                                                                                "14",
                                                                                JsonConvert.SerializeObject(jsonFirmadorContingencia));
                                                }
                                            }
                                        }
                                        break;
                                    case "5":
                                        foreach (DocumentoDTE docDTE in documentosDTE)
                                        {
                                            foreach (ResponseReceptor responseReceptor in consultaLote.procesados)
                                            {
                                                if (responseReceptor.codigoGeneracion == docDTE.jsonFirmadorNC.dteJson.identificacion.codigoGeneracion.ToString())
                                                {
                                                    guardarDatosLoteDTE(responseReceptor,
                                                                        docDTE.docNum,
                                                                        docDTE.jsonFirmadorNC.dteJson.identificacion.numeroControl.ToString(),
                                                                        docDTE.jsonFirmadorNC.dteJson.identificacion.tipoModelo,
                                                                        docDTE.jsonFirmadorNC.dteJson.identificacion.tipoOperacion,
                                                                        "05",
                                                                        JsonConvert.SerializeObject(docDTE.jsonFirmadorNC.dteJson),
                                                                        docDTE.jsonFirmado,
                                                                        responseReceptorLote.codigoLote);
                                                    guardarDatosContingenciaDTE(responseContingencia,
                                                                                docDTE.docNum,
                                                                                myComboBoxListContin.Selected.Value,
                                                                                "05",
                                                                                JsonConvert.SerializeObject(jsonFirmadorContingencia));
                                                }
                                            }
                                        }
                                        break;
                                    case "6":
                                        foreach (DocumentoDTE docDTE in documentosDTE)
                                        {
                                            foreach (ResponseReceptor responseReceptor in consultaLote.procesados)
                                            {
                                                if (responseReceptor.codigoGeneracion == docDTE.jsonFirmadorND.dteJson.identificacion.codigoGeneracion.ToString())
                                                {
                                                    guardarDatosLoteDTE(responseReceptor,
                                                                        docDTE.docNum,
                                                                        docDTE.jsonFirmadorND.dteJson.identificacion.numeroControl.ToString(),
                                                                        docDTE.jsonFirmadorND.dteJson.identificacion.tipoModelo,
                                                                        docDTE.jsonFirmadorND.dteJson.identificacion.tipoOperacion,
                                                                        "06",
                                                                        JsonConvert.SerializeObject(docDTE.jsonFirmadorND.dteJson),
                                                                        docDTE.jsonFirmado,
                                                                        responseReceptorLote.codigoLote);
                                                    guardarDatosContingenciaDTE(responseContingencia,
                                                                                docDTE.docNum,
                                                                                myComboBoxListContin.Selected.Value,
                                                                                "06",
                                                                                JsonConvert.SerializeObject(jsonFirmadorContingencia));
                                                }
                                            }
                                        }
                                        break;
                                    case "4":
                                        foreach (DocumentoDTE docDTE in documentosDTE)
                                        {
                                            foreach (ResponseReceptor responseReceptor in consultaLote.procesados)
                                            {
                                                if (responseReceptor.codigoGeneracion == docDTE.jsonFirmadorNR.dteJson.identificacion.codigoGeneracion.ToString())
                                                {
                                                    guardarDatosLoteDTE(responseReceptor,
                                                                        docDTE.docNum,
                                                                        docDTE.jsonFirmadorNR.dteJson.identificacion.numeroControl.ToString(),
                                                                        docDTE.jsonFirmadorNR.dteJson.identificacion.tipoModelo,
                                                                        docDTE.jsonFirmadorNR.dteJson.identificacion.tipoOperacion,
                                                                        "04",
                                                                        JsonConvert.SerializeObject(docDTE.jsonFirmadorNR.dteJson),
                                                                        docDTE.jsonFirmado,
                                                                        responseReceptorLote.codigoLote);
                                                    guardarDatosContingenciaDTE(responseContingencia,
                                                                                docDTE.docNum,
                                                                                myComboBoxListContin.Selected.Value,
                                                                                "04",
                                                                                JsonConvert.SerializeObject(jsonFirmadorContingencia));
                                                }
                                            }
                                        }
                                        break;
                                }

                                mySBO_Application.MessageBox("Documentos no procesados: \n\n" + JsonConvert.SerializeObject(consultaLote.rechazados));
                            }
                            else
                            {
                                mySBO_Application.MessageBox("Lote no pudo ser procesado: \n\n" + jsonRespuesta);
                            }
                        }
                        else {
                            mySBO_Application.MessageBox("Contingencia no procesada: \n\n" + JsonConvert.SerializeObject(responseContingencia));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mySBO_Application.StatusBar.SetText("Error CargarInfoDocs1: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                Console.WriteLine("Error CargarInfoDocs1: " + ex.Message);
            }
            finally
            {
                mySBO_Company.Disconnect();
            }
        }

        private async Task stringJSONContingencia() {

            try
            {

                ConsumoFirmador consumoFirmador = new ConsumoFirmador();

                jsonReceptor jsonReceptor = new jsonReceptor();

                dynamic jsonRespuesta;



                EmisorDTE emisorDTE = new EmisorDTE();
                emisorDTE = consultas.obtenerInfoEmisor();

                jsonFirmadorContingencia jsonFirmadorContingencia = new jsonFirmadorContingencia();//json a transmitir
                jsonFirmadorContingencia.contentType = "application/JSON";
                jsonFirmadorContingencia.nit = emisorDTE.userDTE;
                jsonFirmadorContingencia.activo = "True";
                jsonFirmadorContingencia.passwordPri = emisorDTE.passDTE;

                DocsContingencia docsContingencia = new DocsContingencia();
                docsContingencia.noItem = 1;
                docsContingencia.codigoGeneracion = Guid.NewGuid().ToString().ToUpper();
                docsContingencia.tipoDoc = "01";

                EmisorContingencia emisorContingencia = new EmisorContingencia();
                emisorContingencia.nit = emisorDTE.nit;
                emisorContingencia.nombre = emisorDTE.nombre;
                emisorContingencia.nombreResponsable = emisorDTE.nombre;
                emisorContingencia.tipoDocResponsable = "36";
                emisorContingencia.numeroDocResponsable = emisorDTE.nit;
                emisorContingencia.tipoEstablecimiento = emisorDTE.tipoEstablecimiento;
                emisorContingencia.telefono = emisorDTE.telefono;
                emisorContingencia.correo = emisorDTE.correo;

                IdentificacionContingencia identificacionContingencia = new IdentificacionContingencia();
                identificacionContingencia.version = 3;
                identificacionContingencia.ambiente = "00";
                identificacionContingencia.codigoGeneracion = Guid.NewGuid().ToString().ToUpper();
                identificacionContingencia.fTransmision = DateTime.Now.ToString("yyyy-MM-dd");
                identificacionContingencia.hTransmision = DateTime.Now.ToString("HH:mm:ss");

                MotivoContingencia motivoContingencia = new MotivoContingencia();
                motivoContingencia.tipoContingencia = 2;
                motivoContingencia.fInicio = "2024-02-22";//DateTime.Now.ToString("yyyy-MM-dd");
                motivoContingencia.fFin = "2024-02-22";//DateTime.Now.ToString("HH:mm:ss");
                motivoContingencia.hInicio = "08:00:00"; //DateTime.Now.ToString("yyyy-MM-dd");
                motivoContingencia.hFin = "13:00:00"; //DateTime.Now.ToString("HH:mm:ss"); 

                JSONContingencia jSONContingencia = new JSONContingencia();
                jSONContingencia.detalleDTE = new List<DocsContingencia>();
                jSONContingencia.detalleDTE.Add(docsContingencia);
                jSONContingencia.emisor = emisorContingencia;
                jSONContingencia.identificacion = identificacionContingencia;
                jSONContingencia.motivo = motivoContingencia;

                jsonFirmadorContingencia.dteJson = jSONContingencia;

                string json = JsonConvert.SerializeObject(jsonFirmadorContingencia);

                jsonRespuesta = await consumoFirmador.postFirmador("http://" + serverDB + ":8113/firmardocumento/", json); //ENVIAR A SERVICIO DE FIRMADO
                Response response = JsonConvert.DeserializeObject<Response>(jsonRespuesta.ToString());
                jsonContingencia jsonContingencia = new jsonContingencia();
                jsonContingencia.nit = emisorDTE.nit;
                jsonContingencia.documento = response.body.ToString();
                Console.WriteLine(JsonConvert.SerializeObject(jsonFirmadorContingencia));
                Console.WriteLine(JsonConvert.SerializeObject(jsonContingencia));
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
        private void cargarContingencias()
        {
            try
            {
                SAPbouiCOM.IItems oItems;
                SAPbouiCOM.Item oItem;
                List<Contingencia> contingencias = new List<Contingencia>();
                int count = 0;

                myForm.DataSources.DataTables.Item("EA_tabCon1").Clear();

                myForm.DataSources.DataTables.Item("EA_tabCon1").Columns.Add("Tipo contingencia", SAPbouiCOM.BoFieldsType.ft_Text, 5);
                myForm.DataSources.DataTables.Item("EA_tabCon1").Columns.Add("Desc contingencia", SAPbouiCOM.BoFieldsType.ft_Text, 100);
                myForm.DataSources.DataTables.Item("EA_tabCon1").Columns.Add("motivo", SAPbouiCOM.BoFieldsType.ft_Text, 200);
                myForm.DataSources.DataTables.Item("EA_tabCon1").Columns.Add("Fecha inicio", SAPbouiCOM.BoFieldsType.ft_Text, 10);
                myForm.DataSources.DataTables.Item("EA_tabCon1").Columns.Add("Hora inicio", SAPbouiCOM.BoFieldsType.ft_Text, 10);
                myForm.DataSources.DataTables.Item("EA_tabCon1").Columns.Add("Fecha fin", SAPbouiCOM.BoFieldsType.ft_Text, 10);
                myForm.DataSources.DataTables.Item("EA_tabCon1").Columns.Add("Hora fin", SAPbouiCOM.BoFieldsType.ft_Text, 10);
                myForm.DataSources.DataTables.Item("EA_tabCon1").Columns.Add("Sello", SAPbouiCOM.BoFieldsType.ft_Text, 20);

                contingencias = consultas.obtenerContingencias();

                foreach (Contingencia contingencia in contingencias)
                {
                    myForm.DataSources.DataTables.Item("EA_tabCon1").Rows.Add();
                    myForm.DataSources.DataTables.Item("EA_tabCon1").SetValue("Tipo contingencia", count, contingencia.tipoContingencia);
                    myForm.DataSources.DataTables.Item("EA_tabCon1").SetValue("Desc contingencia", count, contingencia.descContingencia);
                    myForm.DataSources.DataTables.Item("EA_tabCon1").SetValue("motivo", count, contingencia.motivo);
                    myForm.DataSources.DataTables.Item("EA_tabCon1").SetValue("Fecha inicio", count, contingencia.fechaIni);
                    myForm.DataSources.DataTables.Item("EA_tabCon1").SetValue("Hora inicio", count, contingencia.horaIni);
                    myForm.DataSources.DataTables.Item("EA_tabCon1").SetValue("Fecha fin", count, contingencia.fechaFin);
                    myForm.DataSources.DataTables.Item("EA_tabCon1").SetValue("Hora fin", count, contingencia.horaFin);
                    myForm.DataSources.DataTables.Item("EA_tabCon1").SetValue("Sello", count, contingencia.selloRecepcion);
                    count++;
                }


                SAPbouiCOM.Grid myGrid = (SAPbouiCOM.Grid)myForm.Items.Item("EA_tabCont").Specific;
                myGrid.DataTable = myForm.DataSources.DataTables.Item("EA_tabCon1");
                myGrid.Item.Enabled = false;
                myForm.Update();

            }
            catch (Exception ex)
            {
                mySBO_Application.StatusBar.SetText("Error cargarContingencias: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                //mySBO_Company.Disconnect();
            }
        }

        private void guardaContingencia()
        {
            try
            {
                SAPbouiCOM.IItems oItems;
                SAPbouiCOM.Item oItem;

                conectarDI();
                if (mySBO_Company.Connect() != 0)
                {
                    mySBO_Application.MessageBox("Error de conexion a la base datos: " + mySBO_Company.GetLastErrorDescription());
                }
                else
                {
                    SAPbobsCOM.UserTable oUserTable = mySBO_Company.UserTables.Item("FE_CONTINGENCIA");

                    SAPbouiCOM.ComboBox myComboBox_tipoCont = (SAPbouiCOM.ComboBox)myForm.Items.Item("EA_cmbTCon").Specific;
                    SAPbouiCOM.EditText myEditText_Motivo = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtMoti").Specific;
                    SAPbouiCOM.EditText myEditText_FechaIni = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtFeDe").Specific;
                    SAPbouiCOM.EditText myEditText_FechaFin = (SAPbouiCOM.EditText)myForm.Items.Item("EA_txtFeAl").Specific;
                    SAPbouiCOM.ComboBox myComboBox_HoraIni = (SAPbouiCOM.ComboBox)myForm.Items.Item("EA_cbHrIni").Specific;
                    SAPbouiCOM.ComboBox myComboBox_HoraFin = (SAPbouiCOM.ComboBox)myForm.Items.Item("EA_cbHrFin").Specific;


                    DateTime fechaIni= DateTime.ParseExact(myEditText_FechaIni.Value, "yyyyMMdd", CultureInfo.InvariantCulture);
                    DateTime fechaFin = DateTime.ParseExact(myEditText_FechaFin.Value, "yyyyMMdd", CultureInfo.InvariantCulture);

                    oUserTable.Code = myComboBox_tipoCont.Selected.Value + "_" + myEditText_FechaIni.Value + "_" + myComboBox_HoraIni.Selected.Value;
                    oUserTable.Name = myComboBox_tipoCont.Selected.Value + "_" + myEditText_FechaIni.Value + "_" + myComboBox_HoraIni.Selected.Value;
                    oUserTable.UserFields.Fields.Item("U_tipoContingencia").Value = myComboBox_tipoCont.Selected.Value;
                    oUserTable.UserFields.Fields.Item("U_descContingencia").Value = myComboBox_tipoCont.Selected.Description;
                    if(myEditText_Motivo.Value != null && myEditText_Motivo.Value != "")
                    oUserTable.UserFields.Fields.Item("U_motivo").Value = myEditText_Motivo.Value;
                    oUserTable.UserFields.Fields.Item("U_fechaIni").Value = fechaIni.ToString();
                    oUserTable.UserFields.Fields.Item("U_horaIni").Value = myComboBox_HoraIni.Selected.Value;
                    oUserTable.UserFields.Fields.Item("U_fechaFin").Value = fechaFin.ToString();
                    oUserTable.UserFields.Fields.Item("U_horaFin").Value = myComboBox_HoraFin.Selected.Value;
                    if (oUserTable.Add() != 0)
                    {
                        mySBO_Application.StatusBar.SetText("Error al guardar informacion de Contingencia: " + mySBO_Company.GetLastErrorCode() + " - " + mySBO_Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    cargarContingencias();
                }
            }
            catch (Exception ex)
            {
                mySBO_Application.StatusBar.SetText("Error guardaContingencia: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                mySBO_Company.Disconnect();
            }
        }

        private void WriteToFile(string Message)
        {
            string path = "C:\\Logs_FEL";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filepath = "C:\\Logs_FEL\\AddOn_FEL_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".log";
            using (StreamWriter sw = File.AppendText(filepath))
            {
                sw.WriteLine("INFO: " + DateTime.Now.ToString() + ": " +Message);
            }
        }
    }
}