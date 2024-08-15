using System;

using static Google.Apis.YouTube.v3.SearchResource.ListRequest;

namespace OpenJournalist.Core.Service.Model
{
    public class YoutubeBasicSearchRequest : YoutubeServiceRequestBase
    {
        /// <summary>
        /// Filter parameter - wild card (search string)
        /// </summary>
        public string WildCard { get; set; }

        public YoutubeBasicSearchRequest()
        {
        }
    }
}
