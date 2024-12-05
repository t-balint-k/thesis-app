using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ThesisApplication.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PortfolioListPage : ContentPage
    {
        /* Initialize */

        public ICommand ClickItem { get; }
        public ICommand ClickAdd { get; }
        public ICommand Refresh { get; }
        public bool createSuccessful { get; set; }

        public PortfolioListPage()
        {
            InitializeComponent();

            // Binding
            ClickAdd = new Command(GoCreatePortfolioPage);
            ClickItem = new Command(GoPortfolioPage);
            Refresh = new Command(DoRefresh);
            BindingContext = this;

            // Database init
            DBInit();
        }

        /* Database initializer */

        private async void DBInit()
        {
            string init = await DbHelper.DatabaseInit();

            if (init != "")
            {
                infoMessage = init;
                return;
            }

            // Load
            DoRefresh();
        }

        /* Info label */

        private string infoMessage
        {
            set
            {
                infoLabel.Text = value;
                infoLabel.IsVisible = infoLabel.Text != "";
            }
        }

        /* Refresh this page */

        private async void DoRefresh()
        {
            PortfolioList.Children.Clear();

            // Get portfolio list
            EndpointResponse response = await NetHelper.SendRequest("PortfolioList", RequestVariable.FromSingle("user", EnvironmentVariable.userid));

            // Fail
            if (!response.success)
            {
                infoMessage = "Hiba a portfoliók lekérdezése során!";
                RefreshContainer.IsRefreshing = false;
                return;
            }

            // Parsing results
            List<Portfolio> portfolios = NetHelper.CastResponse<Portfolio>(response.message);

            // Draw results
            foreach (Portfolio n in portfolios)
            {
                GestureRecognizer click = new TapGestureRecognizer() { Command = ClickItem, CommandParameter = n };
                Frame f = Tools.PrettyFrame(n.name, $"{n.currency} {n.pool}", "", n.creation_time, Color.Black, Color.DimGray, click);
                PortfolioList.Children.Add(f);
            }

            // Done
            RefreshContainer.IsRefreshing = false;
        }

        /* Adding a new portfolio */

        private async void GoCreatePortfolioPage()
        {
            await Navigation.PushAsync(new PortfolioCreatePage() { parent = this });
        }

        /* After creating a new portfolio */

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (createSuccessful)
            {
                createSuccessful = false;

                createLabel.IsVisible = true;
                DoRefresh();
                await createLabel.FadeTo(0, 3000);
            }
        }

        /* Opening existing portfolio */

        private async void GoPortfolioPage(object parameter)
        {
            await Navigation.PushAsync(new PortfolioViewPage((Portfolio)parameter));
        }
    }
}