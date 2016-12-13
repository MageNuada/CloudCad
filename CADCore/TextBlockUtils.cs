// This file is part of great Engine NeoAxis!
// Copyright (C) 2006-2016 NeoAxis Group Ltd.
// Visit http://www.Neoaxis.com, to see it!

using System;
using System.IO;
using Engine;

namespace CADCore
{
	/// <summary>
	/// Auxiliary class for work with <see cref="TextBlock"/>.
	/// </summary>
	public static class TextBlockUtils
	{
		/// <summary>
		/// Loads the block from a file of virtual file system.
		/// </summary>
		/// <param name="path">The virtual file path.</param>
		/// <param name="errorString">The information on an error.</param>
		/// <returns><see cref="TextBlock"/> if the block has been loaded; otherwise, <b>null</b>.</returns>
		public static TextBlock LoadFromVirtualFile( string path, out string errorString )
		{
			errorString = null;

			try
			{
				using( Stream stream = File.Open( path, FileMode.Open, FileAccess.Read ) )
				{
					using( StreamReader streamReader = new StreamReader( stream ) )
					{
						string error;
						TextBlock textBlock = TextBlock.Parse( streamReader.ReadToEnd(), out error );
						if( textBlock == null )
						{
							errorString = string.Format( "Parsing text block failed \"{0}\" ({1}).", path, error );
						}

						return textBlock;
					}
				}
			}
			catch( Exception )
			{
				errorString = string.Format( "Reading file failed \"{0}\".", path );
				return null;
			}
		}

		/// <summary>
		/// Loads the block from a file of virtual file system.
		/// </summary>
		/// <param name="path">The virtual file path.</param>
		/// <returns><see cref="TextBlock"/> if the block has been loaded; otherwise, <b>null</b>.</returns>
		public static TextBlock LoadFromVirtualFile( string path )
		{
			string errorString;
			TextBlock textBlock = LoadFromVirtualFile( path, out errorString );
			if( textBlock == null )
				Log.Error( errorString );
			return textBlock;
		}

		/// <summary>
		/// Loads the block from a file of real file system.
		/// </summary>
		/// <param name="path">The real file path.</param>
		/// <param name="errorString">The information on an error.</param>
		/// <returns><see cref="TextBlock"/> if the block has been loaded; otherwise, <b>null</b>.</returns>
		public static TextBlock LoadFromRealFile( string path, out string errorString )
		{
			errorString = null;

			try
			{
				using( FileStream stream = new FileStream( path, 
					FileMode.Open, FileAccess.Read, FileShare.Read ) )
				{
					using( StreamReader streamReader = new StreamReader( stream ) )
					{
						string error;
						TextBlock textBlock = TextBlock.Parse( streamReader.ReadToEnd(), out error );
						if( textBlock == null )
						{
							errorString = string.Format( "Parsing text block failed \"{0}\" ({1}).", path, error );
						}

						return textBlock;
					}
				}
			}
			catch( Exception )
			{
				errorString = string.Format( "Reading file failed \"{0}\".", path );
				return null;
			}
		}

		/// <summary>
		/// Loads the block from a file of real file system.
		/// </summary>
		/// <param name="path">The real file path.</param>
		/// <returns><see cref="TextBlock"/> if the block has been loaded; otherwise, <b>null</b>.</returns>
		public static TextBlock LoadFromRealFile( string path )
		{
			string errorString;
			TextBlock textBlock = LoadFromRealFile( path, out errorString );
			if( textBlock == null )
				Log.Error( errorString );
			return textBlock;
		}

	}
}
