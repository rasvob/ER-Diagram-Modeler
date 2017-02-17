using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ICSharpCode.AvalonEdit;
using Oracle.ManagedDataAccess.Client;

namespace ER_Diagram_Modeler.CommandOutput
{
	public class OutputPanelListener: IOutputListener
	{
		public TextEditor Editor { get; set; }

		public OutputPanelListener(TextEditor editor)
		{
			Editor = editor;
		}

		public void Write(string text)
		{
			Editor.AppendText(text);
			Editor.ScrollToEnd();
		}

		public void WriteLine(string line)
		{
			Write(line + Environment.NewLine);
		}

		public static string PrepareSql(SqlCommand command, SqlParameterCollection parameters)
		{
			string res = command.CommandText;

			if(command.CommandType == CommandType.StoredProcedure)
			{
				var strParams = (from SqlParameter parameter in parameters select $"{parameter.ParameterName} = {parameter.Value}").ToList();
				return $"{res} {string.Join(", ", strParams)}";
			}

			return parameters.Cast<SqlParameter>().Aggregate(res, (current, parameter) => current.Replace($"@{parameter.ParameterName}", parameter.Value.ToString()));
		}

		public static string PrepareSql(OracleCommand command, OracleParameterCollection parameters)
		{
			string res = command.CommandText;
			return parameters.Cast<SqlParameter>().Aggregate(res, (current, parameter) => current.Replace($":{parameter.ParameterName}", parameter.Value.ToString()));
		}

		public static string PrepareException(string text)
		{
			return $"EXCEPTION: {text}";
		}
	}
}