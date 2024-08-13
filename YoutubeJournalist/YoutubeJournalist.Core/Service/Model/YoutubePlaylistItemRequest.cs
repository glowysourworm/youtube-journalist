using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeJournalist.Core.Service.Model
{
    public class YoutubePlaylistItemRequest : YoutubeServiceRequestBase
    {
        public string PlaylistId { get; set; }
    }
}
