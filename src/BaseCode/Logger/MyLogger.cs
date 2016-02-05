using System;
using UnityEngine;

public class MyLogger
{
	public static bool enableLog = true;
	public static bool enableWarnLog = true;
	public static bool enableErrorLog = true;

	public static IRecord record = new GameLogger();
	private static bool ms_bEditor = false;

	public static void CheckEditor()
	{
		if(Application.platform == RuntimePlatform.WindowsEditor
		   || Application.platform == RuntimePlatform.OSXEditor)
		{
			ms_bEditor = true;
		}
		else
		{
			ms_bEditor = false;
		}
	}

	public static void Log(string msg)
	{
		if(ms_bEditor)
		{
			Debug.Log(msg);
		}
		if(enableLog)
		{
			record.Log(msg);
		}
	}

	public static void LogError(string msg)
	{
		if(ms_bEditor)
		{
			Debug.LogError(msg);
		}
		if(enableErrorLog)
		{
			record.LogError(msg);
		}
	}

	public static void LogError(string msg,Exception ex)
	{
		if(enableErrorLog)
		{
			record.LogError(msg + "\n Error Type:"+ ex.Message + "Detial:\n" + ex.StackTrace);
		}
	}

	public static void LogWarning(string msg)
	{
		if(ms_bEditor)
		{
			Debug.LogWarning(msg);
		}
		if(enableWarnLog)
		{
			record.LogWarning(msg);
		}
	}
}
