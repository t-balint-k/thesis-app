using Xamarin.Forms;

namespace thesis_application
{
    public partial class App : Application
    {
        public static string api_key { get; set; }

        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new login_page());
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