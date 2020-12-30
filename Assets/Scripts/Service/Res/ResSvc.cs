using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public enum ResPriority
{
    High = 0,
    Middle,
    Low,
    MaxPriority,
}

public class ResItemInfo
{
    public uint CRC = 0;
    public int InsID = 0;
    public string AssetPath = string.Empty;

    protected int refCount = 0;

    public Object obj = null;


    public int RefCount
    {
        get { return refCount; }
        set
        {
            refCount = value;
            if (refCount < 0)
            {
                Debug.LogErrorFormat("refcount < 0 {0}, {1}", refCount, obj != null ? obj.name : "name is null");
            }
        }
    }
}

public class AsyncLoadResTask
{
    public List<AsyncFinshAction> FinishActionList = new List<AsyncFinshAction>();
    public uint CRC;
    public string Path;
    public bool IsSprite = false;
    public ResPriority Priority = ResPriority.Low;
    public object Param;

    public void Reset()
    {
        FinishActionList.Clear();
        CRC = 0;
        Path = string.Empty;
        IsSprite = false;
        Priority = ResPriority.Low;
    }
}

public class AsyncFinshAction
{
    public Action<string, Object, object> FinishAction = null;
    public object Param;

    public void Reset()
    {
        FinishAction = null;
        Param = null;
    }
}


public class ResSvc : MonoSingleton<ResSvc>
{
    private ResBaseLoader loader;

    protected MonoBehaviour mono;

    private Dictionary<uint, ResItemInfo> resCacheDic = new Dictionary<uint, ResItemInfo>();
    private Dictionary<int, ResItemInfo> objCacheDic = new Dictionary<int, ResItemInfo>();

    private ClassObjectPool<AsyncLoadResTask> asyncLoadTaskPool = new ClassObjectPool<AsyncLoadResTask>(Constants.LoadTaskPool);
    private ClassObjectPool<AsyncFinshAction> asyncFinishActionPool = new ClassObjectPool<AsyncFinshAction>(Constants.FinishActionPool);

    private Dictionary<uint, AsyncLoadResTask> loadingAssetTaskDic = new Dictionary<uint, AsyncLoadResTask>();
    private List<AsyncLoadResTask>[] loadingAssetTaskList = new List<AsyncLoadResTask>[(int)ResPriority.MaxPriority];

    private LRUCache<ResItemInfo> noRefResCache = null;

    public void InitSvc(MonoBehaviour mono, bool isRunTime)
    {
        this.mono = mono;
#if UNITY_EDITOR
        if (isRunTime)
            loader = new ResRuntimeLoader();
        else
            loader = new ResEditorLoader();
#else
        loader = new ResRuntimeLoader();
#endif
        noRefResCache = new LRUCache<ResItemInfo>(Constants.LRUCache);
        loader.Init();

        for (int i = 0; i < (int)ResPriority.MaxPriority; ++i)
        {
            loadingAssetTaskList[i] = new List<AsyncLoadResTask>();
        }
        this.mono.StartCoroutine(AsyncLoader());
    }

    public void PreloadAsset<T>(string path) where T : Object
    {
        var crc = CRC32.GetCRC32(path);
        if (IsExistCacheRes(crc))
            return;

        var asset = loader.LoadAsset<T>(path, crc);
        if (asset != null)
            CacheNoRefRes(path, crc, asset);
    }

    public T LoadAsset<T>(string path) where T: Object
    {
        if (string.IsNullOrEmpty(path))
            return null;

        var crc = CRC32.GetCRC32(path);
        var cacheItem = GetCacheResItem(crc);
        if (cacheItem != null)
            return cacheItem.obj as T;
        var asset = loader.LoadAsset<T>(path, crc);
        if (asset != null)
        {
            CacheRes(path, crc, asset, 1);
            return asset;
        }
        return null;
    }

