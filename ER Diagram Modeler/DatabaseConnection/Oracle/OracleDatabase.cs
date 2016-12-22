using Oracle.ManagedDataAccess.Client;

namespace ER_Diagram_Modeler.DatabaseConnection.Oracle
{
	public class OracleDatabase: DatabaseProxy<OracleCommand, OracleDataReader>
	{
		public OracleConnection Connection { get; set; }
		public override string ConnectionString { get; set; }

		public override bool Connect()
		{
			return true;
		}

		public override bool Connect(string conString)
		{
			return true;
		}

		public override void Close()
		{
			
		}

		public override int ExecuteNonQuery(OracleCommand command)
		{
			throw new System.NotImplementedException();
		}

		public override OracleCommand CreateCommand(string sql)
		{
			throw new System.NotImplementedException();
		}

		public override OracleDataReader Select(OracleCommand command)
		{
			throw new System.NotImplementedException();
		}
	}
}