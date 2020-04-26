using Mirror;
using Mirror.Weaver.Tests.Extra;

namespace Mirror.Weaver.Tests.GivesErrorForClassWithNoValidConstructor
{
    public class GivesErrorForClassWithNoValidConstructor : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcDoSomething(SomeOtherData data)
        {
            // empty
        }
    }

    public class SomeOtherData
    {
        public int usefulNumber;

        public SomeOtherData(int usefulNumber)
        {
            this.usefulNumber = usefulNumber;
        } 
    }
}
