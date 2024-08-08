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

    public class YoutubeServiceResponse<T, K, V> where T : EntityObject where
                                                   K : EntityObject where
                                                   V : EntityObject
    {
        public IEnumerable<T> Collection { get; set; }

        /// <summary>
        /// Collection of entities that have no foreign key constraints; but were affected
        /// by the API call. This is a slight mismatch in the APIs data structures.
        /// </summary>
        public IEnumerable<K> LooseCollection1 { get; set; }

        /// <summary>
        /// Collection of entities that have no foreign key constraints; but were affected
        /// by the API call.  This is a slight mismatch in the APIs data structures.
        /// </summary>
        public IEnumerable<V> LooseCollection2 { get; set; }

        public PageInfo PageInfo { get; set; }

        public YoutubeServiceResponse()
        {
            this.Collection = new List<T>();
            this.LooseCollection1 = new List<K>();
            this.LooseCollection2 = new List<V>();
            this.PageInfo = new PageInfo();
        }
    }
}
