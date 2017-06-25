using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DemoData
{
	public class CustomFunction
	{
		public string Name;
		public string Func;
	}

	public class CustomClass
	{
		public string Inherit;
		public CustomFunction[ ] Local;
	}

	public class Compiler
	{
		public static void Compile ( string Culture )
		{
			StringBuilder oCode = new StringBuilder( );
			TextInfo oTextInfo = new CultureInfo( "en", false ).TextInfo;
			string szFile = string.Format( @"{0}\Culture\{1}\func.json", Path.GetDirectoryName( System.Reflection.Assembly.GetEntryAssembly( ).Location ), Culture );
			string szJson = File.ReadAllText( szFile ).ToLower( );
			CustomClass oCustomClass = JsonConvert.DeserializeObject<CustomClass>( szJson );

			oCode.AppendLine( "namespace DemoData {" );
			oCode.AppendLine( string.Format( "public class DAL{0} : DAL{1} {{", Culture, oCustomClass.Inherit ) );

			foreach ( CustomFunction oCustomFunction in oCustomClass.Local )
			{
				// \[.*?\]
				string szFormat = string.Empty;
				string szMethod = string.Empty;
				int nIndex = 0;

				Match oMatch = Regex.Match( oCustomFunction.Func, @"\[.*?\]" );

				//foreach ( string szPart in oCustomFunction.Func )
				//{
				//if ( !string.IsNullOrEmpty( szPart ) )
				//{
				//if ( szPart.StartsWith( "{" ) && szPart.EndsWith( "}" ) )
				//{
				//szFormat += string.Format( "{{{0}}}", nIndex++ );

				//szMethod += Regex.Match( szPart, "[^{}]+" ) + ", ";
				//}
				//else
				//{
				//szFormat += szPart;
				//}
				//}
				//}

				if ( !string.IsNullOrEmpty( szMethod ) )
				{
					szMethod = szMethod.TrimEnd( new char[ ] { ',', ' ' } );
				}

				string szCmd = string.Format( "return(string.Format(\"{0}\", {1}));", szFormat, oTextInfo.ToTitleCase( szMethod ) );

				oCode.AppendLine( string.Format( "public static string {0} () {{{1}}}", oTextInfo.ToTitleCase( oCustomFunction.Name ), szCmd ) );
			}

			oCode.AppendLine( "}" );
			oCode.AppendLine( "}" );

			string szCode = oCode.ToString( );
		}
	}
}

/*
﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
 
namespace ConsoleApplication1
{
	class Program
	{
		static List<string> Func = new List<string>( );
		static int Index = 0;
 
		static dynamic Result ( )
		{
			string szResult = "{3}:{0}@dummy.com-{4}:{1}@dummy.com:{2}@dummy.com";
			while ( szResult.Contains( '{' ) )
			{
				szResult = string.Format( szResult, Get( "last" ), Get( "first" ), Get( "last" ), number( 3 ), number( 6 ) );
			}
			return ( szResult );
		}
 
 
		static string Get ( string name )
		{
			return ( name );
		}
 
		static int number ( int value )
		{
			return ( value );
		}
 
		static void Main ( string[ ] args )
		{
			string szFunc = "<number(3)>:[last]@dummy.com-<number(6)>:[first]@dummy.com:[last]@dummy.com";
			//               3          :last  @dummy.com-6          :first  @dummy.com:last  @dummy.com
 
			Regex oRegResource = new Regex( @"\[(.*?)\]" );
			string szResultResource = oRegResource.Replace( szFunc, new MatchEvaluator( Resource ) );
 
			Regex oRegFunction = new Regex( @"\<(.*?)\>" );
			string szResultFunction = oRegFunction.Replace( szResultResource, new MatchEvaluator( Function ) );
 
			string szResult = string.Format( "string szResult = \"{0}\";", szResultFunction );
			string szLoop = string.Format( "while ( szResult.Contains( '{{' ) ) {{ szResult = string.Format( szResult, {0} ); }}", string.Join( ", ", Func.ToArray( ) ) );
 
			szResult += szLoop;
			szResult += "return( szResult );";
 
			Result( );
		}
 
		static string Resource ( Match Match )
		{
			string szMatch = Match.ToString( );
 
			Func.Add( string.Format( "Get({0})", szMatch.Replace( '[', '"' ).Replace( ']', '"' ) ) );
 
			return ( string.Format( "{{{0}}}", Index++ ) );
		}
 
		static string Function ( Match Match )
		{
			string szMatch = Match.ToString( );
 
			string szFunc = string.Format( "{0}", szMatch.Replace( "<", "" ).Replace( ">", "" ) );
 
			Func.Add( szFunc.Contains( '{' ) ? string.Format( "\"{0}\"", szFunc ) : szFunc );
 
			return ( string.Format( "{{{0}}}", Index++ ) );
		}
	}
}

*/
