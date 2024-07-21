using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class TestLoad : MonoBehaviour
{
    void Start()
    {
        Load();
    }

    void Load()
    {
        //TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/GameData/Data/ABData/AssetBundleConfig.bytes");
        //AssetBundle configAB = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/AssetBundle/assetbundleconfig");
        //TextAsset textAsset = configAB.LoadAsset<TextAsset>("AssetBundleConfig");
        //MemoryStream stream = new MemoryStream(textAsset.bytes);
        //BinaryFormatter bf = new BinaryFormatter();
        //AssetBundleConfig testSerilize = (AssetBundleConfig)bf.Deserialize(stream);
        //stream.Close();
        //string path = "Assets/GameData/Prefabs/Attack.prefab";
        //uint crc = _CRC32.GetCRC32(path);
        //ABBase abBase = null;
        //for (int i = 0; i < testSerilize.AssetBundleList.Count; i++)
        //{
        //    if (testSerilize.AssetBundleList[i].Crc == crc)
        //    {
        //        abBase = testSerilize.AssetBundleList[i];
        //    }
        //}
        //for (int i = 0; i < abBase.AssetBundleDependce.Count; i++)
        //{
        //    AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/AssetBundle/" + abBase.AssetBundleDependce[i]);
        //}
        //AssetBundle assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/AssetBundle/" + abBase.AssetBundleName);
        //GameObject obj = GameObject.Instantiate(assetBundle.LoadAsset<GameObject>(abBase.AssetName));
    }
}
