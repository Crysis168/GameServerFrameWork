using UnityEngine;
using System.Collections;

public interface IRecord 
{
	void Log(string msg);
	void LogWarning(string msg);
	void LogError(string msg);
}
