using System.Security.Cryptography;
using System.Text;
using System.Web;
using Xamarin.Forms;

namespace ThesisApplication
{
    /* Common utilites */

    public class Tools
    {
        // Transform email-password combo
        public static (string, string) TransformInputs(string email, string password)
        {
            // email
            email = HttpUtility.UrlEncode(email);

            // password
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte n in bytes) builder.Append(n.ToString("x2"));
                password = builder.ToString();
            }

            // done
            return (email, password);
        }

        // Pretty print label content
        public static string PrettyPrint(double d)
        {
            return d.ToString("#,0.00").Replace(",", " ").Replace(".", ",");
        }

        // Translating instrument names
        public static string Translate(string type)
        {
            switch (type)
            {
                case "stocks":           return "részvény";
                case "forex_pairs":      return "deviza pár";
                case "cryptocurrencies": return "kriptovaluta";
                case "funds":            return "alap";
                case "bonds":            return "kötvény";
                case "etfs":             return "tőzsdei alap";
                case "indices":          return "index";
                case "commodities":      return "árupiaci termék";
                default: return "ismeretlen instrumentum";
            }
        }

        // Pretty frame for result lists
        public static Frame PrettyFrame(string upperLeftText, string upperRightText, string lowerLeftText, string lowerRightText, Color leftColor, Color rightColor, GestureRecognizer gesture = null)
        {
            Label upperLeftLabel = new Label()
            {
                Text = upperLeftText,
                TextColor = leftColor,
                FontAttributes = FontAttributes.Bold,
                FontSize = 16,
                Margin = new Thickness(12, 0, 0, 0)
            };

            Label upperRightLabel = new Label()
            {
                Text = upperRightText,
                TextColor = rightColor,
                FontAttributes = FontAttributes.Bold,
                FontSize = 16,
                Margin = new Thickness(0, 0, 12, 0)
            };

            Label lowerLeftLabel = new Label()
            {
                Text = lowerLeftText,
                TextColor = Color.DimGray,
                FontAttributes = FontAttributes.None,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 0)
            };

            Label lowerRightLabel = new Label()
            {
                Text = lowerRightText,
                TextColor = Color.DimGray,
                FontAttributes = FontAttributes.Italic,
                FontSize = 12,
                Margin = new Thickness(0, 0, 3, 3)
            };

            AbsoluteLayout.SetLayoutBounds(upperLeftLabel,  new Rectangle(0, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutBounds(upperRightLabel, new Rectangle(1, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutBounds(lowerLeftLabel,  new Rectangle(0,   1, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutBounds(lowerRightLabel, new Rectangle(1,   1, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

            AbsoluteLayout.SetLayoutFlags(upperLeftLabel,  AbsoluteLayoutFlags.PositionProportional);
            AbsoluteLayout.SetLayoutFlags(upperRightLabel, AbsoluteLayoutFlags.PositionProportional);
            AbsoluteLayout.SetLayoutFlags(lowerRightLabel, AbsoluteLayoutFlags.PositionProportional);

            // Frame entity
            return new Frame()
            {
                HasShadow = true,
                Padding = 0,
                HeightRequest = 64,
                Content = new AbsoluteLayout() { Children = { upperLeftLabel, upperRightLabel, lowerRightLabel } },
                GestureRecognizers = { gesture }
            };
        }
    }

    /* Environmental variables datastructure */

    public static class EnvironmentVariable
    {
        // Session
        public static string APIKey;
        public static string userid;
        public static bool debugMode = true;

        // Webserver
        public static string webIP = "172.213.140.243";
        public static string webPort = "5000";
        public static string webProtocol = "http";
        public static string APIVersion = "v1";
    }

    /* User preferences */

    public static class SearchFilterPreferences
    {
        public static bool stock = true;
        public static bool forex = true;
        public static bool crypto = true;
        public static bool fund = false;
        public static bool bond = false;
        public static bool etf = false;
        public static bool index = false;
        public static bool commodity = false;

        public static string country = "United States";
        public static string exchange = "NASDAQ";
        public static string currency = "";
    }
}