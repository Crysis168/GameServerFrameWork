using System;
using System.IO;
using UnityEngine;

public class GameLogger:IRecord
{
	private FileStream mFile;
	private BinaryWriter mFileWriter;
	
	public GameLogger()
	{
		DateTime now = DateTime.Now;
		string dir = "";
		if(Application.platform == RuntimePlatform.Android)
		{
			dir = Application.persistentDataPath + "/logs/";
		}
		else if(Application.platform == RuntimePlatform.IPhonePlayer)
		{
			dir = Application.persistentDataPath + "/logs/";
		}
		else if(Application.platform == RuntimePlatform.WindowsPlayer)
		{
			dir =  Application.dataPath + "/logs/";
		}
		else
			return;
		if(!Directory.Exists(dir))
		{
			Directory.CreateDirectory(dir);
		}
		this.mFile = new FileStream(dir + "log_" + now.ToString("MM-dd_hh-mm-ss") + ".log",FileMode.Append);
		this.mFileWriter = new BinaryWriter(this.mFile);
	}
	
	~GameLogger()
	{
		if(this.mFileWriter != null)
		{
			this.mFileWriter.Close();
			this.mFileWriter = null;
		}
		if(this.mFile != null)
		{
			this.mFile.Close();
			this.mFile = null;
		}
	}
	
	private void LogRecord(string msg,string title)
	{
		if(mFileWriter != null)
		{
			string[] formatStrs = new string[] { "["+title+"]",DateTime.Now.ToString("yyyy-MM-DD HH:mm:ss\r"),msg,"\r\n"};
			this.mFileWriter.Write(string.Concat(formatStrs).ToCharArray());
			this.mFileWriter.Flush();
		}
	}

	public void Log(string msg)
	{
		LogRecord (msg, "LOG");
	}
	public void LogWarning(string msg)
	{
		LogRecord (msg, "WARN");
	}
	public void LogError(string msg)
	{
		LogRecord (msg, "ERROR");
	}
}

