using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 双向链表类
/// </summary>
public class DoubleLinkedList<T> where T : class, new()
{
    // 前一个节点
    public DoubleLinkedListNode<T> Head = null;
    // 最后后一个节点
    public DoubleLinkedListNode<T> Tail = null;
    // 双向链表结构对象池
    public ClassObjectPool<DoubleLinkedListNode<T>> m_DoubleLinkedNodePool = ObjectManager.Instance.GetOrCreateClassPool<DoubleLinkedListNode<T>>(500);
    // 节点的总个数
    protected int m_Count = 0;
    public int Count
    {
        get 
        { 
            return m_Count; 
        } 
    }

    // 添加一个节点到头部
    public DoubleLinkedListNode<T> AddToHeader(T t)
    {
        DoubleLinkedListNode<T> pList = m_DoubleLinkedNodePool.Spawn(true);
        pList.prev = null;
        pList.next = null;
        pList.t = t;
        return AddToHeader(pList);
    }

    // 添加一个节点到头部
    public DoubleLinkedListNode<T> AddToHeader(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null)
        {
            return null;
        }
        pNode.prev = null;
        if (Head == null)
        {
            Head = pNode;
            Tail = pNode;
        }
        else
        {
            pNode.next = Head;
            Head.prev = pNode;
            Head = pNode;
        }
        m_Count++;
        return Head;
    }

    // 添加一个节点到尾部
    public DoubleLinkedListNode<T> AddToTail(T t)
    {
        DoubleLinkedListNode<T> pList = m_DoubleLinkedNodePool.Spawn(true);
        pList.prev = null;
        pList.next = null;
        pList.t = t;
        return AddToTail(pList);
    }

    // 添加一个节点到尾部
    public DoubleLinkedListNode<T> AddToTail(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null)
        {
            return null;
        }
        pNode.next = null;
        if (Head == null)
        {
            Head = pNode;
            Tail = pNode;
        }
        else
        {
            pNode.prev = Tail;
            Tail.next = pNode;
            Tail = pNode;
        }
        m_Count++;
        return Tail;
    }

    // 移除掉某一个节点
    public void RemoveNode(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null)
        {
            return;
        }
        if (pNode == Head)
        {
            Head = pNode.next;
        }
        if (pNode == Tail)
        {
            Tail = pNode.prev;
        }
        if (pNode.next != null)
        {
            pNode.prev.next = pNode.next;
        }
        if (pNode.prev != null)
        {
            pNode.next.prev = pNode.prev;
        }
        pNode.prev = pNode.next = null;
        pNode.t = null;
        m_DoubleLinkedNodePool.Recycle(pNode);
        m_Count--;
    }

    // 把某一个节点移至头部
    public void MoveToHead(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null || pNode == Head)
        {
            return;
        }
        if (pNode.prev == null && pNode.next == Head)
        {
            return;
        }
        if (pNode == Tail)
        {
            Tail = pNode.prev; 
        }
        if (pNode.prev != Tail)
        {
            pNode.prev.next = pNode.next;
        }
        if (pNode.next != Tail)
        {
            pNode.next.prev = pNode.prev;
        }
        pNode.prev = Tail;
        pNode.next = Head;
        pNode.prev = pNode;
        Head = pNode;
        if (Tail == null)
        {
            Tail = Head;
        }
    }
}

/// <summary>
/// 双向链表节点类
/// </summary>
public class DoubleLinkedListNode<T> where T : class, new()
{
    // 前一个节点
    public DoubleLinkedListNode<T> prev = null;
    // 后一个节点
    public DoubleLinkedListNode<T> next = null;
    // 当前节点
    public T t = null;
}

/// <summary>
/// 对双向链表进行封装
/// </summary>
public class CMapList<T> where T : class, new()
{
    DoubleLinkedList<T> m_Dlink = new DoubleLinkedList<T>();
    Dictionary<T, DoubleLinkedListNode<T>> m_FindMap = new Dictionary<T, DoubleLinkedListNode<T>>();

    ~CMapList() 
    {
        Clear();
    }

    // 清空整个双向链表
    public void Clear()
    {
        while (m_Dlink.Tail != null)
        {
            Remove(m_Dlink.Tail.t);
        }
    }

    // 插入一个节点到表头
    public void InsertToHead(T t)
    {
        DoubleLinkedListNode<T> node = null;
        if (m_FindMap.TryGetValue(t, out node) == true && node != null)
        {
            m_Dlink.AddToHeader(node);
            return;
        }
        m_Dlink.AddToHeader(t);
        m_FindMap.Add(t, m_Dlink.Head);
    }

