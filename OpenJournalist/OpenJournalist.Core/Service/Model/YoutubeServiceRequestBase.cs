using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.Util;

using static Google.Apis.YouTube.v3.SearchResource.ListRequest;

namespace OpenJournalist.Core.Service.Model
{
    public abstract class YoutubeServiceRequestBase
    {
        /// <summary>
        /// Max page size is set by Youtube at 50.
        /// </summary>
        public readonly int PageSize = 50;

        /// <summary>
        /// Token used to request the next page of information
        /// </summary>
        public string PageToken { get; set; }

        /// <summary>
        /// Request is part of a paged request, so must use the page token
        /// </summary>
        public bool UsePageToken { get; set; }

        /// <summary>
        /// Set for request to tell service components to run through all pages 
        /// synchronously, on the current stack
        /// </summary>
        public bool CompletePagingSynchronously { get; set; }

        public YoutubeServiceRequestBase(bool completePagingSynchronously = false)
        {
            this.PageToken = "";
            this.UsePageToken = false;            
            this.CompletePagingSynchronously = completePagingSynchronously;
        }
    }
}
