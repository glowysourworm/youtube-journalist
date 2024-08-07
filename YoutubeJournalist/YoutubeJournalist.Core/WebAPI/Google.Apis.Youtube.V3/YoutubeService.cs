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

namespace YoutubeJournalist.Core.WebAPI.Google.Apis.Youtube.V3
{
    public class YoutubeService : IWebAPI
    {
        public IClientService ServiceBase { get; private set; }

        readonly int _maxResults;
        readonly string _apiKey;
        readonly string _clientId;
        readonly string _clientSecret;

        public YoutubeService(string apiKey, string clientId, string clientSecret, int maxResults)
        {
            _maxResults = maxResults;
            _apiKey = apiKey;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public YoutubeServiceSearchChannelsData SearchChannels(string pageToken = null)
        {
            // User Credentials
            using (var stream = File.OpenRead("C:\\Backup\\_Source\\google-oauth.json"))
            {
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

            var result = new YoutubeServiceSearchChannelsData();

            // Store page info for paging through results
            result.ResponsePageInfo = response.PageInfo;

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            foreach (var resultItem in response.Items)
            {
                var item = new Youtube_Channel();
                var auditDetails = new Youtube_ChannelAuditDetails();
                var brandingSettings = new Youtube_ChannelBrandingSettings();

                // Primary fields
                item.ETag = resultItem.ETag;
                item.Id = resultItem.Id;
                item.Kind = resultItem.Kind;

                // Content Owner Details
                item.ContentOwnerDetails_ContentOwner = resultItem.ContentOwnerDetails?.ContentOwner;
                item.ContentOwnerDetails_ETag = resultItem.ContentOwnerDetails?.ETag;
                item.ContentOwnerDetails_TimeLinked = resultItem.ContentOwnerDetails?.TimeLinked ?? DateTime.Now;
                item.ContentOwnerDetails_TimeLinkedDateTimeOffset = resultItem.ContentOwnerDetails?.TimeLinkedDateTimeOffset;
                item.ContentOwnerDetails_TimeLinkedRaw = resultItem.ContentOwnerDetails?.TimeLinkedRaw;

                // Statistics
                item.Statistics_CommentCount = (long)(resultItem.Statistics?.CommentCount ?? 0);
                item.Statistics_ETag = resultItem.Statistics?.ETag;
                item.Statistics_HiddenSubscriberCount = resultItem.Statistics?.HiddenSubscriberCount;
                item.Statistics_SubscriberCount = (long)(resultItem.Statistics?.SubscriberCount ?? 0);
                item.Statistics_VideoCount = (long)(resultItem.Statistics?.VideoCount ?? 0);
                item.Statistics_ViewCount = (long)(resultItem.Statistics?.ViewCount ?? 0);

                // Status
                item.Status_ETag = resultItem.Status?.ETag;
                item.Status_IsLinked = resultItem.Status?.IsLinked;
                item.Status_LongUploadsStatus = resultItem.Status?.LongUploadsStatus;
                item.Status_MadeForKids = resultItem.Status?.MadeForKids;
                item.Status_PrivacyStatus = resultItem.Status?.PrivacyStatus;
                item.Status_SelfDeclaredMadeForKids = resultItem.Status?.SelfDeclaredMadeForKids;

                // Audit Details
                auditDetails.CommunityGuidelinesGoodStanding = resultItem.AuditDetails?.CommunityGuidelinesGoodStanding;
                auditDetails.ContentIdClaimsGoodStanding = resultItem.AuditDetails?.ContentIdClaimsGoodStanding;
                auditDetails.CopyrightStrikesGoodStanding = resultItem.AuditDetails?.CopyrightStrikesGoodStanding;
                auditDetails.ETag = resultItem.AuditDetails?.ETag;

                item.Youtube_ChannelAuditDetails = auditDetails;

                // Branding Settings
                brandingSettings.BannerExternalUrl = resultItem.BrandingSettings?.Image?.BannerExternalUrl;
                brandingSettings.BannerImageUrl = resultItem.BrandingSettings?.Image?.BannerImageUrl;
                brandingSettings.BannerMobileExtraHdImageUrl = resultItem.BrandingSettings?.Image?.BannerMobileExtraHdImageUrl;
                brandingSettings.BannerMobileHdImageUrl = resultItem.BrandingSettings?.Image?.BannerMobileHdImageUrl;
                brandingSettings.BannerMobileImageUrl = resultItem.BrandingSettings?.Image?.BannerMobileImageUrl;
                brandingSettings.BannerMobileLowImageUrl = resultItem.BrandingSettings?.Image?.BannerMobileLowImageUrl;
                brandingSettings.BannerMobileMediumHdImageUrl = resultItem.BrandingSettings?.Image?.BannerMobileMediumHdImageUrl;
                brandingSettings.BannerTabletExtraHdImageUrl = resultItem.BrandingSettings?.Image?.BannerTabletExtraHdImageUrl;
                brandingSettings.BannerTabletHdImageUrl = resultItem.BrandingSettings?.Image?.BannerTabletHdImageUrl;
                brandingSettings.BannerTabletImageUrl = resultItem.BrandingSettings?.Image?.BannerTabletImageUrl;
                brandingSettings.BannerTabletLowImageUrl = resultItem.BrandingSettings?.Image?.BannerTabletLowImageUrl;
                brandingSettings.BannerTvHighImageUrl = resultItem.BrandingSettings?.Image?.BannerTvHighImageUrl;
                brandingSettings.BannerTvImageUrl = resultItem.BrandingSettings?.Image?.BannerTvImageUrl;
                brandingSettings.BannerTvLowImageUrl = resultItem.BrandingSettings?.Image?.BannerTvLowImageUrl;
                brandingSettings.BannerTvMediumImageUrl = resultItem.BrandingSettings?.Image?.BannerTvMediumImageUrl;
                brandingSettings.ETag = resultItem.BrandingSettings?.Image?.ETag;
                brandingSettings.TrackingImageUrl = resultItem.BrandingSettings?.Image?.TrackingImageUrl;
                brandingSettings.WatchIconImageUrl = resultItem.BrandingSettings?.Image?.WatchIconImageUrl;

                item.Youtube_ChannelBrandingSettings = brandingSettings;

                // Conversation Pings
                if (resultItem.ConversionPings != null)
                {
                    foreach (var ping in resultItem.ConversionPings.Pings)
                    {
                        var conversationPing = new Youtube_ChannelConversationPing();

                        conversationPing.Context = ping.Context;
                        conversationPing.ConversionUrl = ping.ConversionUrl;
                        conversationPing.ETag = ping.ETag;
                        conversationPing.Youtube_Channel = item;

                        item.Youtube_ChannelConversationPing.Add(conversationPing);
                    }
                }

                result.Channels.Add(item);
            }

            return result;
        }

        public void Dispose()
        {
            this.ServiceBase.Dispose();
        }

        //public async void Run(string search)
        //{
        //    // 
        //    var searchListRequest = (this.ServiceBase as YouTubeService).Search.List("snippet");
        //    searchListRequest.Q = search; 
        //    searchListRequest.MaxResults = _maxResults;

        //    // Call the search.list method to retrieve results matching the specified query term.
        //    var searchListResponse = await searchListRequest.ExecuteAsync();

        //    List<string> videos = new List<string>();
        //    List<string> channels = new List<string>();
        //    List<string> playlists = new List<string>();

        //    // Add each result to the appropriate list, and then display the lists of
        //    // matching videos, channels, and playlists.
        //    foreach (var searchResult in searchListResponse.Items)
        //    {
        //        switch (searchResult.Id.Kind)
        //        {
        //            case "youtube#video":
        //                videos.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.VideoId));
        //                break;

        //            case "youtube#channel":
        //                channels.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.ChannelId));
        //                break;

        //            case "youtube#playlist":
        //                playlists.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.PlaylistId));
        //                break;
        //        }
        //    }

        //    Console.WriteLine(String.Format("Videos:\n{0}\n", string.Join("\n", videos)));
        //    Console.WriteLine(String.Format("Channels:\n{0}\n", string.Join("\n", channels)));
        //    Console.WriteLine(String.Format("Playlists:\n{0}\n", string.Join("\n", playlists)));
        //}
    }
}
