using SQLite;

namespace ThesisApplication
{
    public class Portfolio
    {
        [PrimaryKey]
        public int id { get; set; }
        public string creation_time { get; set; }
        public string name { get; set; }
        public double pool { get; set; }
        public string currency { get; set; }
    }
}