using SQLite;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Essentials;
using System.Net;
using Plugin.Connectivity;

namespace thesis_application
{
    public static class utility
    {
        // database connection
        public static SQLiteConnection db_context;

        // database init process
        public static void database_init()
        {
            string path = Path.Combine(FileSystem.AppDataDirectory, "thesis_app_local.db");
            db_context = new SQLiteConnection(path);

            foreach (sync_log n in db_context.Table<sync_log>().Where(x => x.status == "frissítés folyamatban"))
            {
                n.status = "hiba történt";
                db_context.Update(n);
            }
        }

        // security types hungarian translation
        public static string translate(security_type t, bool capitalize = false)
        {
            string result = "";

            switch (t)
            {
                case security_type.stock:     result = "részvény"; break;
                case security_type.forex:     result = "deviza pár"; break;
                case security_type.crypto:    result = "kriptovaluta"; break;
                case security_type.fund:      result = "alap"; break;
                case security_type.bond:      result = "kötvény"; break;
                case security_type.etf:       result = "tőzsdei alap"; break;
                case security_type.index:     result = "index"; break;
                case security_type.commodity: result = "árupiaci termék"; break;
            }

            if (capitalize) result = result.Substring(0, 1).ToUpper() + result.Substring(1);

            return result;
        }

        // pritifyers
        public static string prittify(double d)
        {
            return d.ToString("#,0.00").Replace(",", " ").Replace(".", ",");
        }

        public static string prittify(DateTime d)
        {
            return d.ToString("yyyy-MM-dd HH:mm:ss");
        }

        // get security based on type
        public static security get_security(security_type t, int fk)
        {
            switch (t)
            {
                case security_type.stock:     return db_context.Table<stock>().Where(x => x.id == fk).FirstOrDefault();
                case security_type.forex:     return db_context.Table<forex>().Where(x => x.id == fk).FirstOrDefault();
                case security_type.crypto:    return db_context.Table<crypto>().Where(x => x.id == fk).FirstOrDefault();
                case security_type.fund:      return db_context.Table<fund>().Where(x => x.id == fk).FirstOrDefault();
                case security_type.bond:      return db_context.Table<bond>().Where(x => x.id == fk).FirstOrDefault();
                case security_type.etf:       return db_context.Table<etf>().Where(x => x.id == fk).FirstOrDefault();
                case security_type.index:     return db_context.Table<index>().Where(x => x.id == fk).FirstOrDefault();
                case security_type.commodity: return db_context.Table<commodity>().Where(x => x.id == fk).FirstOrDefault();
            }
            return new stock();
        }

        // reching the webserver
        public static async Task<(bool, string)> reach_server(string endpoint, string email, string password)
        {
            // email
            email = HttpUtility.UrlEncode(email);
            
            // password
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte n in bytes) builder.Append(n.ToString("x2"));
                password = builder.ToString();
            }

            // call
            (bool success, string response) = await send_request($"http://172.213.140.243:5000/v1/{endpoint}?email={email}&password={password}");

            // done
            return (success, response);
        }

        // sending an asyncronous call
        public static async Task<(bool, string)> send_request(string fullurl)
        {
            // check internet connectivity
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                return (false, "Nincs internet kapcsolat!");
            }

            // check server reachability
            /*
            var available = await CrossConnectivity.Current.IsRemoteReachable(baseurl, 5000, 8000);
            if (!available)
            {
                return (false, "A szerver jelenleg nem érhető el!");
            }
            */

            // call
            HttpStatusCode status;
            string response;

            try
            {
                HttpClient http = new HttpClient() { Timeout = new TimeSpan(0, 0, 8) };
                HttpResponseMessage msg = await http.GetAsync(fullurl);

                status = msg.StatusCode;
                response = await msg.Content.ReadAsStringAsync();
            }

            // generic error
            catch
            {
                return (false, "Ismeretlen hiba történt!");
            }

            // unsuccessful
            switch (status)
            {
                // timeout
                case HttpStatusCode.RequestTimeout:
                    return (false, "A szerver nem válaszolt.");

                // internal server error
                case HttpStatusCode.InternalServerError:
                    return (false, "Belső szerver hiba!");

                // bad request
                case HttpStatusCode.BadRequest:
                    return (false, "A mezők kitöltése kötelező!");

                // not found
                case HttpStatusCode.NotFound:
                    return (false, "NOTFOUND");

                // OK
                case HttpStatusCode.OK:
                    return (true, response);

                // everything else -> unknown error
                default:
                    return (false, $"Ismeretlen hiba történt: HTTP {status}");
            }
        }
    }
}
