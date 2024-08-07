using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Http;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System.IO;
using System.Collections.Generic;
using Google.Apis.YouTube.v3.Data;

namespace YoutubeJournalist.Core.WebAPI.Google.Apis.Youtube.V3
{
    public class YoutubeService : IWebAPI
    {
        public IClientService ServiceBase { get; private set; }

        readonly int _maxResults;
        readonly string _apiKey;
        readonly string _clientId;
        readonly string _clientSecret;

        protected const string SearchTypeChannel = "channel";
        protected const string SearchTypePlaylist = "playlist";
        protected const string SearchTypeVideo = "video";

        protected const string ResponseKindChannel = "youtube#channel";
        protected const string ResponseKindPlaylist = "youtube#playlist";
        protected const string ResponseKindVideo = "youtube#video";

        public YoutubeService(string apiKey, string clientId, string clientSecret, int maxResults)
        {
            _maxResults = maxResults;
            _apiKey = apiKey;
            _clientId = clientId;
            _clientSecret = clientSecret;

            var credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(new GoogleAuthorizationCodeFlow.Initializer()
            {
                ClientSecrets = new ClientSecrets()
                {
                    ClientId = _clientId,
                    ClientSecret = _clientSecret,
                },
                Prompt = "none"
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
                ApiKey = _apiKey,
                ApplicationName = "Channels"
            });
        }

        public YoutubeServiceResponse<Youtube_SearchResult> SearchVideos(string searchString, string pageToken = null)
        {
            return SearchImpl(searchString, SearchTypeVideo, pageToken);
        }
        public YoutubeServiceResponse<Youtube_SearchResult> SearchPlaylists(string searchString, string pageToken = null)
        {
            return SearchImpl(searchString, SearchTypePlaylist, pageToken);
        }
        public YoutubeServiceResponse<Youtube_SearchResult> SearchChannels(string searchString, string pageToken = null)
        {
            return SearchImpl(searchString, SearchTypeChannel, pageToken);
        }

        public YoutubeServiceResponse<Youtube_SearchResult> SearchImpl(string searchString, string searchType, string pageToken = null)
        {
            // SearchListRequest
            var request = (this.ServiceBase as YouTubeService).Search.List("snippet");
            request.Q = searchString;
            request.MaxResults = _maxResults;
            request.Type = searchType;
            request.PageToken = pageToken ?? null;

            // Call the search.list method to retrieve results matching the specified query term.
            var response = request.Execute();

            var result = new YoutubeServiceResponse<Youtube_SearchResult>();
            var resultCollection = new List<Youtube_SearchResult>();

            // Store page info for paging through results
            result.PageInfo = response.PageInfo;
            result.Collection = resultCollection;

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            foreach (var item in response.Items)
            {
                if (item.Id.Kind != ResponseKindChannel)
                    throw new Exception(String.Format("Unexpected search result type {0} != {1}", item.Id.Kind, searchType));

                switch (searchType)
                {
                    case SearchTypeChannel:
                    case SearchTypePlaylist:
                    case SearchTypeVideo:
                        break;

                    default:
                        throw new Exception(String.Format("Unexpected search result type {0} != {1}", item.Id.Kind, searchType));
                }

                resultCollection.Add(CreateYoutube_SearchResult(item));
            }

            return result;
        }

        public YoutubeServiceResponse<Youtube_Channel> SearchUserChannels(string userHandle, string pageToken = null)
        {
            // Search for list of channels (search string "repeatable", by tokens)
            //
            // Channel part names:  auditDetails, brandingSettings, contentDetails, contentOwnerDetails,
            //                      conversionPings, etag, id, kind, localizations, snippet, statistics,
            //                      status, topicDetails
            var request = (this.ServiceBase as YouTubeService).Channels.List("id,snippet");

            // Search configuration
            request.PageToken = pageToken ?? null;
            request.MaxResults = _maxResults;
            request.ForHandle = "ryanchicago6028";
            //request.OauthToken = credentials.Result.Token.AccessToken;

            //request.ForHandle = "ryanchicago6028";

            // Call the search.list method to retrieve results matching the specified query term.
            var response = request.Execute();

            var result = new YoutubeServiceResponse<Youtube_Channel>();
            var resultChannels = new List<Youtube_Channel>();

            // Store page info for paging through results
            result.PageInfo = response.PageInfo;
            result.Collection = resultChannels;

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            foreach (var resultItem in response.Items)
            {
                var item = CreateYoutube_Channel(resultItem);

                resultChannels.Add(item);
            }

            return result;
        }

