using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Google.Apis.YouTube.v3.Data;

using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.Extensions.Collection;

using OpenJournalist.Core;
using OpenJournalist.Core.Service;
using OpenJournalist.Core.Service.Interface;
using OpenJournalist.Core.Service.Model;
using OpenJournalist.ViewModel;

namespace OpenJournalist
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
                    return unitOfWork.GetSearchResults().Collection
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
                    var videos = unitOfWork.GetVideos().Collection;
                    var commentThreads = videos.SelectMany(video => unitOfWork.GetCommentThreads(video.Id).Collection)
                                               .Actualize();


                    return unitOfWork.GetChannels().Collection.Select(result =>
                    {
                        var channelVideos = videos.Where(video => video.VideoSnippet_ChannelId == result.Id).Actualize();

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
                    var videos = unitOfWork.GetVideos(channelId).Collection;
                    var commentThreads = videos.SelectMany(video => unitOfWork.GetCommentThreads(video.Id).Collection)
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
                    var videos = unitOfWork.GetVideos().Collection;
                    var commentThreads = videos.SelectMany(video => unitOfWork.GetCommentThreads(video.Id).Collection)
                                               .Actualize();

                    return unitOfWork.GetVideos().Collection
                                     .Select(result => 
                                        CreateVideoViewModel(result, 
                                            commentThreads.Where(thread => 
                                                thread.VideoId == result.Id))).Actualize();
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
                    var commentThreads = unitOfWork.GetCommentThreads(videoId).Collection;

                    return CreateVideoViewModel(video, commentThreads);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<CommentThreadViewModel> GetVideoCommentThreads(string videoId)
        {
            try
            {
                using (var unitOfWork = CreateConnection())
                {
                    var commentThreads = unitOfWork.GetCommentThreads(videoId);

                    return commentThreads.Collection.Select(thread =>
                    {
                        var threadComment = thread.Youtube_Comment.FirstOrDefault(x => x.CommentThreadId == thread.Id && x.IsTopLevelComment);

                        if (threadComment == null)
                            throw new FormattedException("Invalid Comment Thread Data (no top level comment) UnitOfWork.SearchCommentThreads");

                        return CreateCommentThreadViewModel(thread, threadComment);
                    }).Actualize();
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
                    return unitOfWork.BasicSearch(request).Collection
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
                    var channel = unitOfWork.SearchUpdateAllChannelDetails(request);

                    if (channel == null)
                        return null;

                    // Query videos
                    var videos = unitOfWork.GetVideos(channel.Id).Collection;

                    // Query comment threads for all videos
                    var commentThreads = videos.SelectMany(x => unitOfWork.GetCommentThreads(x.Id).Collection);

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
                    var videos = unitOfWork.SearchUpdateVideoDetails(request).Collection;

                    // Query comment threads for all videos
                    var commentThreads = videos.SelectMany(x => unitOfWork.GetCommentThreads(x.Id).Collection);

                    return videos.Select(video => 
                            CreateVideoViewModel(video, 
                                commentThreads.Where(x => 
                                    x.VideoId == video.Id).Actualize())).Actualize();
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
                    var playlists = unitOfWork.SearchUpdatePlaylistDetails(request).Collection;

                    var result = new List<PlaylistViewModel>();

                    // Search for playlist items for each channel from Youtube
                    foreach (var playlist in playlists)
                    {
                        // Query -> Commit to local database
                        var playlistItems = unitOfWork.SearchUpdatePlaylistItemDetails(new YoutubePlaylistItemRequest()
                        {
                            PlaylistId = playlist.Id
                        }).Collection;

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
                    var commentThreads = unitOfWork.SearchCommentThreads(request).Collection;

                    return commentThreads.Select(thread =>
                    {
                        var threadComment = thread.Youtube_Comment.FirstOrDefault(x => x.CommentThreadId == thread.Id && x.IsTopLevelComment);

                        if (threadComment == null)
                            throw new FormattedException("Invalid Comment Thread Data (no top level comment) UnitOfWork.SearchCommentThreads");

                        return CreateCommentThreadViewModel(thread, threadComment);
                    }).Actualize();
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
                Thumbnail = result.Snippet_ThumbnailDetails_Default__Url,
                Title = result.Snippet_Title
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
                BannerUrl = result.BannerExternalUrl,
                IconUrl = result.Youtube_ChannelSnippet.ThumbnailDetails_Default__Url,
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
                                thread.VideoId == video.Id).Actualize());
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
                ThumbnailUrl = result.PlaylistSnippet_ThumnailDetails_Default__Url,
                Title = result.PlaylistSnippet_Title
            };
        }
        private PlaylistItemViewModel CreatePlaylistItemViewModel(Youtube_PlaylistItem result, Youtube_Playlist playlist)
        {
            return new PlaylistItemViewModel()
            {
                ChannelId = playlist.PlaylistSnippet_ChannelId,
                Id = result.Id,
                Note = result.PlaylistContentDetails_Note,
                OwnerChannelId = result.PlaylistItemSnippet_VideoOwnerChannelId,
                PlaylistId = playlist.Id,
                Position = result.PlaylistItemSnippet_Position ?? 0,
                PrivacyStatus = result.PlaylistItemStatus_PrivacyStatus,
                ThumbnailUrl = result.PlaylistItemSnippet_ThumbnailDetails_Default_Url,
                Title = result.PlaylistItemSnippet_Title,
                VideoId = result.PlaylistContentDetails_VideoId
            };
        }
        private VideoViewModel CreateVideoViewModel(Youtube_Video result, IEnumerable<Youtube_CommentThread> commentThreads)
        {
            return new VideoViewModel()
            {
                Id = result.Id,
                CategoryId = result.VideoSnippet_CategoryId,
                CommentCount = result.Youtube_VideoStatistics.CommentCount ?? 0,
                Description = result.VideoSnippet_Localized_Description,
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
                ThumbnailUrl = result.VideoSnippet_ThumbnailDetails_Default_Url,
                Title = result.VideoSnippet_Localized_Title,
                UploadStatus = result.Youtube_VideoStatus.UploadStatus,
                CommentThreads = new ObservableCollection<CommentThreadViewModel>(commentThreads.Select(thread =>
                {
                    var threadComment = thread.Youtube_Comment.FirstOrDefault(x => x.Id == thread.Id && x.IsTopLevelComment);

                    if (threadComment == null)
                        throw new FormattedException("Invalid comment thread - no top level comment: CommentThreadId {0}", thread.Id);

                    return CreateCommentThreadViewModel(thread, threadComment);
                }).Actualize())
            };
        }
        private CommentThreadViewModel CreateCommentThreadViewModel(Youtube_CommentThread thread, Youtube_Comment threadComment)
        {
            return new CommentThreadViewModel()
            {
                Comment = new CommentViewModel()
                {
                    AuthorChannelId = threadComment.AuthorChannelId_Value,
                    AuthorDisplayName = threadComment.AuthorDisplayName,
                    AuthorImageUrl = threadComment.AuthorProfileImageUrl,
                    AuthorUrl = threadComment.AuthorChannelUrl,
                    Display = threadComment.TextDisplay,
                    DisplayOriginal = threadComment.TextOriginal,
                    LikeCount = threadComment.LikeCount ?? 0,
                    ModerationStatus = threadComment.ModerationStatus,
                    PublishedDate = threadComment.PublishedAtDateTimeOffset.HasValue ? threadComment.PublishedAtDateTimeOffset.Value.UtcDateTime : DateTime.MinValue,
                    UpdatedAtDate = threadComment.UpdatedAtDateTimeOffset.HasValue ? threadComment.UpdatedAtDateTimeOffset.Value.UtcDateTime : DateTime.MinValue
                },
                IsPublic = thread.IsPublic ?? false,
                TotalReplyCount = (int)(thread.TotalReplyCount ?? 0),
                Replies = new ObservableCollection<CommentViewModel>(thread.Youtube_Comment.Select(reply => CreateCommentViewModel(reply)).Actualize())                
            };
        }
        private CommentViewModel CreateCommentViewModel(Youtube_Comment entity)
        {
            return new CommentViewModel()
            {
                AuthorChannelId = entity.AuthorChannelId_Value,
                AuthorDisplayName = entity.AuthorDisplayName,
                AuthorImageUrl = entity.AuthorProfileImageUrl,
                AuthorUrl = entity.AuthorChannelUrl,
                Display = entity.TextDisplay,
                DisplayOriginal = entity.TextOriginal,
                LikeCount = (int)(entity.LikeCount ?? 0),
                ModerationStatus = entity.ModerationStatus,
                PublishedDate = (DateTime)(entity.PublishedAtDateTimeOffset?.UtcDateTime ?? DateTime.MinValue),
                UpdatedAtDate = (DateTime)(entity.UpdatedAtDateTimeOffset?.UtcDateTime ?? DateTime.MinValue)
            };
        }
    }
}
