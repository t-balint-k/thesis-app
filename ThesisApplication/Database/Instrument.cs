using SQLite;
using System.Linq;

namespace ThesisApplication
{
    public class Instrument
    {
        // key
        [PrimaryKey]
        public int id { get; set; }

        // composite primary key
        [Indexed(Name = "SecuritiesUniqueIndex", Order = 1, Unique = true)]
        public string symbol { get; set; }
        [Indexed(Name = "SecuritiesUniqueIndex", Order = 2, Unique = true)]
        public string exchange { get; set; }
        [Indexed(Name = "SecuritiesUniqueIndex", Order = 3, Unique = true)]
        public string country { get; set; }
        [Indexed(Name = "SecuritiesUniqueIndex", Order = 4, Unique = true)]
        public string currency_base { get; set; }
        [Indexed(Name = "SecuritiesUniqueIndex", Order = 5, Unique = true)]
        public string currency_quote { get; set; }

        // other data
        public string name { get; set; }
        public string currency { get; set; }
        public string type { get; set; }
        public string valid_from { get; set; }
        public string valid_to { get; set; }
        public string instrument_type { get; set; }

        // search string
        public string search_string { get; set; }
        public void consolidateInstrument()
        {
            if (exchange == null) exchange = string.Empty;
            if (country == null) country = string.Empty;
            if (currency_base == null) currency_base = string.Empty;
            if (currency_quote == null) currency_quote = string.Empty;
            if (name == null) name = string.Empty;
            if (currency == null) currency = string.Empty;

            search_string = $"{symbol}{name}{currency_base}{currency_quote}".Replace(" ", "").ToLower();
        }

        // price argument
        public string getAPIArguments()
        {
            if (instrument_type == "forex_pairs" || instrument_type == "cryptocurrencies") return $"&symbol={symbol}";

            string countryCode = DbHelper.Context.Table<Country>().Where(x => x.name == country).Select(x => x.iso3).FirstOrDefault();
            return $"&symbol={symbol}&exchange={exchange}&country={countryCode}";
        }
    }
}