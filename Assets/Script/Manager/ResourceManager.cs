using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
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
    public List<AsyncCallback> m_CallbackList = new List<AsyncCallback>();
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
public class AsyncCallback
{
    // 加载完成的回调
    public OnAsyncObjFinish m_DealObjFinish = null;
    public ResourceObj m_resourceObj = null;
    // 加载完成的回调
    public OnAsyncFinish m_DealFinish = null;
    public object m_param1 = null;
    public object m_param2 = null;
    public object m_param3 = null;
    public object m_param4 = null;
    public object m_param5 = null;

    public void ReSet()
    {
        m_DealFinish = null;
        m_DealObjFinish = null;
        m_resourceObj = null;
        m_param1 = null;
        m_param2 = null;
        m_param3 = null;
        m_param4 = null;
        m_param5 = null;
    }
}

/// <summary>
/// 异步加载回调
/// </summary>
public delegate void OnAsyncObjFinish(string path, Object obj, object param1 = null, object param2 = null, object param3 = null, object param4 = null, object param5 = null);
/// <summary>
/// 实例化加载回调
/// </summary>
public delegate void OnAsyncFinish(string path, ResourceObj resourceObj, object param1 = null, object param2 = null, object param3 = null, object param4 = null, object param5 = null);

/// <summary>
/// 游戏对象加载单位
/// </summary>
public class ResourceObj 
{
    // crc
    public uint m_crc = 0;
    // 为实例化的资源 ResourceItem
    public ResourceItem m_resItem = null;
    // 实例化的GameObject
    public GameObject m_cloneObj = null;
    // 是否跳场景清除
    public bool m_bClear = true;
    // 存储GUID
    public long m_Guid = 0;
    // 是否已经放回对象池
    public bool m_Already = false;
    // 是否设置父对象
    public bool m_SetSceneParent = false;
    // 实例化完成之后的回调
    public OnAsyncObjFinish m_DealFinish = null;
    // 异步参数
    public object m_param1 = null;
    public object m_param2 = null; 
    public object m_param3 = null; 
    public object m_param4 = null;
    public object m_param5 = null;

    // 重置
    public void ReSet()
    {
        m_crc = 0;
        m_resItem = null;
        m_cloneObj = null;
        m_bClear = true;
        m_Guid = 0;
        m_Already = false;
        m_SetSceneParent = false;
        m_DealFinish = null;
        m_param1 = null;
        m_param2 = null;
        m_param3 = null;
        m_param4 = null;
        m_param5 = null;
}
}

/// <summary>
/// 基于AssetBundle的资源管理
/// </summary>
public class ResourceManager : Singleton<ResourceManager>
{
    protected long m_Guid = Random.Range(123456789, 987654321);
    public bool m_LoadFromAssetBundle = false;
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
    protected ClassObjectPool<AsyncCallback> m_AsyncCallackPool = new ClassObjectPool<AsyncCallback>(200);
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

