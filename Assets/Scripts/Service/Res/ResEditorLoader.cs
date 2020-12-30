#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ResEditorLoader : ResBaseLoader
{
    public override LoaderType GetLoaderType()
    {
        return LoaderType.Editor;
    }

    public override T LoadAsset<T>(string path, uint crc)
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    public override void ReleaseAsset(string path, uint crc)
    {
        Resources.UnloadUnusedAssets();
    }
}
#endif