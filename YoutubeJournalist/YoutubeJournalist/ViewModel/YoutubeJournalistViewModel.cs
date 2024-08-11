using System;
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
        VideoViewModel _selectedVideo;

        ObservableCollection<ChannelViewModel> _channels;
        ObservableCollection<VideoViewModel> _looseVideos;
        ObservableCollection<SearchResultViewModel> _searchResults;
        ObservableCollection<SearchResultViewModel> _localResults;
        ObservableCollection<LogViewModel> _outputLog;

        public SimpleEventHandler<string, bool> GetVideoDetailsEvent;
        public SimpleEventHandler<string, bool> GetChannelDetailsEvent;

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
        public VideoViewModel SelectedVideo
        {
            get { return _selectedVideo; }
            set { this.RaiseAndSetIfChanged(ref _selectedVideo, value); }
        }
        public ObservableCollection<ChannelViewModel> Channels
        {
            get { return _channels; }
            set { this.RaiseAndSetIfChanged(ref _channels, value); }
        }
        public ObservableCollection<VideoViewModel> LooseVideos
        {
            get { return _looseVideos; }
            set { this.RaiseAndSetIfChanged(ref _looseVideos, value); }
        }
        public ObservableCollection<SearchResultViewModel> SearchResults
        {
            get { return _searchResults; }
            set { this.RaiseAndSetIfChanged(ref _searchResults, value); }
        }
        public ObservableCollection<SearchResultViewModel> LocalResults
        {
            get { return _localResults; }
            set { this.RaiseAndSetIfChanged(ref _localResults, value); }
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
            this.LocalResults = new ObservableCollection<SearchResultViewModel>();
            this.OutputLog = new ObservableCollection<LogViewModel>();
            this.Channels = new ObservableCollection<ChannelViewModel>();
            this.LooseVideos = new ObservableCollection<VideoViewModel>();

            this.SearchResults.CollectionChanged += OnSearchCollectionChanged;
            this.LocalResults.CollectionChanged += OnSearchCollectionChanged;
        }

        private void OnSearchCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Rehook events
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.Cast<SearchResultViewModel>())
                {
                    item.GetVideoDetailsEvent -= OnGetVideoDetails;
                    item.GetChannelDetailsEvent -= OnGetChannelDetails;
                }
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.Cast<SearchResultViewModel>())
                {
                    item.GetVideoDetailsEvent += OnGetVideoDetails;
                    item.GetChannelDetailsEvent += OnGetChannelDetails;
                }
            }
        }

        private void OnGetVideoDetails(string videoId, bool isLocal)
        {
            if (this.GetVideoDetailsEvent != null)
                this.GetVideoDetailsEvent(videoId, isLocal);
        }
        private void OnGetChannelDetails(string channelId, bool isLocal)
        {
            if (this.GetChannelDetailsEvent != null)
                this.GetChannelDetailsEvent(channelId, isLocal);
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
