// This file is part of great Engine NeoAxis!
// Copyright (C) 2006-2016 NeoAxis Group Ltd.
// Visit http://www.Neoaxis.com, to see it!

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Engine;

namespace CADCore.Serialization
{
	/// <summary>
	/// The class allows to store the text information in the hierarchical form.
	/// Supports creation of children and attributes.
	/// </summary>
	public class TextBlock
	{
	    private TextBlock parent;
	    private string name;
	    private string data;
	    private List<TextBlock> children = new List<TextBlock>();
	    private ReadOnlyCollection<TextBlock> childrenAsReadOnly;
	    private List<Attribute> attributes = new List<Attribute>();
	    private ReadOnlyCollection<Attribute> attributesAsReadOnly;

		//

		/// <summary>
		/// Defines <see cref="TextBlock"/> attribute.
		/// </summary>
		public class Attribute
		{
			internal string name;
			internal string value;

			internal Attribute() { }

			/// <summary>
			/// Gets the attribute name.
			/// </summary>
			public string Name
			{
				get { return name; }
			}

			/// <summary>
			/// Gets the attribute value.
			/// </summary>
			public string Value
			{
				get { return value; }
			}

			/// <summary>
			/// Returns a string that represents the current attribute.
			/// </summary>
			/// <returns>A string that represents the current attribute.</returns>
			public override string ToString()
			{
				return string.Format( "Name: \"{0}\", Value \"{1}\"", name, value );
			}
		}

		/// <summary>
		/// It is applied only to creation root blocks. Not for creation of children.
		/// </summary>
		/// <example>Example of creation of the block and filling by data.
		/// <code>
		/// TextBlock block = new TextBlock();
		/// TextBlock childBlock = block.AddChild( "childBlock", "child block data" );
		/// childBlock.SetAttribute( "attribute", "attribute value" );
		/// </code>
		/// </example>
		/// <seealso cref="TextBlock.AddChild(string,string)"/>
		/// <seealso cref="TextBlock.SetAttribute(string,string)"/>
		public TextBlock()
		{
			childrenAsReadOnly = new ReadOnlyCollection<TextBlock>( children );
			attributesAsReadOnly = new ReadOnlyCollection<Attribute>( attributes );
		}

		//Hierarchy

		/// <summary>
		/// Gets the parent block.
		/// </summary>
		public TextBlock Parent
		{
			get { return parent; }
		}

		/// <summary>
		/// Gets or set block name.
		/// </summary>
		public string Name
		{
			get { return name; }
			set
			{
				if( name == value )
					return;
				name = value;

				if( string.IsNullOrEmpty( name ) )
					Log.Fatal( "TextBlock.set Name: \"name\" is null or empty." );
			}
		}

		/// <summary>
		/// Gets or set block string data.
		/// </summary>
		public string Data
		{
			get { return data; }
			set { data = value; }
		}

		/// <summary>
		/// Gets the children collection.
		/// </summary>
		public IList<TextBlock> Children
		{
			get { return childrenAsReadOnly; }
		}

		/// <summary>
		/// Finds child block by name.
		/// </summary>
		/// <param name="name">The block name.</param>
		/// <returns><see cref="TextBlock"/> if the block has been exists; otherwise, <b>null</b>.</returns>
		public TextBlock FindChild( string name )
		{
			for( int n = 0; n < children.Count; n++ )
			{
				TextBlock child = children[ n ];
				if( child.Name == name )
					return child;
			}
			return null;
		}

		/// <summary>
		/// Creates the child block.
		/// </summary>
		/// <param name="name">The block name.</param>
		/// <param name="data">The block data string.</param>
		/// <returns>The child block.</returns>
		/// <remarks>
		/// Names of blocks can repeat.
		/// </remarks>
		public TextBlock AddChild( string name, string data )
		{
			if( string.IsNullOrEmpty( name ) )
				Log.Fatal( "TextBlock.AddChild: \"name\" is null or empty." );

			TextBlock child = new TextBlock();
			child.parent = this;
			child.name = name;
			child.data = data;
			children.Add( child );
			return child;
		}

		/// <summary>
		/// Creates the child block.
		/// </summary>
		/// <param name="name">The block name.</param>
		/// <returns>The child block.</returns>
		/// <remarks>
		/// Names of blocks can repeat.
		/// </remarks>
		public TextBlock AddChild( string name )
		{
			return AddChild( name, "" );
		}

