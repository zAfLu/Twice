﻿using Anotar.NLog;
using LinqToTwitter;
using LitJson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Twice.Models.Twitter.Streaming
{
	/// <summary>
	///     Parser for twitter streams.
	/// </summary>
	internal class StreamParser : IDisposable, IStreamParser
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="StreamParser" /> class.
		/// </summary>
		/// <param name="stream">The user stream.</param>
		private StreamParser( IStreamingConnection stream )
		{
			Connections = new List<IStreaming>();
			Stream = stream;
		}

		/// <summary>
		///     Creates a new parser for the specified stream.
		/// </summary>
		/// <param name="userStream">The stream.</param>
		/// <returns>The created parser.</returns>
		public static StreamParser Create( IStreamingConnection userStream )
		{
			return new StreamParser( userStream );
		}

		/// <summary>
		///     Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing">
		///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
		///     unmanaged resources.
		/// </param>
		protected void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( Connections != null )
				{
					foreach( var s in Connections )
					{
						s.CloseStream();
					}
				}
			}
		}

		/// <summary>
		///     Handles a twitter event.
		/// </summary>
		/// <param name="json">The json describing the event.</param>
		private void HandleEvent( string json )
		{
			JsonData parsed = JsonMapper.ToObject( json );

			string eventStr = (string)parsed["event"];
			StreamEventType eventType = StreamEventType.Unknown;

			if( !string.IsNullOrWhiteSpace( eventStr ) )
			{
				try
				{
					eventType = (StreamEventType)Enum.Parse( typeof(StreamEventType), eventStr.Replace( "_", string.Empty ), true );
				}
				catch( ArgumentException )
				{
					eventType = StreamEventType.Unknown;
				}
			}

			switch( eventType )
			{
			case StreamEventType.Favorite:
				LogTo.Debug( "Favorite received" );
				var favhandler = FavoriteEventReceived;
				favhandler?.Invoke( this, new FavoriteStreamEventArgs( json ) );
				break;

			default:
				LogTo.Debug( "Unknown event received" );
				var handler = UnknownEventReceived;
				handler?.Invoke( this, new EventStreamEventArgs( json ) );
				break;
			}
		}

		/// <summary>
		///     Parses the content and raises events.
		/// </summary>
		/// <param name="content">The content.</param>
		private void ParseContent( IStreamContent content )
		{
			if( string.IsNullOrEmpty( content.Content ) )
			{
				LogTo.Trace( "Received Keep-Alive on streaming connection" );
				return;
			}

			JsonData parsed = JsonMapper.ToObject( content.Content );
			JsonData temp;

			// Was this a direct message?
			if( parsed.TryGetValue( "recipient", out temp ) && parsed.TryGetValue( "sender", out temp ) )
			{
				LogTo.Debug( "DirectMessage received" );
				var handler = DirectMessageReceived;
				handler?.Invoke( this, new DirectMessageStreamEventArgs( content.Content ) );
			}

			// Was this a tweet?
			else if( parsed.TryGetValue( "text", out temp ) )
			{
				LogTo.Debug( "Status recevied" );
				var handler = StatusReceived;
				handler?.Invoke( this, new StatusStreamEventArgs( content.Content ) );
			}

			// Or has something been deleted?
			else if( parsed.TryGetValue( "delete", out temp ) )
			{
				JsonData deleted;
				if( temp.TryGetValue( "status", out deleted ) )
				{
					LogTo.Debug( "Status deletion received" );
					var handler = StatusDeleted;
					handler?.Invoke( this, new DeleteStreamEventArgs( content.Content ) );
				}
				else
				{
					LogTo.Debug( "DirectMessage deletion received" );
					var handler = DirectMessageDeleted;
					handler?.Invoke( this, new DeleteStreamEventArgs( content.Content ) );
				}
			}

			// Or a different event?
			else if( parsed.TryGetValue( "event", out temp ) )
			{
				HandleEvent( content.Content );
			}

			// Is this the friend list of the user?
			else if( parsed.TryGetValue( "friends", out temp ) )
			{
				LogTo.Debug( "Friend list received" );
				var handler = FriendsReceived;
				handler?.Invoke( this, new FriendsStreamEventArgs( content.Content ) );
			}
			else
			{
				LogTo.Debug( "Unknown data received" );
				var handler = UnknownDataReceived;
				handler?.Invoke( this, new StreamEventArgs( content.Content ) );
			}
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting
		///     unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		///     Occurs when a status was deleted.
		/// </summary>
		public event EventHandler<DeleteStreamEventArgs> DirectMessageDeleted;

		/// <summary>
		///     Occurs when a direct message was received.
		/// </summary>
		public event EventHandler<DirectMessageStreamEventArgs> DirectMessageReceived;

		/// <summary>
		///     Occurs when a status was favourited.
		/// </summary>
		public event EventHandler<FavoriteStreamEventArgs> FavoriteEventReceived;

		/// <summary>
		///     Occurs when the friend list was received.
		/// </summary>
		public event EventHandler<FriendsStreamEventArgs> FriendsReceived;

		/// <summary>
		///     Occurs when a status was deleted.
		/// </summary>
		public event EventHandler<DeleteStreamEventArgs> StatusDeleted;

		/// <summary>
		///     Occurs when a status was received.
		/// </summary>
		public event EventHandler<StatusStreamEventArgs> StatusReceived;

		/// <summary>
		///     Occurs when unknown data has been received.
		/// </summary>
		public event EventHandler<StreamEventArgs> UnknownDataReceived;

		/// <summary>
		///     Occurs when an event was received.
		/// </summary>
		public event EventHandler<EventStreamEventArgs> UnknownEventReceived;

		public void StartStreaming()
		{
			if( Started )
			{
				return;
			}

			LogTo.Info( "Start realtime streaming on connection" );
			Started = true;
			StreamingTask = Stream.Start( c => Task.Run( () => ParseContent( c ) ) )
				.ContinueWith( t => Connections.AddRange( t.Result ) );
		}

		public Task StreamingTask { get; private set; }
		private readonly List<IStreaming> Connections;
		private readonly IStreamingConnection Stream;
		private bool Started;
	}
}