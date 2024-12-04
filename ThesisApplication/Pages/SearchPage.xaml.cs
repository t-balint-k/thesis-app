using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ThesisApplication.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchPage : ContentPage
    {
        /* Initialize */

        public ICommand ClickFilter { get; }
        public ICommand ClickItem { get; set; }
        private DateTime lastKeyStoke { get; set; }
        private Portfolio portfolio { get; set; }
        private Tranzaction[] tranzactions { get; set; }
        public bool updatePending { get; set; }

        public SearchPage(Portfolio p, Tranzaction[] t)
        {
            InitializeComponent();

            portfolio = p;
            tranzactions = t;

            // Binding
            ClickFilter = new Command(GoFiltersPage);
            ClickItem = new Command(GoItemPage);
            BindingContext = this;

            //
            SearchBar.TextChanged += userTyping;
            DoSearch();
        }

        /* On appearing && after filters set */

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (updatePending)
            {
                updatePending = false;
                DoSearch();
            }
        }

        /* User typing */

        private async void userTyping(object sender, TextChangedEventArgs e)
        {
            // Register keystroke then wait
            lastKeyStoke = DateTime.Now;
            await Task.Run(() => waiting(DateTime.Now));

            void waiting(DateTime start)
            {
                // Delay then check
                Task.Delay(1000).Wait();
                if (start < lastKeyStoke) return;

                // Search
                lastKeyStoke = DateTime.Now;
                Device.BeginInvokeOnMainThread(() => { DoSearch(); });
            }
        }

        /* Searching the instruments in the local cache */

        void DoSearch()
        {
            ResultList.Children.Clear();

            // Instrument types
            string types = string.Join(",", new string[]
            {
                (SearchFilterPreferences.stock     ? "stocks" : ""),
                (SearchFilterPreferences.forex     ? "forex_pairs" : ""),
                (SearchFilterPreferences.crypto    ? "cryptocurrencies" : ""),
                (SearchFilterPreferences.fund      ? "funds" : ""),
                (SearchFilterPreferences.bond      ? "bonds" : ""),
                (SearchFilterPreferences.etf       ? "etfs" : ""),
                (SearchFilterPreferences.index     ? "indices" : ""),
                (SearchFilterPreferences.commodity ? "commodities" : "")
            });

            // Query local cache
            string userInput = (SearchBar.Text ?? "").Replace(" ", "").ToLower();
            Instrument[] instruments = DbHelper.Context.Table<Instrument>()
                .Where(x => 
                    // other filters
                    x.country.Contains(SearchFilterPreferences.country) && 
                    x.exchange.Contains(SearchFilterPreferences.exchange) && 
                    x.currency.Contains(SearchFilterPreferences.currency) &&
                    types.Contains(x.instrument_type) &&
                    x.search_string.Contains(userInput))
                .Take(50)
                .ToArray();

            // Print
            foreach (Instrument n in instruments)
            {
                string upperLeftText = string.IsNullOrEmpty(n.name) ? n.symbol : n.name;
                string upperRightText = Tools.Translate(n.instrument_type);
                string lowerLeftText = $"{n.exchange} ({n.country})  -  {n.currency}";
                if (upperLeftText.Length > 30) upperLeftText = upperLeftText.Substring(0, 27) + "...";

                Tuple<Portfolio, Instrument> tuple = Tuple.Create(portfolio, n);
                GestureRecognizer click = new TapGestureRecognizer() { Command = ClickItem, CommandParameter = tuple };

                Frame f = Tools.PrettyFrame(upperLeftText, upperRightText, lowerLeftText, "", Color.Black, Color.DimGray, click);
                ResultList.Children.Add(f);
            }

            // Notice
            if (instruments.Length == 50) ResultList.Children.Add(new Label() { Text = "Figyelem! Csak ez első 50 találat jelenik meg.", TextColor = Color.Red });
        }

        /* Go to filters page */

        async void GoFiltersPage()
        {
            await Navigation.PushAsync(new SearchFilterPage(this));
        }

        /* Clock search result */

        async void GoItemPage(object parameter)
        {
            (Portfolio p, Instrument i) = parameter as Tuple<Portfolio, Instrument>;
            Tranzaction[] t = tranzactions.Where(x => x.instrument_fk == i.id).ToArray();
            await Navigation.PushAsync(new InstrumentViewPage(p, i ,t));
        }
    }
}