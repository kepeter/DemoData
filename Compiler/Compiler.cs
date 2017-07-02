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
	public enum CommandOutput
	{
		CSV,
		JSON,
	}

	public struct CommandTableColum
	{
		public string Name;
		public string Func;
	}

	public struct CommandTable
	{
		public string Name;
		public CommandOutput Output;
		public bool Header;
		public int Rows;
		public CommandTableColum[ ] Columns;
	}

	public struct Command
	{
		public bool Compile;
		public CommandTable[ ] Tables;
	}

	public class CommandHandler
	{
		public static bool Execute ( string CommandFile, string Culture )
		{
			StringBuilder oCode = new StringBuilder( );
			string szFile = string.Format( @"{0}\{1}", Path.GetDirectoryName( System.Reflection.Assembly.GetEntryAssembly( ).Location ), CommandFile );
			TextInfo oTextInfo = new CultureInfo( "en", false ).TextInfo;

			if ( string.IsNullOrEmpty( Culture ) )
			{
				Console.WriteLine( "...ERROR: Culture must be defined!" );

				return ( false );
			}

			if ( !File.Exists( szFile ) )
			{
				Console.WriteLine( "...ERROR: Can not find command file!" );

				return ( false );
			}

			string szJson = File.ReadAllText( szFile ).ToLower( );
			Command oCommand = JsonConvert.DeserializeObject<Command>( szJson );

			if ( oCommand.Compile )
			{
				if ( !CultureCompiler.Compile( Culture ) )
				{
					return ( false );
				}
			}

			Regex oRegResource = new Regex( @"\[(.*?)\]" );
			Regex oRegFunction = new Regex( @"\<(.*?)\>" );
			Dictionary<string, string> oProperties = new Dictionary<string, string>( );

			oCode.AppendLine( "using System.Linq;" );
			oCode.AppendLine( "namespace DemoData {" );

			foreach ( CommandTable oTable in oCommand.Tables )
			{
				oCode.AppendLine( string.Format( "public class {0} {{", oTextInfo.ToTitleCase( oTable.Name ) ) );
				List<string> oLines = new List<string>( );
				List<string> oFunc = new List<string>( );
				int nIndex = 0;

				foreach ( CommandTableColum oColumn in oTable.Columns )
				{
					string szFinalFormat = oColumn.Func;

					Match oMatch = oRegResource.Match( oColumn.Func );

					while ( oMatch.Success )
					{
						string szKey = oTextInfo.ToTitleCase( oMatch.Value.Replace( "[", "" ).Replace( "]", "" ) );

						if ( !oProperties.ContainsKey( szKey ) )
						{
							oProperties.Add( szKey, string.Format( "Resource({0})", oMatch.Value.Replace( '[', '"' ).Replace( ']', '"' ) ) );

							oCode.AppendLine( string.Format( "private dynamic _{0};", szKey ) );
							oCode.AppendLine( string.Format( "public dynamic {0} {{ get {{ return ( _{0} ); }} }}", szKey ) );
						}

						oFunc.Add( szKey );

						szFinalFormat = szFinalFormat.Replace( oMatch.Value, string.Format( "{{{0}}}", nIndex++ ) );

						oMatch = oMatch.NextMatch( );
					}

					oMatch = oRegFunction.Match( oColumn.Func );

					while ( oMatch.Success )
					{
						oFunc.Add( oTextInfo.ToTitleCase( string.Format( "{0}", oMatch.Value.Replace( "<", "" ).Replace( ">", "" ) ) ) );

						szFinalFormat = szFinalFormat.Replace( oMatch.Value, string.Format( "{{{0}}}", nIndex++ ) );

						oMatch = oMatch.NextMatch( );
					}

					if ( szFinalFormat.Contains( '{' ) )
					{
						oLines.Add( string.Format( "{0} = string.Format({1}),", oTextInfo.ToTitleCase( oColumn.Name ), szFinalFormat ) );
					}
				}

				oCode.AppendLine( "public void Next() {" );
				foreach ( KeyValuePair<string, string> oProperty in oProperties )
				{
					oCode.AppendLine( string.Format( "_{0} = DAL{1}.{2};", oProperty.Key, Culture, oProperty.Value ) );
				}
				oCode.AppendLine( "}" );

				oCode.AppendLine( "public dynamic Record() {" );

				string szObject = string.Join( Environment.NewLine, oLines.ToArray( ) );
				szObject = string.Format( szObject, oFunc.ToArray( ) );
				szObject = string.Format( "return( new {{{0}}} );", szObject );

				oCode.AppendLine( szObject );

				oCode.AppendLine( "}" );

				oCode.AppendLine( "}" );
			}

			oCode.AppendLine( "}" );

			string szCode = oCode.ToString( );

			return ( true );
		}
	}

	public struct CultureFunction
	{
		public string Name;
		public string Func;
	}

	public struct Culture
	{
		public string Inherit;
		public CultureFunction[ ] Local;
	}

	public class CultureCompiler
	{
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
			oCode.AppendLine( string.Format( "public class DAL{0} : DAL{1} {{", Culture, oCustom.Inherit ) );

			foreach ( CultureFunction oCustomFunction in oCustom.Local )
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
	}
}