    // 从表尾弹出一个节点
    public void Pop()
    {
        if (m_Dlink.Tail != null)
        {
            Remove(m_Dlink.Tail.t);
        }
    }

    // 删除某个节点
    public void Remove(T t)
    {
        DoubleLinkedListNode<T> node = null;
        if (m_FindMap.TryGetValue(t, out node) == false || node == null)
        {
            return;
        }
        m_Dlink.RemoveNode(node);
        m_FindMap.Remove(t);
    }

    // 返回尾部最后一个节点
    public T Back()
    {
        if (m_Dlink.Tail == null)
        {
            return null;
        }
        else
        {
            return m_Dlink.Tail.t;
        }
    }

    // 获取双向列表的个数
    public int Size()
    {
        return m_FindMap.Count;
    }

    // 获取双向列表的个数
    public int Len()
    {
        return m_FindMap.Count;
    }

    // 查找是否包含这个节点
    public bool Find(T t)
    {
        DoubleLinkedListNode<T> node = null;
        if (m_FindMap.TryGetValue(t, out node) == false || node == null)
        {
            return false;
        }
        return true;
    }

    // 刷新某一个节点并把节点移动至头部
    public bool Reflesh(T t)
    {
        DoubleLinkedListNode<T> node = null;
        if (m_FindMap.TryGetValue(t, out node) == false || node == null)
        {
            return false;
        }
        m_Dlink.MoveToHead(node);
        return true;
    }
}

/// <summary>
/// 资源加载优先级
/// </summary>
public enum LoadResPriority
{
    RES_HIGHEST = 0, // 最高优先级
    RES_MIDDLE, // 一般优先级
    RES_SLOWEST, // 最低优先级
    RES_NUM,
}

/// <summary>
/// 异步加载资源传入参数类
/// </summary>
public class AsyncLoadResParam
{
    public List<AsyncCallack> m_CallbackList = new List<AsyncCallack>();
    public uint m_Crc = 0;
    public bool m_Sprite = false;
    public string m_Path = string.Empty;
    public LoadResPriority m_Priority = LoadResPriority.RES_SLOWEST;
    // 在对象池中用到所以说要设置一个reset方法
    public void Reset()
    {
        m_Crc = 0;
        m_Path = string.Empty;
        m_Sprite = false;
        m_Priority = LoadResPriority.RES_SLOWEST;
        m_CallbackList.Clear();
    }
}

/// <summary>
/// 异步回调函数类
/// </summary>
public class AsyncCallack
{
    public OnAsyncObjFinish m_DealFinish = null;
    public object param1 = null;
    public object param2 = null;
    public object param3 = null;
    public object param4 = null;
    public object param5 = null;

    public void ReSet()
    {
        m_DealFinish = null;
        param1 = null;
        param2 = null;
        param3 = null;
        param4 = null;
        param5 = null;
    }
}

/// <summary>
/// 异步加载回调
/// </summary>
public delegate void OnAsyncObjFinish(string path, Object obj, object param1 = null, object param2 = null, object param3 = null, object param4 = null, object param5 = null);

/// <summary>
/// 基于AssetBundle的资源管理
/// </summary>
public class ResourceManager : Singleton<ResourceManager>
{
    public bool m_LoadFromAssetBundle = true;
    // 缓存使用的资源列表
    // 下面.net 4.0之后属性新写法 虽然说时候我也不知道为什么这样写 但写成属性试验没问题
    public Dictionary<uint, ResourceItem> AssetDic { get; set; } = new Dictionary<uint, ResourceItem>();
    // 没有引用的资源块 达到缓存最大时释放这个列表里面没用的资源块
    protected CMapList<ResourceItem> m_NoRefrenceAssetMapList = new CMapList<ResourceItem>();
    // Mono脚本
    protected MonoBehaviour m_MonoStart;
    // 正在异步加载资源的列表
    protected List<AsyncLoadResParam>[] m_LoadingAssetList = new List<AsyncLoadResParam>[(int)LoadResPriority.RES_NUM];
    // 正在异步加载的dictionary
    protected Dictionary<uint, AsyncLoadResParam> m_LoadingAssetDic = new Dictionary<uint, AsyncLoadResParam>();
    // 异步回调函数对象池
    protected ClassObjectPool<AsyncCallack> m_AsyncCallackPool = new ClassObjectPool<AsyncCallack>(200);
    // 异步回调参数的对象池
    protected ClassObjectPool<AsyncLoadResParam> m_AsyncLoadResParamPool = new ClassObjectPool<AsyncLoadResParam>(100);
    // 连续卡着加载的最长时间 单位微秒
    private long MAXLOADRESTIME = 200000;

