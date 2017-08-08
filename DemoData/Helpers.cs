using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DemoData
{
	public class Helpers
	{
		public static Dictionary<string, string> SquareToQuote = new Dictionary<string, string> { { "[", "\"" }, { "]", "\"" } };
		public static Dictionary<string, string> SquareToNothing = new Dictionary<string, string> { { "[", string.Empty }, { "]", string.Empty } };
		public static Dictionary<string, string> AngleToNothing = new Dictionary<string, string> { { "<", string.Empty }, { ">", string.Empty } };

		public static Regex SquareToQuoteRegex = new Regex( "(" + string.Join( "|", SquareToQuote.Keys.Select( Regex.Escape ) ) + ")" );
		public static Regex SquareToNothingRegex = new Regex( "(" + string.Join( "|", SquareToNothing.Keys.Select( Regex.Escape ) ) + ")" );
		public static Regex AngleToNothingRegex = new Regex( "(" + string.Join( "|", AngleToNothing.Keys.Select( Regex.Escape ) ) + ")" );

		public struct Commands
		{
			public static string List = "-list";
			public static string Compile = "-comp";
			public static string Command = "-cmd";
		}

		public static string Root = Path.GetDirectoryName( Assembly.GetEntryAssembly( ).Location );
		public static string CultureRoot = string.Format( @"{0}\Culture", Root );
		public static string[ ] Cultures = Directory.GetDirectories( Helpers.CultureRoot );

		public static TextInfo TextInfo = new CultureInfo( "en", false ).TextInfo;

		public static Regex Resource = new Regex( @"\[(.*?)\]" );
		public static Regex Function = new Regex( @"\<(.*?)\>" );

		public static void Dump ( string Code )
		{
			string[ ] szLines = Code.Split( new string[ ] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries );
			int nLine = 1;
			int nTabs = 1;

			Console.WriteLine( );

			foreach ( string szLine in szLines )
			{
				if ( szLine.StartsWith( "}" ) )
				{
					nTabs--;
				}

				Console.WriteLine( string.Format( "{0:D4}{2}{1}", nLine, szLine, new string( ' ', nTabs * 2 ) ) );

				if ( szLine.EndsWith( "{" ) )
				{
					nTabs++;
				}

				nLine++;
			}
		}

		public static string Replace ( Regex Condition, Dictionary<string, string> ReplaceList, string Value )
		{
			return ( Condition.Replace( Value, oMatch => ReplaceList[oMatch.Value] ) );
		}
	}
}
