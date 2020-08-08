// Generated by AttributeTestGenerator.cs
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirror.Tests.Generated
{
    
    public class AttributeBehaviour_NetworkBehaviour : NetworkBehaviour
    {
        public static readonly float Expected_float = 2020f;
        public static readonly bool Expected_bool = true;
        public static readonly char Expected_char = 'a';
        public static readonly byte Expected_byte = 224;
        public static readonly int Expected_int = 103;
        public static readonly Vector3 Expected_Vector3 = new Vector3(29, 1, 10);
        public static readonly ClassWithNoConstructor Expected_ClassWithNoConstructor = new ClassWithNoConstructor { a = 10 };
        public static readonly ClassWithConstructor Expected_ClassWithConstructor = new ClassWithConstructor(29);

        
        [Client]
        public float Client_float_Function()
        {
            return Expected_float;
        }

        [Client]
        public bool Client_bool_Function()
        {
            return Expected_bool;
        }

        [Client]
        public char Client_char_Function()
        {
            return Expected_char;
        }

        [Client]
        public byte Client_byte_Function()
        {
            return Expected_byte;
        }

        [Client]
        public int Client_int_Function()
        {
            return Expected_int;
        }

        [Client]
        public Vector3 Client_Vector3_Function()
        {
            return Expected_Vector3;
        }

        [Client]
        public ClassWithNoConstructor Client_ClassWithNoConstructor_Function()
        {
            return Expected_ClassWithNoConstructor;
        }

        [Client]
        public ClassWithConstructor Client_ClassWithConstructor_Function()
        {
            return Expected_ClassWithConstructor;
        }

        [Server]
        public float Server_float_Function()
        {
            return Expected_float;
        }

        [Server]
        public bool Server_bool_Function()
        {
            return Expected_bool;
        }

        [Server]
        public char Server_char_Function()
        {
            return Expected_char;
        }

        [Server]
        public byte Server_byte_Function()
        {
            return Expected_byte;
        }

        [Server]
        public int Server_int_Function()
        {
            return Expected_int;
        }

        [Server]
        public Vector3 Server_Vector3_Function()
        {
            return Expected_Vector3;
        }

        [Server]
        public ClassWithNoConstructor Server_ClassWithNoConstructor_Function()
        {
            return Expected_ClassWithNoConstructor;
        }

        [Server]
        public ClassWithConstructor Server_ClassWithConstructor_Function()
        {
            return Expected_ClassWithConstructor;
        }

        [ClientCallback]
        public float ClientCallback_float_Function()
        {
            return Expected_float;
        }

        [ClientCallback]
        public bool ClientCallback_bool_Function()
        {
            return Expected_bool;
        }

        [ClientCallback]
        public char ClientCallback_char_Function()
        {
            return Expected_char;
        }

        [ClientCallback]
        public byte ClientCallback_byte_Function()
        {
            return Expected_byte;
        }

        [ClientCallback]
        public int ClientCallback_int_Function()
        {
            return Expected_int;
        }

        [ClientCallback]
        public Vector3 ClientCallback_Vector3_Function()
        {
            return Expected_Vector3;
        }

        [ClientCallback]
        public ClassWithNoConstructor ClientCallback_ClassWithNoConstructor_Function()
        {
            return Expected_ClassWithNoConstructor;
        }

        [ClientCallback]
        public ClassWithConstructor ClientCallback_ClassWithConstructor_Function()
        {
            return Expected_ClassWithConstructor;
        }

        [ServerCallback]
        public float ServerCallback_float_Function()
        {
            return Expected_float;
        }

        [ServerCallback]
        public bool ServerCallback_bool_Function()
        {
            return Expected_bool;
        }

        [ServerCallback]
        public char ServerCallback_char_Function()
        {
            return Expected_char;
        }

        [ServerCallback]
        public byte ServerCallback_byte_Function()
        {
            return Expected_byte;
        }

        [ServerCallback]
        public int ServerCallback_int_Function()
        {
            return Expected_int;
        }

        [ServerCallback]
        public Vector3 ServerCallback_Vector3_Function()
        {
            return Expected_Vector3;
        }

        [ServerCallback]
        public ClassWithNoConstructor ServerCallback_ClassWithNoConstructor_Function()
        {
            return Expected_ClassWithNoConstructor;
        }

        [ServerCallback]
        public ClassWithConstructor ServerCallback_ClassWithConstructor_Function()
        {
            return Expected_ClassWithConstructor;
        }
    }


    public class AttributeTest_NetworkBehaviour 
    {
        AttributeBehaviour_NetworkBehaviour behaviour;
        GameObject go;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject();
            behaviour = go.AddComponent<AttributeBehaviour_NetworkBehaviour>();
        }
        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(go);
            NetworkClient.connectState = ConnectState.None;
            NetworkServer.active = false;
        }

        
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Client_WithfloatReturn(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;

            float expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_float : default;
            
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Client] function 'System.Single Mirror.Tests.Generated.AttributeBehaviour_NetworkBehaviour::Client_float_Function()' called when client was not active");
            }
            float actual = behaviour.Client_float_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Client_WithboolReturn(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;

            bool expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_bool : default;
            
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Client] function 'System.Boolean Mirror.Tests.Generated.AttributeBehaviour_NetworkBehaviour::Client_bool_Function()' called when client was not active");
            }
            bool actual = behaviour.Client_bool_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Client_WithcharReturn(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;

            char expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_char : default;
            
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Client] function 'System.Char Mirror.Tests.Generated.AttributeBehaviour_NetworkBehaviour::Client_char_Function()' called when client was not active");
            }
            char actual = behaviour.Client_char_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Client_WithbyteReturn(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;

            byte expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_byte : default;
            
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Client] function 'System.Byte Mirror.Tests.Generated.AttributeBehaviour_NetworkBehaviour::Client_byte_Function()' called when client was not active");
            }
            byte actual = behaviour.Client_byte_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Client_WithintReturn(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;

            int expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_int : default;
            
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Client] function 'System.Int32 Mirror.Tests.Generated.AttributeBehaviour_NetworkBehaviour::Client_int_Function()' called when client was not active");
            }
            int actual = behaviour.Client_int_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Client_WithVector3Return(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;

            Vector3 expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_Vector3 : default;
            
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Client] function 'UnityEngine.Vector3 Mirror.Tests.Generated.AttributeBehaviour_NetworkBehaviour::Client_Vector3_Function()' called when client was not active");
            }
            Vector3 actual = behaviour.Client_Vector3_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Client_WithClassWithNoConstructorReturn(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;

            ClassWithNoConstructor expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_ClassWithNoConstructor : default;
            
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Client] function 'Mirror.Tests.ClassWithNoConstructor Mirror.Tests.Generated.AttributeBehaviour_NetworkBehaviour::Client_ClassWithNoConstructor_Function()' called when client was not active");
            }
            ClassWithNoConstructor actual = behaviour.Client_ClassWithNoConstructor_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Client_WithClassWithConstructorReturn(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;

            ClassWithConstructor expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_ClassWithConstructor : default;
            
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Client] function 'Mirror.Tests.ClassWithConstructor Mirror.Tests.Generated.AttributeBehaviour_NetworkBehaviour::Client_ClassWithConstructor_Function()' called when client was not active");
            }
            ClassWithConstructor actual = behaviour.Client_ClassWithConstructor_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Server_WithfloatReturn(bool active)
        {
            NetworkServer.active = active;

            float expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_float : default;
            
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Server] function 'System.Single Mirror.Tests.Generated.AttributeBehaviour_NetworkBehaviour::Server_float_Function()' called when server was not active");
            }
            float actual = behaviour.Server_float_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Server_WithboolReturn(bool active)
        {
            NetworkServer.active = active;

            bool expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_bool : default;
            
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Server] function 'System.Boolean Mirror.Tests.Generated.AttributeBehaviour_NetworkBehaviour::Server_bool_Function()' called when server was not active");
            }
            bool actual = behaviour.Server_bool_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Server_WithcharReturn(bool active)
        {
            NetworkServer.active = active;

            char expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_char : default;
            
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Server] function 'System.Char Mirror.Tests.Generated.AttributeBehaviour_NetworkBehaviour::Server_char_Function()' called when server was not active");
            }
            char actual = behaviour.Server_char_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Server_WithbyteReturn(bool active)
        {
            NetworkServer.active = active;

            byte expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_byte : default;
            
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Server] function 'System.Byte Mirror.Tests.Generated.AttributeBehaviour_NetworkBehaviour::Server_byte_Function()' called when server was not active");
            }
            byte actual = behaviour.Server_byte_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Server_WithintReturn(bool active)
        {
            NetworkServer.active = active;

            int expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_int : default;
            
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Server] function 'System.Int32 Mirror.Tests.Generated.AttributeBehaviour_NetworkBehaviour::Server_int_Function()' called when server was not active");
            }
            int actual = behaviour.Server_int_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Server_WithVector3Return(bool active)
        {
            NetworkServer.active = active;

            Vector3 expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_Vector3 : default;
            
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Server] function 'UnityEngine.Vector3 Mirror.Tests.Generated.AttributeBehaviour_NetworkBehaviour::Server_Vector3_Function()' called when server was not active");
            }
            Vector3 actual = behaviour.Server_Vector3_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Server_WithClassWithNoConstructorReturn(bool active)
        {
            NetworkServer.active = active;

            ClassWithNoConstructor expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_ClassWithNoConstructor : default;
            
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Server] function 'Mirror.Tests.ClassWithNoConstructor Mirror.Tests.Generated.AttributeBehaviour_NetworkBehaviour::Server_ClassWithNoConstructor_Function()' called when server was not active");
            }
            ClassWithNoConstructor actual = behaviour.Server_ClassWithNoConstructor_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Server_WithClassWithConstructorReturn(bool active)
        {
            NetworkServer.active = active;

            ClassWithConstructor expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_ClassWithConstructor : default;
            
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Server] function 'Mirror.Tests.ClassWithConstructor Mirror.Tests.Generated.AttributeBehaviour_NetworkBehaviour::Server_ClassWithConstructor_Function()' called when server was not active");
            }
            ClassWithConstructor actual = behaviour.Server_ClassWithConstructor_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ClientCallback_WithfloatReturn(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;

            float expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_float : default;
            
            float actual = behaviour.ClientCallback_float_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ClientCallback_WithboolReturn(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;

            bool expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_bool : default;
            
            bool actual = behaviour.ClientCallback_bool_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ClientCallback_WithcharReturn(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;

            char expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_char : default;
            
            char actual = behaviour.ClientCallback_char_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ClientCallback_WithbyteReturn(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;

            byte expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_byte : default;
            
            byte actual = behaviour.ClientCallback_byte_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ClientCallback_WithintReturn(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;

            int expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_int : default;
            
            int actual = behaviour.ClientCallback_int_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ClientCallback_WithVector3Return(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;

            Vector3 expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_Vector3 : default;
            
            Vector3 actual = behaviour.ClientCallback_Vector3_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ClientCallback_WithClassWithNoConstructorReturn(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;

            ClassWithNoConstructor expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_ClassWithNoConstructor : default;
            
            ClassWithNoConstructor actual = behaviour.ClientCallback_ClassWithNoConstructor_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ClientCallback_WithClassWithConstructorReturn(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;

            ClassWithConstructor expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_ClassWithConstructor : default;
            
            ClassWithConstructor actual = behaviour.ClientCallback_ClassWithConstructor_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ServerCallback_WithfloatReturn(bool active)
        {
            NetworkServer.active = active;

            float expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_float : default;
            
            float actual = behaviour.ServerCallback_float_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ServerCallback_WithboolReturn(bool active)
        {
            NetworkServer.active = active;

            bool expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_bool : default;
            
            bool actual = behaviour.ServerCallback_bool_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ServerCallback_WithcharReturn(bool active)
        {
            NetworkServer.active = active;

            char expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_char : default;
            
            char actual = behaviour.ServerCallback_char_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ServerCallback_WithbyteReturn(bool active)
        {
            NetworkServer.active = active;

            byte expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_byte : default;
            
            byte actual = behaviour.ServerCallback_byte_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ServerCallback_WithintReturn(bool active)
        {
            NetworkServer.active = active;

            int expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_int : default;
            
            int actual = behaviour.ServerCallback_int_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ServerCallback_WithVector3Return(bool active)
        {
            NetworkServer.active = active;

            Vector3 expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_Vector3 : default;
            
            Vector3 actual = behaviour.ServerCallback_Vector3_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ServerCallback_WithClassWithNoConstructorReturn(bool active)
        {
            NetworkServer.active = active;

            ClassWithNoConstructor expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_ClassWithNoConstructor : default;
            
            ClassWithNoConstructor actual = behaviour.ServerCallback_ClassWithNoConstructor_Function();

            Assert.AreEqual(expected, actual); 
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ServerCallback_WithClassWithConstructorReturn(bool active)
        {
            NetworkServer.active = active;

            ClassWithConstructor expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_ClassWithConstructor : default;
            
            ClassWithConstructor actual = behaviour.ServerCallback_ClassWithConstructor_Function();

            Assert.AreEqual(expected, actual); 
        }
    }
}