    public void Init(MonoBehaviour mono)
    {
        for (int i = 0;i < (int)LoadResPriority.RES_NUM;i++)
        {
            m_LoadingAssetList[i] = new List<AsyncLoadResParam>();
        }
        m_MonoStart = mono;
        m_MonoStart.StartCoroutine(AsyncLoadCor());
    }

    // 同步资源加载方法 从AB包中加载
    // 外部直接调用 且只加载不需要实例化的资源 如音频文字等
    public T LoadResource<T>(string path) where T : UnityEngine.Object
    {
        // 使用这个函数判断字符串是否为空效率会高上很多
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }
        uint crc = _CRC32.GetCRC32(path);
        ResourceItem item = GetCacheResourceItem(crc);
        if (item != null)
        {
            return item.m_Obj as T;
        }
        T obj = null;
#if UNITY_EDITOR 
        if (m_LoadFromAssetBundle == false)
        {
            obj = LoadAssetByEditor<T>(path);
            if (item != null)
            {
                if (item.m_Obj != null)
                {
                    obj = item.m_Obj as T;

                }
            }
            else
            {
                item = AssetBundleManager.Instance.FindResourceItem(crc);
            }
        }
#endif
        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResouceAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                {
                    obj = item.m_Obj as T;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<T>(item.m_AssetName);
                }
            }
        }
        CacheResourceItem(path, ref item, crc, obj);
        return obj;
    }

#if UNITY_EDITOR
    // 通过editor内置API从editor加载
    protected T LoadAssetByEditor<T>(string path) where T : UnityEngine.Object
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
    }
