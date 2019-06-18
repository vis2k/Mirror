using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Mirror
{
    // Handles network messages on client and server
    public delegate void NetworkMessageDelegate(NetworkMessage netMsg);

    // Handles requests to spawn objects on the client
    public delegate GameObject SpawnDelegate(Vector3 position, Guid assetId);

    // Handles requests to unspawn objects on the client
    public delegate void UnSpawnDelegate(GameObject spawned);

    // invoke type for Cmd/Rpc/SyncEvents
    public enum MirrorInvokeType
    {
        Command,
        ClientRpc,
        SyncEvent
    }

    // built-in system network messages
    // original HLAPI uses short, so let's keep short to not break packet header etc.
    // => use .ToString() to get the field name from the field value
    // => we specify the short values so it's easier to look up opcodes when debugging packets
    [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Use Send<T>  with no message id instead")]
    public enum MsgType : short
    {
        // internal system messages - cannot be replaced by user code
        ObjectDestroy = 1,
        Rpc = 2,
        Owner = 4,
        Command = 5,
        SyncEvent = 7,
        UpdateVars = 8,
        SpawnPrefab = 3,
        SpawnSceneObject = 10,
        SpawnStarted = 11,
        SpawnFinished = 12,
        ObjectHide = 13,
        LocalClientAuthority = 15,

        // public system messages - can be replaced by user code
        Connect = 32,
        Disconnect = 33,
        Error = 34,
        Ready = 35,
        NotReady = 36,
        AddPlayer = 37,
        RemovePlayer = 38,
        Scene = 39,

        // time synchronization
        Ping = 43,
        Pong = 44,

        Highest = 47
    }

    public enum Version
    {
        Current = 1
    }

    public static class Channels
    {
        public const int DefaultReliable = 0;
        public const int DefaultUnreliable = 1;
    }

    // -- helpers for float conversion --
    [StructLayout(LayoutKind.Explicit)]
    internal struct UIntFloat
    {
        [FieldOffset(0)]
        public float floatValue;

        [FieldOffset(0)]
        public uint intValue;

        [FieldOffset(0)]
        public double doubleValue;

        [FieldOffset(0)]
        public ulong longValue;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct UIntDecimal
    {
        [FieldOffset(0)]
        public uint intValue1;

        [FieldOffset(4)]
        public uint intValue2;

        [FieldOffset(8)]
        public uint intValue3;

        [FieldOffset(12)]
        public uint intValue4;

        [FieldOffset(0)]
        public decimal decimalValue;
    }

    internal class FloatConversion
    {
        public static float ToSingle(uint value)
        {
            UIntFloat uf = new UIntFloat();
            uf.intValue = value;
            return uf.floatValue;
        }

        public static double ToDouble(ulong value)
        {
            UIntFloat uf = new UIntFloat();
            uf.longValue = value;
            return uf.doubleValue;
        }

        public static decimal ToDecimal(uint value1, uint value2, uint value3, uint value4)
        {
            UIntDecimal uf = new UIntDecimal();
            uf.intValue1 = value1;
            uf.intValue2 = value2;
            uf.intValue3 = value3;
            uf.intValue4 = value4;
            return uf.decimalValue;
        }
    }
}
