using System;
using System.Linq;
using Mono.CecilX;

namespace Mirror.Weaver
{
    public static class WeaverTypes
    {
        // Network types
        public static TypeReference NetworkBehaviourType;
        public static TypeReference RemoteCallHelperType;
        public static TypeReference MonoBehaviourType;
        public static TypeReference ScriptableObjectType;
        public static TypeReference NetworkConnectionType;

        public static TypeReference SyncListType;
        public static TypeReference SyncSetType;
        public static TypeReference SyncDictionaryType;

        public static MethodReference ScriptableObjectCreateInstanceMethod;

        public static MethodReference NetworkBehaviourDirtyBitsReference;
        public static MethodReference GetPooledWriterReference;
        public static MethodReference RecycleWriterReference;
        public static TypeReference NetworkClientType;
        public static TypeReference NetworkServerType;

        public static TypeReference NetworkIdentityType;

        public static TypeReference ClientSceneType;
        public static MethodReference ReadyConnectionReference;

        public static TypeReference ComponentType;
        public static TypeReference ObjectType;

        public static MethodReference CmdDelegateConstructor;

        public static MethodReference NetworkServerGetActive;
        public static MethodReference NetworkServerGetLocalClientActive;
        public static MethodReference NetworkClientGetActive;

        // custom attribute types
        public static TypeReference SyncVarType;
        public static TypeReference CommandType;
        public static TypeReference ClientRpcType;
        public static TypeReference TargetRpcType;
        public static MethodReference InitSyncObjectReference;

        // array segment
        public static TypeReference ArraySegmentType;
        public static MethodReference ArraySegmentConstructorReference;
        public static MethodReference ArraySegmentArrayReference;
        public static MethodReference ArraySegmentOffsetReference;
        public static MethodReference ArraySegmentCountReference;

        // list
        public static TypeReference ListType;
        public static MethodReference ListConstructorReference;
        public static MethodReference ListCountReference;
        public static MethodReference ListGetItemReference;
        public static MethodReference ListAddReference;

        // system types
        public static TypeReference typeType;
        public static TypeReference gameObjectType;
        public static TypeReference transformType;

        public static MethodReference syncVarEqualReference;
        public static MethodReference syncVarNetworkIdentityEqualReference;
        public static MethodReference syncVarGameObjectEqualReference;
        public static MethodReference setSyncVarReference;
        public static MethodReference setSyncVarHookGuard;
        public static MethodReference getSyncVarHookGuard;
        public static MethodReference setSyncVarGameObjectReference;
        public static MethodReference getSyncVarGameObjectReference;
        public static MethodReference setSyncVarNetworkIdentityReference;
        public static MethodReference getSyncVarNetworkIdentityReference;
        public static MethodReference registerCommandDelegateReference;
        public static MethodReference registerRpcDelegateReference;
        public static MethodReference getTypeReference;
        public static MethodReference getTypeFromHandleReference;
        public static MethodReference logErrorReference;
        public static MethodReference logWarningReference;
        public static MethodReference sendCommandInternal;
        public static MethodReference sendRpcInternal;
        public static MethodReference sendTargetRpcInternal;


        public static void SetupUnityTypes(AssemblyDefinition unityAssembly, AssemblyDefinition mirrorAssembly)
        {
            gameObjectType = unityAssembly.MainModule.GetType("UnityEngine.GameObject");
            transformType = unityAssembly.MainModule.GetType("UnityEngine.Transform");

            NetworkClientType = mirrorAssembly.MainModule.GetType("Mirror.NetworkClient");
            NetworkServerType = mirrorAssembly.MainModule.GetType("Mirror.NetworkServer");

            SyncVarType = mirrorAssembly.MainModule.GetType("Mirror.SyncVarAttribute");
            CommandType = mirrorAssembly.MainModule.GetType("Mirror.CommandAttribute");
            ClientRpcType = mirrorAssembly.MainModule.GetType("Mirror.ClientRpcAttribute");
            TargetRpcType = mirrorAssembly.MainModule.GetType("Mirror.TargetRpcAttribute");
        }

        static ModuleDefinition ResolveSystemModule(AssemblyDefinition currentAssembly)
        {
            AssemblyNameReference name = AssemblyNameReference.Parse("mscorlib");
            ReaderParameters parameters = new ReaderParameters
            {
                AssemblyResolver = currentAssembly.MainModule.AssemblyResolver
            };
            return currentAssembly.MainModule.AssemblyResolver.Resolve(name, parameters).MainModule;
        }

