using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Snappy.Models.Yahama
{
    [XmlRoot(ElementName = "Power_Control")]
    public class PowerControl
    {

        [XmlElement(ElementName = "Power")]
        public string Power { get; set; }

        [XmlElement(ElementName = "Sleep")]
        public string Sleep { get; set; }
    }

    [XmlRoot(ElementName = "Lvl")]
    public class Lvl
    {

        [XmlElement(ElementName = "Val")]
        public int Val { get; set; }

        [XmlElement(ElementName = "Exp")]
        public int Exp { get; set; }

        [XmlElement(ElementName = "Unit")]
        public string Unit { get; set; }
    }

    [XmlRoot(ElementName = "Volume")]
    public class Volume
    {

        [XmlElement(ElementName = "Lvl")]
        public Lvl Lvl { get; set; }

        [XmlElement(ElementName = "Mute")]
        public string Mute { get; set; }
    }

    [XmlRoot(ElementName = "Icon")]
    public class Icon
    {

        [XmlElement(ElementName = "On")]
        public string On { get; set; }

        [XmlElement(ElementName = "Off")]
        public string Off { get; set; }
    }

    [XmlRoot(ElementName = "Current_Input_Sel_Item")]
    public class CurrentInputSelItem
    {

        [XmlElement(ElementName = "Param")]
        public string Param { get; set; }

        [XmlElement(ElementName = "RW")]
        public string RW { get; set; }

        [XmlElement(ElementName = "Title")]
        public string Title { get; set; }

        [XmlElement(ElementName = "Icon")]
        public Icon Icon { get; set; }

        [XmlElement(ElementName = "Src_Name")]
        public object SrcName { get; set; }

        [XmlElement(ElementName = "Src_Number")]
        public int SrcNumber { get; set; }
    }

    [XmlRoot(ElementName = "Input")]
    public class Input
    {

        [XmlElement(ElementName = "Input_Sel")]
        public string InputSel { get; set; }

        [XmlElement(ElementName = "Current_Input_Sel_Item")]
        public CurrentInputSelItem CurrentInputSelItem { get; set; }
    }

    [XmlRoot(ElementName = "Current")]
    public class Current
    {

        [XmlElement(ElementName = "Straight")]
        public string Straight { get; set; }

        [XmlElement(ElementName = "Enhancer")]
        public string Enhancer { get; set; }

        [XmlElement(ElementName = "Sound_Program")]
        public string SoundProgram { get; set; }
    }

    [XmlRoot(ElementName = "Program_Sel")]
    public class ProgramSel
    {

        [XmlElement(ElementName = "Current")]
        public Current Current { get; set; }
    }

    [XmlRoot(ElementName = "Surround")]
    public class Surround
    {

        [XmlElement(ElementName = "Program_Sel")]
        public ProgramSel ProgramSel { get; set; }
    }

    [XmlRoot(ElementName = "Basic_Status")]
    public class BasicStatus
    {

        [XmlElement(ElementName = "Power_Control")]
        public PowerControl PowerControl { get; set; }

        [XmlElement(ElementName = "Volume")]
        public Volume Volume { get; set; }

        [XmlElement(ElementName = "Input")]
        public Input Input { get; set; }

        [XmlElement(ElementName = "Surround")]
        public Surround Surround { get; set; }
    }

    [XmlRoot(ElementName = "Main_Zone")]
    public class MainZone
    {

        [XmlElement(ElementName = "Basic_Status")]
        public BasicStatus BasicStatus { get; set; }

        [XmlElement(ElementName = "Power_Control")]
        public PowerControl PowerControl { get; set; }

        [XmlElement(ElementName = "Config")]
        public Config Config { get; set; }

        [XmlElement(ElementName="Volume")]
	    public Volume Volume { get; set; }
    }

    [XmlRoot(ElementName = "Zone_2")]
    public class Zone_2
    {

        [XmlElement(ElementName = "Basic_Status")]
        public BasicStatus BasicStatus { get; set; }

        [XmlElement(ElementName = "Power_Control")]
        public PowerControl PowerControl { get; set; }

        [XmlElement(ElementName = "Config")]
        public Config Config { get; set; }
    }


    [XmlRoot(ElementName = "Zone_3")]
    public class Zone_3
    {

        [XmlElement(ElementName = "Basic_Status")]
        public BasicStatus BasicStatus { get; set; }

        [XmlElement(ElementName = "Power_Control")]
        public PowerControl PowerControl { get; set; }

        [XmlElement(ElementName = "Config")]
        public Config Config { get; set; }
    }


    [XmlRoot(ElementName = "Scene")]
    public class Scene
    {

        [XmlElement(ElementName = "Scene_1")]
        public string Scene1 { get; set; }

        [XmlElement(ElementName = "Scene_2")]
        public string Scene2 { get; set; }

        [XmlElement(ElementName = "Scene_3")]
        public string Scene3 { get; set; }

        [XmlElement(ElementName = "Scene_4")]
        public string Scene4 { get; set; }

        [XmlElement(ElementName = "Scene_5")]
        public string Scene5 { get; set; }

        [XmlElement(ElementName = "Scene_6")]
        public string Scene6 { get; set; }

        [XmlElement(ElementName = "Scene_7")]
        public string Scene7 { get; set; }

        [XmlElement(ElementName = "Scene_8")]
        public string Scene8 { get; set; }

        [XmlElement(ElementName = "Scene_9")]
        public string Scene9 { get; set; }

        [XmlElement(ElementName = "Scene_10")]
        public string Scene10 { get; set; }

        [XmlElement(ElementName = "Scene_11")]
        public string Scene11 { get; set; }

        [XmlElement(ElementName = "Scene_12")]
        public string Scene12 { get; set; }
    }

    [XmlRoot(ElementName = "Name")]
    public class Name
    {

        [XmlElement(ElementName = "Zone")]
        public string Zone { get; set; }

        [XmlElement(ElementName = "Scene")]
        public Scene Scene { get; set; }
    }

    [XmlRoot(ElementName = "Config")]
    public class Config
    {

        [XmlElement(ElementName = "Feature_Existence")]
        public int FeatureExistence { get; set; }

        [XmlElement(ElementName = "Feature_Availability")]
        public string FeatureAvailability { get; set; }

        [XmlElement(ElementName = "Name")]
        public Name Name { get; set; }

        [XmlElement(ElementName = "Volume_Existence")]
        public string VolumeExistence { get; set; }
    }



    [XmlRoot(ElementName = "YAMAHA_AV")]
    public class YamahaAvMainZone
    {

        [JsonPropertyName("mainZone")]
        [XmlElement(ElementName = "Main_Zone")]
        public MainZone MainZone { get; set; }

        [XmlAttribute(AttributeName = "rsp")]
        public string Rsp { get; set; }

        [XmlAttribute(AttributeName = "RC")]
        public int RC { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "YAMAHA_AV")]
    public class YamahaAvZone2
    {

        [JsonPropertyName("zone2")]
        [XmlElement(ElementName = "Zone_2")]
        public Zone_2 Zone_2 { get; set; }

        [XmlAttribute(AttributeName = "rsp")]
        public string Rsp { get; set; }

        [XmlAttribute(AttributeName = "RC")]
        public int RC { get; set; }

        [XmlText]
        public string Text { get; set; }
    }


    [XmlRoot(ElementName = "YAMAHA_AV")]
    public class YamahaAvZone3
    {

        [JsonPropertyName("zone3")]
        [XmlElement(ElementName = "Zone_3")]
        public Zone_3 Zone_3 { get; set; }

        [XmlAttribute(AttributeName = "rsp")]
        public string Rsp { get; set; }

        [XmlAttribute(AttributeName = "RC")]
        public int RC { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}