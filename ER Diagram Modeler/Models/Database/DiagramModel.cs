using System.ComponentModel;
using System.Runtime.CompilerServices;
using ER_Diagram_Modeler.Annotations;

namespace ER_Diagram_Modeler.Models.Database
{
	/// <summary>
	/// Model of diagram in DB
	/// </summary>
	public class DiagramModel: INotifyPropertyChanged
	{
		private string _name;
		private string _xml;

		/// <summary>
		/// Diagram name
		/// </summary>
		public string Name
		{
			get { return _name; }
			set
			{
				if (value == _name) return;
				_name = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// XML serialized data
		/// </summary>
		public string Xml
		{
			get { return _xml; }
			set
			{
				if (value == _xml) return;
				_xml = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// For data binding
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}