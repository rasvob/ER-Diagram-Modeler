using System.Xml.Linq;

namespace ER_Diagram_Modeler.DiagramConstruction.Serialization
{
	/// <summary>
	/// Interface for XML Serialization
	/// </summary>
	public interface IDiagramSerializable
	{
		/// <summary>
		/// Create XML element
		/// </summary>
		/// <returns>XML serialized data</returns>
		XElement CreateElement();

		/// <summary>
		/// Load property values from XML element
		/// </summary>
		/// <param name="element">XML serialized data from CreateElement()</param>
		void LoadFromElement(XElement element);
	}
}