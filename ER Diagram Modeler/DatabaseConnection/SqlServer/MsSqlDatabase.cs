using System;
using System.Data;
using System.Data.SqlClient;

namespace ER_Diagram_Modeler.DatabaseConnection.SqlServer
{
	public class MsSqlDatabase: DatabaseProxy<SqlCommand, SqlDataReader>
	{
		public SqlConnection Connection { get; set; }
		public override string ConnectionString { get; set; }

		public MsSqlDatabase()
		{
			Connection = new SqlConnection();
		}

		public override bool Connect()
		{
			return Connect(ConnectionString);
		}

		public override bool Connect(string conString)
		{
			if (Connection.State != ConnectionState.Open)
			{
				Connection.ConnectionString = conString;
				ConnectionString = conString;
				Connection.Open();
				return true;
			}
			return false;
		}

		public override void Close()
		{
			Connection.Close();
		}

		public override int ExecuteNonQuery(SqlCommand command)
		{
			int rowNumber = 0;
			try
			{
				rowNumber = command.ExecuteNonQuery();
			}
			catch(Exception e)
			{
				throw e;
			}
			return rowNumber;
		}

		public override SqlCommand CreateCommand(string sql)
		{
			return new SqlCommand(sql, Connection);
		}

		public override SqlDataReader Select(SqlCommand command)
		{
			return command.ExecuteReader();
		}
	}
}