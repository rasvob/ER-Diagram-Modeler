using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;
using ER_Diagram_Modeler.DiagramConstruction.Serialization;
using ER_Diagram_Modeler.Models.Helpers;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Models.Designer
{
	/// <summary>
	/// Foreign keys constraint representation (Source:Destination =&gt; 1:N)
	/// </summary>
	public class RelationshipModel: IDiagramSerializable
	{
		/// <summary>
		/// Constraint ObjectId
		/// </summary>
		public string Id { get; set; } = Guid.NewGuid().ToString();

		/// <summary>
		/// Constraint name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Primary key table
		/// </summary>
		public TableModel Source { get; set; }

		/// <summary>
		/// Foreign key table
		/// </summary>
		public TableModel Destination { get; set; }

		/// <summary>
		/// Optional or mandatory
		/// </summary>
		public Optionality Optionality { get; set; } = Optionality.Optional;

		/// <summary>
		/// Action ON UPDATE
		/// </summary>
		public string UpdateAction { get; set; }

		/// <summary>
		/// Action ON DELETE
		/// </summary>
		public string DeleteAction { get; set; }

		/// <summary>
		/// Last modified or created
		/// </summary>
		public DateTime LastModified { get; set; }

		/// <summary>
		/// Columns in constraint
		/// </summary>
		public List<RowModelPair> Attributes { get; set; } = new List<RowModelPair>();

		/// <summary>
		/// Refresh from DB
		/// </summary>
		/// <param name="fresh"></param>
		public void RefreshModel(RelationshipModel fresh)
		{
			Id = fresh.Id;
			Name = fresh.Name;
			Source = fresh.Source;
			Destination = fresh.Destination;
			Optionality = fresh.Optionality;
			UpdateAction = fresh.UpdateAction;
			DeleteAction = fresh.DeleteAction;
			LastModified = fresh.LastModified;
			Attributes.Clear();
			Attributes.AddRange(fresh.Attributes);
		}

		/// <summary>
		/// Create XML element
		/// </summary>
		/// <returns>XML serialized element</returns>
		public XElement CreateElement()
		{
			return new XElement("RelationshipModel", new XAttribute("Id", Id), new XAttribute("Name", Name), new XAttribute("LastModified", LastModified), new XElement("Source", Source.CreateElement()), new XElement("Destination", Destination.CreateElement()));
		}

		/// <summary>
		/// Load data from XML
		/// </summary>
		/// <param name="element">Data in XML</param>
		public void LoadFromElement(XElement element)
		{
			Id = element.Attribute("Id")?.Value;
			Name = element.Attribute("Name")?.Value;
			LastModified = Convert.ToDateTime(element.Attribute("LastModified")?.Value);
			Source = new TableModel();
			Destination = new TableModel();
			Source.LoadFromElement(element.Element("Source")?.Element("TableModel"));
			Destination.LoadFromElement(element.Element("Destination")?.Element("TableModel"));
		}
	}
}