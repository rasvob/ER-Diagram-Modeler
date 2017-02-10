using System.Xml.Linq;

namespace ER_Diagram_Modeler.DiagramConstruction.Serialization
{
	public interface IDiagramSerializable
	{
		XElement CreateElement();
		void LoadFromElement(XElement element);
	}
}