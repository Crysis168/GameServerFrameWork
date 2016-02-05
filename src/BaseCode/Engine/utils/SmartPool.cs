using UnityEngine;
using System.Collections.Generic;
[AddComponentMenu("")]
/// <summary>
/// Class to manage a pool of prefabs
/// </summary>
/// 
public class SmartPool : MonoBehaviour {
	
	public class poolObject
	{
		public GameObject obj;
		public float fTime;
	}
	
	public static Dictionary<string, SmartPool> _Pools = new Dictionary<string, SmartPool>();
	/// <summary>
	/// Name of the Pool
	/// </summary>
	public string PoolName;
	/// <summary>
	/// The Prefab being managed by this pool
	/// </summary>
	public GameObject Prefab;
	/// <summary>
	/// Keep persistent between scene changes
	/// </summary>
	public bool DontDestroy=false;
	/// <summary>
	/// Automatically prepare when the game starts. If false, you'll need to call Prepare() by yourself
	/// </summary>
	public bool PrepareAtStart = false;
	/// <summary>
	/// Blocksize when the pool needs to grow or shrink
	/// </summary>
	public int AllocationBlockSize=1;
	/// <summary>
	/// Minimum pool size
	/// </summary>
	public int MinPoolSize=1;
	/// <summary>
	/// Maximum pool size
	/// </summary>
	public int MaxPoolSize=30;
	/// <summary>
	/// Behaviour when the maximum pool size is exceeded
	/// </summary>
	public PoolExceededMode OnMaxPoolSize = PoolExceededMode.ReUse;
	/// <summary>
	/// Automatically cull items until the pool reaches MaxPoolSize
	/// </summary>
	public bool AutoCull = false;
	/// <summary>
	/// Time in seconds between automatic culling occurs
	/// </summary>
	public float CullingSpeed = 5.0f;
	/// <summary>
	/// Whether all actions should be logged
	/// </summary>
	
	public bool DebugLog = false;

    private Transform trCache;
	public class delayDespawn
	{
		internal float _ft;
		internal GameObject _obj;
	}
	Stack<GameObject> mStock=new Stack<GameObject>();
	Stack<GameObject> mPreStock = new Stack<GameObject>();
	List<GameObject> mSpawned=new List<GameObject>();
	List<delayDespawn> mDealyDespawn = new List<delayDespawn>();
	
	float mLastCullingTime;
	
	public int InStock { get { return mStock.Count; } }
	public int Spawned { get { return mSpawned.Count; } }
	
	#region ### Unity Callbacks ###
	void Awake()
	{
		if (DontDestroy)
			DontDestroyOnLoad(gameObject);
        trCache = transform;
	}
	
	void OnEnable()
	{
		if (Prefab != null) {
			if (GetPoolByName(PoolName) == null) {
				_Pools.Add(PoolName, this);
				if (DebugLog)
					MyLogger.Log("SmartPool: Adding '" + PoolName + "' to the pool dictionary!");
			}
		}
		
	}
	
	void Start()
	{
		if (PrepareAtStart)
			Prepare();
		
		InvokeRepeating("stockUpdate",0.1f,3);
	}
	
	void LateUpdate()
	{
		if (AutoCull && Time.time - mLastCullingTime > CullingSpeed) {
			mLastCullingTime = Time.time;
			Cull(true);
		}

        delayDespawn item = null;
        for (int i = 0; i < mDealyDespawn.Count;++i )
        {
            item = mDealyDespawn[i];
            if (item._obj == null)
            {
                mDealyDespawn.Remove(item);
                return;
            }
            if (item._ft < Time.time)
            {
                DespawnItem(item._obj);
                return;
            }
        }
	}
	
	void stockUpdate()
	{
		while (mPreStock.Count > 0)
		{
			GameObject item = mPreStock.Pop();
			mStock.Push(item);                                                                                                                                      
		}
	}
	void OnDisable()
	{
		if (!DontDestroy) {
			Clear();
			if (_Pools.Remove(PoolName) && DebugLog)
				MyLogger.Log("SmartPool: Removing " + PoolName + " from the pool dictionary!");
		}
	}
	
	void Reset()
	{
		PoolName = "";
		Prefab = null;
		DontDestroy = false;
		AllocationBlockSize = 1;
		MinPoolSize = 1;
		MaxPoolSize = 40;
		OnMaxPoolSize = PoolExceededMode.Ignore;
		DebugLog = false;
		AutoCull = true;
		CullingSpeed = 1f;
		mLastCullingTime = 0;
	}
	
	#endregion
	
	#region ### Methods for the current pool ###
	
