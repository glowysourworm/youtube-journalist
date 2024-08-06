using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Google.Apis.Services;

using YoutubeJournalist.Core.Data;

namespace YoutubeJournalist.Core.WebAPI.Base
{
    public interface IWebAPIService
    {
        /// <summary>
        /// Primary service connection to the API
        /// </summary>
        IClientService ServiceBase { get; }

        /// <summary>
        /// Runs search service using specified stringS
        /// </summary>
        /// <exception cref="Exception">Service error</exception>
        Task<List<SearchResult>> Search(string searchString);
    }
}
