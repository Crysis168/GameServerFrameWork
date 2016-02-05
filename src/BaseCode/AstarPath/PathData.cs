using UnityEngine;
using System.IO;
using System.Text;

public class PathData
{
	static readonly string s_ExternName = ".pd";

	static string GetRealPath(string pdName)
	{
		return Application.dataPath + "/" + pdName;
	}

	public static void SavePathData(string pdName,Texture2D texture)
	{
		MyLogger.Log (pdName + " SavePathData begin!!!");
		int width = texture.width;
		int height = texture.height;
		int size = sizeof(int) + sizeof(int) + sizeof(byte) * width * height;
		ByteArray.StartWrite (size);
		ByteArray.WriteInt (width);
		ByteArray.WriteInt (height);
		int x, y;
		for (x = 0; x < width; x++) 
		{
			for (y = 0; y < height; y++) 
			{
				byte pixel = (byte)(texture.GetPixel (x, y) == Color.white ? 0 : 1);
				ByteArray.WriteByte(pixel);
			}
		}
		byte[] bytes = ByteArray.EndWrite ();
		string pdPath = GetRealPath (pdName)+s_ExternName;
		FileTools.WriteFile (pdPath, bytes);
		MyLogger.Log (pdPath + " SavePathData ok!!!");
	}

	public static void SavePathJPG(string jpgName,Texture2D textureGrid)
	{
		MyLogger.Log (jpgName + " SavePathJPG begin!!!");
		string jpgPath = GetRealPath (jpgName)+".jpg";
		byte[] bytes = textureGrid.EncodeToJPG ();
		File.WriteAllBytes (jpgPath, bytes);
		MyLogger.Log (jpgPath + " SavePathJPG ok!!!");
	}

	public static Texture2D LoadPathJPG(string jpgName)
	{
		string jpgPath = GetRealPath (jpgName)+".jpg";
		MyLogger.Log (jpgPath + " LoadPathJPG begin!!!");
		byte[] bytes = File.ReadAllBytes (jpgPath);
		Texture2D texture = new Texture2D (1,1);
		texture.LoadImage (bytes);
		MyLogger.Log (jpgPath + " LoadPathJPG ok!!!");
		return texture;
	}

	public static byte[] LoadPathData(string pdName)
	{
		string pdPath = GetRealPath (pdName)+s_ExternName;
		MyLogger.Log (pdPath + " LoadPathData begin!!!");
		byte[] bytes = File.ReadAllBytes (pdPath);
		MyLogger.Log (pdPath + " LoadPathData ok!!!");
		return bytes;
	}
}

