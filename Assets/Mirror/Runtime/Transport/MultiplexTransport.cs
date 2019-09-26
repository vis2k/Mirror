using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mirror
{
    // a transport that can listen to multiple underlying transport at the same time
    public class MultiplexTransport : Transport
    {
        public Transport[] transports;

        // used to partition recipients for each one of the base transports
        // without allocating a new list every time
        private List<int>[] recipientsCache;

        public void Awake()
        {
            if (transports == null || transports.Length == 0)
            {
                Debug.LogError("Multiplex transport requires at least 1 underlying transport");
            }
            InitClient();
            InitServer();
        }

        #region Client
        // clients always pick the first transport
        void InitClient()
        {
            // wire all the base transports to my events
            foreach (Transport transport in transports)
            {
                transport.OnClientConnected.AddListener(OnClientConnected.Invoke );
                transport.OnClientDataReceived.AddListener(OnClientDataReceived.Invoke);
                transport.OnClientError.AddListener(OnClientError.Invoke );
                transport.OnClientDisconnected.AddListener(OnClientDisconnected.Invoke);
            }
        }

        // The client just uses the first transport available
        Transport GetAvailableTransport()
        {
            foreach (Transport transport in transports)
            {
                if (transport.Available())
                {
                    return transport;
                }
            }
            throw new Exception("No transport suitable for this platform");
        }

        public override void ClientConnect(string address)
        {
            GetAvailableTransport().ClientConnect(address);
        }

        public override bool ClientConnected()
        {
            return GetAvailableTransport().ClientConnected();
        }

        public override void ClientDisconnect()
        {
            GetAvailableTransport().ClientDisconnect();
        }

        public override bool ClientSend<T>(int channelId, T msg)
        {
            return GetAvailableTransport().ClientSend(channelId, msg);
        }

        public override int GetMaxPacketSize(int channelId = 0)
        {
            return GetAvailableTransport().GetMaxPacketSize(channelId);
        }

        #endregion


        #region Server
        // connection ids get mapped to base transports
        // if we have 3 transports,  then
        // transport 0 will produce connection ids [0, 3, 6, 9, ...]
        // transport 1 will produce connection ids [1, 4, 7, 10, ...]
        // transport 2 will produce connection ids [2, 5, 8, 11, ...]
        int FromBaseId(int transportId, int connectionId)
        {
            return connectionId * transports.Length + transportId;
        }

        int ToBaseId(int connectionId)
        {
            return connectionId / transports.Length;
        }

        int ToTransportId(int connectionId)
        {
            return connectionId % transports.Length;
        }

        void InitServer()
        {
            recipientsCache = new List<int>[transports.Length];

            // wire all the base transports to my events
            for (int i = 0; i < transports.Length; i++)
            {
                recipientsCache[i] = new List<int>();

                // this is required for the handlers,  if I use i directly
                // then all the handlers will use the last i
                int locali = i;
                Transport transport = transports[i];

                transport.OnServerConnected.AddListener(baseConnectionId =>
                {
                    OnServerConnected.Invoke(FromBaseId(locali, baseConnectionId));
                });

                transport.OnServerDataReceived.AddListener((baseConnectionId, data) =>
                {
                    OnServerDataReceived.Invoke(FromBaseId(locali, baseConnectionId), data);
                });

                transport.OnServerError.AddListener((baseConnectionId, error) =>
                {
                    OnServerError.Invoke(FromBaseId(locali, baseConnectionId), error);
                });
                transport.OnServerDisconnected.AddListener(baseConnectionId =>
                {
                    OnServerDisconnected.Invoke(FromBaseId(locali, baseConnectionId));
                });
            }
        }

        public override bool ServerActive()
        {
            return transports.All(t => t.ServerActive());
        }

        public override string ServerGetClientAddress(int connectionId)
        {
            int baseConnectionId = ToBaseId(connectionId);
            int transportId = ToTransportId(connectionId);
            return transports[transportId].ServerGetClientAddress(baseConnectionId);
        }

        public override bool ServerDisconnect(int connectionId)
        {
            int baseConnectionId = ToBaseId(connectionId);
            int transportId = ToTransportId(connectionId);
            return transports[transportId].ServerDisconnect(baseConnectionId);
        }

        public override void ServerSend<T>(IList<int> recipients, int channelId, T msg)
        {
            // the message may be for different transports,
            // partition the recipients by transport and update the base

            foreach (List<int> list in recipientsCache)
            {
                list.Clear();
            }

            foreach (int connectionId in recipients)
            {
                int baseConnectionId = ToBaseId(connectionId);
                int transportId = ToTransportId(connectionId);
                recipientsCache[transportId].Add(baseConnectionId);
            }

            for (int i=0; i< transports.Length; i++)
            {
                List<int> baseRecipiens = recipientsCache[i];
                if (baseRecipiens.Count > 0)
                {
                    transports[i].ServerSend(baseRecipiens, channelId, msg);
                }
            }
        }

        public override void ServerStart()
        {
            foreach (Transport transport in transports)
            {
                transport.ServerStart();
            }
        }

        public override void ServerStop()
        {
            foreach (Transport transport in transports)
            {
                transport.ServerStop();
            }
        }
        #endregion

        public override void Shutdown()
        {
            foreach (Transport transport in transports)
            {
                transport.Shutdown();
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (Transport transport in transports)
            {
                builder.AppendLine(transport.ToString());
            }
            return builder.ToString().Trim();
        }
    }
}
