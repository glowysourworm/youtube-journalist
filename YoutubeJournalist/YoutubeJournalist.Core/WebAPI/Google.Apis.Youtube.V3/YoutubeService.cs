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

        public YoutubeServiceResponse<Youtube_Video, Youtube_TopicId, Youtube_TopicCategory> SearchWithFilterVideos(string categoryId, string pageToken = null)
        {
            return SearchWithFilterVideoImpl(categoryId, pageToken);
        }
        public YoutubeServiceResponse<Youtube_Channel, Youtube_TopicId, Youtube_TopicCategory> SearchWithFilterChannels(string categoryId, string pageToken = null)
        {
            return SearchWithFilterChannelImpl(categoryId, pageToken);
        }

        private YoutubeServiceResponse<Youtube_Video, Youtube_TopicId, Youtube_TopicCategory> SearchWithFilterVideoImpl(string categoryId, string pageToken = null)
        {
            // Search for list of channels (search string "repeatable", by tokens)
            //
            // Channel part names:  auditDetails, brandingSettings, contentDetails, contentOwnerDetails,
            //                      conversionPings, etag, id, kind, localizations, snippet, statistics,
            //                      status, topicDetails
            var request = (this.ServiceBase as YouTubeService).Videos.List("id,snippet");

            // Search configuration
            request.PageToken = pageToken ?? null;
            request.MaxResults = _maxResults;
            request.VideoCategoryId = categoryId;

            // Call the search.list method to retrieve results matching the specified query term.
            var response = request.Execute();

            var result = new YoutubeServiceResponse<Youtube_Video, Youtube_TopicId, Youtube_TopicCategory>();
            var resultVideos = new List<Youtube_Video>();
            var resultTopicIds = new List<Youtube_TopicId>();
            var resultTopicCategories = new List<Youtube_TopicCategory>();

            // Store page info for paging through results
            result.PageInfo = response.PageInfo;
            result.Collection = resultVideos;

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            foreach (var resultItem in response.Items)
            {
                var item = CreateYoutube_Video(resultItem, ref resultTopicIds, ref resultTopicCategories);

                resultVideos.Add(item);
            }

            return result;
        }
        private YoutubeServiceResponse<Youtube_Channel, Youtube_TopicId, Youtube_TopicCategory> SearchWithFilterChannelImpl(string categoryId, string pageToken = null)
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
            request.CategoryId = categoryId;

            // Call the search.list method to retrieve results matching the specified query term.
            var response = request.Execute();

            var result = new YoutubeServiceResponse<Youtube_Channel, Youtube_TopicId, Youtube_TopicCategory>();
            var resultChannels = new List<Youtube_Channel>();
            var resultTopicIds = new List<Youtube_TopicId>();
            var resultTopicCategories = new List<Youtube_TopicCategory>();

            // Store page info for paging through results
            result.PageInfo = response.PageInfo;
            result.Collection = resultChannels;

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            foreach (var resultItem in response.Items)
            {
                var item = CreateYoutube_Channel(resultItem, ref resultTopicIds, ref resultTopicCategories);

                resultChannels.Add(item);
            }

            return result;
        }
        private YoutubeServiceResponse<Youtube_SearchResult> SearchImpl(string searchString, string searchType, string pageToken = null)
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

        //private YoutubeServiceResponse<Youtube_Video>

        private Youtube_Channel CreateYoutube_Channel(Channel channel, ref List<Youtube_TopicId> topicIds, ref List<Youtube_TopicCategory> topicCategories)
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

            // ref Lists
            if (channel.TopicDetails?.TopicIds != null)
            {
                foreach (var topicId in channel.TopicDetails.TopicIds)
                {
                    var entityTopicId = new Youtube_TopicId();

                    entityTopicId.Url = topicId;            // Primary Key
                    entityTopicId.ChannelId = channel.Id;   // Loose Foreign Key                    
                    entityTopicId.VideoId = null;           // Loose Foreign Key

                    entityTopicId.Relevant = false;         // Not used for Channel

                    topicIds.Add(entityTopicId);
                }
            }

            if (channel.TopicDetails?.TopicCategories != null)
            {
                foreach (var topicCategory in channel.TopicDetails.TopicCategories)
                {
                    var entityTopicCategory = new Youtube_TopicCategory();

                    entityTopicCategory.Url = topicCategory;            // Primary Key
                    entityTopicCategory.ChannelId = channel.Id;         // Loose Foreign Key                    
                    entityTopicCategory.VideoId = null;                 // Loose Foreign Key

                    topicCategories.Add(entityTopicCategory);
                }
            }

            return channelEntity;
        }
        private Youtube_Video CreateYoutube_Video(Video video, ref List<Youtube_TopicId> topicIds, ref List<Youtube_TopicCategory> topicCategories)
        {
            var entity = new Youtube_Video();

            entity.AgeGating_AlcoholContent = video.AgeGating.AlcoholContent;
            entity.AgeGating_ETag = video.AgeGating.ETag;
            entity.AgeGating_Restricted = video.AgeGating.Restricted;
            entity.AgeGating_VideoGameRating = video.AgeGating.VideoGameRating;
            entity.ContentDetails_Caption = video.ContentDetails.Caption;
            entity.ContentDetails_Definition = video.ContentDetails.Definition;
            entity.ContentDetails_Dimension = video.ContentDetails.Dimension;
            entity.ContentDetails_Duration = video.ContentDetails.Duration;
            entity.ContentDetails_ETag = video.ContentDetails.ETag;
            entity.ContentDetails_HasCustomThumbnail = video.ContentDetails.HasCustomThumbnail;
            entity.ContentDetails_LicensedContent = video.ContentDetails.LicensedContent;
            entity.ContentDetails_Projection = video.ContentDetails.Projection;
            entity.ContentDetails_RegionRestriction_ETag = video.ContentDetails.RegionRestriction.ETag;
            entity.ETag = video.ETag;
            entity.Id = video.Id;
            entity.Kind = video.Kind;
            entity.MonetizationDetails_AccessPolicy_Allowed = video.MonetizationDetails.Access?.Allowed;
            entity.TopicDetails_ETag = video.TopicDetails.ETag;

            // Foreign Key Relationships
            entity.Youtube_VideoSnippet = CreateYoutube_VideoSnippet(video.Snippet);

            var statistic = new Youtube_VideoStatistics();
            statistic.CommentCount = (long)video.Statistics.CommentCount;
            statistic.DislikeCount = (long)video.Statistics.DislikeCount;
            statistic.FavoriteCount = (long)video.Statistics.FavoriteCount;
            statistic.LikeCount = (long)video.Statistics.LikeCount;
            statistic.ViewCount = (long)video.Statistics.ViewCount;
            statistic.ETag = video.ETag;
            
            entity.Youtube_VideoStatistics = statistic;

            var status = new Youtube_VideoStatus();
            status.Description = "FIX THIS FIELD";
            status.Embeddable = video.Status.Embeddable;
            status.ETag = video.Status.ETag;
            status.FailureReason = video.Status.FailureReason;
            status.License = video.Status.License;
            status.MadeForKids = video.Status.MadeForKids;
            status.PrivacyStatus = video.Status.PrivacyStatus;
            status.PublicStatsViewable = video.Status.PublicStatsViewable;
            status.PublishAt = video.Status.PublishAt;
            status.PublishAtDateTimeOffset = video.Status.PublishAtDateTimeOffset;
            status.PublishAtRaw = video.Status.PublishAtRaw;
            status.RejectionReason = video.Status.RejectionReason;
            status.SelfDeclaredMadeForKids = video.Status.SelfDeclaredMadeForKids;
            status.UploadStatus = video.Status.UploadStatus;

            entity.Youtube_VideoStatus = status;

            // ref Lists
            if (video.TopicDetails?.TopicIds != null)
            {
                foreach (var topicId in video.TopicDetails.TopicIds)
                {
                    var entityTopicId = new Youtube_TopicId();

                    entityTopicId.Url = topicId;            // Primary Key
                    entityTopicId.ChannelId = video.Id;     // Loose Foreign Key                    
                    entityTopicId.VideoId = null;           // Loose Foreign Key

                    entityTopicId.Relevant = false;         // Not used for Channel

                    topicIds.Add(entityTopicId);
                }
            }

            if (video.TopicDetails?.TopicCategories != null)
            {
                foreach (var topicCategory in video.TopicDetails.TopicCategories)
                {
                    var entityTopicCategory = new Youtube_TopicCategory();

                    entityTopicCategory.Url = topicCategory;            // Primary Key
                    entityTopicCategory.ChannelId = video.Id;           // Loose Foreign Key                    
                    entityTopicCategory.VideoId = null;                 // Loose Foreign Key

                    topicCategories.Add(entityTopicCategory);
                }
            }

            return entity;
        }
        private Youtube_ChannelSnippet CreateYoutube_ChannelSnippet(ChannelSnippet snippet)
        {
            var entity = new Youtube_ChannelSnippet();

            entity.Country = snippet.Country;
            entity.CustomUrl = snippet.CustomUrl;
            entity.DefaultLanguage = snippet.DefaultLanguage;
            entity.Description = snippet.Description;
            entity.ETag = snippet.ETag;
            entity.Localized_Description = snippet.Localized.Description;
            entity.Localized_ETag = snippet.Localized.ETag;
            entity.Localized_Title = snippet.Localized.Title;
            entity.PublishedAt = snippet.PublishedAt;
            entity.PublishedAtDateTimeOffset = snippet.PublishedAtDateTimeOffset;
            entity.PublishedAtRaw = snippet.PublishedAtRaw;
            entity.Title = snippet.Title;

            entity.Youtube_ThumbnailDetails = CreateYoutube_ThumbnailDetails(snippet.Thumbnails);

            return entity;
        }
        private Youtube_VideoSnippet CreateYoutube_VideoSnippet(VideoSnippet snippet)
        {
            var entity = new Youtube_VideoSnippet();

            entity.CategoryId = snippet.CategoryId;
            entity.ChannelId = snippet.ChannelId;
            entity.ChannelTitle = snippet.ChannelTitle;
            entity.DefaultAudioLanguage = snippet.DefaultAudioLanguage;
            entity.DefaultLanguage = snippet.DefaultLanguage;
            entity.Description = snippet.Description;
            entity.ETag = snippet.ETag;
            entity.LiveBroadcastContent = snippet.LiveBroadcastContent;
            entity.Localized_Description = snippet.Localized.Description;
            entity.Localized_ETag = snippet.Localized.ETag;
            entity.Localized_Title = snippet.Localized.Title;
            entity.PublishedAt = snippet.PublishedAt;
            entity.PublishedAtDateTimeOffset = snippet.PublishedAtDateTimeOffset;
            entity.PublishedAtRaw = snippet.PublishedAtRaw;
            entity.Youtube_ThumbnailDetails = CreateYoutube_ThumbnailDetails(snippet.Thumbnails);

            return entity;
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
            entity.Snippet_Title = searchResult.Snippet.Title;
            entity.Youtube_ThumbnailDetails = CreateYoutube_ThumbnailDetails(searchResult.Snippet.Thumbnails);            

            return entity;
        }
        private Youtube_ThumbnailDetails CreateYoutube_ThumbnailDetails(ThumbnailDetails thumbnailDetails)
        {
            var entity = new Youtube_ThumbnailDetails();

            entity.ETag = thumbnailDetails.ETag;

            entity.Default__ETag = thumbnailDetails.Default__?.ETag;
            entity.Default__Url = thumbnailDetails.Default__?.Url;
            entity.Default__Height = thumbnailDetails.Default__?.Height;
            entity.Default__Width = thumbnailDetails.Default__?.Width;

            entity.High_ETag = thumbnailDetails.High?.ETag;
            entity.High_Url = thumbnailDetails.High?.Url;
            entity.High_Height = thumbnailDetails.High?.Height;
            entity.High_Width = thumbnailDetails.High?.Width;

            entity.Maxres_ETag = thumbnailDetails.Maxres?.ETag;
            entity.Maxres_Url = thumbnailDetails.Maxres?.Url;
            entity.Maxres_Height = thumbnailDetails.Maxres?.Height;
            entity.Maxres_Width = thumbnailDetails.Maxres?.Width;

            entity.Medium_ETag = thumbnailDetails.Medium?.ETag;
            entity.Medium_Url = thumbnailDetails.Medium?.Url;
            entity.Medium_Height = thumbnailDetails.Medium?.Height;
            entity.Medium_Width = thumbnailDetails.Medium?.Width;

            entity.Standard_ETag = thumbnailDetails.Standard?.ETag;
            entity.Standard_Url = thumbnailDetails.Standard?.Url;
            entity.Standard_Height = thumbnailDetails.Standard?.Height;
            entity.Standard_Width = thumbnailDetails.Standard?.Width;

            return entity;
        }

        public void Dispose()
        {
            this.ServiceBase.Dispose();
        }
    }
}
