using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeJournalist
{
    public class Configuration
    {
        public string ApiKey { get; set; }
        public int MaxSearchResults { get; set; }

        public Configuration()
        {
            this.ApiKey = "";
            this.MaxSearchResults = int.MaxValue;
        }
    }
}