    public void AsyncLoadAsset(string path, Action<string, Object, object> finishAction, ResPriority priority, bool isSprite = false, object param = null)
    {
        if (string.IsNullOrEmpty(path))
            return;

        if (finishAction == null)
        {
            Debug.LogWarningFormat("load {0} finishAction is null", path);
            return;
        }
        var crc = CRC32.GetCRC32(path);
        var cacheItem = GetCacheResItem(crc);
        if (cacheItem != null)
        {
            finishAction(path, cacheItem.obj, param);
            return;
        }

        AsyncLoadResTask task = null;
        if (!loadingAssetTaskDic.TryGetValue(crc, out task))
        {
            task = asyncLoadTaskPool.Spawn(true);
            task.CRC = crc;
            task.Path = path;
            task.IsSprite = isSprite;
            task.Priority = priority;
            loadingAssetTaskDic.Add(crc, task);
            loadingAssetTaskList[(int)priority].Add(task);
        }
        var aFinishAction = asyncFinishActionPool.Spawn(true);
        aFinishAction.FinishAction = finishAction;
        aFinishAction.Param = param;
        task.FinishActionList.Add(aFinishAction);
    }

    public bool CancelLoadAsset(ResObj resObj)
    {
        AsyncLoadResTask task = null;
        if (loadingAssetTaskDic.TryGetValue(resObj.CRC, out task))
        {
            for (int i = task.FinishActionList.Count; i >= 0; --i)
            {
                var action = task.FinishActionList[i];
                if (action != null && task.Param is ResObj && task.Param == resObj)
                {
                    action.Reset();
                    asyncFinishActionPool.Recycle(action);
                    task.FinishActionList.Remove(action);
                }
            }

            if (task.FinishActionList.Count == 0)
            {
                loadingAssetTaskDic.Remove(resObj.CRC);
                loadingAssetTaskList[(int)task.Priority].Remove(task);
                task.Reset();
                asyncLoadTaskPool.Recycle(task);
            }

            return true;
        }

        return false;
    }

    ResItemInfo GetCacheResItem(uint crc, int addRefCount = 1)
    {
        ResItemInfo item = null;
        if (resCacheDic.TryGetValue(crc, out item))
        {
            item.RefCount += addRefCount;
        }
        else
        {
            //LRU缓存中查找
            item = noRefResCache.Get(crc);
            if (item != null)
            {
                item.RefCount += addRefCount;
                resCacheDic.Add(crc, item);
                objCacheDic.Add(item.InsID, item);
            }
        }
        return item;
    }

    bool IsExistCacheRes(uint crc)
    {
        if (resCacheDic.ContainsKey(crc) || noRefResCache.IsExist(crc))
            return true;
        return false;
    }

    void CacheRes(string path, uint crc, Object obj, int addRefCount)
    {
        WashOut();
        var item = new ResItemInfo();
        item.AssetPath = path;
        item.CRC = crc;
        item.obj = obj;
        item.RefCount = addRefCount;
        item.InsID = obj.GetInstanceID();
        resCacheDic.Add(crc, item);
        objCacheDic.Add(item.InsID, item);
    }

    void CacheNoRefRes(string path, uint crc, Object obj)
    {
        var item = new ResItemInfo();
        item.AssetPath = path;
        item.CRC = crc;
        item.obj = obj;
        item.RefCount = 0;
        item.InsID = obj.GetInstanceID();
        noRefResCache.Cache(item.CRC, item);
    }

    void WashOut()
    {
        if (noRefResCache.IsFull())
        {
            var size = noRefResCache.Size;
            for (int i = 0; i < size / 2; ++i)
            {
                var item = noRefResCache.Back();
                noRefResCache.Remove(item.CRC);
                item.obj = null;
                loader.ReleaseAsset(item.AssetPath, item.CRC);
            }
        }
    }

