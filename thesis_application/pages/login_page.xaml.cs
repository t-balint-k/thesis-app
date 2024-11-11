using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

            // binding
            click_login = new Command(do_login);
            click_signup = new Command(go_signup);

            BindingContext = this;
        }

        async void do_login()
        {
            /*   validations   */
            error.IsVisible = false;
            string email = entry_email.Text ?? "";
            string password = entry_password.Text ?? "";

            // required fields
            if (email == "" || password == "")
            {
                error.Text = "A mezők kitöltése kötelező!";
                error.IsVisible = true;
                return;
            }

            // email
            if (!Regex.IsMatch(email, "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$") || email.Length > 50)
            {
                error.Text = "Az email cím nem megfelelő!";
                error.IsVisible = true;
                return;
            }

            // login try, acquiring API key
            (bool success, string response) = await utility.reach_server("login", email, password);

            // login fail
            if (!success)
            {
                error.Text = response == "NOTFOUND" ? "Email/jelszó pár nem megfelelő!" : response;
                error.IsVisible = true;
                return;
            }

            // login successful
            App.api_key = response.Replace("\"", "");
            App.Current.MainPage = new NavigationPage(new portfolio_page());
        }

        async void go_signup()
        {
            // go to sign up page
            await Navigation.PushAsync(new signup_page());
        }
    }
}