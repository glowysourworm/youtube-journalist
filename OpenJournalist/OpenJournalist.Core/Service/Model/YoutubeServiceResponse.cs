using System.Collections.Generic;
using System.Data.Objects.DataClasses;

using Google.Apis.Requests;
using Google.Apis.YouTube.v3.Data;

namespace OpenJournalist.Core.Service.Model
{
    public class YoutubeServiceResponse<T>
    {
        public IEnumerable<T> Collection { get; set; }

        public PageInfo PageInfo { get; set; }

        public string NextPageToken { get; set; }

        public YoutubeServiceResponse()
        {
            this.Collection = new List<T>();
            this.PageInfo = new PageInfo();
            this.NextPageToken = "";
        }
    }
}
