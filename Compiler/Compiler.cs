using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CSharp;
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
		static List<string> _Func = new List<string>( );
		static int _Index = 0;
		static TextInfo _TextInfo = new CultureInfo( "en", false ).TextInfo;

		public static bool Compile ( string Culture )
		{
			StringBuilder oCode = new StringBuilder( );
			string szFile = string.Format( @"{0}\Culture\{1}\func.json", Path.GetDirectoryName( System.Reflection.Assembly.GetEntryAssembly( ).Location ), Culture );

			if ( !File.Exists( szFile ) )
			{
				Console.WriteLine( "...ERROR: Can not find culture!" );

				return ( false );
			}

			string szJson = File.ReadAllText( szFile ).ToLower( );
			CustomClass oCustomClass = JsonConvert.DeserializeObject<CustomClass>( szJson );

			oCode.AppendLine( "using System.Linq;" );
			oCode.AppendLine( "namespace DemoData {" );
			oCode.AppendLine( string.Format( "public class DAL{0} : DAL{1} {{", Culture, oCustomClass.Inherit ) );

			foreach ( CustomFunction oCustomFunction in oCustomClass.Local )
			{
				_Func.Clear( );
				_Index = 0;

				Regex oRegResource = new Regex( @"\[(.*?)\]" );
				string szResultResource = oRegResource.Replace( oCustomFunction.Func, new MatchEvaluator( Resource ) );

				Regex oRegFunction = new Regex( @"\<(.*?)\>" );
				string szResultFunction = oRegFunction.Replace( szResultResource, new MatchEvaluator( Function ) );


				string szResult = string.Format( "string szResult = \"{0}\";", szResultFunction );
				string szLoop = string.Format( "while ( szResult.Contains( '{{' ) ) {{ szResult = string.Format( szResult, {0} ); }}", string.Join( ", ", _Func.ToArray( ) ) );

				szResult += szLoop;
				szResult += "return( szResult );";

				oCode.AppendLine( string.Format( "public dynamic {0} () {{", _TextInfo.ToTitleCase( oCustomFunction.Name ) ) );
				oCode.AppendLine( szResult );
				oCode.AppendLine( "}" );
			}

			oCode.AppendLine( "}" );
			oCode.AppendLine( "}" );

			string szCode = oCode.ToString( );

			CSharpCodeProvider provider = new CSharpCodeProvider( );
			CompilerParameters parameters = new CompilerParameters( );

			parameters.ReferencedAssemblies.Add( "System.Core.dll" );
			parameters.ReferencedAssemblies.Add( "DAL.dll" );
			parameters.GenerateInMemory = false;
			parameters.GenerateExecutable = false;
			parameters.OutputAssembly = string.Format( "DAL{0}.dll", Culture );
			parameters.MainClass = string.Format( "DAL{0}", Culture );

			CompilerResults results = provider.CompileAssemblyFromSource( parameters, szCode );

			if ( results.Errors.HasErrors )
			{
				Console.WriteLine( "...there were some errors:" );
				foreach ( CompilerError error in results.Errors )
				{
					Console.WriteLine( String.Format( "\t({0}): {1}", error.ErrorNumber, error.ErrorText ) );
				}

				return ( false );
			}
			else
			{
				Console.WriteLine( "...done." );

				return ( true );
			}
		}


		static string Resource ( Match Match )
		{
			string szMatch = Match.ToString( );

			_Func.Add( string.Format( "Resource({0})", szMatch.Replace( '[', '"' ).Replace( ']', '"' ) ) );

			return ( string.Format( "{{{0}}}", _Index++ ) );
		}

		static string Function ( Match Match )
		{
			string szMatch = Match.ToString( );

			string szFunc = string.Format( "{0}", szMatch.Replace( "<", "" ).Replace( ">", "" ) );
			szFunc = _TextInfo.ToTitleCase( szFunc );

			_Func.Add( szFunc.Contains( '{' ) ? string.Format( "\"{0}\"", szFunc ) : szFunc );

			return ( string.Format( "{{{0}}}", _Index++ ) );
		}
	}
}
