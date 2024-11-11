using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Text.RegularExpressions;

namespace thesis_application
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class signup_page : ContentPage
    {
        public ICommand click_signup { get; }

        public signup_page()
        {
            InitializeComponent();

            // binding
            click_signup = new Command(do_signup);

            BindingContext = this;
        }

        async void do_signup()
        {
            /*   validations   */
            error.IsVisible = false;

            // required fields
            if (entry_email.Text == "" || entry_password.Text == "" || entry_repass.Text == "")
            {
                error.Text = "A mezők kitöltése kötelező!";
                error.IsVisible = true;
                return;
            }

            // passwords
            if (entry_password.Text != entry_repass.Text)
            {
                error.Text = "A jelszavak nem egyeznek!";
                error.IsVisible = true;
                return;
            }

            // email
            if (!Regex.IsMatch(entry_email.Text, "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$") || entry_email.Text.Length > 50)
            {
                error.Text = "Az email cím nem megfelelő!";
                error.IsVisible = true;
                return;
            }

            // sign up try
            (bool success, string response) = await utility.reach_server("register", entry_email.Text, entry_password.Text);

            // sign up fail
            if (!success)
            {
                error.Text = response == "NOTFOUND" ? "Ez az email cím már regisztrálva van!" : response;
                error.IsVisible = true;
                return;
            }

            // sign up successful
            Navigation.RemovePage(this);
        }
    }
}