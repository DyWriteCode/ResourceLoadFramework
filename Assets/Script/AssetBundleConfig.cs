using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

/// <summary>
/// 自定义AB包xml数据结构
/// </summary>
[System.Serializable]
public class AssetBundleConfig { 
    [XmlElement("AssetBundleList")]
    public List<ABBase> AssetBundleList { get; set; }
}

/// <summary>
/// AB包xml数据结构Base类
/// </summary>
[System.Serializable]
public class ABBase
{
    [XmlAttribute("Path")]
    public string Path { get; set; }
    [XmlAttribute("Crc")]
    public uint Crc { get; set; } // 类似于MD5码
    [XmlAttribute("AssetBundleName")]
    public string AssetBundleName { get; set; }
    [XmlAttribute("AssetName")]
    public string AssetName { get; set; }
    [XmlElement("AssetBundleDependce")]
    public List<string> AssetBundleDependce {  get; set; }
}
