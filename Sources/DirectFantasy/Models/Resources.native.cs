using System.Xml.Serialization;

namespace DirectFantasy.Models;

[XmlRoot("Resources")]
public class Resources<T>
{
    [XmlElement("Resource")]
    public List<T> ResourceItems { get; set; }
}