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
