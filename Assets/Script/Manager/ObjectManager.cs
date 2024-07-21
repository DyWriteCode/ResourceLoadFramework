using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 基于ResourceManager的对象管理
/// </summary>
public class ObjectManager : Singleton<ObjectManager>
{
    protected Dictionary<Type, object> m_ClassPoolDic;

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
}
