using System;

using WpfCustomUtilities.Extensions;

namespace YoutubeJournalist.ViewModel
{
    public class SearchResultViewModel : ViewModelBase
    {
        string _thumbnail;
        string _id;
        string _etag;
        string _title;
        string _description;
        DateTime _created;
        DateTime _updated;

        public string Thumbnail
        {
            get { return _thumbnail; }
            set { RaiseAndSetIfChanged(ref _thumbnail, value); }
        }
        public string Id
        {
            get { return _id; }
            set { RaiseAndSetIfChanged(ref _id, value); }
        }
        public string ETag
        {
            get { return _etag; }
            set { RaiseAndSetIfChanged(ref _etag, value); }
        }
        public string Title
        {
            get { return _title; }
            set { RaiseAndSetIfChanged(ref _title, value); }
        }
        public string Description
        {
            get { return _description; }
            set { RaiseAndSetIfChanged(ref _description, value); }
        }
        public DateTime Created
        {
            get { return _created; }
            set { RaiseAndSetIfChanged(ref _created, value); }
        }
        public DateTime Updated
        {
            get { return _updated; }
            set { RaiseAndSetIfChanged(ref _updated, value); }
        }

        public SearchResultViewModel()
        {
            this.Thumbnail = "";
            this.Id = "";
            this.ETag = "";
            this.Title = "";
            this.Description = "";
            this.Created = DateTime.Now;
            this.Updated = DateTime.Now;
        }
    }
}
