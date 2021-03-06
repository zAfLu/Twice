﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Fody;
using GalaSoft.MvvmLight.CommandWpf;
using LinqToTwitter;
using Ninject;
using Twice.Models.Twitter;
using Twice.Resources;
using Twice.ViewModels.Twitter;

namespace Twice.ViewModels.Profile
{
	[ConfigureAwait( false )]
	internal class ProfileDialogViewModel : DialogViewModel, IProfileDialogViewModel
	{
		public async Task OnLoad( object data )
		{
			if( ProfileId == 0 )
			{
				Close( false );
				return;
			}

			IsBusy = true;
			Context = ContextList.Contexts.First();

			var user = await Context.Twitter.Users.ShowUser( ProfileId, true );
			if( user == null )
			{
				Notifier.DisplayMessage( Strings.UserNotFound, NotificationType.Error );
				Close( false );
				return;
			}

			User = new UserViewModel( user );
			Friendship = await Context.Twitter.Friendships.GetFriendshipWith( Context.UserId, User.UserId );

			UserPages = new List<UserSubPage>
			{
				new UserSubPage( Strings.Tweets, LoadStatuses, LoadMoreStatuses, User.Model.StatusesCount )
				{
					Dispatcher = Dispatcher
				},
				new UserSubPage( Strings.Following, LoadFollowings, User.Model.FriendsCount )
				{
					Dispatcher = Dispatcher
				},
				new UserSubPage( Strings.Followers, LoadFollowers, User.Model.FollowersCount )
				{
					Dispatcher = Dispatcher
				}
			};
			RaisePropertyChanged( nameof( UserPages ) );

			IsBusy = false;
		}

		public void Setup( ulong profileId )
		{
			ProfileId = profileId;
		}

		private async void ExecuteFollowUserCommand()
		{
			await Context.Twitter.Users.FollowUser( User.UserId );
		}

		private async void ExecuteUnfollowUserCommand()
		{
			await Context.Twitter.Users.UnfollowUser( User.UserId );
		}

		private async Task<IEnumerable<object>> LoadFollowers()
		{
			var users = await Context.Twitter.Friendships.ListFollowers( User.UserId );

			return users.Select( u => new UserViewModel( u ) );
		}

		private async Task<IEnumerable<object>> LoadFollowings()
		{
			var users = await Context.Twitter.Friendships.ListFriends( User.UserId );

			return users.Select( u => new UserViewModel( u ) );
		}

		private async Task<IEnumerable<object>> LoadMoreStatuses()
		{
			return await LoadStatuses( MaxId );
		}

		private async Task<IEnumerable<object>> LoadStatuses( ulong? maxId )
		{
			var newStatuses = await Context.Twitter.Statuses.GetUserTweets( User.UserId, 0, maxId ?? 0 );

			var statuses = newStatuses.OrderByDescending( s => s.StatusID ).Select(
				s => new StatusViewModel( s, Context, Configuration, ViewServiceRepository ) ).ToArray();

			if( statuses.Any() )
			{
				MaxId = Math.Min( MaxId, statuses.Min( s => s.Id ) );

				await Task.WhenAll( statuses.Select( s => s.LoadQuotedTweet() ) );
			}
			return statuses;
		}

		private async Task<IEnumerable<object>> LoadStatuses()
		{
			return await LoadStatuses( null );
		}

		public ICommand FollowUserCommand => _FollowUserCommand ?? ( _FollowUserCommand = new RelayCommand(
			ExecuteFollowUserCommand ) );

		public Friendship Friendship
		{
			[DebuggerStepThrough] get { return _Friendship; }
			set
			{
				if( _Friendship == value )
				{
					return;
				}

				_Friendship = value;
				RaisePropertyChanged();
			}
		}

		public bool IsBusy
		{
			[DebuggerStepThrough] get { return _IsBusy; }
			set
			{
				if( _IsBusy == value )
				{
					return;
				}

				_IsBusy = value;
				RaisePropertyChanged();
			}
		}

		[Inject]
		public INotifier Notifier { get; set; }

		public ICommand UnfollowUserCommand => _UnfollowUserCommand ?? ( _UnfollowUserCommand = new RelayCommand(
			ExecuteUnfollowUserCommand ) );

		public UserViewModel User
		{
			[DebuggerStepThrough] get { return _User; }
			set
			{
				if( _User == value )
				{
					return;
				}

				_User = value;
				RaisePropertyChanged();
			}
		}

		public ICollection<UserSubPage> UserPages { get; private set; }
		private RelayCommand _FollowUserCommand;

		[DebuggerBrowsable( DebuggerBrowsableState.Never )] private Friendship _Friendship;

		[DebuggerBrowsable( DebuggerBrowsableState.Never )] private bool _IsBusy;

		private RelayCommand _UnfollowUserCommand;

		[DebuggerBrowsable( DebuggerBrowsableState.Never )] private UserViewModel _User;

		private IContextEntry Context;
		private ulong MaxId = ulong.MaxValue;
		private ulong ProfileId;
	}
}