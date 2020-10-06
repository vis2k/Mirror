using System;
using UnityEngine;

namespace Mirror
{
    [Obsolete("Implement INetworkMessage instead. Use extension methods instead of Serialize/Deserialize, see https://github.com/vis2k/Mirror/pull/2317", true)]
    public interface IMessageBase { }

    [Obsolete("Implement INetworkMessage instead. Use extension methods instead of Serialize/Deserialize, see https://github.com/vis2k/Mirror/pull/2317", true)]
    public class MessageBase : IMessageBase { }

    public interface INetworkMessage
    {
    }

    #region Public System Messages
    public struct ErrorMessage : INetworkMessage
    {
        public byte value;

        public ErrorMessage(byte v)
        {
            value = v;
        }
    }

    public struct ReadyMessage : INetworkMessage
    {
    }

    public struct NotReadyMessage : INetworkMessage
    {
    }

    public struct AddPlayerMessage : INetworkMessage
    {
    }

    // Deprecated 5/2/2020
    /// <summary>
    /// Obsolete: Removed as a security risk. Use <see cref="NetworkServer.RemovePlayerForConnection(NetworkConnection, bool)">NetworkServer.RemovePlayerForConnection</see> instead.
    /// </summary>
    [Obsolete("Removed as a security risk. Use NetworkServer.RemovePlayerForConnection(NetworkConnection conn, bool keepAuthority = false) instead")]
    public struct RemovePlayerMessage : INetworkMessage
    {
    }

    public struct DisconnectMessage : INetworkMessage
    {
    }

    public struct ConnectMessage : INetworkMessage
    {
    }

    public struct SceneMessage : INetworkMessage
    {
        public string sceneName;
        // Normal = 0, LoadAdditive = 1, UnloadAdditive = 2
        public SceneOperation sceneOperation;
        public bool customHandling;
    }

    public enum SceneOperation : byte
    {
        Normal,
        LoadAdditive,
        UnloadAdditive
    }

    #endregion

    #region System Messages requried for code gen path
    public struct CommandMessage : INetworkMessage
    {
        public uint netId;
        public int componentIndex;
        public int functionHash;
        // the parameters for the Cmd function
        // -> ArraySegment to avoid unnecessary allocations
        public ArraySegment<byte> payload;
    }

    public struct RpcMessage : INetworkMessage
    {
        public uint netId;
        public int componentIndex;
        public int functionHash;
        // the parameters for the Cmd function
        // -> ArraySegment to avoid unnecessary allocations
        public ArraySegment<byte> payload;
    }
    #endregion

    #region Internal System Messages
    public struct SpawnMessage : INetworkMessage
    {
        /// <summary>
        /// netId of new or existing object
        /// </summary>
        public uint netId;
        /// <summary>
        /// Is the spawning object the local player. Sets ClientScene.localPlayer
        /// </summary>
        public bool isLocalPlayer;
        /// <summary>
        /// Sets hasAuthority on the spawned object
        /// </summary>
        public bool isOwner;
        /// <summary>
        /// The id of the scene object to spawn
        /// </summary>
        public ulong sceneId;
        /// <summary>
        /// The id of the prefab to spawn
        /// <para>If sceneId != 0 then it is used instead of assetId</para>
        /// </summary>
        public Guid assetId;
        /// <summary>
        /// Local position
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// Local rotation
        /// </summary>
        public Quaternion rotation;
        /// <summary>
        /// Local scale
        /// </summary>
        public Vector3 scale;
        /// <summary>
        /// The serialized component data
        /// <remark>ArraySegment to avoid unnecessary allocations</remark>
        /// </summary>
        public ArraySegment<byte> payload;
    }

    public struct ObjectSpawnStartedMessage : INetworkMessage
    {
    }

    public struct ObjectSpawnFinishedMessage : INetworkMessage
    {
    }

    public struct ObjectDestroyMessage : INetworkMessage
    {
        public uint netId;
    }

    public struct ObjectHideMessage : INetworkMessage
    {
        public uint netId;
    }

    public struct UpdateVarsMessage : INetworkMessage
    {
        public uint netId;
        // the serialized component data
        // -> ArraySegment to avoid unnecessary allocations
        public ArraySegment<byte> payload;
    }

    // A client sends this message to the server
    // to calculate RTT and synchronize time
    public struct NetworkPingMessage : INetworkMessage
    {
        public double clientTime;

        public NetworkPingMessage(double value)
        {
            clientTime = value;
        }
    }

    // The server responds with this message
    // The client can use this to calculate RTT and sync time
    public struct NetworkPongMessage : INetworkMessage
    {
        public double clientTime;
        public double serverTime;
    }
    #endregion
}
