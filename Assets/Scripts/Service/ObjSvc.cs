using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class ResObj
{
    public long Guid = 0;
    public uint CRC;
    public bool IsSceneRoot;
    public bool IsSceneClear; // 场景跳转是否清理
    public GameObject CloneObj = null;
    public int CloneInsId = 0;
    public bool IsCachePool = false;

    public Action<string, Object, object> FinishAction = null;
    public object param = null;


    public void Reset()
    {
        Guid = 0;
        CRC = 0;
        IsSceneRoot = false;
        IsSceneClear = true;
        CloneObj = null;
        CloneInsId = 0;
        FinishAction = null;
        param = null;
    }
}


public class ObjSvc : MonoSingleton<ObjSvc> 
{
    Dictionary<int, ResObj> resObjDic = new Dictionary<int, ResObj>();
    Dictionary<uint, List<ResObj>> objCachePoolDic = new Dictionary<uint, List<ResObj>>();

    Dictionary<long, ResObj> createingResObjs = new Dictionary<long, ResObj>();

    ClassObjectPool<ResObj> resObjPool = null;

    long guid = 0;
    Transform recyleRoot;
    Transform sceneRoot;

    public void Init(Transform recyleRoot, Transform sceneRoot)
    {
        this.recyleRoot = recyleRoot;
        this.sceneRoot = sceneRoot;
        resObjPool = new ClassObjectPool<ResObj>(Constants.MaxResObj);
    }

    public void ClearCache()
    {
        foreach(uint key in objCachePoolDic.Keys)
        {
            var list = objCachePoolDic[key];
            for (int i = list.Count - 1; i>= 0; --i)
            {
                var resObj = list[i];
                if (!System.Object.ReferenceEquals(resObj.CloneObj, null) && resObj.IsSceneClear)
                {
                    GameObject.Destroy(resObj.CloneObj);
                    resObjDic.Remove(resObj.CloneInsId);
                    resObj.Reset();
                    resObjPool.Recycle(resObj);
                    list.Remove(resObj);
                    ResSvc.Ins.ReleaseRes(resObj.CRC, false);
                }
            }
        }
    }

    public void PreloadGameObj(string path, int count = 1, bool isSceneRoot = false, bool isSceneClear = false)
    {
        var tmpObjs = new List<GameObject>();
        for (int i = 0; i < count; ++i)
        {
            var obj = InsObj(path, isSceneRoot, isSceneClear);
            tmpObjs.Add(obj);
        }
        for (int i = 0; i < count; ++i)
        {
            var obj = tmpObjs[i];
            ReleaseObj(tmpObjs[i]);
            obj = null;
        }
        tmpObjs.Clear();
    }

    public GameObject InsObj(string path, bool isSceneRoot = false, bool isSceneClear = true)
    {
        var crc = CRC32.GetCRC32(path);
        var resObj = GetCacheResObj(crc);

        if (resObj == null)
        {
            resObj = resObjPool.Spawn(true);
            resObj.CRC = crc;
            resObj.IsSceneClear = isSceneClear;
            var gameObj = ResSvc.Ins.LoadAsset<Object>(path);
            if (gameObj != null)
                resObj.CloneObj = GameObject.Instantiate(gameObj) as GameObject;
        }

        if (isSceneRoot)
            resObj.CloneObj.transform.SetParent(sceneRoot, false);

        var cloneInsID = resObj.CloneObj.GetInstanceID();
        resObj.CloneInsId = cloneInsID;
        if (!resObjDic.ContainsKey(cloneInsID))
        {
            resObjDic.Add(cloneInsID, resObj);
        }

        return resObj.CloneObj;
    }


    public long AsyncInsObj(string path, Action<string, Object, object> finishAction, ResPriority priority, bool isSceneRoot, bool isSceneClear = true, object param = null)
    {
        var crc = CRC32.GetCRC32(path);
        var resObj = GetCacheResObj(crc);
        if (resObj != null)
        {
            if (isSceneRoot)
                resObj.CloneObj.transform.SetParent(sceneRoot, false);
            if (finishAction != null)
                finishAction(path, resObj.CloneObj, param);
            return resObj.Guid;
        }
        var guid = CreateGuid();
        resObj = resObjPool.Spawn(true);
        resObj.Guid = guid;
        resObj.CRC = crc;
        resObj.IsSceneRoot = isSceneRoot;
        resObj.IsSceneClear = isSceneClear;
        resObj.FinishAction = finishAction;
        resObj.param = param;
        createingResObjs.Add(guid, resObj);
        ResSvc.Ins.AsyncLoadAsset(path, OnLoadAssetFinish, priority, false, resObj);
        return guid;
    }

