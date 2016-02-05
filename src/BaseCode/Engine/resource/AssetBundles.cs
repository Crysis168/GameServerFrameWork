using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Engine
{
	[AddComponentMenu("")]
	//读取本地资源
	public class AssetBundles : SingletonMonoBehaviour<AssetBundles> 
	{       
		public static readonly string AssetBundlesPath = "AssetBundles/";
        public static readonly string LocalStreamPath = "file://" + Application.dataPath + "/StreamingAssets/win/";
        public static readonly string LocalCacheBundlePath = "file://" + Application.dataPath + "/StreamingAssets/win/" + AssetBundlesPath;
        public static readonly string LocalVersionPath = "file://" + Application.dataPath + "/StreamingAssets/win/version.txt";

        public delegate void HandleInitEnd();
		public event HandleInitEnd OnInitEnd;

		AssetBundleManifest mainfest;
		bool bInited = false;
	    ResourceCache resourceCache;        
        public static string LocalBundlePath
		{
			get{ return LocalStreamPath + AssetBundlesPath;}
		}

		public static string getNewestBundlePath(string strName)
        {
            return LocalBundlePath + strName;
        }

		public IEnumerator Init()
		{
			if(bInited == false)
			{
				resourceCache = new ResourceCache();
				yield return StartCoroutine ("LoadAssetBundles");
			}
		}
		
		IEnumerator LoadAssetBundles()
		{
            string mUrl = AssetBundles.getNewestBundlePath("AssetBundles");            
			WWW mwww = new WWW(mUrl);            
			yield return mwww;
			if (!string.IsNullOrEmpty(mwww.error))
			{
                MyLogger.LogError(mwww.error);                
			}
			else
			{
				AssetBundle mab = mwww.assetBundle;
				mainfest = (AssetBundleManifest)mab.LoadAsset("AssetBundleManifest");
				mab.Unload(false);
			}

			yield return StartCoroutine (DownLoadTextAssets(EngineURLConst.DATA_CONFIG));
			yield return StartCoroutine (DownLoadSprites(EngineURLConst.SKILLICON_PATH));
			yield return StartCoroutine (DownLoadSprites(EngineURLConst.HEADICON_PATH));

            yield return StartCoroutine(DownLoadSprites(EngineURLConst.HEADICON_PATH));
            yield return StartCoroutine(DownLoadBundle("UI/LoginUI", false));
            yield return StartCoroutine(DownLoadBundle("UI/CreateUI", false));
            yield return StartCoroutine(DownLoadBundle("UI/LoadingUI", false));
            yield return StartCoroutine(DownLoadBundle("BaseUI/GuideTip", false));
            yield return StartCoroutine(DownLoadBundle("UI/MainUI", false));
            yield return StartCoroutine(DownLoadBundle("UI/BackUI", false));
            yield return StartCoroutine(DownLoadBundle("UI/BackTopUI", false));

            bInited = true;
			MyLogger.Log ("LoadAssetBundles End");
			OnInitEnd ();
		}
		
		public UnityEngine.Object GetPrefab(string realName)
		{
			realName = EngineURLConst.GetResource (realName);
			UnityEngine.Object  cacheObj = resourceCache.GetPrefab (realName);
			if(cacheObj != null)
			{
				return cacheObj;
			}

            return null;
		}


		public Sprite GetSprite(string realName)
		{
			return resourceCache.GetSpriteCache(realName);
		}

		public TextAsset GetTextAsset(string realName)
		{
			return resourceCache.GetTextAsset(realName);
		}
		
        public AssetBundle GetMapAsset(string realName)
        {
            return resourceCache.GetResource(realName).assetBundle as AssetBundle;
        }
		public IEnumerator DownLoadTextAssets(string path)
		{
			string realName = EngineURLConst.CONFIG_PATH + path;
            Debug.Log(realName);
			WWW www = new WWW(AssetBundles.getNewestBundlePath(realName));
			yield return www;
			if (!string.IsNullOrEmpty(www.error))
			{
                Debug.Log(www.error);
			}
			else
			{
				AssetBundle ab = www.assetBundle;
				resourceCache.storeTextAsset(ab);
				ab.Unload(false);
			}
		}

		public IEnumerator DownLoadSprites(string path)
		{
			string realName = EngineURLConst.RESOURCES_PATH + path;
			WWW www = new WWW(AssetBundles.getNewestBundlePath(realName));
			yield return www;
			if (!string.IsNullOrEmpty(www.error))
			{
				MyLogger.Log(www.error);
			}
			else
			{
				AssetBundle ab = www.assetBundle;
				resourceCache.storeSprites(ab);
				ab.Unload(false);
			}
		}

      public  IEnumerator DownLoadBundle(string realName, bool temporary)
		{
			realName = EngineURLConst.GetResource (realName);
			UnityEngine.Object  gobj = null;
			string[] dps = mainfest.GetAllDependencies(realName);
			for (int i = 0; i < dps.Length; i++)
			{
				if(string.IsNullOrEmpty(dps[i]))
				   continue;
				string dUrl = AssetBundles.getNewestBundlePath(dps[i]);
                if (resourceCache.IsDpsLoaded(dUrl) == false)
                {
                    WWW dwww = new WWW(dUrl);
                    yield return dwww;
                    resourceCache.AddDps(dUrl,dwww.assetBundle);
                }
			}
			WWW www = new WWW(AssetBundles.getNewestBundlePath(realName));
			yield return www;
            AssetBundle ab = null;
			if (!string.IsNullOrEmpty(www.error))
			{
				MyLogger.Log(www.error);
			}
			else
			{
				ab = www.assetBundle;
				ab.LoadAllAssets();
				gobj = ab.LoadAsset(realName);
                if (temporary)
    				resourceCache.storeTemporaryCache(realName,gobj);
                else
                    resourceCache.storePermanentCache(realName, gobj);

                ab.Unload(false);
			}
		}

      public  IEnumerator DownLoadAllBundle(string realName)
        {            
            UnityEngine.Object gobj = null;
            string[] dps = mainfest.GetAllDependencies(realName);
            AssetBundle[] abs = new AssetBundle[dps.Length];
            for (int i = 0; i < dps.Length; i++)
            {
                if (string.IsNullOrEmpty(dps[i]))
                    continue;
                string dUrl = AssetBundles.getNewestBundlePath(dps[i]);
                WWW dwww = new WWW(dUrl);
                yield return dwww;
                abs[i] = dwww.assetBundle;
            }
            WWW www = new WWW(AssetBundles.getNewestBundlePath(realName));
            yield return www;
            AssetBundle ab = null;
            if (!string.IsNullOrEmpty(www.error))
            {
				MyLogger.Log(www.error);
            }
            else
            {
                Debug.Log(realName);
                ab = www.assetBundle;
                ab.LoadAllAssets();
                gobj = ab.LoadAsset(realName);
                resourceCache.storeTemporaryCache(realName, gobj);
                ab.Unload(false);
            }

            for (int i = 0; i < abs.Length; ++i)
            {
                ab = abs[i];
                if (ab != null)
                    ab.Unload(false);
            }
        }

        public string[] GetAllDependencies(string realName)
        {            
            return mainfest.GetAllDependencies(realName);
        }       
	}
}


