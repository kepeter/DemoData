using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CSharp;
using Newtonsoft.Json;

using static DemoData.Command.CommandList;

namespace DemoData
{
	public class Command
	{
#pragma warning disable CS0649
		internal struct CommandList
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

			public struct Relation
			{
				public string Parent;
				public string Child;
			}

			public struct Table
			{
				public string Name;
				public int Rows;
				public Relation[ ] Relations;
				public Table[ ] ChildTables;
				public Column[ ] Columns;
			}

			public bool Compile;
			public Format Output;
			public Table[ ] Tables;
		}
#pragma warning restore CS0649

		private static void WriteTable ( Table Table, StringBuilder Code, List<string> TableName, string Culture )
		{
			Dictionary<string, string> oProperties = new Dictionary<string, string>( );
			bool bNeedNext = false;

			Code.AppendLine( string.Format( "public class {0} {{", Table.Name ) );
			List<string> oLines = new List<string>( );

			TableName.Add( Table.Name );

			foreach ( Column oColumn in Table.Columns )
			{
				Relation[ ] oRelations = Table.Relations?.Where( oRelation => oRelation.Child == oColumn.Name ).ToArray( );

				if ( oRelations?.Length == 1 )
				{
					oLines.Add( string.Format( "{{\"{0}\", Parent[\"{1}\"].Value<string>()}},", Helpers.TextInfo.ToTitleCase( oColumn.Name ), Helpers.TextInfo.ToTitleCase( oRelations[0].Parent ) ) );

					continue;
				}
				else if ( oRelations?.Length > 1 )
				{
					Console.WriteLine( string.Format( "WARNING: More then one relation for the same column ({0}) in tabel ({1})! Ignored...", oColumn.Name, Table.Name ) );
				}

				List<string> oFunc = new List<string>( );
				int nIndex = 0;

				string szFinalFormat = oColumn.Func;

				Match oMatch = Helpers.Resource.Match( oColumn.Func );

				while ( oMatch.Success )
				{
					bNeedNext = true;

					string szKey = Helpers.TextInfo.ToTitleCase( Helpers.Replace( Helpers.SquareToNothingRegex, Helpers.SquareToNothing, oMatch.Value ) );

					if ( !oProperties.ContainsKey( szKey ) )
					{
						oProperties.Add( szKey, string.Format( "Resource({0})", Helpers.Replace( Helpers.SquareToQuoteRegex, Helpers.SquareToQuote, oMatch.Value ) ) );

						Code.AppendLine( string.Format( "private static dynamic {0};", szKey ) );
					}

					oFunc.Add( szKey );

					szFinalFormat = szFinalFormat.Replace( oMatch.Value, string.Format( "{{{0}}}", nIndex++ ) );

					oMatch = oMatch.NextMatch( );
				}

				oMatch = Helpers.Function.Match( oColumn.Func );

				while ( oMatch.Success )
				{
					oFunc.Add( string.Format( "Data{0}.{1}", Culture, Helpers.TextInfo.ToTitleCase( string.Format( "{0}", oMatch.Value.Replace( "<", "" ).Replace( ">", "" ) ) ) ) );

					szFinalFormat = szFinalFormat.Replace( oMatch.Value, string.Format( "{{{0}}}", nIndex++ ) );

					oMatch = oMatch.NextMatch( );
				}

				if ( nIndex > 0 )
				{
					oLines.Add( string.Format( "{{\"{0}\", string.Format(\"{1}\", {2})}},", Helpers.TextInfo.ToTitleCase( oColumn.Name ), szFinalFormat, string.Join( ", ", oFunc.ToArray( ) ) ) );
				}
				else
				{
					oLines.Add( string.Format( "{{\"{0}\", \"{1}\"}},", Helpers.TextInfo.ToTitleCase( oColumn.Name ), szFinalFormat ) );
				}
			}

			if ( bNeedNext )
			{
				Code.AppendLine( "private static void Next() {" );
				foreach ( KeyValuePair<string, string> oProperty in oProperties )
				{
					Code.AppendLine( string.Format( "{0} = Data{1}.{2};", oProperty.Key, Culture, oProperty.Value ) );
				}
				Code.AppendLine( "}" );
			}

			Code.AppendLine( "private static JObject Record( JObject Parent = null ) {" );
			if ( bNeedNext )
			{
				Code.AppendLine( "Next();" );
			}

			string szObject = string.Join( Environment.NewLine, oLines.ToArray( ) );
			szObject = string.Format( "return( new JObject {{{0}}} );", szObject );

			Code.AppendLine( szObject );

			Code.AppendLine( "}" );

			Code.AppendLine( "public static void LoadData ( Dictionary<string, Stack> Storage, JObject Parent = null ) {" );
			Code.AppendLine( "Data.PushSID();" );
			Code.AppendLine( string.Format( "for (int i = 0; i < {0}; i++ ) {{", Table.Rows ) );
			Code.AppendLine( string.Format( "Storage[\"{0}\"].Push( Record( Parent ) );", Table.Name ) );

			foreach ( Table oChild in Table.ChildTables ?? Enumerable.Empty<Table>( ) )
			{
				Code.AppendLine( string.Format( "{0}.LoadData( Storage, ( JObject )Storage[\"{1}\"].Peek( ) );", oChild.Name, Table.Name ) );
			}

			Code.AppendLine( "}" );
			Code.AppendLine( "Data.PopSID();" );
			Code.AppendLine( "}" );

			Code.AppendLine( "}" );

			foreach ( Table oChild in Table.ChildTables ?? Enumerable.Empty<Table>( ) )
			{
				WriteTable( oChild, Code, TableName, Culture );
			}
		}

