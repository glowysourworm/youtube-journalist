using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.Extensions.Event;

namespace YoutubeJournalist.ViewModel
{
    public class YoutubeJournalistViewModel : ViewModelBase
    {
        ConfigurationViewModel _configuration;
        SearchParametersViewModel _searchParameters;

        ChannelViewModel _selectedChannel;
        CommentThreadViewModel _selectedCommentThread;

        ObservableCollection<ChannelViewModel> _channels;
        ObservableCollection<SearchResultViewModel> _searchResults;
        ObservableCollection<SearchResultViewModel> _localSearchResults;
        ObservableCollection<LogViewModel> _outputLog;

        public SimpleEventHandler<string> GetChannelDetailsEvent;

        public ConfigurationViewModel Configuration
        {
            get { return _configuration; }
            set { this.RaiseAndSetIfChanged(ref _configuration, value); }
        }
        public SearchParametersViewModel SearchParameters
        {
            get { return _searchParameters; }
            set { this.RaiseAndSetIfChanged(ref _searchParameters, value); }
        }
        public ChannelViewModel SelectedChannel
        {
            get { return _selectedChannel; }
            set { this.RaiseAndSetIfChanged(ref _selectedChannel, value); }
        }
        public CommentThreadViewModel SelectedCommentThread
        {
            get { return _selectedCommentThread; }
            set { this.RaiseAndSetIfChanged(ref _selectedCommentThread, value); }
        }
        public ObservableCollection<ChannelViewModel> Channels
        {
            get { return _channels; }
            set { this.RaiseAndSetIfChanged(ref _channels, value); }
        }
        public ObservableCollection<SearchResultViewModel> SearchResults
        {
            get { return _searchResults; }
            set { this.RaiseAndSetIfChanged(ref _searchResults, value); }
        }
        public ObservableCollection<SearchResultViewModel> LocalSearchResults
        {
            get { return _localSearchResults; }
            set { this.RaiseAndSetIfChanged(ref _localSearchResults, value); }
        }
        public ObservableCollection<LogViewModel> OutputLog
        {
            get { return _outputLog; }
            set { this.RaiseAndSetIfChanged(ref _outputLog, value); }
        }

        public YoutubeJournalistViewModel(ConfigurationViewModel configuration, SearchParametersViewModel searchParameters)
        {
            this.Configuration = configuration;
            this.SearchParameters = searchParameters;
            this.SearchResults = new ObservableCollection<SearchResultViewModel>();
            this.LocalSearchResults = new ObservableCollection<SearchResultViewModel>();
            this.OutputLog = new ObservableCollection<LogViewModel>();
            this.Channels = new ObservableCollection<ChannelViewModel>();

            this.SearchResults.CollectionChanged += OnSearchCollectionChanged;
        }

        private void OnSearchCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Rehook events
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.Cast<SearchResultViewModel>())
                {
                    item.GetChannelDetailsEvent -= OnGetChannelDetails;
                }
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.Cast<SearchResultViewModel>())
                {
                    item.GetChannelDetailsEvent += OnGetChannelDetails;
                }
            }
        }
        private void OnGetChannelDetails(string channelId)
        {
            if (this.GetChannelDetailsEvent != null)
                this.GetChannelDetailsEvent(channelId);
        }

        private void SearchParameters_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Clear out search results when we switch between youtube / local
            if (e.PropertyName == nameof(this.SearchParameters.YoutubeAPIEnable))
            {
                this.SearchResults.Clear();
            }
        }
    }
}
