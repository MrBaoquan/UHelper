using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace UHelper
{
    public class ScreenConfig {
        [XmlAttribute()]
        public int Width = Screen.width;
        [XmlAttribute()]
        public int Height = Screen.height;

        [XmlAttribute]
        public int RefreshRate = 30;

        [XmlAttribute()]
        public UFullScreenMode Mode = UFullScreenMode.ExclusiveFullScreen;

    }

    public class AppConfig : UConfig
    {
        public float KeepTopWindowInterval = 0;
        public ScreenConfig Screen = new ScreenConfig();

        [XmlArray("Displays")]
        [XmlArrayItem("Display")]
        public List<ScreenConfig> Displays = new List<ScreenConfig>();
    }
}