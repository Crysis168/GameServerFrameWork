namespace Engine
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class TimerItem
	{
		public Action mIntervalHandler = null;
		float _eTime;
		float _dTime;
		float _escapeTime;
		bool  _bOver;
		//-----------------------------
		public TimerItem(float eTime)
		{
			_dTime = _eTime = eTime;
			_bOver = false;
		}

		public float eTime
		{
			get{ return _eTime;}
		}

		public float escapeTime
		{
			get{ return _escapeTime;}
		}

		public bool over
		{
			get{ return _bOver;}
			set{ _bOver = value;}
		}

		public int Id
		{
			get
			{ 				
				if(mIntervalHandler!=null)
					return mIntervalHandler.GetHashCode();
				return 0;
			}
		}

		public void UpdateTime(float dt)
		{
			_dTime -= dt;
			_escapeTime += dt;
			if(_dTime <= 0f)
			{
				_dTime = _eTime;
				if(mIntervalHandler!=null)
					mIntervalHandler();
			}
		}
	}
	[AddComponentMenu("")]
	public class TimerManager : SingletonMonoBehaviour<TimerManager> 
	{
		private Dictionary<int,TimerItem> timersDic = new Dictionary<int,TimerItem> ();

		public TimerItem Add(float eTime,Action handle)
		{
			if(timersDic.ContainsKey(handle.GetHashCode()) == false)
			{
				TimerItem timerItem = new TimerItem(eTime);
				timersDic.Add (handle.GetHashCode(), timerItem);
				timerItem.mIntervalHandler = handle;
				return timerItem;
			}
			else
				return timersDic[handle.GetHashCode()];
		}

		public void Remove(Action handle)
		{
			int timerId = handle.GetHashCode ();
			if(timersDic.ContainsKey(timerId))
			{
				TimerItem timerItem = timersDic[timerId];
				if(timerItem != null)
					timerItem.over = true;
			}
		}
		
		void FixedUpdate()
		{
			if (timersDic.Count == 0)
				return;
			List<int> overTimers = new List<int> ();
            var e = timersDic.GetEnumerator();
            TimerItem timerItem = null;
            while(e.MoveNext())
            {
                timerItem = e.Current.Value;
                timerItem.UpdateTime(Time.fixedDeltaTime);
                if (timerItem.over)
                    overTimers.Add(timerItem.Id);
            }

            overTimers.ForEach(timerId => 
            {
                if (timersDic.ContainsKey(timerId))
                {
                    timersDic.Remove(timerId);
                }
            });
		}
	}
}

