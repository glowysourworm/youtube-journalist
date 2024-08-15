using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenJournalist.Core.Service.Model
{
    public class YoutubePlaylistItemRequest : YoutubeServiceRequestBase
    {
        public string PlaylistId { get; set; }
    }
}
