using System;
using System.Linq;
using System.Windows;

using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.Extensions.ObservableCollection;

using YoutubeJournalist.Core.Extension;
using YoutubeJournalist.Core.Service.Model;
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

            _viewModel.GetVideoDetailsEvent += OnGetVideoDetails;
            _viewModel.GetChannelDetailsEvent += OnGetChannelDetails;

            this.DataContext = _viewModel;

            this.Loaded += OnLoaded;
        }

        // TODO: Use Application -> Exit to and IOC container to complete the proper shutdown
        protected override void OnClosed(EventArgs e)
        {
            // Local DB, Youtube Service DeAuth
            //_controller.Dispose();

            base.OnClosed(e);
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                RefreshLocal();
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void OnException(Exception ex)
        {
            MessageBox.Show(ex.ToString());

            if (!string.IsNullOrEmpty(ex.InnerException?.Message))
                MessageBox.Show(ex.InnerException?.Message);
        }

        private void RefreshLocal()
        {
            try
            {
                _viewModel.LocalResults.Clear();
                _viewModel.Channels.Clear();
                _viewModel.LooseVideos.Clear();

                _viewModel.SelectedChannel = null;
                _viewModel.SelectedVideo = null;

                _viewModel.LocalResults.AddRange(_controller.GetSearchResults());

                if (_viewModel.LocalResults.Count > 0)
                    this.LocalSearchTab.IsSelected = true;
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void OnGetChannelDetails(string channelId, bool isLocal)
        {
            try
            {
                if (isLocal)
                {
                    _viewModel.SelectedChannel = _viewModel.Channels.First(x => x.Id == channelId);
                }
                else
                {
                    var channel = _controller.SearchUpdateChannelDetails(new YoutubeChannelDetailsRequest()
                    {
                        ChannelId = channelId
                    });

                    // Local view model, or add view model
                    var existingChannel = _viewModel.Channels.FirstOrDefault(x => x.Id == channelId);
                    if (existingChannel != null)
                        _viewModel.SelectedChannel = existingChannel;

                    else
                    {
                        _viewModel.Channels.Add(channel);
                        _viewModel.SelectedChannel = channel;
                    }
                }

                // Select Local Channel Tab
                this.ChannelTab.IsSelected = true;
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void OnGetVideoDetails(string videoId, bool isLocal)
        {
            try
            {
                if (isLocal)
                {
                    _viewModel.SelectedVideo = _viewModel.LooseVideos.First(x => x.Id == videoId);
                }
                else
                {
                    var video = _controller.SearchUpdateVideoDetails(new YoutubeVideoDetailsRequest()
                    {
                        VideoIds = videoId.ToRepeatable()

                    }).First();

                    // Local view model, or add view model
                    var existingVideo = _viewModel.LooseVideos.FirstOrDefault(x => x.Id == videoId);
                    if (existingVideo != null)
                        _viewModel.SelectedVideo = existingVideo;

                    else
                    {
                        _viewModel.LooseVideos.Add(video);
                        _viewModel.SelectedVideo = video;
                    }
                }

                // Select Local Channel Tab
                this.LooseVideosTab.IsSelected = true;
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void BasicSearchControl_SearchEvent(object sender, string e)
        {
            try
            {
                // Local
                if (!_viewModel.SearchParameters.YoutubeAPIEnable)
                {
                    var result = _controller.GetSearchResults();

                    _viewModel.LocalResults.Clear();
                    _viewModel.LocalResults.AddRange(result);
                }

                // Youtube
                else
                {
                    var result = _controller.BasicSearch(new YoutubeBasicSearchRequest()
                    {
                        WildCard = _viewModel.SearchParameters.FilterString,
                        SortOrder = Google.Apis.YouTube.v3.SearchResource.ListRequest.OrderEnum.Title
                    });

                    _viewModel.SearchResults.AddRange(result);
                }
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void BasicSearchControl_PlatformTypeChanged(object sender, PlatformType e)
        {
            _viewModel.SearchParameters.YoutubeAPIEnable = (e == PlatformType.Youtube);
        }

        private void OnSearchResultDoubleClick(object sender, RoutedEventArgs e)
        {
            var viewModel = sender as SearchResultViewModel;

            switch (viewModel.Type)
            {
                case BasicSearchType.Channel:
                    OnGetChannelDetails(viewModel.Id, viewModel.IsLocal);
                    break;
                case BasicSearchType.Video:
                    OnGetVideoDetails(viewModel.Id, viewModel.IsLocal);
                    break;
                case BasicSearchType.Playlist:
                default:
                    throw new FormattedException("Unhandled Search Result Type {0}", viewModel.Type);
            }
        }
    }
}
