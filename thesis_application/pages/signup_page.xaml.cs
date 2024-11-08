using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace thesis_application
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class signup_page : ContentPage
    {
        public ICommand click_signup { get; }

        public signup_page()
        {
            InitializeComponent();

            click_signup = new Command(do_signup);

            BindingContext = this;
        }

        public void do_signup()
        {

        }
    }
}