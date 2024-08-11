using System;

namespace YoutubeJournalist.Core.Service.Model
{
    public static class YoutubeConstants
    {
        /// <summary>
        /// Video part names:  contentDetails,fileDetails*,id,liveStreamingDetails,localizations,player,processingDetails*,recordingDetails,snippet,
        ///                    statistics,status,suggestions*,topicDetails
        ///                    
        ///                    *NOTE: These fields require enhanced permissions
        /// </summary>
        public static string VideoParts = @"contentDetails,id,liveStreamingDetails,localizations,player,
                                              recordingDetails,snippet,statistics,status,topicDetails";

        /// <summary>
        /// Channel part names:  auditDetails*, brandingSettings, contentDetails, contentOwnerDetails,
        ///                      id, localizations, snippet, statistics,
        ///                      status, topicDetails
        ///                      
        ///                      *NOTE: Some of these fields require enhanced scope permissions
        /// </summary>
        public static string ChannelParts = @"brandingSettings,contentDetails,contentOwnerDetails,id,localizations,snippet,statistics,status,topicDetails";

        /// <summary>
        /// Search part names:  etag, id, kind, snippet
        /// </summary>
        public static string SearchParts = "id, snippet";

        /// <summary>
        /// Comment Thread part names: id,replies,snippet
        /// </summary>
        public static string CommentThreadParts = "id,replies,snippet";

        public static string SearchTypeChannel = "channel";
        public static string SearchTypePlaylist = "playlist";
        public static string SearchTypeVideo = "video";
        public static string SearchCommentThread = "commentThreads";

        public static string ResponseKindChannel = "youtube#channel";
        public static string ResponseKindPlaylist = "youtube#playlist";
        public static string ResponseKindVideo = "youtube#video";
        public static string ResponseKindCommentThread = "youtube#commentThread";
    }

    [Flags]
    public enum BasicFilterType
    {
        /// <summary>
        /// Add this filter to use category ID
        /// </summary>
        CategoryId,

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

    public enum UserFilterType
    {
        /// <summary>
        /// Add this filter to use a user's handle to filter
        /// </summary>
        ForHandle,

        /// <summary>
        /// Add this fitler to use the user's user name to add filtering
        /// </summary>
        ForUser,
    }

    [Flags]
    public enum BasicSearchType
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
}
