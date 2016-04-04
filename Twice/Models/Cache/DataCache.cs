﻿using Akavache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Twice.Models.Cache
{
	internal class DataCache : IDataCache
	{
		public DataCache( ISecureBlobCache secure, IBlobCache data )
		{
			Secure = secure;
			Cache = data;
		}

		public async Task AddHashtag( string hashtag )
		{
			var data = new HashtagCacheEntry( hashtag );
			await Cache.InsertObject( data.GetKey(), data, DateTimeOffset.Now.Add( Constants.Cache.HashtagExpiration ) );
		}

		public async Task AddUser( ulong id, string name )
		{
			UserCacheEntry data = new UserCacheEntry( id, name );

			await Cache.InsertObject( data.GetKey(), data, DateTimeOffset.Now.Add( Constants.Cache.UserInfoExpiration ) );
		}

		public async Task<IEnumerable<string>> GetKnownHashtags()
		{
			var result = await Cache.GetAllObjects<HashtagCacheEntry>();

			return result.Select( r => r.Hashtag );
		}

		public async Task<IEnumerable<ulong>> GetKnownUserIds()
		{
			var result = await Cache.GetAllObjects<UserCacheEntry>();

			return result.Select( u => u.Id );
		}

		public async Task<IEnumerable<UserCacheEntry>> GetKnownUsers()
		{
			return await Cache.GetAllObjects<UserCacheEntry>();
		}

		public IBlobCache Cache { get; }
		public ISecureBlobCache Secure { get; }
	}
}