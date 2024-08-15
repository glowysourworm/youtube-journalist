using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenJournalist.Core.Service.Model;

namespace OpenJournalist.Core.Service.Interface
{
    /// <summary>
    /// Primary interface from 3rd party services to the local database
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Returns entire collection of search results from local database
        /// </summary>
        /// <returns></returns>
        LocalServiceResult<Youtube_SearchResult> GetSearchResults();

        /// <summary>
        /// Returns entire set of channel entities from local database
        /// </summary>
        LocalServiceResult<Youtube_Channel> GetChannels();

        /// <summary>
        /// Returns channel from local database
        /// </summary>
        Youtube_Channel GetChannel(string channelId);

        /// <summary>
        /// Returns entire set of video entities from local database
        /// </summary>
        LocalServiceResult<Youtube_Video> GetVideos();

        /// <summary>
        /// Returns set of videos for a specific channel from local database
        /// </summary>
        LocalServiceResult<Youtube_Video> GetVideos(string channelId);

        /// <summary>
        /// Returns specific video details from local database
        /// </summary>
        Youtube_Video GetVideo(string videoId);

        /// <summary>
        /// Returns true if local database has channel
        /// </summary>
        bool HasChannel(string channelId);

        /// <summary>
        /// Returns true if local database has channel
        /// </summary>
        bool HasVideo(string videoId);

        /// <summary>
        /// Returns entire set of comment threads for a video from local database
        /// </summary>
        LocalServiceResult<Youtube_CommentThread> GetCommentThreads(string videoId);

        /// <summary>
        /// Executes basic search as a user on the Youtube platform
        /// </summary>
        LocalServiceResult<Youtube_SearchResult> BasicSearch(YoutubeBasicSearchRequest request);

        /// <summary>
        /// Executes search for playlists for a specified channel, and updates the local database
        /// </summary>
        LocalServiceResult<Youtube_Playlist> SearchUpdatePlaylistDetails(YoutubePlaylistRequest request);

        /// <summary>
        /// Executes search for playlist items for specific playlist, and updates the local database
        /// </summary>
        LocalServiceResult<Youtube_PlaylistItem> SearchUpdatePlaylistItemDetails(YoutubePlaylistItemRequest request);

        /// <summary>
        /// Executes search for channel details, and updates local database
        /// </summary>
        Youtube_Channel SearchUpdateChannelDetails(YoutubeChannelDetailsRequest request);

        /// <summary>
        /// Executes update / add for all channel details to local database
        /// </summary>
        Youtube_Channel SearchUpdateAllChannelDetails(YoutubeChannelDetailsRequest request);

        /// <summary>
        /// Get will apply service request to look for specific channel, video, or playlist entities,
        /// pulling over extended detail about the entity.
        /// </summary>
        LocalServiceResult<Youtube_Video> SearchUpdateVideoDetails(YoutubeVideoDetailsRequest request);

        /// <summary>
        /// Search that retrieves comment threads for:  1) An entire channel, or 2) A set of video (ids)
        /// </summary>
        LocalServiceResult<Youtube_CommentThread> SearchCommentThreads(YoutubeCommentThreadRequest request);
    }
}
