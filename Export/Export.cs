using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DemoData
{
	public class Export
	{
		private static TextWriter _StandardOutput = Console.Out;

		public static void SetOutput ( string Out )
		{
			Directory.CreateDirectory( Path.GetDirectoryName( Out ) );

			Console.Out.Flush( );
			Console.SetOut( new StreamWriter( Out ) );
		}

		public static void RestoreOutput ( )
		{
			Console.Out.Flush( );
			Console.SetOut( _StandardOutput );
		}

		public static void ToJson ( JObject[ ] Set )
		{
			Console.WriteLine( JsonConvert.SerializeObject( Set ) );
		}

		public static void ToCsv ( JObject[ ] Set )
		{
			StringBuilder oSB = new StringBuilder( );
			DataTable oData = JsonConvert.DeserializeObject<DataTable>( JsonConvert.SerializeObject( Set ) );

			IEnumerable<string> szColumns = oData.Columns.Cast<DataColumn>( ).Select( oColumn => oColumn.ColumnName );

			oSB.AppendLine( string.Join( ",", szColumns ) );

			foreach ( DataRow oRow in oData.Rows )
			{
				IEnumerable<string> szValues = oRow.ItemArray.Select( szValue => string.Format( "\"{0}\"", szValue.ToString( ) ) );

				oSB.AppendLine( string.Join( ",", szValues ) );
			}

			Console.WriteLine( oSB.ToString( ) );
		}
	}
}
