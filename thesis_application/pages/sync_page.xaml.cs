using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace thesis_application
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class sync_page : ContentPage
    {
        public ICommand sync_data { get; }
        public ICommand refresh { get; }

        public sync_page()
        {
            InitializeComponent();

            sync_data = new Command(sync);
            refresh = new Command(do_refresh);

            BindingContext = this;

            do_refresh();
        }

        // snyc local cache in the background
        void sync(object parameter)
        {
            Enum.TryParse(parameter.ToString(), out security_type result);
            Task.Run(() => do_data_sync(result));
        }

        // refresh the page
        void do_refresh()
        {
            main_container.Children.Clear();

            foreach (security_type n in Enum.GetValues(typeof(security_type)))
            {
                // get or create most recent sync log
                sync_log security_context = utility.db_context.Table<sync_log>().Where(x => x.type == n).OrderByDescending(x => x.last_sync).FirstOrDefault();

                if (security_context == null)
                {
                    security_context = new sync_log() { type = n, last_sync = DateTime.Now, row_count = 0, status = "üres tábla" };
                    utility.db_context.Insert(security_context);
                }

                // refresh button
                Button b = new Button() { Text = "frissítés", Command = sync_data, CommandParameter = n.ToString(), IsEnabled = security_context.status != "frissítés folyamatban" };
                b.Clicked += b_click;

                // type header
                Label l1 = new Label() { Text = n.ToString().ToUpper(), TextColor = Color.Black, FontAttributes = FontAttributes.Bold };

                // last synv time
                Label l2 = new Label() { Text = $"Legutóbb frissítve: {security_context.last_sync}" };
                
                // status indicator
                Label l3 = new Label() { Text = $"Státusz: {security_context.status}" };
                l3.TextColor = security_context.status == "naprakész" ? Color.Green : (security_context.status == "frissítés folyamatban" ? Color.Blue : Color.Red);

                // row count
                Label l4 = new Label() { Text = $"Rekordok: {security_context.row_count}" };

                // done
                main_container.Children.Add(new StackLayout() { Children = { l1,l2,l3,l4,b } } );
            }

            refresh_container.IsRefreshing = false;
        }

        // incativate the buttons on click event
        private void b_click(object sender, EventArgs e)
        {
            ((Button)sender).IsEnabled = false;
        }

        // DATA SYNC
        void do_data_sync(security_type type)
        {
            HttpClient http = new HttpClient();

            // current log record
            sync_log current = new sync_log() { type = type, last_sync = DateTime.Now, row_count = 0, status = "frissítés folyamatban" };
            utility.db_context.Insert(current);

            // updating the local cache
            switch (type)
            {
                case security_type.stock: get_data<stock>("stocks"); break;
                case security_type.forex: get_data<forex>("forex_pairs"); break;
                case security_type.crypto: get_data<crypto>("cryptocurrencies"); break;
                case security_type.fund: get_data<fund>("funds"); break;
                case security_type.bond: get_data<bond>("bonds"); break;
                case security_type.etf: get_data<etf>("etfs"); break;
                case security_type.index: get_data<index>("indices"); break;
                case security_type.commodity: get_data<commodity>("commodities"); break;
            }

            async void get_data<T>(string endpoint)
            {
                // truncate local db
                utility.db_context.DropTable<T>();
                utility.db_context.CreateTable<T>();

                // get and parse API repsone
                string response = await http.GetStringAsync($"https://api.twelvedata.com/{endpoint}");
                List<T> elements = JsonConvert.DeserializeObject<packet<T>>(response).data;

                // precalculating search_string for the elements
                foreach (security n in elements) n.construct_search_string();

                // done
                current.row_count = utility.db_context.InsertAll(elements);
                current.status = "naprakész";
                current.last_sync = DateTime.Now;
                utility.db_context.Update(current);
            }
        }

        class packet<T> { public List<T> data { get; set; } }
    }
}