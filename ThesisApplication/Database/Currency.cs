using SQLite;

namespace ThesisApplication
{

    public class Currency
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string symbol { get; set; }
    }
}