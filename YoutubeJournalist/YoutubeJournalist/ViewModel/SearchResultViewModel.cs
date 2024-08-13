using System;

using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.Extensions.Command;
using WpfCustomUtilities.Extensions.Event;

using YoutubeJournalist.Core.Service.Model;

namespace YoutubeJournalist.ViewModel
{
    public class SearchResultViewModel : ViewModelBase
    {
        bool _isLocal;
        string _thumbnail;
        string _videoId;
        string _channelId;
        string _playlistId;
        string _title;
        string _description;
        DateTime _created;
        BasicSearchType _type;

        public SimpleEventHandler<string, bool> GetChannelDetailsEvent;
        public SimpleEventHandler<string, bool> GetVideoDetailsEvent;

        SimpleCommand<string> _getVideoDetailsCommand;
        SimpleCommand<string> _getChannelDetailsCommand;

        public SimpleCommand<string> GetVideoDetailsCommand
        {
            get { return _getVideoDetailsCommand; }
            set { RaiseAndSetIfChanged(ref _getVideoDetailsCommand, value); }
        }
        public SimpleCommand<string> GetChannelDetailsCommand
        {
            get { return _getChannelDetailsCommand; }
            set { RaiseAndSetIfChanged(ref _getChannelDetailsCommand, value); }
        }
        public bool IsLocal
        {
            get { return _isLocal; }
            set { this.RaiseAndSetIfChanged(ref _isLocal, value); }
        }
        public string Thumbnail
        {
            get { return _thumbnail; }
            set { this.RaiseAndSetIfChanged(ref _thumbnail, value); }
        }
        public string ChannelId
        {
            get { return _channelId; }
            set { this.RaiseAndSetIfChanged(ref _channelId, value); }
        }
        public string VideoId
        {
            get { return _videoId; }
            set { this.RaiseAndSetIfChanged(ref _videoId, value); }
        }
        public string PlaylistId
        {
            get { return _playlistId; }
            set { this.RaiseAndSetIfChanged(ref _playlistId, value); }
        }
        public string Title
        {
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
        }
        public string Description
        {
            get { return _description; }
            set { this.RaiseAndSetIfChanged(ref _description, value); }
        }
        public DateTime Created
        {
            get { return _created; }
            set { this.RaiseAndSetIfChanged(ref _created, value); }
        }
        public BasicSearchType Type
        {
            get { return _type; }
            set { this.RaiseAndSetIfChanged(ref _type, value); }
        }

        public SearchResultViewModel()
        {
            this.Thumbnail = "";
            this.ChannelId = "";
            this.VideoId = "";
            this.PlaylistId = "";
            this.Title = "";
            this.Description = "";
            this.Created = DateTime.Now;
            this.Type = BasicSearchType.Video;
            this.IsLocal = false;

            this.GetVideoDetailsCommand = new SimpleCommand<string>(videoId => this.GetVideoDetailsEvent(videoId, _isLocal));
            this.GetChannelDetailsCommand = new SimpleCommand<string>(channelId => this.GetChannelDetailsEvent(channelId, _isLocal));
        }
    }
}
