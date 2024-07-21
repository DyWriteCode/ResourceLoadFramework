using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 类对象池基类
/// </summary>
public class ClassObjectPool<T> where T : class, new()
{
    // 类对象池
    protected Stack<T> m_Pool = new Stack<T>();
    // 最大对象个数 <=0 表示不限个数
    protected int m_MaxCount = 0;
    // 没有回收对象个数
    protected int m_NoRecycleCount = 0;

    // 初始化函数
    public ClassObjectPool(int maxCount)
    {
        m_MaxCount = maxCount;
        for (int i = 0; i < m_MaxCount; i++)
        {
            m_Pool.Push(new T());
        }
    }

    // 从池内取对象
    public T Spawn(bool createIfPoolEmpty = false)
    {
        if (m_Pool.Count > 0)
        {
            T rtn = m_Pool.Pop();
            if (rtn == null)
            {
                if (createIfPoolEmpty == true)
                {
                    rtn = new T();
                }
            }
            m_NoRecycleCount++;
            return rtn;
        }
        else
        {
            if (createIfPoolEmpty == true)
            {
                T rtn = new T();
                m_NoRecycleCount++;
                return rtn;
            }
        }
        return null;
    }

    // 从池内回收对象
    public bool Recycle(T obj)
    {
        if (obj == null)
        {
            return false;
        }
        m_NoRecycleCount--;
        if (m_Pool.Count >= m_MaxCount && m_MaxCount > 0)
        {
            obj = null;
            return false;
        }
        m_Pool.Push(obj);
        return true;
    }
}
