#if UNITY_WEBGL && !UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AOT;
using Ninja.WebSockets;
using UnityEngine;

namespace Mirror.Websocket
{
    // this is the client implementation used by browsers
    public class Client
    {
        static int idGenerator = 0;
        static readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();

        public bool NoDelay = true;

        public event Action Connected;
        public event Action<ArraySegment<byte>> ReceivedData;
        public event Action Disconnected;
        public event Action<Exception> ReceivedError;

        public bool Connecting { get; set; }
        public bool IsConnected
        {
            get
            {
                return SocketState(m_NativeRef) != 0;
            }
        }

        int m_NativeRef = 0;
        readonly int id;

        static byte[] buffer;

        public Client(int BufferSize)
        {
            id = Interlocked.Increment(ref idGenerator);
            buffer = new byte[BufferSize];
        }

        public void Connect(Uri uri)
        {
            clients[id] = this;

            Connecting = true;

            m_NativeRef = SocketCreate(uri.ToString(), id, OnOpen, OnData, OnClose);
        }

        public void Disconnect()
        {
            SocketClose(m_NativeRef);
        }

        // send the data or throw exception
        public void Send(byte[] data)
        {
            SocketSend(m_NativeRef, data, data.Length);
        }


        #region Javascript native functions
        [DllImport("__Internal")]
        static extern int SocketCreate(
            string url,
            int id,
            Action<int> onpen,
            Action<int, IntPtr, int> ondata,
            Action<int> onclose);

        [DllImport("__Internal")]
        static extern int SocketState(int socketInstance);

        [DllImport("__Internal")]
        static extern void SocketSend(int socketInstance, byte[] ptr, int length);

        [DllImport("__Internal")]
        static extern void SocketClose(int socketInstance);

        #endregion

        #region Javascript callbacks

        [MonoPInvokeCallback(typeof(Action))]
        public static void OnOpen(int id)
        {
            clients[id].Connecting = false;
            clients[id].Connected?.Invoke();
        }

        [MonoPInvokeCallback(typeof(Action))]
        public static void OnClose(int id)
        {
            clients[id].Connecting = false;
            clients[id].Disconnected?.Invoke();
            clients.Remove(id);
        }

        [MonoPInvokeCallback(typeof(Action))]
        public static void OnData(int id, IntPtr ptr, int length)
        {
            if (length <= buffer.Length)
            {
                Marshal.Copy(ptr, buffer, 0, length);
                ArraySegment<byte> data = new ArraySegment<byte>(buffer, 0, length);
                clients[id].ReceivedData(data);
            }
            else
            {
                clients[id].ReceivedError(new Exception("Message is larger than specified max message length"));
            }
        }
        #endregion
    }
}

#endif
