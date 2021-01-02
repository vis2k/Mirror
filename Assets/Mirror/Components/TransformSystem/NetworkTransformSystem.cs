using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JamesFrowen.BitPacking;
using UnityEngine;

namespace Mirror.TransformSyncing
{
    public class NetworkTransformSystem : MonoBehaviour
    {
        // todo make this work with network Visibility
        // todo add maxMessageSize (splits up update message into multiple messages if too big)


        [Header("Reference")]
        [SerializeField] internal NetworkTransformSystemRuntimeReference runtime;

        [Header("Sync")]
        [Tooltip("How often 1 behaviour should update")]
        public float syncInterval = 0.1f;
        [Tooltip("Check if behaviours need update every frame, If false then checks every syncInterval")]
        public bool checkEveryFrame = true;

        [Header("Id Compression")]
        [SerializeField] int smallBitCount = 6;
        [SerializeField] int mediumBitCount = 12;
        [SerializeField] int largeBitCount = 18;


        [Header("Position Compression")]
        [SerializeField] Vector3 min = Vector3.one * -100;
        [SerializeField] Vector3 max = Vector3.one * 100;
        [SerializeField] float precision = 0.01f;

        [Header("Rotation Compression")]
        [SerializeField] int bitCount = 9;


        [Header("Debug And Gizmo")]
        [SerializeField] private bool drawGizmo;
        [SerializeField] private Color gizmoColor;
        [Tooltip("readonly")]
        [SerializeField] private int _bitCount;
        [SerializeField] private Vector3Int _bitCountAxis;
        [Tooltip("readonly")]
        [SerializeField] private int _byteCount;


        [NonSerialized] BitWriter bitWriter = new BitWriter();
        [NonSerialized] internal BitReader bitReader = new BitReader();
        [NonSerialized] internal UIntPackcer idPacker;
        [NonSerialized] internal PositionPacker positionPacker;
        [NonSerialized] internal QuaternionPacker rotationPacker;


        [NonSerialized] float nextSyncInterval;

        private void Awake()
        {
            idPacker = new UIntPackcer(smallBitCount, mediumBitCount, largeBitCount);
            positionPacker = new PositionPacker(min, max, precision);
            rotationPacker = new QuaternionPacker(bitCount);
        }

        private void OnEnable()
        {
            runtime.System = this;
        }
        private void OnDisable()
        {
            runtime.System = null;
        }

        public void RegisterHandlers()
        {
            if (NetworkClient.active)
            {
                NetworkClient.RegisterHandler<NetworkPositionMessage>(ClientHandleNetworkPositionMessage);
            }

            if (NetworkServer.active)
            {
                NetworkServer.RegisterHandler<NetworkPositionSingleMessage>(ServerHandleNetworkPositionMessage);
            }
        }

        public void UnregisterHandlers()
        {
            if (NetworkClient.active)
            {
                NetworkClient.UnregisterHandler<NetworkPositionMessage>();
            }

            if (NetworkServer.active)
            {
                NetworkServer.UnregisterHandler<NetworkPositionSingleMessage>();
            }
        }

