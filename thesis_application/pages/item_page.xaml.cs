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

namespace thesis_application
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class item_page : ContentPage
    {
        // the instrument
        security instrument;
        security_type instrument_type;

        // financials
        double instrument_price;
        int holdings;

        // command interfaces
        public ICommand refresh { get; }
        public ICommand buy { get; }
        public ICommand sell { get; }

        // local cache for time series'
        ChartEntry[] entries_1min;
        ChartEntry[] entries_1hour;
        ChartEntry[] entries_1day;
        ChartEntry[] entries_1month;

        // chart step sizes
        List<Button> step_buttons;
        tranzaction[] history;

        public item_page(security_type type, object data)
        {
            InitializeComponent();

            // the instrument
            instrument = (security)data;
            instrument_type = type;

            //labels
            (company_label.Text, exchange_label.Text) = instrument.construct_identifiers();
            
            // bindings
            refresh = new Command(do_refresh);
            buy = new Command(do_buy);
            sell = new Command(do_sell);

            BindingContext = this;

            // creating step size buttons
            step_buttons = new List<Button>();

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

                b.Clicked += step_clicked;
                step_buttons.Add(b);
                step_sizes.Children.Add(b);
            }

            // first load
            do_draw("nap");
            do_refresh();
        }

        private void step_clicked(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            do_draw(b.Text);
        }

        async void do_refresh()
        {
            // api call for real time instrument price (real time as in, real time response from the endpoint; their service shows a delayed price by design)
            try
            {
                (bool success, string response) = await utility.send_request($"https://api.twelvedata.com/price?{instrument.construct_api_arguments()}&apikey={App.api_key}");

                // fail
                if (!success)
                {
                    has_error(response);
                    return;
                }

                // success
                string actual_price = (string)JObject.Parse(response)["price"];
                instrument_price = double.Parse(actual_price);
            }
            catch
            {
                has_error("A szerver hibás adatokat küldött!");
                return;
            }

            // tranzacion history
            history = utility.db_context.Table<tranzaction>().Where(x => x.type == instrument_type && x.security_fk == instrument.id).ToArray();
            tranzaction_history.Children.Clear();

            // calc
            holdings = history.Sum(x => x.amount);
            double expenditure = Math.Truncate(history.Sum(x => x.amount * x.price) * 10000) / 10000;
            double earnings = expenditure == 0 ? 0 : Math.Truncate((((holdings * instrument_price) / expenditure) - 1) * 10000) / 10000;

            /* labels */

            // there is available tranzaction histoy
            if (history.Length > 0)
            {
                // expenditure
                exp_label.Text = "Befektetések: $ " + utility.prittify(expenditure);
                exp_label.IsVisible = true;

                // history block
                string first = history.Select(x => x.timestamp).Min().ToString("yyyy-MM-dd");
                string last = history.Select(x => x.timestamp).Max().ToString("yyyy-MM-dd");
                int c = history.Count();

                if (first == last) history_details_label.Text = $"{c} tranzakció ({first})";
                else history_details_label.Text = $"{c} tranzakció {first} - {last}";

                history_details_label.FontAttributes = FontAttributes.None;
                foreach (tranzaction n in history.OrderByDescending(x => x.timestamp))
                {
                    //

                    Label history_time = new Label()
                    {
                        Text = utility.prittify(n.timestamp),
                        TextColor = Color.DarkGray,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 16,
                        Margin = new Thickness(12, 0, 0, 0)
                    };

                    Label history_price = new Label()
                    {
                        Text = n.amount < 0 ? "-$" + utility.prittify(n.price * n.amount * -1) : "+$" + utility.prittify(n.price * n.amount),
                        TextColor = n.amount < 0 ? Color.Red : Color.Green,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 16,
                        Margin = new Thickness(0, 0, 12, 0)
                    };

                    Label history_calc = new Label()
                    {
                        Text = $"{n.amount} {utility.translate(instrument_type)} {n.price} áron",
                        TextColor = Color.Gray,
                        FontAttributes = FontAttributes.Italic,
                        FontSize = 12,
                        Margin = new Thickness(0, 0, 3, 3)
                    };

                    AbsoluteLayout.SetLayoutBounds(history_time,  new Rectangle(0, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
                    AbsoluteLayout.SetLayoutBounds(history_price, new Rectangle(1, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
                    AbsoluteLayout.SetLayoutBounds(history_calc,  new Rectangle(1,   1, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

                    AbsoluteLayout.SetLayoutFlags(history_time,  AbsoluteLayoutFlags.PositionProportional);
                    AbsoluteLayout.SetLayoutFlags(history_price, AbsoluteLayoutFlags.PositionProportional);
                    AbsoluteLayout.SetLayoutFlags(history_calc,  AbsoluteLayoutFlags.PositionProportional);

                    Frame f = new Frame()
                    {
                        HasShadow = true,
                        Padding = 0,
                        HeightRequest = 64,
                        Content = new AbsoluteLayout() { Children = { history_time, history_price, history_calc } }
                    };

                    tranzaction_history.Children.Add(f);
                }
            }

            // no tranzactions yet
            else
            {
                history_details_label.Text = "Még nem volt tranzakció";
                history_details_label.FontAttributes = FontAttributes.Italic;
                exp_label.Text = "Befektetések: $0";
                exp_label.IsVisible = false;
            }

            // API error
            if (instrument_price == -1)
            {
                price_label.Text = "HIBA!";
                price_label.TextColor = Color.White;
                price_label.BackgroundColor = Color.Red;

                earnings_label.Text = "";

                buy_button.IsEnabled = false;
                sell_button.IsEnabled = false;
            }

            // all good
            else
            {
                // current price
                price_label.Text = "$" +  utility.prittify(instrument_price);
                price_label.TextColor = Color.Black;
                price_label.BackgroundColor = Color.Transparent;

                // profit
                if (earnings > 0)
                {
                    earnings_label.Text = $"+{utility.prittify(earnings)}%";
                    earnings_label.TextColor = Color.Green;
                }

                // loss
                else if(earnings < 0)
                {
                    earnings_label.Text = $"{utility.prittify(earnings)}%";
                    earnings_label.TextColor = Color.Red;
                }

                // even
                else
                {
                    if (history.Length == 0) earnings_label.Text = "";
                    else earnings_label.Text = "0%";
                    earnings_label.TextColor = Color.Blue;
                }

                // buttons
                sell_button.IsEnabled = holdings > 0;
                buy_button.IsEnabled = true;
            }

            // done
            refresh_container.IsRefreshing = false;
        }

        void do_buy()
        {
            // BUY PROMPS
            prompt(false);
        }

        void do_sell()
        {
            // SELL PROMPT
            prompt(true);
        }

        async void prompt(bool selling)
        {
            // user input 
            string user_input = await DisplayPromptAsync($"{utility.translate(instrument_type, true)} vásárlás", $"A portfolióban jelenleg {holdings} {instrument.symbol} {utility.translate(instrument_type)} található.", "Mehet", "Mégse", "Mennyiség...", 18, Keyboard.Numeric, "");
            if (user_input == null) return;

            // parse
            if (!int.TryParse(user_input, out int parsed) || parsed <= 0)
            {
                await DisplayAlert("Hiba!", "A bevitt adat nem megfelelő.", "Ok");
                return;
            }

            // cheating
            if (selling && parsed > holdings)
            {
                await DisplayAlert("Hiba!", $"Nincs ennyi {instrument.symbol} {utility.translate(instrument_type)} a portfolióban.", "Ok");
                return;
            }

            // new tranzaction and refresh
            int amount = selling ? parsed * -1 : parsed;
            utility.db_context.Insert(new tranzaction() { amount = amount, price = instrument_price, timestamp = DateTime.Now, security_fk = instrument.id, type = instrument_type });
            do_refresh();
        }

        async void do_draw(string step)
        {
            // highlight
            foreach (Button n in step_buttons)
            {
                n.BackgroundColor = Color.WhiteSmoke;
                n.TextColor = Color.Gray;
            }

            Button b = step_buttons.Where(x => x.Text == step).FirstOrDefault();
            if (b != null)
            {
                b.BackgroundColor = Color.DarkGray;
                b.TextColor = Color.WhiteSmoke;
            }

            // entries list reference and runtime cache
            ChartEntry[] current = new ChartEntry[0];
            switch (step)
            {
                case "perc":
                    if (entries_1min == null) entries_1min = await get_data("1min", 60);
                    current = entries_1min;
                    break;
                case "óra":
                    if (entries_1hour == null) entries_1hour = await get_data("1h", 24);
                    current = entries_1hour;
                    break;
                case "nap":
                    if (entries_1day == null) entries_1day = await get_data("1day", 30);
                    current = entries_1day;
                    break;
                case "hónap":
                    if (entries_1month == null) entries_1month = await get_data("1month", 12);
                    current = entries_1month;
                    break;
            }

            // has errors
            if (current.Length == 0) return;

            // math
            int min =     (int)Math.Floor(current.Select(x => x.Value).Min());
            int max =     (int)Math.Ceiling(current.Select(x => x.Value).Max());
            int pedding = (int)Math.Round((max - min) * 0.1);

            // chart
            chart_view.Chart = new LineChart()
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

            // api call//

            async Task<ChartEntry[]> get_data(string interval, int size)
            {
                JObject[] time_series;

                // http request
                try
                {
                    (bool success, string response) = await utility.send_request($"https://api.twelvedata.com/time_series?{instrument.construct_api_arguments()}&interval={interval}&outputsize={size}&apikey={App.api_key}");

                    // fail
                    if (!success)
                    {
                        has_error(response);
                        return new ChartEntry[0];
                    }

                    // success
                    error.IsVisible = false;

                    // extracting the datapoints
                    time_series = JObject.Parse(response)["values"].ToObject<JObject[]>();
                }

                catch
                {
                    has_error("A szerver hibás adatokat küldött!");
                    return new ChartEntry[0];
                }

                // parsing response data
                List<ChartEntry> entries = new List<ChartEntry>();
                foreach (JObject n in time_series)
                {
                    // data point
                    float f = (float.Parse((string)n["open"]) + float.Parse((string)n["close"])) / 2;
                    ChartEntry e = new ChartEntry(f) { Color = SKColor.Parse("#f5d86e") };

                    // labels
                    string label = (string)n["datetime"];
                    tranzaction[] relevant = null;

                    switch (interval)
                    {
                        case "1min": // yyyy-mm-dd hh:mm:ss
                            relevant = history.Where(x => x.timestamp.Date.ToString("yyyy-MM-dd hh:mm") == label.Substring(0, 16)).ToArray();
                            label = label.Substring(14, 2);
                            break;
                        case "1h": // yyyy-mm-dd hh:mm:ss
                            relevant = history.Where(x => x.timestamp.Date.ToString("yyyy-MM-dd hh") == label.Substring(0, 13)).ToArray();
                            label = label.Substring(11, 2);
                            break;
                        case "1day": // yyyy-mm-dd
                            relevant = history.Where(x => x.timestamp.Date.ToString("yyyy-MM-dd") == label.Substring(0, 10)).ToArray(); 
                            label = label.Substring( 5, 5);
                            break;
                        case "1month": // yyyy-mm-dd
                            relevant = history.Where(x => x.timestamp.Date.ToString("yyyy-MM") == label.Substring(0, 7)).ToArray();
                            label = label.Substring( 0, 7);
                            break;
                    }
                    e.Label = label;

                    int value_label = relevant == null ? 0 : relevant.Select(x => x.amount).Sum();
                    if (value_label > 0)
                    {
                        e.ValueLabel = $"+{value_label}";
                        e.ValueLabelColor = SKColor.Parse("#08e600");
                    }
                    else if (value_label < 0)
                    {
                        e.ValueLabel = $"{value_label}";
                        e.ValueLabelColor = SKColor.Parse("#d40202");
                    }
                    else e.ValueLabel = " ";

                    // done
                    entries.Add(e);
                }

                // done
                return entries.OrderBy(x => x.Label).ToArray();
            }
        }

        void has_error(string reason)
        {
            // label
            error.Text = reason;
            error.IsVisible = true;

            // chart
            chart_view.Chart = new LineChart();
            foreach (Button n in step_buttons) n.IsVisible = false;

            //
            refresh_container.IsRefreshing = false;
        }
    }
}