	/// <summary>
	/// Remove all instances held by this pool
	/// </summary>
	public void Clear()
	{
		if (DebugLog&&Prefab)
			MyLogger.Log("SmartPool (" + PoolName + "): Clearing all instances of " + Prefab.name);
        mSpawned.ForEach(go =>
            {
                Destroy(go);
            });
        mSpawned.Clear();

        var e = mStock.GetEnumerator();
        while(e.MoveNext())
        {
            Destroy(e.Current);
        }
		mStock.Clear();

        e = mPreStock.GetEnumerator();
        while(e.MoveNext())
        {
            Destroy(e.Current);
        }
		mPreStock.Clear();
        mDealyDespawn.ForEach(go =>
            {
                Destroy(go._obj);
            });
		mDealyDespawn.Clear();
		Prefab = null;
		Resources.UnloadUnusedAssets();
	}
	
	public void deatchAndDelayDestory(float t)
	{
		this.transform.DetachChildren();
        mSpawned.ForEach(go =>
            {
                Destroy(go, t);
            });
		mSpawned.Clear();

        var e = mStock.GetEnumerator();        
        while(e.MoveNext())
        {
            Destroy(e.Current);
        }
		mStock.Clear();

        e = mPreStock.GetEnumerator();
        while(e.MoveNext())
        {
            Destroy(e.Current);
        }
		mPreStock.Clear();

        mDealyDespawn.ForEach(go =>
            {
                Destroy(go._obj);
            });
		mDealyDespawn.Clear();

		Prefab = null;
		Resources.UnloadUnusedAssets();
		
	}
	/// <summary>
	/// Shrink the stock to match MaxPoolSize
	/// </summary>
	public void Cull() { Cull(false); }
	
	/// <summary>
	/// Shrink the stock to match MaxPoolSize
	/// </summary>
	/// <param name="smartCull">if true a maximum of AllocationBlockSize items are culled</param>
	public void Cull(bool smartCull)
	{
		int toCull=(smartCull) ? Mathf.Min(AllocationBlockSize,mStock.Count-MaxPoolSize) : mStock.Count-MaxPoolSize;
		if (DebugLog && toCull>0)
			MyLogger.Log("SmartPool (" + PoolName + "): Culling "+(toCull)+" items");
		
		while (toCull-->0) {
			GameObject item = mStock.Pop();
			Destroy(item);
		}
	}
	/// <summary>
	/// Despawn a gameobject and add it to the stock
	/// </summary>
	/// <param name="item"></param>
	public void DespawnItem(GameObject item)
	{
		if (!item)
		{
			if (DebugLog)
				MyLogger.LogWarning("SmartPool (" + PoolName + ").DespawnItem: item is null!");
			return;
		}
		if (IsSpawned(item)) {
			#if UNITY_3
			item.active = false;
			#else
			item.SetActive(false);
			#endif
			//item.name = Prefab.name + "_stock";            
			mSpawned.Remove(item);
            mPreStock.Push(item);
            mStock.Push(item);
            //从mDealyDespawn中移除
            mDealyDespawn.Find(dealyItem =>
                {
                    if (dealyItem._obj.GetInstanceID() == item.GetInstanceID())
                    {
                        mDealyDespawn.Remove(dealyItem);
                        return true;
                    }
                    return false;
                });			
			if (DebugLog)
				MyLogger.Log("SmartPool (" + PoolName + "): Despawning '" + item.name);
		}
		else 
		{
			if ( !mStock.Contains(item) && !mPreStock.Contains(item) )
			{
				GameObject.Destroy(item);
				if (DebugLog)
					MyLogger.LogWarning("SmartPool (" + PoolName + "): Cant Despawn" + item.name + "' because it's not managed by this pool! However, SmartPool destroyed it!");
			}
			else
			{
				item.SetActive(false);
			}
		}
	}
	/// <summary>
	/// Despawn all items of this pool
	/// </summary>
	public void DespawnAllItems()
	{
		while (mSpawned.Count > 0)
			DespawnItem(mSpawned[0]);
	}
	
	/// <summary>
	/// Destroys a spawned item instead of despawning it
	/// </summary>
	/// <param name="item">the spawned item to destroy</param>
	public void KillItem(GameObject item)
	{
		if (!item) {
			if (DebugLog)
				MyLogger.LogWarning("SmartPool (" + PoolName + ").KillItem: item is null!");
			return;
		}
		mSpawned.Remove(item);
		Destroy(item);
	}
	
