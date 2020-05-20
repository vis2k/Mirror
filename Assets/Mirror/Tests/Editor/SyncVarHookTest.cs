using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Mirror.Tests
{
    class HookBehaviour : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnValueChanged))]
        public int value = 0;

        public event Action<int, int> HookCalled;

        void OnValueChanged(int oldValue, int newValue)
        {
            HookCalled.Invoke(oldValue, newValue);
        }
    }

    class GameObjectHookBehaviour : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnValueChanged))]
        public GameObject value = null;

        public event Action<GameObject, GameObject> HookCalled;

        void OnValueChanged(GameObject oldValue, GameObject newValue)
        {
            HookCalled.Invoke(oldValue, newValue);
        }
    }

    class NetworkIdentityHookBehaviour : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnValueChanged))]
        public NetworkIdentity value = null;

        public event Action<NetworkIdentity, NetworkIdentity> HookCalled;

        void OnValueChanged(NetworkIdentity oldValue, NetworkIdentity newValue)
        {
            HookCalled.Invoke(oldValue, newValue);
        }
    }

    public class SyncVarHookTest
    {
        private List<GameObject> spawned = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject item in spawned)
            {
                GameObject.DestroyImmediate(item);
            }
            spawned.Clear();

            NetworkIdentity.spawned.Clear();
        }


        T CreateObject<T>() where T : NetworkBehaviour
        {
            GameObject gameObject = new GameObject();
            spawned.Add(gameObject);

            gameObject.AddComponent<NetworkIdentity>();

            T behaviour = gameObject.AddComponent<T>();
            behaviour.syncInterval = 0f;

            return behaviour;
        }

        NetworkIdentity CreateNetworkIdentity(uint netId)
        {
            GameObject gameObject = new GameObject();
            spawned.Add(gameObject);

            NetworkIdentity networkIdentity = gameObject.AddComponent<NetworkIdentity>();
            networkIdentity.netId = netId;
            NetworkIdentity.spawned[netId] = networkIdentity;
            return networkIdentity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverObject"></param>
        /// <param name="clientObject"></param>
        /// <param name="initialState"></param>
        /// <returns>If data was written by OnSerialize</returns>
        public static bool SyncToClient<T>(T serverObject, T clientObject, bool initialState) where T : NetworkBehaviour
        {
            NetworkWriter writer = new NetworkWriter();
            bool written = serverObject.OnSerialize(writer, initialState);

            NetworkReader reader = new NetworkReader(writer.ToArray());
            clientObject.OnDeserialize(reader, initialState);

            int writeLength = writer.Length;
            int readLength = reader.Position;
            Assert.That(writeLength == readLength, $"OnSerializeAll and OnDeserializeAll calls write the same amount of data\n    writeLength={writeLength}\n    readLength={readLength}");

            return written;
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void OldNew_HookCalledWhenSyncingChangedValue(bool intialState)
        {
            HookBehaviour serverObject = CreateObject<HookBehaviour>();
            HookBehaviour clientObject = CreateObject<HookBehaviour>();

            const int clientValue = 10;
            const int serverValue = 24;

            serverObject.value = serverValue;
            clientObject.value = clientValue;

            int callCount = 0;
            clientObject.HookCalled += (oldValue, newValue) =>
            {
                callCount++;
                Assert.That(oldValue, Is.EqualTo(clientValue));
                Assert.That(newValue, Is.EqualTo(serverValue));
            };

            bool written = SyncToClient(serverObject, clientObject, intialState);
            Assert.IsTrue(written);
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void OldNew_HookNotCalledWhenSyncingSameValue(bool intialState)
        {
            HookBehaviour serverObject = CreateObject<HookBehaviour>();
            HookBehaviour clientObject = CreateObject<HookBehaviour>();

            const int clientValue = 16;
            const int serverValue = 16;

            serverObject.value = serverValue;
            clientObject.value = clientValue;

            int callCount = 0;
            clientObject.HookCalled += (oldValue, newValue) =>
            {
                callCount++;
            };

            bool written = SyncToClient(serverObject, clientObject, intialState);
            Assert.IsTrue(written);
            Assert.That(callCount, Is.EqualTo(0));
        }


        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GameObjectHook_HookCalledWhenSyncingChangedValue(bool intialState)
        {
            GameObjectHookBehaviour serverObject = CreateObject<GameObjectHookBehaviour>();
            GameObjectHookBehaviour clientObject = CreateObject<GameObjectHookBehaviour>();

            GameObject clientValue = null;
            GameObject serverValue = CreateNetworkIdentity(2032).gameObject;

            serverObject.value = serverValue;
            clientObject.value = clientValue;

            int callCount = 0;
            clientObject.HookCalled += (oldValue, newValue) =>
            {
                callCount++;
                Assert.That(oldValue, Is.EqualTo(clientValue));
                Assert.That(newValue, Is.EqualTo(serverValue));
            };

            bool written = SyncToClient(serverObject, clientObject, intialState);
            Assert.IsTrue(written);
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void NetworkIdentityHook_HookCalledWhenSyncingChangedValue(bool intialState)
        {
            NetworkIdentityHookBehaviour serverObject = CreateObject<NetworkIdentityHookBehaviour>();
            NetworkIdentityHookBehaviour clientObject = CreateObject<NetworkIdentityHookBehaviour>();

            NetworkIdentity clientValue = null;
            NetworkIdentity serverValue = CreateNetworkIdentity(2033);

            serverObject.value = serverValue;
            clientObject.value = clientValue;

            int callCount = 0;
            clientObject.HookCalled += (oldValue, newValue) =>
            {
                callCount++;
                Assert.That(oldValue, Is.EqualTo(clientValue));
                Assert.That(newValue, Is.EqualTo(serverValue));
            };

            bool written = SyncToClient(serverObject, clientObject, intialState);
            Assert.IsTrue(written);
            Assert.That(callCount, Is.EqualTo(1));
        }
    }
}
