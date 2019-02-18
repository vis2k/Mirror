using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
    public class NetworkClient
    {
        // the client (can be a regular NetworkClient or a LocalClient)
        public static NetworkClient singleton;

        [Obsolete("Use NetworkClient.singleton instead. There is always exactly one client.")]
        public static List<NetworkClient> allClients => new List<NetworkClient>{singleton};

        public readonly Dictionary<short, NetworkMessageDelegate> handlers = new Dictionary<short, NetworkMessageDelegate>();

        public NetworkConnection connection { get; protected set; }

        protected enum ConnectState
        {
            None,
            Connecting,
            Connected,
            Disconnected
        }
        protected ConnectState connectState = ConnectState.None;

        public string serverIp { get; private set; } = "";

        // active is true while a client is connecting/connected
        // (= while the network is active)
        public static bool active { get; protected set; }

        public bool isConnected => connectState == ConnectState.Connected;

        public NetworkClient()
        {
            if (LogFilter.Debug) { Debug.Log("Client created version " + Version.Current); }

            if (singleton != null)
            {
                Debug.LogError("NetworkClient: can only create one!");
                return;
            }
            singleton = this;
        }

        internal void SetHandlers(NetworkConnection conn)
        {
            conn.SetHandlers(handlers);
        }

        public void Connect(string ip)
        {
            if (LogFilter.Debug) { Debug.Log("Client Connect: " + ip); }

            active = true;
            RegisterSystemHandlers(false);
            Transport.activeTransport.enabled = true;
            InitializeTransportHandlers();

            serverIp = ip;

            connectState = ConnectState.Connecting;
            Transport.activeTransport.ClientConnect(ip);

            // setup all the handlers
            connection = new NetworkConnection(serverIp, 0);
            connection.SetHandlers(handlers);
        }

        private void InitializeTransportHandlers()
        {
            Transport.activeTransport.OnClientConnected.AddListener(OnConnected);
            Transport.activeTransport.OnClientDataReceived.AddListener(OnDataReceived);
            Transport.activeTransport.OnClientDisconnected.AddListener(OnDisconnected);
            Transport.activeTransport.OnClientError.AddListener(OnError);
        }

        void OnError(Exception exception)
        {
            Debug.LogException(exception);
        }

        void OnDisconnected()
        {
            connectState = ConnectState.Disconnected;

            ClientScene.HandleClientDisconnect(connection);

            connection?.InvokeHandlerNoData((short)MsgType.Disconnect);
        }

        protected void OnDataReceived(byte[] data)
        {
            if (connection != null)
            {
                connection.TransportReceive(data);
            }
            else Debug.LogError("Skipped Data message handling because m_Connection is null.");
        }

        void OnConnected()
        {
            if (connection != null)
            {
                // reset network time stats
                NetworkTime.Reset();

                // the handler may want to send messages to the client
                // thus we should set the connected state before calling the handler
                connectState = ConnectState.Connected;
                NetworkTime.UpdateClient(this);
                connection.InvokeHandlerNoData((short)MsgType.Connect);
            }
            else Debug.LogError("Skipped Connect message handling because m_Connection is null.");
        }

        public virtual void Disconnect()
        {
            connectState = ConnectState.Disconnected;
            ClientScene.HandleClientDisconnect(connection);
            if (connection != null)
            {
                connection.Disconnect();
                connection.Dispose();
                connection = null;
                RemoveTransportHandlers();
            }

            // the client's network is not active anymore.
            active = false;
        }

        void RemoveTransportHandlers()
        {
            // so that we don't register them more than once
            Transport.activeTransport.OnClientConnected.RemoveListener(OnConnected);
            Transport.activeTransport.OnClientDataReceived.RemoveListener(OnDataReceived);
            Transport.activeTransport.OnClientDisconnected.RemoveListener(OnDisconnected);
            Transport.activeTransport.OnClientError.RemoveListener(OnError);
        }

        [Obsolete("Use SendMessage<T> instead with no message id instead")]
        public bool Send(short msgType, MessageBase msg)
        {
            if (connection != null)
            {
                if (connectState != ConnectState.Connected)
                {
                    Debug.LogError("NetworkClient Send when not connected to a server");
                    return false;
                }
                return connection.Send(msgType, msg);
            }
            Debug.LogError("NetworkClient Send with no connection");
            return false;
        }

        public bool Send<T>(T message) where T : MessageBase
        {
            // TODO use int instead to avoid collisions
            if (connection != null)
            {
                if (connectState != ConnectState.Connected)
                {
                    Debug.LogError("NetworkClient Send when not connected to a server");
                    return false;
                }
                return connection.Send(message);
            }
            Debug.LogError("NetworkClient Send with no connection");
            return false;
        }

        internal virtual void Update()
        {
            // only update things while connected
            if (active && connectState == ConnectState.Connected)
            {
                NetworkTime.UpdateClient(this);
            }
        }

        /* TODO use or remove
        void GenerateConnectError(byte error)
        {
            Debug.LogError("UNet Client Error Connect Error: " + error);
            GenerateError(error);
        }

        void GenerateDataError(byte error)
        {
            NetworkError dataError = (NetworkError)error;
            Debug.LogError("UNet Client Data Error: " + dataError);
            GenerateError(error);
        }

        void GenerateDisconnectError(byte error)
        {
            NetworkError disconnectError = (NetworkError)error;
            Debug.LogError("UNet Client Disconnect Error: " + disconnectError);
            GenerateError(error);
        }

        void GenerateError(byte error)
        {
            if (handlers.TryGetValue((short)MsgType.Error, out NetworkMessageDelegate msgDelegate))
            {
                ErrorMessage msg = new ErrorMessage
                {
                    value = error
                };

                // write the message to a local buffer
                NetworkWriter writer = new NetworkWriter();
                msg.Serialize(writer);

                NetworkMessage netMsg = new NetworkMessage
                {
                    msgType = (short)MsgType.Error,
                    reader = new NetworkReader(writer.ToArray()),
                    conn = connection
                };
                msgDelegate(netMsg);
            }
        }
        */

        [Obsolete("Use NetworkTime.rtt instead")]
        public float GetRTT()
        {
            return (float)NetworkTime.rtt;
        }

        internal void RegisterSystemHandlers(bool localClient)
        {
            // local client / regular client react to some messages differently.
            // but we still need to add handlers for all of them to avoid
            // 'message id not found' errors.
            if (localClient)
            {
                RegisterHandler<ObjectDestroyMessage>(ClientScene.OnLocalClientObjectDestroy);
                RegisterHandler<ObjectHideMessage>(ClientScene.OnLocalClientObjectHide);
                RegisterHandler(MsgType.Owner, (msg) => {});
                RegisterHandler(MsgType.Pong, (msg) => {});
                RegisterHandler<SpawnPrefabMessage>(ClientScene.OnLocalClientSpawnPrefab);
                RegisterHandler<SpawnSceneObjectMessage>(ClientScene.OnLocalClientSpawnSceneObject);
                RegisterHandler<ObjectSpawnStartedMessage>((msg) => {});
                RegisterHandler<ObjectSpawnFinishedMessage>((msg) => {});
                RegisterHandler(MsgType.UpdateVars, (msg) => {});
            }
            else
            {
                RegisterHandler<ObjectDestroyMessage>(ClientScene.OnObjectDestroy);
                RegisterHandler<ObjectHideMessage>(ClientScene.OnObjectHide);
                RegisterHandler(MsgType.Owner, ClientScene.OnOwnerMessage);
                RegisterHandler(MsgType.Pong, NetworkTime.OnClientPong);
                RegisterHandler<SpawnPrefabMessage>(ClientScene.OnSpawnPrefab);
                RegisterHandler<SpawnSceneObjectMessage>(ClientScene.OnSpawnSceneObject);
                RegisterHandler<ObjectSpawnStartedMessage>(ClientScene.OnObjectSpawnStarted);
                RegisterHandler<ObjectSpawnFinishedMessage>(ClientScene.OnObjectSpawnFinished);
                RegisterHandler(MsgType.UpdateVars, ClientScene.OnUpdateVarsMessage);
            }
            RegisterHandler<ClientAuthorityMessage>(ClientScene.OnClientAuthority);
            RegisterHandler(MsgType.Rpc, ClientScene.OnRPCMessage);
            RegisterHandler(MsgType.SyncEvent, ClientScene.OnSyncEventMessage);
        }

        [Obsolete("Use RegisterHandler<T> instead")]
        public void RegisterHandler(short msgType, NetworkMessageDelegate handler)
        {
            if (handlers.ContainsKey(msgType))
            {
                if (LogFilter.Debug) { Debug.Log("NetworkClient.RegisterHandler replacing " + msgType); }
            }
            handlers[msgType] = handler;
        }

        [Obsolete("Use RegisterHandler<T> instead")]
        public void RegisterHandler(MsgType msgType, NetworkMessageDelegate handler)
        {
            RegisterHandler((short)msgType, handler);
        }

        public void RegisterHandler<T>(Action<T> handler) where T : MessageBase, new()
        {
            // TODO use int instead to avoid collisions
            short msgType = MessageBase.GetId<T>();
            if (handlers.ContainsKey(msgType))
            {
                if (LogFilter.Debug) { Debug.Log("NetworkClient.RegisterHandler replacing " + msgType); }
            }
            handlers[msgType] = (networkMessage) =>
            {
                handler(networkMessage.ReadMessage<T>());
            };
        }

        [Obsolete("Use UnregisterHandler<T> instead")]
        public void UnregisterHandler(short msgType)
        {
            handlers.Remove(msgType);
        }

        [Obsolete("Use UnregisterHandler<T> instead")]
        public void UnregisterHandler(MsgType msgType)
        {
            UnregisterHandler((short)msgType);
        }

        public void UnregisterHandler<T>() where T : MessageBase
        {
            // use int to minimize collisions
            short msgType = MessageBase.GetId<T>();
            handlers.Remove(msgType);
        }

        internal static void UpdateClient()
        {
            singleton?.Update();
        }

        public void Shutdown()
        {
            if (LogFilter.Debug) Debug.Log("Shutting down client.");
            singleton = null;
            active = false;
        }

        public static void ShutdownAll()
        {
            singleton?.Shutdown();
            singleton = null;
            active = false;
            ClientScene.Shutdown();
        }
    }
}
