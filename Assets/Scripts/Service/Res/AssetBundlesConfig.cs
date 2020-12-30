using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AssetBundlesConfig : ScriptableObject
{
    public List<AssetBundleConfig> AssetBundleConfigList = new List<AssetBundleConfig>();
}

[System.Serializable]
public class AssetBundleConfig
{
    public string Path = string.Empty;
    public uint CRC = 0;
    public string BundleName = string.Empty;
    public List<string> DependBundles = new List<string>();
}