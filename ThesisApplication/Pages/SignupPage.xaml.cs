using System.Text.RegularExpressions;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ThesisApplication.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignupPage : ContentPage
    {
        /* Initialize */
        public ICommand ClickSignup { get; }
        public LoginPage parent { get; set; }

        public SignupPage()
        {
            InitializeComponent();

            // Binding
            ClickSignup = new Command(DoSignup);
            BindingContext = this;
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

        /* Singup */

        async void DoSignup()
        {
            // inputs
            errorMessage = "";
            string email = emailEntry.Text ?? "";
            string password = passwordEntry.Text ?? "";
            string repass = repassEntry.Text ?? "";

            // required fields
            if (email == "" || password == "" || repass == "") errorMessage = "A mezők kitöltése kötelező!";

            // passwords
            if (password != repass) errorMessage = "A jelszavak nem egyeznek!";

            // email
            if (!Regex.IsMatch(email, "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$") || email.Length > 50) errorMessage = "Az email cím nem megfelelő!";

            if (errorLabel.IsVisible) return;

            // try sign up
            (email, password) = Tools.TransformInputs(email, password);
            string[] keys = { "email", "password" };
            string[] values = { email, password };

            EndpointResponse response = await NetHelper.SendRequest("signup", RequestVariable.FromArray(keys, values));

            // sign up fail
            if (!response.success)
            {
                errorMessage = response.message == "NOTFOUND" ? "Ez az email cím már regisztrálva van!" : response.message;
                return;
            }
            else errorMessage = "";

            if (errorLabel.IsVisible) return;

            // sign up successful
            parent.successfulSingup = true;
            Navigation.RemovePage(this);
        }
    }
}