        static TypeReference ImportSystemModuleType(AssemblyDefinition currentAssembly, ModuleDefinition systemModule, string fullName)
        {
            TypeDefinition type = systemModule.GetType(fullName) ?? systemModule.ExportedTypes.First(t => t.FullName == fullName).Resolve();
            if (type != null)
            {
                return currentAssembly.MainModule.ImportReference(type);
            }
            Weaver.Error("Failed to import mscorlib type: " + fullName + " because Resolve failed. (Might happen when trying to Resolve in NetStandard dll, see also: https://github.com/vis2k/Mirror/issues/791)");
            return null;
        }

        private static AssemblyDefinition currentAssembly;
        private static ModuleDefinition systemModule;

        public static TypeReference Import<T>() => Import(typeof(T));

        public static TypeReference Import(Type t) => currentAssembly.MainModule.ImportReference(t);

        public static void SetupTargetTypes(AssemblyDefinition unityAssembly, AssemblyDefinition mirrorAssembly, AssemblyDefinition currentAssembly)
        {
            // system types
            WeaverTypes.currentAssembly = currentAssembly;
            systemModule = ResolveSystemModule(currentAssembly);

            typeType = Import<System.Type>();

            ArraySegmentType = ImportSystemModuleType(currentAssembly, systemModule, "System.ArraySegment`1");
            ArraySegmentArrayReference = Resolvers.ResolveProperty(ArraySegmentType, currentAssembly, "Array");
            ArraySegmentCountReference = Resolvers.ResolveProperty(ArraySegmentType, currentAssembly, "Count");
            ArraySegmentOffsetReference = Resolvers.ResolveProperty(ArraySegmentType, currentAssembly, "Offset");
            ArraySegmentConstructorReference = Resolvers.ResolveMethod(ArraySegmentType, currentAssembly, ".ctor");

            ListType = ImportSystemModuleType(currentAssembly, systemModule, "System.Collections.Generic.List`1");
            ListCountReference = Resolvers.ResolveProperty(ListType, currentAssembly, "Count");
            ListGetItemReference = Resolvers.ResolveMethod(ListType, currentAssembly, "get_Item");
            ListAddReference = Resolvers.ResolveMethod(ListType, currentAssembly, "Add");
            ListConstructorReference = Resolvers.ResolveMethod(ListType, currentAssembly, ".ctor");

            NetworkServerGetActive = Resolvers.ResolveMethod(NetworkServerType, currentAssembly, "get_active");
            NetworkServerGetLocalClientActive = Resolvers.ResolveMethod(NetworkServerType, currentAssembly, "get_localClientActive");
            NetworkClientGetActive = Resolvers.ResolveMethod(NetworkClientType, currentAssembly, "get_active");

            TypeDefinition cmdDelegateReference = mirrorAssembly.MainModule.GetType("Mirror.RemoteCalls.CmdDelegate");
            CmdDelegateConstructor = Resolvers.ResolveMethod(cmdDelegateReference, currentAssembly, ".ctor");

            currentAssembly.MainModule.ImportReference(gameObjectType);
            currentAssembly.MainModule.ImportReference(transformType);

            TypeReference networkIdentityTmp = mirrorAssembly.MainModule.GetType("Mirror.NetworkIdentity");
            NetworkIdentityType = currentAssembly.MainModule.ImportReference(networkIdentityTmp);

            NetworkBehaviourType = mirrorAssembly.MainModule.GetType("Mirror.NetworkBehaviour");
            RemoteCallHelperType = mirrorAssembly.MainModule.GetType("Mirror.RemoteCalls.RemoteCallHelper");
            NetworkConnectionType = mirrorAssembly.MainModule.GetType("Mirror.NetworkConnection");

            MonoBehaviourType = unityAssembly.MainModule.GetType("UnityEngine.MonoBehaviour");
            ScriptableObjectType = unityAssembly.MainModule.GetType("UnityEngine.ScriptableObject");

            ScriptableObjectCreateInstanceMethod = Resolvers.ResolveMethod(
                ScriptableObjectType, currentAssembly,
                md => md.Name == "CreateInstance" && md.HasGenericParameters);

            NetworkConnectionType = mirrorAssembly.MainModule.GetType("Mirror.NetworkConnection");
            NetworkConnectionType = currentAssembly.MainModule.ImportReference(NetworkConnectionType);

            SyncListType = mirrorAssembly.MainModule.GetType("Mirror.SyncList`1");
            SyncSetType = mirrorAssembly.MainModule.GetType("Mirror.SyncSet`1");
            SyncDictionaryType = mirrorAssembly.MainModule.GetType("Mirror.SyncDictionary`2");

            NetworkBehaviourDirtyBitsReference = Resolvers.ResolveProperty(NetworkBehaviourType, currentAssembly, "syncVarDirtyBits");
            TypeDefinition NetworkWriterPoolType = mirrorAssembly.MainModule.GetType("Mirror.NetworkWriterPool");
            GetPooledWriterReference = Resolvers.ResolveMethod(NetworkWriterPoolType, currentAssembly, "GetWriter");
            RecycleWriterReference = Resolvers.ResolveMethod(NetworkWriterPoolType, currentAssembly, "Recycle");

            ComponentType = unityAssembly.MainModule.GetType("UnityEngine.Component");
            ObjectType = unityAssembly.MainModule.GetType("UnityEngine.Object");
            ClientSceneType = mirrorAssembly.MainModule.GetType("Mirror.ClientScene");
            ReadyConnectionReference = Resolvers.ResolveMethod(ClientSceneType, currentAssembly, "get_readyConnection");

            syncVarEqualReference = Resolvers.ResolveMethod(NetworkBehaviourType, currentAssembly, "SyncVarEqual");
            syncVarNetworkIdentityEqualReference = Resolvers.ResolveMethod(NetworkBehaviourType, currentAssembly, "SyncVarNetworkIdentityEqual");
            syncVarGameObjectEqualReference = Resolvers.ResolveMethod(NetworkBehaviourType, currentAssembly, "SyncVarGameObjectEqual");
            setSyncVarReference = Resolvers.ResolveMethod(NetworkBehaviourType, currentAssembly, "SetSyncVar");
            setSyncVarHookGuard = Resolvers.ResolveMethod(NetworkBehaviourType, currentAssembly, "setSyncVarHookGuard");
            getSyncVarHookGuard = Resolvers.ResolveMethod(NetworkBehaviourType, currentAssembly, "getSyncVarHookGuard");

            setSyncVarGameObjectReference = Resolvers.ResolveMethod(NetworkBehaviourType, currentAssembly, "SetSyncVarGameObject");
            getSyncVarGameObjectReference = Resolvers.ResolveMethod(NetworkBehaviourType, currentAssembly, "GetSyncVarGameObject");
            setSyncVarNetworkIdentityReference = Resolvers.ResolveMethod(NetworkBehaviourType, currentAssembly, "SetSyncVarNetworkIdentity");
            getSyncVarNetworkIdentityReference = Resolvers.ResolveMethod(NetworkBehaviourType, currentAssembly, "GetSyncVarNetworkIdentity");
            registerCommandDelegateReference = Resolvers.ResolveMethod(RemoteCallHelperType, currentAssembly, "RegisterCommandDelegate");
            registerRpcDelegateReference = Resolvers.ResolveMethod(RemoteCallHelperType, currentAssembly, "RegisterRpcDelegate");
            getTypeFromHandleReference = Resolvers.ResolveMethod(typeType, currentAssembly, "GetTypeFromHandle");
            logErrorReference = Resolvers.ResolveMethod(unityAssembly.MainModule.GetType("UnityEngine.Debug"), currentAssembly, "LogError");
            logWarningReference = Resolvers.ResolveMethod(unityAssembly.MainModule.GetType("UnityEngine.Debug"), currentAssembly, "LogWarning");
            sendCommandInternal = Resolvers.ResolveMethod(NetworkBehaviourType, currentAssembly, "SendCommandInternal");
            sendRpcInternal = Resolvers.ResolveMethod(NetworkBehaviourType, currentAssembly, "SendRPCInternal");
            sendTargetRpcInternal = Resolvers.ResolveMethod(NetworkBehaviourType, currentAssembly, "SendTargetRPCInternal");

            InitSyncObjectReference = Resolvers.ResolveMethod(NetworkBehaviourType, currentAssembly, "InitSyncObject");
        }
    }
}
