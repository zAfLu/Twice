using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Twice.ViewModels.Twitter
{
	internal class MediaItem
	{
		public MediaItem( ulong mediaId, byte[] mediaData )
		{
			MediaId = mediaId;
			MediaData = mediaData;
		}

		private static ImageSource LoadImage( byte[] imageData )
		{
			if( imageData == null || imageData.Length == 0 )
			{
				return null;
			}

			var image = new BitmapImage();
			using( var mem = new MemoryStream( imageData ) )
			{
				mem.Position = 0;
				image.BeginInit();
				image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.UriSource = null;
				image.StreamSource = mem;
				image.EndInit();
			}
			image.Freeze();
			return image;
		}

		public Lazy<ImageSource> Image => new Lazy<ImageSource>( () => LoadImage( MediaData ) );
		public ulong MediaId { get; }
		private readonly byte[] MediaData;
	}
}