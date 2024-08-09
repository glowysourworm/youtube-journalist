using System;
using System.Linq;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Threading.Tasks;

using Microsoft.SqlServer;

using YoutubeJournalist.Core.WebAPI;
using YoutubeJournalist.Core.WebAPI.Google.Apis.Youtube.V3;
using YoutubeJournalist.ViewModel;
using System.Collections.Generic;
using YoutubeJournalist.Core;
using WpfCustomUtilities.Extensions.Collection;
using Google.Apis.YouTube.v3.Data;

namespace YoutubeJournalist
{
    /// <summary>
    /// Primary component for querying and storing data from social media web services
    /// </summary>
    public class Controller : IDisposable
    {
        readonly YoutubeService _youtubeService;
        readonly YoutubeJournalistEntities _dbContext;

        bool _disposed;

        public Controller(ConfigurationViewModel configuration)
        {
            // Service connection
            _youtubeService = new YoutubeService(configuration.ApiKey, 
                                                 configuration.ClientID, 
                                                 configuration.ClientSecret);

            // EF Database Connection - FIX DEFAULT CONNECTION
            _dbContext = new YoutubeJournalistEntities(GetConnectionString());

            _disposed = false;
        }

        private string GetConnectionString()
        {
            // FIX!! MS HAD TO BE A PROBLEM:  App.config file being used for EF6 inside YoutubeJournalist.Core project
            return "metadata = res://*/YoutubeJournalistEntityModel.csdl|res://*/YoutubeJournalistEntityModel.ssdl|res://*/YoutubeJournalistEntityModel.msl;provider=System.Data.SqlClient;provider connection string='data source=LAPTOP-JG4V86VG\\LOCALDB;initial catalog=YoutubeJournalist;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework'";
        }

