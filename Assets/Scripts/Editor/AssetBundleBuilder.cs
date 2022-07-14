using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;


public static class AssetBundleBuilder
{
    public static string ABPath = Application.streamingAssetsPath + "/AssetsBundles";
    public static int Index = 0;
    public class ABMarkItem
    {
        public string Path;
        public Func<string, bool> Filter;
        public string Name;
    }
    
    static Dictionary<string, AssetBundleBuild> allABBuildDict = new Dictionary<string, AssetBundleBuild>();
    /// <summary>
    /// 实现动态设置ab包名
    /// </summary>
    [MenuItem("Tools/BuildABSymbol")]
    public static void BuildABSymbol()
    {
        List<ABMarkItem> list = new List<ABMarkItem>
        {
            new ABMarkItem{Path = "Assets/Res/Framework", Filter = name => name.EndsWith(".prefab")},
            new ABMarkItem{Path = "Assets/Res/UI", Filter = name => name.EndsWith(".prefab") },
        };
        List<string> processAsset = new List<string>();
        
        foreach (var item in list)
        {
            LoopAllUnityObjectsInFolder(item.Path,obj => CollectABNameWithDependence(obj, allABBuildDict, processAsset), item.Filter);
        }
    }

    public static void CollectABNameWithDependence(Object obj, Dictionary<string, AssetBundleBuild> allABBuildDict, List<string> processAsset)
    {
        string path = AssetDatabase.GetAssetPath(obj);
        string tag = GetBundleName(path);
        InnerCollectABWithDependence(path, tag, allABBuildDict, processAsset);
    }

    public static void InnerCollectABWithDependence(string path, string tag, Dictionary<string, AssetBundleBuild> allABBuildDict, List<string> processAsset, string root = null)
    {
        string lowerPath = path.ToLower();
        if (lowerPath.EndsWith(".cs")) return;
        if (lowerPath.EndsWith(".dll")) return;
        if (lowerPath.EndsWith(".shader")) return;
        if (lowerPath.StartsWith("assets/res/ui/texture/")) return;
        if (string.IsNullOrEmpty(tag)) return;
        if (processAsset.Contains(path)) return;
        
        processAsset.Add(path);
        CollectOneABItem(path, tag, allABBuildDict);
        
        //获取依赖资产路径
        var dependencies = AssetDatabase.GetDependencies(path, false);
        foreach (var dependency in dependencies)
        {
            string lowerPa = dependency.ToLower();
            if (lowerPa.EndsWith(".cs")) continue;
            if (lowerPa.EndsWith(".dll")) continue;
            if (dependency.Equals(path)) continue;

            if (dependency == root)
            {
                EditorUtility.DisplayDialog("Error", string.Format("发现循环依赖:\n{0}\n{1}", path, root), "确定");
            }

            string bundleName = GetBundleName(dependency);
            InnerCollectABWithDependence(dependency, bundleName,allABBuildDict, processAsset);
        }
    }

    public static string GetBundleName(string path)
    {
        Index += 1;
        return Index.ToString();
    }

    /// <summary>
    /// 将一个确定的ab存入字典
    /// </summary>
    /// <param name="path"></param>
    /// <param name="tag"></param>
    /// <param name="allABBuildDict"></param>
    public static void CollectOneABItem(string path, string tag, Dictionary<string, AssetBundleBuild> allABBuildDict)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(tag)) return;
        if (allABBuildDict == null) return;
        AssetBundleBuild abBuild;
        if (!allABBuildDict.TryGetValue(tag, out abBuild))
        {
            abBuild = new AssetBundleBuild();
        }

        abBuild.assetBundleName = tag;
        if (abBuild.assetNames == null)
        {
            abBuild.assetNames = new string[] { path };
        }
        else
        {
            bool bingo = false;
            for (int i = 0; i < abBuild.assetNames.Length; i++)
            {
                if (abBuild.assetNames[i] == path)
                {
                    bingo = true;
                    break;
                }
            }
            if (!bingo)
            {
                string[] oldArray = abBuild.assetNames;
                abBuild.assetNames = new string[abBuild.assetNames.Length + 1];
                abBuild.assetNames[0] = path;
                oldArray.CopyTo(abBuild.assetNames, 1);
            }
        }

        allABBuildDict[tag] = abBuild;
    }
    
    /// <summary>
    /// 遍历文件夹内的资源
    /// </summary>
    /// <param name="relativePath"></param>
    /// <param name="processAction"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static bool LoopAllUnityObjectsInFolder(string relativePath, Action<Object> processAction, Func<string, bool> filter = null)
    {
        //路径是否合法
        if (!IsFolder(relativePath))
        {
            EditorUtility.DisplayDialog("Error", string.Format("路径{0}不合法，请选择文件夹进行操作", relativePath), "确定");
            return false;
        }
        else
        {
            if (EditorUtility.DisplayDialog("Tip", string.Format("即将对{0}进行操作", relativePath), "确定", "取消"))
            {
                var dir = relativePath.ToAbsolatePath();
                if (!Directory.Exists(dir))
                {
                    return false;
                }
                LoopAllUnityObjectsInFolderRecursive(relativePath.ToAbsolatePath(), processAction, filter);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 递归遍历文件夹内的资源
    /// </summary>
    /// <param name="path"></param>
    /// <param name="processAction"></param>
    /// <param name="filter"></param>
    public static void LoopAllUnityObjectsInFolderRecursive(string path, Action<Object> processAction, Func<string, bool> filter)
    {
        //查找子目录
        var directories = Directory.GetDirectories(path);
        foreach (var dir in directories)
        {
            LoopAllUnityObjectsInFolderRecursive(dir, processAction, filter);
        }
        //走到这里表示已经走到目录最里面，没有子目录了 就去找此目录下的所有文件
        var files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            //不满足fileter条件
            if (null != filter && !filter(file))
            {
                continue;
            }

            if (!file.EndsWith(".meta"))
            { 
                //加载资源
                var obj = AssetDatabase.LoadMainAssetAtPath(file.ToRelativePath());
                if (null != processAction)
                {
                    processAction(obj);
                }
            }
        }

    }
    /// <summary>
    /// 路径是否合法
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsFolder(string path)
    {
        var type = AssetDatabase.GetMainAssetTypeAtPath(path);
        if (type != typeof(DefaultAsset))
        {
            return false;
        }

        return Directory.Exists(ToAbsolatePath(path));
    }

    /// <summary>
    /// 相对路径转换为绝对路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string ToAbsolatePath(this string relativePath)
    {
        var assetLength = "Assets".Length;
        var absolatePath = Application.dataPath +
                           relativePath.Substring(assetLength, relativePath.Length - assetLength).Replace("\\", "/");
        return absolatePath;
    }

    public static string ToRelativePath(this string absolatePath)
    {
        var relativePath = absolatePath.Replace(Application.dataPath, "Assets").Replace("\\", "/");
        return relativePath;
    }

    /// <summary>
    /// 打AB包
    /// </summary>
    [MenuItem("Tools/CreateAssetBundles")]
    public static void BuildAssetBundle()
    {
        if (Directory.Exists(ABPath))
        {
            Directory.CreateDirectory(ABPath);
        }
        
        foreach (var dic in allABBuildDict)
        {
            Debug.Log(dic.Key);
            string[] name = dic.Value.assetNames;
            foreach (var n in name)
            {
                Debug.Log(n);
            }
        }
        
        BuildPipeline.BuildAssetBundles(ABPath, allABBuildDict.Values.ToArray(),BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }
    
}
