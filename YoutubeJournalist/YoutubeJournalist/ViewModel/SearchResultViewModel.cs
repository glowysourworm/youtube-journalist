using System;

using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.Extensions.Command;
using WpfCustomUtilities.Extensions.Event;

using YoutubeJournalist.Core.Service.Model;

namespace YoutubeJournalist.ViewModel
{
    public class SearchResultViewModel : ViewModelBase
    {
        string _thumbnail;
        string _channelId;
        string _title;
        string _description;
        DateTime _created;
        BasicSearchType _type;

        public SimpleEventHandler<string> GetChannelDetailsEvent;

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
            this.Title = "";
            this.Description = "";
            this.Created = DateTime.Now;
            this.Type = BasicSearchType.Video;

            this.GetChannelDetailsCommand = new SimpleCommand<string>(channelId => this.GetChannelDetailsEvent(channelId));
        }
    }
}