        /// <summary>
        /// Search will return search result "snippet(s)"; and apply the available search filters for the
        /// specified entity type.
        /// </summary>
        public IEnumerable<SearchResultViewModel> Search(YoutubeServiceRequest request)
        {
            try
            {
                // Query Youtube
                var viewModels = _youtubeService.Search(request).Collection.Select(result =>
                {
                    // Add / Update -> SaveChanges()
                    AddUpdateSearchResult(result);

                    // Create new view model
                    return CreateSearchViewModel(result);

                }).Actualize();

                return viewModels;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Returns local database search results, from past searches.
        /// </summary>
        public IEnumerable<SearchResultViewModel> GetSearchResults(YoutubeServiceRequest request)
        {
            try
            {
                // Query Youtube
                var viewModels = _dbContext.Youtube_SearchResult
                                           .Actualize()             // Loading entire set at once!
                                           .Select(result =>
                {
                    // Create new view model
                    return CreateSearchViewModel(result);

                }).Actualize();

                return viewModels;
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
        public IEnumerable<SearchResultViewModel> GetVideos(YoutubeServiceRequest request)
        {
            try
            {
                // Query Youtube
                var queryResult = _youtubeService.GetVideos(request);

                var viewModels = queryResult.Collection.Select(result =>
                {
                    // Add / Update -> SaveChanges()
                    AddUpdateVideo(result);

                    // Create new view model
                    return CreateSearchViewModel(result);

                }).Actualize();

                // Loose Collections
                foreach (var topicId in queryResult.LooseCollection1)
                    _dbContext.Youtube_TopicId.AddObject(topicId);

                foreach (var category in queryResult.LooseCollection2)
                    _dbContext.Youtube_TopicCategory.AddObject(category);

                _dbContext.SaveChanges();

                return viewModels;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<SearchResultViewModel> GetChannels(YoutubeServiceRequest request)
        {
            try
            {
                // Query Youtube
                var queryResult = _youtubeService.GetChannels(request);

                var viewModels = queryResult.Collection.Select(result =>
                {
                    // Add / Update -> SaveChanges()
                    AddUpdateChannel(result);

                    // Create new view model
                    return CreateSearchViewModel(result);

                }).Actualize();

                // Loose Collections
                foreach (var topicId in queryResult.LooseCollection1)
                    _dbContext.Youtube_TopicId.AddObject(topicId);

                foreach (var category in queryResult.LooseCollection2)
                    _dbContext.Youtube_TopicCategory.AddObject(category);

                _dbContext.SaveChanges();

                return viewModels;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private SearchResultViewModel CreateSearchViewModel(Youtube_SearchResult entity)
        {
            // Return view model for query results
            return new SearchResultViewModel()
            {
                Created = entity.Snippet_PublishedAt ?? DateTime.MinValue,
                Updated = entity.Snippet_PublishedAt ?? DateTime.MinValue,
                Description = entity.Snippet_Description,
                ETag = entity.Snippet_ETag ?? "NULL ETag",
                Id = entity.Id_ChannelId ?? "NULL ID",
                Thumbnail = entity.Youtube_ThumbnailDetails.Default__Url,
                Title = entity.Snippet_Title ?? "No Title",
                IsChannel = entity.Id_ChannelId != null
            };
        }
        private SearchResultViewModel CreateSearchViewModel(Youtube_Video entity)
        {
            // Return view model for query results
            return new SearchResultViewModel()
            {
                Created = entity.Youtube_VideoStatus.PublishAt ?? DateTime.MinValue,
                Updated = entity.Youtube_VideoSnippet.PublishedAt ?? DateTime.MinValue,
                Description = entity.Youtube_VideoSnippet.Description ?? "No Descr.",
                ETag = entity.ETag ?? "NULL ETag",
                Id = entity.Id ?? "NULL ID",
                Thumbnail = entity.Youtube_VideoSnippet.Youtube_ThumbnailDetails.Default__Url,
                Title = entity.Youtube_VideoSnippet.Title ?? "No Title",
                IsChannel = false
            };
        }
        private SearchResultViewModel CreateSearchViewModel(Youtube_Channel entity)
        {
            // Return view model for query results
            return new SearchResultViewModel()
            {
                Created = entity.ContentOwnerDetails_TimeLinked ?? DateTime.MinValue,
                //Updated = entity.ContentOwnerDetails_TimeLinkedDateTimeOffset.DateTime ?? DateTime.MinValue,
                //Description = entity.Description ?? "No Descr.",
                ETag = entity.ETag ?? "NULL ETag",
                Id = entity.Id ?? "NULL ID",
                Thumbnail = entity.Youtube_ChannelSnippet.Youtube_ThumbnailDetails.Default__Url,
                Title = entity.Youtube_ChannelSnippet.Title ?? "No Title",
                IsChannel = false
            };
        }

        private void AddUpdateVideo(Youtube_Video entity)
        {
            // Storey query results into local database
            // TODO: Check for Add-Or-Update
            _dbContext.Youtube_Video.AddObject(entity);

            // Foreign Key Relationships
            //
            // ThumbnailDetails
            if (entity.Youtube_VideoSnippet?.Youtube_ThumbnailDetails != null)
                _dbContext.Youtube_ThumbnailDetails.AddObject(entity.Youtube_VideoSnippet?.Youtube_ThumbnailDetails);

            // VideoSnippet
            if (entity.Youtube_VideoSnippet != null)
                _dbContext.Youtube_VideoSnippet.AddObject(entity.Youtube_VideoSnippet);

            // VideoStatus
            if (entity.Youtube_VideoStatus != null)
                _dbContext.Youtube_VideoStatus.AddObject(entity.Youtube_VideoStatus);

            // VideoStatistics
            if (entity.Youtube_VideoStatistics != null)
                _dbContext.Youtube_VideoStatistics.AddObject(entity.Youtube_VideoStatistics);

            // Commit changes
            _dbContext.SaveChanges();
        }
        private void AddUpdateChannel(Youtube_Channel entity)
        {
            // Storey query results into local database
            // TODO: Check for Add-Or-Update
            _dbContext.Youtube_Channel.AddObject(entity);

            // Foreign Key Relationships
            //

            // ChannelSnippet
            if (entity.Youtube_ChannelSnippet != null)
            {
                _dbContext.Youtube_ChannelSnippet.AddObject(entity.Youtube_ChannelSnippet);

                // ThumbnailDetails
                if (entity.Youtube_ChannelSnippet?.Youtube_ThumbnailDetails != null)
                    _dbContext.Youtube_ThumbnailDetails.AddObject(entity.Youtube_ChannelSnippet.Youtube_ThumbnailDetails);
            }

            // ChannelAuditDetails
            if (entity.Youtube_ChannelAuditDetails != null)
                _dbContext.Youtube_ChannelAuditDetails.AddObject(entity.Youtube_ChannelAuditDetails);

            // ChannelSettings
            if (entity.Youtube_ChannelBrandingSettings.Youtube_ChannelSettings != null)
                _dbContext.Youtube_ChannelSettings.AddObject(entity.Youtube_ChannelBrandingSettings.Youtube_ChannelSettings);

            // ChannelBrandingSettings
            if (entity.Youtube_ChannelBrandingSettings != null)
                _dbContext.Youtube_ChannelBrandingSettings.AddObject(entity.Youtube_ChannelBrandingSettings);

            //// ChannelStatistics
            //if (entity.channel != null)
            //    _dbContext.Youtube_VideoStatistics.AddObject(entity.Youtube_VideoStatistics);

            // Commit changes
            _dbContext.SaveChanges();
        }
        private void AddUpdateSearchResult(Youtube_SearchResult entity)
        {
            // Storey query results into local database
            // TODO: Check for Add-Or-Update
            _dbContext.Youtube_ThumbnailDetails.AddObject(entity.Youtube_ThumbnailDetails);

            _dbContext.Youtube_SearchResult.AddObject(entity);

            // Commit changes
            _dbContext.SaveChanges();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _youtubeService.Dispose();
                _dbContext.Dispose();

                _disposed = true;
            }
        }
    }
}
