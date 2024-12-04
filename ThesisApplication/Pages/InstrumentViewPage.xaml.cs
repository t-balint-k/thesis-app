using Microcharts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ThesisApplication.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InstrumentViewPage : ContentPage
    {
        /* Initilaize */

        public ICommand Refresh { get; }
        public ICommand ClickBuy { get; }
        public ICommand ClickSell { get; }
        private Portfolio portfolio { get; set; }
        private Instrument instrument { get; set; }
        private Tranzaction[] tranzactions { get; set; }
        private List<Button> stepSizeButtons { get; set; }
        private double realtimePrice { get; set; }
        private double holdings { get; set; }

        public InstrumentViewPage(Portfolio p, Instrument i, Tranzaction[] t)
        {
            InitializeComponent();

            portfolio = p;
            instrument = i;
            tranzactions = t;

            Title = string.IsNullOrEmpty(i.name) ? i.symbol : i.name;

            // Binding
            Refresh = new Command(DoRefresh);
            ClickBuy = new Command(DoBuy);
            ClickSell = new Command(DoSell);
            BindingContext = this;

            // Creating step size buttons
            stepSizeButtons = new List<Button>();
            foreach (string n in new string[] { "perc", "óra", "nap", "hónap" })
            {
                Button b = new Button()
                {
                    Text = n,
                    FontSize = 10,
                    WidthRequest = 48,
                    HeightRequest = 16,
                    CornerRadius = 8,
                    Padding = 0,

                    BackgroundColor = Color.WhiteSmoke,
                    TextColor = Color.Gray
                };

                b.Clicked += StepClicked;
                stepSizeButtons.Add(b);
                StepSizes.Children.Add(b);
            }

            //
            DoRefresh();
        }

        /* Local cache for time series' */

        ChartEntry[] entries_1min;
        ChartEntry[] entries_1hour;
        ChartEntry[] entries_1day;
        ChartEntry[] entries_1month;

        /* Redraw the graph area */

        private void StepClicked(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            DoDraw(b.Text);
        }

        /* Refreshing this page */

        private async void DoRefresh()
        {
            // Listing history elements
            tranzactionHistory.Children.Clear();
            foreach (Tranzaction n in tranzactions)
            {
                string upperRightText = n.amount < 0 ? "-$" + Tools.PrettyPrint(n.price * n.amount * -1) : "+$" + Tools.PrettyPrint(n.price * n.amount);
                string lowerRightText = $"{n.amount} {Tools.Translate(instrument.instrument_type)} {n.price} áron";
                Color upperRightColor = n.amount < 0 ? Color.Red : Color.Green;

                Frame f = Tools.PrettyFrame(n.creation_time, upperRightText, "", lowerRightText, Color.Black, upperRightColor);
                tranzactionHistory.Children.Add(f);
            }

            historyDetailsLabel.Text = tranzactions.Length > 0 ? $"összesen {tranzactions.Length} tranzakció" : "még nem volt tranzakció";

            // Instrument data
            companyLabel.Text = string.IsNullOrEmpty(instrument.name) ? instrument.symbol : instrument.name;
            exchangeLabel.Text = $"{instrument.exchange} ({instrument.country})";

            // Acquering real time price
            string debug = $"https://api.twelvedata.com/price?apikey={EnvironmentVariable.APIKey}{instrument.getAPIArguments()}";
            EndpointResponse response = await NetHelper.SendRequest(debug);

            // fail
            if (!response.success)
            {
                priceLabel.Text = response.message.ToLower().Contains("http forbidden") ? "Ez az eszköz nem elérhető!": "Hiba történt!";
                holdingsAmountLabel.Text = "";
                holdingsValueLabel.Text = "";
                earningsLabel.Text = "";

                buyButton.IsEnabled = false;
                sellButton.IsEnabled = false;

                priceLabel.TextColor = Color.Red;
                RefreshContainer.IsRefreshing = false;
                return;
            }

            // success
            else
            {
                JObject json = JObject.Parse(response.message);
                realtimePrice = double.Parse((string)json["price"]);
                priceLabel.Text = $"${Tools.PrettyPrint(realtimePrice)}";
                priceLabel.TextColor = Color.Black;

                buyButton.IsEnabled = true;
                sellButton.IsEnabled = true;
            }

            // Holdings (amount)
            holdings = tranzactions.Select(x => x.amount).Sum();
            holdingsAmountLabel.Text = holdings > 0 ? $"portfolióban: {Tools.PrettyPrint(holdings)}" : "";

            // Holdings (value)
            double currentInvestment = holdings * realtimePrice;
            holdingsValueLabel.Text = holdings > 0 ? $"összesen: {Tools.PrettyPrint(currentInvestment)} értékben" : "";

            // Earnings
            double alltimeInvestment = tranzactions.Select(x => x.amount * x.price).Sum();
            double d = holdings > 0 ? Math.Round((currentInvestment - alltimeInvestment) * 10000 / alltimeInvestment / 100) : 0; // 2 decimals
            earningsLabel.Text = holdings > 0 ? d.ToString() + "%" : "";
            earningsLabel.TextColor = d >= 0 ? Color.Green : Color.Red;

            //finish
            DoDraw("nap");
            RefreshContainer.IsRefreshing = false;
        }

        /* Drawing line chart && creating tranzaction history list */

        private async void DoDraw(string step)
        {
            // Button highlight
            foreach (Button n in stepSizeButtons)
            {
                n.BackgroundColor = Color.WhiteSmoke;
                n.TextColor = Color.Gray;
            }

            Button b = stepSizeButtons.Where(x => x.Text == step).FirstOrDefault();
            if (b != null)
            {
                b.BackgroundColor = Color.DarkGray;
                b.TextColor = Color.WhiteSmoke;
            }

            // Line chart
            // ....
        }

        /* Buying */

        private async void DoBuy()
        {
            MakeTransaction(false);
        }

        /* Selling */

        private async void DoSell()
        {
            MakeTransaction(true);
        }

        /* Tranzaction */

        private async void MakeTransaction(bool selling)
        {
            // User input 
            string type = Tools.Translate(instrument.instrument_type);
            string action = selling ? "eladás" : "vásárlás";
            string userInput = await DisplayPromptAsync($"{type} {action}", $"A portfolióban jelenleg {holdings} {instrument.symbol} {type} található.", "Mehet", "Mégse", "Mennyiség...", 18, Keyboard.Numeric, "");
            if (userInput == null) return;

            // Parse
            if (!int.TryParse(userInput, out int parsed) || parsed <= 0 || (selling && holdings < parsed))
            {
                await DisplayAlert("Hiba!", "A bevitt mennyiség nem megfelelő.", "Ok");
                return;
            }

            // Call
            double amount = selling ? parsed * +1 : parsed;
            string[] keys = { "portfolio", "instrument", "amount", "price" };
            string[] values = { portfolio.id.ToString(), instrument.id.ToString(), amount.ToString(), realtimePrice.ToString() };
            EndpointResponse response = await NetHelper.SendRequest("TranzactionMake", RequestVariable.FromArray(keys, values));

            // fail
            if (!response.success)
            {
                // ...
                return;
            }

            // success
            // portfolio page redirect
        }
    }
}