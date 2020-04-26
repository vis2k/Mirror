using UnityEngine;
using Mirror;

namespace Mirror.Weaver.Tests.SyncListNestedInStruct
{
    class SyncListNestedStruct : NetworkBehaviour
    {
        SomeData.SyncList Foo;
    

        public struct SomeData 
        {
            public int usefulNumber;

            public class SyncList : Mirror.SyncList<SomeData> { }
        }
    }
}
