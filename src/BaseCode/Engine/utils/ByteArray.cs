using System;
public class ByteArray
{
	// alloc byte array
	//----read
	static byte[] rbytes;
	static int    curReadIndex;
	static int    readStringAmount;
	//---write
	static byte[] wbytes;
	static int    curWriteIndex;
	static int	  writeStringIndex;
	static int    writeStringAmount;

	public static void StartRead(byte[] buffs)
	{
		rbytes = buffs;
		curReadIndex = 0;
		readStringAmount = 0;
	}
	
	public static byte ReadByte()
	{
		byte value = rbytes[curReadIndex];
		curReadIndex += sizeof(byte);
		return value;
	}

	public static int ReadInt()
	{
		int value = BitConverter.ToInt32(rbytes,curReadIndex);
		curReadIndex += sizeof(int);
		return value;
	}

	public static ushort ReadUInt16()
	{
		ushort value = BitConverter.ToUInt16(rbytes,curReadIndex);
		curReadIndex += sizeof(ushort);
		return value;
	}

	public static uint ReadUInt32()
	{
		uint value = BitConverter.ToUInt32(rbytes,curReadIndex);
		curReadIndex += sizeof(uint);
		return value;
	}

	public static string ReadStringEx(int tSize=32)
	{
		string readString = ByteConvert.MsgBytesToStringEx(rbytes,curReadIndex,tSize);
		curReadIndex += tSize;
		return readString;
	}

	public static string ReadString()
	{
		if (readStringAmount == 0)
			readStringAmount = rbytes[curReadIndex++];
		if (curReadIndex >= rbytes.Length)
			return "";
		string readString = ByteConvert.MsgBytesToString(rbytes,curReadIndex);
		curReadIndex += readString.Length + sizeof(byte);
		return readString;
	}

	//---------------------------------------------
	public static void StartWrite(int size=1024)
	{
		wbytes = new byte[size];
		curWriteIndex = 0;
		writeStringAmount = 0;
	}

	public static void WriteByte(byte value)
	{
		wbytes [curWriteIndex] = value;
		curWriteIndex += sizeof(byte);
	}

	public static void WriteUshort(ushort value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		wbytes[curWriteIndex] = bytes[0];
		wbytes[curWriteIndex + 1] = bytes[1];
		curWriteIndex += sizeof(ushort);
	}

	public static void WriteInt(int value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		wbytes[curWriteIndex] = bytes[0];
		wbytes[curWriteIndex + 1] = bytes[1];
		wbytes[curWriteIndex + 2] = bytes[2];
		wbytes[curWriteIndex + 3] = bytes[3];
		curWriteIndex += sizeof(int);
	}

	public static void WriteUint32(uint value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		wbytes[curWriteIndex] = bytes[0];
		wbytes[curWriteIndex + 1] = bytes[1];
		wbytes[curWriteIndex + 2] = bytes[2];
		wbytes[curWriteIndex + 3] = bytes[3];
        curWriteIndex += sizeof(uint);
	}

	public static void WriteStringEx(string wString,int tSize=32)
	{
		byte[] strBuffs = System.Text.Encoding.UTF8.GetBytes(wString);
		strBuffs.CopyTo (wbytes, curWriteIndex);
		curWriteIndex += tSize;
	}

	public static void WriteString(string wString)
	{
		if (writeStringAmount == 0) {
			writeStringIndex = curWriteIndex;
			curWriteIndex += sizeof(byte);
		}
		writeStringAmount += 1;
		wbytes[writeStringIndex] = (byte)writeStringAmount;
		byte[] strBuffs = ByteConvert.StringToMsgBytes (wString);
		strBuffs.CopyTo (wbytes, curWriteIndex);
		curWriteIndex += strBuffs.Length;
	}

	public static byte[] EndWrite()
	{
		byte[] endbytes = new byte[curWriteIndex];
		Array.Copy (wbytes, 0, endbytes, 0, curWriteIndex);
		return endbytes;
	}
}

