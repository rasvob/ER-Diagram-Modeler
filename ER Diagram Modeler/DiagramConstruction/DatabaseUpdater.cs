using System;
using System.Data.SqlClient;
using System.Windows;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DiagramConstruction.Strategy;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.Models.Designer;
using MahApps.Metro.Controls.Dialogs;

namespace ER_Diagram_Modeler.DiagramConstruction
{
	public class DatabaseUpdater
	{
		public string RenameTable(string oldName, TableModel model)
		{
			try
			{
				var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
				ctx.RenameTable(oldName, model.Title);
				return null;
			}
			catch(SqlException exception)
			{
				model.Title = oldName;
				return exception.Message;
			}
		}

		public string AddColumn(string table, TableRowModel column)
		{
			try
			{
				var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
				ctx.AddColumn(table, column);
				return null;
			}
			catch(SqlException exception)
			{
				return exception.Message;
			}
		}

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

		public string RemoveColumn(TableModel table, ref EditRowEventArgs args)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
			try
			{
				ctx.RemoveColumn(table.Title, args.RowModel.Name);
				return null;
			}
			catch (SqlException exception)
			{
				return exception.Message;
			}
			finally
			{
				TableModel model = ctx.ReadTableDetails(table.Id, table.Title);
				table.RefreshModel(model);
				args = null;
			}
		}

		public string UpdateColumn(string table, TableRowModel column)
		{
			try
			{
				var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
				ctx.UpdateColumn(table, column);
				return null;
			}
			catch(SqlException exception)
			{
				return exception.Message;
			}
		}

		public string RenameColumn(string table, string oldName, string newName)
		{
			try
			{
				var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
				ctx.RenameColumn(table, oldName, newName);
				return null;
			}
			catch(SqlException exception)
			{
				return exception.Message;
			}
		}

		public TableModel RefreshModel(TableModel model)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
			return ctx.ReadTableDetails(model.Id, model.Title);
		}

		public string DropTable(TableModel table)
		{
			try
			{
				var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
				ctx.RemoveTable(table);
				return null;
			}
			catch(SqlException exception)
			{
				return exception.Message;
			}
		}

		public string UpdatePrimaryKeyConstraint(TableModel model)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);

			try
			{
				ctx.UpdatePrimaryKeyConstraint(model);
				return null;
			}
			catch (SqlException exception)
			{
				return exception.Message;
			}
			finally
			{
				TableModel fresh = ctx.ReadTableDetails(model.Id, model.Title);
				model.RefreshModel(fresh);
			}
		}

		public string RemoveRelationship(RelationshipModel model)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);

			try
			{
				ctx.RemoveRelationship(model);
				return null;
			}
			catch(SqlException exception)
			{
				return exception.Message;
			}
		}

		public string AddRelationship(RelationshipModel model)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);

			try
			{
				ctx.AddRelationship(model);
				return null;
			}
			catch(SqlException exception)
			{
				return exception.Message;
			}
		}
	}
}