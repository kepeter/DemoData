using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace DemoData
{
	public class DAL
    {
		private static Random _Random = new Random( );
		private static CultureInfo _CultureInfo;
		private static int _SID = 0;
		private static string _BaseName = string.Format( @"{0}\Culture\{{0}}\{{1}}.json", Path.GetDirectoryName( System.Reflection.Assembly.GetEntryAssembly( ).Location ) );
		private static string _MissingResource = "###There is no resource '{0}' in culture '{1}'!";
		private static Dictionary<string, Dictionary<string, string[ ]>> _Resources = new Dictionary<string, Dictionary<string, string[ ]>>( );

		public static void Reset ( string Culture )
		{
			_CultureInfo = string.IsNullOrEmpty( Culture ) ? CultureInfo.GetCultureInfo( "en" ) : CultureInfo.GetCultureInfo( Culture );

			_SID = 0;
		}

		#region Pure randoms

		public static string Number ( int MinLength = 0, int MaxLength = 0 )
		{
			if ( MaxLength == 0 )
			{
				MaxLength = MinLength;
				MinLength = 0;
			}

			return (
				string.Format( "{0}{1}",
					_Random.Next( -1, 1 ) > 0 ? "" : "-",
					Convert.ToString(
						_Random.Next(
							Convert.ToInt32( "1".PadRight( MinLength, '0' ) ),
							Convert.ToInt32( "1".PadRight( MaxLength, '0' ) )
						)
					)
				)
			);
		}

		public static string Range ( int Min = 0, int Max = 0 )
		{
			if ( Max == 0 )
			{
				Max = Min;
				Min = 0;
			}

			return ( Convert.ToString( _Random.Next( Min, Max + 1 ) ) );
		}

		public static string Guid ( )
		{
			return ( System.Guid.NewGuid( ).ToString( ) );
		}

		public static string Alpha ( int MinLength = 0, int MaxLength = 0 )
		{
			if ( MaxLength == 0 )
			{
				MaxLength = MinLength;
				MinLength = 1;
			}

			int nLength = _Random.Next( MinLength, MaxLength + 1 );

			string szChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			string szResult = new string(
				Enumerable.Repeat( szChars, nLength )
						  .Select( szArray => szArray[_Random.Next( szArray.Length )] )
						  .ToArray( ) );

			return ( szResult );
		}

		public static string Ip4 ( )
		{
			return ( Convert.ToString( string.Format( "{0}.{1}.{2}.{3}",
				_Random.Next( 1, 255 ),
				_Random.Next( 0, 256 ),
				_Random.Next( 0, 256 ),
				_Random.Next( 0, 256 ) ) ) );
		}

		public static string Ip6 ( )
		{
			return ( Convert.ToString( string.Format( "{0:X}:{1:X}:{2:X}:{3:X}:{4:X}:{5:X}:{6:X}:{7:X}",
				_Random.Next( 1, Int16.MaxValue + 1 ),
				_Random.Next( 0, Int16.MaxValue + 1 ),
				_Random.Next( 0, Int16.MaxValue + 1 ),
				_Random.Next( 0, Int16.MaxValue + 1 ),
				_Random.Next( 0, Int16.MaxValue + 1 ),
				_Random.Next( 0, Int16.MaxValue + 1 ),
				_Random.Next( 0, Int16.MaxValue + 1 ),
				_Random.Next( 0, Int16.MaxValue + 1 ) ) ) );
		}

		public static string Latitude ( bool Formated = false )
		{
			if ( Formated )
			{
				return ( string.Format( "{0}° {1}'", _Random.Next( -90, 90 ), _Random.Next( 0, 60 ) ) );
			}
			else
			{
				return ( string.Format( "{0}.{1}", _Random.Next( -90, 90 ), _Random.Next( 0, 100 ) ) );
			}
		}

		public static string Longitude ( bool Formated = false )
		{
			if ( Formated )
			{
				return ( string.Format( "{0}° {1}'", _Random.Next( -180, 180 ), _Random.Next( 0, 60 ) ) );
			}
			else
			{
				return ( string.Format( "{0}.{1}", _Random.Next( -180, 180 ), _Random.Next( 0, 100 ) ) );
			}
		}

		public static string Date ( int MaxYear = 9999 )
		{
			int nYear = _Random.Next( 1900, Math.Max( 2000, MaxYear ) + 1 );
			int nMonth = _Random.Next( 1, 13 );
			int nDay = _Random.Next( 1, DateTime.DaysInMonth( nYear, nMonth ) + 1 );

			DateTime oDate = new DateTime(
				nYear,
				nMonth,
				nDay
			);

			return ( oDate.ToString( _CultureInfo.DateTimeFormat.ShortDatePattern ) );
		}

		public static string Time ( )
		{
			DateTime oDate = new DateTime( 1, 1, 1, _Random.Next( 0, 24 ), _Random.Next( 0, 60 ), 0 );

			return ( oDate.ToString( _CultureInfo.DateTimeFormat.ShortTimePattern ) );
		}

		public static string Datetime ( int MaxYear = 9999 )
		{
			return ( string.Format( "{0} {1}", Date( MaxYear ), Time( ) ) );
		}

		public static string Sid ( )
		{
			return ( Convert.ToString( ++_SID ) );
		}

		#endregion

		#region Resource handling

		protected static string Resource ( string Name )
		{
			if ( LoadResource( Name ) )
			{
				return ( _Resources[_CultureInfo.Name][Name][_Random.Next( _Resources[_CultureInfo.Name][Name].Length )] );
			}

			return ( string.Format( _MissingResource, Name, _CultureInfo.Name ) );
		}

		private static bool LoadResource ( string Name )
		{
			if ( !File.Exists( string.Format( _BaseName, _CultureInfo.Name, Name ) ) )
			{
				return ( false );
			}
			else
			{
				if ( !_Resources.ContainsKey( _CultureInfo.Name ) )
				{
					_Resources[_CultureInfo.Name] = new Dictionary<string, string[ ]>( );
				}

				if ( !_Resources[_CultureInfo.Name].ContainsKey( Name ) )
				{
					_Resources[_CultureInfo.Name][Name] = JsonConvert.DeserializeObject<string[ ]>(
						System.IO.File.ReadAllText( string.Format( _BaseName, _CultureInfo.Name, Name ) )
					);
				}
			}

			return ( true );
		}

		#endregion

	}
}
