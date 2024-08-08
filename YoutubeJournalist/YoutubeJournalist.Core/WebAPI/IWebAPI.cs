using System;

using Google.Apis.Services;

using YoutubeJournalist.Core.WebAPI.Google.Apis.Youtube.V3;

namespace YoutubeJournalist.Core.WebAPI
{
    public interface IWebAPI : IDisposable
    {
        /// <summary>
        /// Primary service connection to the API
        /// </summary>
        IClientService ServiceBase { get; }

        /// <summary>
        /// Searches for videos using query method - as a user would from their main page
        /// </summary>
        /// <exception cref="Exception">Service error</exception>
        YoutubeServiceResponse<Youtube_SearchResult> SearchVideos(string searchString, string pageToken = null);

        /// <summary>
        /// Searches for playlists using query method - as a user would from their main page
        /// </summary>
        /// <exception cref="Exception">Service error</exception>
        YoutubeServiceResponse<Youtube_SearchResult> SearchPlaylists(string searchString, string pageToken = null);

        /// <summary>
        /// Searches for channels using query method - as a user would from their main page
        /// </summary>
        /// <exception cref="Exception">Service error</exception>
        YoutubeServiceResponse<Youtube_SearchResult> SearchChannels(string searchString, string pageToken = null);

        /// <summary>
        /// Searches for channels using resource list feature of youtube v3
        /// </summary>
        /// <exception cref="Exception">Service error</exception>
        YoutubeServiceResponse<Youtube_Video, Youtube_TopicId, Youtube_TopicCategory> SearchWithFilterVideos(string categoryId, string pageToken = null);

        ///// <summary>
        ///// Searches for channels using resource list feature of youtube v3
        ///// </summary>
        ///// <exception cref="Exception">Service error</exception>
        //YoutubeServiceResponse<Youtube_Channel> SearchWithFilterPlaylists(string categoryId, string pageToken = null);

        /// <summary>
        /// Searches for channels using resource list feature of youtube v3
        /// </summary>
        /// <exception cref="Exception">Service error</exception>
        YoutubeServiceResponse<Youtube_Channel, Youtube_TopicId, Youtube_TopicCategory> SearchWithFilterChannels(string categoryId, string pageToken = null);
    }
}
