using System.Collections.Generic;
using System.Data.Objects.DataClasses;

using Google.Apis.YouTube.v3.Data;

namespace YoutubeJournalist.Core.WebAPI.Google.Apis.Youtube.V3
{
    public class YoutubeServiceResponse<T> where T : EntityObject
    {
        public IEnumerable<T> Collection { get; set; }
        public PageInfo PageInfo { get; set; }

        public YoutubeServiceResponse()
        {
            this.Collection = new List<T>();
            this.PageInfo = new PageInfo();
        }
    }
}
