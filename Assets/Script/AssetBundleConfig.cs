using System.Collections.Generic;
using System.Xml.Serialization;

/// <summary>
/// 自定义AB包xml数据结构
/// </summary>
[System.Serializable]
public class AssetBundleConfig {
    /// <summary>
    /// AssetBundle列表
    /// 这个数据结构由一个个AssetBundleBase组成
    /// </summary>
    [XmlElement("AssetBundleList")]
    public List<ABBase> AssetBundleList { get; set; }
}

/// <summary>
/// AB包xml数据结构Base类
/// </summary>
[System.Serializable]
public class ABBase
{
    /// <summary>
    /// 资源包的路径
    /// </summary>
    [XmlAttribute("Path")]
    public string Path { get; set; }
    /// <summary>
    /// 资源包的唯一标识符 类似于MD5码
    /// </summary>
    [XmlAttribute("Crc")]
    public uint Crc { get; set; } 
    /// <summary>
    /// 资源包的名字
    /// </summary>
    [XmlAttribute("AssetBundleName")]
    public string AssetBundleName { get; set; }
    /// <summary>
    /// 资源的名字
    /// </summary>
    [XmlAttribute("AssetName")]
    public string AssetName { get; set; }
    /// <summary>
    /// 这个资源包所依赖的其他资源包
    /// </summary>
    [XmlElement("AssetBundleDependce")]
    public List<string> AssetBundleDependce {  get; set; }
}
