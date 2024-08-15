using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenJournalist.Core.Service.Model
{
    public class YoutubeCommentsRequest : YoutubeServiceRequestBase
    {
        public string CommentId { get; set; }
    }
}