		/// <summary>
		/// Deletes child block.
		/// </summary>
		/// <param name="child">The child block.</param>
		public void DeleteChild( TextBlock child )
		{
			children.Remove( child );
			child.parent = null;
			child.name = "";
			child.data = "";
			child.children = null;
			child.attributes = null;
		}

		/// <summary>
		/// Returns the attribute value by name.
		/// </summary>
		/// <param name="name">The attribute name.</param>
		/// <param name="defaultValue">Default value. If the attribute does not exist that this value will return.</param>
		/// <returns>The attribute value if the attribute exists; otherwise, default value.</returns>
		public string GetAttribute( string name, string defaultValue )
		{
			for( int n = 0; n < attributes.Count; n++ )
			{
				Attribute attribute = attributes[ n ];
				if( attribute.Name == name )
					return attribute.Value;
			}
			return defaultValue;
		}

		/// <summary>
		/// Returns the attribute value by name.
		/// </summary>
		/// <param name="name">The attribute name.</param>
		/// <returns>The attribute value if the attribute exists; otherwise, empty string.</returns>
		public string GetAttribute( string name )
		{
			return GetAttribute( name, "" );
		}

		/// <summary>
		/// Gets the attributes collection.
		/// </summary>
		public IList<Attribute> Attributes
		{
			get { return attributesAsReadOnly; }
		}

		/// <summary>
		/// Checks existence of attribute.
		/// </summary>
		/// <param name="name">The attribute name.</param>
		/// <returns><b>true</b> if the block exists; otherwise, <b>false</b>.</returns>
		public bool IsAttributeExist( string name )
		{
			for( int n = 0; n < attributes.Count; n++ )
			{
				Attribute attribute = attributes[ n ];
				if( attribute.Name == name )
					return true;
			}
			return false;
		}

		/// <summary>
		/// Sets attribute. If such already there is that rewrites him.
		/// </summary>
		/// <param name="name">The attribute name.</param>
		/// <param name="value">The attribute value.</param>
		public void SetAttribute( string name, string value )
		{
			if( string.IsNullOrEmpty( name ) )
				Log.Fatal( "TextBlock.AddChild: \"name\" is null or empty." );
			if( value == null )
				Log.Fatal( "TextBlock.AddChild: \"value\" is null." );

			for( int n = 0; n < attributes.Count; n++ )
			{
				Attribute attribute = attributes[ n ];
				if( attribute.Name == name )
				{
					attribute.value = value;
					return;
				}
			}
			Attribute a = new Attribute();
			a.name = name;
			a.value = value;
			attributes.Add( a );
		}

		/// <summary>
		/// Deletes attribute if he exists.
		/// </summary>
		/// <param name="name">The attribute name.</param>
		public void DeleteAttribute( string name )
		{
			for( int n = 0; n < attributes.Count; n++ )
			{
				if( name == attributes[ n ].name )
				{
					Attribute attribute = attributes[ n ];
					attribute.name = "";
					attribute.value = "";
					attributes.RemoveAt( n );
					return;
				}
			}
		}

		/// <summary>
		/// Deletes all attributes.
		/// </summary>
		public void DeleteAllAttributes()
		{
			attributes.Clear();
		}

		static string TabLevelToString( int level )
		{
			string str = "";
			for( int n = 0; n < level; n++ )
				str += "\t";
			return str;
		}

		static bool IsNeedQuotesForLexeme( string text, bool thisIsAttributeValue )
		{
			if( !thisIsAttributeValue )
			{
				foreach( char c in text )
				{
					bool good = ( c >= 'A' && c <= 'Z' ) || ( c >= 'a' && c <= 'z' ) ||
						( c >= '0' && c <= '9' ) || c == '_' || c == '#' || c == '$';
					if( !good )
						return true;
				}
				return false;
			}
			else
			{
				if( text.Length > 0 )
				{
					if( text[ 0 ] == ' ' || text[ text.Length - 1 ] == ' ' )
						return true;
				}

				foreach( char c in text )
				{
					bool good = ( c >= 'A' && c <= 'Z' ) || ( c >= 'a' && c <= 'z' ) ||
						( c >= '0' && c <= '9' ) || c == '_' || c == '#' || c == '$' || c == '.' ||
						c == ',' || c == '-' || c == '!' || c == '%' || c == '&' || c == '(' ||
						c == ')' || c == '*' || c == '+' || c == '?' || c == '[' || c == ']' ||
						c == '^' || c == '|' || c == '~' || c == ' ';

					if( !good )
						return true;
				}

				return false;
			}
		}

