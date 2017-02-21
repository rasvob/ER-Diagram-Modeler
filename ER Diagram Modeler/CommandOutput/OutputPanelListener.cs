using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ICSharpCode.AvalonEdit;
using Oracle.ManagedDataAccess.Client;

namespace ER_Diagram_Modeler.CommandOutput
{
	/// <summary>
	/// Console-like output panel
	/// </summary>
	public class OutputPanelListener: IOutputListener
	{
		/// <summary>
		/// Ouput textbox with syntax highlight
		/// </summary>
		public TextEditor Editor { get; set; }

		public OutputPanelListener(TextEditor editor)
		{
			Editor = editor;
		}

		/// <summary>
		/// Write text to console
		/// </summary>
		/// <param name="text">Text for write</param>
		public void Write(string text)
		{
			Editor.AppendText(text);
			Editor.ScrollToEnd();
		}

		/// <summary>
		/// Write line to console
		/// </summary>
		/// <param name="line">Text for write</param>
		public void WriteLine(string line)
		{
			Write(line + Environment.NewLine);
		}

		/// <summary>
		/// Prepare executed SQL Command by replacing parameters with values - MS Sql
		/// </summary>
		/// <param name="command">Executed command</param>
		/// <param name="parameters">Command parameters</param>
		/// <returns>SQL Command</returns>
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

		/// <summary>
		/// Prepare executed SQL Command by replacing parameters with values - Oracle
		/// </summary>
		/// <param name="command">Executed command</param>
		/// <param name="parameters">Command parameters</param>
		/// <returns>SQL Command</returns>
		public static string PrepareSql(OracleCommand command, OracleParameterCollection parameters)
		{
			string res = command.CommandText;
			return parameters.Cast<SqlParameter>().Aggregate(res, (current, parameter) => current.Replace($":{parameter.ParameterName}", parameter.Value.ToString()));
		}

		/// <summary>
		/// Prefix exception text
		/// </summary>
		/// <param name="text">Exception message</param>
		/// <returns>Prefixed message</returns>
		public static string PrepareException(string text)
		{
			return $"EXCEPTION: {text}";
		}
	}
}