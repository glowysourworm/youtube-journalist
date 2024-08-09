using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;

using WpfCustomUtilities.Extensions.ObservableCollection;

using YoutubeJournalist.Core;
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
            // TODO: Use typed routed event
            var viewModel = (e as CustomRoutedEventArgs<SearchResultViewModel>).Data;

            ExecuteGetChannel(viewModel.Id);
        }
    }
}
