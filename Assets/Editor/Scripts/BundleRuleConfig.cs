using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum RuleType
{
    OneFileOneBundle,
    FolderOneBundle,
    SubFolderOneBundle
}

public class BundleRuleConfig : ScriptableObject 
{
    [System.Serializable]
    public class Config
    {
        public RuleType RuleType;
        public List<string> RulePaths;
    }

    public List<Config> ConfigList;

    public void Init()
    {
        for (int i = 0; i <= (int)(RuleType.SubFolderOneBundle); ++i)
        {
            var config = new Config
            {
                RuleType = (RuleType)i,
                RulePaths = new List<string>()
            };
            ConfigList.Add(config);
        }
    }

    public bool VerifyConfig()
    {
        foreach (var config in ConfigList)
        {
            var paths = config.RulePaths;
            foreach (var path in paths)
            {
                if (path.IndexOf("Assets/") != 0)
                {
                    Debug.LogErrorFormat("path setting error: {0}, type: {1}", path, config.RuleType);
                    return false;
                }
            }
        }

        foreach (var config in ConfigList)
        {
            var paths = config.RulePaths;
            for (int i = 0; i < paths.Count; ++i)
            {
                var path = paths[i];
                if (IsExceedCount(path, paths, 2))
                {
                    Debug.LogErrorFormat("type:{0} path: {1} already", config.RuleType, path);
                    return false;
                }
                foreach (var tmpConfig in ConfigList)
                {
                    if (tmpConfig.RuleType == config.RuleType)
                        continue;
                    var tmpPaths = tmpConfig.RulePaths;
                    if (IsExceedCount(path, tmpPaths, 1))
                    {
                        Debug.LogErrorFormat("type:{0} path: {1} already other type {2}", config.RuleType, path, tmpConfig.RuleType);
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private bool IsExceedCount(string path, List<string> paths, int count)
    {
        var tmpCount = 0;
        for (int j = 0; j < paths.Count; ++j)
        {
            if (path == paths[j])
            {
                tmpCount++;
                if (tmpCount >= count)
                    return true;
            }
        }

        return false;
    }

    public static string GetBundleName(string assetPath, string rulePath, RuleType type)
    {
        string bundleName = string.Empty;
        if (type == RuleType.OneFileOneBundle)
        {
            var fileName = Path.GetFileNameWithoutExtension(assetPath);
            var bundleNamePart = rulePath.Replace('/', '@');
            bundleName = bundleNamePart + fileName;
        }
        else if (type == RuleType.FolderOneBundle)
        {
            bundleName = rulePath.Replace('/', '@');
        }
        else
        {
            var tmpStr = assetPath.Substring(rulePath.Length);
            var subFolder = tmpStr.Substring(0, tmpStr.IndexOf('/'));
            bundleName = rulePath.Replace('/', '@') + subFolder;
        }
        return bundleName.Substring("Assets@".Length - 1);
    }
}