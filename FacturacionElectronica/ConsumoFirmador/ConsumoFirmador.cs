using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Web;
using RestSharp;
using Newtonsoft.Json;

namespace ConsumoAPI
{
    public class ConsumoFirmador
    {

        public ConsumoFirmador() 
        { 

        }

        public async Task<dynamic> postFirmador(String url, String json)
        {
            try
            {
                var client = new RestClient(url);
                var request = new RestRequest(url,Method.Post);
 
             //   request.AddHeader("content-Type","application/JSON");
                request.AddBody(json,ContentType.Json);

                RestResponse response = client.Execute(request);

                dynamic datos = JsonConvert.DeserializeObject(response.Content);
                return datos;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error Servicio rest Metodo postFirmador: " + ex.Message);
                return null;
            }
        }

        public async Task<dynamic> postAutenticador(String url, String user, String pwd)
        {
            try
            {
                var client = new RestClient(url);
                var request = new RestRequest(url, Method.Post);

                request.AddHeader("content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36");
                request.AddParameter("application/x-www-form-urlencoded", $"user={user}&pwd={pwd}", ParameterType.RequestBody);

                RestResponse response = client.Execute(request);

                dynamic datos = JsonConvert.DeserializeObject(response.Content);
                return datos;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Servicio rest Metodo postAutenticador: " + ex.Message);
                return null;
            }
        }

        public async Task<dynamic> postReceptor(String url, String token, String json)
        {
            try
            {
                var client = new RestClient(url);
                var request = new RestRequest(url, Method.Post);

                request.AddHeader("content-Type", "application/json");
                request.AddHeader("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36");
                request.AddHeader("Authorization", token);

                request.AddBody(json, ContentType.Json);

                RestResponse response = client.Execute(request);

                dynamic datos = JsonConvert.DeserializeObject(response.Content);
                return datos;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Servicio rest Metodo postReceptor: " + ex.Message);
                return null;
            }
        }

        public async Task<dynamic> postAnulacion(String url, String token, String json)
        {
            try
            {
                var client = new RestClient(url);
                var request = new RestRequest(url, Method.Post);

                request.AddHeader("content-Type", "application/json");
                request.AddHeader("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36");
                request.AddHeader("Authorization", token);

                request.AddBody(json, ContentType.Json);

                RestResponse response = client.Execute(request);

                dynamic datos = JsonConvert.DeserializeObject(response.Content);
                return datos;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Servicio rest Metodo postAnulacion: " + ex.Message);
                return null;
            }
        }

        public async Task<dynamic> postContingencia(String url, String token, String json)
        {
            try
            {
                var client = new RestClient(url);
                var request = new RestRequest(url, Method.Post);

                request.AddHeader("content-Type", "application/json");
                request.AddHeader("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36");
                request.AddHeader("Authorization", token);

                request.AddBody(json, ContentType.Json);

                RestResponse response = client.Execute(request);

                dynamic datos = JsonConvert.DeserializeObject(response.Content);
                return datos;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Servicio rest Metodo postContingencia: " + ex.Message);
                return null;
            }
        }

        public async Task<dynamic> postLote(String url, String token, String json)
        {
            try
            {
                var client = new RestClient(url);
                var request = new RestRequest(url, Method.Post);

                request.AddHeader("content-Type", "application/json");
                request.AddHeader("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36");
                request.AddHeader("Authorization", token);

                request.AddBody(json, ContentType.Json);

                RestResponse response = client.Execute(request);

                dynamic datos = JsonConvert.DeserializeObject(response.Content);
                return datos;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Servicio rest Metodo postLote: " + ex.Message);
                return null;
            }
        }

        public async Task<dynamic> getConsultaLote(string url, string token, string codigoLote)
        {
            try
            {
                var client = new RestClient((url + codigoLote));
                var request = new RestRequest((url + codigoLote), Method.Get);


               // request.AddHeader("Content-Type", "application/JSON");
                request.AddHeader("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36");
                request.AddHeader("Authorization", token);
               // request.AddBody("{}", ContentType.Json);

                Console.WriteLine(client.GetAsync(request).Result);

                RestResponse response = (await client.GetAsync(request));

                // handle error
                if (!response.IsSuccessful)
                {
                    Console.WriteLine($"ERROR: {response.ErrorException?.Message}");
                }

                Console.WriteLine("JSON RESPONSE: " + JsonConvert.DeserializeObject(response.Content));

                dynamic datos = JsonConvert.DeserializeObject(response.Content);
                return datos;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Servicio rest Metodo getConsultaLote: " + ex.Message);
                MessageBox.Show("Error Servicio rest Metodo getConsultaLote: " + ex.Message);
                return null;
            }
        }


        public dynamic GetConsultaLote(string url, string token, string codigoLote)
        {
            try
            {
                HttpWebRequest myWebRequest = (HttpWebRequest)WebRequest.Create(url+codigoLote);
                myWebRequest.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36";
                //myWebRequest.CookieContainer = myCookie;
                //myWebRequest.Credentials = CredentialCache.DefaultCredentials;
                myWebRequest.Headers.Add("Authorization",token);
                myWebRequest.Proxy = null;
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myWebRequest.GetResponse();
                Stream myStream = myHttpWebResponse.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myStream);
                //Leemos los datos
                string Datos = HttpUtility.HtmlDecode(myStreamReader.ReadToEnd());

                dynamic data = JsonConvert.DeserializeObject(Datos);

            return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Servicio rest Metodo getConsultaLote: " + ex.Message);
                MessageBox.Show("Error Servicio rest Metodo getConsultaLote: " + ex.Message);
                return null;
            }
        }
    }
}
