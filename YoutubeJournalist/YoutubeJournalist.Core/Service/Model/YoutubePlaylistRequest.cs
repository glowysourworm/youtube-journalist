using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeJournalist.Core.Service.Model
{
    public class YoutubePlaylistRequest : YoutubeServiceRequestBase
    {
        /// <summary>
        /// YOUTUBE ISSUE!!!  THE PLAYLIST ID REFERS TO THE ContentDetails.RelatedPlaylists.Uploads FIELD, OR
        ///                   THE ACTUAL PLAYLIST ID, (WHICH MEANS IT WAS ALREADY RETRIEVED). THIS WAS HARD TO
        ///                   ACCESS.....
        ///                   
        ///                   The "Uploads" playlist is assumed to be the primary channel playlist, that contains
        ///                   all videos for the channel. Awfully hard to get to.....
        ///                   
        ///                   1) Channel -> Videos -> Comment Threads (nope!)
        ///                   2) Channel -> Playlists -> Comment Threads & Videos (nope!)
        ///                   3) Channel -> Channel.ContentDetails.RelatedPlaylists.Uploads (string),  (yes.)
        ///                   
        ///                   Then, get playlists, then comment threads, and videos (if desired)
        /// </summary>
        public string PlaylistId { get; set; }
    }
}
