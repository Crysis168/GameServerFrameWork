using System.Collections.Generic;
using UnityEngine;

namespace Engine
{
	public class ResourceCache : Singleton<ResourceCache>
	{
		private Dictionary<string,Resource> permanentCache;
		private Dictionary<string,Resource> temporaryCache;
		private Dictionary<string,Sprite>   uiSprites;
		private Dictionary<string,TextAsset> configDatas;

        private Dictionary<string, AssetBundle> loadedDps;
		public ResourceCache()
		{
			permanentCache = new Dictionary<string,Resource>();
			temporaryCache = new Dictionary<string,Resource>();
			uiSprites = new Dictionary<string,Sprite>();
			configDatas = new Dictionary<string,TextAsset>();
            loadedDps = new Dictionary<string, AssetBundle>();
		}

		public Sprite GetSpriteCache(string realName)
		{
			if (uiSprites.ContainsKey(realName))
			{
				return uiSprites[realName];
			}
			return null;
		}

		public TextAsset GetTextAsset(string realName)
		{
			if (configDatas.ContainsKey(realName))
			{
				return configDatas[realName];
			}
			return null;
		}

		public void storeTextAsset(AssetBundle  assetBundle)
		{
			TextAsset[] testAssets = assetBundle.LoadAllAssets<TextAsset>();
			TextAsset testAsset = null;
			int i = 0;
			for(i=0;i<testAssets.Length;i++)
			{
				testAsset = testAssets[i];
				if (configDatas.ContainsKey(testAsset.name) == false)
				{
					configDatas.Add (testAsset.name, testAsset);
				}
			}
		}

		public void storeSprites(AssetBundle  assetBundle)
		{
			Sprite[] sprites = assetBundle.LoadAllAssets<Sprite>();
			Sprite sprite = null;
			int i = 0;
			for(i=0;i<sprites.Length;i++)
			{
				sprite = sprites[i];
				if (uiSprites.ContainsKey(sprite.name) == false)
				{
					uiSprites.Add (sprite.name, sprite);
				}
			}
		}
		
		public Resource GetResource(string realName)
		{
			if (permanentCache.ContainsKey(realName))
			{
				return permanentCache[realName];
			}
			if (temporaryCache.ContainsKey(realName))
			{
				return temporaryCache[realName];
			}
			return null;
		}

		public UnityEngine.Object  GetPrefab(string realName)
		{
			Resource resource = GetResource (realName);
			if (resource!=null)
			{
				return resource.assetBundle;
			}
			return null;
		}
		
		public bool storeTemporaryCache(string realName, UnityEngine.Object  assetBundle)
		{
			removeCache (realName);
			Resource resource = new Resource (realName, assetBundle);
			temporaryCache.Add (realName, resource);
			return true;
		}
		
		public bool storePermanentCache(string realName, UnityEngine.Object  assetBundle)
		{
			removeCache(realName);
			Resource resource = new Resource (realName, assetBundle);
			permanentCache.Add(realName,resource);
			return true;
		}
		
		public void removeCache(string realName)
		{
			if (temporaryCache.ContainsKey(realName))
			{
				temporaryCache.Remove(realName);
			}
			if (permanentCache.ContainsKey(realName))
			{
				permanentCache.Remove(realName);
			}
		}
		
		public void clearTemporaryCache()
		{
            foreach (Resource ab in temporaryCache.Values)
            {
                GameObject.Destroy(ab.assetBundle);
            }
			temporaryCache.Clear ();
		}

        public bool IsDpsLoaded(string dps)
        {
            if (loadedDps.ContainsKey(dps))
                return true;
            return false;
        }

        public void AddDps(string dps, AssetBundle ab)
        {
            if (loadedDps.ContainsKey(dps) == false)
            {
                loadedDps.Add(dps, ab);
            }
        }

        public void UnloadDps()
        {
            foreach (AssetBundle ab in loadedDps.Values)
            {
                if (ab != null)
                    ab.Unload(true);
            }
            loadedDps.Clear();
        }
	}
}

