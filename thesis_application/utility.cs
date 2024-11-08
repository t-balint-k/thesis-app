using SQLite;
using System;
using System.IO;
using Xamarin.Essentials;

namespace thesis_application
{
    public static class utility
    {
        // API key
        private const string api_key = "0dd18a256cb649e48b71f593c5dd963f";

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

        // api call constructor for real time price data
        public static string call_price_api(security s)
        {
            return $"https://api.twelvedata.com/price?{s.construct_api_arguments()}&apikey={api_key}";
        }

        // api call constructor for time series
        public static string call_time_api(security s, string interval, int size)
        {
            return $"https://api.twelvedata.com/time_series?{s.construct_api_arguments()}&interval={interval}&outputsize={size}&apikey={api_key}";
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
    }
}
