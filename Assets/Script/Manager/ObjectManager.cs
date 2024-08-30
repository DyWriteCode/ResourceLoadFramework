using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 基于ResourceManager的对象管理
/// </summary>
public class ObjectManager : Singleton<ObjectManager>
{
    // 对象池节点
    public Transform RecyclePoolTrs;
    // 场景节点
    public Transform SceneTrs;
    protected Dictionary<Type, object> m_ClassPoolDic;
    // 暂时存ResObj字典
    protected Dictionary<int, ResourceObj> m_ResourceObjDic = new Dictionary<int, ResourceObj>();
    // 对象池
    protected Dictionary<uint, List<ResourceObj>> m_ObjectPoolDic = new Dictionary<uint, List<ResourceObj>>();
    // ResourceObj对象池
    protected ClassObjectPool<ResourceObj> m_ResourceObjClassPool;
    // 根据异步的GUID储存的resource object 用来判断是否在异步加载
    protected Dictionary<long, ResourceObj> m_AsyncResObj = new Dictionary<long, ResourceObj>();

    public ObjectManager() 
    {
        m_ClassPoolDic = new Dictionary<Type, object>();
    }

    // 创建类对象池 可保存ClassObjectPool 然后调用Spawn和Recyclc来创建和回收类对您
    public ClassObjectPool<T> GetOrCreateClassPool<T>(int maxCount) where T : class, new()
    {
        Type type = typeof(T);
        object outObj = null;
        if (m_ClassPoolDic.TryGetValue(type, out outObj) == false || outObj == null)
        {
            ClassObjectPool<T> newPool = new ClassObjectPool<T>(maxCount);
            m_ClassPoolDic.Add(type, newPool);
            return newPool;
        }
        return outObj as ClassObjectPool<T>;
    }

    // 从对象池中获取为T类型的对象
    // 基本无实际作用
    public T NewClassObjectFromPool<T>(int maxCount) where T : class, new()
    {
        ClassObjectPool<T> pool = GetOrCreateClassPool<T>(maxCount);
        if (pool == null)
        {
            return null;
        }
        return pool.Spawn(true);
    }

    // 从对象池中回收为T类型的对象
    // 基本无实际作用
    public bool RecyclcClassObjectFromPool<T>(int maxCount, T obj) where T : class, new()
    {
        ClassObjectPool<T> pool = GetOrCreateClassPool<T>(maxCount);
        if (pool == null)
        {
            return false;
        }
        return pool.Recycle(obj);
    }

    // 初始化
    internal void Init(Transform recyclePoolTrs, Transform sceneTrs)
    {
        RecyclePoolTrs = recyclePoolTrs;
        SceneTrs = sceneTrs;
        m_ResourceObjClassPool = GetOrCreateClassPool<ResourceObj>(1000);
    }

    // 加载对象
    // 同步加载
    public GameObject InstantiateObject(string path, bool setSceneObj = false, bool bClear = true)
    {
        uint crc = _CRC32.GetCRC32(path);
        ResourceObj resourceObj = GetObjectFromPool(crc);
        if (resourceObj == null)
        {
            resourceObj = m_ResourceObjClassPool.Spawn(true);
            resourceObj.m_crc = crc;
            resourceObj.m_bClear = bClear;
            // ResourceManager提供加载方法
            resourceObj = ResourceManager.Instance.LoadResource(path, resourceObj);
            if (resourceObj.m_resItem.m_Object != null)
            {
                resourceObj.m_cloneObj = GameObject.Instantiate(resourceObj.m_resItem.m_Object) as GameObject;
            }
        }
        if (setSceneObj == true)
        {
            resourceObj.m_cloneObj.transform.SetParent(SceneTrs, false);
        }
        int tempId = resourceObj.m_cloneObj.GetInstanceID();
        if (m_ResourceObjDic.ContainsKey(tempId) == false)
        {
            m_ResourceObjDic.Add(tempId, resourceObj);
        }
        return resourceObj.m_cloneObj;
    }