	/// <summary>
	/// Whether a gameobject is managed by this pool
	/// </summary>
	/// <param name="item">an item</param>
	/// <returns>true if item is managed by this pool</returns>
	public bool IsManagedObject(GameObject item)
	{
		if (!item) {
			if (DebugLog)
				MyLogger.LogWarning("SmartPool (" + PoolName + ").IsManagedObject: item is null!");
			return false;
		}
		if (mSpawned.Contains(item) || mStock.Contains(item)||mPreStock.Contains(item))
			return true;
		else
			return false;
	}
	/// <summary>
	/// Whether a gameobject is spawned by this pool
	/// </summary>
	/// <param name="item">an item</param>
	/// <returns>true if the item is spawned by this pool</returns>
	public bool IsSpawned(GameObject item)
	{
		if (!item) {
			if (DebugLog)
				MyLogger.LogWarning("SmartPool (" + PoolName + ").IsSpawned: item is null!");
			return false;
		}
		return (mSpawned.Contains(item));
	}
	/// <summary>
	/// Create instances and add them to the stock
	/// </summary>
	/// <param name="no"></param>
	void Populate(int no,Vector3 position, Quaternion rotation)
	{
		while (no > 0&&Prefab) 
		{
			GameObject go = (GameObject)Instantiate(Prefab,position,rotation);
			go.SetActive(false);
			//go.name = Prefab.name + "_stock";
			mStock.Push(go);
			no--;
		}
		if (DebugLog)
			MyLogger.Log("SmartPool (" + PoolName + "): Instantiated " + mStock.Count + " instances of " + Prefab.name);
	}
	/// <summary>
	/// Clear all instances and repopulate the pool to match MinPoolSize
	/// </summary>
	public void Prepare()
	{
		Clear();
		mStock = new Stack<GameObject>(MinPoolSize);
		Populate(MinPoolSize,Vector3.zero,Quaternion.identity);
	}
	/// <summary>
	/// Spawn an instance, make it active and add it to the Spawned list
	/// </summary>
	/// <returns>the instance spawned</returns>
	public GameObject SpawnItem(Vector3 position, Quaternion rotation)
	{        
		GameObject item=null;
		// if we ran out of objects, create some
		if (InStock == 0) {
			if (Spawned < MaxPoolSize || OnMaxPoolSize==PoolExceededMode.Ignore)
				Populate(AllocationBlockSize,position,rotation);
		}
		// maybe we've got objects ready now
		if (InStock > 0) {
			item = mStock.Pop();
			if (DebugLog)
				MyLogger.Log("SmartPool (" + PoolName + "): Spawning item, taking it from the stock!");
			// or maybe we want to reuse the oldest spawned item
		}
		else if (OnMaxPoolSize == PoolExceededMode.ReUse) 
		{
			if (mSpawned.Count>0)
			{
				item = mSpawned[0];
				mSpawned.RemoveAt(0);
			}
			if (DebugLog)
				MyLogger.Log("SmartPool (" + PoolName + "): Spawning item, reusing an existing item!");
		} else if (DebugLog)
			MyLogger.Log("SmartPool (" + PoolName + "): MaxPoolSize exceeded, nothing was spawned!");
		if (item != null) {
			mSpawned.Add(item);
			
			//item.name = Prefab.name + "_clone";
			item.transform.position =position;
			item.transform.rotation = rotation;
            item.transform.SetParent(trCache);            
			item.SetActive(false);
			item.SetActive(true);
		}
		return item;
	}
	
	public void setPoolName(string name)
	{
		PoolName = name;
		SmartPool p;
		_Pools.TryGetValue(name, out p);
		if(p==null)
		{
			_Pools.Add(name, this);
		}
	}
	#endregion
	
	#region ### Methods to access pools (static) ###
	
	/// <summary>
	/// Shrink a stock to match MaxPoolSize
	/// </summary>
	/// <param name="poolName"></param>
	public static void Cull(string poolName) { Cull(poolName, false); }
	
	/// <summary>
	/// Shrink a stock to match MaxPoolSize
	/// </summary>
	/// <param name="smartCull">if true a maximum of AllocationBlockSize items are culled</param>
	public static void Cull(string poolName, bool smartCull)
	{
		SmartPool P = GetPoolByName(poolName);
		if (P)
			P.Cull();
	}
	
	/// <summary>
	/// Despawn a managed item, returning it to the pool
	/// </summary>
	/// <param name="item">an item</param>
	/// <remarks>If the object is unmanaged, it will be destroyed</remarks>
	public static void Despawn(GameObject item)
	{
		if (item) {
			SmartPool P = GetPoolByItem(item);
			if (P != null)
			{
				P.DespawnItem(item);
			}
			else
				GameObject.Destroy(item);
		}
	}
	
	public static void Despawn(GameObject item,float t)
	{
		if (item)
		{
			SmartPool P = GetPoolByItem(item);
			if (P != null)
			{
				bool bFind = false;
                P.mDealyDespawn.Find(objItem =>
                {
                    if (objItem._obj == item)
                    {
                        objItem._ft = Mathf.Min(objItem._ft, Time.time + t);
                        bFind = true;
                        return true;
                    }
                    return false;
                });
                
				if (!bFind)
				{
					delayDespawn delayItem = new delayDespawn();
					delayItem._ft = Time.time + t;
					delayItem._obj = item;
					P.mDealyDespawn.Add(delayItem);
				}
			}
			else
				GameObject.Destroy(item,t);
		}
	}
	
