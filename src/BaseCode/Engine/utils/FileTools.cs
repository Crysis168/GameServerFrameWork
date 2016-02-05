using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

public class FileTools
{
	public static string NewLine = "\r\n";
	
	public static bool IsExistDirectory(string directoryPath)
	{
		return Directory.Exists(directoryPath);
	}
	
	public static bool IsExistFile(string filePath)
	{
		return File.Exists(filePath);
	}
	
	public static bool IseEmptyDirectory(string directoryPath)
	{
		try
		{
			string[] fileNames = GetFileNames(directoryPath);
			if(fileNames.Length > 0)
				return false;
			return true;
		}
		catch(Exception ex)
		{
			MyLogger.LogError("FileUtil:IsEmptyDirectory;Common",ex);
			return true;
		}
	}
	
	public static bool Contains(string directoryPath,string searchPattern)
	{
		try
		{
			string[] fileNames = GetFileNames(directoryPath,searchPattern,false);
			if(fileNames.Length == 0)
				return false;
			else
				return true;
		}
		catch(Exception ex)
		{
			MyLogger.LogError("FileUtil:Contains;Common",ex);
			return true;
		}
	}
	
	public static bool Contains(string directoryPath,string searchPattern,bool isSearchChild)
	{
		try
		{
			string[] fileNames = GetFileNames(directoryPath,searchPattern,true);
			if(fileNames.Length == 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		catch(Exception ex)
		{
			MyLogger.LogError("FileUtil:Contains;Common",ex);
			return false;
		}
	}
	
	public static void CreateDirectory(string directoryPath)
	{
		if(!IsExistDirectory(directoryPath))
		{
			Directory.CreateDirectory(directoryPath);
		}
	}
	
	public static void CreateFile(string filePath)
	{
		try
		{
			if(!IsExistFile(filePath))
			{
				string dir = filePath.Replace("/","\\");
				if(dir.IndexOf("\\") > -1)
					CreateDirectory(dir.Substring(0,dir.LastIndexOf("\\")));
				FileInfo file = new FileInfo(filePath);
				FileStream fs = file.Create();
				fs.Close();
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}
	
	public static void CreateFile(string filePath,byte[] buffer)
	{
		try
		{
			if(!IsExistFile(filePath))
			{
                string dir = filePath;
                if (dir.IndexOf("\\") > -1)
                    CreateDirectory(dir.Substring(0, dir.LastIndexOf("\\")));
                FileInfo file = new FileInfo(filePath);
				FileStream fs = file.Create();
				fs.Write(buffer,0,buffer.Length);
				fs.Close();
			}
		}
		catch(Exception ex)
		{
			throw ex;
		}
	}

	public static void WriteFile(string filePath,byte[] buffer)
	{
		try
		{
			if(!IsExistFile(filePath))
			{
				CreateFile(filePath,buffer);
			}
			else
			{
				FileInfo file = new FileInfo(filePath);
				FileStream fs = file.OpenWrite();
				fs.Write(buffer,0,buffer.Length);
				fs.Close();
			}
		}
		catch(Exception ex)
		{
			throw ex;
		}
	}
	
	public static bool CreateFile(string filePath,string s,string encode)
	{
		bool ret = true;
		Encoding code = Encoding.GetEncoding (encode);
		if(!IsExistFile(filePath))
		{
			string dir = filePath.Replace("/","\\");
			if(dir.IndexOf("\\") > -1)
				CreateDirectory(dir.Substring(0,dir.LastIndexOf("\\")));
		}
		StreamWriter sw = null;
		try
		{
			sw = new StreamWriter(filePath,false,code);
			sw.Write(s);
			sw.Flush();
		}
		catch(Exception ex)
		{
			ret = false;
			throw ex;
		}
		finally
		{
			sw.Close();
		}
		return ret;
	}
	
	public static int GetLineCount(string filePath)
	{
		string[] rows = File.ReadAllLines (filePath);
		return rows.Length;
	}
	
	public static int GetFileSize(string filePath)
	{
		FileInfo fi = new FileInfo (filePath);
		return (int)fi.Length;
	}
	
	public static double GetFileSizeByKB(string filePath)
	{
		FileInfo fi = new FileInfo (filePath);
		return Convert.ToDouble (Convert.ToDouble (fi.Length) / 1024);
	}
	
	public static double GetFileSizeByMB(string filePath)
	{
		FileInfo fi = new FileInfo (filePath);
		return Convert.ToDouble (Convert.ToDouble (fi.Length) / 1024 / 1024);
	}
	
	public static string[] GetFileNames(string directoryPath)
	{
		if(!IsExistDirectory(directoryPath))
		{
			throw new FileNotFoundException();
		}
		return Directory.GetFiles(directoryPath);
	}
	
	public static string[] GetFileNames(string directoryPath,string searchPattern,bool isSearchChild)
	{
		if(!IsExistDirectory(directoryPath))
		{
			throw new FileNotFoundException();
		}
		try
		{
			if(isSearchChild)
			{
				return Directory.GetFiles(directoryPath,searchPattern,SearchOption.AllDirectories);
			}
			else
			{
				return Directory.GetFiles(directoryPath,searchPattern,SearchOption.TopDirectoryOnly);
			}
		}
		catch(IOException ex)
		{
			throw ex;
		}
	}
	
	public static string[] GetDirectories(string directoryPath)
	{
		try
		{
			return Directory.GetDirectories(directoryPath);
		}
		catch(IOException ex)
		{
			throw ex;
		}
	}
	
	public static int GetFilesCount(DirectoryInfo dirInfo)
	{
		int total = 0;
		total += dirInfo.GetFiles ().Length;
		foreach(DirectoryInfo subDir in dirInfo.GetDirectories())
		{
			total += GetFilesCount(subDir);
		}
		return total;
	}
	
	public static string[] GetDirectories(string directoryPath,string searchPattern,bool isSearchChild)
	{
		try
		{
			if(isSearchChild)
			{
				return Directory.GetDirectories(directoryPath,searchPattern,SearchOption.AllDirectories);
			}
			else
			{
				return Directory.GetDirectories(directoryPath,searchPattern,SearchOption.TopDirectoryOnly);
			}
		}
		catch(IOException ex)
		{
			throw ex;
		}
	}
	
	public static void WriteText(string filePath,string content)
	{
		File.WriteAllText (filePath, content);
	}
	
	public static void AppendText(string filePath,string content)
	{
		File.AppendAllText (filePath, content);
	}
	
	public static void Copy(string sourceFilePath,string destFilePath)
	{
		File.Copy (sourceFilePath, destFilePath,true);
	}
	
	public static void Move(string sourceFilePath,string descDirectoryPath)
	{
		string sourceFileName = GetFileName (sourceFilePath);
		if (IsExistDirectory (descDirectoryPath)) 
		{
			if(IsExistFile(descDirectoryPath+"\\"+sourceFileName))
			{
				DeleteFile(descDirectoryPath + "\\"+sourceFileName);
			}
			File.Move(sourceFilePath,descDirectoryPath+"\\"+sourceFileName);
		}
	}
	
	public static void CopyDirectroy(string from,string to)
	{
		try
		{
			if((to.Length -1 )!= Path.DirectorySeparatorChar)
			{
				to += Path.DirectorySeparatorChar;
			}
			if(!Directory.Exists(to))
			{
				Directory.CreateDirectory(to);
			}
			string[] fileList = Directory.GetFileSystemEntries(from);
			foreach(string file in fileList)
			{
				if(Directory.Exists(file))
				{
					CopyDirectroy(file,to+Path.GetFileName(file));
				}
				else
				{
					File.Copy(file,to+Path.GetFileName(file),true);
				}
			}
		}
		catch(Exception ex)
		{
			MyLogger.LogError("Copy Directory Error"+ex.Message);
		}
	}
	
	public static byte[] StreamToBytes(Stream stream)
	{
		try
		{
			byte[] buffer = new byte[stream.Length];
			stream.Read(buffer,0,Convert.ToInt32(stream.Length));
			return buffer;
		}
		catch(Exception ex)
		{
			throw ex;
		}
		finally
		{
			stream.Close();
		}
	}
	
	public static byte[] FileToBytes(string filePath)
	{
		int fileSize = GetFileSize (filePath);
		byte[] buffer = new byte[fileSize];
		FileInfo fi = new FileInfo (filePath);
		FileStream fs = fi.Open (FileMode.Open);
		try
		{
			fs.Read(buffer,0,fileSize);
			return buffer;
		}
		catch(IOException ex)
		{
			throw ex;
		}
		finally
		{
			fs.Close();
		}
	}
	
	public static string FileToString(string filePath)
	{
		return FileToString(filePath,Encoding.Default);
	}
	
	public static string FileToString(string filePath,Encoding encoding)
	{
		StreamReader reader = new StreamReader (filePath, encoding);
		try
		{
			return reader.ReadToEnd();
		}
		catch(Exception ex)
		{
			throw ex;
		}
		finally
		{
			reader.Close();
		}
	}

	public static bool Save(string path,byte[] bytes)
	{
		if(bytes == null)
		{
			if(File.Exists(path)) File.Delete(path);
			return true;
		}

#if (UNITY_STANDALONE_WIN || UNITY_EDITOR)
        path = path.Replace("/", "\\"); 
#endif

		if (!IsExistFile (path)) 
		{
			FileTools.CreateFile(path,bytes);
			return true;
		}
		FileStream file = null;
		try
		{
			file = File.Create(path);
		}
		catch(System.Exception ex)
		{
			MyLogger.Log(ex.Message);
			return false;
		}
		
		file.Write (bytes, 0, bytes.Length);
		file.Close ();
		return true;
	}
	
	public static string GetFileName(string filePath)
	{
		FileInfo fi = new FileInfo (filePath);
		return fi.Name;
	}
	
	public static string GetFileNameNoExtension(string filePath)
	{
		FileInfo fi = new FileInfo (filePath);
		return fi.Name.Split('.')[0];
	}
	
	public static string GetDirectoryName(string filePath)
	{
		DirectoryInfo di = new DirectoryInfo (filePath);
		return di.Name;
	}
	
	public static string GetExtension(string filePath)
	{
		FileInfo fi = new FileInfo (filePath);
		return fi.Extension;
	}
	
	public static void ClearDirectory(string directoryPath)
	{
		if(IsExistDirectory(directoryPath))
		{
			string[] fileNames = GetFileNames(directoryPath);
			for(int i=0;i<fileNames.Length;i++)
			{
				DeleteFile(fileNames[i]);
			}
			string[] directoryNames = GetDirectories(directoryPath);
			for(int i=0;i<directoryNames.Length;i++)
			{
				DeleteDirectory(directoryNames[i]);
			}
		}
	}
	
	public static void ClearFIle(string filePath)
	{
		File.Delete (filePath);
		CreateFile (filePath);
	}
	
	public static void DeleteFile(string filePath)
	{
		if(IsExistFile(filePath))
		{
			File.Delete(filePath);
		}
	}
	
	public static void DeleteDirectory(string directoryPath,Action deleteCallBack = null)
	{
		if (IsExistDirectory (directoryPath)) 
		{
			string[] files = Directory.GetFiles(directoryPath);
			string[] dirs = Directory.GetDirectories(directoryPath);
			foreach(string file in files)
			{
				File.SetAttributes(file,FileAttributes.Normal);
				File.Delete(file);
				if(deleteCallBack != null)
				{
					deleteCallBack();
				}
			}
			foreach(string dir in dirs)
			{
				DeleteDirectory(dir,deleteCallBack);
			}
			Directory.Delete(directoryPath,false);
		}
	}

	public static string GetMD5HashFromFile(string fileName)  
	{  
		try  
		{  
			FileStream file = new FileStream(fileName, System.IO.FileMode.Open);  
			MD5 md5 = new MD5CryptoServiceProvider();  
			byte[] retVal = md5.ComputeHash(file);  
			file.Close();
			string result = System.BitConverter.ToString(retVal);
			result = result.Replace("-","");
			return result;  
		}  
		catch (Exception ex)  
		{  
			throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);  
		}  
	}  
}

