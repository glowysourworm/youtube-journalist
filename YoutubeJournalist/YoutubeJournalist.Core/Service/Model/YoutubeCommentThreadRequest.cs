using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.Util;

namespace YoutubeJournalist.Core.Service.Model
{
    /// <summary>
    /// Youtube request that selects either a comment thread for specific video(s), or ALL threads related to
    /// a single channel.
    /// </summary>
    public class YoutubeCommentThreadRequest : YoutubeServiceRequestBase
    {
        public string ChannelId { get; set; }

        public Repeatable<string> VideoIds { get; set; }

        public YoutubeCommentThreadRequest()
        {
            this.VideoIds = new Repeatable<string>(Array.Empty<string>());
        }
    }
}
