//----------------------------------------------
//			        SUGUI
// Copyright © 2012-2015 xiaobao1993.com
//----------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SUGUI
{

/// <summary>
/// 透明度补间
/// </summary>

[AddComponentMenu("SUGUI/Tween/Tween Alpha")]
public class TweenAlpha : UITweener
{
#if UNITY_3_5
	public float from = 1f;
	public float to = 1f;
#else
	[Range(0f, 1f)] public float from = 1f;
	[Range(0f, 1f)] public float to = 1f;
#endif    

//    private Dictionary<MaskableGraphic,Color> _dicGraphics=new Dictionary<MaskableGraphic,Color>();
     MaskableGraphic[] mMaskableGraphic;
     Color mColor;

    public MaskableGraphic[] cachedMaskableGraphic
	{
		get
		{
            if (mMaskableGraphic == null)
			{
                mMaskableGraphic = GetComponentsInChildren<MaskableGraphic>();
                if (mMaskableGraphic == null) mMaskableGraphic = GetComponentsInChildren<MaskableGraphic>();
			}
            mColor = new Color();
            return mMaskableGraphic;
		}
	}

	public float value 
    { 
        get 
        {
            return alpha;
        } 
        set 
        {
            alpha = value;
        }
    }

    public float alpha
    {
        get
        {
            return mColor.a;
        }
        set
        {
            for (int i = 0; i < cachedMaskableGraphic.Length;i++ )
            {
                cachedMaskableGraphic[i].color = new Color(cachedMaskableGraphic[i].color.r, cachedMaskableGraphic[i].color.g, cachedMaskableGraphic[i].color.b, value);
            }
            mColor.a = value;
        }
    }

	protected override void OnUpdate (float factor, bool isFinished) { value = Mathf.Lerp(from, to, factor); }

	/// <summary>
	/// 开始补间操作
	/// </summary>

	static public TweenAlpha Begin (GameObject go, float duration, float alpha)
	{
		TweenAlpha comp = UITweener.Begin<TweenAlpha>(go, duration);
		comp.from = comp.value;
		comp.to = alpha;

		if (duration <= 0f)
		{
			comp.Sample(1f, true);
			comp.enabled = false;
		}
		return comp;
	}

	public override void SetStartToCurrentValue () { from = value; }
	public override void SetEndToCurrentValue () { to = value; }
}

}
