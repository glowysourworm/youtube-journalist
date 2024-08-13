using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.Extensions.Collection;

using YoutubeJournalist.Core.Service.Interface;
using YoutubeJournalist.Core.Service.Model;

using static Google.Apis.Requests.BatchRequest;

namespace YoutubeJournalist.Core.Service
{
    public class UnitOfWork : IUnitOfWork
    {
        readonly YoutubeJournalistEntities _dbContext;
        readonly IYoutubeService _youtubeService;

        bool _disposed;

        public UnitOfWork(string apiKey, string clientId, string clientSecret, string connectionString)
        {
            // EF Database Connection
            _dbContext = new YoutubeJournalistEntities(connectionString);

            _youtubeService = new YoutubeService(apiKey, clientId, clientSecret);

            _disposed = false;
        }

        #region Local DB Methods

        public IEnumerable<Youtube_SearchResult> GetSearchResults()
        {
            try
            {
                return _dbContext.Youtube_SearchResult.Actualize();      
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<Youtube_Channel> GetChannels()
        {
            try
            {
                return _dbContext.Youtube_Channel.Actualize();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<Youtube_Video> GetVideos()
        {
            try
            {
                return _dbContext.Youtube_Video.Actualize();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<Youtube_Video> GetVideos(string channelId)
        {
            try
            {
                return _dbContext.Youtube_Video
                                 .Where(video => video.Youtube_VideoSnippet.ChannelId == channelId)
                                 .Actualize();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public Youtube_Video GetVideo(string videoId)
        {
            try
            {
                return _dbContext.Youtube_Video
                                 .Where(video => video.Id == videoId)
                                 .First();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public Youtube_Channel GetChannel(string channelId)
        {
            try
            {
                return _dbContext.Youtube_Channel
                                 .Where(x => x.Id == channelId)
                                 .First();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<Youtube_CommentThread> GetCommentThreads(string videoId)
        {
            try
            {
                return _dbContext.Youtube_CommentThread
                                 .Where(x => x.Youtube_CommentThreadSnippet.VideoId == videoId)
                                 .Actualize();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool HasChannel(string channelId)
        {
            try
            {
                return _dbContext.Youtube_Channel
                                 .Any(x => x.Id == channelId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool HasVideo(string videoId)
        {
            try
            {
                return _dbContext.Youtube_Video
                                 .Any(video => video.Id == videoId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region IYoutubeService Methods

        /// <summary>
        /// Search will return search result "snippet(s)"; and apply the available search filters for the
        /// specified entity type.
        /// </summary>
        public IEnumerable<Youtube_SearchResult> BasicSearch(YoutubeBasicSearchRequest request)
        {
            try
            {
                var response = _youtubeService.Search(request);

                // Create separate list to return, for DB context disposal reasons
                var entityList = new List<Youtube_SearchResult>();

                // Add Entities to database
                foreach (var result in response.Collection)
                {
                    // Create Entity from Response
                    var entity = _dbContext.Youtube_SearchResult.CreateObject();
                    var thumbnailDetails = _dbContext.Youtube_ThumbnailDetails.CreateObject();

                    Map_Youtube_SearchResult(result, entity, thumbnailDetails);

                    // Add Entity to DB context
                    _dbContext.Youtube_ThumbnailDetails.AddObject(thumbnailDetails);
                    _dbContext.Youtube_SearchResult.AddObject(entity);

                    // Use other list for return value
                    entityList.Add(entity);
                }

                // Commit changes
                _dbContext.SaveChanges();

                return entityList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Executes search for channel details, and updates local database
        /// </summary>
        public Youtube_Channel SearchUpdateChannelDetails(YoutubeChannelDetailsRequest request)
        {
            try
            {
                // Query Youtube
                var response = _youtubeService.GetChannelDetails(request);

                // Should be just a single channel in response
                var channel = response.Collection.FirstOrDefault();

                if (channel == null)
                    return null;

                // Prune response (topic ids, topic categories)
                if (channel.TopicDetails != null && channel.TopicDetails.TopicIds != null)
                    channel.TopicDetails.TopicIds.Remove(x => string.IsNullOrEmpty(x));

                if (channel.TopicDetails != null && channel.TopicDetails.TopicCategories != null)
                    channel.TopicDetails.TopicCategories.Remove(x => string.IsNullOrEmpty(x));

                // Check DB for existing entry
                var entity = _dbContext.Youtube_Channel.FirstOrDefault(channelEntity => channelEntity.Id == channel.Id);

                Youtube_ChannelSnippet snippet;
                Youtube_ChannelAuditDetails auditDetails;
                Youtube_ChannelBrandingSettings brandingSettings;
                Youtube_ChannelSettings channelSettings;
                Youtube_ThumbnailDetails thumbnailDetails;
                //IList<Youtube_ChannelConversationPing> conversationPings;
                IList<Youtube_TopicId> topicIds;
                IList<Youtube_TopicCategory> topicCategories;

                // Locate Entity Data
                if (entity == null)
                {
                    entity = _dbContext.Youtube_Channel.CreateObject();
                    auditDetails = _dbContext.Youtube_ChannelAuditDetails.CreateObject();
                    brandingSettings = _dbContext.Youtube_ChannelBrandingSettings.CreateObject();
                    channelSettings = _dbContext.Youtube_ChannelSettings.CreateObject();
                    snippet = _dbContext.Youtube_ChannelSnippet.CreateObject();
                    thumbnailDetails = _dbContext.Youtube_ThumbnailDetails.CreateObject();

                    //conversationPings = channel.ConversionPings
                    //                           .Pings
                    //                           .Select(ping => _dbContext.Youtube_ChannelConversationPing.CreateObject())
                    //                           .ToList();

                    // Youtube's API produces null references for missing data!
                    topicIds = channel.TopicDetails?
                                      .TopicIds?
                                      .Select(x => _dbContext.Youtube_TopicId.CreateObject())?
                                      .ToList() ?? new List<Youtube_TopicId>();

                    // Youtube's API produces null references for missing data!
                    topicCategories = channel.TopicDetails?
                                             .TopicCategories?
                                             .Select(x => _dbContext.Youtube_TopicCategory.CreateObject())?
                                             .ToList() ?? new List<Youtube_TopicCategory>();

                    // Add / Update
                    Map_Youtube_Channel(channel, entity, snippet, auditDetails, brandingSettings, channelSettings, thumbnailDetails);

                    // New (topic ids, topic categories)
                    for (int index = 0; index < (channel.TopicDetails?.TopicIds?.Count ?? 0); index++)
                    {
                        var topicId = topicIds[index];

                        topicId.ChannelId = channel.Id;
                        topicId.Url = channel.TopicDetails.TopicIds[index];
                        topicId.Relevant = false;
                        topicId.VideoId = topicId.VideoId ?? null;
                    }

                    for (int index = 0; index < (channel.TopicDetails?.TopicCategories?.Count ?? 0); index++)
                    {
                        var topicCategory = topicCategories[index];

                        topicCategory.ChannelId = channel.Id;
                        topicCategory.Url = channel.TopicDetails.TopicCategories[index];
                        topicCategory.VideoId = topicCategory.VideoId ?? null;
                    }

                    _dbContext.Youtube_Channel.AddObject(entity);
                    _dbContext.Youtube_ChannelSnippet.AddObject(snippet);
                    _dbContext.Youtube_ChannelAuditDetails.AddObject(auditDetails);
                    _dbContext.Youtube_ChannelBrandingSettings.AddObject(brandingSettings);
                    _dbContext.Youtube_ChannelSettings.AddObject(channelSettings);
                    _dbContext.Youtube_ThumbnailDetails.AddObject(thumbnailDetails);

                    foreach (var topicId in topicIds)
                    {
                        if (!_dbContext.Youtube_TopicId.Any(x => x.Url == topicId.Url))
                            _dbContext.Youtube_TopicId.AddObject(topicId);
                    }

                    foreach (var topicCategory in topicCategories)
                    {
                        if (!_dbContext.Youtube_TopicCategory.Any(x => x.Url == topicCategory.Url))
                            _dbContext.Youtube_TopicCategory.AddObject(topicCategory);
                    }
                }
                else
                {
                    snippet = entity.Youtube_ChannelSnippet;
                    auditDetails = entity.Youtube_ChannelAuditDetails;
                    brandingSettings = entity.Youtube_ChannelBrandingSettings;
                    channelSettings = entity.Youtube_ChannelBrandingSettings.Youtube_ChannelSettings;
                    thumbnailDetails = entity.Youtube_ChannelSnippet.Youtube_ThumbnailDetails;
                    //conversationPings = entity.Youtube_ChannelConversationPing.Where(ping => ping.Our_ChannelId == entity.Id).ToList();

                    // Delete and Re-Add TopicDetails collections
                    topicIds = _dbContext.Youtube_TopicId.Where(topicId => topicId.ChannelId == entity.Id).ToList();
                    topicCategories = _dbContext.Youtube_TopicCategory.Where(topicCategory => topicCategory.ChannelId == entity.Id).ToList();

                    foreach (var topicId in topicIds)
                        _dbContext.Youtube_TopicId.DeleteObject(topicId);

                    foreach (var topicCategory in topicCategories)
                        _dbContext.Youtube_TopicCategory.DeleteObject(topicCategory);

                    // Add / Update
                    Map_Youtube_Channel(channel, entity, snippet, auditDetails, brandingSettings, channelSettings, thumbnailDetails);

                    // Commit changes
                    _dbContext.SaveChanges();

                    if (channel.TopicDetails?.TopicIds != null)
                    {
                        foreach (var topicId in channel.TopicDetails?.TopicIds)
                            topicIds.Add(_dbContext.Youtube_TopicId.CreateObject());
                    }

                    if (channel.TopicDetails?.TopicCategories != null)
                    {
                        foreach (var topicCategory in channel.TopicDetails?.TopicCategories)
                            topicCategories.Add(_dbContext.Youtube_TopicCategory.CreateObject());
                    }

                    // Re-Add
                    for (int index = 0;index < channel.TopicDetails?.TopicIds?.Count; index++)
                    {
                        var topicId = topicIds[index];

                        topicId.ChannelId = topicId.ChannelId ?? channel.Id;
                        topicId.Relevant = false;
                        topicId.Url = channel.TopicDetails.TopicIds[index];
                        topicId.VideoId = topicId.VideoId ?? null;

                        _dbContext.Youtube_TopicId.AddObject(topicId);
                    }

                    for (int index = 0; index < channel.TopicDetails?.TopicCategories?.Count; index++)
                    {
                        var topicCategory = topicCategories[index];

                        topicCategory.ChannelId = topicCategory.ChannelId ?? channel.Id;
                        topicCategory.Url = channel.TopicDetails.TopicCategories[index];
                        topicCategory.VideoId = topicCategory.VideoId ?? null;

                        _dbContext.Youtube_TopicCategory.AddObject(topicCategory);
                    }
                }

                // Commit changes
                _dbContext.SaveChanges();

                return entity;
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
        public IEnumerable<Youtube_Video> SearchUpdateVideoDetails(YoutubeVideoDetailsRequest request)
        {
            try
            {
                // Query Youtube
                var response = _youtubeService.GetVideoDetails(request);

                var result = new List<Youtube_Video>();

                Youtube_Video videoEntity;
                Youtube_VideoSnippet snippet;
                Youtube_VideoStatistics statistics;
                Youtube_VideoStatus status;
                Youtube_ThumbnailDetails thumbnailDetails;
                IList<Youtube_TopicId> topicIds;
                IList<Youtube_TopicCategory> topicCategories;

                // Youtube Videos from service
                foreach (var video in response.Collection)
                {
                    // Prune response (topic ids, topic categories)
                    if (video.TopicDetails != null && video.TopicDetails.TopicIds != null)
                        video.TopicDetails.TopicIds.Remove(x => string.IsNullOrEmpty(x));

                    if (video.TopicDetails != null && video.TopicDetails.TopicCategories != null)
                        video.TopicDetails.TopicCategories.Remove(x => string.IsNullOrEmpty(x));

                    videoEntity = _dbContext.Youtube_Video.FirstOrDefault(x => x.Id == video.Id);

                    // New
                    if (videoEntity == null)
                    {
                        videoEntity = _dbContext.Youtube_Video.CreateObject();
                        snippet = _dbContext.Youtube_VideoSnippet.CreateObject();
                        statistics = _dbContext.Youtube_VideoStatistics.CreateObject();
                        status = _dbContext.Youtube_VideoStatus.CreateObject();
                        thumbnailDetails = _dbContext.Youtube_ThumbnailDetails.CreateObject();

                        // Youtube's API produces null references for missing data!
                        topicIds = video.TopicDetails?
                                          .TopicIds?
                                          .Select(x => _dbContext.Youtube_TopicId.CreateObject())?
                                          .ToList() ?? new List<Youtube_TopicId>();

                        // Youtube's API produces null references for missing data!
                        topicCategories = video.TopicDetails?
                                                 .TopicCategories?
                                                 .Select(x => _dbContext.Youtube_TopicCategory.CreateObject())?
                                                 .ToList() ?? new List<Youtube_TopicCategory>();

                        Map_Youtube_Video(video, videoEntity, snippet, statistics, status, thumbnailDetails);

                        // New (topic ids, topic categories)
                        for (int index = 0; index < video.TopicDetails.TopicIds?.Count; index++)
                        {
                            var topicId = topicIds[index];

                            topicId.ChannelId = topicId.ChannelId ?? video.Snippet.ChannelId;
                            topicId.Url = video.TopicDetails.TopicIds[index];
                            topicId.Relevant = video.TopicDetails.RelevantTopicIds?.Contains(topicId.Url) ?? false;
                            topicId.VideoId = topicId.VideoId ?? null;
                        }

                        for (int index = 0; index < video.TopicDetails.TopicCategories?.Count; index++)
                        {
                            var topicCategory = topicCategories[index];

                            topicCategory.ChannelId = topicCategory.ChannelId ?? video.Id;
                            topicCategory.Url = video.TopicDetails.TopicCategories[index];
                            topicCategory.VideoId = topicCategory.VideoId ?? null;
                        }

                        _dbContext.Youtube_Video.AddObject(videoEntity);
                        _dbContext.Youtube_VideoSnippet.AddObject(snippet);
                        _dbContext.Youtube_VideoStatistics.AddObject(statistics);
                        _dbContext.Youtube_VideoStatus.AddObject(status);
                        _dbContext.Youtube_ThumbnailDetails.AddObject(thumbnailDetails);

                        foreach (var topicId in topicIds)
                            _dbContext.Youtube_TopicId.AddObject(topicId);

                        foreach (var topicCategory in topicCategories)
                            _dbContext.Youtube_TopicCategory.AddObject(topicCategory);
                    }

                    // Existing
                    else
                    {
                        snippet = videoEntity.Youtube_VideoSnippet;
                        statistics = videoEntity.Youtube_VideoStatistics;
                        status = videoEntity.Youtube_VideoStatus;
                        thumbnailDetails = videoEntity.Youtube_VideoSnippet.Youtube_ThumbnailDetails;

                        // Delete and Re-Add TopicDetails collections
                        topicIds = _dbContext.Youtube_TopicId.Where(topicId => topicId.VideoId == videoEntity.Id).ToList();
                        topicCategories = _dbContext.Youtube_TopicCategory.Where(topicCategory => topicCategory.VideoId == videoEntity.Id).ToList();

                        foreach (var topicId in topicIds)
                            _dbContext.Youtube_TopicId.DeleteObject(topicId);

                        foreach (var topicCategory in topicCategories)
                            _dbContext.Youtube_TopicCategory.DeleteObject(topicCategory);

                        Map_Youtube_Video(video, videoEntity, snippet, statistics, status, thumbnailDetails);

                        // Commit changes (deletions)
                        _dbContext.SaveChanges();

                        topicIds.Clear();
                        topicCategories.Clear();

                        if (video.TopicDetails?.TopicIds != null)
                        {
                            foreach (var topicId in video.TopicDetails?.TopicIds)
                                topicIds.Add(_dbContext.Youtube_TopicId.CreateObject());
                        }

                        if (video.TopicDetails?.TopicCategories != null)
                        {
                            foreach (var topicCategory in video.TopicDetails?.TopicCategories)
                                topicCategories.Add(_dbContext.Youtube_TopicCategory.CreateObject());
                        }

                        // Re-Add
                        for (int index = 0; index < video.TopicDetails.TopicIds?.Count; index++)
                        {
                            var topicId = topicIds[index];

                            topicId.ChannelId = topicId.ChannelId ?? video.Snippet.ChannelId;
                            topicId.Url = video.TopicDetails.TopicIds[index];
                            topicId.Relevant = video.TopicDetails.RelevantTopicIds?.Contains(topicId.Url) ?? false;
                            topicId.VideoId = topicId.VideoId ?? null;

                            _dbContext.Youtube_TopicId.AddObject(topicId);
                        }

                        for (int index = 0; index < video.TopicDetails.TopicCategories?.Count; index++)
                        {
                            var topicCategory = topicCategories[index];

                            topicCategory.ChannelId = topicCategory.ChannelId ?? video.Id;
                            topicCategory.Url = video.TopicDetails.TopicCategories[index];
                            topicCategory.VideoId = topicCategory.VideoId ?? null;

                            _dbContext.Youtube_TopicCategory.AddObject(topicCategory);
                        }
                    }

                    // Result collection
                    result.Add(videoEntity);
                }

                // Commit changes
                _dbContext.SaveChanges();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<Youtube_Playlist> SearchUpdatePlaylistDetails(YoutubePlaylistRequest request)
        {
            try
            {
                // Playlist
                var response = _youtubeService.GetPlaylists(request);

                var result = new List<Youtube_Playlist>();

                Youtube_Playlist entity;
                Youtube_Channel channel;
                Youtube_ThumbnailDetails thumbnailDetails;

                foreach (var playlist in response.Collection)
                {
                    entity = _dbContext.Youtube_Playlist.FirstOrDefault(x => x.Id == playlist.Id);
                    channel = _dbContext.Youtube_Channel.FirstOrDefault(x => x.Id == playlist.Snippet.ChannelId);

                    // Channel must be available
                    if (channel == null)
                        throw new FormattedException("No local channel for playlist/channel query:  {0} / {1}", playlist.Id, playlist.Snippet.ChannelId);

                    // New
                    if (entity == null)
                    {
                        entity = _dbContext.Youtube_Playlist.CreateObject();
                        thumbnailDetails = _dbContext.Youtube_ThumbnailDetails.CreateObject();

                        Map_Youtube_Playlist(playlist, entity, channel, thumbnailDetails);

                        _dbContext.Youtube_Playlist.AddObject(entity);
                        _dbContext.Youtube_ThumbnailDetails.AddObject(thumbnailDetails);
                    }

                    // Existing
                    else
                    {
                        thumbnailDetails = _dbContext.Youtube_ThumbnailDetails
                                                     .First(x => x.Our_Id == entity.PlaylistSnippet_ThumnailDetails_Id);

                        Map_Youtube_Playlist(playlist, entity, channel, thumbnailDetails);
                    }

                    result.Add(entity);
                }

                // Commit changes
                _dbContext.SaveChanges();

                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<Youtube_PlaylistItem> SearchUpdatePlaylistItemDetails(YoutubePlaylistItemRequest request)
        {
            try
            {
                // Query for existing playlist
                var playlistEntity = _dbContext.Youtube_Playlist.FirstOrDefault(x => x.Id == request.PlaylistId);

                if (playlistEntity == null)
                    throw new FormattedException("No playlist found for playlist items request:  {0}", request.PlaylistId);

                // Youtube Playlist Items Query
                var response = _youtubeService.GetPlaylistItems(request);

                var result = new List<Youtube_PlaylistItem>();

                Youtube_PlaylistItem entity;
                Youtube_Channel channel;
                Youtube_Channel ownerChannel;
                Youtube_ThumbnailDetails thumbnailDetails;

                foreach (var playlistItem in response.Collection)
                {
                    entity = _dbContext.Youtube_PlaylistItem.FirstOrDefault(x => x.Id == playlistItem.Id);
                    channel = _dbContext.Youtube_Channel.FirstOrDefault(x => x.Id == playlistItem.Snippet.ChannelId);
                    ownerChannel = _dbContext.Youtube_Channel.FirstOrDefault(x => x.Id == playlistItem.Snippet.VideoOwnerChannelId);

                    // Channel / Owner Channel must be available
                    if (ownerChannel == null)
                        throw new FormattedException("No local owner channel for playlist item/channel query:  {0} / {1}",
                                                      playlistItem.Id, playlistItem.Snippet.VideoOwnerChannelId);

                    if (channel == null)
                        throw new FormattedException("No local channel for playlist item/channel query:  {0} / {1}",
                                                      playlistItem.Id, playlistItem.Snippet.ChannelId);

                    // New
                    if (entity == null)
                    {
                        entity = _dbContext.Youtube_PlaylistItem.CreateObject();
                        thumbnailDetails = _dbContext.Youtube_ThumbnailDetails.CreateObject();

                        Map_Youtube_PlaylistItem(playlistItem, entity, playlistEntity, channel, ownerChannel, thumbnailDetails);

                        _dbContext.Youtube_PlaylistItem.AddObject(entity);
                        _dbContext.Youtube_ThumbnailDetails.AddObject(thumbnailDetails);
                    }

                    // Existing
                    else
                    {
                        thumbnailDetails = _dbContext.Youtube_ThumbnailDetails
                                                     .First(x => x.Our_Id == entity.PlaylistItemSnippet_ThumbnailDetails_Id);

                        Map_Youtube_PlaylistItem(playlistItem, entity, playlistEntity, channel, ownerChannel, thumbnailDetails);
                    }

                    result.Add(entity);
                }

                // Commit changes
                _dbContext.SaveChanges();

                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets comment threads for:  1) An entire channel, or 2) A set of video (ids)
        /// </summary>
        public IEnumerable<Youtube_CommentThread> SearchCommentThreads(YoutubeCommentThreadRequest request)
        {
            try
            {
                // Procedure
                //
                // 1) Add comment replies
                // 2) Add comment thread
                // 3) Add comment maps
                // 4) Delete unused maps
                // 5) Delete unused comments
                // 6) Delete unused threads
                //

                var response = _youtubeService.GetCommentThreads(request);

                // Thread
                Youtube_CommentThread entity;
                Youtube_CommentThreadSnippet snippet;
                Youtube_Channel channel;
                Youtube_Video video;
                Youtube_Comment comment;
                Youtube_CommentSnippet commentSnippet;

                // Reply
                Youtube_Comment reply;
                Youtube_CommentSnippet replySnippet;

                // Map
                Youtube_CommentListMap replyMap;

                // Comment Replies (from all threads)
                foreach (var threadReply in response.Collection
                                                    .Where(x => x.Replies != null)
                                                    .SelectMany(x => x.Replies.Comments))
                {
                    reply = _dbContext.Youtube_Comment.FirstOrDefault(x => x.Id == threadReply.Id);

                    // New
                    if (reply == null)
                    {
                        reply = _dbContext.Youtube_Comment.CreateObject();
                        replySnippet = _dbContext.Youtube_CommentSnippet.CreateObject();

                        _dbContext.Youtube_CommentSnippet.AddObject(replySnippet);
                        _dbContext.Youtube_Comment.AddObject(reply);
                    }

                    // Existing
                    else
                    {
                        replySnippet = reply.Youtube_CommentSnippet;
                    }

                    Map_Youtube_Comment(threadReply, reply, replySnippet);
                }

                // Comment Threads
                foreach (var commentThread in response.Collection)
                {
                    entity = _dbContext.Youtube_CommentThread.FirstOrDefault(x => x.Id == commentThread.Id);
                    channel = _dbContext.Youtube_Channel.FirstOrDefault(x => x.Id == commentThread.Snippet.ChannelId);
                    video = _dbContext.Youtube_Video.FirstOrDefault(x => x.Id == commentThread.Snippet.VideoId);

                    if (channel == null || video == null)
                        throw new FormattedException("No video, or channel, found for comment thread {0}", commentThread.Id);

                    // New
                    if (entity == null)
                    {
                        entity = _dbContext.Youtube_CommentThread.CreateObject();
                        snippet = _dbContext.Youtube_CommentThreadSnippet.CreateObject();
                        comment = _dbContext.Youtube_Comment.CreateObject();
                        commentSnippet = _dbContext.Youtube_CommentSnippet.CreateObject();

                        _dbContext.Youtube_CommentSnippet.AddObject(commentSnippet);
                        _dbContext.Youtube_Comment.AddObject(comment);
                        _dbContext.Youtube_CommentThreadSnippet.AddObject(snippet);
                        _dbContext.Youtube_CommentThread.AddObject(entity);
                    }
                    // Existing
                    else
                    {
                        snippet = entity.Youtube_CommentThreadSnippet;
                        comment = entity.Youtube_CommentThreadSnippet.Youtube_Comment;
                        commentSnippet = comment.Youtube_CommentSnippet;
                    }

                    Map_Youtube_CommentThread(commentThread, channel, video, entity, snippet, comment, commentSnippet);
                }

                // Commit all changes!
                _dbContext.SaveChanges();

                // Comment Maps
                foreach (var thread in response.Collection.Where(x => x.Replies != null && x.Replies.Comments != null))
                {
                    foreach (var userReply in thread.Replies.Comments)
                    {
                        replyMap = _dbContext.Youtube_CommentListMap
                                            .FirstOrDefault(x => x.CommentId == userReply.Id && x.CommentThreadId == thread.Id);

                        // New
                        if (replyMap == null)
                        {
                            replyMap = _dbContext.Youtube_CommentListMap.CreateObject();

                            // Just added / updated
                            reply = _dbContext.Youtube_Comment.First(x => x.Id == userReply.Id);
                            replySnippet = reply.Youtube_CommentSnippet;

                            // Foreign Key
                            replyMap.Youtube_Comment = reply;
                            replyMap.Youtube_CommentThread = _dbContext.Youtube_CommentThread.First(x => x.Id == thread.Id);

                            _dbContext.Youtube_CommentListMap.AddObject(replyMap);
                        }

                        // Existing
                        else
                        {
                            // Nothing to do
                        }
                    }
                }

                // Commit all changes!
                _dbContext.SaveChanges();

                // Delete unused maps
                IEnumerable<Youtube_CommentListMap> potentialUnusedMaps;

                // Video Search (Could be across multiple channels)
                if (!string.IsNullOrWhiteSpace(request.VideoId))
                {
                    potentialUnusedMaps = _dbContext.Youtube_CommentListMap
                                                    .Where(x => request.VideoId == x.Youtube_CommentThread.Youtube_CommentThreadSnippet.VideoId)
                                                    .Actualize();
                }

                else
                    throw new Exception("Unknown error:  UnitOfWork.GetCommentThreads");

                foreach (var map in potentialUnusedMaps)
                {
                    // Extraneous Thread (unused if, for this channel / video search, no matching threads were returned)
                    if (!response.Collection.Any(x => x.Id == map.CommentThreadId))
                    {
                        var unusedThread = map.Youtube_CommentThread;
                        var unusedComments = _dbContext.Youtube_Comment.Where(x => map.Youtube_CommentThread.Id == map.CommentThreadId);

                        // Delete Comments (replies)
                        foreach (var unusedComment in unusedComments)
                        {
                            _dbContext.Youtube_Comment.DeleteObject(unusedComment);
                            _dbContext.Youtube_CommentSnippet.DeleteObject(unusedComment.Youtube_CommentSnippet);
                        }

                        // Delete Top Level Comment
                        _dbContext.Youtube_Comment.DeleteObject(unusedThread.Youtube_CommentThreadSnippet.Youtube_Comment);
                        _dbContext.Youtube_CommentThreadSnippet.DeleteObject(unusedThread.Youtube_CommentThreadSnippet);

                        // Delete Thread
                        _dbContext.Youtube_CommentThread.DeleteObject(unusedThread);

                        // Delete Map
                        _dbContext.Youtube_CommentListMap.DeleteObject(map);
                    }
                }

                // Commit all changes!
                _dbContext.SaveChanges();

                // Take simple string collection for EF serializer to handle
                var commentThreadIds = response.Collection.Select(x => x.Id).ToList();

                return _dbContext.Youtube_CommentThread
                                 .Where(x => commentThreadIds.Contains(x.Id))
                                 .Actualize();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Mapper Methods
        private void Map_Youtube_Channel(Channel channel, 
                                        Youtube_Channel entity, 
                                        Youtube_ChannelSnippet snippet,
                                        Youtube_ChannelAuditDetails auditDetails,
                                        Youtube_ChannelBrandingSettings brandingSettings,
                                        Youtube_ChannelSettings settings,
                                        Youtube_ThumbnailDetails thumbnailDetails)
        {
            // Primary fields
            entity.ETag = channel.ETag;
            entity.Id = channel.Id;
            entity.Kind = channel.Kind;

            // Channel Snippet, Thumbnail Details
            Map_Youtube_ChannelSnippet(channel.Snippet, snippet, thumbnailDetails);

            // Content Details
            entity.ChannelContentDetails_RelatedPlaylistsData_Favorites = channel.ContentDetails.RelatedPlaylists.Favorites;
            entity.ChannelContentDetails_RelatedPlaylistsData_Likes = channel.ContentDetails.RelatedPlaylists.Likes;
            entity.ChannelContentDetails_RelatedPlaylistsData_Uploads = channel.ContentDetails.RelatedPlaylists.Uploads;
            entity.ChannelContentDetails_RelatedPlaylistsData_WatchHistory = channel.ContentDetails.RelatedPlaylists.WatchHistory;
            entity.ChannelContentDetails_RelatedPlaylistsData_WatchLater = channel.ContentDetails.RelatedPlaylists.WatchLater;

            // Content Owner Details
            entity.ContentOwnerDetails_ContentOwner = channel.ContentOwnerDetails?.ContentOwner;
            entity.ContentOwnerDetails_ETag = channel.ContentOwnerDetails?.ETag;
            entity.ContentOwnerDetails_TimeLinked = channel.ContentOwnerDetails?.TimeLinked ?? DateTime.Now;
            entity.ContentOwnerDetails_TimeLinkedDateTimeOffset = channel.ContentOwnerDetails?.TimeLinkedDateTimeOffset;
            entity.ContentOwnerDetails_TimeLinkedRaw = channel.ContentOwnerDetails?.TimeLinkedRaw;

            // Statistics
            entity.Statistics_CommentCount = (long?)(channel.Statistics?.CommentCount ?? 0);
            entity.Statistics_ETag = channel.Statistics?.ETag;
            entity.Statistics_HiddenSubscriberCount = channel.Statistics?.HiddenSubscriberCount;
            entity.Statistics_SubscriberCount = (long?)(channel.Statistics?.SubscriberCount ?? 0);
            entity.Statistics_VideoCount = (long?)(channel.Statistics?.VideoCount ?? 0);
            entity.Statistics_ViewCount = (long?)(channel.Statistics?.ViewCount ?? 0);

            // Status
            entity.Status_ETag = channel.Status?.ETag;
            entity.Status_IsLinked = channel.Status?.IsLinked;
            entity.Status_LongUploadsStatus = channel.Status?.LongUploadsStatus;
            entity.Status_MadeForKids = channel.Status?.MadeForKids;
            entity.Status_PrivacyStatus = channel.Status?.PrivacyStatus;
            entity.Status_SelfDeclaredMadeForKids = channel.Status?.SelfDeclaredMadeForKids;

            // Audit Details
            auditDetails.CommunityGuidelinesGoodStanding = channel.AuditDetails?.CommunityGuidelinesGoodStanding;
            auditDetails.ContentIdClaimsGoodStanding = channel.AuditDetails?.ContentIdClaimsGoodStanding;
            auditDetails.CopyrightStrikesGoodStanding = channel.AuditDetails?.CopyrightStrikesGoodStanding;
            auditDetails.ETag = channel.AuditDetails?.ETag;

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

            // Branding Settings -> Channel Settings
            Map_Youtube_ChannelSettings(channel.BrandingSettings.Channel, settings);

            // Foreign Key:  Branding Settings -> Channel Settings
            brandingSettings.Youtube_ChannelSettings = settings;

            // Foreign Keys: Andit Details, Branding Settings, Snippet
            entity.Youtube_ChannelAuditDetails = auditDetails;
            entity.Youtube_ChannelBrandingSettings = brandingSettings;
            entity.Youtube_ChannelSnippet = snippet;
        }

        private void Map_Youtube_Video(Video video, 
                                      Youtube_Video entity,
                                      Youtube_VideoSnippet snippet,
                                      Youtube_VideoStatistics statistics,
                                      Youtube_VideoStatus status,
                                      Youtube_ThumbnailDetails thumbnailDetails)
        {
            entity.AgeGating_AlcoholContent = video.AgeGating?.AlcoholContent;
            entity.AgeGating_ETag = video.AgeGating?.ETag;
            entity.AgeGating_Restricted = video.AgeGating?.Restricted;
            entity.AgeGating_VideoGameRating = video.AgeGating?.VideoGameRating;
            entity.ContentDetails_Caption = video.ContentDetails.Caption;
            entity.ContentDetails_Definition = video.ContentDetails.Definition;
            entity.ContentDetails_Dimension = video.ContentDetails.Dimension;
            entity.ContentDetails_Duration = video.ContentDetails.Duration;
            entity.ContentDetails_ETag = video.ContentDetails.ETag;
            entity.ContentDetails_HasCustomThumbnail = video.ContentDetails.HasCustomThumbnail;
            entity.ContentDetails_LicensedContent = video.ContentDetails.LicensedContent;
            entity.ContentDetails_Projection = video.ContentDetails.Projection;
            entity.ContentDetails_RegionRestriction_ETag = video.ContentDetails.RegionRestriction?.ETag;
            entity.ETag = video.ETag;
            entity.Id = video.Id;
            entity.Kind = video.Kind;
            entity.MonetizationDetails_AccessPolicy_Allowed = video.MonetizationDetails?.Access?.Allowed;
            entity.TopicDetails_ETag = video.TopicDetails?.ETag;

            // Foreign Key Relationships
            Map_Youtube_VideoSnippet(video.Snippet, snippet, thumbnailDetails);

            statistics.CommentCount = (long?)video.Statistics.CommentCount;
            statistics.DislikeCount = (long?)video.Statistics.DislikeCount;
            statistics.FavoriteCount = (long)(video.Statistics.FavoriteCount ?? 0);
            statistics.LikeCount = (long)(video.Statistics.LikeCount ?? 0);
            statistics.ViewCount = (long)(video.Statistics.ViewCount ?? 0);
            statistics.ETag = video.ETag;

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

            // Foreign Keys (These may already have been set)
            entity.Youtube_VideoStatus = status;
            entity.Youtube_VideoStatistics = statistics;
            entity.Youtube_VideoSnippet = snippet;
        }

        private void Map_Youtube_ChannelSnippet(ChannelSnippet snippet, Youtube_ChannelSnippet entity, Youtube_ThumbnailDetails thumbnailDetails)
        {
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

            entity.Youtube_ThumbnailDetails = thumbnailDetails;

            Map_Youtube_ThumbnailDetails(snippet.Thumbnails, thumbnailDetails);
        }
        private void Map_Youtube_ChannelSettings(ChannelSettings settings, Youtube_ChannelSettings entity)
        {
            entity.Country = settings.Country;
            entity.DefaultLanguage = settings.DefaultLanguage;
            entity.DefaultTab = settings.DefaultTab;
            entity.Description = settings.Description;
            entity.ETag = settings.ETag;
            entity.FeaturedChannelsTitle = settings.FeaturedChannelsTitle;
            entity.FeaturedChannelsUrls = settings.FeaturedChannelsUrls != null ? String.Join("\n", settings.FeaturedChannelsUrls) : "";
            entity.Keywords = settings.Keywords;
            entity.ModerateComments = settings.ModerateComments;
            entity.ProfileColor = settings.ProfileColor;
            entity.ShowBrowseView = settings.ShowBrowseView;
            entity.ShowRelatedChannels = settings.ShowRelatedChannels;
            entity.Title = settings.Title;
            entity.TrackingAnalyticsAccountId = settings.TrackingAnalyticsAccountId;
            entity.UnsubscribedTrailer = settings.UnsubscribedTrailer;
        }
        private void Map_Youtube_VideoSnippet(VideoSnippet snippet, Youtube_VideoSnippet entity, Youtube_ThumbnailDetails thumbnailDetails)
        {
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

            // Foreign Key
            entity.Youtube_ThumbnailDetails = thumbnailDetails;

            Map_Youtube_ThumbnailDetails(snippet.Thumbnails, entity.Youtube_ThumbnailDetails);
        }
        private void Map_Youtube_SearchResult(SearchResult searchResult, Youtube_SearchResult entity, Youtube_ThumbnailDetails thumbnailDetails)
        {
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

            entity.Youtube_ThumbnailDetails = thumbnailDetails;
            
            Map_Youtube_ThumbnailDetails(searchResult.Snippet.Thumbnails, thumbnailDetails);
        }
        private void Map_Youtube_Playlist(Playlist playlist, Youtube_Playlist entity, Youtube_Channel channel, Youtube_ThumbnailDetails thumbnailDetails)
        {
            // NOTE:  Not mapping channel values here. The foreign key is for reference

            entity.ETag = playlist.ETag;
            entity.Id = playlist.Id;
            entity.Kind = playlist.Kind;
            entity.PlaylistSnippet_ChannelDescription = playlist.Snippet.Description;
            entity.PlaylistSnippet_ChannelTitle = playlist.Snippet.ChannelTitle;
            entity.PlaylistSnippet_PublishedAtDateTimeOffset = playlist.Snippet.PublishedAtDateTimeOffset;
            entity.PlaylistSnippet_Title = playlist.Snippet.Title;

            // Foreign Keys
            entity.Youtube_Channel = channel;
            entity.Youtube_ThumbnailDetails = thumbnailDetails;
            
            Map_Youtube_ThumbnailDetails(playlist.Snippet.Thumbnails, thumbnailDetails);
        }
        private void Map_Youtube_PlaylistItem(PlaylistItem playlistItem, 
                                              Youtube_PlaylistItem entity, 
                                              Youtube_Playlist playlistEntity, 
                                              Youtube_Channel channel, 
                                              Youtube_Channel ownerChannel, 
                                              Youtube_ThumbnailDetails thumbnailDetails)
        {
            entity.ETag = playlistItem.ETag;
            entity.Id = playlistItem.Id;
            entity.Kind= playlistItem.Kind;
            entity.PlaylistContentDetails_ETag = playlistItem.ContentDetails.ETag;
            entity.PlaylistContentDetails_Note = playlistItem.ContentDetails.Note;
            entity.PlaylistContentDetails_VideoId = playlistItem.ContentDetails.VideoId; // Nullable (Not required)
            entity.PlaylistContentDetails_VideoPublishedAtDateTimeOffset = playlistItem.ContentDetails.VideoPublishedAtDateTimeOffset;
            entity.PlaylistItemSnippet_ChannelId = playlistItem.Snippet.ChannelId;
            entity.PlaylistItemSnippet_ChannelTitle = playlistItem.Snippet.Title;
            entity.PlaylistItemSnippet_Description = playlistItem.Snippet.Description;
            entity.PlaylistItemSnippet_ETag = playlistItem.Snippet.ETag;
            entity.PlaylistItemSnippet_Position = playlistItem.Snippet.Position;
            entity.PlaylistItemSnippet_PublishedAtDateTimeOffset = playlistItem.Snippet.PublishedAtDateTimeOffset;
            entity.PlaylistItemSnippet_Title = playlistItem.Snippet.Title;
            entity.PlaylistItemSnippet_VideoOwnerChannelTitle = playlistItem.Snippet.Title;
            entity.PlaylistItemStatus_PrivacyStatus = playlistItem.Status.PrivacyStatus;

            Map_Youtube_ThumbnailDetails(playlistItem.Snippet.Thumbnails, thumbnailDetails);

            // Foreign Keys
            entity.Youtube_Playlist = playlistEntity;
            entity.Youtube_ThumbnailDetails = thumbnailDetails;
            entity.Youtube_Channel = channel;
            entity.Youtube_Channel1 = ownerChannel;
        }
        private void Map_Youtube_CommentThread(CommentThread commentThread, 
                                               Youtube_Channel channel,                  // Comment Thread
                                               Youtube_Video video,                      // Comment Thread
                                               Youtube_CommentThread entity,             
                                               Youtube_CommentThreadSnippet snippet,     // Comment Thread
                                               Youtube_Comment comment,                  // Comment Thread -> Comment
                                               Youtube_CommentSnippet commentSnippet)    // Comment Thread -> Comment -> Comment Snippet
        {
            entity.Id = commentThread.Id;
            entity.ETag = commentThread.ETag;
            entity.Kind = commentThread.Kind;

            snippet.ETag = commentThread.Snippet.ETag;
            snippet.IsPublic = commentThread.Snippet.IsPublic;
            snippet.TotalReplyCount = commentThread.Snippet.TotalReplyCount;

            // Top level comment -> snippet
            commentSnippet.AuthorChannelId_ETag = commentThread.Snippet.TopLevelComment.Snippet.AuthorChannelId.ETag;
            commentSnippet.AuthorChannelId_Value = commentThread.Snippet.TopLevelComment.Snippet.AuthorChannelId.Value;            
            commentSnippet.AuthorChannelUrl = commentThread.Snippet.TopLevelComment.Snippet.AuthorChannelUrl;
            commentSnippet.AuthorDisplayName = commentThread.Snippet.TopLevelComment.Snippet.AuthorDisplayName;
            commentSnippet.AuthorProfileImageUrl = commentThread.Snippet.TopLevelComment.Snippet.AuthorProfileImageUrl;
            commentSnippet.CanRate = commentThread.Snippet.TopLevelComment.Snippet.CanRate;
            commentSnippet.ChannelId = commentThread.Snippet.TopLevelComment.Snippet.ChannelId;
            commentSnippet.LikeCount = commentThread.Snippet.TopLevelComment.Snippet.LikeCount;
            commentSnippet.ModerationStatus = commentThread.Snippet.TopLevelComment.Snippet.ModerationStatus;
            commentSnippet.ParentId = commentThread.Snippet.TopLevelComment.Id;     // ParentId field wasn't set
            commentSnippet.PublishedAt = commentThread.Snippet.TopLevelComment.Snippet.PublishedAt;
            commentSnippet.PublishedAtDateTimeOffset = commentThread.Snippet.TopLevelComment.Snippet.PublishedAtDateTimeOffset;
            commentSnippet.PublishedAtRaw = commentThread.Snippet.TopLevelComment.Snippet.PublishedAtRaw;
            commentSnippet.TextDisplay = commentThread.Snippet.TopLevelComment.Snippet.TextDisplay;
            commentSnippet.TextOriginal = commentThread.Snippet.TopLevelComment.Snippet.TextOriginal;
            commentSnippet.UpdatedAt = commentThread.Snippet.TopLevelComment.Snippet.UpdatedAt;
            commentSnippet.UpdatedAtDateTimeOffset = commentThread.Snippet.TopLevelComment.Snippet.UpdatedAtDateTimeOffset;
            commentSnippet.UpdatedAtRaw = commentThread.Snippet.TopLevelComment.Snippet.UpdatedAtRaw;
            commentSnippet.VideoId = commentThread.Snippet.TopLevelComment.Snippet.VideoId;
            commentSnippet.ViewerRating = commentThread.Snippet.TopLevelComment.Snippet.ViewerRating;

            comment.Youtube_CommentSnippet = commentSnippet;
            comment.Youtube_CommentSnippet.ParentId = commentThread.Snippet.TopLevelComment.Id;

            snippet.Youtube_Channel = channel;
            snippet.Youtube_Video = video;
            snippet.Youtube_Comment = comment;

            // Foreign Key (Comment is mapped using a separate map entity)
            entity.Youtube_CommentThreadSnippet = snippet;

            // Foreign Key
            comment.Youtube_CommentSnippet = commentSnippet;
        }

        private void Map_Youtube_Comment(Comment comment, Youtube_Comment entity, Youtube_CommentSnippet snippet)
        {
            entity.ETag = comment.ETag;
            entity.Id = comment.Id;
            entity.Kind = comment.Kind;

            // Snippet
            snippet.AuthorChannelId_ETag = comment.Snippet.AuthorChannelId.ETag;
            snippet.AuthorChannelId_Value = comment.Snippet.AuthorChannelId.Value;
            snippet.AuthorChannelUrl = comment.Snippet.AuthorChannelUrl;
            snippet.AuthorDisplayName = comment.Snippet.AuthorDisplayName;
            snippet.AuthorProfileImageUrl = comment.Snippet.AuthorProfileImageUrl;
            snippet.CanRate = comment.Snippet.CanRate;
            snippet.ChannelId = comment.Snippet.ChannelId;
            snippet.ETag = comment.Snippet.ETag;
            snippet.LikeCount = comment.Snippet.LikeCount;
            snippet.ModerationStatus = comment.Snippet.ModerationStatus;
            snippet.ParentId = comment.Id;          // ParentId field isn't set
            //entitySnippet.ParentId = comment.Snippet.ParentId;                // Set via entity reference
            snippet.PublishedAt = comment.Snippet.PublishedAt;
            snippet.PublishedAtDateTimeOffset = comment.Snippet.PublishedAtDateTimeOffset;
            snippet.PublishedAtRaw = comment.Snippet.PublishedAtRaw;
            snippet.TextDisplay = comment.Snippet.TextDisplay;
            snippet.TextOriginal = comment.Snippet.TextOriginal;
            snippet.UpdatedAt = comment.Snippet.UpdatedAt;
            snippet.UpdatedAtDateTimeOffset = comment.Snippet.UpdatedAtDateTimeOffset;
            snippet.UpdatedAtRaw = comment.Snippet.UpdatedAtRaw;
            snippet.VideoId = comment.Snippet.VideoId;
            snippet.ViewerRating = comment.Snippet.ViewerRating;

            // Foreign Key
            entity.Youtube_CommentSnippet = snippet;
        }
        private void Map_Youtube_ThumbnailDetails(ThumbnailDetails thumbnailDetails, Youtube_ThumbnailDetails entity)
        {
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
        }
        #endregion


        #region "Repository Pattern" Methods

        /// <summary>
        /// Synchronizes collection to match source, item count and field details
        /// </summary>
        /// <typeparam name="T1">Source item type</typeparam>
        /// <typeparam name="T2">Destination item type</typeparam>
        /// <typeparam name="TKey">Key type</typeparam>
        private void SynchronizeCollection<T1, T2, TKey>(IList<T1> source, 
                                                         IList<T2> dest, 
                                                         Func<T1, TKey> sourceKeySelector,
                                                         Func<T2, TKey> destKeySelector,
                                                         Func<T2> destConstructor,
                                                         Action<T1, T2> fieldCopier)
        {
            if (source.Count == dest.Count)
                return;

            var maxCount = Math.Max(source.Count, dest.Count);

            // Synchronize Source <- Destination
            foreach (var sourceItem in source)
            {
                var destItem = dest.FirstOrDefault(item => destKeySelector(item).Equals(sourceKeySelector(sourceItem)));

                // Construct new item
                if (destItem == null)
                    destItem = destConstructor();

                // Copy fields
                fieldCopier(sourceItem, destItem);
            }

            // Remove extraneous destination items
            for (int index = dest.Count - 1; index >= 0; index--)
            {
                var sourceItem = source.FirstOrDefault(item => sourceKeySelector(item).Equals(destKeySelector(dest[index])));

                // No matching source item
                if (sourceItem == null)
                    dest.RemoveAt(index);
            }
        }
        #endregion

        public void Dispose()
        {
            if (!_disposed)
            {
                _dbContext.Connection.Close();
                _dbContext.Dispose();

                _youtubeService.Dispose();

                _disposed = true;
            }
        }
    }
}
