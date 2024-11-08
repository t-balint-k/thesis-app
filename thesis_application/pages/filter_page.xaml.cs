using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace thesis_application
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class filter_page : ContentPage
    {
        public filter_page()
        {
            InitializeComponent();

            // countries
            if (filter_options.coutries == null)
            {
                List<string> list = new List<string>() { "Mindegyik" };
                list.AddRange(utility.db_context.Table<stock>().Select(x => x.country).Where(x => x != "").OrderBy(x => x).Distinct());
                filter_options.coutries = list.ToArray();
            }

            foreach (string n in filter_options.coutries) country_picker.Items.Add(n);
            country_picker.SelectedItem = filter_options.country;
            country_picker.SelectedIndexChanged += country_picked;

            // defaults
            stock_switch.IsToggled = filter_options.stock;
            forex_switch.IsToggled = filter_options.forex;
            crypto_switch.IsToggled = filter_options.crypto;
            bond_switch.IsToggled = filter_options.bond;
            fund_switch.IsToggled = filter_options.fund;
            etf_switch.IsToggled = filter_options.etf;
            index_switch.IsToggled = filter_options.index;
            commodity_switch.IsToggled = filter_options.commodity;

            // event handlers
            stock_switch.Toggled += Stock_switch_Toggled;
            forex_switch.Toggled += Forex_switch_Toggled;
            crypto_switch.Toggled += Crypto_switch_Toggled;
            bond_switch.Toggled += Bond_switch_Toggled;
            fund_switch.Toggled += Fund_switch_Toggled;
            etf_switch.Toggled += Etf_switch_Toggled;
            index_switch.Toggled += Index_switch_Toggled;
            commodity_switch.Toggled += Commodity_switch_Toggled;
        }

        private void country_picked(object sender, System.EventArgs e)
        {
            filter_options.country = (string)country_picker.SelectedItem;
        }

        /* SWITCH EVENT HANDLERS */

        private void Commodity_switch_Toggled(object sender, ToggledEventArgs e)
        {
            filter_options.commodity = commodity_switch.IsToggled;
        }

        private void Index_switch_Toggled(object sender, ToggledEventArgs e)
        {
            filter_options.index = index_switch.IsToggled;
        }

        private void Etf_switch_Toggled(object sender, ToggledEventArgs e)
        {
            filter_options.etf = etf_switch.IsToggled;
        }

        private void Fund_switch_Toggled(object sender, ToggledEventArgs e)
        {
            filter_options.fund = fund_switch.IsToggled;
        }

        private void Bond_switch_Toggled(object sender, ToggledEventArgs e)
        {
            filter_options.bond = bond_switch.IsToggled;
        }

        private void Crypto_switch_Toggled(object sender, ToggledEventArgs e)
        {
            filter_options.crypto = crypto_switch.IsToggled;
        }

        private void Forex_switch_Toggled(object sender, ToggledEventArgs e)
        {
            filter_options.forex = forex_switch.IsToggled;
        }

        private void Stock_switch_Toggled(object sender, ToggledEventArgs e)
        {
            filter_options.stock = stock_switch.IsToggled;
        }
    }
}