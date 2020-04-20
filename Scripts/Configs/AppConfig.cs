using UnityEngine;
using System.Xml.Serialization;

namespace UHelper
{
    public class ScreenConfig {
        [XmlAttribute()]
        public int Width = Screen.width;
        [XmlAttribute()]
        public int Height = Screen.height;

        [XmlAttribute()]
        public FullScreenMode Mode = FullScreenMode.ExclusiveFullScreen;

    }
    public class AppConfig : UConfig
    {
        public ScreenConfig Screen = new ScreenConfig();
    }
}