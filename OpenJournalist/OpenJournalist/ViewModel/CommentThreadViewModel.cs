using System.Collections.ObjectModel;

using WpfCustomUtilities.Extensions;

namespace OpenJournalist.ViewModel
{
    public class CommentThreadViewModel : ViewModelBase
    {
        private CommentViewModel _comment;
        private ObservableCollection<CommentViewModel> _replies;
        private bool _isPublic;
        private int _totalReplyCount;
        private string _videoId;
        private string _channelId;

        public CommentViewModel Comment
        {
            get { return _comment; }
            set { this.RaiseAndSetIfChanged(ref _comment, value); }
        }
        public ObservableCollection<CommentViewModel> Replies
        {
            get { return _replies; }
            set { this.RaiseAndSetIfChanged(ref _replies, value); }
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
        public bool IsPublic
        {
            get { return _isPublic; }
            set { this.RaiseAndSetIfChanged(ref _isPublic, value); }
        }
        public int TotalReplyCount
        {
            get { return _totalReplyCount; }
            set { this.RaiseAndSetIfChanged(ref _totalReplyCount, value); }
        }

        public CommentThreadViewModel()
        {
            this.Comment = new CommentViewModel();
            this.Replies = new ObservableCollection<CommentViewModel>();
        }
    }
}