    public bool IsAsynCreatingIns(long guid)
    {
        return createingResObjs.ContainsKey(guid);
    }

    public void CancelCreatIns(long guid)
    {
        ResObj resObj = null;
        if (createingResObjs.TryGetValue(guid, out resObj) && ResSvc.Ins.CancelLoadAsset(resObj))
        {
            createingResObjs.Remove(guid);
            resObj.Reset();
            resObjPool.Recycle(resObj);
        }
    }

    public void ReleaseObj(GameObject obj, int maxCacheCount = -1, bool isDestroyCache = false, bool isRecyleParent = false)
    {
        var insID = obj.GetInstanceID();
        ResObj resObj = null;
        if (resObjDic.TryGetValue(insID, out resObj))
        {
            Debug.LogWarningFormat("{0} obj is not created by ObjMgr", obj.name);
            return;
        }
        if (resObj.IsCachePool)
        {
            Debug.LogWarningFormat("{0} obj recyled to cache pool", obj.name);
            return;
        }

#if UNITY_EDITOR
        obj.name += "(Recycle)";
#endif

        if (maxCacheCount == 0)
        {
            resObjDic.Remove(insID);
            ResSvc.Ins.ReleaseRes(resObj.CRC, false);
            resObj.Reset();
            resObjPool.Recycle(resObj);
        }
        else
        {
            List<ResObj> list = null;
            if (!objCachePoolDic.TryGetValue(resObj.CRC, out list))
            {
                list = new List<ResObj>();
                objCachePoolDic.Add(resObj.CRC, list);
            }
            if (recyleRoot)
                resObj.CloneObj.transform.SetParent(recyleRoot);
            else
                resObj.CloneObj.SetActive(false);

            if (maxCacheCount < 0 || list.Count < maxCacheCount)
            {
                list.Add(resObj);
                resObj.IsCachePool = true;
                //ResSvc.Ins.DecreaseResRef(resObj.CRC);
            }
            else
            {
                resObjDic.Remove(insID);
                ResSvc.Ins.ReleaseRes(resObj.CRC, false);
                resObj.Reset();
                resObjPool.Recycle(resObj);
            }

        }

    }

    void OnLoadAssetFinish(string path, Object obj, object param)
    {
        var resObj = (ResObj)param;

        if (!createingResObjs.TryGetValue(resObj.Guid, out resObj))
        {
            // TD: 可能已经取消了，应该减少内部引用计数
            if (obj == null)
            {
                ResSvc.Ins.ReleaseRes(obj, true);
                return;
            }
        }

        createingResObjs.Remove(guid);
        resObj.CloneObj = GameObject.Instantiate(obj) as GameObject;
        if (resObj.IsSceneRoot)
            resObj.CloneObj.transform.SetParent(sceneRoot, false);

        var cloneInsID = resObj.CloneObj.GetInstanceID();
        resObj.CloneInsId = cloneInsID;

        if (!resObjDic.ContainsKey(cloneInsID))
            resObjDic.Add(cloneInsID, resObj);

        if (resObj.FinishAction != null)
            resObj.FinishAction(path, resObj.CloneObj, resObj.param);
    }

    ResObj GetCacheResObj(uint crc)
    {
        List<ResObj> list = null;
        if (objCachePoolDic.TryGetValue(crc, out list) && list.Count > 0)
        {
            var tmpIndex = list.Count - 1;
            var resObj = list[tmpIndex];
            list.RemoveAt(tmpIndex);
            var obj = resObj.CloneObj;
            if (!System.Object.ReferenceEquals(obj, null))
            {
                resObj.IsCachePool = false;
#if UNITY_EDITOR
                if (obj.name.EndsWith("(Recycle)"))
                {
                    obj.name = obj.name.Replace("(Recycle)", "");
                }
#endif
            }

            return resObj;
        }

        return null;
    }

    long CreateGuid()
    {
        return ++guid;
    }
}