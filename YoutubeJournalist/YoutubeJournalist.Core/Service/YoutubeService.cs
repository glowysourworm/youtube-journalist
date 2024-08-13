using System;
using System.Threading;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

using WpfCustomUtilities.Extensions;

using YoutubeJournalist.Core.Extension;
using YoutubeJournalist.Core.Service.Interface;
using YoutubeJournalist.Core.Service.Model;

namespace YoutubeJournalist.Core.Service
{
    public class YoutubeService : IYoutubeService
    {
        public IClientService ServiceBase { get; private set; }

        public YoutubeService(string apiKey, string clientId, string clientSecret)
        {
            var credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(new GoogleAuthorizationCodeFlow.Initializer()
            {
                ClientSecrets = new ClientSecrets()
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret                    
                }
            },
            new string[]
            {
                YouTubeService.ScopeConstants.Youtube,
                YouTubeService.ScopeConstants.YoutubeForceSsl,
                YouTubeService.ScopeConstants.YoutubeReadonly
            },
            "rdolan.music.2@gmail.com",
            CancellationToken.None,
            new FileDataStore(".\\"));                          // Must have file data store for Google to cache data (bearer tokens! required!)

            this.ServiceBase = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials.Result,
                ApiKey = apiKey,
                ApplicationName = "youtube#commentThreadList"
            });
        }

        #region IWebAPI
        public YoutubeServiceResponse<SearchResult> Search(YoutubeBasicSearchRequest serviceRequest)
        {
            // SearchListRequest
            var request = (this.ServiceBase as YouTubeService).Search.List(YoutubeConstants.SearchParts.ToRepeatable());
            request.MaxResults = serviceRequest.PageSize;
            request.Type = serviceRequest.SearchType.ToRepeatable();
            request.PageToken = serviceRequest.UsePageToken ? serviceRequest.PageToken : null;
            request.Order = serviceRequest.SortOrder;

            if (serviceRequest.FilterType.HasFlag(BasicFilterType.CategoryId))
                request.VideoCategoryId = serviceRequest.CategoryId;

            if (serviceRequest.FilterType.HasFlag(BasicFilterType.WildCard))
                request.Q = serviceRequest.WildCard;

            if (serviceRequest.FilterType.HasFlag(BasicFilterType.Date))
            {
                if (serviceRequest.PublishedAfter >= serviceRequest.PublishedBefore)
                    throw new Exception("Youtube search request has improper published date 'window'");

                request.PublishedBeforeDateTimeOffset = serviceRequest.PublishedBefore;
                request.PublishedAfterDateTimeOffset = serviceRequest.PublishedAfter;
            }

            if (serviceRequest.FilterType.HasFlag(BasicFilterType.Duration))
                request.VideoDuration = serviceRequest.Duration;

            // Call Youtube Service
            var result = request.Execute();

            return new YoutubeServiceResponse<SearchResult>()
            {
                PageInfo = result.PageInfo,
                Collection = result.Items ?? Array.Empty<SearchResult>()    // Youtube's API can produce null references
            };
        }
        public YoutubeServiceResponse<SearchResult> SearchUser(YoutubeUserSearchRequest serviceRequest)
        {
            // SearchListRequest
            var request = (this.ServiceBase as YouTubeService).Search.List(YoutubeConstants.SearchParts.ToRepeatable());

            request.MaxResults = serviceRequest.PageSize;
            request.Type = new Repeatable<string>(new string[] { YoutubeConstants.SearchTypeVideo, YoutubeConstants.SearchTypeChannel });
            request.PageToken = serviceRequest.UsePageToken ? serviceRequest.PageToken : null;

            // EXPERIMENTAL:  Try to find method to get public videos for user handle.
            request.ForMine = true;

            //if (serviceRequest.FilterType == UserFilterType.ForHandle)
            //    request.for = serviceRequest.Handle;

            //else if (serviceRequest.FilterType == UserFilterType.ForUser)
            //    request.ForUser = serviceRequest.Username;

            //else
            //    throw new Exception("Youtube user search request must have a filter");

            // Call the search.list method to retrieve results matching the specified query term.
            var response = request.Execute();

            return new YoutubeServiceResponse<SearchResult>()
            {
                Collection = response.Items ?? Array.Empty<SearchResult>(),    // Youtube's API can produce null references
                PageInfo = response.PageInfo
            };
        }
        public YoutubeServiceResponse<Video> GetVideoDetails(YoutubeVideoDetailsRequest serviceRequest)
        {
            // Create Youtube video list request
            var request = (this.ServiceBase as YouTubeService).Videos.List(YoutubeConstants.VideoParts.ToRepeatable());

            if (string.IsNullOrEmpty(serviceRequest.VideoIds.ToString()))
                throw new Exception("Must set the VideoIds field for YoutubeVideoDetailsRequest");

            // Set Channel Id for the search
            request.Id = serviceRequest.VideoIds;

            // Call the search.list method to retrieve results matching the specified query term.
            var response = request.Execute();

            return new YoutubeServiceResponse<Video>()
            {
                Collection = response.Items ?? Array.Empty<Video>(),    // Youtube's API can produce null references
                PageInfo = response.PageInfo
            };
        }
        public YoutubeServiceResponse<Channel> GetChannelDetails(YoutubeChannelDetailsRequest serviceRequest)
        {
            // Create Youtube channel list request
            var request = (this.ServiceBase as YouTubeService).Channels.List(YoutubeConstants.ChannelParts.ToRepeatable());

            // Try and get all public channels for a user
            // request.ForUsername

            // Set Channel Id for the search
            request.Id = serviceRequest.ChannelId;

            // Call the search.list method to retrieve results matching the specified query term.
            var response = request.Execute();

            return new YoutubeServiceResponse<Channel>()
            {
                Collection = response.Items ?? Array.Empty<Channel>(),    // Youtube's API can produce null references
                PageInfo = response.PageInfo
            };
        }

        public YoutubeServiceResponse<Playlist> GetPlaylists(YoutubePlaylistRequest serviceRequest)
        {
            // Create Youtube channel list request
            var request = (this.ServiceBase as YouTubeService).Playlists.List(YoutubeConstants.PlaylistParts.ToRepeatable());

            // Set (Playlist) Id. Channel id searches have permissions issues with Youtube
            if (!String.IsNullOrWhiteSpace(serviceRequest.PlaylistId))
                request.Id = serviceRequest.PlaylistId.ToRepeatable();

            else
                throw new Exception("Must specify either PlaylistId for YoutubePlaylistRequest");

            // Call the search.list method to retrieve results matching the specified query term.
            var response = request.Execute();

            return new YoutubeServiceResponse<Playlist>()
            {
                Collection = response.Items ?? Array.Empty<Playlist>(),    // Youtube's API can produce null references
                PageInfo = response.PageInfo
            };
        }

        public YoutubeServiceResponse<PlaylistItem> GetPlaylistItems(YoutubePlaylistItemRequest serviceRequest)
        {
            // Create Youtube channel list request
            var request = (this.ServiceBase as YouTubeService).PlaylistItems.List(YoutubeConstants.PlaylistItemParts.ToRepeatable());

            // Set Channel Id for the search
            request.PlaylistId = serviceRequest.PlaylistId;

            // Call the search.list method to retrieve results matching the specified query term.
            var response = request.Execute();

            return new YoutubeServiceResponse<PlaylistItem>()
            {
                Collection = response.Items ?? Array.Empty<PlaylistItem>(),    // Youtube's API can produce null references
                PageInfo = response.PageInfo
            };
        }

        public YoutubeServiceResponse<CommentThread> GetCommentThreads(YoutubeCommentThreadRequest serviceRequest)
        {
            // Create Youtube video list request
            var request = (this.ServiceBase as YouTubeService).CommentThreads
                                                              .List(YoutubeConstants.CommentThreadParts.ToRepeatable());

            // Search configuration
            request.PageToken = serviceRequest.UsePageToken ? serviceRequest.PageToken : null;

            if (string.IsNullOrEmpty(serviceRequest.VideoId))
                throw new Exception("Youtube commentThread request must specify either video ids");

            request.VideoId = serviceRequest.VideoId;

            // Call the commehtThreadsResource.listRequest method to retrieve results matching the specified query term.
            var response = request.Execute();

            return new YoutubeServiceResponse<CommentThread>()
            {
                Collection = response.Items ?? Array.Empty<CommentThread>(),    // Youtube's API can produce null references
                PageInfo = response.PageInfo
            };
        }
        #endregion

        public void Dispose()
        {
            this.ServiceBase.Dispose();
        }
    }
}
