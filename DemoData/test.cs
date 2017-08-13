using Newtonsoft.Json.Linq;

namespace DemoData
{
	public class resources
	{
		public static dynamic First;
		public static dynamic Last;

		public static void Next ( )
		{
			First = Data.Resource( "first" );
			Last = Data.Resource( "last" );
		}
	}

	public class email
	{
		private static JObject Record ( JObject Parent = null )
		{
			resources.Next( );

			JObject oRecord = new JObject {
				{"Person", string.Format("{0}", Data.Sid())},
				{"Id", string.Format("{0}", Data.Sid())},
				{"Email", string.Format("{0}.{1}@lazy.com", resources.First, resources.Last)}
			};

			return ( oRecord );
		}

		public static void WriteResult ( JObject Parent = null )
		{
			Data.Reset( "en" );
			JObject[ ] oRecordSet = new JObject[1];

			for ( int i = 0; i < 1; i++ )
			{
				oRecordSet[i] = Record( );
			}

			Export.SetOutput( @"C:\Users\peter\Source\Repos\demodata\DemoData\bin\Debug\results\email.csv" );
			Export.ToCsv( oRecordSet );
			Export.RestoreOutput( );
		}
	}

	public class person
	{
		private static JObject Record ( JObject Parent = null )
		{
			resources.Next( );

			JObject oRecord = new JObject {
				{"Id", string.Format("{0}", Data.Sid())},
				{"First_Name", string.Format("{0}", resources.First)},
				{"Last_Name", string.Format("{0}", resources.Last)},
				{"Age", string.Format("{0}", Data.Number(2))}
			};

			email.WriteResult( oRecord );

			return ( oRecord );
		}

		public static void WriteResult ( JObject Parent = null )
		{
			Data.Reset( "en" );
			JObject[ ] oRecordSet = new JObject[1000];

			for ( int i = 0; i < 1000; i++ )
			{
				oRecordSet[i] = Record( );
			}

			Export.SetOutput( @"C:\Users\peter\Source\Repos\demodata\DemoData\bin\Debug\results\person.csv" );
			Export.ToCsv( oRecordSet );
			Export.RestoreOutput( );
		}
	}

	public class Execute
	{
		public static void Run ( )
		{
			person.WriteResult( );
		}
	}
}
