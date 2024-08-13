using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using WpfCustomUtilities.Extensions.Collection;

using YoutubeJournalist.Core;
using YoutubeJournalist.Core.Service;
using YoutubeJournalist.Core.Service.Interface;
using YoutubeJournalist.Core.Service.Model;
using YoutubeJournalist.ViewModel;

namespace YoutubeJournalist
{
    /// <summary>
    /// Primary component for querying and storing data from social media web services
    /// </summary>
    public class Controller
    {
        readonly string _apiKey;
        readonly string _clientId;
        readonly string _clientSecret;
        readonly string _localDatabaseConnectionString;

        public Controller(ConfigurationViewModel configuration)
        {
            // Youtube service connection
            _apiKey = configuration.ApiKey;
            _clientId = configuration.ClientID;
            _clientSecret = configuration.ClientSecret;
            _localDatabaseConnectionString = configuration.LocalDatabaseConnectionString;
        }

        private IUnitOfWork CreateConnection()
        {
            return new UnitOfWork(_apiKey, _clientId, _clientSecret, _localDatabaseConnectionString);
        }

        /// <summary>
        /// Returns entire collection of search results from local database
        /// </summary>
        public IEnumerable<SearchResultViewModel> GetSearchResults()
        {
            try
            {
                using (var unitOfWork = CreateConnection())
                {
                    return unitOfWork.GetSearchResults()
                                     .Select(result => 
                                     {
                                         return CreateSearchViewModel(result);
                                     })
                                     .Actualize();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Returns entire set of channel entities from local database
        /// </summary>
        public IEnumerable<ChannelViewModel> GetChannels()
        {
            try
            {
                using (var unitOfWork = CreateConnection())
                {
                    var videos = unitOfWork.GetVideos();
                    var commentThreads = videos.SelectMany(video => unitOfWork.GetCommentThreads(video.Id))
                                               .Actualize();


                    return unitOfWork.GetChannels().Select(result =>
                    {
                        var channelVideos = videos.Where(video => video.Youtube_VideoSnippet.ChannelId == result.Id).Actualize();

                        return CreateChannelViewModel(result, channelVideos, commentThreads);
                    }).Actualize();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Returns channel from local database
        /// </summary>
        public ChannelViewModel GetChannel(string channelId)
        {
            try
            {
                using (var unitOfWork = CreateConnection())
                {
                    var channel = unitOfWork.GetChannel(channelId);
                    var videos = unitOfWork.GetVideos(channelId);
                    var commentThreads = videos.SelectMany(video => unitOfWork.GetCommentThreads(video.Id))
                                               .Actualize();

                    return CreateChannelViewModel(channel, videos, commentThreads);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Returns true if channel is in local database
        /// </summary>
        public bool HasChannel(string channelId)
        {
            try
            {
                using (var unitOfWork = CreateConnection())
                {
                    return unitOfWork.HasChannel(channelId);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Returns entire set of video entities from local database
        /// </summary>
        public IEnumerable<VideoViewModel> GetVideos()
        {
            try
            {
                using (var unitOfWork = CreateConnection())
                {
                    var videos = unitOfWork.GetVideos();
                    var commentThreads = videos.SelectMany(video => unitOfWork.GetCommentThreads(video.Id))
                                               .Actualize();

                    return unitOfWork.GetVideos()
                                     .Select(result => 
                                        CreateVideoViewModel(result, 
                                            commentThreads.Where(thread => 
                                                thread.Youtube_CommentThreadSnippet.VideoId == result.Id))).Actualize();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Returns video details from local database
        /// </summary>
        public VideoViewModel GetVideo(string videoId)
        {
            try
            {
                using (var unitOfWork = CreateConnection())
                {
                    var video = unitOfWork.GetVideo(videoId);
                    var commentThreads = unitOfWork.GetCommentThreads(videoId);

                    return CreateVideoViewModel(video, commentThreads);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<CommentThreadViewModel> GetCommentThreads(string videoId)
        {
            try
            {
                using (var unitOfWork = CreateConnection())
                {
                    return unitOfWork.GetCommentThreads(videoId).Select(result => CreateCommentThreadViewModel(result));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Executes basic search as a user on the Youtube platform, and stores the results in the local database
        /// </summary>
        public IEnumerable<SearchResultViewModel> BasicSearch(YoutubeBasicSearchRequest request)
        {
            try
            {
                using (var unitOfWork = CreateConnection())
                {
                    return unitOfWork.BasicSearch(request)
                                     .Select(result =>
                                     {
                                         return CreateSearchViewModel(result);

                                     }).Actualize();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Executes search for channel details, and updates local database
        /// </summary>
        public ChannelViewModel SearchUpdateChannelDetails(YoutubeChannelDetailsRequest request)
        {
            try
            {
                using (var unitOfWork = CreateConnection())
                {
                    // Channel search from Youtube
                    var channel = unitOfWork.SearchUpdateChannelDetails(request);

                    // Query videos
                    var videos = unitOfWork.GetVideos(channel.Id);

                    // Query comment threads for all videos
                    var commentThreads = videos.SelectMany(x => unitOfWork.GetCommentThreads(x.Id));

                    return CreateChannelViewModel(channel, videos, commentThreads);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get will apply service request to look for specific channel, video, or playlist entities,
        /// pulling over extended detail about the entity.
        /// </summary>
        public IEnumerable<VideoViewModel> SearchUpdateVideoDetails(YoutubeVideoDetailsRequest request)
        {
            try
            {
                using (var unitOfWork = CreateConnection())
                {
                    // Channel search from Youtube
                    var videos = unitOfWork.SearchUpdateVideoDetails(request);

                    // Query comment threads for all videos
                    var commentThreads = videos.SelectMany(x => unitOfWork.GetCommentThreads(x.Id));

                    return videos.Select(video => 
                            CreateVideoViewModel(video, 
                                commentThreads.Where(x => 
                                    x.Youtube_CommentThreadSnippet.VideoId == video.Id).Actualize())).Actualize();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets playlist details from Youtube service for the specified channel
        /// </summary>
        public IEnumerable<PlaylistViewModel> SearchUpdatePlaylistDetails(YoutubePlaylistRequest request)
        {
            try
            {
                using (var unitOfWork = CreateConnection())
                {
                    // Channel search from Youtube
                    var playlists = unitOfWork.SearchUpdatePlaylistDetails(request);

                    var result = new List<PlaylistViewModel>();

                    // Search for playlist items for each channel from Youtube
                    foreach (var playlist in playlists)
                    {
                        // Query -> Commit to local database
                        var playlistItems = unitOfWork.SearchUpdatePlaylistItemDetails(new YoutubePlaylistItemRequest()
                        {
                            PlaylistId = playlist.Id
                        });

                        // Create playlist result
                        var viewModel = CreatePlaylistViewModel(playlist, playlistItems);

                        result.Add(viewModel);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Search that retrieves comment threads for:  1) An entire channel, or 2) A set of video (ids)
        /// </summary>
        public IEnumerable<CommentThreadViewModel> SearchCommentThreads(YoutubeCommentThreadRequest request)
        {
            try
            {
                using (var unitOfWork = CreateConnection())
                {
                    // Search for comment threads
                    var commentThreads = unitOfWork.SearchCommentThreads(request);

                    return commentThreads.Select(thread => CreateCommentThreadViewModel(thread))
                                         .Actualize();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private SearchResultViewModel CreateSearchViewModel(Youtube_SearchResult result)
        {
            return new SearchResultViewModel()
            {
                Created = result.Snippet_PublishedAtDateTimeOffset.HasValue ? 
                          result.Snippet_PublishedAtDateTimeOffset.Value.UtcDateTime : DateTime.MinValue,
                Description = result.Snippet_Description,
                ChannelId = result.Id_ChannelId,
                Thumbnail = result.Youtube_ThumbnailDetails.Default__Url,
                Title = result.Snippet_Title,
                Type = result.Id_Kind != YoutubeConstants.ResponseKindVideo ?
                       result.Id_Kind != YoutubeConstants.ResponseKindChannel ?
                       BasicSearchType.Playlist : BasicSearchType.Channel : BasicSearchType.Video
            };
        }
        private ChannelViewModel CreateChannelViewModel(Youtube_Channel result, 
                                                        IEnumerable<Youtube_Video> videos,
                                                        IEnumerable<Youtube_CommentThread> commentThreads)
        {
            return new ChannelViewModel()
            {
                Id = result.Id,
                PrimaryPlaylistId = result.ChannelContentDetails_RelatedPlaylistsData_Uploads,
                BannerUrl = result.Youtube_ChannelBrandingSettings.BannerExternalUrl,
                IconUrl = result.Youtube_ChannelSnippet.Youtube_ThumbnailDetails.Default__Url,
                MadeForKids = result.Status_MadeForKids ?? false,
                Owner = result.ContentOwnerDetails_ContentOwner,
                PrivacyStatus = result.Status_PrivacyStatus,
                SelfDeclaredMadeForKids = result.Status_SelfDeclaredMadeForKids ?? false,
                SubscriberCount = result.Statistics_SubscriberCount ?? 0,
                VideoCount = result.Statistics_VideoCount ?? 0,
                ViewCount = result.Statistics_ViewCount ?? 0,
                Description = result.Youtube_ChannelSnippet.Description,
                Title = result.Youtube_ChannelSnippet.Title,                
                Videos = new ObservableCollection<VideoViewModel>(
                    videos.Select(video =>
                    {
                        return CreateVideoViewModel(video,
                            commentThreads.Where(thread =>
                                thread.Youtube_CommentThreadSnippet.VideoId == video.Id).Actualize());
                    }))
                    
            };
        }
        private PlaylistViewModel CreatePlaylistViewModel(Youtube_Playlist result, IEnumerable<Youtube_PlaylistItem> playlistItems)
        {
            return new PlaylistViewModel()
            {
                ChannelId = result.PlaylistSnippet_ChannelId,
                Description = "",
                Id = result.Id,
                PlaylistItems = new ObservableCollection<PlaylistItemViewModel>(playlistItems.Select(item => CreatePlaylistItemViewModel(item, result)).Actualize()),
                ThumbnailUrl = result.Youtube_ThumbnailDetails.Default__Url,
                Title = result.PlaylistSnippet_Title
            };
        }
        private PlaylistItemViewModel CreatePlaylistItemViewModel(Youtube_PlaylistItem result, Youtube_Playlist playlist)
        {
            return new PlaylistItemViewModel()
            {
                ChannelId = result.PlaylistItemSnippet_ChannelId,
                Description = result.PlaylistItemSnippet_Description,
                Id = result.Id,
                Note = result.PlaylistContentDetails_Note,
                OwnerChannelId = result.PlaylistItemSnippet_VideoOwnerChannelId,
                PlaylistId = playlist.Id,
                Position = result.PlaylistItemSnippet_Position ?? 0,
                PrivacyStatus = result.PlaylistItemStatus_PrivacyStatus,
                ThumbnailUrl = result.Youtube_ThumbnailDetails.Default__Url,
                Title = result.PlaylistItemSnippet_Title,
                VideoId = result.PlaylistContentDetails_VideoId
            };
        }
        private VideoViewModel CreateVideoViewModel(Youtube_Video result, IEnumerable<Youtube_CommentThread> commentThreads)
        {
            return new VideoViewModel()
            {
                Id = result.Id,
                CategoryId = result.Youtube_VideoSnippet.CategoryId,
                CommentCount = result.Youtube_VideoStatistics.CommentCount ?? 0,
                Description = result.Youtube_VideoSnippet.Description,
                DislikeCount = result.Youtube_VideoStatistics.DislikeCount ?? 0,
                LikeCount = result.Youtube_VideoStatistics.LikeCount,
                ViewCount = result.Youtube_VideoStatistics.ViewCount,
                FavoriteCount = result.Youtube_VideoStatistics.FavoriteCount,
                IsMonetized = result.MonetizationDetails_AccessPolicy_Allowed ?? false,
                MadeForKids = result.Youtube_VideoStatus.MadeForKids ?? false,
                Published = result.Youtube_VideoStatus.PublishAtDateTimeOffset.HasValue ? 
                            result.Youtube_VideoStatus.PublishAtDateTimeOffset.Value.UtcDateTime : DateTime.MinValue,
                RejectionReason = result.Youtube_VideoStatus.RejectionReason,
                SelfDeclaredMadeForKids = result.Youtube_VideoStatus.SelfDeclaredMadeForKids ?? false,
                ThumbnailUrl = result.Youtube_VideoSnippet.Youtube_ThumbnailDetails.Default__Url,
                Title = result.Youtube_VideoSnippet.Title,
                UploadStatus = result.Youtube_VideoStatus.UploadStatus,
                CommentThreads = new ObservableCollection<CommentThreadViewModel>(commentThreads.Select(thread => CreateCommentThreadViewModel(thread)).Actualize())
            };
        }
        private CommentThreadViewModel CreateCommentThreadViewModel(Youtube_CommentThread result)
        {
            // Comment Thread -> Thread Snippet -> (Top Level) Comment -> Comment Snippet
            var threadSnippet = result.Youtube_CommentThreadSnippet.Youtube_Comment.Youtube_CommentSnippet;

            // Select thread replies (Comment entities)
            var threadReplies = result.Youtube_CommentListMap.Select(map => map.Youtube_Comment);

            return new CommentThreadViewModel()
            {
                Comment = new CommentViewModel()
                {
                    AuthorChannelId = threadSnippet.AuthorChannelId_Value,
                    AuthorDisplayName = threadSnippet.AuthorDisplayName,
                    AuthorImageUrl = threadSnippet.AuthorProfileImageUrl,
                    AuthorUrl = threadSnippet.AuthorChannelUrl,
                    Display = threadSnippet.TextDisplay,
                    DisplayOriginal = threadSnippet.TextOriginal,
                    LikeCount = threadSnippet.LikeCount ?? 0,
                    ModerationStatus = threadSnippet.ModerationStatus,
                    PublishedDate = threadSnippet.PublishedAtDateTimeOffset.HasValue ? threadSnippet.PublishedAtDateTimeOffset.Value.UtcDateTime : DateTime.MinValue,
                    UpdatedAtDate = threadSnippet.UpdatedAtDateTimeOffset.HasValue ? threadSnippet.UpdatedAtDateTimeOffset.Value.UtcDateTime : DateTime.MinValue,
                    ViewerRating = threadSnippet.ViewerRating
                },
                IsPublic = result.Youtube_CommentThreadSnippet.IsPublic ?? false,
                TotalReplyCount = (int)(result.Youtube_CommentThreadSnippet.TotalReplyCount ?? 0),
                Replies = new ObservableCollection<CommentViewModel>(threadReplies.Select(reply => CreateCommentViewModel(reply)).Actualize())                
            };
        }
        private CommentViewModel CreateCommentViewModel(Youtube_Comment entity)
        {
            return new CommentViewModel()
            {
                AuthorChannelId = entity.Youtube_CommentSnippet.AuthorChannelId_Value,
                AuthorDisplayName = entity.Youtube_CommentSnippet.AuthorDisplayName,
                AuthorImageUrl = entity.Youtube_CommentSnippet.AuthorProfileImageUrl,
                AuthorUrl = entity.Youtube_CommentSnippet.AuthorChannelUrl,
                Display = entity.Youtube_CommentSnippet.TextDisplay,
                DisplayOriginal = entity.Youtube_CommentSnippet.TextOriginal,
                LikeCount = (int)(entity.Youtube_CommentSnippet.LikeCount ?? 0),
                ModerationStatus = entity.Youtube_CommentSnippet.ModerationStatus,
                PublishedDate = (DateTime)(entity.Youtube_CommentSnippet.PublishedAtDateTimeOffset?.UtcDateTime ?? DateTime.MinValue),
                UpdatedAtDate = (DateTime)(entity.Youtube_CommentSnippet.UpdatedAtDateTimeOffset?.UtcDateTime ?? DateTime.MinValue),
                ViewerRating = entity.Youtube_CommentSnippet.ViewerRating
            };
        }
    }
}