		void DumpToString( StringBuilder builder, int tabLevel )
		{
			string tabPrefix = TabLevelToString( tabLevel );

			if( !string.IsNullOrEmpty( Name ) )
			{
				{
					builder.Append( tabPrefix );

					string v;
					if( IsNeedQuotesForLexeme( Name, false ) )
						v = string.Format( "\"{0}\"", StringUtils.EncodeDelimiterFormatString( Name ) );
					else
						v = Name;
					builder.Append( v );
				}

				if( !string.IsNullOrEmpty( Data ) )
				{
					builder.Append( " " );

					string v;
					if( IsNeedQuotesForLexeme( Data, false ) )
						v = string.Format( "\"{0}\"", StringUtils.EncodeDelimiterFormatString( Data ) );
					else
						v = Data;
					builder.Append( v );
				}

				builder.Append( "\r\n" );
				builder.Append( tabPrefix );
				builder.Append( "{\r\n" );
			}

			foreach( Attribute attribute in attributes )
			{
				string name;
				string value;

				if( IsNeedQuotesForLexeme( attribute.Name, false ) )
				{
					name = string.Format( "\"{0}\"",
						StringUtils.EncodeDelimiterFormatString( attribute.Name ) );
				}
				else
					name = attribute.Name;

				if( IsNeedQuotesForLexeme( attribute.Value, true ) )
				{
					value = string.Format( "\"{0}\"",
						StringUtils.EncodeDelimiterFormatString( attribute.Value ) );
				}
				else
					value = attribute.Value;

				builder.Append( tabPrefix );
				builder.Append( tabLevel != -1 ? "\t" : "" );
				builder.AppendFormat( "{0} = {1}\r\n", name, value );
			}

			foreach( TextBlock child in children )
				child.DumpToString( builder, tabLevel + 1 );

			if( !string.IsNullOrEmpty( Name ) )
			{
				builder.Append( tabPrefix );
				builder.Append( "}\r\n" );
			}
		}

		/// <summary>
		/// Returns a string containing all data about the block and his children.
		/// </summary>
		/// <returns>A string containing all data about the block and his children.</returns>
		/// <remarks>
		/// This method is applied at preservation of data of the block in a file.
		/// </remarks>
		/// <example>Example of preservation of data of the block in a file.
		/// <code>
		/// TextBlock block = ...
		/// StreamWriter writer = new StreamWriter( fileName );
		/// writer.Write( block.DumpToString() );
		/// writer.Close();
		/// </code>
		/// </example>
		/// <seealso cref="TextBlock.Parse(string,out string)"/>
		public string DumpToString()
		{
			StringBuilder builder = new StringBuilder();
			DumpToString( builder, -1 );
			return builder.ToString();
		}

		/// <summary>
		/// Returns a string that represents the current text block.
		/// </summary>
		/// <returns>A string that represents the current text block.</returns>
		public override string ToString()
		{
			string text = string.Format( "Name: \"{0}\"", name );
			if( !string.IsNullOrEmpty( data ) )
				text += string.Format( ", Data: \"{0}\"", data );
			return text;
		}

