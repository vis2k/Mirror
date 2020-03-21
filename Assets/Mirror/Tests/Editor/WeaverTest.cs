//#define LOG_WEAVER_OUTPUTS

using System.Collections.Generic;
using System.Linq;
using Mirror.Weaver;
using Mono.CecilX;
using Mono.CecilX.Cil;
using NUnit.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    public class WeaverTest
    {
        #region Private
        List<string> weaverErrors = new List<string>();
        void HandleWeaverError(string msg)
        {
#if LOG_WEAVER_OUTPUTS
            Debug.LogError(msg);
#endif
            weaverErrors.Add(msg);
        }

        List<string> weaverWarnings = new List<string>();
        void HandleWeaverWarning(string msg)
        {
#if LOG_WEAVER_OUTPUTS
            Debug.LogWarning(msg);
#endif
            weaverWarnings.Add(msg);
        }

        void BuildAndWeaveTestAssembly(string baseName)
        {
            WeaverAssembler.OutputFile = baseName + ".dll";
            WeaverAssembler.AddSourceFiles(new string[] { baseName + ".cs" });
            WeaverAssembler.Build();

            Assert.That(WeaverAssembler.CompilerErrors, Is.False);
            if (weaverErrors.Count > 0)
            {
                Assert.That(weaverErrors[0], Does.StartWith("Mirror.Weaver error: "));
            }
        }
        #endregion

        #region Setup and Teardown
        [OneTimeSetUp]
        public void FixtureSetup()
        {
            // TextRenderingModule is only referenced to use TextMesh type to throw errors about types from another module
            WeaverAssembler.AddReferencesByAssemblyName(new string[] { "UnityEngine.dll", "UnityEngine.CoreModule.dll", "UnityEngine.TextRenderingModule.dll", "Mirror.dll" });

            CompilationFinishedHook.UnityLogEnabled = false;
            CompilationFinishedHook.OnWeaverError += HandleWeaverError;
            CompilationFinishedHook.OnWeaverWarning += HandleWeaverWarning;
        }

        [OneTimeTearDown]
        public void FixtureCleanup()
        {
            CompilationFinishedHook.UnityLogEnabled = true;
        }

        [SetUp]
        public void TestSetup()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);
        }

        [TearDown]
        public void TestCleanup()
        {
            WeaverAssembler.DeleteOutputOnClear = true;
            WeaverAssembler.Clear();

            weaverWarnings.Clear();
            weaverErrors.Clear();
        }
        #endregion

        #region General tests

        [Test]
        public void RecursionCount()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: MirrorTest.MirrorTestPlayer/Potato1 can't be serialized because it references itself"));
        }

        #endregion

        #region SyncVar tests
        [Test]
        public void SyncVarsValid()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void SyncVarsNoHook()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: No hook implementation found for System.Int32 MirrorTest.MirrorTestPlayer::health. Add this method to your class:\npublic void OnChangeHealth(System.Int32 oldValue, System.Int32 newValue) { }"));
        }

        [Test]
        public void SyncVarsNoHookParams()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::OnChangeHealth() should have signature:\npublic void OnChangeHealth(System.Int32 oldValue, System.Int32 newValue) { }"));
        }

        [Test]
        public void SyncVarsTooManyHookParams()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::OnChangeHealth(System.Int32,System.Int32,System.Int32) should have signature:\npublic void OnChangeHealth(System.Int32 oldValue, System.Int32 newValue) { }"));
        }

        [Test]
        public void SyncVarsWrongHookType()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::OnChangeHealth(System.Boolean,System.Boolean) should have signature:\npublic void OnChangeHealth(System.Int32 oldValue, System.Int32 newValue) { }"));
        }

        [Test]
        public void SyncVarsDerivedNetworkBehaviour()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: Cannot generate writer for component type MirrorTest.MirrorTestPlayer/MySyncVar. Use a supported type or provide a custom writer"));
        }

        [Test]
        public void SyncVarsStatic()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Int32 MirrorTest.MirrorTestPlayer::invalidVar cannot be static"));
        }

        [Test]
        public void SyncVarsGenericParam()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: Cannot generate writer for generic type MirrorTest.MirrorTestPlayer/MySyncVar`1<System.Int32>. Use a concrete type or provide a custom writer"));
        }

        [Test]
        public void SyncVarsInterface()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: Cannot generate writer for interface MirrorTest.MirrorTestPlayer/MySyncVar. Use a concrete type or provide a custom writer"));
        }

        [Test]
        public void SyncVarsDifferentModule()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: Cannot generate writer for component type UnityEngine.TextMesh. Use a supported type or provide a custom writer"));
        }

        [Test]
        public void SyncVarsCantBeArray()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Int32[] MirrorTest.MirrorTestPlayer::thisShouldntWork has invalid type. Use SyncLists instead of arrays"));
        }

        [Test]
        public void SyncVarsSyncList()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(weaverErrors, Is.Empty);
            Assert.That(weaverWarnings, Contains.Item("Mirror.Weaver warning: MirrorTest.SyncObjImplementer MirrorTest.MirrorTestPlayer::syncobj has [SyncVar] attribute. SyncLists should not be marked with SyncVar"));
            Assert.That(weaverWarnings, Contains.Item("Mirror.Weaver warning: Mirror.SyncListInt MirrorTest.MirrorTestPlayer::syncints has [SyncVar] attribute. SyncLists should not be marked with SyncVar"));
        }

        [Test]
        public void SyncVarsMoreThan63()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: MirrorTest.MirrorTestPlayer has too many SyncVars. Consider refactoring your class into multiple components"));
        }
        #endregion

        #region SyncList tests
        [Test]
        public void SyncListValid()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void SyncListMissingParamlessCtor()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: MirrorTest.MirrorTestPlayer/SyncListString2 MirrorTest.MirrorTestPlayer::Foo does not have a default constructor"));
        }

        [Test]
        public void SyncListByteValid()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(weaverErrors, Is.Empty);
        }

        #endregion

        #region NetworkBehaviour tests
        [Test]
        public void NetworkBehaviourValid()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void NetworkBehaviourAbstractBaseValid()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void NetworkBehaviourGeneric()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: MirrorTest.MirrorTestPlayer`1 cannot have generic parameters"));
        }

        [Test]
        public void NetworkBehaviourCmdGenericParam()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::CmdCantHaveGeneric() cannot have generic parameters"));
        }

        [Test]
        public void NetworkBehaviourCmdCoroutine()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Collections.IEnumerator MirrorTest.MirrorTestPlayer::CmdCantHaveCoroutine() cannot be a coroutine"));
        }

        [Test]
        public void NetworkBehaviourCmdVoidReturn()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Int32 MirrorTest.MirrorTestPlayer::CmdCantHaveNonVoidReturn() cannot return a value.  Make it void instead"));
        }

        [Test]
        public void NetworkBehaviourTargetRpcGenericParam()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::TargetRpcCantHaveGeneric() cannot have generic parameters"));
        }

        [Test]
        public void NetworkBehaviourTargetRpcCoroutine()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Collections.IEnumerator MirrorTest.MirrorTestPlayer::TargetRpcCantHaveCoroutine() cannot be a coroutine"));
        }

        [Test]
        public void NetworkBehaviourTargetRpcVoidReturn()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Int32 MirrorTest.MirrorTestPlayer::TargetRpcCantHaveNonVoidReturn() cannot return a value.  Make it void instead"));
        }

        [Test]
        public void NetworkBehaviourTargetRpcParamOut()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::TargetRpcCantHaveParamOut(Mirror.NetworkConnection,System.Int32&) cannot have out parameters"));
        }

        [Test]
        public void NetworkBehaviourTargetRpcParamOptional()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::TargetRpcCantHaveParamOptional(Mirror.NetworkConnection,System.Int32) cannot have optional parameters"));
        }

        [Test]
        public void NetworkBehaviourTargetRpcParamRef()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: Cannot pass System.Int32& by reference"));
        }

        [Test]
        public void NetworkBehaviourTargetRpcParamAbstract()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: MirrorTest.MirrorTestPlayer/AbstractClass can't be deserialized bcause i has no default constructor"));
        }

        [Test]
        public void NetworkBehaviourTargetRpcParamComponent()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: Cannot generate writer for component type MirrorTest.MirrorTestPlayer/ComponentClass. Use a supported type or provide a custom writer"));
        }

        [Test]
        public void NetworkBehaviourTargetRpcParamNetworkConnection()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void NetworkBehaviourTargetRpcDuplicateName()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: Duplicate Target Rpc name [MirrorTest.MirrorTestPlayer:TargetRpcCantHaveSameName]"));
        }

        [Test]
        public void NetworkBehaviourClientRpcGenericParam()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::RpcCantHaveGeneric() cannot have generic parameters"));
        }

        [Test]
        public void NetworkBehaviourClientRpcCoroutine()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Collections.IEnumerator MirrorTest.MirrorTestPlayer::RpcCantHaveCoroutine() cannot be a coroutine"));
        }

        [Test]
        public void NetworkBehaviourClientRpcVoidReturn()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Int32 MirrorTest.MirrorTestPlayer::RpcCantHaveNonVoidReturn() cannot return a value.  Make it void instead"));
        }

        [Test]
        public void NetworkBehaviourClientRpcParamOut()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::RpcCantHaveParamOut(System.Int32&) cannot have out parameters"));
        }

        [Test]
        public void NetworkBehaviourClientRpcParamOptional()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::RpcCantHaveParamOptional(System.Int32) cannot have optional parameters"));
        }

        [Test]
        public void NetworkBehaviourClientRpcParamRef()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: Cannot pass System.Int32& by reference"));
        }

        [Test]
        public void NetworkBehaviourClientRpcParamAbstract()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: MirrorTest.MirrorTestPlayer/AbstractClass can't be deserialized bcause i has no default constructor"));
        }

        [Test]
        public void NetworkBehaviourClientRpcParamComponent()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: Cannot generate writer for component type MirrorTest.MirrorTestPlayer/ComponentClass. Use a supported type or provide a custom writer"));
        }

        [Test]
        public void NetworkBehaviourClientRpcParamNetworkConnection()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::RpcCantHaveParamOptional(Mirror.NetworkConnection) has invalid parameer monkeyCon. Cannot pass NeworkConnections"));
        }

        [Test]
        public void NetworkBehaviourClientRpcDuplicateName()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: Duplicate ClientRpc name [MirrorTest.MirrorTestPlayer:RpcCantHaveSameName]"));
        }

        [Test]
        public void NetworkBehaviourCmdParamOut()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::CmdCantHaveParamOut(System.Int32&) cannot have out parameters"));
        }

        [Test]
        public void NetworkBehaviourCmdParamOptional()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::CmdCantHaveParamOptional(System.Int32) cannot have optional parameters"));
        }

        [Test]
        public void NetworkBehaviourCmdParamRef()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: Cannot pass System.Int32& by reference"));
        }

        [Test]
        public void NetworkBehaviourCmdParamAbstract()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: MirrorTest.MirrorTestPlayer/AbstractClass can't be deserialized bcause i has no default constructor"));
        }

        [Test]
        public void NetworkBehaviourCmdParamComponent()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: Cannot generate writer for component type MirrorTest.MirrorTestPlayer/ComponentClass. Use a supported type or provide a custom writer"));
        }

        [Test]
        public void NetworkBehaviourCmdParamNetworkConnection()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::CmdCantHaveParamOptional(Mirror.NetworkConnection) has invalid parameer monkeyCon. Cannot pass NeworkConnections"));
        }

        [Test]
        public void NetworkBehaviourCmdDuplicateName()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: Duplicate Command name [MirrorTest.MirrorTestPlayer:CmdCantHaveSameName]"));
        }
        #endregion

        #region Command tests
        [Test]
        public void CommandValid()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void CommandStartsWithCmd()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::DoesntStartWithCmd() must start with Cmd.  Consider renaming it to CmdDoesntStartWithCmd"));
        }

        [Test]
        public void CommandCantBeStatic()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::CmdCantBeStatic() cannot be static"));
        }
        #endregion

        #region ClientRpc tests
        [Test]
        public void ClientRpcValid()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void ClientRpcStartsWithRpc()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::DoesntStartWithRpc() must start with Rpc.  Consider renaming it to RpcDoesntStartWithRpc"));
        }

        [Test]
        public void ClientRpcCantBeStatic()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::RpcCantBeStatic() must not be static"));
        }
        #endregion

        #region TargetRpc tests
        [Test]
        public void TargetRpcValid()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void TargetRpcStartsWithTarget()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::DoesntStartWithTarget(Mirror.NetworkConnection) must start with Target.  Consider renaming it to TargetDoesntStartWithTarget"));
        }

        [Test]
        public void TargetRpcCantBeStatic()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.Void MirrorTest.MirrorTestPlayer::TargetCantBeStatic(Mirror.NetworkConnection) must not be static"));
        }
        #endregion

        #region TargetRpc tests
        [Test]
        public void SyncEventValid()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void SyncEventStartsWithEvent()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: MirrorTest.MirrorTestPlayer/MySyncEventDelegate MirrorTest.MirrorTestPlayer::DoCoolThingsWithExcitingPeople must start with Event.  Consider renaming it to EventDoCoolThingsWithExcitingPeople"));
        }

        [Test]
        public void SyncEventParamGeneric()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: MirrorTest.MirrorTestPlayer/MySyncEventDelegate`1<System.Int32> MirrorTest.MirrorTestPlayer::EventDoCoolThingsWithExcitingPeople must not have generic parameters.  Consider creating a new class that inherits from MirrorTest.MirrorTestPlayer/MySyncEventDelegate`1<System.Int32> instead"));
        }
        #endregion

        #region MonoBehaviour tests
        [Test]
        public void MonoBehaviourValid()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void MonoBehaviourSyncVar()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: [SyncVar] System.Int32 MirrorTest.MirrorTestPlayer::potato must be inside a NetworkBehaviour.  MirrorTest.MirrorTestPlayer is not a NetworkBehaviour"));
        }

        [Test]
        public void MonoBehaviourSyncList()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: Mirror.SyncListInt MirrorTest.MirrorTestPlayer::potato is a SyncObject and must be inside a NetworkBehaviour.  MirrorTest.MirrorTestPlayer is not a NetworkBehaviour"));
        }

        [Test]
        public void MonoBehaviourCommand()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: [Command] System.Void MirrorTest.MirrorTestPlayer::CmdThisCantBeOutsideNetworkBehaviour() must be declared inside a NetworkBehaviour"));
        }

        [Test]
        public void MonoBehaviourClientRpc()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: [ClientRpc] System.Void MirrorTest.MirrorTestPlayer::RpcThisCantBeOutsideNetworkBehaviour() must be declared inside a NetworkBehaviour"));
        }

        [Test]
        public void MonoBehaviourTargetRpc()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: [TargetRpc] System.Void MirrorTest.MirrorTestPlayer::TargetThisCantBeOutsideNetworkBehaviour(Mirror.NetworkConnection) must be declared inside a NetworkBehaviour"));
        }

        [Test]
        public void MonoBehaviourServer()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: [Server] System.Void MirrorTest.MirrorTestPlayer::ThisCantBeOutsideNetworkBehaviour() must be declared inside a NetworkBehaviour"));
        }

        [Test]
        public void MonoBehaviourServerCallback()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: [ServerCallback] System.Void MirrorTest.MirrorTestPlayer::ThisCantBeOutsideNetworkBehaviour() must be declared inside a NetworkBehaviour"));
        }

        [Test]
        public void MonoBehaviourClient()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: [Client] System.Void MirrorTest.MirrorTestPlayer::ThisCantBeOutsideNetworkBehaviour() must be declared inside a NetworkBehaviour"));
        }

        [Test]
        public void MonoBehaviourClientCallback()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: [ClientCallback] System.Void MirrorTest.MirrorTestPlayer::ThisCantBeOutsideNetworkBehaviour() must be declared inside a NetworkBehaviour"));
        }
        #endregion

        #region Message tests
        [Test]
        public void MessageValid()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void MessageSelfReferencing()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: MirrorTest.PrefabClone has field $MirrorTest.PrefabClone MirrorTest.PrefabClone::selfReference that references itself"));
        }

        [Test]
        public void MessageInvalidSerializeFieldType()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.AccessViolationException MirrorTest.PrefabClone::invalidField has unsupported type"));
        }

        [Test]
        public void MessageInvalidDeserializeFieldType()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.AccessViolationException is not a supported type"));
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: System.AccessViolationException MirrorTest.PrefabClone::invalidField has unsupported type"));
        }

        [Test]
        public void MessageMemberGeneric()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: Cannot generate writer for generic type MirrorTest.HasGeneric`1<System.Int32>. Use a concrete type or provide a custom writer"));
        }

        [Test]
        public void MessageMemberInterface()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(weaverErrors, Contains.Item("Mirror.Weaver error: Cannot generate writer for interface MirrorTest.SuperCoolInterface. Use a concrete type or provide a custom writer"));
        }
        #endregion

        #region Server Client Attribute Tests 
        [Test]
        public void NetworkBehaviourServer()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(weaverErrors, Is.Empty);
            CheckAddedCodeServer();
        }

        [Test] 
        public void NetworkBehaviourClient()
        {
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(weaverErrors, Is.Empty);
            CheckAddedCodeClient();
        }

        void CheckAddedCodeServer()
        {
            const string networkServerGetActive = "System.Boolean Mirror.NetworkServer::get_active()";
            CheckAddedCode(networkServerGetActive, "ServerOnlyMethod");
        }

        void CheckAddedCodeClient()
        {
            const string networkClientGetActive = "System.Boolean Mirror.NetworkClient::get_active()";
            CheckAddedCode(networkClientGetActive, "ClientOnlyMethod");
        }

        /// <summary>
        /// Checks that first Instructions in MethodBody is addedString
        /// </summary>
        /// <param name="addedString"></param>
        /// <param name="methodName"></param>
        void CheckAddedCode(string addedString, string methodName)
        {
            string className = "MirrorTest.MirrorTestPlayer";
            
            string assemblyName = WeaverAssembler.OutputDirectory + WeaverAssembler.OutputFile;
            using (AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(assemblyName))
            {
                TypeDefinition type = assembly.MainModule.GetType(className);
                MethodDefinition method = type.Methods.First(m => m.Name == methodName);
                MethodBody body = method.Body;

                Instruction top = body.Instructions[0];

                Assert.AreEqual(top.OpCode, OpCodes.Call);
                Assert.AreEqual(top.Operand.ToString(), addedString);
            }
        }
        #endregion

         [Test]
        public void TestingScriptableObjectArraySerialization()
        {
            UnityEngine.Debug.Log(string.Join("\n",weaverErrors));
            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
        }
    }
}