        private Youtube_Channel CreateYoutube_Channel(Channel channel)
        {
            var channelEntity = new Youtube_Channel();
            var auditDetails = new Youtube_ChannelAuditDetails();
            var brandingSettings = new Youtube_ChannelBrandingSettings();

            // Primary fields
            channelEntity.ETag = channel.ETag;
            channelEntity.Id = channel.Id;
            channelEntity.Kind = channel.Kind;

            // Content Owner Details
            channelEntity.ContentOwnerDetails_ContentOwner = channel.ContentOwnerDetails?.ContentOwner;
            channelEntity.ContentOwnerDetails_ETag = channel.ContentOwnerDetails?.ETag;
            channelEntity.ContentOwnerDetails_TimeLinked = channel.ContentOwnerDetails?.TimeLinked ?? DateTime.Now;
            channelEntity.ContentOwnerDetails_TimeLinkedDateTimeOffset = channel.ContentOwnerDetails?.TimeLinkedDateTimeOffset;
            channelEntity.ContentOwnerDetails_TimeLinkedRaw = channel.ContentOwnerDetails?.TimeLinkedRaw;

            // Statistics
            channelEntity.Statistics_CommentCount = (long)(channel.Statistics?.CommentCount ?? 0);
            channelEntity.Statistics_ETag = channel.Statistics?.ETag;
            channelEntity.Statistics_HiddenSubscriberCount = channel.Statistics?.HiddenSubscriberCount;
            channelEntity.Statistics_SubscriberCount = (long)(channel.Statistics?.SubscriberCount ?? 0);
            channelEntity.Statistics_VideoCount = (long)(channel.Statistics?.VideoCount ?? 0);
            channelEntity.Statistics_ViewCount = (long)(channel.Statistics?.ViewCount ?? 0);

            // Status
            channelEntity.Status_ETag = channel.Status?.ETag;
            channelEntity.Status_IsLinked = channel.Status?.IsLinked;
            channelEntity.Status_LongUploadsStatus = channel.Status?.LongUploadsStatus;
            channelEntity.Status_MadeForKids = channel.Status?.MadeForKids;
            channelEntity.Status_PrivacyStatus = channel.Status?.PrivacyStatus;
            channelEntity.Status_SelfDeclaredMadeForKids = channel.Status?.SelfDeclaredMadeForKids;

            // Audit Details
            auditDetails.CommunityGuidelinesGoodStanding = channel.AuditDetails?.CommunityGuidelinesGoodStanding;
            auditDetails.ContentIdClaimsGoodStanding = channel.AuditDetails?.ContentIdClaimsGoodStanding;
            auditDetails.CopyrightStrikesGoodStanding = channel.AuditDetails?.CopyrightStrikesGoodStanding;
            auditDetails.ETag = channel.AuditDetails?.ETag;

            channelEntity.Youtube_ChannelAuditDetails = auditDetails;

            // Branding Settings
            brandingSettings.BannerExternalUrl = channel.BrandingSettings?.Image?.BannerExternalUrl;
            brandingSettings.BannerImageUrl = channel.BrandingSettings?.Image?.BannerImageUrl;
            brandingSettings.BannerMobileExtraHdImageUrl = channel.BrandingSettings?.Image?.BannerMobileExtraHdImageUrl;
            brandingSettings.BannerMobileHdImageUrl = channel.BrandingSettings?.Image?.BannerMobileHdImageUrl;
            brandingSettings.BannerMobileImageUrl = channel.BrandingSettings?.Image?.BannerMobileImageUrl;
            brandingSettings.BannerMobileLowImageUrl = channel.BrandingSettings?.Image?.BannerMobileLowImageUrl;
            brandingSettings.BannerMobileMediumHdImageUrl = channel.BrandingSettings?.Image?.BannerMobileMediumHdImageUrl;
            brandingSettings.BannerTabletExtraHdImageUrl = channel.BrandingSettings?.Image?.BannerTabletExtraHdImageUrl;
            brandingSettings.BannerTabletHdImageUrl = channel.BrandingSettings?.Image?.BannerTabletHdImageUrl;
            brandingSettings.BannerTabletImageUrl = channel.BrandingSettings?.Image?.BannerTabletImageUrl;
            brandingSettings.BannerTabletLowImageUrl = channel.BrandingSettings?.Image?.BannerTabletLowImageUrl;
            brandingSettings.BannerTvHighImageUrl = channel.BrandingSettings?.Image?.BannerTvHighImageUrl;
            brandingSettings.BannerTvImageUrl = channel.BrandingSettings?.Image?.BannerTvImageUrl;
            brandingSettings.BannerTvLowImageUrl = channel.BrandingSettings?.Image?.BannerTvLowImageUrl;
            brandingSettings.BannerTvMediumImageUrl = channel.BrandingSettings?.Image?.BannerTvMediumImageUrl;
            brandingSettings.ETag = channel.BrandingSettings?.Image?.ETag;
            brandingSettings.TrackingImageUrl = channel.BrandingSettings?.Image?.TrackingImageUrl;
            brandingSettings.WatchIconImageUrl = channel.BrandingSettings?.Image?.WatchIconImageUrl;

            channelEntity.Youtube_ChannelBrandingSettings = brandingSettings;

            // Conversation Pings
            if (channel.ConversionPings != null)
            {
                foreach (var ping in channel.ConversionPings.Pings)
                {
                    var conversationPing = new Youtube_ChannelConversationPing();

                    conversationPing.Context = ping.Context;
                    conversationPing.ConversionUrl = ping.ConversionUrl;
                    conversationPing.ETag = ping.ETag;
                    conversationPing.Youtube_Channel = channelEntity;

                    channelEntity.Youtube_ChannelConversationPing.Add(conversationPing);
                }
            }

            return channelEntity;
        }