    // 加载对象
    // 异步加载
    public long InstantiateObjectAsync(string path, OnAsyncObjFinish dealFinish, LoadResPriority priority = LoadResPriority.RES_MIDDLE, bool setSceneObject = false, object param1 = null, object param2 = null, object param3 = null, object param4 = null, object param5 = null, bool bClear = true)
    {
        if (string.IsNullOrEmpty(path) == true)
        {
            return 0;
        }
        uint crc = _CRC32.GetCRC32(path);
        ResourceObj resourceObj = GetObjectFromPool(crc);
        if (resourceObj != null)
        {
            if (setSceneObject == true)
            {
                resourceObj.m_cloneObj.transform.SetParent(SceneTrs, false);
            }
            if (dealFinish != null)
            {
                dealFinish.Invoke(path, resourceObj.m_cloneObj, param1, param2, param3, param4, param5);
            }
            return resourceObj.m_Guid;
        }
        long guid = ResourceManager.Instance.CreateGuid();
        resourceObj = m_ResourceObjClassPool.Spawn(true);
        resourceObj.m_crc = crc;
        resourceObj.m_SetSceneParent = setSceneObject;
        resourceObj.m_bClear = bClear;
        resourceObj.m_DealFinish = dealFinish;
        resourceObj.m_param1 = param1;
        resourceObj.m_param2 = param2;
        resourceObj.m_param3 = param3;
        resourceObj.m_param4 = param4;
        resourceObj.m_param5 = param5;
        m_AsyncResObj.Add(guid, resourceObj);
        ResourceManager.Instance.AsyncLoadResource(path, resourceObj, OnLoadResourceObjectFinish, priority);
        return guid;
    }

    private void OnLoadResourceObjectFinish(string path, ResourceObj resourceObj, object param1 = null, object param2 = null, object param3 = null, object param4 = null, object param5 = null)
    {
        if (resourceObj == null)
        {
            return;
        }
        if (resourceObj.m_resItem.m_Object == null)
        {
#if UNITY_EDITOR
            Debug.LogError("Load failed");
#endif
            return;
        }
        else
        {
            resourceObj.m_cloneObj = GameObject.Instantiate(resourceObj.m_resItem.m_Object) as GameObject;
        }
        if (m_AsyncResObj.ContainsKey(resourceObj.m_Guid))
        {
            m_AsyncResObj.Remove(resourceObj.m_Guid);
        }
        if (resourceObj.m_cloneObj != null && resourceObj.m_SetSceneParent == true)
        {
            resourceObj.m_cloneObj.transform.SetParent(SceneTrs, false);
        }
        if (resourceObj.m_DealFinish != null)
        {
            int tempID = resourceObj.m_cloneObj.GetInstanceID();
            if (m_ResourceObjDic.ContainsKey(tempID) == false)
            {
                m_ResourceObjDic.Add(tempID, resourceObj);
            }
            resourceObj.m_DealFinish.Invoke(path, resourceObj.m_cloneObj, resourceObj.m_param1, resourceObj.m_param2, resourceObj.m_param3, resourceObj.m_param4, resourceObj.m_param5);
        }
    }

    // 释放对象
    public void ReleaseObject(GameObject obj, int maxCacheCount = -1, bool destoryCache = false, bool recycleParent = true)
    {
        if (obj == null)
        {
            return;
        }
        ResourceObj resObj = null;
        int tempId = obj.GetInstanceID();
        if (m_ResourceObjDic.TryGetValue(tempId, out resObj) == false)
        {
            Debug.LogError($"{obj.name}is not created from object manager");
            return;
        }
        if (resObj == null)
        {
            Debug.LogError("the cache res obj is null");
            return;
        }
        if (resObj.m_Already == true)
        {
            Debug.LogError("this obj cached");
            return;
        }
#if UNITY_EDITOR
        obj.name += "(Recycle)";
#endif
        List<ResourceObj> temp = null;
        if (maxCacheCount == 0)
        {
            m_ResourceObjDic.Remove(tempId);
            ResourceManager.Instance.ReleaseResouce(resObj, destoryCache);
            resObj.ReSet();
            m_ResourceObjClassPool.Recycle(resObj);
        }
        else
        {
            if (m_ObjectPoolDic.TryGetValue(resObj.m_crc, out temp) == false || temp == null)
            {
                temp = new List<ResourceObj>();
                m_ObjectPoolDic.Add(resObj.m_crc, temp);
            }
            if (resObj.m_cloneObj != null)
            {
                if (recycleParent == true)
                {
                    resObj.m_cloneObj.transform.SetParent(RecyclePoolTrs);
                }
                else
                {
                    resObj.m_cloneObj.SetActive(false);
                }
            }
            if (maxCacheCount <0 || temp.Count < maxCacheCount)
            {
                temp.Add(resObj);
                resObj.m_Already = true;
                ResourceManager.Instance.DecreaseResouceRef(resObj);
            }
            else
            {
                m_ResourceObjDic.Remove(tempId);
                ResourceManager.Instance.ReleaseResouce(resObj, destoryCache);
                resObj.ReSet();
                m_ResourceObjClassPool.Recycle(resObj);
            }
        }
    }

