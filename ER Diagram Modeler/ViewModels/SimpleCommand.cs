using System;
using System.Windows.Input;

namespace ER_Diagram_Modeler.ViewModels
{
	public class SimpleCommand: ICommand
	{
		public Predicate<object> CanExecuteDelegate { get; set; }

		public Action<object> ExecuteDelegate { get; set; }

		public SimpleCommand(Predicate<object> canExecuteDelegate, Action<object> executeDelegate)
		{
			CanExecuteDelegate = CanExecuteDelegate;
			ExecuteDelegate = ExecuteDelegate;
		}

		public SimpleCommand(Action<object> executeDelegate)
		{
			ExecuteDelegate = executeDelegate;
		}

		public bool CanExecute(object parameter)
		{
			if (CanExecuteDelegate != null)
			{
				return CanExecuteDelegate(parameter);
			}
			return true;
		}

		public void Execute(object parameter)
		{
			ExecuteDelegate?.Invoke(parameter);
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}
	}
}