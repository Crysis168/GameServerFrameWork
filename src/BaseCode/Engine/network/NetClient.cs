using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Engine
{
	/// <summary>
	/// Net status.
	/// </summary>
	public enum NetStatus
    {
        E_STATUS_NONE,
        E_STATUS_CONNECTED,
        E_STATUS_DISCONNECTED,
		E_STATUS_TIMEOUT,
    };

	
    public class NetClient 
    {
        public NetClient()
        {
			
        }

        public void Init(SocketEvent Receiver)
        {
			_socketEventReceiver = Receiver;
            _socketStatus = NetStatus.E_STATUS_NONE;
        }

        public void SetHost(string IP, int Port)
        {
            _ipAddress   = IPAddress.Parse(IP);
            _port = Port;
			_ipEndPoint = new IPEndPoint(_ipAddress, _port);
        }

        public bool Connect()
        {
			DisConnect ();
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IAsyncResult result = _clientSocket.BeginConnect(_ipEndPoint, new AsyncCallback(OnConnectResult), _clientSocket);
			
			Thread thread = new Thread(TimeOutCheck);
			thread.IsBackground = true;
			thread.Start(result);

            return true;
        }
		
		public void TimeOutCheck(object obj)
		{
			IAsyncResult result = obj as IAsyncResult;
			if(null != result)
			{

				bool bSucced = result.AsyncWaitHandle.WaitOne(10000, true);
				
				if(!bSucced)
				{
					DisConnect();
					_socketStatus = NetStatus.E_STATUS_TIMEOUT;
					_socketEventReceiver.OnConnectTimeOut();
				}
			}
		}

        public void DisConnect()
        {
			if(_recvMsgThread != null)
			{
				_recvMsgThread.Abort();
			}
			
			if (null != _clientSocket && _clientSocket.Connected)
            {
				_clientSocket.Shutdown(SocketShutdown.Both);
				_clientSocket.Close();      
				
				_clientSocket = null;
				_recvMsgThread = null;
				_socketStatus = NetStatus.E_STATUS_DISCONNECTED;
				_socketEventReceiver.OnCloseSocket();
			}
        }

        public NetStatus GetStatus()
        {
            return _socketStatus;
        }

        public bool SendMsg(byte[] buffer)
        {
			if(_clientSocket != null && _clientSocket.Connected)
			{
				_clientSocket.Send(buffer);
				return true;
			}
			return false;
        }

        private void ReceiveMessage()
        {
            // receive message from server in this thread
            while(true)
            {
				if (!_clientSocket.Connected)
                {
                    // disconnect from server
                    _clientSocket.Close();
                    _socketStatus = NetStatus.E_STATUS_DISCONNECTED;
					_socketEventReceiver.OnCloseSocket();
					break;
                }
				
                try
                {
                    byte[] buffer = new byte[NetMsg.MAX_MSG_PACKAGE_SIZE];
					int ilength = _clientSocket.Receive(buffer);
					MyLogger.Log("ReceiveMsg:"+ilength);
                    if (ilength <= 0)
                    {
                        // socket occured some error
						_clientSocket.Close();
						_socketStatus = NetStatus.E_STATUS_DISCONNECTED;
						_socketEventReceiver.OnCloseSocket();
			
                        break;
                    }

                    // all of the  message 's length is big than 2
                    if (buffer.Length > 2)
                    {
						_socketEventReceiver.OnReceiveMsg(buffer, ilength);
                    }
                    else
                    {
                        Debug.LogWarning("length is not > 3");
                    }
                    
                }
                catch (System.Exception ex)
                {
                    // failed to socket error
					Debug.LogException(ex);
					Debug.LogWarning("there is have occur some error");
					
					_clientSocket.Close();
					_socketEventReceiver.OnCloseSocket();
                    break;
                }
            }
        }

        private void OnConnectResult(IAsyncResult result)
        {
			try
			{
				Socket socketClient = (Socket)result.AsyncState;
				socketClient.EndConnect(result);
				if(socketClient.Connected)
				{
					
					_recvMsgThread = new Thread(new ThreadStart(ReceiveMessage));
					_recvMsgThread.IsBackground = true;
					
					_recvMsgThread.Start();
					_socketStatus = NetStatus.E_STATUS_CONNECTED;
					_socketEventReceiver.OnConnectSocket();
				}
				else
				{
					
				}
					
			}
			catch
			{
				Debug.LogWarning("connect result  failed");
			}
        }

        private Socket _clientSocket;
        private IPAddress _ipAddress;
        private IPEndPoint _ipEndPoint;
        private int _port;
        private NetStatus _socketStatus;
		private Thread _recvMsgThread;
		private SocketEvent _socketEventReceiver;
    }
}