	/// <summary>
	/// Despawn all items of a certain pool
	/// </summary>
	/// <param name="poolName">name of the pool</param>
	public static void DespawnAllItems(string poolName)
	{
		SmartPool P = GetPoolByName(poolName);
		if (P!=null)
			P.DespawnAllItems();
	}
	
	/// <summary>
	/// Find the pool an item belongs to
	/// </summary>
	/// <param name="item">an item</param>
	/// <returns>the corresponding pool or null if the item is unmanaged</returns>
	public static SmartPool GetPoolByItem(GameObject item)
	{
        var e = _Pools.GetEnumerator();
        SmartPool P = null;
        while(e.MoveNext())
        {
            P = e.Current.Value;
            if (P.IsManagedObject(item))
                return P;
        }
		return null;
	}
	
	/// <summary>
	/// Gets a pool by it's name
	/// </summary>
	/// <param name="poolName">name of the pool</param>
	/// <returns>a pool or null</returns>
	public static SmartPool GetPoolByName(string poolName)
	{
		SmartPool P;
		_Pools.TryGetValue(poolName, out P);
		return P;
	}
	
	
	/// <summary>
	/// create a pool by it's name
	/// </summary>
	/// <param name="poolName">name of the pool</param>
	///  <param name="prefab">name of the prefab</param>
	/// <returns>a pool or null</returns>
	public static SmartPool CreatePool( string poolName,GameObject prefab = null,string strParentName= "BattleParent")
	{
		SmartPool pool;
		_Pools.TryGetValue(poolName, out pool);
		if (pool == null)
		{
            GameObject parent = GameObject.Find(strParentName);
            if (parent == null)
            {
                parent = new GameObject();
                parent.name = strParentName;
            }
            GameObject go = new GameObject();
			go.name = poolName;
            go.transform.parent = parent.transform;
			pool = go.AddComponent<SmartPool>();
			if (pool != null)
				pool.setPoolName( poolName );          
		}
		
		if (prefab)        
			SmartPool.setPrefabByPoolName(poolName,prefab);
		
		
		return pool;
	}
	
	/// <summary>
	/// Kill a spawned item instead of despawning it
	/// </summary>
	/// <param name="item">the spawned item to kill</param>
	/// <remarks>If the item is unmanaged, it will be destroyed anyway</remarks>
	public static void Kill(GameObject item)
	{
		if (item) {
			SmartPool P = GetPoolByItem(item);
			if (P != null)
				P.KillItem(item);
			else
				GameObject.Destroy(item);
		}
	}
	
	/// <summary>
	/// Clear all instances and repopulate a pool to match MinPoolSize
	/// </summary>
	public static void Prepare(string poolName)
	{
		SmartPool P = GetPoolByName(poolName);
		if (P != null)
			P.Prepare();
	}
	
	/// <summary>
	/// Spawn an item from a specific pool
	/// </summary>
	/// <param name="poolName">the pool's name</param>
	/// <returns>a gameobject or null if spawning failed</returns>
	public static GameObject Spawn(string poolName, Vector3 position, Quaternion rotation)
	{
		SmartPool P;
		if (_Pools.TryGetValue(poolName, out P))
			return P.SpawnItem(position,rotation);
		else {
			MyLogger.LogWarning("SmartPool: No pool with name '" + poolName + "' found!");
			return null;
		}
	}
	/// <summary>
	/// Reset the prefab
	/// </summary>
	/// <param name="poolName">the pool's name</param>
	/// <param name="bIngore">if the prefab alerady exist ,don`t clear</param>
	/// <returns>a gameobject or null if spawning failed</returns>
	public static void setPrefabByPoolName(string poolName, GameObject prefab)
	{
		SmartPool p = SmartPool.GetPoolByName(poolName);
		if (p)
		{
			p.Clear();
			p.Prefab = prefab;
		}
	}
	
	public void clearSpwan()
	{
		mSpawned.Clear();
	}
	#endregion
}

/// <summary>
/// Determining reaction when MaxPoolSize is exceeded
/// </summary>
[System.Serializable]
public enum PoolExceededMode:int 
{
	/// <summary>
	/// MaxPoolSize will be ignored
	/// </summary>
	Ignore = 0,
	/// <summary>
	/// Spawning will fail when MaxPoolSize is exceeded
	/// </summary>
	StopSpawning = 1,
	/// <summary>
	/// Already spawned items will be returned when MaxPoolSize is exceeded
	/// </summary>
	ReUse = 2
}
