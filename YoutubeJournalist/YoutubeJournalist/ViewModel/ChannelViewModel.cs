﻿using System.Collections.ObjectModel;

using WpfCustomUtilities.Extensions;

namespace YoutubeJournalist.ViewModel
{
    public class ChannelViewModel : ViewModelBase
    {
        ObservableCollection<VideoViewModel> _videos;
        ObservableCollection<PlaylistViewModel> _playlists;

        string _id;
        string _owner;
        string _title;
        string _description;
        string _bannerUrl;
        string _iconUrl;
        string _primaryPlaylistId;
        long _subscriberCount;
        long _videoCount;
        long _viewCount;
        bool _madeForKids;
        string _privacyStatus;
        bool _selfDeclaredMadeForKids;

        public string Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public string PrimaryPlaylistId
        {
            get { return _primaryPlaylistId; }
            set { this.RaiseAndSetIfChanged(ref _primaryPlaylistId, value); }
        }
        public string Owner
        {
            get { return _owner; }
            set { this.RaiseAndSetIfChanged(ref _owner, value); }
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
        public string BannerUrl
        {
            get { return _bannerUrl; }
            set { this.RaiseAndSetIfChanged(ref _bannerUrl, value); }
        }
        public string IconUrl
        {
            get { return _iconUrl; }
            set { this.RaiseAndSetIfChanged(ref _iconUrl, value); }
        }
        public long SubscriberCount
        {
            get { return _subscriberCount; }
            set { this.RaiseAndSetIfChanged(ref _subscriberCount, value); }
        }
        public long VideoCount
        {
            get { return _videoCount; }
            set { this.RaiseAndSetIfChanged(ref _videoCount, value); }
        }
        public long ViewCount
        {
            get { return _viewCount; }
            set { this.RaiseAndSetIfChanged(ref _viewCount, value); }
        }
        public bool MadeForKids
        {
            get { return _madeForKids; }
            set { this.RaiseAndSetIfChanged(ref _madeForKids, value); }
        }
        public string PrivacyStatus
        {
            get { return _privacyStatus; }
            set { this.RaiseAndSetIfChanged(ref _privacyStatus, value); }
        }
        public bool SelfDeclaredMadeForKids
        {
            get { return _selfDeclaredMadeForKids; }
            set { this.RaiseAndSetIfChanged(ref _selfDeclaredMadeForKids, value); }
        }

        public ObservableCollection<VideoViewModel> Videos
        {
            get { return _videos; }
            set { this.RaiseAndSetIfChanged(ref _videos, value); }
        }
        public ObservableCollection<PlaylistViewModel> Playlists
        {
            get { return _playlists; }
            set { this.RaiseAndSetIfChanged(ref _playlists, value); }
        }

        public ChannelViewModel()
        {
            this.Videos = new ObservableCollection<VideoViewModel>();
            this.Playlists = new ObservableCollection<PlaylistViewModel>();
        }
    }
}
