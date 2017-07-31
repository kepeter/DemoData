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

using static DemoData.Command.CommandList;
using static DemoData.Command.CommandList.Table;

namespace DemoData
{
	public class Command
	{
#pragma warning disable CS0649
		internal struct CommandList
		{
			public struct Table
			{
				public enum Format
				{
					CSV,
					JSON,
				}

				public struct Column
				{
					public string Name;
					public string Func;
				}

				public string Name;
				public Format Output;
				public int Rows;
				public Column[ ] Columns;
			}

			public bool Compile;
			public Table[ ] Tables;
		}
#pragma warning restore CS0649

		public static bool Execute ( string CommandFile, string Culture )
		{
			StringBuilder oCode = new StringBuilder( );
			string szFile = string.Format( @"{0}\{1}", Path.GetDirectoryName( System.Reflection.Assembly.GetEntryAssembly( ).Location ), CommandFile );
			TextInfo oTextInfo = new CultureInfo( "en", false ).TextInfo;

			if ( !File.Exists( szFile ) )
			{
				Console.WriteLine( "...ERROR: Can not find command file!" );

				return ( false );
			}

			string szJson = File.ReadAllText( szFile ).ToLower( );
			CommandList oCommand = JsonConvert.DeserializeObject<CommandList>( szJson );

			if ( oCommand.Compile )
			{
				if ( !Compiler.Compile( Culture ) )
				{
					return ( false );
				}
			}

			Regex oRegResource = new Regex( @"\[(.*?)\]" );
			Regex oRegFunction = new Regex( @"\<(.*?)\>" );

			oCode.AppendLine( "using Newtonsoft.Json.Linq;" );
			oCode.AppendLine( "namespace DemoData {" );

			foreach ( Table oTable in oCommand.Tables )
			{
				Dictionary<string, string> oProperties = new Dictionary<string, string>( );

				oCode.AppendLine( string.Format( "public class {0} {{", oTextInfo.ToTitleCase( oTable.Name ) ) );
				List<string> oLines = new List<string>( );

				foreach ( Column oColumn in oTable.Columns )
				{
					List<string> oFunc = new List<string>( );
					int nIndex = 0;

					string szFinalFormat = oColumn.Func;

					Match oMatch = oRegResource.Match( oColumn.Func );

					while ( oMatch.Success )
					{
						string szKey = oTextInfo.ToTitleCase( oMatch.Value.Replace( "[", "" ).Replace( "]", "" ) );

						if ( !oProperties.ContainsKey( szKey ) )
						{
							oProperties.Add( szKey, string.Format( "Resource({0})", oMatch.Value.Replace( '[', '"' ).Replace( ']', '"' ) ) );

							oCode.AppendLine( string.Format( "dynamic {0};", szKey ) );
						}

						oFunc.Add( szKey );

						szFinalFormat = szFinalFormat.Replace( oMatch.Value, string.Format( "{{{0}}}", nIndex++ ) );

						oMatch = oMatch.NextMatch( );
					}

					oMatch = oRegFunction.Match( oColumn.Func );

					while ( oMatch.Success )
					{
						oFunc.Add( string.Format( "Data{0}.{1}", Culture, oTextInfo.ToTitleCase( string.Format( "{0}", oMatch.Value.Replace( "<", "" ).Replace( ">", "" ) ) ) ) );

						szFinalFormat = szFinalFormat.Replace( oMatch.Value, string.Format( "{{{0}}}", nIndex++ ) );

						oMatch = oMatch.NextMatch( );
					}

					if ( szFinalFormat.Contains( '{' ) )
					{
						oLines.Add( string.Format( "{{\"{0}\", string.Format(\"{1}\", {2})}},", oTextInfo.ToTitleCase( oColumn.Name ), szFinalFormat, string.Join( ", ", oFunc.ToArray( ) ) ) );
					}
				}

				oCode.AppendLine( "void Next() {" );
				foreach ( KeyValuePair<string, string> oProperty in oProperties )
				{
					oCode.AppendLine( string.Format( "{0} = Data{1}.{2};", oProperty.Key, Culture, oProperty.Value ) );
				}
				oCode.AppendLine( "}" );

				oCode.AppendLine( "private JObject Record() {" );
				oCode.AppendLine( "Next();" );

				string szResultPath = string.Format( @"{0}\results\{1}.{2}", Path.GetDirectoryName( System.Reflection.Assembly.GetEntryAssembly( ).Location ), oTable.Name, oTable.Output == Format.CSV ? "csv" : "json" );

				string szObject = string.Join( Environment.NewLine, oLines.ToArray( ) );
				szObject = string.Format( "return( new JObject {{{0}}} );", szObject );

				oCode.AppendLine( szObject );

				oCode.AppendLine( "}" );

				oCode.AppendLine( "public void WriteResult() {" );
				oCode.AppendLine( string.Format( "Data{1}.Reset( \"{1}\" ); JObject[] oRecordSet = new JObject[{0}]; for (int i = 0; i < {0}; i++ ) {{ oRecordSet[i] = Record(); }} ", oTable.Rows, Culture ) );
				oCode.AppendLine( string.Format( "Export.SetOutput(@\"{0}\");", szResultPath ) );
				oCode.AppendLine( string.Format( "Export.To{0}(oRecordSet);", oTable.Output == Format.CSV ? "Csv" : "Json" ) );
				oCode.AppendLine( "Export.RestoreOutput();" );
				oCode.AppendLine( "}" );

				oCode.AppendLine( "}" );
			}

			oCode.AppendLine( "public class Execute {" );
			oCode.AppendLine( "public static void Run() {" );

			foreach ( Table oTable in oCommand.Tables )
			{
				oCode.AppendLine( string.Format( "{0} o{0} = new {0}( ); o{0}.WriteResult( );", oTextInfo.ToTitleCase( oTable.Name ) ) );
			}

			oCode.AppendLine( "}" );
			oCode.AppendLine( "}" );

			oCode.AppendLine( "}" );

			string szCode = oCode.ToString( );

			CSharpCodeProvider provider = new CSharpCodeProvider( );
			CompilerParameters parameters = new CompilerParameters( );

			parameters.ReferencedAssemblies.Add( "System.dll" );
			parameters.ReferencedAssemblies.Add( "System.Core.dll" );
			parameters.ReferencedAssemblies.Add( "Microsoft.CSharp.dll" );
			parameters.ReferencedAssemblies.Add( "Newtonsoft.Json.dll" );
			parameters.ReferencedAssemblies.Add( "Data.dll" );
			parameters.ReferencedAssemblies.Add( "Export.dll" );
			parameters.ReferencedAssemblies.Add( string.Format( "Data{0}.dll", Culture ));
			parameters.GenerateInMemory = true;
			parameters.GenerateExecutable = false;
			parameters.MainClass = "Execute";

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

			var oType = results.CompiledAssembly.GetType( "DemoData.Execute" );

			if ( oType == null )
			{
				Console.WriteLine( "...ERROR: Compiled Assembly does not contain a class for Execute!" );

				return (false);
			}

			oType.GetMethod( "Run" ).Invoke( null, null );

			Console.WriteLine( "...done." );

			return ( true );
		}
	}
}
