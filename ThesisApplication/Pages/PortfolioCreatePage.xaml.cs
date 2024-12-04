using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ThesisApplication.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PortfolioCreatePage : ContentPage
    {
        /* Initialize */

        public ICommand ClickCreate { get; }
        public PortfolioListPage parent { get; set; }

        public PortfolioCreatePage()
        {
            InitializeComponent();

            // Binding
            ClickCreate = new Command(CreatePortfolio);
            BindingContext = this;

            // Currency dataset
            string[] currencies = DbHelper.Context.Table<Currency>().Select(x => x.symbol).OrderBy(x => x).ToArray();
            foreach (string n in currencies) currencyPicker.Items.Add(n);
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

        async void CreatePortfolio()
        {
            // Inputs
            errorMessage = "";
            string name = nameEntry.Text ?? "";
            string pool = poolEntry.Text ?? "";
            string currency = (string)(currencyPicker.SelectedItem ?? "");

            if (name == "" || pool == "" || currency == "")
            {
                errorMessage = "A mezők kitöltése kötelező!";
                return;
            }

            if (!double.TryParse(pool, out double d) || d <= 0)
            {
                errorMessage = "A keret nem megfelelő!";
                return;
            }

            // Request
            string[] keys = { "user", "name", "pool", "currency" };
            string[] values = { EnvironmentVariable.userid, name, pool, currency };
            EndpointResponse response = await NetHelper.SendRequest("PortfolioCreate", RequestVariable.FromArray(keys, values));

            // Fail
            if (!response.success)
            {
                errorMessage = response.message;
                return;
            }

            // Success
            parent.createSuccessful = true;
            Navigation.RemovePage(this);
        }
    }
}