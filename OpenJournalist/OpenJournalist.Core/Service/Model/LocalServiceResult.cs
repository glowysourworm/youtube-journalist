using System.Collections.Generic;
using System.Data.Objects.DataClasses;

namespace OpenJournalist.Core.Service.Model
{
    public class LocalServiceResult<T>
    {
        public IEnumerable<T> Collection { get; private set; }

        public string NextPageToken { get; private set; }

        public LocalServiceResult(IEnumerable<T> collection, string nextPageToken = null)
        {
            this.Collection = collection;
            this.NextPageToken = nextPageToken;
        }
    }
}
