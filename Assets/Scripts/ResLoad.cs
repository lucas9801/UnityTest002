using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetBundles;
// using CommonEditorTools;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
public class ResLoad
{
    public static UnityEngine.Object LoadRes(string path, System.Type type = null)
    {
        //测试ab
        AssetBundle ab = AssetBundleManager.Instance.LoadAssetBundle("2");
        UnityEngine.Object obj = ab.LoadAsset("TestPanel");
        return obj;
#if !UNITY_EDITOR
        if (type == null)
        {
            type = typeof(object);
        }
        UnityEngine.Object objRet = AssetDatabase.LoadAssetAtPath(path, type);
        if (null == objRet)
        {
            Debug.LogError(string.Format("加载失败：{0}", path));
        }
        return objRet;
#else
        return null;
#endif

    }
}
