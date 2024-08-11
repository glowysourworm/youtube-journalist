using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeJournalist.Core.Service.Model
{
    public class YoutubeChannelDetailsRequest : YoutubeServiceRequestBase
    {
        /// <summary>
        /// Filter parameter - Id of channel
        /// </summary>
        public string ChannelId { get; set; }
    }
}
