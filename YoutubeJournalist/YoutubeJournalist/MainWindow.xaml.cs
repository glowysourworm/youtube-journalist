using System;
using System.Windows;

using WpfCustomUtilities.Extensions.ObservableCollection;

using YoutubeJournalist.Core.WebAPI.Google.Apis.Youtube.V3;
using YoutubeJournalist.Event;
using YoutubeJournalist.ViewModel;

namespace YoutubeJournalist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly Controller _controller;
        readonly YoutubeJournalistViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            var configuration = new ConfigurationViewModel();

            _controller = new Controller(configuration);
            _viewModel = new YoutubeJournalistViewModel(configuration, new SearchParametersViewModel());

            this.DataContext = _viewModel;

            this.Loaded += OnLoaded;
        }

        // TODO: Use Application -> Exit to and IOC container to complete the proper shutdown
        protected override void OnClosed(EventArgs e)
        {
            // Local DB, Youtube Service DeAuth
            _controller.Dispose();

            base.OnClosed(e);
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ExecuteLocalSearch();
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void OnException(Exception ex)
        {
            MessageBox.Show(ex.Message);

            if (!string.IsNullOrEmpty(ex.InnerException?.Message))
                MessageBox.Show(ex.InnerException?.Message);

            // Close DB connection
            _controller.Dispose();

            // Shuts down -> OnClosed()
            App.Current.Shutdown();
        }
        private void ExecuteLocalSearch()
        {
            var result = _controller.GetSearchResults(new YoutubeServiceRequest()
            {
                WildCard = _viewModel.SearchParameters.FilterString,
                Search = _viewModel.SearchParameters.FilterSearchType,
                Filter = YoutubeServiceRequest.FilterType.WildCard,
                SortOrder = Google.Apis.YouTube.v3.SearchResource.ListRequest.OrderEnum.Title
            });

            _viewModel.SearchResults.AddRange(result);
        }
        private void ExecuteBasicSearch()
        {
            var result = _controller.Search(new YoutubeServiceRequest()
            {
                WildCard = _viewModel.SearchParameters.FilterString,
                Search = _viewModel.SearchParameters.FilterSearchType,
                Filter = YoutubeServiceRequest.FilterType.WildCard,
                SortOrder = Google.Apis.YouTube.v3.SearchResource.ListRequest.OrderEnum.Title
            });

            _viewModel.SearchResults.AddRange(result);
        }
        private void ExecuteGetChannel(string channelId)
        {
            var result = _controller.GetChannels(new YoutubeServiceRequest()
            {
                ChannelId = channelId,
                Search = YoutubeServiceRequest.SearchType.Video,
                Filter = YoutubeServiceRequest.FilterType.Id
            });

            _viewModel.SearchResults.AddRange(result);
        }
        private void ExecuteGetVideo(string videoId)
        {
            var result = _controller.GetVideos(new YoutubeServiceRequest()
            {
                VideoId = videoId,
                Search = YoutubeServiceRequest.SearchType.Video,
                Filter = YoutubeServiceRequest.FilterType.Id
            });

            _viewModel.SearchResults.AddRange(result);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExecuteBasicSearch();
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void OnSearchResultDoubleClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // TODO: Use typed routed event
                var viewModel = (e as CustomRoutedEventArgs<SearchResultViewModel>).Data;

                _viewModel.SearchResults.Clear();

                if (viewModel.IsChannel)
                    ExecuteGetChannel(viewModel.Id);

                else
                    ExecuteGetVideo(viewModel.Id);
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void GetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExecuteLocalSearch();
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }
    }
}
