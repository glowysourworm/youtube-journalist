using System;

using Google.Apis.Services;
using Google.Apis.YouTube.v3.Data;

using YoutubeJournalist.Core.Service.Model;

namespace YoutubeJournalist.Core.Service.Interface
{
    public interface IYoutubeService : IDisposable
    {
        /// <summary>
        /// Primary service connection to the API
        /// </summary>
        IClientService ServiceBase { get; }

        YoutubeServiceResponse<SearchResult> Search(YoutubeBasicSearchRequest request);
        YoutubeServiceResponse<SearchResult> SearchUser(YoutubeUserSearchRequest request);
        YoutubeServiceResponse<Playlist> GetPlaylists(YoutubePlaylistRequest request);
        YoutubeServiceResponse<PlaylistItem> GetPlaylistItems(YoutubePlaylistItemRequest request);
        YoutubeServiceResponse<CommentThread> GetCommentThreads(YoutubeCommentThreadRequest request);
        YoutubeServiceResponse<Video> GetVideoDetails(YoutubeVideoDetailsRequest request);
        YoutubeServiceResponse<Channel> GetChannelDetails(YoutubeChannelDetailsRequest request);
    }
}