    // 创建唯一的GUID 
    public long CreateGuid()
    {
        return m_Guid++;
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
            return item.m_Object as T;
        }
        T obj = null;
#if UNITY_EDITOR 
        if (m_LoadFromAssetBundle == false)
        {
            //obj = LoadAssetByEditor<T>(path);
            //if (item != null)
            //{
            //    if (item.m_Object != null)
            //    {
            //        obj = item.m_Object as T;
            //    }
            //}
            //else
            //{
            //    item = AssetBundleManager.Instance.FindResourceItem(crc);
            //}
            item = AssetBundleManager.Instance.FindResourceItem(crc);
            if (item.m_Object != null)
            {
                obj = item.m_Object as T;
            }
            else
            {
                obj = LoadAssetByEditor<T>(path);
            }
        }
#endif
        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResouceAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Object != null)
                {
                    obj = item.m_Object as T;
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

    // 同步加载资源 只需要加载需要实例化的资源如game object
    public ResourceObj LoadResource(string path, ResourceObj resObj)
    {
        if (resObj == null) 
        {
            return null;
        }
        uint crc = resObj.m_crc == 0 ? _CRC32.GetCRC32(path) : resObj.m_crc;
        ResourceItem item = GetCacheResourceItem(crc);
        if (item != null)
        {
            resObj.m_resItem = item;
            return resObj;
        }
        Object obj = null;
#if UNITY_EDITOR
        if (m_LoadFromAssetBundle == false)
        {
            //obj = LoadAssetByEditor<Object>(path);
            //if (item != null)
            //{
            //    if (item.m_Object != null)
            //    {
            //        obj = item.m_Object as Object;
            //    }
            //}
            //else
            //{
            //    item = AssetBundleManager.Instance.FindResourceItem(crc);
            //}
            item = AssetBundleManager.Instance.FindResourceItem(crc);
            if (item.m_Object != null)
            {
                obj = item.m_Object as Object;
            }
            else
            {
                obj = LoadAssetByEditor<Object>(path);
            }
        }
#endif
        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResouceAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Object != null)
                {
                    obj = item.m_Object as Object;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<Object>(item.m_AssetName);
                }
            }
        }
        CacheResourceItem(path, ref item, crc, obj);
        resObj.m_resItem = item;
        resObj.m_bClear = item.m_Clear;
        return resObj;
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
        if (AssetDic.Remove(item.m_crc) == false)
        {
            return;
        }
        if (destroyCache == false)
        {
            // m_NoRefrenceAssetMapList.InsertToHead(item);
            return;
        }
        // 释放asset bundle
        AssetBundleManager.Instance.ReleaseAssetBundle(item);
        ObjectManager.Instance.ClearPoolObject(item.m_crc);
        if (item.m_Object != null)
        {
            item.m_Object = null;
#if UNITY_EDITOR
            Resources.UnloadUnusedAssets();
#endif
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
        item.m_Object = obj;
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

    // 卸载资源
    public bool ReleaseResouce(ResourceObj resObj, bool destroyCache = false)
    {
        if (resObj == null)
        {
            return false;
        }
        ResourceItem item = null;
        if (AssetDic.TryGetValue(resObj.m_crc, out item) == false || item == null)
        {
            Debug.Log($"The resource does not exist or has been released multiple times : {resObj.m_cloneObj.name}");
            return false;
        }
        GameObject.Destroy(resObj.m_cloneObj);
        item.RefCount--;
        DestoryResouceItme(item);
        return true;
    }

    /// <summary>
    /// 释放/卸载资源(不用实例化的资源)
    /// </summary>
    /// <param name="obj">要卸载资源的对象</param>
    /// <param name="destroyCache">是否删除缓存</param>
    /// <returns>是否卸载成功</returns>
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

    /// <summary>
    /// 释放/卸载资源(不用实例化的资源)
    /// </summary>
    /// <param name="path">要卸载资源的对象路径</param>
    /// <param name="destroyCache">是否删除缓存</param>
    /// <returns>是否卸载成功</returns>
    public bool ReleaseResouce(string path, bool destroyCache = false)
    {
        if (string.IsNullOrEmpty(path) == true)
        {
            return false;
        }
        uint crc = _CRC32.GetCRC32(path);
        ResourceItem item = null;
        // start
        //foreach (var items in AssetDic)
        //{
        //    Debug.Log(items.Key);
        //    Debug.Log(crc);
        //}
        // end
        if (AssetDic.TryGetValue(crc, out item) == false || item == null)
        {
            Debug.Log($"The resource does not exist or has been released multiple times : {path}");
            return false;
        }
        item.RefCount--;
        DestoryResouceItme(item);
        return true;
    }

    // 异步加载
    private IEnumerator AsyncLoadCor()
    {
        List<AsyncCallback> callbackList = new List<AsyncCallback>();
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
                    AsyncCallback callback = callbackList[j];
                    if (callback != null && callback.m_DealFinish != null && callback.m_resourceObj != null)
                    {
                        ResourceObj tempResObj = callback.m_resourceObj;
                        tempResObj.m_resItem = item;
                        callback.m_DealFinish?.Invoke(loadingItem.m_Path, tempResObj, tempResObj.m_param1, tempResObj.m_param2, tempResObj.m_param3, tempResObj.m_param4, tempResObj.m_param5);
                        callback.m_DealFinish = null;
                        tempResObj = null;
                    }
                    if (callback != null && callback.m_DealObjFinish != null)
                    {
                        callback.m_DealObjFinish?.Invoke(loadingItem.m_Path, obj, callback.m_param1, callback.m_param2, callback.m_param3, callback.m_param4, callback.m_param5);
                        callback.m_DealObjFinish = null;
                    }
                    callback.ReSet();
                    m_AsyncCallackPool.Recycle(callback);
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
    // 加载实例化的资源
    public void AsyncLoadResource(string path, ResourceObj resourceObj, OnAsyncFinish dealFinish, LoadResPriority priority = LoadResPriority.RES_MIDDLE)
    {
        ResourceItem item = GetCacheResourceItem(resourceObj.m_crc);
        if (item != null)
        {
            resourceObj.m_resItem = item;
            if (dealFinish != null)
            {
                dealFinish.Invoke(path, resourceObj, resourceObj.m_param1, resourceObj.m_param2, resourceObj.m_param3, resourceObj.m_param4, resourceObj.m_param5);
            }
        }
        // 判断是否正在加载
        AsyncLoadResParam param = null;
        if (m_LoadingAssetDic.TryGetValue(resourceObj.m_crc, out param) == false || param == null)
        {
            param = m_AsyncLoadResParamPool.Spawn(true);
            param.m_Crc = resourceObj.m_crc;
            param.m_Path = path;
            param.m_Priority = priority;
            m_LoadingAssetDic.Add(resourceObj.m_crc, param);
            m_LoadingAssetList[(int)priority].Add(param);
        }
        // 向回调资源里面添加回调
        AsyncCallback callback = m_AsyncCallackPool.Spawn(true);
        callback.m_DealFinish = dealFinish;
        callback.m_resourceObj = resourceObj;
        param.m_CallbackList.Add(callback);
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
                dealFinish.Invoke(path, item.m_Object, param1, param2, param3, param4, param5);
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
        AsyncCallback callack = m_AsyncCallackPool.Spawn(true);
        callack.m_DealObjFinish = dealFinish;
        callack.m_param1 = param1;
        callack.m_param2 = param2;
        callack.m_param3 = param3;
        callack.m_param4 = param4;
        callack.m_param5 = param5;
        param.m_CallbackList.Add(callack);
    }

    /// <summary>
    /// 清除缓存
    /// </summary>
    public void ClearCache()
    {
        //while (m_NoRefrenceAssetMapList.Size() > 0)
        //{
        //    ResourceItem item = m_NoRefrenceAssetMapList.Back();
        //    // DestoryResouceItme(item, item.m_Clear);
        //    DestoryResouceItme(item, true);
        //    m_NoRefrenceAssetMapList.Pop();
        //}
        List<ResourceItem> tempList = new List<ResourceItem>();
        foreach (var item in AssetDic.Values)
        {
            if (item.m_Clear == true)
            {
                tempList.Add(item);
            }
        }
        foreach (var item in tempList)
        {
            DestoryResouceItme(item, item.m_Clear);
        }
        tempList.Clear();
    }

    // 取消异步加载
    public bool CancleLoad(ResourceObj resource)
    {
        AsyncLoadResParam param = null;
        if (m_LoadingAssetDic.TryGetValue(resource.m_crc, out param) == true && m_LoadingAssetList[(int)param.m_Priority].Contains(param) == true)
        {
            for (int i = param.m_CallbackList.Count;i >= 0;i --)
            {
                AsyncCallback callback = param.m_CallbackList[i];
                if (callback != null && resource == callback.m_resourceObj)
                {
                    callback.ReSet();
                    m_AsyncCallackPool.Recycle(callback);
                    param.m_CallbackList.Remove(callback);
                }
            }
            if (param.m_CallbackList.Count <= 0)
            {
                param.Reset();
                m_LoadingAssetList[(int)param.m_Priority].Remove(param);
                m_AsyncLoadResParamPool.Recycle(param);
                m_LoadingAssetDic.Remove(resource.m_crc);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 预加载资源
    /// 本质上是事先加载了在卸载但不删除
    /// </summary>
    public void PreloadResource(string path)
    {
        if (string.IsNullOrEmpty(path) == true)
        {
            return;
        }
        uint crc = _CRC32.GetCRC32(path);
        ResourceItem item = GetCacheResourceItem(crc, 0);
        if (item != null)
        {
            return;
        }
        Object obj = null;
#if UNITY_EDITOR 
        if (m_LoadFromAssetBundle == false)
        {
            obj = LoadAssetByEditor<Object>(path);
            if (item != null)
            {
                if (item.m_Object != null)
                {
                    obj = item.m_Object as Object;

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
                if (item.m_Object != null)
                {
                    obj = item.m_Object as Object;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<Object>(item.m_AssetName);
                }
            }
        }
        CacheResourceItem(path, ref item, crc, obj);
        // 转换场景不清空缓存
        item.m_Clear = false;
        ReleaseResouce(path, false);
    }

    // 增加res obj引用计数
    public int IncreaseResouceRef(ResourceObj resObj, int count = 1)
    {
        if (resObj != null)
        {
            return IncreaseResouceRef(resObj.m_crc, count);
        }
        return 0;
    }

    // 增加res obj引用计数
    public int IncreaseResouceRef(uint crc, int count = 1)
    {
        ResourceItem item = null;
        if (AssetDic.TryGetValue(crc, out item) == false || item == null)
        {
            return 0;
        }
        item.RefCount += count;
        item.m_LastUseTime = Time.realtimeSinceStartup;
        return item.RefCount;
    }

    // 减少res obj引用计数
    public int DecreaseResouceRef(ResourceObj resObj, int count = 1)
    {
        if (resObj != null)
        {
            return DecreaseResouceRef(resObj.m_crc, count);
        }
        return 0;
    }

    // 减少res obj引用计数
    public int DecreaseResouceRef(uint crc, int count = 1)
    {
        ResourceItem item = null;
        if (AssetDic.TryGetValue(crc, out item) == false || item == null)
        {
            return 0;
        }
        item.RefCount -= count;
        item.m_LastUseTime = Time.realtimeSinceStartup;
        return item.RefCount;
    }
}