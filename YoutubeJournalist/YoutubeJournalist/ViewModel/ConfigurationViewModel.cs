
using WpfCustomUtilities.Extensions;

namespace YoutubeJournalist.ViewModel
{
    public class ConfigurationViewModel : ViewModelBase
    {
        private string _apiKey;
        private string _clientID;// OAuth 2.0 (Google)
        private string _clientSecret;// OAuth 2.0 (Google)

        public string ApiKey
        {
            get { return _apiKey; }
            set { this.RaiseAndSetIfChanged(ref _apiKey, value); }
        }
        public string ClientID
        {
            get { return _clientID; }
            set { this.RaiseAndSetIfChanged(ref _clientID, value); }
        }
        public string ClientSecret
        {
            get { return _clientSecret; }
            set { this.RaiseAndSetIfChanged(ref _clientSecret, value); }
        }

        public ConfigurationViewModel()
        {
            this.ApiKey = "AIzaSyANk6jYB8BkD0idtuMVShGfeUeIjhJ2xJs";
            this.ClientID = "88533911009-eonq4qpolnrnrjfbfptmipjn0mb4d0jp.apps.googleusercontent.com";
            this.ClientSecret = "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";
        }
    }
}
