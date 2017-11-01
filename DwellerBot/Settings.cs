using System.Xml.Serialization;

namespace DwellerBot
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public partial class Settings
    {
        [XmlArrayItem("key", IsNullable = false)]
        public Key[] keys { get; set; }

        public Paths paths { get; set; }
    }
    
    [XmlType(AnonymousType = true)]
    public partial class Key
    {
        [XmlAttribute()]
        public string name { get; set; }

        [XmlAttribute()]
        public string value { get; set; }
    }
    
    [XmlType(AnonymousType = true)]
    public partial class Paths
    {
        [XmlElement("pathGroup")]
        public PathGroup[] pathGroups { get; set; }

        [XmlElement("path")]
        public Path[] paths { get; set; }
    }
    
    [XmlType(AnonymousType = true)]
    public partial class PathGroup
    {
        [XmlElement("path")]
        public Path[] paths { get; set; }

        [XmlAttribute()]
        public string name { get; set; }
    }
    
    [XmlType(AnonymousType = true)]
    public partial class Path
    {
        [XmlAttribute()]
        public string name { get; set; }
        
        [XmlAttribute()]
        public string value { get; set; }
    }
}

/* Settings example
<Settings>
  <keys>
    <key name="botName" value="@DwellerDebugBot"/>
    <key name="ownerName" value="angelore"/>
    <key name="ownerId" value="99541817"/>
    <key name="dwellerBotKey" value=""/>
    <key name="openWeatherKey" value=""/>
  </keys>
  <paths>
    <pathGroup name="reactionImagePaths">
      <path value="D:\Pictures\Internets\Reaction_images"/>
      <path value="D:\Pictures\Internets\Reaction_images\Macro"/>
    </pathGroup>
    <path name="reactionImageCachePath" value="reactionImageCache.json"/>
    <path name="featureRequestsPath" value="featureRequests.json"/>
    <path name="askMeResponsesPath" value="askStasonResponses.json"/>
  </paths>
</Settings>
*/
