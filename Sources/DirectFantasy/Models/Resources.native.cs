using System.Xml.Serialization;

namespace DirectFantasy.Models;

[XmlRoot("Resources")]
public class Resources
{
    [XmlElement("Resource")]
    public List<ResourceItem> ResourceItems { get; set; }
}