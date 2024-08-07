
using System.Collections.Generic;
using System;

using Google.Apis.Services;
using Google.Apis.YouTube.v3;

using YoutubeJournalist.Core.WebAPI.Base;
using YoutubeJournalist.Core.Data;
using System.Threading.Tasks;

namespace YoutubeJournalist.Core.WebAPI.Google.Apis.Youtube.V3
{
    public class Service : IWebAPIService
    {
        public IClientService ServiceBase { get; private set; }

        int _maxResults;

        public Service(string apiKey, int maxResults)
        {
            _maxResults = maxResults;

            this.ServiceBase = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = "Search"
            });
        }

        public async Task<List<SearchResult>> Search(string searchString)
        {
            // Search for list of channels (search string "repeatable", by tokens)
            var request = (this.ServiceBase as YouTubeService).Search.List("snippet");

            // Search configuration
            request.Q = searchString;
            request.MaxResults = _maxResults;
            request.Type = "channel";

            // Call the search.list method to retrieve results matching the specified query term.
            var response = await request.ExecuteAsync();
            var result = new List<SearchResult>();

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            foreach (var resultItem in response.Items)
            {
                var item = new SearchResult();
                item.Name = resultItem.Id?.ToString() ?? "No - Id";

                resultItem.

                switch (resultItem.Id.Kind)
                {
                    case "youtube#video":
                        item.Description = String.Format("{0} ({1})", resultItem.Snippet.Title, resultItem.Id.VideoId);
                        break;

                    case "youtube#channel":
                        item.Description = String.Format("{0} ({1})", resultItem.Snippet.Title, resultItem.Id.ChannelId);
                        break;

                    case "youtube#playlist":
                        item.Description = String.Format("{0} ({1})", resultItem.Snippet.Title, resultItem.Id.PlaylistId);
                        break;

                    default:
                        item.Description = String.Format("{0} ({1})", resultItem.Snippet.Title, resultItem.Id.PlaylistId);
                        break;
                }

                result.Add(item);
            }

            return result;
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
