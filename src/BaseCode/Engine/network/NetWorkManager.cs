using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Engine
{
	public class SocketEvent
	{		
		virtual public void OnReceiveMsg(byte[] buffer, int legnth){}

		virtual public void OnCloseSocket(){}

		virtual public void OnConnectSocket(){}

		virtual public void OnConnectTimeOut(){}
	}

	public class NetWorkManager : SocketEvent
	{
		public static NetWorkManager Instance;

		public delegate void HandleStatusChanged();
		public event HandleStatusChanged OnStatusChanged;

		Queue<NetMsg> _msgQueue;

		/// <summary>
		/// The net.
		/// </summary>
		private NetClient _netClient;
		
		// for catche unfulled package
		private byte[] _msgBuffers = null;
		private int    _halfBuffindex = 0;

	    public NetWorkManager()
	    {
			_msgQueue = new Queue<NetMsg>();
			Instance = this;
	    }

	    public override void OnReceiveMsg(byte[] buffer, int length)
	    {
	        int CurMsgConnetIndex = 0;  // current index of msg Connet
	        while (CurMsgConnetIndex < length)
	        {
				// check half package
				if(_msgBuffers != null)
				{
					Array.Copy(buffer, 0, _msgBuffers,  _msgBuffers.Length-_halfBuffindex, _halfBuffindex);
					CurMsgConnetIndex = _halfBuffindex;
			  		lock (this)
	                {
						NetMsg msg = NetMsg.CreateMsg(_msgBuffers);
	                    if (null != msg)
	                    {
							_msgQueue.Enqueue(msg);
	                    }
	                }
					_msgBuffers = null;
				}
					
				ushort msgSize = BitConverter.ToUInt16(buffer, CurMsgConnetIndex);
//				MyLogger.Log("MsgSize:"+msgSize);
				if (msgSize > 0 )
				{
					_msgBuffers = new byte[msgSize];
					
					if(CurMsgConnetIndex + msgSize > length)
					{
						int iRemain = length - CurMsgConnetIndex;
						Array.Copy(buffer, CurMsgConnetIndex, _msgBuffers, 0, iRemain);
						_halfBuffindex = msgSize - iRemain;
						break;
					}
					else
					{
						Array.Copy(buffer, CurMsgConnetIndex, _msgBuffers, 0, msgSize);
					}

	                lock (this)
	                {
						NetMsg msg = NetMsg.CreateMsg(_msgBuffers);
	                    if (null != msg)
	                    {
	                        _msgQueue.Enqueue(msg);
	                    }
	                }
					CurMsgConnetIndex += msgSize;
//					MyLogger.Log("CurMsgConnetIndex:"+CurMsgConnetIndex);
					_msgBuffers = null;
	            }
	            else
	            {
	                break;
	            }
	        }
	    }
		
	    /// <summary>
	    /// Ons the close socket.
	    /// </summary>
	    public override void OnCloseSocket()
	    {
			OnStatusChanged ();
	    }

		public override void OnConnectSocket()
		{
			OnStatusChanged ();
		}

		public override void OnConnectTimeOut()
		{
			OnStatusChanged ();
		}
	    /// <summary>
	    /// Init this instance.
	    /// </summary>
	    public void Init()
	    {
			_netClient = new NetClient();
			_netClient.Init(this);
	    }

	    /// <summary>
	    /// Sets the host.
	    /// </summary>
	    /// <param name='IP'>
	    /// I.
	    /// </param>
	    /// <param name='Port'>
	    /// Port.
	    /// </param>
	    public void SetHost(string IP, int Port)
	    {
			_netClient.SetHost(IP, Port);
	    }

	    /// <summary>
	    /// Connect this instance.
	    /// </summary>
	    public void Connect()
	    {
			_netClient.Connect();
	    }

	    /// <summary>
	    /// disconnect the connect.
	    /// </summary>
	    public void DisConnect()
	    {
			_netClient.DisConnect();
	    }

	    public void Send(byte[] buffer)
	    {
			_netClient.SendMsg(buffer);
	    }

		public static bool SendMsg(byte[] buffer)
		{
			if (Instance != null)
			{
				Instance.Send (buffer);
				return true;
			}
			return false;
		}

	    public NetStatus GetStatus()
	    {
			return  _netClient.GetStatus();
	    }

	    /// <summary>
	    /// Process this instance.
	    /// </summary>
	    public void Process()
	    {
			lock (this)
			{
	            while (_msgQueue.Count != 0)
	            {
					NetMsg msg = _msgQueue.Dequeue();
	                msg.Process();
	            }
			}
	    }
	}
}