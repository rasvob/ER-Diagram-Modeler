namespace ER_Diagram_Modeler.DatabaseConnection
{
	/// <summary>
	/// Basic database operations
	/// </summary>
	/// <typeparam name="T">SqlCommand or OracleCommand</typeparam>
	public interface IDatabase<out T>
	{
		/// <summary>
		/// Connection string
		/// </summary>
		string ConnectionString { get; set; }

		/// <summary>
		/// Connect to DB - Session connection string
		/// </summary>
		/// <returns>True if success, false if not</returns>
		bool Connect();

		/// <summary>
		/// Connect to DB
		/// </summary>
		/// <param name="conString">Connection string</param>
		/// <returns>rue if success, false if not</returns>
		bool Connect(string conString);

		/// <summary>
		/// Close DB connection
		/// </summary>
		void Close();

		/// <summary>
		/// Create SqlCommand or OracleCommand
		/// </summary>
		/// <param name="sql">SQL Command text</param>
		/// <returns>SqlCommand or OracleCommand object</returns>
		T CreateCommand(string sql);
	}
}