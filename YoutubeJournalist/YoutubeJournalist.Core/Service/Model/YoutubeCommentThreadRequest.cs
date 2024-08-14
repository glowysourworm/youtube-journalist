namespace YoutubeJournalist.Core.Service.Model
{
    /// <summary>
    /// Youtube request that selects either a comment thread for specific video(s), or ALL threads related to
    /// a single channel.
    /// </summary>
    public class YoutubeCommentThreadRequest : YoutubeServiceRequestBase
    {
        public string VideoId { get; set; }
        public string ChannelId { get; set; }

        public YoutubeCommentThreadRequest()
        {
            this.VideoId = "";
            this.ChannelId = "";
        }
    }
}
