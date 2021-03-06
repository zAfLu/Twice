using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Fody;
using LinqToTwitter;
using Twice.Models.Cache;

namespace Twice.Models.Twitter.Repositories
{
	[ExcludeFromCodeCoverage]
	[ConfigureAwait( false )]
	internal class TwitterUserRepository : TwitterRepositoryBase, ITwitterUserRepository
	{
		public TwitterUserRepository( TwitterContext context, ICache cache )
			: base( context, cache )
		{
		}

		public async Task FollowUser( ulong userId )
		{
			await Context.CreateFriendshipAsync( userId, true );
		}

		public Task<List<User>> LookupUsers( IEnumerable<ulong> userIds )
		{
			var userList = string.Join( ",", userIds );

			return LookupUsers( userList );
		}

		public async Task<List<User>> LookupUsers( string userList )
		{
			if( string.IsNullOrEmpty( userList ) )
			{
				return new List<User>();
			}

			var users = await
				Queryable.Where( s => s.Type == UserType.Lookup && s.UserIdList == userList && s.IncludeEntities == false )
					.ToListAsync();

			await Cache.AddUsers( users.Select( u => new UserCacheEntry( u ) ).ToList() );
			return users;
		}

		public Task<List<User>> Search( string query )
		{
			return Queryable.Where( s => s.Type == UserType.Search && s.Query == query ).ToListAsync();
		}

		public async Task<User> ShowUser( ulong userId, bool includeEntities )
		{
			var cached = await Cache.GetUser( userId );
			if( cached != null )
			{
				return cached;
			}

			var user = await
				Queryable.Where( s => s.UserID == userId && s.IncludeEntities == includeEntities && s.Type == UserType.Show )
					.SingleOrDefaultAsync();

			await Cache.AddUsers( new[] {new UserCacheEntry( user )} );

			return user;
		}

		public async Task UnfollowUser( ulong userId )
		{
			await Context.DestroyFriendshipAsync( userId );
		}

		public TwitterQueryable<User> Queryable => Context.User;
	}
}