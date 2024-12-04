using ThesisApplication.Pages;
using Xamarin.Forms;

namespace ThesisApplication
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            if (EnvironmentVariable.debugMode)
            {
                // DEBUG MODE //
                EnvironmentVariable.userid = "1";
                EnvironmentVariable.APIKey = "0dd18a256cb649e48b71f593c5dd963f";
                MainPage = new NavigationPage(new PortfolioListPage());
                return;
            }

            MainPage = new NavigationPage(new LoginPage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}