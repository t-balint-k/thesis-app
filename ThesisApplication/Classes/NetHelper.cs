using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System;
using Xamarin.Essentials;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ThesisApplication
{
    public class NetHelper
    {
        /* Sending an asyncronous call */

        // 1: backend
        public static async Task<EndpointResponse> SendRequest(string backendEndpoint, RequestVariable[] variables = null)
        {
            // check internet connectivity
            if (Connectivity.NetworkAccess != NetworkAccess.Internet) return new EndpointResponse(false, "Nincs internet kapcsolat!");

            // check heartbeat
            try
            {
                HttpClient http = new HttpClient() { Timeout = new TimeSpan(0, 0, 8) };
                string debug = $"{WebserverRequest}/heartbeat";
                HttpResponseMessage msg = await http.GetAsync(debug);

                if (msg.StatusCode != HttpStatusCode.OK) throw new Exception();
            }

            catch (Exception ex)
            {
                return new EndpointResponse(false, "A webserver jelenleg nem elérhető!");
            }

            // go
            return await SendRequest(WebserverRequest, backendEndpoint, variables);
        }

        // 2: third party
        public static async Task<EndpointResponse> SendRequest(string baseUrl, string endpoint, RequestVariable[] variables = null)
        {
            string fullUrl = $"{baseUrl}/{endpoint}";
            string requestUrl = variables == null ? fullUrl : $"{fullUrl}?{RequestVariable.Join(variables)}";

            return await SendRequest(requestUrl);
        }
        public static async Task<EndpointResponse> SendRequest(string requestUrl)
        {
            // check internet connectivity
            if (Connectivity.NetworkAccess != NetworkAccess.Internet) return new EndpointResponse(false, "Nincs internet kapcsolat!");

            // response
            string response;
            HttpStatusCode status;

            try
            {
                // request
                HttpClient http = new HttpClient() { Timeout = new TimeSpan(0, 0, 16) };
                HttpResponseMessage msg = await http.GetAsync(requestUrl);

                // response
                status = msg.StatusCode;
                response = await msg.Content.ReadAsStringAsync();
            }

            // generic error
            catch (Exception ex)
            {
                return new EndpointResponse(false, "Ismeretlen hiba történt!");
            }

            // unsuccessful
            switch (status)
            {
                // timeout
                case HttpStatusCode.RequestTimeout:
                    return new EndpointResponse(false, "A szerver nem válaszolt.");

                // internal server error
                case HttpStatusCode.InternalServerError:
                    return new EndpointResponse(false, "Belső szerver hiba!");

                // bad request
                case HttpStatusCode.BadRequest:
                    return new EndpointResponse(false, "A mezők kitöltése kötelező!");

                // not found
                case HttpStatusCode.NotFound:
                    return new EndpointResponse(false, "NOTFOUND");

                // already exists
                case HttpStatusCode.Conflict:
                    return new EndpointResponse(false, "Már létezik!");

                // OK
                case HttpStatusCode.OK:
                    return new EndpointResponse(true, response);

                // everything else -> unknown error
                default:
                    return new EndpointResponse(false, $"Ismeretlen hiba történt: HTTP {status}");
            }
        }

        /* Webserver request */
        public static string WebserverRequest
        {
            get
            {
                return $"{EnvironmentVariable.webProtocol}://{EnvironmentVariable.webIP}:{EnvironmentVariable.webPort}/{EnvironmentVariable.APIVersion}";
            }
        }

        /* Response caster */

        public static List<T> CastResponse<T>(string json)
        {
            return JObject.Parse(json)["data"].ToObject<List<T>>();
        }
    }
}