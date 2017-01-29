using System;
using System.Collections.Generic;
using ER_Diagram_Modeler.Models.Helpers;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Models.Designer
{
	public class RelationshipModel
	{
		//Source:Destination => 1:N
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public string Name { get; set; }
		public TableModel Source { get; set; }
		public TableModel Destination { get; set; }
		public Optionality Optionality { get; set; } = Optionality.Optional;
		public List<RowModelPair> Attributes { get; set; } = new List<RowModelPair>();

		public void RefreshModel(RelationshipModel fresh)
		{
			Id = fresh.Id;
			Name = fresh.Name;
			Source = fresh.Source;
			Destination = fresh.Destination;
			Optionality = fresh.Optionality;
			Attributes.Clear();
			Attributes.AddRange(fresh.Attributes);
		}
	}
}