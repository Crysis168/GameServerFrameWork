using UnityEngine;
using UnityEngine.Internal;
namespace Engine
{
	public class Resource
	{
		string _realName;
		UnityEngine.Object  _assetBundle;
		uint   _usedTime;

		public Resource(string realName,UnityEngine.Object  assetobj=null)
		{
			_realName = realName;
			assetBundle = assetobj;
		}

		public string realName 
		{
			get{ return _realName;}
		}
		
		public UnityEngine.Object  assetBundle
		{
			get
			{
				_usedTime++;
				return _assetBundle;
			}
			set
			{
				_assetBundle = value;
				_usedTime = 0;
			}
		}

		public static Object InstantiatePrefab(string prefab, Vector3 point , Quaternion rotation)
		{                        
            Object prefabObj = null;
			if(AssetBundles.Instance != null)
				prefabObj = AssetBundles.Instance.GetPrefab(prefab);
            if(prefabObj == null)
				prefabObj = Resources.Load (prefab);
			if (prefabObj != null)
			{
				return GameObject.Instantiate (prefabObj, point, rotation);
			}
			return null;
		}
        public static Object InstantiatePrefab(string prefab, Vector3 point)
        {
            return InstantiatePrefab(prefab, point, Quaternion.identity);
        }
        public static Object InstantiatePrefab(string prefab)
        {
            return InstantiatePrefab(prefab,Vector3.zero,Quaternion.identity);
        }

        public static Object LoadResource(string resName)
		{
			Object prefabObj = null;
			if(AssetBundles.Instance != null)
				prefabObj = AssetBundles.Instance.GetPrefab(resName);
			if(prefabObj == null)
				prefabObj = Resources.Load (resName);
			return prefabObj;
		}
		
		public static Sprite GetSprite(string path,string realName)
		{
			Sprite sprite = null;
			if(AssetBundles.Instance != null)
				sprite = AssetBundles.Instance.GetSprite (realName);
            if (sprite == null)
			{
				Object[] sprites = Resources.LoadAll(path);
				for(int i=0;i<sprites.Length;i++)
				{
					sprite = sprites[i] as Sprite;
					if(sprite !=null && realName == sprite.name)
						return sprite;
				}
			}
			return null;
		}
		
		public static GameObject PoolSpawnPrefab(string prefab,Vector3 point,Quaternion rotation,int maxSize)
		{
			GameObject prefabObj = null;
			if(AssetBundles.Instance != null)
				prefabObj = AssetBundles.Instance.GetPrefab(prefab) as GameObject;
#if UNITY_EDITOR
			if(prefabObj == null)
				prefabObj = Resources.Load (prefab) as GameObject;
#endif
			if (prefabObj != null)
			{
				SmartPool pool = SmartPool.GetPoolByName(prefab+"_pool");
				if(pool == null)
				{
					pool = SmartPool.CreatePool(prefab+"_pool",prefabObj);
                    pool.MaxPoolSize = maxSize;
				}
				return pool.SpawnItem(point,rotation);
			}
			return null;
		}

        public static GameObject PoolSpawnPrefab(string prefab, Vector3 point, Quaternion rotation)
        {
            return PoolSpawnPrefab(prefab,point,rotation,30);
        }

        public static GameObject PoolSpawnPrefab(string prefab, Vector3 point)
        {
            return PoolSpawnPrefab(prefab, point, Quaternion.identity, 30);
        }
        public static GameObject PoolSpawnPrefab(string prefab)
        {
            return PoolSpawnPrefab(prefab, Vector3.zero, Quaternion.identity, 30);
        }
	}
}

