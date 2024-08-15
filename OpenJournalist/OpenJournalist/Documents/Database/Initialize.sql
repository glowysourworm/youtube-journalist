
USE master;
GO

DROP DATABASE IF EXISTS OpenJournalist
GO

CREATE DATABASE OpenJournalist
GO

USE OpenJournalist;
GO

-- Our Table for helping to manage users
Create Table "User"(
	Id int NOT NULL IDENTITY(1,1),
	Handle varchar(50),
	Subsystem varchar(50),
	Comment varchar(max),

	Primary Key (Id)
);
GO

Create Table SocialMediaLink(
	Id int NOT NULL IDENTITY(1,1),
	UserId int NOT NULL,
	Handle varchar(50),
	SocialMediaName varchar(50)

	Primary Key (Id),
	Foreign Key (UserId) References "User"(Id)
);
GO

Create Table UserEmail(
	Id int NOT NULL IDENTITY(1,1),
	UserId int NOT NULL,
	Email varchar(100),
	Comment varchar(max),

	Primary Key (Id),
	Foreign Key (UserId) References "User"(Id)
);
GO

Create Table Youtube_SearchResult(

	Our_Id int not null IDENTITY(1,1),
	Id_ChannelId varchar(50),
	Id_VideoId varchar(50),
	Id_PlaylistId varchar(50),
	Snippet_ChannelId varchar(50),
	Snippet_ChannelTitle varchar(max),
	Snippet_Description varchar(max),
	Snippet_PublishedAtDateTimeOffset datetimeoffset null,
	Snippet_Title varchar(max),
	Snippet_ThumbnailDetails_Default__Url varchar(max),
	
	Primary Key(Our_Id)
);
GO

Create Table Youtube_Channel(

	Id varchar(50) NOT NULL,

	-- ChannelAuditDetails
	CommunityGuidelinesGoodStanding bit null,
	ContentIdClaimsGoodStanding bit null,
	CopyrightStrikesGoodStanding bit null,

	-- ChannelBrandingSettings
	BannerExternalUrl varchar(max),
	BannerImageUrl varchar(max),

	-- ContentDetails: ChannelContentDetails
	ChannelContentDetails_RelatedPlaylistsData_Favorites varchar(500),
	ChannelContentDetails_RelatedPlaylistsData_Likes varchar(500),
	ChannelContentDetails_RelatedPlaylistsData_Uploads varchar(500),
	ChannelContentDetails_RelatedPlaylistsData_WatchHistory varchar(500),
	ChannelContentDetails_RelatedPlaylistsData_WatchLater varchar(500),

	-- ContentOwner: ContentOwnerDetails
	ContentOwnerDetails_ContentOwner varchar(50),	-- Primary Handle location (to create user table)
	ContentOwnerDetails_TimeLinkedDateTimeOffset datetimeoffset,

	-- Localizations varchar, -- Dictionary<string, ChannelLocalization>

	-- Statistics: ChannelStatistics
	Statistics_CommentCount bigint null,
	Statistics_HiddenSubscriberCount bit null,
	Statistics_SubscriberCount bigint null,
	Statistics_VideoCount bigint null,
	Statistics_ViewCount bigint null,

	-- Status: ChannelStatus
	Status_MadeForKids bit null,
	Status_PrivacyStatus varchar(50),
	Status_SelfDeclaredMadeForKids bit null,

	Primary Key (Id)
);
GO

Create Table Youtube_ChannelSettings(

	ChannelId varchar(50) not null,
	Country varchar(50),
	DefaultLanguage varchar(50),
	"Description" varchar(max),
	ProfileColor varchar(50),
	TrackingAnalyticsAccountId varchar(50),
	Title varchar(max)

	Primary Key(ChannelId),
	Foreign Key(ChannelId) References Youtube_Channel
);
GO

Create Table Youtube_ChannelSnippet(

	ChannelId varchar(50) not null,
	Country varchar(50),
	CustomUrl varchar(max),
	DefaultLanguage varchar(50),
	"Description" varchar(max),
	Localized_Description varchar(max),		-- NOTE: Different entity than other snippets "ChannelLocalization"
	Localized_Title varchar(max),			-- NOTE: Different entity than other snippets "ChannelLocalization"
	PublishedAtDateTimeOffset datetimeoffset null,
	ThumbnailDetails_Default__Url varchar(50),
	Title varchar(max),

	Primary Key(ChannelId),
	Foreign Key(ChannelId) References Youtube_Channel(Id),

);