        private Youtube_SearchResult CreateYoutube_SearchResult(SearchResult searchResult)
        {
            var entity = new Youtube_SearchResult();

            entity.Id_ChannelId = searchResult.Id.ChannelId;
            entity.Id_ETag = searchResult.Id.ETag;
            entity.Id_Kind = searchResult.Id.Kind;
            entity.Id_PlaylistId = searchResult.Id.PlaylistId;
            entity.Id_VideoId = searchResult.Id.VideoId;
            entity.Kind = searchResult.Id.Kind;
            entity.Snippet_ChannelId = searchResult.Snippet.ChannelId;
            entity.Snippet_ChannelTitle = searchResult.Snippet.ChannelTitle;
            entity.Snippet_Description = searchResult.Snippet.Description;
            entity.Snippet_ETag = searchResult.Snippet.ETag;
            entity.Snippet_LiveBroadcastContent = searchResult.Snippet.LiveBroadcastContent;
            entity.Snippet_PublishedAt = searchResult.Snippet.PublishedAt;
            entity.Snippet_PublishedAtDateTimeOffset = searchResult.Snippet.PublishedAtDateTimeOffset;
            entity.Snippet_PublishedAtRaw = searchResult.Snippet.PublishedAtRaw;

            // In foreign key order (see ssms) (Also, nullable thumbnails)
            var defaultThumbnail = searchResult.Snippet.Thumbnails.Default__ != null ? CreateYoutube_Thumbnail(searchResult.Snippet.Thumbnails.Default__) : null;
            var highThumbnail = searchResult.Snippet.Thumbnails.High != null ? CreateYoutube_Thumbnail(searchResult.Snippet.Thumbnails.High) : null;
            var maxresThumbnail = searchResult.Snippet.Thumbnails.Maxres != null ? CreateYoutube_Thumbnail(searchResult.Snippet.Thumbnails.Maxres) : null;
            var mediumThumbnail = searchResult.Snippet.Thumbnails.Medium != null ? CreateYoutube_Thumbnail(searchResult.Snippet.Thumbnails.Medium) : null;
            var standardThumbnail = searchResult.Snippet.Thumbnails.Standard != null ? CreateYoutube_Thumbnail(searchResult.Snippet.Thumbnails.Standard) : null;

            // CHECK PRIMARY KEY:  Youtube_Thumbnail
            entity.Youtube_Thumbnail = string.IsNullOrEmpty(defaultThumbnail.Url) ? null : defaultThumbnail;
            entity.Youtube_Thumbnail1 = string.IsNullOrEmpty(defaultThumbnail.Url) ? null : highThumbnail;
            entity.Youtube_Thumbnail2 = string.IsNullOrEmpty(defaultThumbnail.Url) ? null : maxresThumbnail;
            entity.Youtube_Thumbnail3 = string.IsNullOrEmpty(defaultThumbnail.Url) ? null : mediumThumbnail;
            entity.Youtube_Thumbnail4 = string.IsNullOrEmpty(defaultThumbnail.Url) ? null : standardThumbnail;

            return entity;
        }

        private Youtube_Thumbnail CreateYoutube_Thumbnail(Thumbnail thumbnail)
        {
            var entity = new Youtube_Thumbnail();

            entity.Width = thumbnail.Width;
            entity.Height = thumbnail.Height;
            entity.Url = thumbnail.Url;
            entity.ETag = thumbnail.ETag;

            return entity;
        }

        public void Dispose()
        {
            this.ServiceBase.Dispose();
        }
    }
}
