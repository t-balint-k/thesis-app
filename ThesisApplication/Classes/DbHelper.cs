using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace ThesisApplication
{
    public class DbHelper
    {
        /* Database connection */

        public static SQLiteConnection Context;

        /* Database init process */

        public static async Task<string> DatabaseInit()
        {
            // context
            try
            {
                string path = Path.Combine(FileSystem.AppDataDirectory, "ThesisAppLocalCache.db");
                Context = new SQLiteConnection(path);
            }
            catch (Exception ex) { return ex.Message; }

            // datasets
            if (!EnvironmentVariable.debugMode)
            {
                EndpointResponse instrumentResponse = await LocalCache<Instrument>();
                EndpointResponse exchangeResponse = await LocalCache<Exchange>();
                EndpointResponse countryResponse = await LocalCache<Country>();
                EndpointResponse currencyResponse = await LocalCache<Currency>();

                if (instrumentResponse.success) return instrumentResponse.message;
                if (exchangeResponse.success) return exchangeResponse.message;
                if (countryResponse.success) return countryResponse.message;
                if (currencyResponse.success) return currencyResponse.message;
            }

            // done
            return "";
        }

        /* Local cache */

        private static async Task<EndpointResponse> LocalCache<T>()
        {
            try { Context.DropTable<T>(); } catch { }
            Context.CreateTable<T>();

            // currency
            if (typeof(T) == typeof(Currency))
            {
                Currency[] currencies = Context.Table<Country>().Select(x => x.currency).Distinct().Select(x => new Currency() { symbol = x }).ToArray();
                Context.InsertAll(currencies);
                return new EndpointResponse(true, "");
            }

            // download
            EndpointResponse response = await NetHelper.SendRequest("GetData", RequestVariable.FromSingle("datatype", typeof(T).Name));

            // download error
            if (!response.success) { return new EndpointResponse(false, response.message); }

            // download successful
            try
            {
                List<T> insert = JsonConvert.DeserializeObject<DataPacket<T>>(response.message).data;
                if (typeof(T) == typeof(Instrument)) foreach (Instrument n in insert.Cast<Instrument>()) n.consolidateInstrument();
                Context.InsertAll(insert);
            }
            catch (Exception ex)
            {
                return new EndpointResponse(false, ex.Message);
            }

            // ok
            return new EndpointResponse(true, "");
        }

        /* Data packet */

        private class DataPacket<T>
        {
            public List<T> data;
        }
    }
}