    // 从对象拾取一个对象通过CRC
    protected ResourceObj GetObjectFromPool(uint crc)
    {
        List<ResourceObj> st = null;
        if (m_ObjectPoolDic.TryGetValue(crc, out st) == true && st != null && st.Count > 0)
        {
            ResourceManager.Instance.IncreaseResouceRef(crc);
            ResourceObj resourceObj = st[0];
            st.RemoveAt(0);
            GameObject obj = resourceObj.m_cloneObj;
            if (ReferenceEquals(obj, null) == false)
            {
                resourceObj.m_Already = false;
#if UNITY_EDITOR
                if (obj.name.EndsWith("(Recycle)") == true)
                {
                    obj.name = obj.name.Replace("(Recycle)", "");
                }
#endif
            }
            return resourceObj;
        }
        return null;
    }

    // 取消异步加载
    public bool CancleLoad(long guid)
    {
        ResourceObj resourceObj = null;
        if (m_AsyncResObj.TryGetValue(guid, out resourceObj) == true && ResourceManager.Instance.CancleLoad(resourceObj) == true)
        {
            m_AsyncResObj.Remove(guid);
            resourceObj.ReSet();
            m_ResourceObjClassPool.Recycle(resourceObj);
            return true;
        }
        return false;
    }

    // 预加载GameObject
    public void PreLoadGameObject(string path, int count = 1, bool clear = false)
    {
        List<GameObject> tempGameObject = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            GameObject gameObject = InstantiateObject(path, false, clear);
            tempGameObject.Add(gameObject);
        }
        for (int i = 0; i < count; i++)
        {
            GameObject gameObject = tempGameObject[i];
            ReleaseObject(gameObject);
            gameObject = null;
        }
        tempGameObject.Clear();
    }

    // 是否正在AsyncLoad
    public bool IsingAsyncLoad(long guid)
    {
        return m_AsyncResObj[guid] != null;
    }

    // 是否是object manager创建的对象
    public bool IsObjectManagerCreate(GameObject obj) 
    {
        ResourceObj resourceObj = m_ResourceObjDic[obj.GetInstanceID()];
        if (resourceObj == null)
        {
            return false;
        }
        return true;
    }

    // 清空对象池
    public void ClearCache()
    {
        List<uint> cacheList = new List<uint>();
        foreach (uint key in m_ObjectPoolDic.Keys)
        {
            List<ResourceObj> tempList = m_ObjectPoolDic[key];
            for (int i = tempList.Count - 1;i >= 0;i--)
            {
                ResourceObj tempObj = tempList[i];
                if (System.Object.ReferenceEquals(tempObj.m_cloneObj, null) == false && tempObj.m_bClear == true)
                {
                    GameObject.Destroy(tempObj.m_cloneObj);
                    m_ResourceObjDic.Remove(tempObj.m_cloneObj.GetInstanceID());
                    tempObj.ReSet();
                    m_ResourceObjClassPool.Recycle(tempObj);
                }
            }
            if (tempList.Count <= 0)
            {
                cacheList.Add(key);
            }
        }
        for (int i = 0; i < cacheList.Count; i++)
        {
            uint temp = cacheList[i];
            if (m_ObjectPoolDic.ContainsKey(temp) == true)
            {
                m_ObjectPoolDic.Remove(temp);
            }
        }
        cacheList.Clear();
    }

    // 清除某个对象在对象池里面所有的对象
    public void ClearPoolObject(uint crc)
    {
        List<ResourceObj> tempList = null;
        if (m_ObjectPoolDic.TryGetValue(crc, out tempList) == false || tempList == null)
        {
            return;
        }
        for (int i = tempList.Count - 1; i >= 0; i--)
        {
            ResourceObj temp = tempList[i];
            if (temp.m_bClear == true)
            {
                tempList.Remove(temp);
                int tempId = temp.m_cloneObj.GetInstanceID();
                GameObject.Destroy(temp.m_cloneObj);
                temp.ReSet();
                m_ResourceObjDic.Remove(tempId);
                m_ResourceObjClassPool.Recycle(temp);
            }
        }
        if (tempList.Count <= 0)
        {
            m_ObjectPoolDic.Remove(crc);
        }
    }
}
