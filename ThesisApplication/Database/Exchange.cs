using SQLite;

namespace ThesisApplication
{
    public class Exchange
    {
        [PrimaryKey]
        public string id { get; set; }
        public string name { get; set; }
        public string country { get; set; }
    }
}