        [ServerCallback]
        private void LateUpdate()
        {
            if (checkEveryFrame || ShouldSync())
            {
                SendUpdateToAll();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool ShouldSync()
        {
            float now = Time.time;
            if (now > nextSyncInterval)
            {
                nextSyncInterval += syncInterval;
                return true;
            }
            else
            {
                return false;
            }
        }

        internal void SendUpdateToAll()
        {
            // dont send message if no behaviours
            if (runtime.behaviours.Count == 0) { return; }

            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                PackBehaviours(writer);

                // dont send anything if nothing was written (eg, nothing dirty)
                if (writer.Length == 0) { return; }

                NetworkServer.SendToAll(new NetworkPositionMessage
                {
                    bytes = writer.ToArraySegment()
                });
            }
        }

        internal void PackBehaviours(PooledNetworkWriter netWriter)
        {
            bitWriter.Reset(netWriter);
            float now = Time.time;

            foreach (KeyValuePair<uint, IHasPositionRotation> kvp in runtime.behaviours)
            {
                IHasPositionRotation behaviour = kvp.Value;
                if (!behaviour.NeedsUpdate(now))
                    continue;

                uint id = kvp.Key;
                PositionRotation posRot = behaviour.PositionRotation;

                idPacker.Pack(bitWriter, id);
                positionPacker.Pack(bitWriter, posRot.position);
                rotationPacker.Pack(bitWriter, posRot.rotation);

                behaviour.ClearNeedsUpdate();
            }
            bitWriter.Flush();
        }

        internal void ClientHandleNetworkPositionMessage(NetworkConnection _conn, NetworkPositionMessage msg)
        {
            int count = msg.bytes.Count;
            using (PooledNetworkReader netReader = NetworkReaderPool.GetReader(msg.bytes))
            {
                bitReader.Reset(netReader);
                while (netReader.Position < count)
                {
                    uint id = idPacker.Unpack(bitReader);
                    Vector3 pos = positionPacker.Unpack(bitReader);
                    Quaternion rot = rotationPacker.Unpack(bitReader);

                    if (runtime.behaviours.TryGetValue(id, out IHasPositionRotation behaviour))
                    {
                        behaviour.ApplyOnClient(new PositionRotation(pos, rot));
                    }
                }
                Debug.Assert(netReader.Position == count, "should have read exact amount");
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendMessageToServer(IHasPositionRotation behaviour)
        {
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                bitWriter.Reset(writer);

                uint id = behaviour.Id;
                PositionRotation posRot = behaviour.PositionRotation;

                idPacker.Pack(bitWriter, id);
                positionPacker.Pack(bitWriter, posRot.position);
                rotationPacker.Pack(bitWriter, posRot.rotation);

                behaviour.ClearNeedsUpdate();

                bitWriter.Flush();

                // dont send anything if nothing was written (eg, nothing dirty)
                if (writer.Length == 0) { return; }

                NetworkServer.SendToAll(new NetworkPositionSingleMessage
                {
                    bytes = writer.ToArraySegment()
                });
            }
        }

        /// <summary>
        /// Position from client to server
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        internal void ServerHandleNetworkPositionMessage(NetworkConnection _conn, NetworkPositionSingleMessage msg)
        {
            using (PooledNetworkReader netReader = NetworkReaderPool.GetReader(msg.bytes))
            {
                bitReader.Reset(netReader);
                uint id = idPacker.Unpack(bitReader);
                Vector3 pos = positionPacker.Unpack(bitReader);
                Quaternion rot = rotationPacker.Unpack(bitReader);

                if (runtime.behaviours.TryGetValue(id, out IHasPositionRotation behaviour))
                {
                    behaviour.ApplyOnServer(new PositionRotation(pos, rot));
                }

                Debug.Assert(netReader.Position == msg.bytes.Count, "should have read exact amount");
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawGizmo) { return; }
            Gizmos.color = gizmoColor;
            Bounds bounds = default;
            bounds.min = min;
            bounds.max = max;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
#endif

    }

    public interface IHasPositionRotation
    {
        /// <summary>
        /// Position and rotation of object
        /// <para>Could be localposition or world position writer doesn't care</para>
        /// </summary>
        PositionRotation PositionRotation { get; }

        /// <summary>
        /// Normally NetId, but could be a 
        /// </summary>
        uint Id { get; }

        bool NeedsUpdate(float now);
        void ClearNeedsUpdate();

        /// <summary>
        /// Applies position and rotation on server
        /// </summary>
        /// <param name="values"></param>
        void ApplyOnServer(PositionRotation values);

        /// <summary>
        /// Applies position and rotation on server
        /// <para>this should apply interoperation so it looks smooth to the user</para>
        /// </summary>
        /// <param name="values"></param>
        void ApplyOnClient(PositionRotation values);
    }

    public struct PositionRotation
    {
        public readonly Vector3 position;

        public readonly Quaternion rotation;

        public PositionRotation(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }

    public struct NetworkPositionMessage : NetworkMessage
    {
        public ArraySegment<byte> bytes;
    }
    public struct NetworkPositionSingleMessage : NetworkMessage
    {
        public ArraySegment<byte> bytes;
    }
}