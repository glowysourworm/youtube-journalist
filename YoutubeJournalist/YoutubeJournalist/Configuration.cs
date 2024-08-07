using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeJournalist
{
    public class Configuration
    {
        public string ApiKey { get; set; }
        public string ClientID { get; set; }        // OAuth 2.0 (Google)
        public string ClientSecret { get; set; }    // OAuth 2.0 (Google)
        public int MaxSearchResults { get; set; }

        public Configuration()
        {
            this.ApiKey = "AIzaSyANk6jYB8BkD0idtuMVShGfeUeIjhJ2xJs";
            this.ClientID = "88533911009-eonq4qpolnrnrjfbfptmipjn0mb4d0jp.apps.googleusercontent.com";
            this.ClientSecret = "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";
            this.MaxSearchResults = 50;
        }
    }
}
