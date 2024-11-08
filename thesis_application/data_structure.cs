using SQLite;
using System;

namespace thesis_application
{
    /* SECURITY ABSTACTION */

    public interface security
    {
        int id { get; set; }
        string symbol { get; set; }
        string search_string { get; set; }
        void construct_search_string();
        string construct_api_arguments();
        (string, string) construct_identifiers();
    }

    public enum security_type
    {
        stock,
        forex,
        crypto,
        fund,
        bond,
        etf,
        index,
        commodity
    }

    /* DATA TYPES */

    [Table("stock")]
    public class stock : security
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string symbol { get; set; }
        public string name { get; set; }
        public string exchange { get; set; }
        public string currency { get; set; }
        public string country { get; set; }
        public string type { get; set; }
        [Indexed]
        public string search_string { get; set; }
        // avoiding switches
        public void construct_search_string() { search_string = $"{symbol}{name}".ToLower(); }
        public string construct_api_arguments() { return $"symbol={symbol}&exchange={exchange}&country={country.Replace(" ", "%20")}"; }
        public (string, string) construct_identifiers() { return (name, $"{exchange} ({country})"); }
    }

    [Table("forex")]
    public class forex : security
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string symbol { get; set; }
        public string currency_base { get; set; }
        public string currency_quote { get; set; }
        [Indexed]
        public string search_string { get; set; }
        // avoiding switches
        public void construct_search_string() { search_string = $"{symbol}{currency_base}{currency_quote}".ToLower(); }
        public string construct_api_arguments() { return $"symbol={symbol}"; }
        public (string, string) construct_identifiers() { return (symbol, $"{currency_base} / {currency_quote}"); }
    }

    [Table("crypro")]
    public class crypto : security
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string symbol { get; set; }
        public string exchange { get; set; }
        public string currency_base { get; set; }
        public string currency_quote { get; set; }
        [Indexed]
        public string search_string { get; set; }
        // avoiding switches
        public void construct_search_string() { search_string = $"{symbol}{currency_base}{currency_quote}".ToLower(); }
        public string construct_api_arguments() { return $"symbol={symbol}"; }
        public (string, string) construct_identifiers() { return (symbol, $"{exchange} ({currency_base} / {currency_quote})"); }
    }

    [Table("fund")]
    public class fund : security
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string symbol { get; set; }
        public string exchange { get; set; }
        public string country { get; set; }
        public string type { get; set; }
        [Indexed]
        public string search_string { get; set; }
        // avoiding switches
        public void construct_search_string() { search_string = $"{symbol}{type}".ToLower(); }
        public string construct_api_arguments() { return $"symbol={symbol}&exchange={exchange}&country={country}&type={type}"; }
        public (string, string) construct_identifiers() { return (symbol, $"{exchange} ({country})"); }
    }

    [Table("bond")]
    public class bond :security
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string symbol { get; set; }
        public string exchange { get; set; }
        public string country { get; set; }
        public string type { get; set; }
        [Indexed]
        public string search_string { get; set; }
        // avoiding switches
        public void construct_search_string() { search_string = $"{symbol}{type}".ToLower(); }
        public string construct_api_arguments() { return $"symbol={symbol}&exchange={exchange}&country={country}&type={type}"; }
        public (string, string) construct_identifiers() { return (symbol, $"{exchange} ({country})"); }
    }

    [Table("etf")]
    public class etf : security
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string symbol { get; set; }
        public string exchange { get; set; }
        public string country { get; set; }
        [Indexed]
        public string search_string { get; set; }
        public void construct_search_string() { search_string = $"{symbol}".ToLower(); }
        public string construct_api_arguments() { return $"symbol={symbol}&exchange={exchange}&country={country}"; }
        public (string, string) construct_identifiers() { return (symbol, $"{exchange} ({country})"); }
    }

    [Table("index")]
    public class index : security
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string symbol { get; set; }
        public string exchange { get; set; }
        public string country { get; set; }
        [Indexed]
        public string search_string { get; set; }
        // avoiding switches
        public void construct_search_string() { search_string = $"{symbol}".ToLower(); }
        public string construct_api_arguments() { return $"symbol={symbol}&exchange={exchange}&country={country}"; }
        public (string, string) construct_identifiers() { return (symbol, $"{exchange} ({country})"); }
    }

    [Table("commodity")]
    public class commodity : security
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string symbol { get; set; }
        public string category { get; set; }
        [Indexed]
        public string search_string { get; set; }
        // avoiding switches
        public void construct_search_string() { search_string = $"{symbol}{category}".ToLower(); }
        public string construct_api_arguments() { return $"symbol={symbol}&category={category}"; }
        public (string, string) construct_identifiers() { return (category, $"{symbol}"); }
    }

    /* DATA SYNC METADATA */

    [Table("sync_log")]
    public class sync_log
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public security_type type { get; set; }
        public DateTime last_sync { get; set; }
        public int row_count { get; set; }
        public string status { get; set; }
    }

    /* TRANZACTION HISTORY */

    public class tranzaction
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public int security_fk { get; set; }
        public security_type type { get; set;}
        public int amount { get; set; }
        public double price { get; set; }
        public DateTime timestamp { get; set; }
    }
}