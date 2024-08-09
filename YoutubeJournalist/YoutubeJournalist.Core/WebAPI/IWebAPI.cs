using System;

using Google.Apis.Services;
using Google.Apis.YouTube.v3;

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
        /// Search (videos, channels, playlists) with filters defined by YoutubeServiceRequest
        /// </summary>
        YoutubeServiceResponse<Youtube_SearchResult> Search(YoutubeServiceRequest serviceRequest);

        /// <summary>
        /// Search videos with specified filtering
        /// </summary>
        YoutubeServiceResponse<Youtube_Video, Youtube_TopicId, Youtube_TopicCategory> GetVideos(YoutubeServiceRequest serviceRequest);

        /// <summary>
        /// Search channels with specified filtering
        /// </summary>
        YoutubeServiceResponse<Youtube_Channel, Youtube_TopicId, Youtube_TopicCategory> GetChannels(YoutubeServiceRequest serviceRequest);
    }
}
