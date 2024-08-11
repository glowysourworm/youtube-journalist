using System;
using System.Threading;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

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
                    ClientSecret = clientSecret,
                }
            },
            new string[]
            {
                YouTubeService.ScopeConstants.YoutubeReadonly
            },
            "rdolan.music.2@gmail.com",
            CancellationToken.None);

            this.ServiceBase = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials.Result,
                ApiKey = apiKey,
                ApplicationName = "channels"
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
                Collection = result.Items
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
                Collection = response.Items,
                PageInfo = response.PageInfo
            };
        }
        public YoutubeServiceResponse<Video> GetVideoDetails(YoutubeVideoDetailsRequest serviceRequest)
        {
            // Create Youtube video list request
            var request = (this.ServiceBase as YouTubeService).Videos.List(YoutubeConstants.VideoParts.ToRepeatable());

            // Set Channel Id for the search
            request.Id = serviceRequest.VideoIds;

            // Call the search.list method to retrieve results matching the specified query term.
            var response = request.Execute();

            return new YoutubeServiceResponse<Video>()
            {
                Collection = response.Items,
                PageInfo = response.PageInfo
            };
        }
        public YoutubeServiceResponse<Channel> GetChannelDetails(YoutubeChannelDetailsRequest serviceRequest)
        {
            // Create Youtube channel list request
            var request = (this.ServiceBase as YouTubeService).Channels.List(YoutubeConstants.ChannelParts.ToRepeatable());

            // Set Channel Id for the search
            request.Id = serviceRequest.ChannelId;

            // Call the search.list method to retrieve results matching the specified query term.
            var response = request.Execute();

            return new YoutubeServiceResponse<Channel>()
            {
                Collection = response.Items,
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

            if (string.IsNullOrEmpty(serviceRequest.ChannelId) &&
                string.IsNullOrEmpty(serviceRequest.VideoIds.ToString()))
                throw new Exception("Youtube commentThread request must specify either channel or video ids");

            if (!string.IsNullOrEmpty(serviceRequest.ChannelId))
                request.AllThreadsRelatedToChannelId = serviceRequest.ChannelId;

            else
                request.Id = serviceRequest.VideoIds;

            // Call the commehtThreadsResource.listRequest method to retrieve results matching the specified query term.
            var response = request.Execute();

            return new YoutubeServiceResponse<CommentThread>()
            {
                Collection = response.Items,
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
