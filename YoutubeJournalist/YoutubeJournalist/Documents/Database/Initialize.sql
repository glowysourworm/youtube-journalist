-- Create Database [YoutubeJournalist]

USE master;

DROP DATABASE YoutubeJournalist
GO

CREATE DATABASE YoutubeJournalist
GO

USE YoutubeJournalist;

-- Our Table for helping to manage users
Create Table "User"(
	Id int NOT NULL IDENTITY(1,1),
	Handle varchar,
	Subsystem varchar,
	Comment varchar,

	Primary Key (Id)
);
GO

Create Table SocialMediaLink(
	Id int NOT NULL IDENTITY(1,1),
	UserId int NOT NULL,
	Handle varchar,
	SocialMediaName varchar

	Primary Key (Id),
	Foreign Key (UserId) References "User"(Id)
);
GO

Create Table UserEmail(
	Id int NOT NULL IDENTITY(1,1),
	UserId int NOT NULL,
	Email varchar,
	Comment varchar,

	Primary Key (Id),
	Foreign Key (UserId) References "User"(Id)
);
GO

Create Table Youtube_CommentSnippet(
	AuthorChannelId_Value varchar,
	AuthorChannelId_ETag varchar,
	AuthorChannelUrl varchar,
	AuthorDisplayName varchar,
	AuthorProfileImageUrl varchar,
	CanRate bit,
	ChannelId varchar,
	ETag varchar,
	LikeCount bigint,
	ModerationStatus varchar,
	ParentId varchar NOT NULL,
	PublishedAt DateTime,
	PublishedAtDateTimeOffset DateTimeOffset,
	PublishedAtRaw varchar,
	TextDisplay varchar,
	TextOriginal varchar,
	UpdatedAt DateTime,
	UpdatedAtDateTimeOffset DateTimeOffset,
	UpdatedAtRaw varchar,
	VideoId varchar,
	ViewerRating varchar,

	Primary Key(ParentId)
);
GO

Create Table Youtube_Comment(
	Id varchar NOT NULL,
	ETag varchar,
	Kind varchar,
	SnippetId varchar,

	Primary Key(Id),
	Foreign Key(SnippetId) References Youtube_CommentSnippet(ParentId)
);
GO

Create Table Youtube_CommentThread(
	Id varchar NOT NULL,
	ETag varchar,
	Kind varchar,
	SnippetId varchar NOT NULL,

	-- Google Youtube V3 API:  CommentThreadReplies --
	-- 
	-- Our SQL database will have an M:N mapper table for this. The following ID is the return value
	-- from Youtube, matched via the Youtube_CommentList table, to gather all the Comment entities.
	CommentListId int NOT NULL,

	Primary Key(Id),
	Foreign Key(SnippetId) References Youtube_CommentSnippet(ParentId)
);
GO

Create Table Youtube_CommentList(
	Our_Id int NOT NULL IDENTITY(1,1),
	CommentThreadId varchar NOT NULL,
	CommentId varchar NOT NULL,
	ETag varchar,

	Primary Key(Our_Id),
	Foreign Key(CommentThreadId) References Youtube_CommentThread(Id),
	Foreign Key(CommentId) References Youtube_Comment(Id)
);
GO

Create Table Youtube_ChannelAuditDetails(
	Our_Id int NOT NULL IDENTITY(1,1),
	CommunityGuidelinesGoodStanding bit null,
	ContentIdClaimsGoodStanding bit null,
	CopyrightStrikesGoodStanding bit null,
	ETag varchar,

	Primary Key(Our_Id)
);
GO

Create Table Youtube_ChannelSettings(
	Our_Id int NOT NULL IDENTITY(1,1),
	Country varchar,
	DefaultLanguage varchar,
	DefaultTab varchar,
	"Description" varchar,
	ETag varchar,
	FeaturedChannelsTitle varchar,
	FeaturedChannelsUrls varchar,  -- Repeated string (using comma separator)
	Keywords varchar,			   -- Repeated string (using comma separator)
	ModerateComments bit null,
	ProfileColor varchar,
	ShowBrowseView bit null,
	ShowRelatedChannels bit null,
	Title varchar,
	TrackingAnalyticsAccountId varchar,
	UnsubscribedTrailer varchar,

	Primary Key(Our_Id)
);
GO

