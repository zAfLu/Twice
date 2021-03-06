﻿using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Twice.Controls
{
	/// <summary>
	///     Interaction logic for FileSelectBox.xaml
	/// </summary>
	[ExcludeFromCodeCoverage]
	internal partial class FileSelectBox
	{
		public FileSelectBox()
		{
			InitializeComponent();
		}

		private void BrowseButton_OnClick( object sender, RoutedEventArgs e )
		{
			switch( Mode )
			{
			case FileSelectMode.Open:
				GetOpenFileName();
				break;

			case FileSelectMode.Save:
				GetSaveFileName();
				break;
			}
		}

		private void GetOpenFileName()
		{
			var dlg = new OpenFileDialog
			{
				CheckFileExists = true,
				FileName = Value
			};

			if( dlg.ShowDialog() == true )
			{
				Value = dlg.FileName;
			}
		}

		private void GetSaveFileName()
		{
			var dlg = new SaveFileDialog
			{
				FileName = Value
			};

			if( dlg.ShowDialog() == true )
			{
				Value = dlg.FileName;
			}
		}

		public static readonly DependencyProperty ModeProperty =
			DependencyProperty.Register( "Mode", typeof(FileSelectMode), typeof(FileSelectBox),
				new PropertyMetadata( FileSelectMode.Open ) );

		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register( "Value", typeof(string),
			typeof(FileSelectBox),
			new PropertyMetadata( null ) );

		public FileSelectMode Mode
		{
			get { return (FileSelectMode)GetValue( ModeProperty ); }
			set { SetValue( ModeProperty, value ); }
		}

		public string Value
		{
			get { return (string)GetValue( ValueProperty ); }
			set { SetValue( ValueProperty, value ); }
		}
	}
}