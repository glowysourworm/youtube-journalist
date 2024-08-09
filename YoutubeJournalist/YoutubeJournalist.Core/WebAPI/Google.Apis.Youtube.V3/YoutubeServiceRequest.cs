using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.Util;

using static Google.Apis.YouTube.v3.SearchResource.ListRequest;

namespace YoutubeJournalist.Core.WebAPI.Google.Apis.Youtube.V3
{
    public class YoutubeServiceRequest
    {
        [Flags]
        public enum FilterType
        {
            /// <summary>
            /// Add this filter to supply the ID (repeatable string)
            /// </summary>
            Id,

            /// <summary>
            /// Add this filter to use category ID
            /// </summary>
            CategoryId,

            /// <summary>
            /// Add this filter to use a user's handle to filter
            /// </summary>
            ForHandle,

            /// <summary>
            /// Add this fitler to use the user's user name to add filtering
            /// </summary>
            ForUser,

            /// <summary>
            /// Add this filter to use a simple wild card search
            /// </summary>
            WildCard,

            /// <summary>
            /// Add this filter to apply published date window
            /// </summary>
            Date,

            /// <summary>
            /// Add this filter to apply video duration
            /// </summary>
            Duration,
        }

        [Flags]
        public enum SearchType
        {
            /// <summary>
            /// Add this search type to include channels
            /// </summary>
            Channel = 1,

            /// <summary>
            /// Add this search type to include videos
            /// </summary>
            Video = 2,

            /// <summary>
            /// Add this search type to include playlists
            /// </summary>
            Playlist = 4
        }

        /// <summary>
        /// Filter type(s) OR them into the request
        /// </summary>
        public FilterType Filter { get; set; }

        /// <summary>
        /// Search type(s) OR them into the request. Default is video.
        /// </summary>
        public SearchType Search { get; set; }

        /// <summary>
        /// Max page size is set by Youtube at 50.
        /// </summary>
        public const int PageSize = 50;

        /// <summary>
        /// Token used to request the next page of information
        /// </summary>
        public string PageToken { get; set; }

        /// <summary>
        /// Request is part of a paged request, so must use the page token
        /// </summary>
        public bool UsePageToken { get; set; }

        /// <summary>
        /// Filter parameter - Id of channel
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// Filter parameter - Id of video(s)
        /// </summary>
        public Repeatable<string> VideoId { get; set; }

        /// <summary>
        /// Filter parameter - CategoryId (of video, or channel)
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// Filter parameter - Handle of user
        /// </summary>
        public string Handle { get; set; }

        /// <summary>
        /// Filter parameter - User name
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Filter parameter - wild card (search string)
        /// </summary>
        public string WildCard { get; set; }

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

        /// <summary>
        /// Constructs the proper search type(s) string from the Search property
        /// </summary>
        public Repeatable<string> GetSearchTypes()
        {
            var searchList = new List<string>();

            if (this.Search.HasFlag(SearchType.Channel))
                searchList.Add("channel");

            if (this.Search.HasFlag(SearchType.Playlist))
                searchList.Add("playlist");

            if (this.Search.HasFlag(SearchType.Video))
                searchList.Add("video");

            // Default:  video
            if (searchList.Count <= 0)
                searchList.Add("video");

            return new Repeatable<string>(searchList);
        }

        public YoutubeServiceRequest()
        {
            this.CategoryId = "";
            this.Handle = "";
            this.Username = "";
            this.VideoId = "";
            this.PageToken = "";
            this.UsePageToken = false;
            this.Filter = FilterType.WildCard;
            this.WildCard = "";
            this.PublishedBefore = DateTimeOffset.MaxValue;
            this.PublishedAfter = DateTimeOffset.MinValue;
            this.Duration = null;
            this.SortOrder = null;
            this.Search = SearchType.Video;
        }
    }
}