Create Table Youtube_ChannelBrandingSettings(
	Our_Id int NOT NULL IDENTITY(1,1),
	Our_ChannelSettingsId int NOT NULL,
	ETag varchar,

	Primary Key(Our_Id),
	Foreign Key(Our_ChannelSettingsId) References Youtube_ChannelSettings(Our_Id),

	-- BackgroundImageUrl:  LocalizedProperty
	--BackgroundImageUrl_LanguageTag_ETag varchar,
	--BackgroundImageUrl_LanguageTag_Value varchar,
	--BackgroundImageUrl_Default__ varchar,
	--BackgroundImageUrl_ETag varchar,
	---- Youtube V3:  Has list of background image as List<LocalizedProperty>, so, only using the first entry.
	--BackgroundImageUrl_Language varchar,
	--BackgroundImageUrl_ETag varchar,
	--BackgroundImageUrl_Localized varchar,

	BannerExternalUrl varchar,
	BannerImageUrl varchar,
	BannerMobileExtraHdImageUrl varchar,
	BannerMobileHdImageUrl varchar,
	BannerMobileImageUrl varchar,
	BannerMobileLowImageUrl varchar,
	BannerMobileMediumHdImageUrl varchar,
	BannerTabletExtraHdImageUrl varchar,
	BannerTabletHdImageUrl varchar,
	BannerTabletImageUrl varchar,
	BannerTabletLowImageUrl varchar,
	BannerTvHighImageUrl varchar,
	BannerTvImageUrl varchar,
	BannerTvLowImageUrl varchar,
	BannerTvMediumImageUrl varchar,

	-- LargeBrandedBannerImageImapScript:  LocalizedProperty
	--LargeBrandedBannerImageImapScript varchar,
	--LargeBrandedBannerImageImapScript_LanguageTag_ETag varchar,
	--LargeBrandedBannerImageImapScript_LanguageTag_Value varchar,
	--LargeBrandedBannerImageImapScript_Default__ varchar,
	--LargeBrandedBannerImageImapScript_ETag varchar,
	---- Youtube V3:  Has list of background image as List<LocalizedProperty>, so, only using the first entry.
	--LargeBrandedBannerImageImapScript_Language varchar,
	--LargeBrandedBannerImageImapScript_ETag varchar,
	--LargeBrandedBannerImageImapScript_Localized varchar,

	---- LargeBrandedBannerImageUrl:  LocalizedProperty
	--LargeBrandedBannerImageUrl varchar,
	--LargeBrandedBannerImageUrl_LanguageTag_ETag varchar,
	--LargeBrandedBannerImageUrl_LanguageTag_Value varchar,
	--LargeBrandedBannerImageUrl_Default__ varchar,
	--LargeBrandedBannerImageUrl_ETag varchar,
	---- Youtube V3:  Has list of background image as List<LocalizedProperty>, so, only using the first entry.
	--LargeBrandedBannerImageUrl_Language varchar,
	--LargeBrandedBannerImageUrl_ETag varchar,
	--LargeBrandedBannerImageUrl_Localized varchar,

	---- SmallBrandedBannerImageImapScript:  LocalizedProperty
	--SmallBrandedBannerImageImapScript varchar,
	--SmallBrandedBannerImageImapScript_LanguageTag_ETag varchar,
	--SmallBrandedBannerImageImapScript_LanguageTag_Value varchar,
	--SmallBrandedBannerImageImapScript_Default__ varchar,
	--SmallBrandedBannerImageImapScript_ETag varchar,
	---- Youtube V3:  Has list of background image as List<LocalizedProperty>, so, only using the first entry.
	--SmallBrandedBannerImageImapScript_Language varchar,
	--SmallBrandedBannerImageImapScript_ETag varchar,
	--SmallBrandedBannerImageImapScript_Localized varchar,

	TrackingImageUrl varchar,
	WatchIconImageUrl varchar,
);
GO

Create Table Youtube_Channel(
	Our_ChannelAuditDetailsId int NOT NULL,
	Our_ChannelBrandingSettingsId int NOT NULL,
	Our_SnippetId int NOT NULL,
	ETag varchar,
	Id varchar NOT NULL,
	Kind varchar,

	Primary Key (Id),
	Foreign Key (Our_ChannelAuditDetailsId) References Youtube_ChannelAuditDetails(Our_Id),
	Foreign Key (Our_ChannelBrandingSettingsId) References Youtube_ChannelBrandingSettings(Our_Id),
	Foreign Key (Our_SnippetId) References Youtube_ChannelAuditDetails(Our_Id),

	-- ContentOwner: ContentOwnerDetails
	ContentOwnerDetails_ContentOwner varchar,	-- Primary Handle location (to create user table)
	ContentOwnerDetails_ETag varchar,
	ContentOwnerDetails_TimeLinked datetime,
	ContentOwnerDetails_TimeLinkedDateTimeOffset datetimeoffset,
	ContentOwnerDetails_TimeLinkedRaw varchar,

	-- Localizations varchar, -- Dictionary<string, ChannelLocalization>

	-- Statistics: ChannelStatistics
	Statistics_CommentCount bigint null,
	Statistics_ETag varchar,
	Statistics_HiddenSubscriberCount bit null,
	Statistics_SubscriberCount bigint null,
	Statistics_VideoCount bigint null,
	Statistics_ViewCount bigint null,

	-- Status: ChannelStatus
	Status_ETag varchar,
	Status_IsLinked bit null,
	Status_LongUploadsStatus varchar,
	Status_MadeForKids bit null,
	Status_PrivacyStatus varchar,
	Status_SelfDeclaredMadeForKids bit null,
	
	-- TopicDetails: ChannelTopicDetails
	-- TopicDetails varchar, --ChannelTopicDetails
);
GO

Create Table Youtube_ChannelConversationPing(
	Our_Id int NOT NULL IDENTITY(1,1),
	Our_ChannelId varchar NOT NULL,
	Context varchar,
	ConversionUrl varchar,
	ETag varchar,

	Primary Key (Our_Id),
	Foreign Key (Our_ChannelId) References Youtube_Channel(Id)
);
GO