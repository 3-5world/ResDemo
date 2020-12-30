using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AssetBundleItem
{
    public AssetBundle Bundle = null;
    public int RefCount = 0;

    public void Reset()
    {
        Bundle = null;
        RefCount = 0;
    }
}

public class AssetBundleSvc : Singleton<AssetBundleSvc>
{
    Dictionary<uint, AssetBundleConfig> assetBundleConfigDic = new Dictionary<uint, AssetBundleConfig>();

    Dictionary<uint, AssetBundleItem> cacheAssetBundleItemDic = new Dictionary<uint, AssetBundleItem>();
    ClassObjectPool<AssetBundleItem> assetBundleItemPool = new ClassObjectPool<AssetBundleItem>(500);

    public void InitSvc()
    {
        LoadAssetBundlesConfig();
    }

    void LoadAssetBundlesConfig()
    {
        var configPath = PathDefine.ABLoadPath + Constants.ABConfigFile;
        var configBundle = AssetBundle.LoadFromFile(configPath);
        var config = configBundle.LoadAsset<AssetBundlesConfig>(Constants.ABConfig);
        if (config == null)
        {
            Debug.LogError("AssetBundleConfigs is null");
            return;
        }

        var list = config.AssetBundleConfigList;
        for (var i = 0; i < list.Count; ++i)
        {
            var assetConfig = list[i];
            if (assetBundleConfigDic.ContainsKey(assetConfig.CRC))
            {
                Debug.LogErrorFormat("exist crc asset {0} path {1}", assetConfig.CRC, assetConfig.Path);
                return;
            }
            assetBundleConfigDic.Add(assetConfig.CRC, assetConfig);
        }
    }

    public AssetBundle LoadAssetBundle(uint crc)
    {
        AssetBundleConfig config = null;
        if (!assetBundleConfigDic.TryGetValue(crc, out config))
        {
            return null;
        }
        var bundleCrc = CRC32.GetCRC32(config.BundleName);
        var item = GetCacheBundle(bundleCrc);
        if (item != null)
            return item.Bundle;

        var bundle = LoadAssetBundle(config.BundleName);
        if (config.DependBundles != null && config.DependBundles.Count > 0)
        {
            for (int i = 0; i < config.DependBundles.Count; ++i)
            {
                LoadAssetBundle(config.DependBundles[i]);
            }
        }

        return bundle;
    }

    public void ReleaseAssetBundle(uint crc)
    {
        AssetBundleConfig config = null;
        if (!assetBundleConfigDic.TryGetValue(crc, out config))
            return;
        if (config.DependBundles != null && config.DependBundles.Count > 0)
        {
            for (int i = 0; i < config.DependBundles.Count; ++i)
            {
                UnloadAssetBundle(config.DependBundles[i]);
            }
        }

        UnloadAssetBundle(config.BundleName);
    }

    AssetBundleItem GetCacheBundle(uint bundleCrc)
    {
        AssetBundleItem item = null;
        if (cacheAssetBundleItemDic.TryGetValue(bundleCrc, out item))
        {
            item.RefCount += 1;
            return item;
        }

        return null;
    }

    AssetBundle LoadAssetBundle(string bundleName)
    {
        var bundleCrc = CRC32.GetCRC32(bundleName);
        AssetBundleItem item = null;
        if (cacheAssetBundleItemDic.TryGetValue(bundleCrc, out item))
        {
            item.RefCount += 1;
            return item.Bundle;
        }

        AssetBundle assetBundle = null;
        var fullPath = PathDefine.ABLoadPath + bundleName;
        if (File.Exists(fullPath))
            assetBundle = AssetBundle.LoadFromFile(fullPath);
        if (assetBundle == null)
        {
            Debug.LogErrorFormat("load bundle error {0}", bundleName);
            return null;
        }

        item = assetBundleItemPool.Spawn(true);
        item.Bundle = assetBundle;
        item.RefCount++;
        cacheAssetBundleItemDic.Add(bundleCrc, item);
        return assetBundle;
    }

    void UnloadAssetBundle(string bundleName)
    {
        var bundleCrc = CRC32.GetCRC32(bundleName);
        AssetBundleItem item = null;
        if (cacheAssetBundleItemDic.TryGetValue(bundleCrc, out item))
        {
            item.RefCount--;
            if (item.RefCount <= 0)
            {
                item.Bundle.Unload(true);
                item.Reset();
                assetBundleItemPool.Recycle(item);
                cacheAssetBundleItemDic.Remove(bundleCrc);
            }
        }
    }
}