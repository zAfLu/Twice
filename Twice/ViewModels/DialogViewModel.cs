﻿using GalaSoft.MvvmLight.CommandWpf;
using Ninject;
using System;
using System.Diagnostics;
using System.Windows.Input;
using Twice.Utilities.Ui;
using Twice.ViewModels.Validation;

namespace Twice.ViewModels
{
	internal class DialogViewModel : ValidationViewModel, IDialogViewModel
	{
		protected virtual bool CanExecuteOkCommand()
		{
			return !HasErrors;
		}

		protected void Close( bool result )
		{
			Dispatcher.CheckBeginInvokeOnUI( () =>
			{
				CloseRequested?.Invoke( this, result
					? CloseEventArgs.Ok
					: CloseEventArgs.Cancel );
			} );
		}

		protected virtual bool OnOk()
		{
			return !HasErrors;
		}

		private void ExecuteCancelCommand()
		{
			Close( false );
		}

		private void ExecuteOkCommand()
		{
			ValidateAll();
			if( OnOk() )
			{
				Close( true );
			}
		}

		public event EventHandler<CloseEventArgs> CloseRequested;

		public ICommand CancelCommand => _CancelCommand ?? ( _CancelCommand = new RelayCommand( ExecuteCancelCommand ) );

		public ICommand OkCommand => _OkCommand ?? ( _OkCommand = new RelayCommand( ExecuteOkCommand, CanExecuteOkCommand ) );

		public string Title
		{
			[DebuggerStepThrough] get { return _Title; }
			set
			{
				if( _Title == value )
				{
					return;
				}

				_Title = value;
				RaisePropertyChanged();
			}
		}

		[Inject]
		public IDispatcher Dispatcher { get; set; }

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		private RelayCommand _CancelCommand;

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		private RelayCommand _OkCommand;

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		private string _Title;
	}
}