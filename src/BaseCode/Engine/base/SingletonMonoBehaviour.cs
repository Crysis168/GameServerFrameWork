using UnityEngine;
using System.Collections;


namespace Engine
{

/**
 * 需要绑定某个对象，并且只能存在一个实体，用Exists判断是否已经存在
 * */
public class SingletonMonoBehaviour<T> : MonoBehaviour where T :SingletonMonoBehaviour<T>
{
	private static T uniqueInstance;

	public static T Instance
	{
		get
		{
			return uniqueInstance;
		}
	}

	public static bool Exists
	{
		get;
		private set;
	}

	protected virtual void Awake()
	{
		GameObject.DontDestroyOnLoad (this);
		if(uniqueInstance == null)
		{
			uniqueInstance = (T)this;
			Exists = true;
		}
	}

	protected virtual void OnDestroy()
	{
		if(uniqueInstance == this)
		{
			Exists = false;
			uniqueInstance = null;
		}
	}

	protected S AddComponent<S>() where S : Component
	{
		S component = GetComponent<S>();
		if (component == null)
			component = gameObject.AddComponent<S> ();
		return component;
	}
}

}
