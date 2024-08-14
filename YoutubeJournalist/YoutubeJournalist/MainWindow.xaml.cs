using System;
using System.Linq;
using System.Windows;

using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.Extensions.ObservableCollection;
using WpfCustomUtilities.Extensions.Collection;

using YoutubeJournalist.Core.Extension;
using YoutubeJournalist.Core.Service.Model;
using YoutubeJournalist.Event;
using YoutubeJournalist.ViewModel;
using Google.Apis.Util;

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
                _viewModel.Channels.Clear();
                _viewModel.LocalSearchResults.Clear();

                _viewModel.SelectedChannel = null;
                _viewModel.SelectedCommentThread = null;

                _viewModel.Channels.AddRange(_controller.GetChannels());
                _viewModel.LocalSearchResults.AddRange(_controller.GetSearchResults());

                if (_viewModel.Channels.Count > 0)
                    this.ChannelTab.IsSelected = true;
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void ExecuteGetChannelDetails(string channelId)
        {
            // Procedure
            // 
            // 1) Get complete channel details
            // 2) Get complete playlist details for the channel
            // 3) Get complete set of videos for the channel from playlist video ids
            // 4) Get complete set of comment threads for the channel (Youtube function provided)
            //

            // TODO: IMPLEMENT PAGING!

            try
            {
                var channel = _controller.SearchUpdateChannelDetails(new YoutubeChannelDetailsRequest()
                {
                    ChannelId = channelId
                });

                if (channel == null)
                {
                    OnLog(string.Format("Youtube channel not found:  {0}", channelId), true);
                }

                var playlists = _controller.SearchUpdatePlaylistDetails(new YoutubePlaylistRequest()
                {
                    //ChannelId = channelId,
                    PlaylistId = channel.PrimaryPlaylistId
                });

                var videos = _controller.SearchUpdateVideoDetails(new YoutubeVideoDetailsRequest()
                { 
                    VideoIds = new Repeatable<string>(playlists.SelectMany(x => x.PlaylistItems.Select(z => z.VideoId)).Actualize())
                });

                //foreach (var video in videos)
                //{
                //    var commentThreads = _controller.SearchCommentThreads(new YoutubeCommentThreadRequest()
                //    {
                //        VideoId = video.Id
                //    });

                //    video.CommentThreads.AddRange(commentThreads);
                //}

                var commentThreads = _controller.SearchCommentThreads(new YoutubeCommentThreadRequest()
                {
                    ChannelId = channel.Id
                });

                foreach (var commentThread in commentThreads)
                {
                    var video = videos.FirstOrDefault(x => x.Id == commentThread.VideoId);

                    if (video == null)
                        OnLog(string.Format("Missing video for comment thread:  {0}", commentThread.Comment.Display), true);

                    video.CommentThreads.Add(commentThread);
                }


                // Wire it all up!
                channel.Playlists.AddRange(playlists);
                channel.Videos.AddRange(videos);

                _viewModel.Channels.Remove(x => x.Id == channel.Id);
                _viewModel.Channels.Add(channel);
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void OnGetChannelDetails(string channelId)
        {
            try
            {
                if (_controller.HasChannel(channelId))
                {
                    _viewModel.SelectedChannel = _viewModel.Channels.First(x => x.Id == channelId);
                }
                else
                {
                    ExecuteGetChannelDetails(channelId);
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
                //if (isLocal)
                //{
                //    _viewModel.SelectedVideo = _viewModel.Search.First(x => x.Id == videoId);
                //}
                //else
                //{
                //    var channelId = _viewModel.SearchResults.First(x => x.VideoId == videoId).ChannelId;

                //    ExecuteGetChannelDetails(channelId);
                //}

                //// Select Local Channel Tab
                //this.LooseVideosTab.IsSelected = true;
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void BasicSearchControl_SearchEvent(object sender, string searchText)
        {
            try
            {
                // Local
                if (!_viewModel.SearchParameters.YoutubeAPIEnable)
                {
                    RefreshLocal();
                }

                // Youtube
                else
                {
                    var result = _controller.BasicSearch(new YoutubeBasicSearchRequest()
                    {
                        WildCard = searchText,
                        SearchType = _viewModel.SearchParameters.FilterSearchType,
                        FilterType = BasicFilterType.WildCard,
                        SortOrder = Google.Apis.YouTube.v3.SearchResource.ListRequest.OrderEnum.Title
                    });

                    _viewModel.SearchResults.AddRange(result);

                    RefreshLocal();
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
            var viewModel = (e as CustomRoutedEventArgs<SearchResultViewModel>).Data;

            switch (viewModel.Type)
            {
                case BasicSearchType.Channel:
                    OnGetChannelDetails(viewModel.ChannelId);
                    break;
                case BasicSearchType.Video:
                case BasicSearchType.Playlist:
                default:
                    throw new FormattedException("Unhandled Search Result Type {0}", viewModel.Type);
            }
        }

        private void OnLog(string message, bool isError)
        {
            _viewModel.OutputLog.Add(new LogViewModel()
            {
                IsError = isError,
                Log = message
            });
        }
    }
}
