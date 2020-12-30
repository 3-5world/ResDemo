using UnityEngine;

public class ResRuntimeLoader : ResBaseLoader
{




    public override LoaderType GetLoaderType()
    {
        return LoaderType.RunTime;
    }

    public override void Init()
    {
        AssetBundleSvc.Ins.InitSvc();
    }

    public override T LoadAsset<T>(string path, uint crc)
    {
        var bundle = AssetBundleSvc.Ins.LoadAssetBundle(crc);
        return bundle.LoadAsset<T>(path);
    }

    public override AsyncOperation AsyncLoadAsset<T>(string path, uint crc)
    {
        var bundle = AssetBundleSvc.Ins.LoadAssetBundle(crc);
        return bundle.LoadAssetAsync<T>(path);
    }

    public override void ReleaseAsset(string path, uint crc)
    {
        AssetBundleSvc.Ins.ReleaseAssetBundle(crc);
    }
}