    IEnumerator AsyncLoader()
    {
        while (true)
        {
            bool hadYield = false;
            long lastYield = System.DateTime.Now.Ticks;
            for (int i = 0; i < (int) ResPriority.MaxPriority; ++i)
            {
                if (loadingAssetTaskList[(int)ResPriority.High].Count > 0)
                    i = (int)ResPriority.High;
                else if (loadingAssetTaskList[(int)ResPriority.Middle].Count > 0)
                    i = (int)ResPriority.Middle;

                var list = loadingAssetTaskList[i];
                if (list.Count == 0)
                    continue;
                var loadingTask = list[0];
                list.RemoveAt(0);
                Object obj = null;

                var finishActionList = loadingTask.FinishActionList;
                AsyncOperation request = null;
  

                // TODO，
                if(loader.GetLoaderType() == LoaderType.Editor)
                {
                    if (loadingTask.IsSprite)
                        obj = loader.LoadAsset<Sprite>(loadingTask.Path, loadingTask.CRC);
                    else
                        obj = loader.LoadAsset<Object>(loadingTask.Path, loadingTask.CRC);

                    // 模拟：
                    yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    if (loadingTask.IsSprite)
                        request = loader.AsyncLoadAsset<Sprite>(loadingTask.Path, loadingTask.CRC);
                    else
                        request = loader.AsyncLoadAsset<Object>(loadingTask.Path, loadingTask.CRC);

                    yield return request;
                    while (!request.isDone)
                    {
                        yield return request;
                    }
                    var rRequest = request as AssetBundleRequest;
                    obj = rRequest.asset;
                }

                lastYield = System.DateTime.Now.Ticks;
                if (obj != null)
                    CacheRes(loadingTask.Path, loadingTask.CRC, obj, 1);
                for (int j = 0; j < finishActionList.Count; ++j)
                {
                    var actionInfo = finishActionList[j];
                    actionInfo.FinishAction(loadingTask.Path, obj, actionInfo.Param);
                    actionInfo.Reset();
                    asyncFinishActionPool.Recycle(actionInfo);
                }
                obj = null;
                finishActionList.Clear();
                loadingAssetTaskDic.Remove(loadingTask.CRC);
                loadingTask.Reset();
                asyncLoadTaskPool.Recycle(loadingTask);

                if (System.DateTime.Now.Ticks - lastYield > Constants.MaxLoadResTime)
                {
                    yield return null;
                    lastYield = System.DateTime.Now.Ticks;
                    hadYield = true;
                }
            }

            if (!hadYield || System.DateTime.Now.Ticks - lastYield > Constants.MaxLoadResTime)
            {
                lastYield = System.DateTime.Now.Ticks;
                yield return null;
            }
        }
    }


    public bool ReleaseRes(string path, bool completeClear = false)
    {
        var crc = CRC32.GetCRC32(path);
        return ReleaseRes(crc, completeClear);
    }

    public bool ReleaseRes(uint crc, bool completeClear)
    {
        ResItemInfo item = null;
        if (!resCacheDic.TryGetValue(crc, out item))
        {
            Debug.LogWarningFormat("released not exist res {0}", crc.ToString());
            return false;
        }

        item.RefCount--;
        DestroyResItem(item, completeClear);
        return true;
    }

    public bool ReleaseRes(Object obj, bool completeClear)
    {
        var insID = obj.GetInstanceID();
        ResItemInfo item = null;
        if (!objCacheDic.TryGetValue(insID, out item))
        {
            Debug.LogWarningFormat("released not exist res {0}", obj.name);
            return false;
        }

        item.RefCount--;
        DestroyResItem(item, completeClear);
        return true;
    }

    

    void DestroyResItem(ResItemInfo item, bool completeClear = false)
    {
        if (item.RefCount > 0)
            return;
        if (!resCacheDic.Remove(item.CRC))
            return;
        if (!objCacheDic.Remove(item.InsID))
            return;

        if (!completeClear)
        {
            noRefResCache.Cache(item.CRC, item);
            return;
        }
        noRefResCache.Remove(item.CRC);
        item.obj = null;
        loader.ReleaseAsset(item.AssetPath, item.CRC);
#if UNITY_EDITOR
        Resources.UnloadUnusedAssets();
#endif
    }
}