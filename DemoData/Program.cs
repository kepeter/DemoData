using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DemoData
{
	class Program
	{
		static void Main ( string[ ] args )
		{
			List<string> oArgs = args.ToList( );
			int nIndex;

			if ( args.Count( ) == 0 )
			{
				PrintHelp( );

				return;
			}

			if ( oArgs.Contains( "-list" ) )
			{
				// list cultures
			}

			if ( oArgs.Contains( "-comp" ) )
			{
				nIndex = oArgs.IndexOf( "-comp" ) + 1;

				if ( oArgs.Count > nIndex )
				{
					string szCulture = oArgs[nIndex];

					Console.WriteLine( string.Format("Compiling culture '{0}'...", szCulture));

					if(!Compiler.Compile( szCulture ) )
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
		}

		static void PrintHelp ( )
		{
			Console.WriteLine( );
			Console.WriteLine( string.Format( "VERSION: {0}", Assembly.GetExecutingAssembly().GetName().Version.ToString() ) );

			Console.WriteLine( );
			Console.WriteLine( "USAGE:" );
			Console.WriteLine( "\tDemoData");
			Console.WriteLine( "\t\t-list |" );
			Console.WriteLine( "\t\t-comp={culture} |" );
			Console.WriteLine( "\t\t-cmd={file}" );
			Console.WriteLine( );
			Console.WriteLine( "where:" );
			Console.WriteLine( "\t-list\t\t\tLists all the cultures currently exists" );
			Console.WriteLine( "\t-comp={culture}\t\tCompiles the specified 'culture'" );
			Console.WriteLine( "\t-cmd={file}\t\tRun the commands in 'file'" );

			Console.WriteLine( );
		}
	}
}
