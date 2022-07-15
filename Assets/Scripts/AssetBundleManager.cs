using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace AssetBundles
{
    //ab包加载
    public class AssetBundleManager
    {
        private static AssetBundleManager _instance;
        private Dictionary<string, AssetBundle> bundleDict = new Dictionary<string, AssetBundle>();
        private AssetBundleManifest manifest;
        public static AssetBundleManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AssetBundleManager();
                }

                return _instance;
            }
        }

        private AssetBundleManager()
        {
            string streamingAssetsABPath = Path.Combine(Application.streamingAssetsPath, "AssetsBundles", "AssetsBundles");
            AssetBundle assetBundle = AssetBundle.LoadFromFile(streamingAssetsABPath);
            manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        /// <summary>
        /// 异步加载ab
        /// </summary>
        public AssetBundle LoadAssetBundleAsync(string abName, Action onComplete)
        {
            AssetBundle ab;
            //先去缓存中取 缓存中没有再去加载
            if (bundleDict.TryGetValue(abName, out ab)) return ab; 
            
            //先把依赖加载完 最后在加载主bundle
            string[] dependces = manifest.GetAllDependencies(abName);
            int dependCount = dependces.Length;
            //重点是如何判断所有依赖全部加载完成
            int notHaveLoadDependCount = 0;
            bool isAllLoad = false;
            Action onLoadComplete = () =>
            {
                if (--notHaveLoadDependCount == 0 && isAllLoad)
                {
                    LoadAsset(abName, onComplete);
                }
            };
            
            if (dependCount > 0)
            {
                for (int i = 0; i < dependCount; i++)
                {
                    //缓存中没有这个依赖包 就去加载依赖包
                    isAllLoad = i == dependCount - 1;
                    if (!bundleDict.TryGetValue(dependces[i], out ab))
                    {
                        //所以已加载过的依赖都存在缓存字典中，如果缓存中没有依赖包说明依赖没有被加载完，继续加载，加载完后在加载主bundle
                        notHaveLoadDependCount++;
                        LoadAssetBundleAsync(dependces[i], onLoadComplete);
                    }
                    //最后一个依赖已加载过 表明所有依赖都加载完了
                    else if (isAllLoad && notHaveLoadDependCount == 0)
                    {
                        LoadAsset(abName, onComplete);
                    }
                }
            }
            else
            {
                LoadAsset(abName, onComplete);
            }
            return bundleDict[abName];
        }

        /// <summary>
        /// 同步加载ab
        /// </summary>
        /// <param name="abName"></param>
        /// <returns></returns>
        public AssetBundle LoadAssetBundle(string abName)
        {
            AssetBundle ab;
            //先去缓存中取 缓存中没有再去加载
            if (bundleDict.TryGetValue(abName, out ab)) return ab; 
            
            //先把依赖加载完 最后在加载主bundle
            string[] dependces = manifest.GetAllDependencies(abName);
            int dependCount = dependces.Length;
            
            for (int i = 0; i < dependCount; i++)
            {
                //缓存中没有这个依赖包 就去加载依赖包
                if (!bundleDict.TryGetValue(dependces[i], out ab))
                {
                    LoadAssetBundle(dependces[i]);
                }
            }
            
            LoadAsset(abName, () => {});
            if (bundleDict.TryGetValue(abName, out ab))
                return ab;
            return null;
        }
        
        /// <summary>
        /// 异步加载单独ab包
        /// </summary>
        /// <param name="abName"></param>
        /// <returns></returns>
        public IEnumerator LoadAssetAsync(string abName)
        {
            string abResPath = Path.Combine(Application.streamingAssetsPath, "AssetsBundles", abName);
            var request = AssetBundle.LoadFromFileAsync(abResPath);
            yield return request;
            bundleDict[abName] = request.assetBundle;
        }

        /// <summary>
        /// 同步加载单独的一个ab包
        /// </summary>
        /// <param name="abName"></param>
        public void LoadAsset(string abName, Action onComplete)
        {
            string abResPath = Path.Combine(Application.streamingAssetsPath, "AssetsBundles", abName);
            AssetBundle ab = AssetBundle.LoadFromFile(abResPath);
            bundleDict[abName] = ab;
            onComplete();
        }

        /// <summary>
        /// 同步加载ab
        /// </summary>
        public void LoadAssetBundle()
        {
        
        }


    }   
}
