﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Twice
{
	/// <summary>
	///     Class containing constants that are used throughout the application.
	/// </summary>
	[ExcludeFromCodeCoverage]
	internal static partial class Constants
	{
		public static class Gui
		{
			/// <summary>
			///     Number of user pictures for retweets to display in the detail dialog
			/// </summary>
			internal const int MaxRetweets = 10;
		}

		public static class Cache
		{
			internal static TimeSpan UserInfoExpiration = TimeSpan.FromDays( 100 );
			internal static TimeSpan HashtagExpiration = TimeSpan.FromDays( 30 );
		}

		public static class IO
		{
			private static string P( string file )
			{
#if DEBUG
				return file;
#else
				return Path.Combine( AppDataFolder, file );
#endif
			}

			internal static readonly string CacheFileName = P( "cache.db3" );
			internal static readonly string AccountsFileName = P( "accounts.json" );
			internal static readonly string ColumnDefintionFileName = P( "columns.json" );
			internal static readonly string ConfigFileName = P( "config.json" );

			internal static string AppDataFolder
			{
				get
				{
					var localAppData = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );

					var path = Path.Combine( localAppData, "Twice", "data" );
					if( !Directory.Exists( path ) )
					{
						Directory.CreateDirectory( path );
					}
					return path;
				}
			}
		}

		public static class Updates
		{
			internal const string ReleaseChannelUrl = "http://software.btbsoft.org/twice";
			internal const string BetaChannelUrl = "http://software.btbsoft.org/twice/beta";
		}

		/// <summary>
		///     Constants associated with twitter.
		/// </summary>
		public static class Twitter
		{
			/// <summary>
			///     Prefix for a hashtag.
			/// </summary>
			internal const string HashTag = "#";

			/// <summary>
			///     Maximum characters allowed in a tweet.
			/// </summary>
			internal const int MaxTweetLength = 140;

			/// <summary>
			///     Prefix for a user mention.
			/// </summary>
			internal const string Mention = "@";

			public static class ErrorCodes
			{
				public const int CouldNotAuthenticateYou = 32;
				public const int PageDoesNotExist = 34;
				public const int AccountSuspended = 64;
				public const int RateLimitExceeded = 88;
				public const int InvalidOrExpiredToken = 89;
				public const int OverCapacity = 130;
				public const int InternalError = 131;
				public const int CouldNotAuthenticateYouTimestamp = 135;
				public const int UnableToFollowMorePeople = 161;
				public const int NotAuthorizedToSeePost = 179;
				public const int OverDailyStatusUpdateLimit = 185;
				public const int StatusDuplicate = 187;
			}
		}
	}
}