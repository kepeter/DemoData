using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DemoData
{
	class Program
	{
		static void Main ( string[ ] args )
		{
			List<string> oArgs = args.ToList( );
			int nIndex;

			if ( oArgs.Count( ) == 0 )
			{
				PrintHelp( );

				return;
			}

			if ( oArgs.Contains( "-list" ) )
			{
				string[ ] szFolders = Directory.GetDirectories( string.Format( @"{0}\Culture", Path.GetDirectoryName( Assembly.GetEntryAssembly( ).Location ) ) );

				Console.WriteLine( "Listing cultures..." );

				if ( szFolders.Length > 0 )
				{
					foreach ( string szFolder in szFolders )
					{
						DirectoryInfo oDir = new DirectoryInfo( szFolder );

						Console.WriteLine( string.Format( "\t{0}", oDir.Name ) );
						Console.WriteLine( string.Format( "\t\tFunctions definition file is {0}...", File.Exists( Path.Combine( szFolder, "func.json" ) ) ? "presented" : "missing" ) );
						Console.WriteLine( "\t\tResources:" );

						foreach ( FileInfo oFile in oDir.GetFiles( ) )
						{
							if ( !oFile.Name.ToLower( ).Equals( "func.json" ) )
							{
								Console.WriteLine( string.Format( "\t\t\t{0}", oFile.Name.Replace( oFile.Extension, string.Empty ) ) );
							}
						}
					}
				}
				else
				{
					Console.WriteLine( "...none found..." );
				}
			}

			if ( oArgs.Contains( "-comp" ) )
			{
				nIndex = oArgs.IndexOf( "-comp" ) + 1;

				if ( oArgs.Count > nIndex )
				{
					string szCulture = oArgs[nIndex];

					Console.WriteLine( string.Format( "Compiling culture '{0}'...", szCulture ) );

					if ( !Compiler.Compile( szCulture ) )
					{
						return;
					}
				}
				else
				{
					Console.WriteLine( "ERROR. Missing culture for -comp option..." );

					return;
				}
			}

			if ( oArgs.Contains( "-cmd" ) )
			{
				nIndex = oArgs.IndexOf( "-cmd" ) + 2;

				if ( oArgs.Count > nIndex )
				{
					string szCommandFile = oArgs[nIndex - 1];
					string szCulture = oArgs[nIndex];

					Console.WriteLine( string.Format( "Executing command from '{0}', using culture '{1}'...", szCommandFile, szCulture ) );

					if ( !Command.Execute( szCommandFile, szCulture ) )
					{
						return;
					}
				}
				else
				{
					Console.WriteLine( "ERROR. Missing command file or culture for -cmd option..." );

					return;
				}
			}
		}

		static void PrintHelp ( )
		{
			Console.WriteLine( );
			Console.WriteLine( string.Format( "VERSION: {0}", Assembly.GetEntryAssembly( ).GetName( ).Version.ToString( ) ) );

			Console.WriteLine( );
			Console.WriteLine( "USAGE:" );
			Console.WriteLine( "\tDemoData" );
			Console.WriteLine( "\t\t-list |" );
			Console.WriteLine( "\t\t-comp {culture} |" );
			Console.WriteLine( "\t\t-cmd {file} {culture}" );
			Console.WriteLine( );
			Console.WriteLine( "where:" );
			Console.WriteLine( "\t-list\t\t\tLists all the cultures currently exists" );
			Console.WriteLine( "\t-comp {culture}\t\tCompiles the specified 'culture'" );
			Console.WriteLine( "\t-cmd {file} {culture}\t\tRun the commands in 'file'" );

			Console.WriteLine( );
		}
	}
}