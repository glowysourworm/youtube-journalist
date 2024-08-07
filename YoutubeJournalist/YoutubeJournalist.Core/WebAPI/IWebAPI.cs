using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        /// Searches for a user's channels - relies heavily on permissions for whoever is
        /// logged in, what they can see from their permissions, and so on. Also, returns
        /// user's channel as part of search results.
        /// </summary>
        /// <exception cref="Exception">Service error</exception>
        YoutubeServiceResponse<Youtube_Channel> SearchUserChannels(string userHandle, string pageToken = null);
    }
}
