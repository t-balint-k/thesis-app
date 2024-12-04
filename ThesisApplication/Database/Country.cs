using SQLite;

namespace ThesisApplication
{
    public class Country
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }
        public string iso3 { get; set; }
        public string currency { get; set; }
    }
}