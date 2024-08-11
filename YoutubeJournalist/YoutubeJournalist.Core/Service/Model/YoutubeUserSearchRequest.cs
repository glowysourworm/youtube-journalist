using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeJournalist.Core.Service.Model
{
    public class YoutubeUserSearchRequest : YoutubeServiceRequestBase
    {
        /// <summary>
        /// Filter parameter - Handle of user
        /// </summary>
        public string Handle { get; set; }

        /// <summary>
        /// Filter parameter - User name
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Filter parameter - selects what to use (handle or username)
        /// </summary>
        public UserFilterType FilterType { get; set; }
    }
}