		/// <summary>
		/// Parses the text with data of the block and his children.
		/// </summary>
		/// <param name="str">The data string.</param>
		/// <param name="errorString">The information on an error.</param>
		/// <returns><see cref="TextBlock"/> if the block has been parsed; otherwise, <b>null</b>.</returns>
		/// <seealso cref="TextBlock.DumpToString()"/>
		/// <remarks>
		/// For convenience of loading of blocks there is auxiliary class <see cref="TextBlockUtils"/>.
		/// </remarks>
		/// <example>Example of loading of data of the block from a stream.
		/// <code>
		/// FileStream stream = ...;
		/// StreamReader streamReader = new StreamReader( stream );
		/// string error;
		/// TextBlock block = TextBlock.Parse( streamReader.ReadToEnd(), out error );
		/// streamReader.Dispose();
		/// </code>
		/// </example>
		public static TextBlock Parse( string str, out string errorString )
		{
			return TextBlockParser.Parse( str, out errorString );
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////

	static class TextBlockParser
	{
	    private static string streamString;
	    private static int streamPosition;
	    private static string error;
	    private static int linePosition;
	    private static TextBlock root;

		static bool StreamEOF
		{
			get { return streamPosition >= streamString.Length; }
		}

		static bool StreamReadChar( out char character )
		{
			if( StreamEOF )
			{
				character = (char)0;
				return false;
			}
			character = streamString[ streamPosition ];
			streamPosition++;
			return true;
		}

		static void StreamSeek( int position )
		{
			streamPosition = position;
		}

		static void Error( string str )
		{
			if( error == null )
				error = string.Format( "{0} (line - {1})", str, linePosition );
		}

		static StringBuilder tempStringBuilder = new StringBuilder();

		static string GetLexeme( bool stopOnlyAtSeparatorOrQuotes, out bool intoQuotes )
		{
			intoQuotes = false;

			StringBuilder lex = tempStringBuilder;
			lex.Length = 0;

			while( true )
			{
				char c;
				if( !StreamReadChar( out c ) )
				{
					if( StreamEOF )
						return lex.ToString().Trim();
					Error( "Unexpected end of file" );
					return "";
				}

				//comments
				if( c == '/' )
				{
					char cc;
					if( !StreamReadChar( out cc ) )
					{
						Error( "Unexpected end of file" );
						return "";
					}

					if( cc == '/' )
					{
						while( true )
						{
							if( !StreamReadChar( out c ) )
							{
								if( StreamEOF )
								{
									c = '\n';
									break;
								}
								Error( "Unexpected end of file" );
								return "";
							}
							if( c == '\n' )
								break;
						}
					}
					else if( cc == '*' )
					{
						char oldChar = (char)0;

						while( true )
						{
							if( !StreamReadChar( out c ) )
							{
								if( StreamEOF )
								{
									c = ';';
									break;
								}
								Error( "Unexpected end of file" );
								return "";
							}

							if( c == '\n' )
								linePosition++;

							if( oldChar == '*' && c == '/' )
							{
								c = ';';
								break;
							}

							oldChar = c;
						}
					}
					else
					{
						StreamSeek( streamPosition - 1 );
					}
				}

				if( c == '\n' )
					linePosition++;

				if( c == '=' || c == '{' || c == '}' )
				{
					if( lex.Length != 0 )
					{
						StreamSeek( streamPosition - 1 );
						return lex.ToString().Trim();
					}
					return c.ToString();
				}

				if( ( !stopOnlyAtSeparatorOrQuotes && ( c <= 32 || c == ';' ) ) ||
					( stopOnlyAtSeparatorOrQuotes && ( c == '\n' || c == '\r' || c == ';' ) ) )
				{
					if( lex.Length != 0 || stopOnlyAtSeparatorOrQuotes )
						return lex.ToString().Trim();
					continue;
				}

				if( c == '"' )
				{
					if( lex.Length != 0 )
					{
						StreamSeek( streamPosition - 1 );
						return lex.ToString().Trim();
					}

					//quotes
					while( true )
					{
						if( !StreamReadChar( out c ) )
						{
							Error( "Unexpected end of file" );
							return "";
						}
						if( c == '\n' )
							linePosition++;

						if( c == '\\' )
						{
							char c2;
							if( !StreamReadChar( out c2 ) )
							{
								Error( "Unexpected end of file" );
								return "";
							}

							string ss = "\\" + c2;
							if( c2 == 'x' )
							{
								for( int z = 0; z < 4; z++ )
								{
									if( !StreamReadChar( out c2 ) )
									{
										Error( "Unexpected end of file" );
										return "";
									}
									ss += c2;
								}
							}
							StringUtils._DecodeDelimiterFormatString( lex, ss );
							//lex.Append( StringUtils.DecodeDelimiterFormatString( ss ) );
							continue;
						}

						if( c == '"' )
						{
							intoQuotes = true;
							return lex.ToString();
						}
						lex.Append( c );
					}

				}

				if( lex.Length == 0 && ( c == ' ' || c == '\t' ) )
					continue;

				lex.Append( c );
			}
		}

		static string GetLexeme( bool stopOnlyAtSeparatorOrQuotes )
		{
			bool intoQuotes;
			return GetLexeme( stopOnlyAtSeparatorOrQuotes, out intoQuotes );
		}

		static bool LoadChild( TextBlock child, bool ifEmptyLexReturnTrue )
		{
			while( true )
			{
				bool lexIntoQuotes;
				string lex = GetLexeme( false, out lexIntoQuotes );
				if( lex.Length == 0 )//if( lex == "" )
				{
					if( ifEmptyLexReturnTrue )
						return true;

					Error( "Unexpected end of file" );
					return false;
				}

				if( lex == "}" )
					return true;

				string lex2 = GetLexeme( false );
				if( lex2.Length == 0 )//if( lex2 == "" )
				{
					Error( "Unexpected end of file" );
					return false;
				}

				if( lex2 == "=" )
				{
					string value = GetLexeme( true );
					child.SetAttribute( lex, value );
					continue;
				}

				if( lex2 == "{" )
				{
					TextBlock c = child.AddChild( lex );
					if( !LoadChild( c, false ) )
						return false;
					continue;
				}

				string lex3 = GetLexeme( false );
				if( lex3.Length == 0 )//if( lex3 == "" )
				{
					Error( "Unexpected end of file" );
					return false;
				}

				if( lex3 == "{" )
				{
					TextBlock c = child.AddChild( lex, lex2 );
					if( !LoadChild( c, false ) )
						return false;
					continue;
				}

				Error( "Invalid file format" );
				return false;
			}
		}

		public static TextBlock Parse( string str, out string errorString )
		{
			if( str == null )
				Log.Fatal( "TextBlock.Parse: \"str\" is null." );

			streamString = str;
			streamPosition = 0;
			error = null;
			linePosition = 1;
			root = new TextBlock();

			bool ret = LoadChild( root, true );
			if( !ret )
			{
				errorString = error;
				return null;
			}
			errorString = "";
			return root;
		}

	}

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
        public static TextBlock LoadFromVirtualFile(string path, out string errorString)
        {
            errorString = null;

            try
            {
                using (Stream stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        string error;
                        TextBlock textBlock = TextBlock.Parse(streamReader.ReadToEnd(), out error);
                        if (textBlock == null)
                        {
                            errorString = string.Format("Parsing text block failed \"{0}\" ({1}).", path, error);
                        }

                        return textBlock;
                    }
                }
            }
            catch (Exception)
            {
                errorString = string.Format("Reading file failed \"{0}\".", path);
                return null;
            }
        }

