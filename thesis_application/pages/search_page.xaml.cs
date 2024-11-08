using System.Threading.Tasks;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System;

namespace thesis_application
{
    // filter options
    public static class filter_options
    {
        public static bool stock;
        public static bool forex;
        public static bool crypto;
        public static bool fund;
        public static bool bond;
        public static bool etf;
        public static bool index;
        public static bool commodity;

        public static string[] coutries;
        public static string country;
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class search_page : ContentPage
    {
        // command interfaces
        public ICommand click_item { get; }
        public ICommand click_filter { get; }

        //
        int press;

        public search_page()
        {
            InitializeComponent();

            // textbox update
            search_bar.TextChanged += typing;

            // default filter options
            filter_options.stock = true;
            filter_options.forex = true;
            filter_options.crypto = true;
            filter_options.fund = false;
            filter_options.bond = false;
            filter_options.etf = false;
            filter_options.index = false;
            filter_options.commodity = false;
            filter_options.country = "United States";

            // click events
            click_item = new Command(go_item);
            click_filter = new Command(go_filter);

            // binding
            BindingContext = this;

            //
            press = 0;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (search_bar.Text == null) return;
            list_results();
        }

        async void go_filter()
        {
            // navigate to filter page
            await Navigation.PushAsync(new filter_page());
        }

        async void go_item(object parammeter)
        {
            // navigate to item page
            search_result result = (search_result)parammeter;

            await Navigation.PushAsync(new item_page(result.type, result.data));
        }

        private void typing(object sender, TextChangedEventArgs e)
        {
            press++;
            
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                press--;

                // listing result
                if (press == 0) list_results();

                return false;
            });
        }

        void list_results()
        {
            result_list.Children.Clear();
            if (search_bar.Text.Length == 0) return;

            string user_input = search_bar.Text.ToLower();
            List<search_result> results = new List<search_result>();

            string country_filter = filter_options.country == "Mindegyik" ? "" : filter_options.country;

            // local database lookup
            /* AVAILABLE UNDER FREE TIER: US stocks, forex, crypto */
            if (filter_options.stock)     results.AddRange(utility.db_context.Table<stock>()    .Where(x => x.search_string.Contains(user_input) && x.country.Contains(country_filter)).Select(x => new search_result() { id = x.id, type = security_type.stock,     data = x }));
            if (filter_options.forex)     results.AddRange(utility.db_context.Table<forex>()    .Where(x => x.search_string.Contains(user_input)                                      ).Select(x => new search_result() { id = x.id, type = security_type.forex,     data = x }));
            if (filter_options.crypto)    results.AddRange(utility.db_context.Table<crypto>()   .Where(x => x.search_string.Contains(user_input)                                      ).Select(x => new search_result() { id = x.id, type = security_type.crypto,    data = x }));
            if (filter_options.fund)      results.AddRange(utility.db_context.Table<fund>()     .Where(x => x.search_string.Contains(user_input) && x.country.Contains(country_filter)).Select(x => new search_result() { id = x.id, type = security_type.fund,      data = x }));
            if (filter_options.bond)      results.AddRange(utility.db_context.Table<bond>()     .Where(x => x.search_string.Contains(user_input) && x.country.Contains(country_filter)).Select(x => new search_result() { id = x.id, type = security_type.bond,      data = x }));
            if (filter_options.etf)       results.AddRange(utility.db_context.Table<etf>()      .Where(x => x.search_string.Contains(user_input) && x.country.Contains(country_filter)).Select(x => new search_result() { id = x.id, type = security_type.etf,       data = x }));
            if (filter_options.index)     results.AddRange(utility.db_context.Table<index>()    .Where(x => x.search_string.Contains(user_input) && x.country.Contains(country_filter)).Select(x => new search_result() { id = x.id, type = security_type.index,     data = x }));
            if (filter_options.commodity) results.AddRange(utility.db_context.Table<commodity>().Where(x => x.search_string.Contains(user_input)                                      ).Select(x => new search_result() { id = x.id, type = security_type.commodity, data = x }));

            // printing results
            result_list.Children.Add(new Label() { Text = $"Találatok: {results.Count()}" });
            foreach (search_result n in results.Take(100))
            {
                StackLayout new_item = new StackLayout()
                {
                    Padding = new Thickness(3),
                    Margin = new Thickness(0, 0, 0, 6),
                    GestureRecognizers = { new TapGestureRecognizer() { Command = click_item, CommandParameter = n } }
                };

                foreach (Label m in get_children(n.type, n.data)) new_item.Children.Add(m);

                result_list.Children.Add(new_item);
            }

            if (results.Count() > 200) result_list.Children.Add(new Label() { Text = "Figyelem! Csupán az első 200 találat került megjelenítésre. Szűkítse a keresést!", TextColor = Color.Red });
        }

