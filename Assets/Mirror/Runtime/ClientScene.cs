using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mirror
{
    public static class ClientScene
    {
        static bool s_IsSpawnFinished;

        static List<uint> s_PendingOwnerNetIds = new List<uint>();

        public static NetworkIdentity localPlayer { get; private set; }
        public static bool ready { get; internal set; }
        public static NetworkConnection readyConnection { get; private set; }

        public static Dictionary<byte[], GameObject> prefabs = new Dictionary<byte[], GameObject>(new GuidComparer());
        // scene id to NetworkIdentity
        public static Dictionary<uint, NetworkIdentity> spawnableObjects;

        // spawn handlers
        internal static Dictionary<byte[], SpawnDelegate> spawnHandlers = new Dictionary<byte[], SpawnDelegate>(new GuidComparer());
        internal static Dictionary<byte[], UnSpawnDelegate> unspawnHandlers = new Dictionary<byte[], UnSpawnDelegate>(new GuidComparer());

        internal static void Shutdown()
        {
            NetworkIdentity.spawned.Clear();
            ClearSpawners();
            s_PendingOwnerNetIds.Clear();
            spawnableObjects = null;
            readyConnection = null;
            ready = false;
            s_IsSpawnFinished = false;

            Transport.activeTransport.ClientDisconnect();
        }

        // this is called from message handler for Owner message
        internal static void InternalAddPlayer(NetworkIdentity identity)
        {
            if (LogFilter.Debug) { Debug.LogWarning("ClientScene::InternalAddPlayer"); }

            // NOTE: It can be "normal" when changing scenes for the player to be destroyed and recreated.
            // But, the player structures are not cleaned up, we'll just replace the old player
            localPlayer = identity;
            if (readyConnection == null)
            {
                Debug.LogWarning("No ready connection found for setting player controller during InternalAddPlayer");
            }
            else
            {
                readyConnection.SetPlayerController(identity);
            }
        }

        // use this if already ready
        public static bool AddPlayer() => AddPlayer(null);

        // use this to implicitly become ready
        public static bool AddPlayer(NetworkConnection readyConn) => AddPlayer(readyConn, null);

        // use this to implicitly become ready
        // -> extraMessage can contain character selection, etc.
        public static bool AddPlayer(NetworkConnection readyConn, MessageBase extraMessage)
        {
            // ensure valid ready connection
            if (readyConn != null)
            {
                ready = true;
                readyConnection = readyConn;
            }

            if (!ready)
            {
                Debug.LogError("Must call AddPlayer() with a connection the first time to become ready.");
                return false;
            }

            if (readyConnection.playerController != null)
            {
                Debug.LogError("ClientScene::AddPlayer: a PlayerController was already added. Did you call AddPlayer twice?");
                return false;
            }

            if (LogFilter.Debug) { Debug.Log("ClientScene::AddPlayer() called with connection [" + readyConnection + "]"); }

            AddPlayerMessage msg = new AddPlayerMessage();
            if (extraMessage != null)
            {
                NetworkWriter writer = new NetworkWriter();
                extraMessage.Serialize(writer);
                msg.value = writer.ToArray();
            }
            readyConnection.Send((short)MsgType.AddPlayer, msg);
            return true;
        }

        public static bool RemovePlayer()
        {
            if (LogFilter.Debug) { Debug.Log("ClientScene::RemovePlayer() called with connection [" + readyConnection + "]"); }

            if (readyConnection.playerController != null)
            {
                RemovePlayerMessage msg = new RemovePlayerMessage();
                readyConnection.Send((short)MsgType.RemovePlayer, msg);

                Object.Destroy(readyConnection.playerController.gameObject);

                readyConnection.RemovePlayerController();
                localPlayer = null;

                return true;
            }
            return false;
        }

        public static bool Ready(NetworkConnection conn)
        {
            if (ready)
            {
                Debug.LogError("A connection has already been set as ready. There can only be one.");
                return false;
            }

            if (LogFilter.Debug) { Debug.Log("ClientScene::Ready() called with connection [" + conn + "]"); }

            if (conn != null)
            {
                ReadyMessage msg = new ReadyMessage();
                conn.Send((short)MsgType.Ready, msg);
                ready = true;
                readyConnection = conn;
                readyConnection.isReady = true;
                return true;
            }
            Debug.LogError("Ready() called with invalid connection object: conn=null");
            return false;
        }

        public static NetworkClient ConnectLocalServer()
        {
            LocalClient newClient = new LocalClient();
            NetworkServer.ActivateLocalClientScene();
            newClient.InternalConnectLocalServer();
            return newClient;
        }

        internal static void HandleClientDisconnect(NetworkConnection conn)
        {
            if (readyConnection == conn && ready)
            {
                ready = false;
                readyConnection = null;
            }
        }

        internal static bool ConsiderForSpawning(NetworkIdentity identity)
        {
            // not spawned yet, not hidden, etc.?
            return !identity.gameObject.activeSelf &&
                   identity.gameObject.hideFlags != HideFlags.NotEditable &&
                   identity.gameObject.hideFlags != HideFlags.HideAndDontSave &&
                   identity.sceneId != 0;
        }

        // this needs to be public. If users load/unload a scene in the client after connection
        // they should call this to register the spawnable objects
        public static void PrepareToSpawnSceneObjects()
        {
            // add all unspawned NetworkIdentities to spawnable objects
            spawnableObjects = Resources.FindObjectsOfTypeAll<NetworkIdentity>()
                               .Where(ConsiderForSpawning)
                               .ToDictionary(identity => identity.sceneId, identity => identity);
        }

        internal static NetworkIdentity SpawnSceneObject(uint sceneId)
        {
            if (spawnableObjects.TryGetValue(sceneId, out NetworkIdentity identity))
            {
                spawnableObjects.Remove(sceneId);
                return identity;
            }
            else
            {
                Debug.LogWarning("Could not find scene object with sceneid:" + sceneId);
            }
            return null;
        }

        internal static void RegisterSystemHandlers(NetworkClient client, bool localClient)
        {
            // local client / regular client react to some messages differently.
            // but we still need to add handlers for all of them to avoid
            // 'message id not found' errors.
            if (localClient)
            {
                client.RegisterHandler(MsgType.LocalClientAuthority, OnClientAuthority);
                client.RegisterHandler(MsgType.ObjectDestroy, OnLocalClientObjectDestroy);
                client.RegisterHandler(MsgType.ObjectHide, OnLocalClientObjectHide);
                client.RegisterHandler(MsgType.Owner, (msg) => {});
                client.RegisterHandler(MsgType.Pong, (msg) => {});
                client.RegisterHandler(MsgType.SpawnPrefab, OnLocalClientSpawnPrefab);
                client.RegisterHandler(MsgType.SpawnSceneObject, OnLocalClientSpawnSceneObject);
                client.RegisterHandler(MsgType.SpawnStarted, (msg) => {});
                client.RegisterHandler(MsgType.SpawnFinished, (msg) => {});
                client.RegisterHandler(MsgType.UpdateVars, (msg) => {});
            }
            else
            {
                client.RegisterHandler(MsgType.LocalClientAuthority, OnClientAuthority);
                client.RegisterHandler(MsgType.ObjectDestroy, OnObjectDestroy);
                client.RegisterHandler(MsgType.ObjectHide, OnObjectDestroy);
                client.RegisterHandler(MsgType.Owner, OnOwnerMessage);
                client.RegisterHandler(MsgType.Pong, NetworkTime.OnClientPong);
                client.RegisterHandler(MsgType.SpawnPrefab, OnSpawnPrefab);
                client.RegisterHandler(MsgType.SpawnSceneObject, OnSpawnSceneObject);
                client.RegisterHandler(MsgType.SpawnStarted, OnObjectSpawnStarted);
                client.RegisterHandler(MsgType.SpawnFinished, OnObjectSpawnFinished);
                client.RegisterHandler(MsgType.UpdateVars, OnUpdateVarsMessage);
            }

            client.RegisterHandler(MsgType.Rpc, OnRPCMessage);
            client.RegisterHandler(MsgType.SyncEvent, OnSyncEventMessage);
        }

        // spawn handlers and prefabs //////////////////////////////////////////
        internal static bool GetPrefab(byte[] assetId, out GameObject prefab)
        {
            prefab = null;
            return assetId != null &&
                   prefabs.TryGetValue(assetId, out prefab) && prefab != null;
        }

        // this assigns the newAssetId to the prefab. This is for registering dynamically created game objects for already know assetIds.
        public static void RegisterPrefab(GameObject prefab, byte[] newAssetId)
        {
            NetworkIdentity identity = prefab.GetComponent<NetworkIdentity>();
            if (identity)
            {
                identity.assetId = newAssetId;

                if (LogFilter.Debug) { Debug.Log("Registering prefab '" + prefab.name + "' as asset:" + identity.assetId); }
                prefabs[identity.assetId] = prefab;
            }
            else
            {
                Debug.LogError("Could not register '" + prefab.name + "' since it contains no NetworkIdentity component");
            }
        }

        public static void RegisterPrefab(GameObject prefab)
        {
            NetworkIdentity identity = prefab.GetComponent<NetworkIdentity>();
            if (identity)
            {
                if (LogFilter.Debug) { Debug.Log("Registering prefab '" + prefab.name + "' as asset:" + identity.assetId); }
                prefabs[identity.assetId] = prefab;

                NetworkIdentity[] identities = prefab.GetComponentsInChildren<NetworkIdentity>();
                if (identities.Length > 1)
                {
                    Debug.LogWarning("The prefab '" + prefab.name +
                                     "' has multiple NetworkIdentity components. There can only be one NetworkIdentity on a prefab, and it must be on the root object.");
                }
            }
            else
            {
                Debug.LogError("Could not register '" + prefab.name + "' since it contains no NetworkIdentity component");
            }
        }

        public static void RegisterPrefab(GameObject prefab, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            NetworkIdentity identity = prefab.GetComponent<NetworkIdentity>();
            if (identity == null)
            {
                Debug.LogError("Could not register '" + prefab.name + "' since it contains no NetworkIdentity component");
                return;
            }

            if (spawnHandler == null || unspawnHandler == null)
            {
                Debug.LogError("RegisterPrefab custom spawn function null for " + identity.assetId);
                return;
            }

            if (identity.assetId == null)
            {
                Debug.LogError("RegisterPrefab game object " + prefab.name + " has no prefab. Use RegisterSpawnHandler() instead?");
                return;
            }

            if (LogFilter.Debug) { Debug.Log("Registering custom prefab '" + prefab.name + "' as asset:" + identity.assetId + " " + spawnHandler.GetMethodName() + "/" + unspawnHandler.GetMethodName()); }

            spawnHandlers[identity.assetId] = spawnHandler;
            unspawnHandlers[identity.assetId] = unspawnHandler;
        }

        public static void UnregisterPrefab(GameObject prefab)
        {
            NetworkIdentity identity = prefab.GetComponent<NetworkIdentity>();
            if (identity == null)
            {
                Debug.LogError("Could not unregister '" + prefab.name + "' since it contains no NetworkIdentity component");
                return;
            }
            spawnHandlers.Remove(identity.assetId);
            unspawnHandlers.Remove(identity.assetId);
        }

        public static void RegisterSpawnHandler(byte[] assetId, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            if (spawnHandler == null || unspawnHandler == null)
            {
                Debug.LogError("RegisterSpawnHandler custom spawn function null for " + assetId);
                return;
            }

            if (LogFilter.Debug) { Debug.Log("RegisterSpawnHandler asset '" + assetId + "' " + spawnHandler.GetMethodName() + "/" + unspawnHandler.GetMethodName()); }

            spawnHandlers[assetId] = spawnHandler;
            unspawnHandlers[assetId] = unspawnHandler;
        }

        public static void UnregisterSpawnHandler(byte[] assetId)
        {
            spawnHandlers.Remove(assetId);
            unspawnHandlers.Remove(assetId);
        }

        public static void ClearSpawners()
        {
            prefabs.Clear();
            spawnHandlers.Clear();
            unspawnHandlers.Clear();
        }

        internal static bool InvokeUnSpawnHandler(byte[] assetId, GameObject obj)
        {
            if (unspawnHandlers.TryGetValue(assetId, out UnSpawnDelegate handler) && handler != null)
            {
                handler(obj);
                return true;
            }
            return false;
        }

        public static void DestroyAllClientObjects()
        {
            foreach (KeyValuePair<uint, NetworkIdentity> kvp in NetworkIdentity.spawned)
            {
                NetworkIdentity identity = kvp.Value;
                if (identity != null && identity.gameObject != null)
                {
                    if (!InvokeUnSpawnHandler(identity.assetId, identity.gameObject))
                    {
                        if (identity.sceneId == 0)
                        {
                            Object.Destroy(identity.gameObject);
                        }
                        else
                        {
                            identity.MarkForReset();
                            identity.gameObject.SetActive(false);
                        }
                    }
                }
            }
            NetworkIdentity.spawned.Clear();
        }

        [Obsolete("Use NetworkIdentity.spawned[netId] instead.")]
        public static GameObject FindLocalObject(uint netId)
        {
            if (NetworkIdentity.spawned.TryGetValue(netId, out NetworkIdentity identity))
            {
                return identity.gameObject;
            }
            return null;
        }

        static void ApplySpawnPayload(NetworkIdentity identity, Vector3 position, Quaternion rotation, byte[] payload, uint netId)
        {
            if (!identity.gameObject.activeSelf)
            {
                identity.gameObject.SetActive(true);
            }
            identity.transform.position = position;
            identity.transform.rotation = rotation;
            if (payload != null && payload.Length > 0)
            {
                NetworkReader payloadReader = new NetworkReader(payload);
                identity.OnUpdateVars(payloadReader, true);
            }

            identity.SetNetworkInstanceId(netId);
            NetworkIdentity.spawned[netId] = identity;

            // objects spawned as part of initial state are started on a second pass
            if (s_IsSpawnFinished)
            {
                identity.isClient = true;
                identity.OnStartClient();
                CheckForOwner(identity);
            }
        }

        static void OnSpawnPrefab(NetworkMessage netMsg)
        {
            SpawnPrefabMessage msg = netMsg.ReadMessage<SpawnPrefabMessage>();

            if (msg.assetId == null)
            {
                Debug.LogError("OnObjSpawn netId: " + msg.netId + " has invalid asset Id");
                return;
            }
            if (LogFilter.Debug) { Debug.Log("Client spawn handler instantiating [netId:" + msg.netId + " asset ID:" + msg.assetId + " pos:" + msg.position + "]"); }

            if (NetworkIdentity.spawned.TryGetValue(msg.netId, out NetworkIdentity localObject) && localObject != null)
            {
                // this object already exists (was in the scene), just apply the update to existing object
                localObject.Reset();
                ApplySpawnPayload(localObject, msg.position, msg.rotation, msg.payload, msg.netId);
                return;
            }

            if (GetPrefab(msg.assetId, out GameObject prefab))
            {
                GameObject obj = Object.Instantiate(prefab, msg.position, msg.rotation);
                if (LogFilter.Debug)
                {
                    Debug.Log("Client spawn handler instantiating [netId:" + msg.netId + " asset ID:" + msg.assetId + " pos:" + msg.position + " rotation: " + msg.rotation + "]");
                }

                localObject = obj.GetComponent<NetworkIdentity>();
                if (localObject == null)
                {
                    Debug.LogError("Client object spawned for " + msg.assetId + " does not have a NetworkIdentity");
                    return;
                }
                localObject.Reset();
                ApplySpawnPayload(localObject, msg.position, msg.rotation, msg.payload, msg.netId);
            }
            // lookup registered factory for type:
            else if (spawnHandlers.TryGetValue(msg.assetId, out SpawnDelegate handler))
            {
                GameObject obj = handler(msg.position, msg.assetId);
                if (obj == null)
                {
                    Debug.LogWarning("Client spawn handler for " + msg.assetId + " returned null");
                    return;
                }
                localObject = obj.GetComponent<NetworkIdentity>();
                if (localObject == null)
                {
                    Debug.LogError("Client object spawned for " + msg.assetId + " does not have a network identity");
                    return;
                }
                localObject.Reset();
                localObject.assetId = msg.assetId;
                ApplySpawnPayload(localObject, msg.position, msg.rotation, msg.payload, msg.netId);
            }
            else
            {
                Debug.LogError("Failed to spawn server object, did you forget to add it to the NetworkManager? assetId=" + msg.assetId + " netId=" + msg.netId);
            }
        }

        static void OnSpawnSceneObject(NetworkMessage netMsg)
        {
            SpawnSceneObjectMessage msg = netMsg.ReadMessage<SpawnSceneObjectMessage>();

            if (LogFilter.Debug) { Debug.Log("Client spawn scene handler instantiating [netId:" + msg.netId + " sceneId:" + msg.sceneId + " pos:" + msg.position); }

            if (NetworkIdentity.spawned.TryGetValue(msg.netId, out NetworkIdentity localObject) && localObject != null)
            {
                // this object already exists (was in the scene)
                localObject.Reset();
                ApplySpawnPayload(localObject, msg.position, msg.rotation, msg.payload, msg.netId);
                return;
            }

            NetworkIdentity spawnedId = SpawnSceneObject(msg.sceneId);
            if (spawnedId == null)
            {
                Debug.LogError("Spawn scene object not found for " + msg.sceneId + " SpawnableObjects.Count=" + spawnableObjects.Count);
                // dump the whole spawnable objects dict for easier debugging
                foreach (KeyValuePair<uint, NetworkIdentity> kvp in spawnableObjects)
                    Debug.Log("Spawnable: SceneId=" + kvp.Key + " name=" + kvp.Value.name);
                return;
            }

            if (LogFilter.Debug) { Debug.Log("Client spawn for [netId:" + msg.netId + "] [sceneId:" + msg.sceneId + "] obj:" + spawnedId.gameObject.name); }
            spawnedId.Reset();
            ApplySpawnPayload(spawnedId, msg.position, msg.rotation, msg.payload, msg.netId);
        }

        static void OnObjectSpawnStarted(NetworkMessage netMsg)
        {
            if (LogFilter.Debug) { Debug.Log("SpawnStarted"); }

            PrepareToSpawnSceneObjects();
            s_IsSpawnFinished = false;
        }

        static void OnObjectSpawnFinished(NetworkMessage netMsg)
        {
            if (LogFilter.Debug) { Debug.Log("SpawnFinished"); }

            // paul: Initialize the objects in the same order as they were initialized
            // in the server.   This is important if spawned objects
            // use data from scene objects
            foreach (NetworkIdentity identity in NetworkIdentity.spawned.Values.OrderBy(uv => uv.netId))
            {
                if (!identity.isClient)
                {
                    identity.OnStartClient();
                    CheckForOwner(identity);
                }
            }
            s_IsSpawnFinished = true;
        }

        static void OnObjectDestroy(NetworkMessage netMsg)
        {
            ObjectDestroyMessage msg = netMsg.ReadMessage<ObjectDestroyMessage>();
            if (LogFilter.Debug) { Debug.Log("ClientScene::OnObjDestroy netId:" + msg.netId); }

            if (NetworkIdentity.spawned.TryGetValue(msg.netId, out NetworkIdentity localObject) && localObject != null)
            {
                localObject.OnNetworkDestroy();

                if (!InvokeUnSpawnHandler(localObject.assetId, localObject.gameObject))
                {
                    // default handling
                    if (localObject.sceneId == 0)
                    {
                        Object.Destroy(localObject.gameObject);
                    }
                    else
                    {
                        // scene object.. disable it in scene instead of destroying
                        localObject.gameObject.SetActive(false);
                        spawnableObjects[localObject.sceneId] = localObject;
                    }
                }
                NetworkIdentity.spawned.Remove(msg.netId);
                localObject.MarkForReset();
            }
            else
            {
                if (LogFilter.Debug) { Debug.LogWarning("Did not find target for destroy message for " + msg.netId); }
            }
        }

        static void OnLocalClientObjectDestroy(NetworkMessage netMsg)
        {
            ObjectDestroyMessage msg = netMsg.ReadMessage<ObjectDestroyMessage>();
            if (LogFilter.Debug) { Debug.Log("ClientScene::OnLocalObjectObjDestroy netId:" + msg.netId); }

            NetworkIdentity.spawned.Remove(msg.netId);
        }

        static void OnLocalClientObjectHide(NetworkMessage netMsg)
        {
            ObjectDestroyMessage msg = netMsg.ReadMessage<ObjectDestroyMessage>();
            if (LogFilter.Debug) { Debug.Log("ClientScene::OnLocalObjectObjHide netId:" + msg.netId); }

            if (NetworkIdentity.spawned.TryGetValue(msg.netId, out NetworkIdentity localObject) && localObject != null)
            {
                localObject.OnSetLocalVisibility(false);
            }
        }

        static void OnLocalClientSpawnPrefab(NetworkMessage netMsg)
        {
            SpawnPrefabMessage msg = netMsg.ReadMessage<SpawnPrefabMessage>();

            if (NetworkIdentity.spawned.TryGetValue(msg.netId, out NetworkIdentity localObject) && localObject != null)
            {
                localObject.OnSetLocalVisibility(true);
            }
        }

        static void OnLocalClientSpawnSceneObject(NetworkMessage netMsg)
        {
            SpawnSceneObjectMessage msg = netMsg.ReadMessage<SpawnSceneObjectMessage>();

            if (NetworkIdentity.spawned.TryGetValue(msg.netId, out NetworkIdentity localObject) && localObject != null)
            {
                localObject.OnSetLocalVisibility(true);
            }
        }

        static void OnUpdateVarsMessage(NetworkMessage netMsg)
        {
            UpdateVarsMessage msg = netMsg.ReadMessage<UpdateVarsMessage>();

            if (LogFilter.Debug) { Debug.Log("ClientScene::OnUpdateVarsMessage " + msg.netId); }

            if (NetworkIdentity.spawned.TryGetValue(msg.netId, out NetworkIdentity localObject) && localObject != null)
            {
                localObject.OnUpdateVars(new NetworkReader(msg.payload), false);
            }
            else
            {
                Debug.LogWarning("Did not find target for sync message for " + msg.netId + " . Note: this can be completely normal because UDP messages may arrive out of order, so this message might have arrived after a Destroy message.");
            }
        }

        static void OnRPCMessage(NetworkMessage netMsg)
        {
            RpcMessage msg = netMsg.ReadMessage<RpcMessage>();

            if (LogFilter.Debug) { Debug.Log("ClientScene::OnRPCMessage hash:" + msg.functionHash + " netId:" + msg.netId); }

            if (NetworkIdentity.spawned.TryGetValue(msg.netId, out NetworkIdentity identity))
            {
                identity.HandleRPC(msg.componentIndex, msg.functionHash, new NetworkReader(msg.payload));
            }
        }

        static void OnSyncEventMessage(NetworkMessage netMsg)
        {
            SyncEventMessage msg = netMsg.ReadMessage<SyncEventMessage>();

            if (LogFilter.Debug) { Debug.Log("ClientScene::OnSyncEventMessage " + msg.netId); }

            if (NetworkIdentity.spawned.TryGetValue(msg.netId, out NetworkIdentity identity))
            {
                identity.HandleSyncEvent(msg.componentIndex, msg.functionHash, new NetworkReader(msg.payload));
            }
            else
            {
                Debug.LogWarning("Did not find target for SyncEvent message for " + msg.netId);
            }
        }

        static void OnClientAuthority(NetworkMessage netMsg)
        {
            ClientAuthorityMessage msg = netMsg.ReadMessage<ClientAuthorityMessage>();

            if (LogFilter.Debug) { Debug.Log("ClientScene::OnClientAuthority for  connectionId=" + netMsg.conn.connectionId + " netId: " + msg.netId); }

            if (NetworkIdentity.spawned.TryGetValue(msg.netId, out NetworkIdentity identity))
            {
                identity.HandleClientAuthority(msg.authority);
            }
        }

        // OnClientAddedPlayer?
        static void OnOwnerMessage(NetworkMessage netMsg)
        {
            OwnerMessage msg = netMsg.ReadMessage<OwnerMessage>();

            if (LogFilter.Debug) { Debug.Log("ClientScene::OnOwnerMessage - connectionId=" + netMsg.conn.connectionId + " netId: " + msg.netId); }

            // is there already an owner that is a different object??
            netMsg.conn.playerController?.SetNotLocalPlayer();

            if (NetworkIdentity.spawned.TryGetValue(msg.netId, out NetworkIdentity localObject) && localObject != null)
            {
                // this object already exists
                localObject.connectionToServer = netMsg.conn;
                localObject.SetLocalPlayer();
                InternalAddPlayer(localObject);
            }
            else
            {
                s_PendingOwnerNetIds.Add(msg.netId);
            }
        }

        static void CheckForOwner(NetworkIdentity identity)
        {
            for (int i = 0; i < s_PendingOwnerNetIds.Count; i++)
            {
                uint pendingOwnerNetId = s_PendingOwnerNetIds[i];
                if (pendingOwnerNetId == identity.netId)
                {
                    // found owner, turn into a local player

                    // Set isLocalPlayer to true on this NetworkIdentity and trigger OnStartLocalPlayer in all scripts on the same GO
                    identity.connectionToServer = readyConnection;
                    identity.SetLocalPlayer();

                    if (LogFilter.Debug) { Debug.Log("ClientScene::OnOwnerMessage - player=" + identity.name); }
                    if (readyConnection.connectionId < 0)
                    {
                        Debug.LogError("Owner message received on a local client.");
                        return;
                    }
                    InternalAddPlayer(identity);

                    s_PendingOwnerNetIds.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
