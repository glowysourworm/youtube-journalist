
using WpfCustomUtilities.Extensions;

namespace YoutubeJournalist.ViewModel
{
    public class PlaylistItemViewModel : ViewModelBase
    {
        string _id;
        string _playlistId;
        string _videoId;
        string _channelId;
        string _ownerChannelId;
        string _note;
        string _thumbnailUrl;
        string _title;
        string _description;
        string _privacyStatus;
        long _position;

        public string Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public string PlaylistId
        {
            get { return _playlistId; }
            set { this.RaiseAndSetIfChanged(ref _playlistId, value); }
        }
        public string VideoId
        {
            get { return _videoId; }
            set { this.RaiseAndSetIfChanged(ref _videoId, value); }
        }
        public string ChannelId
        {
            get { return _channelId; }
            set { this.RaiseAndSetIfChanged(ref _channelId, value); }
        }
        public string OwnerChannelId
        {
            get { return _ownerChannelId; }
            set { this.RaiseAndSetIfChanged(ref _ownerChannelId, value); }
        }
        public string Note
        {
            get { return _note; }
            set { this.RaiseAndSetIfChanged(ref _note, value); }
        }
        public string ThumbnailUrl
        {
            get { return _thumbnailUrl; }
            set { this.RaiseAndSetIfChanged(ref _thumbnailUrl, value); }
        }
        public string Title
        {
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
        }
        public string Description
        {
            get { return _description; }
            set { this.RaiseAndSetIfChanged(ref _description, value); }
        }
        public string PrivacyStatus
        {
            get { return _privacyStatus; }
            set { this.RaiseAndSetIfChanged(ref _privacyStatus, value); }
        }
        public long Position
        {
            get { return _position; }
            set { this.RaiseAndSetIfChanged(ref _position, value); }
        }
    }
}
