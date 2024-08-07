using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Google.Apis.Services;

using YoutubeJournalist.Core.WebAPI.Google.Apis.Youtube.V3;

namespace YoutubeJournalist.Core.WebAPI
{
    public interface IWebAPI : IDisposable
    {
        /// <summary>
        /// Primary service connection to the API
        /// </summary>
        IClientService ServiceBase { get; }

        /// <summary>
        /// Runs Youtube search for channel entities
        /// </summary>
        /// <exception cref="Exception">Service error</exception>
        YoutubeServiceSearchChannelsData SearchChannels(string pageToken);
    }
}
