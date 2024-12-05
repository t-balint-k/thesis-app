using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ThesisApplication.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchFilterPage : ContentPage
    {
        /* Initialize */

        public SearchPage parentPage { get; set; }
        bool pause;

        public SearchFilterPage(SearchPage parent)
        {
            InitializeComponent();
            parentPage = parent;

            // Countries
            string[] countries = DbHelper.Context.Table<Country>().Select(x => x.name).Distinct().ToArray();
            countryPicker .Items.Add("Mindegyik");
            foreach (string n in countries) countryPicker.Items.Add(n);
            countryPicker.SelectedIndexChanged += countryChanged;
            countryPicker.SelectedItem = SearchFilterPreferences.country == "" ? "Mindegyik" : SearchFilterPreferences.country;

            // Exchanges
            exchangePicker.SelectedIndexChanged += exchangeChanged;
            exchangePicker.SelectedItem = SearchFilterPreferences.exchange == "" ? "Mindegyik" : SearchFilterPreferences.exchange;

            // Currency
            string[] currencies = DbHelper.Context.Table<Currency>().Select(x => x.symbol).Distinct().ToArray();
            currencyPicker.Items.Add("Mindegyik");
            foreach (string n in currencies) currencyPicker.Items.Add(n);
            currencyPicker.SelectedIndexChanged += currencyChanged;
            currencyPicker.SelectedItem = SearchFilterPreferences.currency == "" ? "Mindegyik" : SearchFilterPreferences.currency;

            // Instrument types
            stock_switch.IsToggled     = SearchFilterPreferences.stock;
            forex_switch.IsToggled     = SearchFilterPreferences.forex;
            crypto_switch.IsToggled    = SearchFilterPreferences.crypto;
            fund_switch.IsToggled      = SearchFilterPreferences.fund;
            bond_switch.IsToggled      = SearchFilterPreferences.bond;
            etf_switch.IsToggled       = SearchFilterPreferences.etf;
            index_switch.IsToggled     = SearchFilterPreferences.index;
            commodity_switch.IsToggled = SearchFilterPreferences.commodity;

            stock_switch.Toggled     += switchToggle;
            forex_switch.Toggled     += switchToggle;
            crypto_switch.Toggled    += switchToggle;
            fund_switch.Toggled      += switchToggle;
            bond_switch.Toggled      += switchToggle;
            etf_switch.Toggled       += switchToggle;
            index_switch.Toggled     += switchToggle;
            commodity_switch.Toggled += switchToggle;
        }

        /* Triggers */

        private void countryChanged(object sender, System.EventArgs e)
        {
            // Countries
            string countryName = (string)countryPicker.SelectedItem;
            SearchFilterPreferences.country = countryName == "Mindegyik" ? "" : countryName;
            parentPage.updatePending = true;

            // Exchanges
            pause = true;
            exchangePicker.Items.Clear();
            string[] exchanges = DbHelper.Context.Table<Exchange>().Where(x => x.country.Contains(SearchFilterPreferences.country)).Select(x => x.name).Distinct().ToArray();
            exchangePicker.Items.Add("Mindegyik");
            foreach (string n in exchanges) exchangePicker.Items.Add(n);

            pause = false;
            if (exchanges.Contains(SearchFilterPreferences.exchange)) exchangePicker.SelectedItem = SearchFilterPreferences.exchange;
            else exchangePicker.SelectedItem = "Mindegyik";
        }

        private void exchangeChanged(object sender, System.EventArgs e)
        {
            if (pause) return;

            // Exchanges
            string exchangeName = (string)exchangePicker.SelectedItem;
            SearchFilterPreferences.exchange = exchangeName == "Mindegyik" ? "" : exchangeName;
            parentPage.updatePending = true;
        }

        private void currencyChanged(object sender, System.EventArgs e)
        {
            // Currencies
            string currencyName = (string)currencyPicker.SelectedItem;
            SearchFilterPreferences.currency = currencyName == "Mindegyik" ? "" : currencyName;
            parentPage.updatePending = true;
        }

        private void switchToggle(object sender, ToggledEventArgs e)
        {
            // Insuranace types
            SearchFilterPreferences.stock     = stock_switch.IsToggled;
            SearchFilterPreferences.forex     = forex_switch.IsToggled;
            SearchFilterPreferences.crypto    = crypto_switch.IsToggled;
            SearchFilterPreferences.fund      = fund_switch.IsToggled;
            SearchFilterPreferences.bond      = bond_switch.IsToggled;
            SearchFilterPreferences.etf       = etf_switch.IsToggled;
            SearchFilterPreferences.index     = index_switch.IsToggled;
            SearchFilterPreferences.commodity = commodity_switch.IsToggled;
        }
    }
}