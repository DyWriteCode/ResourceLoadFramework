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
    // 对象池
    protected Dictionary<uint, List<ResourceObj>> m_ObjectPoolDic = new Dictionary<uint, List<ResourceObj>>();
    // ResourceObj对象池
    protected ClassObjectPool<ResourceObj> m_ResourceObjClassPool;

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
        return resourceObj.m_cloneObj;
    }

    // 从对象拾取一个对象通过CRC
    protected ResourceObj GetObjectFromPool(uint crc)
    {
        List<ResourceObj> st = null;
        if (m_ObjectPoolDic.TryGetValue(crc, out st) == true && st != null && st.Count > 0)
        {
            ResourceObj resourceObj = st[0];
            st.RemoveAt(0);
            GameObject obj = resourceObj.m_cloneObj;
            if (ReferenceEquals(obj, null) == false)
            {
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
}