        class search_result
        {
            public int id;
            public security_type type;
            public object data;
        }

        Label[] get_children(security_type type, object data)
        {
            switch (type)
            {
                case security_type.stock:
                    stock subject1 = (stock)data;
                    return new Label[]
                    {
                        new Label() { Text = subject1.name, TextColor = Color.Black, FontSize = 16, Padding = 0, Margin = 0 },
                        new Label() { Text = subject1.symbol, TextColor = Color.Gray, FontSize = 16, Padding = 0, Margin = 0 },
                        new Label() { Text = $"{subject1.exchange} ({subject1.country})", TextColor = Color.Gray, FontSize = 11, Padding = 0, Margin = 0 }
                    };
                case security_type.forex:
                    forex subject2 = (forex)data;
                    return new Label[]
                    {
                        new Label() { Text = subject2.symbol, TextColor = Color.Black, FontSize = 16, Padding = 0, Margin = 0 },
                        new Label() { Text = $"{subject2.currency_base} / {subject2.currency_quote}", TextColor = Color.Gray, FontSize = 16, Padding = 0, Margin = 0 }
                    };
                case security_type.crypto:
                    crypto subject3 = (crypto)data;
                    return new Label[]
                    {
                        new Label() { Text = subject3.symbol, TextColor = Color.Black, FontSize = 16, Padding = 0, Margin = 0 },
                        new Label() { Text = $"{subject3.currency_base} / {subject3.currency_quote}", TextColor = Color.Gray, FontSize = 16, Padding = 0, Margin = 0 }
                    };
                case security_type.fund:
                    fund subject4 = (fund)data;
                    return new Label[]
                    {
                        new Label() { Text = subject4.symbol, TextColor = Color.Black, FontSize = 16, Padding = 0, Margin = 0 },
                        new Label() { Text = subject4.type, TextColor = Color.Gray, FontSize = 16, Padding = 0, Margin = 0 },
                        new Label() { Text = $"{subject4.exchange} ({subject4.country})", TextColor = Color.Gray, FontSize = 11, Padding = 0, Margin = 0 }
                    };
                case security_type.bond:
                    bond subject5 = (bond)data;
                    return new Label[]
                    {
                        new Label() { Text = subject5.symbol, TextColor = Color.Black, FontSize = 16, Padding = 0, Margin = 0 },
                        new Label() { Text = subject5.type, TextColor = Color.Gray, FontSize = 16, Padding = 0, Margin = 0 },
                        new Label() { Text = $"{subject5.exchange} ({subject5.country})", TextColor = Color.Gray, FontSize = 11, Padding = 0, Margin = 0 }
                    };
                case security_type.etf:
                    etf subject6 = (etf)data;
                    return new Label[]
                    {
                        new Label() { Text = subject6.symbol, TextColor = Color.Black, FontSize = 16, Padding = 0, Margin = 0 },
                        new Label() { Text = $"{subject6.exchange} ({subject6.country})", TextColor = Color.Gray, FontSize = 11, Padding = 0, Margin = 0 }
                    };
                case security_type.index:
                    index subject7 = (index)data;
                    return new Label[]
                    {
                        new Label() { Text = subject7.symbol, TextColor = Color.Black, FontSize = 16, Padding = 0, Margin = 0 },
                        new Label() { Text = $"{subject7.exchange} ({subject7.country})", TextColor = Color.Gray, FontSize = 11, Padding = 0, Margin = 0 }
                    };
                case security_type.commodity:
                    commodity subject8 = (commodity)data;
                    return new Label[]
                    {
                        new Label() { Text = subject8.symbol, TextColor = Color.Black, FontSize = 16, Padding = 0, Margin = 0 },
                        new Label() { Text = subject8.category, TextColor = Color.Gray, FontSize = 11, Padding = 0, Margin = 0 }
                    };

                // mandatory dummy return branch
                default: return new Label[0];
            }
        }
    }
}