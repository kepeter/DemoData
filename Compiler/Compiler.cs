using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
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
			StringBuilder oCode = new StringBuilder( );
			string szFile = string.Format( @"{0}\Culture\{1}\func.json", Path.GetDirectoryName( System.Reflection.Assembly.GetEntryAssembly( ).Location ), Culture );
			TextInfo oTextInfo = new CultureInfo( "en", false ).TextInfo;

			if ( !File.Exists( szFile ) )
			{
				Console.WriteLine( "...ERROR: Can not find culture!" );

				return ( false );
			}

			string szJson = File.ReadAllText( szFile ).ToLower( );
			Culture oCustom = JsonConvert.DeserializeObject<Culture>( szJson );

			Regex oRegResource = new Regex( @"\[(.*?)\]" );
			Regex oRegFunction = new Regex( @"\<(.*?)\>" );

			oCode.AppendLine( "using System.Linq;" );
			oCode.AppendLine( "namespace DemoData {" );
			oCode.AppendLine( string.Format( "public class Data{0} : Data{1} {{", Culture, oCustom.Inherit ) );

			foreach ( Function oCustomFunction in oCustom.Local )
			{
				List<string> oFunc = new List<string>( );
				int nIndex = 0;

				string szFinalFormat = oCustomFunction.Func;

				Match oMatch = oRegResource.Match( oCustomFunction.Func );

				while ( oMatch.Success )
				{
					oFunc.Add( string.Format( "Resource({0})", oMatch.Value.Replace( '[', '"' ).Replace( ']', '"' ) ) );

					szFinalFormat = szFinalFormat.Replace( oMatch.Value, string.Format( "{{{0}}}", nIndex++ ) );

					oMatch = oMatch.NextMatch( );
				}

				oMatch = oRegFunction.Match( szFinalFormat );

				while ( oMatch.Success )
				{
					oFunc.Add( oTextInfo.ToTitleCase( string.Format( "{0}", oMatch.Value.Replace( "<", "" ).Replace( ">", "" ) ) ) );

					szFinalFormat = szFinalFormat.Replace( oMatch.Value, string.Format( "{{{0}}}", nIndex++ ) );

					oMatch = oMatch.NextMatch( );
				}

				string szResult = string.Format( "return( string.Format( \"{0}\", {1} ) );", szFinalFormat, string.Join( ", ", oFunc.ToArray( ) ) );

				oCode.AppendLine( string.Format( "public dynamic {0} () {{", oTextInfo.ToTitleCase( oCustomFunction.Name ) ) );
				oCode.AppendLine( szResult );
				oCode.AppendLine( "}" );
			}

			oCode.AppendLine( "}" );
			oCode.AppendLine( "}" );

			string szCode = oCode.ToString( );

			CSharpCodeProvider provider = new CSharpCodeProvider( );
			CompilerParameters parameters = new CompilerParameters( );

			parameters.ReferencedAssemblies.Add( "System.Core.dll" );
			parameters.ReferencedAssemblies.Add( "Data.dll" );
			parameters.GenerateInMemory = false;
			parameters.GenerateExecutable = false;
			parameters.OutputAssembly = string.Format( "Data{0}.dll", Culture );
			parameters.MainClass = string.Format( "Data{0}", Culture );

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
	}
}
