using System.Collections.ObjectModel;

using WpfCustomUtilities.Extensions;

namespace YoutubeJournalist.ViewModel
{
    public class YoutubeJournalistViewModel : ViewModelBase
    {
        ConfigurationViewModel _configuration;
        SearchParametersViewModel _searchParameters;
        ObservableCollection<SearchResultViewModel> _searchResults;
        ObservableCollection<LogViewModel> _outputLog;

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
        public ObservableCollection<SearchResultViewModel> SearchResults
        {
            get { return _searchResults; }
            set { this.RaiseAndSetIfChanged(ref _searchResults, value); }
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
            this.OutputLog = new ObservableCollection<LogViewModel>();

            this.SearchParameters.PropertyChanged += SearchParameters_PropertyChanged;
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
