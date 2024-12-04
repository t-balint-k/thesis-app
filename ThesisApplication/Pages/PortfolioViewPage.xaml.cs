using System;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ThesisApplication.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PortfolioViewPage : ContentPage
    {
        /* Initialize */

        public ICommand ClickItem { get; }
        public ICommand ClickAdd { get; }
        public ICommand Refresh { get; }
        private Portfolio portfolio { get; set; }
        private Instrument[] instruments { get; set; }
        private Tranzaction[] tranzactions { get; set; }

        public PortfolioViewPage(Portfolio p)
        {
            InitializeComponent();

            portfolio = p;
            Title = p.name;

            // Binding
            ClickItem = new Command(GoInstrumentView);
            ClickAdd = new Command(AddInstrument);
            Refresh = new Command(DoRefresh);
            BindingContext = this;
        }

        /* Error message */

        private string errorMessage
        {
            set
            {
                errorLabel.Text = value;
                errorLabel.IsVisible = errorLabel.Text != "";
            }
        }

        /* On appearance */

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // acquiring tranzaction list
            EndpointResponse response = await NetHelper.SendRequest("TranzactionList", RequestVariable.FromSingle("portfolio", portfolio.id.ToString()));
            if (!response.success)
            {
                errorMessage = "Hiba a tranzakciós lista lekérdezése közben!";
                InstrumentList.Children.Clear();
                return;
            }

            tranzactions = NetHelper.CastResponse<Tranzaction>(response.message).ToArray();

            // acquiring instrument list
            int[] instrumentIDs = tranzactions.Select(x => x.instrument_fk).Distinct().ToArray();
            instruments = DbHelper.Context.Table<Instrument>().Where(x => instrumentIDs.Contains(x.id)).ToArray();

            DoRefresh();
        }

        /* Refresh this page */

        private async void DoRefresh()
        {
            // Draw results
            InstrumentList.Children.Clear();
            foreach (Instrument n in instruments)
            {
                // still relevant?
                Tranzaction[] t = tranzactions.Where(x => x.instrument_fk == n.id).ToArray();
                double holdings = t.Select(x => x.amount).Sum();
                if (holdings == 0) continue;

                // yes
                string nameField = string.IsNullOrEmpty(n.name) ? n.symbol : n.name;
                Tuple<Portfolio, Instrument, Tranzaction[]> tuple = Tuple.Create(portfolio, n, t);
                GestureRecognizer click = new TapGestureRecognizer() { Command = ClickItem, CommandParameter = tuple };

                Frame f = Tools.PrettyFrame(nameField, $"{Tools.PrettyPrint(holdings)} {Tools.Translate(n.instrument_type)}", "", "", Color.Black, Color.DimGray, click);
                InstrumentList.Children.Add(f);
            }

            // Done
            RefreshContainer.IsRefreshing = false;
        }

        /* Viewing an instrument */

        private async void GoInstrumentView(object parameter)
        {
            (Portfolio p, Instrument i, Tranzaction[] t) = parameter as Tuple<Portfolio, Instrument, Tranzaction[]>;
            await Navigation.PushAsync(new InstrumentViewPage(p, i ,t));
        }

        /* Adding a new instrument */

        private async void AddInstrument()
        {
            await Navigation.PushAsync(new SearchPage(portfolio, tranzactions));
        }
    }
}