using System;
using System.Linq;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Threading.Tasks;

using Microsoft.SqlServer;

using YoutubeJournalist.Core.WebAPI;
using YoutubeJournalist.Core.WebAPI.Google.Apis.Youtube.V3;

namespace YoutubeJournalist
{
    /// <summary>
    /// Primary component for querying and storing data from social media web services
    /// </summary>
    public class Controller : IDisposable
    {
        readonly YoutubeService _youtubeService;
        readonly DbContext _dbContext;

        bool _disposed;

        public Controller(Configuration configuration)
        {
            // Service connection
            _youtubeService = new YoutubeService(configuration.ApiKey, 
                                                 configuration.ClientID, 
                                                 configuration.ClientSecret, 
                                                 configuration.MaxSearchResults);

            // EF Database Connection - FIX DEFAULT CONNECTION
            //var connectionString = ConfigurationManager.ConnectionStrings["YoutubeJournalistEntities"].ConnectionString;
            var connectionString = ConfigurationManager.ConnectionStrings[0]?.ConnectionString ?? null;

            _dbContext = new DbContext(new SqlConnection(connectionString), true);

            _disposed = false;
        }

        public YoutubeServiceSearchChannelsData GetChannels(string pageToken = null)
        {
            try
            {
                return _youtubeService.SearchChannels(pageToken);
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
