using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwellerBot
{
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Settings
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("key", IsNullable = false)]
        public Key[] keys { get; set; }

        /// <remarks/>
        public Paths paths { get; set; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Key
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value { get; set; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Paths
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("pathGroup")]
        public PathGroup[] pathGroups { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("path")]
        public Path[] paths { get; set; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class PathGroup
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("path")]
        public Path[] paths { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name { get; set; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Path
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value { get; set; }
    }
}

/* Settings example

<?xml version="1.0" encoding="utf-8" ?>
<Settings>
  <keys>
    <key name="dwellerBotKey" value=""/>
    <key name="openWeatherKey" value=""/>
  </keys>
  <paths>
    <pathGroup name="reactionImagePaths">
      <path value="D:\Pictures\"/>
      <path value="D:\Pictures\Macro"/>
    </pathGroup>
    <path name="reactionImageCachePath" value="reactionImageCache.json"/>
  </paths>
</Settings>
*/