		public static bool Execute ( string CommandFile, string Culture )
		{
			StringBuilder oCode = new StringBuilder( );
			List<string> oTableNames = new List<string>( );
			string szFile = string.Format( @"{0}\{1}", Helpers.Root, CommandFile );

			if ( !File.Exists( szFile ) )
			{
				Console.WriteLine( "ERROR: Can not find command file!" );

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

			oCode.AppendLine( "using System;" );
			oCode.AppendLine( "using System.Collections;" );
			oCode.AppendLine( "using System.Collections.Generic;" );
			oCode.AppendLine( "using Newtonsoft.Json.Linq;" );

			oCode.AppendLine( "namespace DemoData {" );

			foreach ( Table oTable in oCommand.Tables )
			{
				WriteTable( oTable, oCode, oTableNames, Culture );
			}

			oCode.AppendLine( "public class Execute {" );
			oCode.AppendLine( "public static void Run() {" );

			oCode.AppendLine( "Dictionary<string, Stack> oStorage = new Dictionary<string, Stack>( );" );
			oCode.AppendLine( string.Format( "Data.Reset( \"{0}\" );", Culture ) );

			foreach ( string szTable in oTableNames )
			{
				oCode.AppendLine( string.Format( "oStorage.Add( \"{0}\", new Stack( ) );", szTable ) );
			}

			foreach ( Table oTable in oCommand.Tables )
			{
				oCode.AppendLine( string.Format( "{0}.LoadData( oStorage );", oTable.Name ) );
			}

			oCode.AppendLine( "foreach ( KeyValuePair<string, Stack> oTable in oStorage ) {" );
			oCode.AppendLine( string.Format( "Export.SetOutput( string.Format( @\"C:\\Users\\peter\\Source\\Repos\\demodata\\DemoData\\bin\\Debug\\results\\{{0}}.{0}\", oTable.Key ) );", oCommand.Output ) );
			oCode.AppendLine( "Export.ToCsv(Array.ConvertAll(oTable.Value.ToArray(), oItem => (JObject)oItem));" );
			oCode.AppendLine( "Export.RestoreOutput( );" );
			oCode.AppendLine( "}" );

			oCode.AppendLine( "}" );
			oCode.AppendLine( "}" );

			oCode.AppendLine( "}" );

			string szCode = oCode.ToString( );

			CSharpCodeProvider oCodeProvider = new CSharpCodeProvider( );
			CompilerParameters oParameters = new CompilerParameters( );

			oParameters.ReferencedAssemblies.Add( "System.dll" );
			oParameters.ReferencedAssemblies.Add( "System.Core.dll" );
			oParameters.ReferencedAssemblies.Add( "Microsoft.CSharp.dll" );
			oParameters.ReferencedAssemblies.Add( "Newtonsoft.Json.dll" );
			oParameters.ReferencedAssemblies.Add( "Data.dll" );
			oParameters.ReferencedAssemblies.Add( "Export.dll" );
			oParameters.ReferencedAssemblies.Add( string.Format( "Data{0}.dll", Culture ) );

			oParameters.GenerateInMemory = true;
			oParameters.GenerateExecutable = false;
			oParameters.MainClass = "Execute";

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

			var oType = oResults.CompiledAssembly.GetType( "DemoData.Execute" );

			if ( oType == null )
			{
				Console.WriteLine( "ERROR: Compiled Assembly does not contain a class for Execute..." );

				return ( false );
			}

			oType.GetMethod( "Run" ).Invoke( null, null );

			Console.WriteLine( "  ...done..." );

			return ( true );
		}
	}
}
