/// <summary>
/// 工具类：对象与二进制流间的转换
/// </summary>
///
using System;
using System.IO;
using System.Runtime.InteropServices;

class ByteConvert
{
	public static byte[] StruntToBytes(object structObj)
	{
		// get object size
		int size = Marshal.SizeOf(structObj);
		
		// alloc byte array
		byte[] bytes = new byte[size];
		// 
		IntPtr structPtr = Marshal.AllocHGlobal(size);
		
		Marshal.StructureToPtr(structObj, structPtr, false);
		
		Marshal.Copy(structPtr, bytes, 0, size);
		
		Marshal.FreeHGlobal(structPtr);
		
		return bytes;
	}
	
	/// <summary>
	/// 将byte数组转换成对象
	/// </summary>
	/// <param name="buff">被转换byte数组</param>
	/// <param name="typ">转换成的类名</param>
	/// <returns>转换完成后的对象</returns>
	public static object BytesToStructs(byte[] bytes, Type type)
	{
		int size = Marshal.SizeOf(type);
		
		if (size > bytes.Length)
			return null;
		
		// alloc memory 
		IntPtr structPtr = Marshal.AllocHGlobal(size);
		Marshal.Copy(bytes, 0, structPtr, size);
		object obj = Marshal.PtrToStructure(structPtr, type);
		Marshal.FreeHGlobal(structPtr);
		
		return obj;
	}

	public static string MsgBytesToStringEx(byte[] bytes,int startIndex,int tSize)
	{
		byte[] bytestring = new byte[tSize];
		Array.Copy (bytes, startIndex, bytestring, 0, tSize);
		return System.Text.Encoding.UTF8.GetString ( bytestring );
	}

	public static string MsgBytesToString(byte[] bytes,int startIndex)
	{
		int strLength = (int)bytes[startIndex++];
		if (strLength == 0)
			return "";
		byte[] bytestring = new byte[strLength];
		Array.Copy (bytes, startIndex, bytestring, 0, strLength);
		return System.Text.Encoding.UTF8.GetString ( bytestring );
	}
	
	public static byte[] StringToMsgBytes(string str)
	{
		byte[] bytestring = System.Text.Encoding.UTF8.GetBytes(str);
		byte[] msgByte = new byte[bytestring.Length + 1];
		
		// first set first element as the string's length
		msgByte[0] = (byte)bytestring.Length;
		
		bytestring.CopyTo(msgByte, 1);
		
		return msgByte;
	}
}
