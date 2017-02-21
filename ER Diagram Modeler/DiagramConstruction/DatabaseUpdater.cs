using System;
using System.Data.SqlClient;
using System.Windows;
using ER_Diagram_Modeler.CommandOutput;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DiagramConstruction.Strategy;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.Models.Designer;
using MahApps.Metro.Controls.Dialogs;
using Oracle.ManagedDataAccess.Client;

namespace ER_Diagram_Modeler.DiagramConstruction
{
	/// <summary>
	/// DB scheme changes
	/// </summary>
	public class DatabaseUpdater
	{
		/// <summary>
		/// Rename table in DB
		/// </summary>
		/// <param name="oldName">Old table name</param>
		/// <param name="model">Table model</param>
		/// <returns>Exception message if failed, NULL if not</returns>
		public string RenameTable(string oldName, TableModel model)
		{
			try
			{
				var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
				ctx.RenameTable(oldName, model.Title);
				TableModel fresh = RefreshModel(model);
				model.Title = fresh.Title;
				return null;
			}
			catch(Exception exception) when(exception is SqlException || exception is OracleException)
			{
				model.Title = oldName;
				Output.WriteLine(OutputPanelListener.PrepareException(exception.Message));
				return exception.Message;
			}
		}

		/// <summary>
		/// Add columnd to table in DB
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="column">Column model</param>
		/// <returns>Exception message if failed, NULL if not</returns>
		public string AddColumn(string table, TableRowModel column)
		{
			try
			{
				var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
				ctx.AddColumn(table, column);
				return null;
			}
			catch(Exception exception) when(exception is SqlException || exception is OracleException)
			{
				Output.WriteLine(OutputPanelListener.PrepareException(exception.Message));
				return exception.Message;
			}
		}

		/// <summary>
		/// Add new column or update existing in DB
		/// </summary>
		/// <param name="model">Table model</param>
		/// <param name="column">Column model</param>
		/// <param name="refreshedModel">Model with new attributes</param>
		/// <param name="args">Arguments provided by add/update event</param>
		/// <returns>Exception message if failed, NULL if not</returns>
		public string AddOrUpdateCollumn(TableModel model, TableRowModel column, out TableModel refreshedModel, ref EditRowEventArgs args)
		{
			var err = args == null ? AddColumn(model.Title, column) : UpdateColumn(model.Title, column);

			if (args != null)
			{
				if (!args.RowModel.Name.Equals(column.Name))
				{
					err = RenameColumn(model.Title, args.RowModel.Name, column.Name);
				}
			}

			refreshedModel = err != null ? null : RefreshModel(model);
			args = null;
			return err;
		}

		/// <summary>
		/// Drop column in table
		/// </summary>
		/// <param name="table">Table model</param>
		/// <param name="args">Arguments provided by drop event</param>
		/// <returns>Exception message if failed, NULL if not</returns>
		public string RemoveColumn(TableModel table, ref EditRowEventArgs args)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
			try
			{
				ctx.RemoveColumn(table.Title, args.RowModel.Name);
				return null;
			}
			catch(Exception exception) when(exception is SqlException || exception is OracleException || exception is ApplicationException)
			{
				Output.WriteLine(OutputPanelListener.PrepareException(exception.Message));
				return exception.Message;
			}
			finally
			{
				TableModel model = ctx.ReadTableDetails(table.Id, table.Title);
				table.RefreshModel(model);
				args = null;
			}
		}

		/// <summary>
		/// Alter columnd to table in DB
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="column">Column model</param>
		/// <returns>Exception message if failed, NULL if not</returns>
		public string UpdateColumn(string table, TableRowModel column)
		{
			try
			{
				var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
				ctx.UpdateColumn(table, column);
				return null;
			}
			catch(Exception exception) when(exception is SqlException || exception is OracleException)
			{
				Output.WriteLine(OutputPanelListener.PrepareException(exception.Message));
				return exception.Message;
			}
		}

		/// <summary>
		/// Rename column in table
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="oldName">Old name</param>
		/// <param name="newName">New name</param>
		/// <returns>Exception message if failed, NULL if not</returns>
		public string RenameColumn(string table, string oldName, string newName)
		{
			try
			{
				var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
				ctx.RenameColumn(table, oldName, newName);
				return null;
			}
			catch(Exception exception) when(exception is SqlException || exception is OracleException)
			{
				Output.WriteLine(OutputPanelListener.PrepareException(exception.Message));
				return exception.Message;
			}
		}

		/// <summary>
		/// Refresh current table
		/// </summary>
		/// <param name="model">Table model</param>
		/// <returns>Refreshed table model</returns>
		public TableModel RefreshModel(TableModel model)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
			return ctx.ReadTableDetails(model.Id, model.Title);
		}

		/// <summary>
		/// Drop table from DB
		/// </summary>
		/// <param name="table">Table model</param>
		/// <returns>Exception message if failed, NULL if not</returns>
		public string DropTable(TableModel table)
		{
			try
			{
				var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
				ctx.RemoveTable(table);
				return null;
			}
			catch(Exception exception) when(exception is SqlException || exception is OracleException || exception is ApplicationException)
			{
				Output.WriteLine(OutputPanelListener.PrepareException(exception.Message));
				return exception.Message;
			}
		}

		/// <summary>
		/// Update PK constraint
		/// </summary>
		/// <param name="model">Table model</param>
		/// <returns>Exception message if failed, NULL if not</returns>
		public string UpdatePrimaryKeyConstraint(TableModel model)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);

			try
			{
				ctx.UpdatePrimaryKeyConstraint(model);
				return null;
			}
			catch(Exception exception) when(exception is SqlException || exception is OracleException)
			{
				Output.WriteLine(OutputPanelListener.PrepareException(exception.Message));
				return exception.Message;
			}
			finally
			{
				TableModel fresh = ctx.ReadTableDetails(model.Id, model.Title);
				model.RefreshModel(fresh);
			}
		}

		/// <summary>
		/// Drop foreign key constraint
		/// </summary>
		/// <param name="model">Relationship model</param>
		/// <returns>Exception message if failed, NULL if not</returns>
		public string RemoveRelationship(RelationshipModel model)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);

			try
			{
				ctx.RemoveRelationship(model);
				return null;
			}
			catch(Exception exception) when(exception is SqlException || exception is OracleException)
			{
				Output.WriteLine(OutputPanelListener.PrepareException(exception.Message));
				return exception.Message;
			}
		}

		/// <summary>
		/// Create foreign key constraint
		/// </summary>
		/// <param name="model">Relationship model</param>
		/// <returns>Exception message if failed, NULL if not</returns>
		public string AddRelationship(RelationshipModel model)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);

			try
			{
				ctx.AddRelationship(model);
				return null;
			}
			catch(Exception exception) when(exception is SqlException || exception is OracleException)
			{
				Output.WriteLine(OutputPanelListener.PrepareException(exception.Message));
				return exception.Message;
			}
		}
	}
}