using Anotar.NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Twice.Utilities;

namespace Twice.Models.Columns
{
	internal class ColumnDefinitionList : IColumnDefinitionList
	{
		public ColumnDefinitionList( string fileName )
		{
			FileName = fileName;
		}

		public event EventHandler ColumnsChanged;

		public void AddColumns( IEnumerable<ColumnDefinition> newColumns )
		{
			var columns = Load();
			var columnsToAdd = newColumns.Where( c => !columns.Contains( c ) );

			Save( columns.Concat( columnsToAdd ) );
		}

		public IEnumerable<ColumnDefinition> Load()
		{
			if( !File.Exists( FileName ) )
			{
				LogTo.Info( "Column configuration file does not exist. Not loading any columns" );
				return Enumerable.Empty<ColumnDefinition>();
			}

			var json = File.ReadAllText( FileName );
			var loaded = Serializer.Deserialize<List<ColumnDefinition>>( json ) ?? new List<ColumnDefinition>();
			LogTo.Info( $"Loaded {loaded.Count} columns from config" );
			return loaded;
		}

		public void RaiseChanged()
		{
			ColumnsChanged?.Invoke( this, EventArgs.Empty );
		}

		public void Remove( IEnumerable<ColumnDefinition> columnDefinitions )
		{
			var columns = Load().Except( columnDefinitions );

			Save( columns );
		}

		public void Save( IEnumerable<ColumnDefinition> definitions )
		{
			Update( definitions );

			RaiseChanged();
		}

		public void Update( IEnumerable<ColumnDefinition> definitions )
		{
			var json = Serializer.Serialize( definitions.ToList() );
			File.WriteAllText( FileName, json );
		}

		public ISerializer Serializer { get; set; }
		private readonly string FileName;
	}
}