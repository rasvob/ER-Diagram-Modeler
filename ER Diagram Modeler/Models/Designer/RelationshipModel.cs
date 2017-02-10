using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;
using ER_Diagram_Modeler.DiagramConstruction.Serialization;
using ER_Diagram_Modeler.Models.Helpers;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Models.Designer
{
	public class RelationshipModel: IDiagramSerializable
	{
		//Source:Destination => 1:N
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public string Name { get; set; }
		public TableModel Source { get; set; }
		public TableModel Destination { get; set; }
		public Optionality Optionality { get; set; } = Optionality.Optional;
		public string UpdateAction { get; set; }
		public string DeleteAction { get; set; }
		public DateTime LastModified { get; set; }
		public List<RowModelPair> Attributes { get; set; } = new List<RowModelPair>();

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

		public XElement CreateElement()
		{
			return new XElement("RelationshipModel", new XAttribute("Id", Id), new XAttribute("Name", Name), new XAttribute("LastModified", LastModified), new XElement("Source", Source.CreateElement()), new XElement("Destination", Destination.CreateElement()));
		}

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