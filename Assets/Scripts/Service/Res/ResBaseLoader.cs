using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public enum LoaderType
{
    Editor,
    RunTime
}
public abstract class ResBaseLoader 
{
    public abstract LoaderType GetLoaderType();

    public virtual void Init()
    {

    }

    public abstract T LoadAsset<T>(string path, uint crc) where T : UnityEngine.Object;

    //TD:
    public virtual AsyncOperation AsyncLoadAsset<T>(string path, uint crc) where T:UnityEngine.Object
    {
        return null;
    }

    public abstract void ReleaseAsset(string path, uint crc);
    
}