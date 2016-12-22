namespace ER_Diagram_Modeler.DatabaseConnection
{
	public abstract class DatabaseProxy<T, U>
	{
		public abstract string ConnectionString { get; set; }
		public abstract bool Connect();
		public abstract bool Connect(string conString);
		public abstract void Close();
		public abstract int ExecuteNonQuery(T command);
		public abstract T CreateCommand(string sql);
		public abstract U Select(T command);
	}
}