﻿using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twice.Models.Twitter.Repositories;

namespace Twice.Models.Twitter
{
	internal interface ITwitterContext : IDisposable
	{
		Task<User> CreateBlockAsync( ulong userId, string screenName, bool skipStatus );

		Task<Status> CreateFavoriteAsync( ulong statusId );

		Task<Status> DeleteTweetAsync( ulong statusId );

		Task<Status> DestroyFavoriteAsync( ulong statusId );

		Task<LinqToTwitter.Configuration> GetConfig();

		Task LogCurrentRateLimits();

		Task ReportAsSpam( ulong userId );

		Task<Status> RetweetAsync( ulong statusId );

		Task<Status> TweetAsync( string text, IEnumerable<ulong> medias, ulong inReplyTo = 0 );

		Task<LinqToTwitter.Media> UploadMediaAsync( byte[] mediaData, string mediaType, IEnumerable<ulong> additionalOwners );

		Task<bool> VerifyCredentials();

		IAuthorizer Authorizer { get; }
		ITwitterFriendshipRepository Friendships { get; }
		ITwitterSearchRepository Search { get; }
		ITwitterStatusRepository Statuses { get; }
		ITwitterStreamingRepository Streaming { get; }
		ITwitterUserRepository Users { get; }
	}
}