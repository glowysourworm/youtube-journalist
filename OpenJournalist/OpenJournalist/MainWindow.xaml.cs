﻿using System;
using System.Linq;
using System.Windows;

using OpenJournalist.Event;
using OpenJournalist.Service.Model;
using OpenJournalist.Service.Model.Message;
using OpenJournalist.ViewModel;
using OpenJournalist.ViewModel.SearchResult;

using WpfCustomUtilities.Extensions.Collection;
using WpfCustomUtilities.Extensions.ObservableCollection;

namespace OpenJournalist
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
            _viewModel = new YoutubeJournalistViewModel(configuration);

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
            OnLog(ex.Message, true);

            if (!string.IsNullOrEmpty(ex.InnerException?.Message))
            {
                MessageBox.Show(ex.InnerException?.Message);
                OnLog(ex.InnerException.Message, true);
            }
        }

        private async void RefreshLocal()
        {
            try
            {
                //_viewModel.Channels.Clear();
                //_viewModel.LocalSearchResults.Clear();

                //_viewModel.SelectedChannel = null;
                //_viewModel.SelectedCommentThread = null;

                //var results = await _controller.GetLocalSearchResults(new LocalPaginatedRequest(OnMessageCallback));

                //_viewModel.Channels.AddRange(_controller.GetChannels());
                //_viewModel.LocalSearchResults.AddRange(_controller.GetSearchResults());

                //if (_viewModel.Channels.Count > 0)
                //    this.ChannelTab.IsSelected = true;
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void ExecuteGetChannelDetails(int channelId)
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
                //var channel = _controller.SearchUpdateChannelDetails(new YoutubeChannelDetailsRequest()
                //{
                //    ChannelId = channelId
                //});

                //if (channel == null)
                //{
                //    OnLog(string.Format("Youtube channel not found:  {0}", channelId), true);
                //    return;
                //}

                //var playlists = _controller.SearchUpdatePlaylistDetails(new YoutubePlaylistRequest()
                //{
                //    //ChannelId = channelId,
                //    PlaylistId = channel.PrimaryPlaylistId
                //});

                //var videos = _controller.SearchUpdateVideoDetails(new YoutubeVideoDetailsRequest()
                //{ 
                //    VideoIds = new Repeatable<string>(playlists.SelectMany(x => x.PlaylistItems.Select(z => z.VideoId)).Actualize())
                //});

                //foreach (var video in videos)
                //{
                //    var commentThreads = _controller.SearchCommentThreads(new YoutubeCommentThreadRequest()
                //    {
                //        VideoId = video.Id
                //    });

                //    video.CommentThreads.AddRange(commentThreads);
                //}

                //var commentThreads = _controller.SearchCommentThreads(new YoutubeCommentThreadRequest()
                //{
                //    ChannelId = channel.Id
                //});

                //foreach (var commentThread in commentThreads)
                //{
                //    var video = videos.FirstOrDefault(x => x.Id == commentThread.VideoId);

                //    if (video == null)
                //        OnLog(string.Format("Missing video for comment thread:  {0}", commentThread.Comment.Display), true);

                //    video.CommentThreads.Add(commentThread);
                //}


                // Wire it all up!
                //channel.Playlists.AddRange(chan);
                //channel.Videos.AddRange(videos);

                //_viewModel.Channels.Remove(x => x.Id == channel.Id);
                //_viewModel.Channels.Add(channel);

                //_viewModel.SelectedChannel = channel;

                //this.ChannelTab.IsSelected = true;
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
                //if (_controller.HasChannel(channelId))
                //{
                //    _viewModel.SelectedChannel = _viewModel.Channels.First(x => x.Id == channelId);
                //}
                //else
                //{
                //    ExecuteGetChannelDetails(channelId);
                //}

                //// Select Local Channel Tab
                //this.ChannelTab.IsSelected = true;
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

        private async void OnLocalBasicSearch(object sender, string searchText)
        {
            //try
            //{
            //    // Local
            //    var results = await _controller.GetSearchResults(new LocalPaginatedRequest(OnMessageCallback, searchText));


            //    RefreshLocal();
            //}
            //catch (Exception ex)
            //{
            //    OnException(ex);
            //}
        }
        private void OnPlatformBasicSearch(object sender, string searchText)
        {
            //try
            //{
            //    // Local
            //    if (!_viewModel.SelectedPlatformEnable)
            //    {
            //        RefreshLocal();
            //    }

            //    // Youtube
            //    else
            //    {
            //        var result = _controller.BasicSearch(new YoutubeBasicSearchRequest()
            //        {
            //            WildCard = searchText
            //        });

            //        _viewModel.SearchResults.AddRange(result);

            //        RefreshLocal();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    OnException(ex);
            //}
        }

        private void OnSearchResultDoubleClick(object sender, RoutedEventArgs e)
        {
            var viewModel = (e as CustomRoutedEventArgs<YoutubeSearchResultViewModel>).Data;

            OnGetChannelDetails(viewModel.ChannelId);
        }

        private void OnSelectedChannelRefreshEvent(object sender, EventArgs e)
        {
            ExecuteGetChannelDetails(_viewModel.SelectedChannel.Id);
        }

        private void OnMessageCallback(MessagingCallbackEventArgsBase args)
        {
            switch (args.CallbackType)
            {
                case MessageCallbackType.SimpleMessage:

                    if (args is ServiceMessageEventArgs)
                    {
                        OnLog(args.Message, false);
                    }
                    else
                        throw new Exception("Unhandled service callback event args type:  MainWindow.OnMessageCallback");

                    break;
                case MessageCallbackType.ErrorMessage:

                    OnLog(args.Message, true);

                    break;
                case MessageCallbackType.PagedResultsMessage:

                    if (args is PaginatedCallbackEventArgs)
                    {
                        var eventArgs = args as PaginatedCallbackEventArgs;


                        var message = string.Format("Retrieved results {0} to {1} of {2}",
                                                    eventArgs.ResultFrom, eventArgs.ResultTo, eventArgs.ResultTotal);

                        OnLog(message, false);
                    }
                    else
                        throw new Exception("Unhandled service callback event args type:  MainWindow.OnMessageCallback");

                    break;
                default:
                    throw new Exception("Unhandled service callback event args type:  MainWindow.OnMessageCallback");
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