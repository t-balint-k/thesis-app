using Microcharts;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public SearchPage parentSearchPage { get; set; }
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
            foreach (Tranzaction n in tranzactions.OrderByDescending(x => x.creation_time))
            {
                string upperRightText = instrument.instrument_type == "forex_pairs" || instrument.instrument_type == "cryptocurrencies"
                    ? (n.amount < 0 ? $"- {Tools.PrettyPrint(n.price * n.amount * -1, 4)}" : $"+ {Tools.PrettyPrint(n.price * n.amount, 4)}")
                    : (n.amount < 0 ? $"- {instrument.currency} {Tools.PrettyPrint(n.price * n.amount * -1)}" : $"+ {instrument.currency} {Tools.PrettyPrint(n.price * n.amount)}");
                string lowerRightText = $"{Tools.PrettyPrint(n.amount)} {Tools.Translate(instrument.instrument_type)} {n.price} áron";
                Color upperRightColor = n.amount < 0 ? Color.Red : Color.Green;

                Frame f = Tools.PrettyFrame(n.creation_time, upperRightText, "", lowerRightText, Color.Black, upperRightColor);
                tranzactionHistory.Children.Add(f);
            }

            historyDetailsLabel.Text = tranzactions.Length > 0 ? $"összesen {tranzactions.Length} tranzakció" : "még nem volt tranzakció";

            // Instrument data
            companyLabel.Text = string.IsNullOrEmpty(instrument.name) ? instrument.symbol : instrument.name;
            exchangeLabel.Text = instrument.instrument_type == "forex_pairs" || instrument.instrument_type == "cryptocurrencies"
                ? $"{instrument.currency_base} / {instrument.currency_quote}"
                : $"{instrument.exchange} ({instrument.country})";

            // Holdings (amount)
            holdings = tranzactions.Select(x => x.amount).Sum();
            holdingsAmountLabel.Text = holdings > 0 ? $"portfolióban: {Tools.PrettyPrint(holdings)}" : "";

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
                JObject json;
                try
                {
                    json = JObject.Parse(response.message);
                    realtimePrice = double.Parse((string)json["price"]);
                }

                // Depleting API credits
                catch
                {
                    await DisplayAlert("Hiba!", "Ingyenes API keret kimerítve, próbáld meg később.", "Ok");
                    RefreshContainer.IsRefreshing = false;
                    return;
                }

                priceLabel.TextColor = Color.Black;
                priceLabel.Text = instrument.instrument_type == "forex_pairs" || instrument.instrument_type == "cryptocurrencies"
                    ? $"{Tools.PrettyPrint(realtimePrice, 4)}"
                    : $"{instrument.currency} {Tools.PrettyPrint(realtimePrice)}";

                buyButton.IsEnabled = true;
                sellButton.IsEnabled = holdings != 0;
            }

            // Holdings (value)
            double currentInvestment = holdings * realtimePrice;
            holdingsValueLabel.Text = holdings > 0 ? $"összesen: {Tools.PrettyPrint(currentInvestment)} értékben" : "";

            // Earnings
            double alltimeInvestment = tranzactions.Select(x => x.amount * x.price).Sum();
            double d = holdings > 0 ? Math.Round((currentInvestment - alltimeInvestment) * 10000 / alltimeInvestment / 100) : 0; // 2 decimals
            earningsLabel.Text = holdings > 0 ? d.ToString() + "%" : "";
            earningsLabel.TextColor = d >= 0 ? Color.Green : Color.Red;

            // Finish
            DoDraw("nap");
            RefreshContainer.IsRefreshing = false;
        }

        /* Drawing line chart */

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

            // Instrument unavailable
            if (priceLabel.TextColor == Color.Red)
            {
                await DisplayAlert("Hiba!", "Ez az eszköz nem elérhető.", "Ok");
                return;
            }

            // Line chart
            ChartEntry[] current = new ChartEntry[0];
            switch (step)
            {
                case "perc":
                    if (entries_1min == null) entries_1min = await GetTimeSeries("1min", 60);
                    current = entries_1min;
                    break;
                case "óra":
                    if (entries_1hour == null) entries_1hour = await GetTimeSeries("1h", 24);
                    current = entries_1hour;
                    break;
                case "nap":
                    if (entries_1day == null) entries_1day = await GetTimeSeries("1day", 30);
                    current = entries_1day;
                    break;
                case "hónap":
                    if (entries_1month == null) entries_1month = await GetTimeSeries("1month", 12);
                    current = entries_1month;
                    break;
            }

            // Has errors
            if (current.Length == 0) return;

            // Math
            int min = (int)Math.Floor(current.Select(x => x.Value).Min());
            int max = (int)Math.Ceiling(current.Select(x => x.Value).Max());
            int pedding = (int)Math.Round((max - min) * 0.1);

            // The chart
            if (min == 0 && max == 0)
            {
                await DisplayAlert("Hiba!", "Ennek az eszköznek nincs nincs árfolyam története.", "Ok");
                buyButton.IsEnabled = false;
                sellButton.IsEnabled = false;
                return;
            }

            chartView.Chart = new LineChart()
            {
                Margin = 0,
                LabelOrientation = Orientation.Vertical,
                LabelTextSize = 24,
                ValueLabelOrientation = Orientation.Horizontal,
                LineMode = LineMode.Straight,
                EnableYFadeOutGradient = true,

                Entries = current,
                MinValue = min - pedding,
                MaxValue = max + pedding
            };

            // API call
            async Task<ChartEntry[]> GetTimeSeries(string interval, int size)
            {
                JObject[] timeSeries;

                // Http request
                try
                {
                    EndpointResponse response = await NetHelper.SendRequest($"https://api.twelvedata.com/time_series?{instrument.getAPIArguments()}&interval={interval}&outputsize={size}&apikey={EnvironmentVariable.APIKey}");

                    // fail
                    if (!response.success)
                    {
                        await DisplayAlert("Hiba!", "Külső szolgáltató hiba lépett fel, próbáld meg később.", "Ok");
                        return new ChartEntry[0];
                    }

                    // extracting the datapoints
                    timeSeries = JObject.Parse(response.message)["values"].ToObject<JObject[]>();
                }

                catch
                {
                    await DisplayAlert("Hiba!", "A historikus adatok lekérdezésében hiba lépett fel, próbáld meg később.", "Ok");
                    return new ChartEntry[0];
                }

                // Parsing response data
                List<ChartEntry> entries = new List<ChartEntry>();
                foreach (JObject n in timeSeries)
                {
                    // data point
                    float f = float.Parse((string)n["close"]);
                    ChartEntry e = new ChartEntry(f) { Label = "", Color = SKColor.Parse("#f5d86e") };
                    entries.Add(e);
                }

                // Done
                return entries.OrderBy(x => x.Label).ToArray();
            }
        }

        /* Buying */

        private async void DoBuy()
        {
            if (instrument.valid_to != null)
            {
                // Instrument retired
                await DisplayAlert("Ez az eszköz kivezetésre került!", "", "Ok");
                return;
            }

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
            if (!double.TryParse(userInput, out double parsed) || parsed <= 0 || (selling && holdings < parsed))
            {
                await DisplayAlert("Hiba!", "A bevitt mennyiség nem megfelelő.", "Ok");
                return;
            }

            // Call
            double amount = selling ? parsed * -1 : parsed;
            string[] keys = { "portfolio", "instrument", "amount", "price" };
            string[] values = { portfolio.id.ToString(), instrument.id.ToString(), amount.ToString(), realtimePrice.ToString() };
            EndpointResponse response = await NetHelper.SendRequest("TranzactionMake", RequestVariable.FromArray(keys, values));

            // fail
            if (!response.success)
            {
                string msg = response.message == "Már létezik!" ? "Kerettúllépés!" : response.message;
                await DisplayAlert("Hiba!", msg, "Ok");
                return;
            }

            // success
            if (parentSearchPage != null) Navigation.RemovePage(parentSearchPage);
            await Navigation.PopAsync();
        }
    }
}