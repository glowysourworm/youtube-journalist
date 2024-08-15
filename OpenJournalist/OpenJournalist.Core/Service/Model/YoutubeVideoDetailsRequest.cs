using System;

using Google.Apis.Util;

namespace OpenJournalist.Core.Service.Model
{
    public class YoutubeVideoDetailsRequest : YoutubeServiceRequestBase
    {
        /// <summary>
        /// Filter parameter - Id of video(s)
        /// </summary>
        public Repeatable<string> VideoIds { get; set; }

        public YoutubeVideoDetailsRequest()
        {
            this.VideoIds = new Repeatable<string>(Array.Empty<string>());
        }
    }
}
