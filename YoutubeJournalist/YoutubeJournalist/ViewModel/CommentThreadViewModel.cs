using System.Collections.ObjectModel;

using WpfCustomUtilities.Extensions;

namespace YoutubeJournalist.ViewModel
{
    public class CommentThreadViewModel : ViewModelBase
    {
        private CommentViewModel _comment;
        private ObservableCollection<CommentViewModel> _replies;
        private bool _isPublic;
        private int _totalReplyCount;

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
