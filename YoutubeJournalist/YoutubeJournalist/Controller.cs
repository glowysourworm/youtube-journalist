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

        public Controller(Configuration configuration)
        {
            // Service connection
            _youtubeService = new YoutubeService(configuration.ApiKey, 
                                                 configuration.ClientID, 
                                                 configuration.ClientSecret, 
                                                 configuration.MaxSearchResults);

            // EF Database Connection - FIX DEFAULT CONNECTION
            _dbContext = new YoutubeJournalistEntities(GetConnectionString());

            _disposed = false;
        }

        private string GetConnectionString()
        {
            // FIX!! MS HAD TO BE A PROBLEM:  App.config file being used for EF6 inside YoutubeJournalist.Core project
            return "metadata = res://*/YoutubeJournalistEntityModel.csdl|res://*/YoutubeJournalistEntityModel.ssdl|res://*/YoutubeJournalistEntityModel.msl;provider=System.Data.SqlClient;provider connection string='data source=LAPTOP-JG4V86VG\\LOCALDB;initial catalog=YoutubeJournalist;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework'";
        }

        public IEnumerable<SearchResultViewModel> GetChannels(string searchString)
        {
            try
            {
                // Query Youtube
                var viewModels = _youtubeService.SearchChannels(searchString).Collection.Select(result =>
                {

                    // Storey query results into local database
                    // TODO: Check for Add-Or-Update
                    if (result.Youtube_Thumbnail != null)
                        _dbContext.Youtube_Thumbnail.AddObject(result.Youtube_Thumbnail);

                    if (result.Youtube_Thumbnail1 != null)
                        _dbContext.Youtube_Thumbnail.AddObject(result.Youtube_Thumbnail1);

                    if (result.Youtube_Thumbnail2 != null)
                        _dbContext.Youtube_Thumbnail.AddObject(result.Youtube_Thumbnail2);

                    if (result.Youtube_Thumbnail3 != null)
                        _dbContext.Youtube_Thumbnail.AddObject(result.Youtube_Thumbnail3);

                    if (result.Youtube_Thumbnail4 != null)
                        _dbContext.Youtube_Thumbnail.AddObject(result.Youtube_Thumbnail4);

                    // Commit changes
                    // _dbContext.SaveChanges();

                    // AUTO_INCREMENT, or IDENTITY(1,1), try doing this using Attach method
                    //

                    // Set primary key
                    //result.Our_Id = nextId;

                    _dbContext.Youtube_SearchResult.AddObject(result);

                    // Commit changes
                    _dbContext.SaveChanges();

                    // Return view model for query results
                    return new SearchResultViewModel()
                    {
                        Created = result.Snippet_PublishedAt ?? DateTime.MinValue,
                        Updated = result.Snippet_PublishedAt ?? DateTime.MinValue,
                        Description = result.Snippet_Description ?? "No Descr.",
                        ETag = result.Snippet_ETag ?? "NULL ETag",
                        Id = result.Id_ChannelId ?? "NULL ID",
                        Thumbnail = result.Snippet_ThumbnailDetails_Default__Url,
                        Title = result.Snippet_Title ?? "No Title"
                    };

                }).Actualize();

                return viewModels;
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
