using System;
using System.Collections;
using UnityEngine;
using Mirror;

namespace Mirror.Weaver.Tests.NetworkBehaviourClientRpcDuplicateName
{
    class NetworkBehaviourClientRpcDuplicateName : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcCantHaveSameName(int abc) {}

        [ClientRpc]
        public void RpcCantHaveSameName(int abc, int def) {}
    }
}
