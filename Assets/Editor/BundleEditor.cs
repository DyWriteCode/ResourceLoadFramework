using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// AB包打包工具类
/// </summary>
public class BundleEditor
{
    public static string ABCONFIGPATH = "Assets/Editor/ABConfig.asset";
    public static string m_BundleTargetPath = Application.streamingAssetsPath + "/AssetBundle";
    public static string xmlPath = Application.dataPath + "/AssetBundleConfig.xml";
    public static string bytePath = "Assets/GameData/Data/ABData/AssetBundleConfig.bytes";
    // key为AB包名字 value为文件路径
    public static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();
    // 过滤的list
    public static List<string> m_AllFileAB = new List<string>();
    // 单个prefab的AB包
    public static Dictionary<string, List<string>> m_AllPrefabDir = new Dictionary<string, List<string>>();
    // 储存所有有效路径
    public static List<string> m_ConfigFile = new List<string>();

    /// <summary>
    /// 进行打包
    /// </summary>
    [MenuItem("Tools/AssetBundle/BuildAssetBundle")]
    public static void BuildAssetBundles()
    {
        m_AllFileDir.Clear();
        m_AllFileAB.Clear();
        m_AllPrefabDir.Clear();
        m_ConfigFile.Clear();
        ClearAssetBundles();
        ABConfig abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);
        if (abConfig.m_AllPrefabPath.Count == 0)
        {
            Debug.Log($"No Asset Bundle path im the list => Name : m_AllPrefabPath");
            return;
        }
        if (abConfig.m_AllFileDirAB.Count == 0)
        {
            Debug.Log($"No Asset Bundle path im the dict => Name : m_AllFileDirAB");
            return;
        }
        foreach (ABConfig.FileDirABName fileDir in abConfig.m_AllFileDirAB)
        {
            Debug.Log($"ABName : {fileDir.ABName} | Path : {fileDir.Path}");
            // AB包配置名字重复 返回
            if (m_AllFileDir.ContainsKey(fileDir.ABName))
            {
                Debug.Log("Please check if the AB package configuration name is repeated");
                return;
            }
            else
            {
                m_AllFileDir.Add(fileDir.ABName, fileDir.Path);
                m_AllFileAB.Add(fileDir.Path);
                m_ConfigFile.Add(fileDir.Path);
            }
        }
        Debug.Log("------------------------------------------------------------");
        string[] allStr = AssetDatabase.FindAssets("t:Prefab", abConfig.m_AllPrefabPath.ToArray());
        for (int i = 0; i < allStr.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
            Debug.Log($"GUID : {allStr[i]} | PrefabPath : {AssetDatabase.GUIDToAssetPath(allStr[i])}");
            EditorUtility.DisplayProgressBar("Find Prefab Path", "Prefab : ", i * 1.0f / allStr.Length);
            m_ConfigFile.Add(path);
            if (ContainAllFileAB(path) == false)
            {
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                string[] allDepend = AssetDatabase.GetDependencies(path);
                List<string> allDependPath = new List<string>();
                for (int j = 0; j < allDepend.Length; j++)
                {
                    Debug.Log($"Depend : {allDepend[j]}");
                    if (ContainAllFileAB(allDepend[j]) == false && allDepend[j].EndsWith(".cs") == false)
                    {
                        m_AllFileAB.Add(allDepend[j]);
                        allDependPath.Add(allDepend[j]);
                    }
                }
                // Prefab名字重复 返回
                if (m_AllPrefabDir.ContainsKey(obj.name))
                {
                    Debug.Log($"Prefab with the same name exists => Name : {obj.name}");
                    return;
                }
                else
                {
                    m_AllPrefabDir.Add(obj.name, allDependPath);
                }
            }
        }
        foreach (string name in m_AllFileDir.Keys)
        {
            SetABName(name, m_AllFileDir[name]);
        }
        foreach (string name in m_AllPrefabDir.Keys)
        {
            SetABName(name, m_AllPrefabDir[name]);
        }
        // 避免时间过久问题 不进行刷新
        // AssetDatabase.SaveAssets();
        // AssetDatabase.Refresh();
        // 进行打包
        BuildAssetBundle();
        string[] oldABNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < oldABNames.Length; i++)
        {
            AssetDatabase.RemoveAssetBundleName(oldABNames[i], true);
            EditorUtility.DisplayProgressBar("Clear AB Names", $"Name : {oldABNames[i]}", i * 1.0f / oldABNames.Length);
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/AssetBundle/ClearAssetBundle")]
    public static void ClearAssetBundles()
    {
        m_AllFileDir.Clear();
        m_AllFileAB.Clear();
        m_AllPrefabDir.Clear();
        m_ConfigFile.Clear();
        string[] oldABNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < oldABNames.Length; i++)
        {
            AssetDatabase.RemoveAssetBundleName(oldABNames[i], true);
            EditorUtility.DisplayProgressBar("Clear AB Names", $"Name : {oldABNames[i]}", i * 1.0f / oldABNames.Length);
        }
        EditorUtility.ClearProgressBar();
        DirectoryInfo dir = new DirectoryInfo(m_BundleTargetPath);
        FileSystemInfo[] files = dir.GetFileSystemInfos();
        foreach (FileSystemInfo item in files)
        {
            if (item is DirectoryInfo) //判断是否文件夹
            {
                DirectoryInfo subdir = new DirectoryInfo(item.FullName);
                subdir.Delete(true); //删除子目录和文件
            }
            else
            {
                File.Delete(item.FullName);//删除指定文件
            }
        }
        string xmlPath = Application.dataPath + "/AssetBundleConfig.xml";
        if (File.Exists(xmlPath))
        {
            File.Delete($"{xmlPath}.meta");
            File.Delete(xmlPath);
        }
        string bytePath = m_BundleTargetPath + "/AssetBundleConfig.bytes";
        if (File.Exists(bytePath))
        {
            File.Delete($"{bytePath}.meta");
            File.Delete(bytePath);
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();
    }


    public static bool ContainAllFileAB(string path)
    {
        for (int i = 0; i < m_AllFileAB.Count; i++)
        {
            if (path == m_AllFileAB[i] || path.Contains(m_AllFileAB[i]))
            {
                return true;
            }
        }
        return false;
    }

    public static bool ContainABName(string name, string[] strs)
    {
        for (int i = 0; i < strs.Length; i++)
        {
            if (name == strs[i])
            {
                return true;
            }
        }
        return false;
    }

    public static void SetABName(string name, string path)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(path);
        if (assetImporter == null)
        {
            Debug.Log($"This path file does not exist => Name : {path}");
            return;
        }
        else
        {
            assetImporter.assetBundleName = name;
        }

    }

    public static void SetABName(string name, List<string> paths)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            SetABName(name, paths[i]);
        }
    }

    public static void BuildAssetBundle()
    {
        string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
        // key为全路径 value为包名
        Dictionary<string, string> resPathDic = new Dictionary<string, string>();
        for (int i = 0; i < allBundles.Length; i++)
        {
            string[] allBundlePath = AssetDatabase.GetAssetPathsFromAssetBundle(allBundles[i]);
            for (int j = 0; j < allBundlePath.Length; j++)
            {
                if (allBundlePath[j].EndsWith(".cs"))
                {
                    continue;
                }
                Debug.Log($"This AB Name : {allBundles[i]} | Include : {allBundlePath[j]}");
                if (ValidPath(allBundlePath[j]) == true)
                {
                    resPathDic.Add(allBundlePath[j], allBundles[i]);
                }
            }
        }
        DeleteAB();
        // 生成自己的配置表
        WriteData(resPathDic);
        BuildPipeline.BuildAssetBundles(m_BundleTargetPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
    }

    public static void DeleteAB()
    {
        string[] allBundlesName = AssetDatabase.GetAllAssetBundleNames();
        DirectoryInfo direction = new DirectoryInfo(m_BundleTargetPath);
        FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (ContainABName(files[i].Name, allBundlesName) == true || files[i].Name.EndsWith(".meta") == true)
            {
                continue;
            }
            else
            {
                Debug.Log($"This AB package has been renamed or deleted => Name : {files[i].Name}");
                if (File.Exists(files[i].FullName) == true)
                {
                    File.Delete(files[i].FullName);
                }
            }
        }
    }

    public static void WriteData(Dictionary<string, string> resPathDic)
    {
        AssetBundleConfig config = new AssetBundleConfig();
        config.AssetBundleList = new List<ABBase>();
        foreach (string path in resPathDic.Keys)
        {
            ABBase abBase = new ABBase();
            abBase.Path = path;
            abBase.Crc = _CRC32.GetCRC32(path);
            abBase.AssetBundleName = resPathDic[path];
            abBase.AssetName = path.Remove(0, path.LastIndexOf("/") + 1);
            abBase.AssetBundleDependce = new List<string>();
            string[] resDependce = AssetDatabase.GetDependencies(path);
            for (int i = 0; i < resDependce.Length; i++)
            {
                string tempPath = resDependce[i];
                if (tempPath == path || path.EndsWith(".cs"))
                {
                    continue;
                }
                string abName = "";
                if (resPathDic.TryGetValue(tempPath, out abName))
                {
                    if (abName == resPathDic[path])
                    {
                        continue;
                    }
                    if (abBase.AssetBundleDependce.Contains(abName) == false)
                    {
                        abBase.AssetBundleDependce.Add(abName);
                    }
                }
            }
            config.AssetBundleList.Add(abBase);
        }
        // 写入XML
        if (File.Exists(xmlPath))
        {
            File.Delete(xmlPath);
        }
        FileStream fileStream = new FileStream(xmlPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        StreamWriter sw = new StreamWriter(fileStream, System.Text.Encoding.UTF8);
        XmlSerializer xs = new XmlSerializer(config.GetType());
        xs.Serialize(sw, config);
        sw.Close();
        fileStream.Close();
        // 写入二进制
        foreach (ABBase abBase in config.AssetBundleList)
        {
            abBase.Path = "";
        }
        FileStream fs = new FileStream(bytePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, config);
        fs.Close();
    }

    public static bool ValidPath(string path)
    {
        for (int i = 0; i < m_ConfigFile.Count; i++)
        {
            if (path.Contains(m_ConfigFile[i]) == true)
            {
                return true;
            }
        }
        return false;
    }
}
