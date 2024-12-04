using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ThesisApplication.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        /* Initialize */

        public ICommand Refresh { get; }
        public ICommand ClickLogin { get; }
        public ICommand ClickSignup { get; }
        public bool successfulSingup { get; set; }

        public LoginPage()
        {
            InitializeComponent();

            // Binding
            Refresh = new Command(DoRefresh);
            ClickLogin = new Command(DoLogin);
            ClickSignup = new Command(GoSignup);
            BindingContext = this;

            // Start
            DoRefresh();
        }

        /* Error label */

        string errorMessage
        {
            set
            {
                errorLabel.Text = value;
                errorLabel.IsVisible = errorLabel.Text != "";
            }
        }

        /* Refresh this page */

        void DoRefresh()
        {
            // Initializing
            errorLabel.IsVisible = false;

            // Controls
            LoginButton.IsEnabled = !errorLabel.IsVisible;
            SignupButton.IsEnabled = !errorLabel.IsVisible;

            // Done
            RefreshContainer.IsRefreshing = false;
        }

        /* Login */

        async void DoLogin()
        {
            // inputs
            errorMessage = "";
            string email = emailEntry.Text ?? "";
            string password = passwordEntry.Text ?? "";

            // required fields
            if (email == "" || password == "")
            {
                errorMessage = "A mezők kitöltése kötelező!";
                return;
            }

            // email regex
            if (!Regex.IsMatch(email, "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$") || email.Length > 50)
            {
                errorMessage = "Az email cím nem megfelelő!";
                return;
            }

            if (errorLabel.IsVisible) return;

            // try login
            (email, password) = Tools.TransformInputs(email, password);
            string[] keys = { "email", "password" };
            string[] values = { email, password };

            EndpointResponse response = await NetHelper.SendRequest("login", RequestVariable.FromArray(keys, values));

            // login fail
            if (!response.success)
            {
                errorMessage = response.message == "NOTFOUND" ? "Email/jelszó pár nem megfelelő!" : response.message;
                return;
            }
            else errorMessage = "";

            if (errorLabel.IsVisible) return;

            // login successful
            JObject sessionVariables = (JObject)JObject.Parse(response.message)["data"][0];
            EnvironmentVariable.userid = (string)sessionVariables["id"];
            EnvironmentVariable.APIKey = (string)sessionVariables["api_key"];

            App.Current.MainPage = new NavigationPage(new PortfolioListPage());
        }

        /* Signup */

        async void GoSignup()
        {
            await Navigation.PushAsync(new SignupPage() { parent = this });
        }

        /* After signup */

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (successfulSingup)
            {
                successfulSingup = false;
                errorMessage = "";

                signupLabel.IsVisible = true;
                await signupLabel.FadeTo(0, 3000);
            }
        }
    }
}