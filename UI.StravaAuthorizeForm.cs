/*
 * Had to 
 * Tools > NuGet Package Manager > Package Manager Console to open the Package Manager Console window
 * NuGet\Install-Package Microsoft.Web.WebView2 -Version 1.0.1462.37
 */

namespace FellrnrTrainingAnalysis.UI
{
    public partial class StravaAuthorizeForm : Form
    {
        public StravaAuthorizeForm(string callbackUrl)
        {
            InitializeComponent();
            CallbackUrl = callbackUrl;
        }

        string CallbackUrl { get; set; }
        public async void Navigate(string url)
        {
            await stravaWebView.EnsureCoreWebView2Async();
            stravaWebView.CoreWebView2.Navigate(url);
        }

        public string FinalUrl { get; set; } = "";

        private void stravaWebView_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Uri?.StartsWith(CallbackUrl) ?? false) // in case we are forewarded to the callback URL
            {
                FinalUrl = e.Uri;
                this.Hide();
            }
        }
    }
}