#endif

    // 从缓存的Item列表中获取对应的resource item
    public ResourceItem GetCacheResourceItem(uint crc, int addRefCount = 1)
    {
        ResourceItem item = null;
        if (AssetDic.TryGetValue(crc, out item) == true)
        {
            if (item != null)
            {
                item.RefCount += addRefCount;
                item.m_LastUseTime = Time.realtimeSinceStartup;
                if (item.RefCount <= 1)
                {
                    m_NoRefrenceAssetMapList.Remove(item);
                }
            }
        }
        return item;
    }

    // 清除缓存 缓存太多了
    protected void WashOut()
    {
        // 当目前内存使用超过80%的时候进行清除缓存操作
        //while () 
        //{
        //    if (m_NoRefrenceAssetMapList.Size() <= 0)
        //    {
        //        break;
        //    }
        //    ResourceItem item = m_NoRefrenceAssetMapList.Back();
        //    DestoryResouceItme(item, true);
        //    m_NoRefrenceAssetMapList.Pop();
        //}
    }

    // 删除/回收掉资源块
    protected void DestoryResouceItme(ResourceItem item, bool destroyCache = false)
    {
        if (item == null || item.RefCount > 0)
        {
            return;
        }
        if (destroyCache == false)
        {
            m_NoRefrenceAssetMapList.InsertToHead(item);
        }
        if (AssetDic.Remove(item.m_crc) == false)
        {
            return;
        }
        // 释放asset bundle
        AssetBundleManager.Instance.ReleaseAssetBundle(item);
        if (item.m_Obj != null)
        {
            item.m_Obj = null;
        }
    }

    // 缓存资源块
    public void CacheResourceItem(string path, ref ResourceItem item, uint crc, Object obj, int addRefCount = 1)
    {
        // 缓存太多了清除掉部分没有用的资源
        WashOut();
        if (item == null)
        {
            Debug.LogError($"ResourceLoad Fail, Path: {path}");
        }
        if (obj == null)
        {
            Debug.LogError($"ResourceLoad Fail, Path: {path}");
        }
        item.m_Obj = obj;
        item.m_crc = crc;
        item.m_Guid = obj.GetInstanceID();
        item.m_LastUseTime = Time.realtimeSinceStartup;
        item.RefCount += addRefCount;
        ResourceItem oldItem = null;
        if (AssetDic.TryGetValue(item.m_crc, out oldItem) == true)
        {
            AssetDic[item.m_crc] = item;
        }
        else
        {
            AssetDic.Add(item.m_crc, item);
        }

    }

    // 释放/卸载资源(不用实例化的资源)
    public bool ReleaseResouce(Object obj, bool destroyCache = false)
    {
        if (obj == null)
        {
            return false;
        }
        ResourceItem item = null;
        foreach(ResourceItem res in AssetDic.Values)
        {
            if (res.m_Guid == obj.GetInstanceID())
            {
                item = res;
            }
        }
        if (item == null)
        {
            return false;
        }
        item.RefCount--;
        DestoryResouceItme(item);
        return true;
    }

    // 异步加载
    IEnumerator AsyncLoadCor()
    {
        List<AsyncCallack> callbackList = new List<AsyncCallack>();
        // 上一次yield的时间
        long lastYieldTime = System.DateTime.Now.Ticks;
        while (true)
        {
            bool haveYield = false;
            for (int i = 0; i < (int)LoadResPriority.RES_NUM; i++)
            {
                List<AsyncLoadResParam> loadingList = m_LoadingAssetList[i];
                if (loadingList == null || loadingList.Count <= 0)
                {
                    continue;
                }
                AsyncLoadResParam loadingItem = loadingList[0];
                loadingList.RemoveAt(0);
                callbackList = loadingItem.m_CallbackList;
                Object obj = null;
                ResourceItem item = null;
#if UNITY_EDITOR
                if (m_LoadFromAssetBundle == false)
                {
                    obj = LoadAssetByEditor<Object>(loadingItem.m_Path);
                    // 模拟异步加载
                    yield return new WaitForSeconds(0.1f);
                    item = AssetBundleManager.Instance.FindResourceItem(loadingItem.m_Crc);
                }
#endif
                if (obj == null)
                {
                    item = AssetBundleManager.Instance.LoadResouceAssetBundle(loadingItem.m_Crc);
                    if (item != null && item.m_AssetBundle != null)
                    {
                        AssetBundleRequest assetBundleRequest = null;
                        if (loadingItem.m_Sprite == true)
                        {
                            assetBundleRequest = item.m_AssetBundle.LoadAssetAsync<Sprite>(item.m_AssetName);
                        }
                        else
                        {
                            assetBundleRequest = item.m_AssetBundle.LoadAssetAsync(item.m_AssetName);
                        }
                        yield return assetBundleRequest;
                        if (assetBundleRequest.isDone == true)
                        {
                            obj = assetBundleRequest.asset;
                        }
                        lastYieldTime = System.DateTime.Now.Ticks;
                    }
                }
                CacheResourceItem(loadingItem.m_Path, ref item, loadingItem.m_Crc, obj, callbackList.Count);
                for (int j = 0; j < callbackList.Count; j++)
                {
                    AsyncCallack callack = callbackList[j];
                    if (callack != null && callack.m_DealFinish != null)
                    {
                        callack.m_DealFinish?.Invoke(loadingItem.m_Path, obj, callack.param1, callack.param2, callack.param3, callack.param4, callack.param5);
                    }
                    callack.ReSet();
                    m_AsyncCallackPool.Recycle(callack);
                }
                obj = null;
                callbackList.Clear();
                m_LoadingAssetDic.Remove(loadingItem.m_Crc);
                loadingItem.Reset();
                m_AsyncLoadResParamPool.Recycle(loadingItem);
                if (System.DateTime.Now.Ticks - lastYieldTime > MAXLOADRESTIME)
                {
                    yield return null;
                    lastYieldTime = System.DateTime.Now.Ticks;
                    haveYield = true;
                }
            }
            if (haveYield == false || System.DateTime.Now.Ticks - lastYieldTime > MAXLOADRESTIME)
            {
                lastYieldTime = System.DateTime.Now.Ticks;
                yield return null;
            }
        }
    }

    // 异步加载资源
    // 只加载不需要实例化的资源 如音频文字等
    public void AsyncLoadResource(string path, OnAsyncObjFinish dealFinish, LoadResPriority priority = LoadResPriority.RES_SLOWEST, object param1 = null, object param2 = null, object param3 = null, object param4 = null, object param5 = null, uint crc = 0)
    {
        if (crc == 0)
        {
            crc = _CRC32.GetCRC32(path);
        }
        ResourceItem item = GetCacheResourceItem(crc);
        if (item != null)
        {
            if (dealFinish != null)
            {
                dealFinish(path, item.m_Obj, param1, param2, param3, param4, param5);
            }
            return;
        }
        // 判断是否正在加载
        AsyncLoadResParam param = null;
        if (m_LoadingAssetDic.TryGetValue(crc, out param) == false || param == null)
        {
            param = m_AsyncLoadResParamPool.Spawn(true);
            param.m_Crc = crc;
            param.m_Path = path;
            param.m_Priority = priority;
            m_LoadingAssetDic.Add(crc, param);
            m_LoadingAssetList[(int)priority].Add(param);
        }
        // 向回调资源里面添加回调
        AsyncCallack callack = m_AsyncCallackPool.Spawn(true);
        callack.m_DealFinish = dealFinish;
        callack.param1 = param1;
        callack.param2 = param2;
        callack.param3 = param3;
        callack.param4 = param4;
        callack.param5 = param5;
        param.m_CallbackList.Add(callack);
    }
}