        /// <summary>
        /// Loads the block from a file of virtual file system.
        /// </summary>
        /// <param name="path">The virtual file path.</param>
        /// <returns><see cref="TextBlock"/> if the block has been loaded; otherwise, <b>null</b>.</returns>
        public static TextBlock LoadFromVirtualFile(string path)
        {
            string errorString;
            TextBlock textBlock = LoadFromVirtualFile(path, out errorString);
            if (textBlock == null)
                Log.Error(errorString);
            return textBlock;
        }

        /// <summary>
        /// Loads the block from a file of real file system.
        /// </summary>
        /// <param name="path">The real file path.</param>
        /// <param name="errorString">The information on an error.</param>
        /// <returns><see cref="TextBlock"/> if the block has been loaded; otherwise, <b>null</b>.</returns>
        public static TextBlock LoadFromRealFile(string path, out string errorString)
        {
            errorString = null;

            try
            {
                using (FileStream stream = new FileStream(path,
                    FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        string error;
                        TextBlock textBlock = TextBlock.Parse(streamReader.ReadToEnd(), out error);
                        if (textBlock == null)
                        {
                            errorString = string.Format("Parsing text block failed \"{0}\" ({1}).", path, error);
                        }

                        return textBlock;
                    }
                }
            }
            catch (Exception)
            {
                errorString = string.Format("Reading file failed \"{0}\".", path);
                return null;
            }
        }

        /// <summary>
        /// Loads the block from a file of real file system.
        /// </summary>
        /// <param name="path">The real file path.</param>
        /// <returns><see cref="TextBlock"/> if the block has been loaded; otherwise, <b>null</b>.</returns>
        public static TextBlock LoadFromRealFile(string path)
        {
            string errorString;
            TextBlock textBlock = LoadFromRealFile(path, out errorString);
            if (textBlock == null)
                Log.Error(errorString);
            return textBlock;
        }

    }
}
