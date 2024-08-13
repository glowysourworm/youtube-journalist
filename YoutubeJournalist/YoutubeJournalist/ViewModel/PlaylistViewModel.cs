
using System.Collections.ObjectModel;

using WpfCustomUtilities.Extensions;

namespace YoutubeJournalist.ViewModel
{
    public class PlaylistViewModel : ViewModelBase
    {
        string _id;
        string _channelId;
        string _title;
        string _description;
        string _thumbnailUrl;

        ObservableCollection<PlaylistItemViewModel> _playlistItems;

        public string Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public string ChannelId
        {
            get { return _channelId; }
            set { this.RaiseAndSetIfChanged(ref _channelId, value); }
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
        public string ThumbnailUrl
        {
            get { return _thumbnailUrl; }
            set { this.RaiseAndSetIfChanged(ref _thumbnailUrl, value); }
        }

        public ObservableCollection<PlaylistItemViewModel> PlaylistItems
        {
            get { return _playlistItems; }
            set { this.RaiseAndSetIfChanged(ref _playlistItems, value); }
        }

        public PlaylistViewModel()
        {
            this.PlaylistItems = new ObservableCollection<PlaylistItemViewModel>();
        }
    }
}
