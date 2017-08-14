using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DemoData
{
	class Program
	{
		static void Main ( string[ ] args )
		{
			List<string> oArgs = args.ToList( );
			int nIndex;

			PrintHeader( );

			if ( ( oArgs.Count( ) == 0 ) ||
				( !oArgs.Contains( Helpers.Commands.List ) &&
				  !oArgs.Contains( Helpers.Commands.Compile ) &&
				  !oArgs.Contains( Helpers.Commands.Command ) ) )
			{
				PrintHelp( );

				return;
			}

			if ( oArgs.Contains( Helpers.Commands.List ) )
			{
				Console.WriteLine( "Listing cultures..." );

				if ( Helpers.Cultures.Length > 0 )
				{
					foreach ( string szCulture in Helpers.Cultures )
					{
						DirectoryInfo oDir = new DirectoryInfo( szCulture );

						Console.WriteLine( );
						Console.WriteLine( string.Format( "  {0}", oDir.Name ) );
						Console.WriteLine( string.Format( "    Functions definitions file is {0}...", File.Exists( Path.Combine( oDir.FullName, "func.json" ) ) ? "presented" : "missing" ) );
						Console.WriteLine( "    Resources:" );

						foreach ( FileInfo oFile in oDir.GetFiles( ) )
						{
							if ( !oFile.Name.ToLower( ).Equals( "func.json" ) )
							{
								Console.WriteLine( string.Format( "      {0}", oFile.Name.Replace( oFile.Extension, string.Empty ) ) );
							}
						}
					}
				}
				else
				{
					Console.WriteLine( "  ...none found..." );
				}
			}

			if ( oArgs.Contains( Helpers.Commands.Compile ) )
			{
				nIndex = oArgs.IndexOf( Helpers.Commands.Compile ) + 1;

				if ( oArgs.Count > nIndex )
				{
					string szCulture = oArgs[nIndex];

					Console.WriteLine( );
					Console.WriteLine( string.Format( "Compiling culture '{0}'...", szCulture ) );

					if ( !Compiler.Compile( szCulture ) )
					{
						return;
					}
				}
				else
				{
					Console.WriteLine( "ERROR. Missing culture..." );

					return;
				}
			}

			if ( oArgs.Contains( Helpers.Commands.Command ) )
			{
				nIndex = oArgs.IndexOf( Helpers.Commands.Command ) + 2;

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
					Console.WriteLine( "ERROR. Missing command file and/or culture..." );

					return;
				}
			}
		}

		static void PrintHeader ( )
		{
			Console.WriteLine( "DemoData" );
			Console.WriteLine( string.Format( "  Version: {0}", Assembly.GetEntryAssembly( ).GetName( ).Version.ToString( ) ) );

			Console.WriteLine( );
		}

		static void PrintHelp ( )
		{
			Console.WriteLine( "USAGE:" );
			Console.WriteLine( "  DemoData -list | -comp {culture} | -cmd {file} {culture}" );
			Console.WriteLine( );
			Console.WriteLine( "Where:" );
			Console.WriteLine( "  -list                  Lists all the cultures currently exists," );
			Console.WriteLine( "                         including their resources" );
			Console.WriteLine( "  -comp {culture}        Compiles the specified 'culture'" );
			Console.WriteLine( "  -cmd {file} {culture}  Runs the commands in 'file' using 'culture'" );

			Console.WriteLine( );
			Console.WriteLine( "Details: https://www.codeproject.com/Articles/1198666/Demo-data" );
		}
	}
}
