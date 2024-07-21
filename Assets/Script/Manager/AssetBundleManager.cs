using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 资源块
/// </summary>
public class ResourceItem
{
    // 资源路径的crc
    public uint m_crc = 0;
    // 资源的名字
    public string m_AssetName = string.Empty;
    // 资源所在的AssetBundle
    public string m_AssetBundleName = string.Empty;
    // 该资源所依赖的AssetBundle
    public List<string> m_DependAssetBundle = null;
    // 该资源加载完成的AssetBundle
    public AssetBundle m_AssetBundle = null;
    // --------------------------------------------
    // 资源对象
    public Object m_Obj = null;
    // 资源唯一标识符
    public int m_Guid = 0;
    // 资源最后使用的时间
    public float m_LastUseTime = 0.0f;
    // 引用计数
    private int m_RefCount = 0;
    public int RefCount
    {
        get
        {
            return m_RefCount;
        }
        set
        {
            m_RefCount = value;
        }
    }
}

/// <summary>
/// AssetBundle块
/// </summary>
public class AssetBundleItem
{
    // 该资源加载完成的AssetBundle
    public AssetBundle AssetBundle = null;
    // 引用计数
    public int RefCount = 0;

    public void ReSet()
    {
        AssetBundle = null;
        RefCount = 0;
    }
}

/// <summary>
/// AssetBundle管理
/// </summary>
public class AssetBundleManager : Singleton<AssetBundleManager>
{
    // 资源关系配置表 可以根据资源的crc寻找到对应的资源块
    protected Dictionary<uint, ResourceItem> m_ResourceItemDic = new Dictionary<uint, ResourceItem>();
    // 储存已加载的AB包 可以根据资源的crc寻找到对应的asset bundle资源块
    protected Dictionary<uint, AssetBundleItem> m_AssetBundleItemDic = new Dictionary<uint, AssetBundleItem>();
    // AssetBundle类对象池
    protected ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool = ObjectManager.Instance.GetOrCreateClassPool<AssetBundleItem>(1000);

    // 加载AssetBundle配置表
    public bool LoadAssetBundleConfig()
    {
        m_ResourceItemDic.Clear();
        m_AssetBundleItemDic.Clear();
        string configPath = $"{Application.streamingAssetsPath}/AssetBundle/assetbundleconfig";
        AssetBundle configAssetBundle = AssetBundle.LoadFromFile(configPath);
        TextAsset textAsset = configAssetBundle.LoadAsset<TextAsset>("assetbundleconfig");          
        if (textAsset == null)
        {
            return false;
        }
        MemoryStream stream = new MemoryStream(textAsset.bytes);
        BinaryFormatter bf = new BinaryFormatter();
        AssetBundleConfig config = (AssetBundleConfig)bf.Deserialize(stream);
        stream.Close();
        for (int i = 0;i < config.AssetBundleList.Count;i++)
        {
            ABBase abBase = config.AssetBundleList[i];
            ResourceItem item = new ResourceItem();
            item.m_crc = abBase.Crc;
            item.m_AssetName = abBase.AssetName;
            item.m_AssetBundleName = abBase.AssetBundleName;
            item.m_DependAssetBundle = abBase.AssetBundleDependce;
            if (m_ResourceItemDic.ContainsKey(item.m_crc) == false)
            {
                m_ResourceItemDic.Add(item.m_crc, item);
            }
        }
        return true;
    }

    // 根据资源路径的CRC去加载中间类ResouceItem
    public ResourceItem LoadResouceAssetBundle(uint crc)
    {
        ResourceItem item = null;
        if (m_ResourceItemDic.TryGetValue(crc, out item) == false || item == null)
        {
            return item;
        }
        if (item.m_AssetBundle != null)
        {
            return item;
        }
        item.m_AssetBundle = LoadAssetBundle(item.m_AssetBundleName);
        if (item.m_DependAssetBundle != null)
        {
            for (int i = 0; i < item.m_DependAssetBundle.Count; i++)
            {
                LoadAssetBundle(item.m_DependAssetBundle[i]);
            }
        }
        return item;
    }

    // 根据名字加载单个资源
    private AssetBundle LoadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        uint crc = _CRC32.GetCRC32(name);
        if (m_AssetBundleItemDic.TryGetValue(crc, out item) == false || item == null)
        {
            AssetBundle assetBundle = null;
            string fullPath = $"{Application.streamingAssetsPath}/AssetBundle/{name}";
            if (File.Exists(fullPath) == true)
            {  
                assetBundle = AssetBundle.LoadFromFile(fullPath);
            }
            if (assetBundle == null)
            {
                return null;
            }
            item = m_AssetBundleItemPool.Spawn(true);
            item.AssetBundle = assetBundle;
            item.RefCount++;
            m_AssetBundleItemDic.Add(crc, item);
        }
        else
        {
            item.RefCount++;
        }
        return item.AssetBundle;
    }

    // 根据ResouceItem释放AB包
    public void ReleaseAssetBundle(ResourceItem item)
    {
        if (item == null)
        {
            return;
        }
        if (item.m_DependAssetBundle != null && item.m_DependAssetBundle.Count > 0)
        {
            for (int i = 0;i < item.m_DependAssetBundle.Count;i++)
            {
                UnLoadAssetBundle(item.m_DependAssetBundle[i]);
            }
        }
        UnLoadAssetBundle(item.m_AssetBundleName);
    }

    // 根据名字卸载AB包
    private void UnLoadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        uint crc = _CRC32.GetCRC32(name);
        if (m_AssetBundleItemDic.TryGetValue(crc, out item) == true && item != null)
        {
            item.RefCount--;
            if (item.RefCount <= 0 && item.AssetBundle != null)
            {
                item.AssetBundle.Unload(true);
                item.ReSet();
                m_AssetBundleItemPool.Recycle(item);
                m_AssetBundleItemDic.Remove(crc);
            }
        }
    }

    // 根据CRC卸载AB包
    private void UnLoadAssetBundle(uint crc)
    {
        AssetBundleItem item = null;
        if (m_AssetBundleItemDic.TryGetValue(crc, out item) == true && item != null)
        {
            item.RefCount--;
            if (item.RefCount <= 0 && item.AssetBundle != null)
            {
                item.AssetBundle.Unload(true);
                item.ReSet();
                m_AssetBundleItemPool.Recycle(item);
                m_AssetBundleItemDic.Remove(crc);
            }
        }
    }

    // 根据CRC寻找资源块
    public ResourceItem FindResourceItem(uint crc)
    {
        return m_ResourceItemDic[crc];
    }

    // 根据资源名字寻找资源块
    public ResourceItem FindResourceItem(string name)
    {
        return FindResourceItem(_CRC32.GetCRC32(name));
    }
}