using System.Threading.Tasks;

namespace ER_Diagram_Modeler.DatabaseConnection
{
	public interface IDatabase<out T>
	{
		string ConnectionString { get; set; }
		bool Connect();
		bool Connect(string conString);
		void Close();
		T CreateCommand(string sql);
	}
}