using Microcharts;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace thesis_application
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class portfolio_page : ContentPage
    {
        // command interfaces
        public ICommand click_search { get; }
        public ICommand click_sync { get; }
        public ICommand click_item { get; }
        public ICommand refresh { get; }

        public portfolio_page()
        {
            InitializeComponent();

            // binding
            click_search = new Command(go_search);
            click_sync = new Command(go_sync);
            click_item = new Command(go_item);
            refresh = new Command(do_refresh);

            BindingContext = this;

            // init
            utility.database_init();
            do_refresh();
        }

        async void go_search()
        {
            // go to search page
            await Navigation.PushAsync(new search_page());
        }

        async void go_sync()
        {
            // go to data sync page
            await Navigation.PushAsync(new sync_page());
        }

        async void go_item(object parameter)
        {
            // go to item page
            element result = (element)parameter;
            await Navigation.PushAsync(new item_page(result.type, result.itself));
        }

        async void do_refresh()
        {
            // portfolio elements
            portfolio_list.Children.Clear();
            element[] elements = utility.db_context.Table<tranzaction>().GroupBy(x => new { x.type, x.security_fk }).Select(x => new element() { type = x.Key.type, fk = x.Key.security_fk, sum = x.Sum(y => y.amount), exp = x.Sum(y => y.amount * y.price), max = x.Max(y => y.timestamp) }).Where(x => x.sum > 0).ToArray();

            // loop
            foreach (element n in elements.OrderByDescending(x => x.max))
            {
                // the underlying security
                n.itself = utility.get_security(n.type, n.fk);
                (string name, string dispose) = n.itself.construct_identifiers();

                // the api call for the real time price
                try
                {
                    (bool success, string response) = await utility.send_request($"https://api.twelvedata.com/price?{n.itself.construct_api_arguments()}&apikey={App.api_key}");

                    // fail
                    if (!success)
                    {
                        has_error(response);
                        break;
                    }

                    // success
                    string actual_price = (string)JObject.Parse(response)["price"];
                    n.price = double.Parse(actual_price);
                }

                catch
                {
                    has_error("A szerver hibás adatokat küldött!");
                    break;
                }

                double d = Math.Truncate((((n.sum * n.price) / n.exp) - 1) * 10000) / 10000;

                Frame f = new Frame()
                {
                    HasShadow = true,
                    Padding = 6,
                    HeightRequest = 80,
                    HorizontalOptions = LayoutOptions.Fill,
                    GestureRecognizers = { new TapGestureRecognizer() { Command = click_item, CommandParameter = n } },
                    Content = new StackLayout()
                    {
                        Orientation = StackOrientation.Horizontal,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        Children =
                        {
                            new StackLayout()
                            {
                                Orientation = StackOrientation.Vertical,
                                HorizontalOptions = LayoutOptions.StartAndExpand,
                                Children =
                                {
                                    new Label()
                                    {
                                        Text = name,
                                        HorizontalOptions = LayoutOptions.Start,
                                        TextColor = Color.Gray,
                                        FontSize = 20
                                    },
                                    new Label()
                                    {
                                        Text = $"{n.sum} {utility.translate(n.type)}",
                                        HorizontalOptions = LayoutOptions.Start,
                                        TextColor = Color.Gray,
                                        FontAttributes = FontAttributes.Italic,
                                        FontSize = 12
                                    },
                                    new Label()
                                    {
                                        Text = $"${utility.prittify(n.price * n.sum)}",
                                        HorizontalOptions = LayoutOptions.Start,
                                        TextColor = Color.Gray,
                                        FontSize = 12
                                    }
                                }
                            },
                            new Label()
                            {
                                Text = d > 0 ? $"+{utility.prittify(d)}%" : $"{utility.prittify(d)}%",
                                HorizontalOptions = LayoutOptions.EndAndExpand,
                                VerticalOptions = LayoutOptions.CenterAndExpand,
                                TextColor = d < 0 ? Color.Red : Color.Green,
                                FontSize = 20
                            }
                        }
                    }
                };

                portfolio_list.Children.Add(f);
            }
            
            // done
            refresh_container.IsRefreshing = false;
        }

        void has_error(string reason)
        {
            refresh_container.IsRefreshing = false;
        }

        class element
        {
            public security_type type;
            public int fk;
            public int sum;
            public double exp;
            public DateTime max;
            //
            public string name;
            public double price;
            public security itself;
        }

        // chart? ....
    }
}