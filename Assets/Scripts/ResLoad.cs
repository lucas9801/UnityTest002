using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using AssetBundles;
// using CommonEditorTools;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace DefaultNamespace
{
    public class ResLoad
    {
        public static UnityEngine.Object LoadRes(string path, System.Type type = null)
        {
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
        }
    }
}