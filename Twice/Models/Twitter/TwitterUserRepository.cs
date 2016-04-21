using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using LinqToTwitter;

namespace Twice.Models.Twitter
{
	[ExcludeFromCodeCoverage]
	internal class TwitterUserRepository : TwitterRepositoryBase, ITwitterUserRepository
	{
		public TwitterUserRepository( TwitterContext context )
			: base( context )
		{
			Queryable = new TwitterQueryableWrapper<User>( context.User );
		}

		public Task<List<User>> LookupUsers( string userList )
		{
			return Queryable.Where( s => s.Type == UserType.Lookup && s.UserIdList == userList && s.IncludeEntities == false ).ToListAsync();
		}

		public Task<User> ShowUser( ulong userId, bool includeEntities )
		{
			return
				Queryable.Where( s => s.UserID == userId && s.IncludeEntities == includeEntities && s.Type == UserType.Show )
					.SingleOrDefaultAsync();
		}

		public ITwitterQueryable<User> Queryable { get; }
	}
}