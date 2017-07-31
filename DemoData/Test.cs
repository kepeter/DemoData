using Newtonsoft.Json.Linq;
namespace DemoData
{
	public class Person
	{
		dynamic First;
		dynamic Last;
		void Next ( )
		{
			First = Dataen.Resource( "first" );
			Last = Dataen.Resource( "last" );
		}
		private JObject Record ( )
		{
			Next( );
			return ( new JObject {{"First_Name", string.Format("{0}", First)},
{"Last_Name", string.Format("{0}", Last)},
{"Email", string.Format("{0}.{1}@lazy.com", First, Last)},
{"Age", string.Format("{0}", Dataen.Number(2))},
{"Salary", string.Format("{0}.{1}", Dataen.Number(7), Dataen.Number(2))},} );
		}
		public void WriteResult ( )
		{
			Dataen.Reset( "en" );
			JObject[ ] oRecordSet = new JObject[1000];
			for ( int i = 0; i < 1000; i++ )
			{
				oRecordSet[i] = Record( );
			}
			Export.SetOutput( @"C:\Users\peter\Documents\Visual Studio 2015\Projects\DemoData\DemoData\bin\Debug\results\person.json" );
			Export.ToJson( oRecordSet );
			Export.RestoreOutput( );
		}
	}
	public class Person_Phones
	{
		dynamic First;
		dynamic Last;
		void Next ( )
		{
			First = Dataen.Resource( "first" );
			Last = Dataen.Resource( "last" );
		}
		private JObject Record ( )
		{
			Next( );
			return ( new JObject {{"First_Name", string.Format("{0}", First)},
{"Last_Name", string.Format("{0}", Last)},
{"Email", string.Format("{0}.{1}@lazy.com", First, Last)},
{"Age", string.Format("{0}", Dataen.Number(2))},
{"Salary", string.Format("{0}.{1}", Dataen.Number(7), Dataen.Number(2))},} );
		}
		public void WriteResult ( )
		{
			Dataen.Reset( "en" );
			JObject[ ] oRecordSet = new JObject[1000];
			for ( int i = 0; i < 1000; i++ )
			{
				oRecordSet[i] = Record( );
			}
			Export.SetOutput( @"C:\Users\peter\Documents\Visual Studio 2015\Projects\DemoData\DemoData\bin\Debug\results\person_phones.csv" );
			Export.ToCsv( oRecordSet );
			Export.RestoreOutput( );
		}
	}
	public class Execute
	{
		public static void Run ( )
		{
			Person oPerson = new Person( );
			oPerson.WriteResult( );
			Person_Phones oPerson_Phones = new Person_Phones( );
			oPerson_Phones.WriteResult( );
		}
	}
}
