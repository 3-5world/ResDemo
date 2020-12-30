using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class BundleTools
{
    private static string rulesPathConfig = "Assets/Editor/BundleRulesConfig.asset";
    private static string assetBundlesConfigs = "Assets/Prefab/Config/Bundle/AssetBundlesConfig.asset";

    private static string bunleTargetPath = Application.dataPath + "/../AssetBundle/BundleOut";
    private static Dictionary<string, string> allBundleDir = new Dictionary<string, string>();
    private static Dictionary<string, List<string>> allBundleAssets = new Dictionary<string, List<string>>();
    private static List<string> _filterPath = new List<string>();
    public const string META_SUFFIX = ".meta";
    public const string MANIFEST_SUFFIX = ".manifest";
    public const string ASSET_BUNDLES_CONFIG = "assetbundlesconfig";


    [MenuItem("Bundle/CreateBundleRuleConfig")]
    public static void CreateBundleRuleConfig()
    {
        if (!File.Exists(rulesPathConfig))
        {
            var tmpConfig = ScriptableObject.CreateInstance<BundleRuleConfig>();
            tmpConfig.Init();
            AssetDatabase.CreateAsset(tmpConfig, rulesPathConfig);
            AssetDatabase.Refresh();
        }
    }

    [MenuItem("Bundle/PackBundle")]
    public static void Build()
    {
        string[] oldBundleNames = AssetDatabase.GetAllAssetBundleNames();
        var oldCount = oldBundleNames.Length;
        for (int i = 0; i < oldCount; ++i)
        {
            AssetDatabase.RemoveAssetBundleName(oldBundleNames[i], true);
            EditorUtility.DisplayProgressBar("clear bundle name", "name：" + oldBundleNames[i], i * 1.0f / oldCount);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        allBundleDir.Clear();
        _filterPath.Clear();
        allBundleAssets.Clear();
        var ruleConfig = AssetDatabase.LoadAssetAtPath<BundleRuleConfig>(rulesPathConfig);

        foreach (var tmpConfig in ruleConfig.ConfigList)
        {
            var type = tmpConfig.RuleType;
            if (type != RuleType.OneFileOneBundle)
            {
                var list = tmpConfig.RulePaths;
                for (int i = 0; i < list.Count; ++i)
                {
                    var path = list[i];
                    if (ContainFilterPath(path))
                        continue;
                    if (type == RuleType.FolderOneBundle)
                    {
                        _filterPath.Add(path);
                        var bundleName = GetBundleName(path);
                        if (!allBundleDir.ContainsKey(bundleName))
                        {
                            allBundleDir.Add(bundleName, list[i]);
                        }
                        else
                        {
                            Debug.LogErrorFormat("bundle {0} already exist!", bundleName);
                            return;
                        }

                    }
                    else if (type == RuleType.SubFolderOneBundle)
                    {
                        var dirs = new DirectoryInfo(path);
                        var dirsInfo = dirs.GetDirectories();
                        foreach (var folder in dirsInfo)
                        {
                            var subPath = path + "/" + folder.Name;
                            if (ContainFilterPath(subPath))
                                continue;
                            _filterPath.Add(subPath);
                            var bundleName = GetBundleName(subPath);
                            if (!allBundleDir.ContainsKey(bundleName))
                            {
                                allBundleDir.Add(bundleName, subPath);
                            }
                            else
                            {
                                Debug.LogErrorFormat("bundle {0} already exist!", bundleName);
                                return;
                            }
                        }
                    }
                }
            }
        }

        foreach (var tmpConfig in ruleConfig.ConfigList)
        {
            var type = tmpConfig.RuleType;

            if (type == RuleType.OneFileOneBundle)
            {
                string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab", tmpConfig.RulePaths.ToArray());
                for (int i = 0; i < allPrefabs.Length; ++i)
                {
                    var path = AssetDatabase.GUIDToAssetPath(allPrefabs[i]);
                    if (!ContainFilterPath(path))
                    {
                        string[] allDependAssets = AssetDatabase.GetDependencies(path);
                        List<string> filterdependAssets = new List<string>();
                        for (int j = 0; j < allDependAssets.Length; ++j)
                        {
                            var asset = allDependAssets[j];
                            if (ContainFilterPath(asset) || asset.EndsWith(".cs"))
                                continue;
                            filterdependAssets.Add(asset);
                            _filterPath.Add(asset);
                        }
                        var bundleName = GetBundleName(path);
                        allBundleAssets.Add(bundleName, filterdependAssets);
                    }
                }
            }
        }

        foreach (string name in allBundleDir.Keys)
        {
            SetBundleName(name, allBundleDir[name]);
        }

        foreach (string name in allBundleAssets.Keys)
        {
            SetBundleName(name, allBundleAssets[name]);
        }

        BuildAssetBundleName();
    }

    static string GetBundleName(string path)
    {
        var bundleName = path.Replace('/', '@');
        var index = path.LastIndexOf('.');
        if (index != -1)
            bundleName = bundleName.Substring(0, index);
        return bundleName.Substring("Assets@".Length);
    }

    static bool ContainFilterPath(string path)
    {
        for (int i = 0; i < _filterPath.Count; i++)
        {
            if (path == _filterPath[i] || (path.Contains(_filterPath[i]) && (path.Replace(_filterPath[i], "")[0] == '/')))
                return true;
        }

        return false;
    }

    static void SetBundleName(string name, List<string> paths)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            SetBundleName(name, paths[i]);
        }
    }

    static void SetBundleName(string bundleName, string path)
    {
        var importer = AssetImporter.GetAtPath(path);
        if (importer != null)
            importer.assetBundleName = bundleName;
        else
            Debug.LogErrorFormat("not exist path: {0}" + path);
    }

    static void BuildAssetBundleName()
    {
        string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
        ClearUnUseBundle(allBundles);
        BuildAssetsConfig(allBundles);
        if (!Directory.Exists(bunleTargetPath))
        {
            Directory.CreateDirectory(bunleTargetPath);
        }
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(bunleTargetPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        if (manifest == null)
        {
            Debug.LogError("AssetBundle 打包失败！");
        }
        else
        {
            Debug.Log("AssetBundle 打包完毕");
        }

        AssetDatabase.Refresh();
    }

    static void ClearUnUseBundle(string[] allBundles)
    {
        var dirInfo = new DirectoryInfo(bunleTargetPath);
        if (!dirInfo.Exists)
            return;
        var allFile = dirInfo.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < allFile.Length; ++i)
        {
            var tmpFile = allFile[i];
            var fileName = tmpFile.Name;
            if (fileName.EndsWith(MANIFEST_SUFFIX))
                continue;
            bool isExist = false;
            for (int j = allBundles.Length - 1; j >= 0; --j)
            {
                var bundName = allBundles[j];
                if (bundName == fileName)
                {
                    isExist = true;
                    break;
                }
            }

            if (isExist)
                continue;
            File.Delete(tmpFile.FullName);
            if (File.Exists(tmpFile.FullName + MANIFEST_SUFFIX))
                File.Delete(tmpFile.FullName + MANIFEST_SUFFIX);
        }
    }

    static void BuildAssetsConfig(string[] allBundles)
    {
        Dictionary<string, string> assetsDic = new Dictionary<string, string>();
        for (int i = 0; i < allBundles.Length; ++i)
        {
            var bundleName = allBundles[i];
            var allAssets = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
            for (int j = 0; j < allAssets.Length; ++j)
            {
                var asset = allAssets[j];
                if (asset.EndsWith(".cs"))
                    continue;
                if (!ContainFilterPath(asset))
                {
                    Debug.LogWarningFormat("not exist in config: {0}", asset);
                    continue;
                }
                assetsDic.Add(asset, bundleName);
            }
        }


        if (!File.Exists(assetBundlesConfigs))
        {
            var tmpConfig = ScriptableObject.CreateInstance<AssetBundlesConfig>();
            AssetDatabase.CreateAsset(tmpConfig, assetBundlesConfigs);
        }

        var config = AssetDatabase.LoadAssetAtPath<AssetBundlesConfig>(assetBundlesConfigs);
        config.AssetBundleConfigList.Clear();
        foreach (string assetPath in assetsDic.Keys)
        {
            var assetBundleInfo = new AssetBundleConfig();
            //assetBundleInfo.Path = assetPath.Substring("Assets/".Length);
            assetBundleInfo.Path = assetPath;
            assetBundleInfo.CRC = CRC32.GetCRC32(assetPath);
            assetBundleInfo.BundleName = assetsDic[assetPath];

            string[] assetDependce = AssetDatabase.GetDependencies(assetPath);
            for (int i = 0; i < assetDependce.Length; ++i)
            {
                string tmpPath = assetDependce[i];
                if (tmpPath == assetPath || tmpPath.EndsWith(".cs"))
                    continue;
                string tmpBundleName = string.Empty;
                if (assetsDic.TryGetValue(tmpPath, out tmpBundleName))
                {
                    if (tmpBundleName == assetsDic[assetPath])
                        continue;
                    if (!assetBundleInfo.DependBundles.Contains(tmpBundleName))
                        assetBundleInfo.DependBundles.Add(tmpBundleName);
                }
            }
            config.AssetBundleConfigList.Add(assetBundleInfo);
        }


        SetBundleName(ASSET_BUNDLES_CONFIG, assetBundlesConfigs);
        EditorUtility.SetDirty(config);
    }
}