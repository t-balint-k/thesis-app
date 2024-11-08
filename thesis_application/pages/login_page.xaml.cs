using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace thesis_application
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class login_page : ContentPage
    {
        public ICommand click_login { get; }
        public ICommand click_signup { get; }

        public login_page()
        {
            InitializeComponent();

            click_login = new Command(do_login);
            click_signup = new Command(go_signup);

            BindingContext = this;
        }

        public void do_login()
        {
            App.Current.MainPage = new NavigationPage(new portfolio_page());
        }

        async public void go_signup()
        {
            await Navigation.PushAsync(new signup_page());
        }
    }
}