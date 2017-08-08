using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CSharp;
using Newtonsoft.Json;

using static DemoData.Compiler.Culture;

namespace DemoData
{
	public class Compiler
	{
#pragma warning disable CS0649
		internal struct Culture
		{
			public struct Function
			{
				public string Name;
				public string Func;
			}

			public string Inherit;
			public Function[ ] Local;
		}
#pragma warning restore CS0649

		public static bool Compile ( string Culture )
		{
			string szFile = string.Format( @"{0}\{1}\func.json", Helpers.CultureRoot, Culture );

			if ( !File.Exists( szFile ) )
			{
				Console.WriteLine( "ERROR: Can not find functions definitions file..." );

				return ( false );
			}

			Culture oCustom = JsonConvert.DeserializeObject<Culture>( File.ReadAllText( szFile ).ToLower( ) );
			StringBuilder oCode = new StringBuilder( );

			oCode.AppendLine( "using System.Linq;" );
			oCode.AppendLine( "namespace DemoData {" );
			oCode.AppendLine( string.Format( "public class Data{0} : Data{1} {{", Culture, oCustom.Inherit ) );

			foreach ( Function oCustomFunction in oCustom.Local )
			{
				List<string> oFunc = new List<string>( );
				int nIndex = 0;

				string szFinalFormat = oCustomFunction.Func;

				Match oMatch = Helpers.Resource.Match( oCustomFunction.Func );

				while ( oMatch.Success )
				{
					oFunc.Add( string.Format( "Resource({0})", Helpers.Replace( Helpers.SquareToQuoteRegex, Helpers.SquareToQuote, oMatch.Value ) ) );

					szFinalFormat = szFinalFormat.Replace( oMatch.Value, string.Format( "{{{0}}}", nIndex++ ) );

					oMatch = oMatch.NextMatch( );
				}

				oMatch = Helpers.Function.Match( szFinalFormat );

				while ( oMatch.Success )
				{
					oFunc.Add( Helpers.TextInfo.ToTitleCase( string.Format( "{0}", Helpers.Replace( Helpers.AngleToNothingRegex, Helpers.AngleToNothing, oMatch.Value ) ) ) );

					szFinalFormat = szFinalFormat.Replace( oMatch.Value, string.Format( "{{{0}}}", nIndex++ ) );

					oMatch = oMatch.NextMatch( );
				}

				string szResult = string.Format( "return( \"{0}\" );", szFinalFormat );

				if ( nIndex > 0 )
				{
					szResult = string.Format( "return( string.Format( \"{0}\", {1} ) );", szFinalFormat, string.Join( ", ", oFunc.ToArray( ) ) );
				}

				oCode.AppendLine( string.Format( "public static dynamic {0} () {{", Helpers.TextInfo.ToTitleCase( oCustomFunction.Name ) ) );
				oCode.AppendLine( szResult );
				oCode.AppendLine( "}" );
			}

			oCode.AppendLine( "}" );
			oCode.AppendLine( "}" );

			string szCode = oCode.ToString( );

			CSharpCodeProvider oCodeProvider = new CSharpCodeProvider( );
			CompilerParameters oParameters = new CompilerParameters( );

			oParameters.ReferencedAssemblies.Add( "System.Core.dll" );
			oParameters.ReferencedAssemblies.Add( string.Format( "Data{0}.dll", oCustom.Inherit ) );

			oParameters.GenerateInMemory = false;
			oParameters.GenerateExecutable = false;
			oParameters.OutputAssembly = string.Format( "Data{0}.dll", Culture );
			oParameters.MainClass = string.Format( "Data{0}", Culture );

			CompilerResults oResults = oCodeProvider.CompileAssemblyFromSource( oParameters, szCode );

			if ( oResults.Errors.HasErrors )
			{
				Console.WriteLine( "There were some errors:" );
				foreach ( CompilerError oError in oResults.Errors )
				{
					Console.WriteLine( String.Format( "  ({0}): {1}, at ({2}, {3})", oError.ErrorNumber, oError.ErrorText, oError.Line, oError.Column ) );
				}

				Helpers.Dump( szCode );

				return ( false );
			}
			else
			{
				Console.WriteLine( "  ...done..." );

				return ( true );
			}
		}
	}
}
