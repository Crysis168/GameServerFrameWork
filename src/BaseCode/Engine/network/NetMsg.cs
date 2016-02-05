using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using PureMVC.Patterns;

namespace Engine
{
	public class NetMsg : Notifier
	{
		public static readonly int HEAD_SIZE = 4;
		public static readonly int MAX_NAMESIZE = 32;
		public static readonly int MAX_MSG_PACKAGE_SIZE = 1024;

		static Dictionary<uint,Type> s_msgMap = new Dictionary<uint,Type>();

		protected ushort  _msgType;
		protected ushort  _msgSize;
		protected byte[]  _buffer;

		public NetMsg ()
		{
		}

		public void SetBuffer(byte[] bytes)
		{
			_buffer = bytes;
		}

		public bool Send()
		{
			byte[] msgBuffer = FormatMsgBuffers();
			NetWorkManager.SendMsg(msgBuffer);
			return true;
		}

		byte[] FormatMsgBuffers()
		{
			// Fill buffer size
			_msgSize = (ushort)(_buffer.Length + HEAD_SIZE);
			byte[] msgBuffers = new byte[_msgSize];
			byte[] wBytes = BitConverter.GetBytes(_msgSize);
			wBytes.CopyTo (msgBuffers, 0);
			wBytes = BitConverter.GetBytes(_msgType);
			wBytes.CopyTo (msgBuffers, 2);
			if(_buffer != null)
				_buffer.CopyTo(msgBuffers, HEAD_SIZE);
			return msgBuffers;
		}

		virtual protected void ReadMsg()
		{
		}

		virtual protected void FormatMsg()
		{
		}

		virtual public void Process()
		{
		}

		public ushort type
		{
			get{ return _msgType;}
		}

		public ushort size
		{
			get{ return _msgSize; }
		}

		public static NetMsg CreateMsg(byte[] buffers)
		{
			ushort msgSize = BitConverter.ToUInt16 (buffers, 0);
			ushort msgType = BitConverter.ToUInt16 (buffers, 2);
			MyLogger.Log("CreateMsg:"+msgType+"-"+msgSize);

			if(s_msgMap.ContainsKey(msgType))
			{
				NetMsg msg = Activator.CreateInstance(s_msgMap[msgType]) as NetMsg;
				byte[] msgbf = new byte[msgSize - HEAD_SIZE];
				Array.Copy(buffers,HEAD_SIZE,msgbf,0,buffers.Length - HEAD_SIZE);
				msg.SetBuffer(msgbf);
				msg.ReadMsg ();
				return msg;
			}
			return null;
		}

		public static void RegisterMsg(uint msgCode, Type msgClass)
		{
			if(s_msgMap.ContainsKey(msgCode) == false)
				s_msgMap[msgCode] = msgClass;
		}

		public static void ClearAllMsg()
		{
			s_msgMap.Clear ();
		}
	}
}

