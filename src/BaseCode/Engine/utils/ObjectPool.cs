namespace Engine
{
	using System;
	using System.Collections.Generic;

	public class ObjectPool
	{
		private readonly static Dictionary<Type,Queue<object>> pools = new Dictionary<Type, Queue<object>>();

		public static Queue<object> GetPool(Type type)
		{
			if(pools.ContainsKey(type))
			{
				return pools[type];
			}
			pools [type] = new Queue<object> ();
			return pools [type];
		}

		public static T GetObject<T>(params object[] list)
		{
			var pool = GetPool(typeof(T));
			if(pool.Count > 0)
			{
				return (T)pool.Dequeue ();
			}
			else
			{
				return (T)System.Activator.CreateInstance(typeof(T),list);
			}
		}

		public static void DisposeObject(object obj)
		{
			if(obj != null)
			{
				var pool = GetPool(obj.GetType());
				pool.Enqueue(obj);
			}
		}

		public static void ClearObjects<T>()
		{
			var type = typeof(T);
			if(pools.ContainsKey(type))
			{
				pools.Remove(type);
			}
		}

		public static int GetObjectCount<T>()
		{
			var pool = GetPool (typeof(T));
			return pool.Count;
		}
	}
}

