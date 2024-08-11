using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.Util;

namespace YoutubeJournalist.Core.Service.Model
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
