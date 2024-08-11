using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WpfCustomUtilities.Extensions;

using YoutubeJournalist.Core.Service.Model;

using static Google.Apis.YouTube.v3.SubscriptionsResource.ListRequest;

namespace YoutubeJournalist.ViewModel
{
    public class SearchParametersViewModel : ViewModelBase
    {
        private bool _youtubeAPIEnable;
        private bool _youtubeBasicSearch;
        private BasicSearchType _filterSearchType;
        private string _filterString;
        private DateTimeOffset? _filterPublishedBefore;
        private DateTimeOffset? _filterPublishedAfter;
        private OrderEnum? _filterSortOrder;

		[Category("Youtube API")]
        [DisplayName("Youtube Enable (Uses Quota!)")]
        [Description("This will enable the Youtube API V3, making draws from your account.")]
		public bool YoutubeAPIEnable
		{
			get { return _youtubeAPIEnable; }
			set { this.RaiseAndSetIfChanged(ref _youtubeAPIEnable, value); }
		}

		[Category("Youtube API")]
		[DisplayName("Basic Search")]
		[Description("This turns on/off the basic or 'snippet' search. Turn off to use filtered search options.")]
		public bool YoutubeBasicSearch
		{
			get { return _youtubeBasicSearch; }
			set { this.RaiseAndSetIfChanged(ref _youtubeBasicSearch, value); }
		}

		[Category("Filter")]
		[DisplayName("Search Type")]
		[Description("Specifies basic OR filtered search type")]
		public BasicSearchType FilterSearchType
		{
			get { return _filterSearchType; }
			set { this.RaiseAndSetIfChanged(ref _filterSearchType, value); }
		}

		[Category("Filter")]
		[DisplayName("Search String")]
		[Description("Wildcard search string used by Youtube service")]
		public string FilterString
		{
			get { return _filterString; }
			set { this.RaiseAndSetIfChanged(ref _filterString, value); }
		}

		[Category("Filter")]
		[DisplayName("Published Before")]
		[Description("Constraint on video, channel, or playlist publish date")]
		//[Editor("DateTimeOffset", "DateTimeOffsetPicker")]
		public DateTimeOffset? FilterPublishedBefore
		{
			get { return _filterPublishedBefore; }
			set { this.RaiseAndSetIfChanged(ref _filterPublishedBefore, value); }
		}

		[Category("Filter")]
		[DisplayName("Published After")]
		[Description("Constraint on video, channel, or playlist publish date")]
		public DateTimeOffset? FilterPublishedAfter
		{
			get { return _filterPublishedAfter; }
			set { this.RaiseAndSetIfChanged(ref _filterPublishedAfter, value); }
		}

		[Category("Filter")]
		[DisplayName("Sort Order")]
		[Description("Order by a specific field, or relevance")]
		public OrderEnum? FilterSortOrder
		{
			get { return _filterSortOrder; }
			set { this.RaiseAndSetIfChanged(ref _filterSortOrder, value); }
		}

		public SearchParametersViewModel()
        {
			this.YoutubeAPIEnable = false;
			this.YoutubeBasicSearch = true;
			this.FilterSearchType = BasicSearchType.Video;
        }
	}
}
