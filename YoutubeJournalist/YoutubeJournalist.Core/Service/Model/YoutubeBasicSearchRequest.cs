using System;

using static Google.Apis.YouTube.v3.SearchResource.ListRequest;

namespace YoutubeJournalist.Core.Service.Model
{
    public class YoutubeBasicSearchRequest : YoutubeServiceRequestBase
    {
        /// <summary>
        /// Search type(s) OR them into the request. Default is video.
        /// </summary>
        public BasicSearchType SearchType { get; set; }

        /// <summary>
        /// Filter type for basic
        /// </summary>
        public BasicFilterType FilterType { get; set; }

        /// <summary>
        /// Filter parameter - wild card (search string)
        /// </summary>
        public string WildCard { get; set; }

        /// <summary>
        /// Filter parameter - CategoryId (of video, or channel)
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// Filter parameter - published before (certain date)
        /// </summary>
        public DateTimeOffset PublishedBefore { get; set; }

        /// <summary>
        /// Filter parameter - published after (certain date)
        /// </summary>
        public DateTimeOffset PublishedAfter { get; set; }

        /// <summary>
        /// Filter parameter - video duration
        /// </summary>
        public VideoDurationEnum? Duration { get; set; }

        /// <summary>
        /// Sort order of Youtube results
        /// </summary>
        public OrderEnum? SortOrder { get; set; }

        public YoutubeBasicSearchRequest()
        {
            this.SearchType = BasicSearchType.Channel | BasicSearchType.Video;
            this.FilterType = BasicFilterType.WildCard;
        }
    }
}
