using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Google.Apis.YouTube.v3.Data;

namespace YoutubeJournalist.Core.WebAPI.Google.Apis.Youtube.V3
{
    public class YoutubeServiceSearchChannelsData
    {
        public List<Youtube_Channel> Channels { get; set; }
        public PageInfo ResponsePageInfo { get; set; }

        public YoutubeServiceSearchChannelsData()
        {
            this.Channels = new List<Youtube_Channel>();
            this.ResponsePageInfo = new PageInfo();
        }
    }
}
