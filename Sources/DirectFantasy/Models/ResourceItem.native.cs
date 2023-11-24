using System.Xml.Serialization;

namespace DirectFantasy.Models;

public class ResourceItem
{
    [XmlAttribute("Id")]
    public string ResourceId { get; set; }

    [XmlAttribute("Home")]
    public string HomeValue { get; set; }

    [XmlAttribute("Away")]
    public string AwayValue { get; set; }
}