Create Table Youtube_Video(

	Id varchar(50) not null,

	VideoSnippet_CategoryId varchar(50),
	VideoSnippet_ChannelId varchar(50),
	VideoSnippet_DefaultLanguage varchar(50),
	VideoSnippet_Localized_Description varchar(max),
	VideoSnippet_Localized_Title varchar(max),
	VideoSnippet_PublishedAtDateTimeOffset datetimeoffset null,
	VideoSnippet_ThumbnailDetails_Default_Url varchar(50),

	-- ContentDetails: VideoContentDetails
	VideoSnippet_ContentDetails_Caption varchar(max),

	-- MonetizationDetails: VideoMonetizationDetails
	MonetizationDetails_AccessPolicy_Allowed bit null, -- (skipped some of the region / localization detail)

	Primary Key(Id),
	Foreign Key(VideoSnippet_ChannelId) References Youtube_Channel(Id)
);
GO

Create Table Youtube_VideoStatistics(

	VideoId varchar(50) not null,
	CommentCount bigint null,
	DislikeCount bigint null,
	FavoriteCount bigint not null,
	LikeCount bigint not null,
	ViewCount bigint not null,

	Primary Key(VideoId),
	Foreign Key(VideoId) References Youtube_Video(Id)
);
GO

Create Table Youtube_VideoStatus(

	VideoId varchar(50) not null,
	License varchar(max),
	MadeForKids bit null,
	PrivacyStatus varchar(100),
	PublishAtDateTimeOffset datetimeoffset null,
	RejectionReason varchar(max),
	SelfDeclaredMadeForKids bit null,
	UploadStatus varchar(100),
	"Description" varchar(max),

	Primary Key(VideoId),
	Foreign Key(VideoId) References Youtube_Video(Id)
);
GO


Create Table Youtube_CommentThread(

	Id varchar(50) NOT NULL,
	VideoId varchar(50) not null,
	IsPublic bit null,
	TotalReplyCount bigint null,
	
	Primary Key(Id),
	Foreign Key(VideoId) References Youtube_Video(Id)
);
GO

Create Table Youtube_Comment(

	Id varchar(50) not null,
	CommentThreadId varchar(50) not null,
	AuthorChannelId_Value varchar(50),
	AuthorChannelUrl varchar(8000),
	AuthorDisplayName varchar(100),
	AuthorProfileImageUrl varchar(8000),
	IsTopLevelComment bit not null,
	LikeCount bigint null,
	ModerationStatus varchar(50),
	--ParentId varchar(50) NOT NULL, Not used by Youtube (correctly)
	PublishedAtDateTimeOffset DateTimeOffset,
	TextDisplay varchar(max),
	TextOriginal varchar(max),
	UpdatedAtDateTimeOffset DateTimeOffset

	Primary Key(Id),
	Foreign Key(CommentThreadId) References Youtube_CommentThread(Id)

);
GO

Create Table Youtube_RegionRestriction(
	Our_Id int not null IDENTITY(1,1),
	VideoId varchar(50) not null,
	Region varchar(50),
	Blocked bit not null

	Primary Key(Our_Id),
	Foreign Key(VideoId) References Youtube_Video(Id)
);
GO

Create Table Youtube_Playlist(

	Id varchar(50) not null,
	
	PlaylistSnippet_ChannelId varchar(50) not null,
	PlaylistSnippet_PublishedAtDateTimeOffset datetimeoffset null,
	PlaylistSnippet_Title varchar(max),
	PlaylistSnippet_ThumnailDetails_Default__Url varchar(50),
	
	Primary Key(Id),
	Foreign Key(PlaylistSnippet_ChannelId) References Youtube_Channel(Id),
);
GO

Create Table Youtube_PlaylistItem(
	
	Id varchar(50) not null,
	
	PlaylistContentDetails_Note varchar(max),
	PlaylistContentDetails_VideoId varchar(50) null, -- Nullable because it won't yet be part of the Video data (youtube service issue)
	PlaylistContentDetails_VideoPublishedAtDateTimeOffset datetimeoffset null,
	
	PlaylistItemSnippet_PlaylistId varchar(50) not null,
	PlaylistItemSnippet_Position bigint null,
	PlaylistItemSnippet_PublishedAtDateTimeOffset datetimeoffset null,
	PlaylistItemSnippet_ThumbnailDetails_Default_Url varchar(50),
	PlaylistItemSnippet_Title varchar(max),
	PlaylistItemSnippet_VideoOwnerChannelId varchar(50),
	PlaylistItemSnippet_VideoOwnerChannelTitle varchar(max),
	
	PlaylistItemStatus_PrivacyStatus varchar(50),
	
	Primary Key(Id),
	Foreign Key(PlaylistItemSnippet_PlaylistId) References Youtube_Playlist(Id)
	
);
GO