using NUnit.Framework;

namespace Mirror.Weaver.Tests
{
    // Some tests for SyncObjects are in WeaverSyncListTests and apply to SyncDictionary too
    public class WeaverSyncDictionaryTests : WeaverTestsBuildFromTestName
    {
        [Test]
        public void SyncDictionary()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void SyncDictionaryGenericAbstractInheritance()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void SyncDictionaryGenericInheritance()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void SyncDictionaryInheritance()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void SyncDictionaryStructKey()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void SyncDictionaryStructItem()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void SyncDictionaryStructKeyWithCustomDeserializeOnly()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void SyncDictionaryStructItemWithCustomDeserializeOnly()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void SyncDictionaryStructKeyWithCustomMethods()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void SyncDictionaryStructItemWithCustomMethods()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void SyncDictionaryStructKeyWithCustomSerializeOnly()
        {
            Assert.That(weaverErrors, Is.Empty);
        }


        [Test]
        public void SyncDictionaryStructItemWithCustomSerializeOnly()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void SyncDictionaryErrorForGenericStructKey()
        {
            HasError("Can not create Serialize or Deserialize for generic element in MyGenericStructDictionary. Override virtual methods with custom Serialize and Deserialize to use WeaverSyncDictionaryTests.SyncDictionaryErrorForGenericStructKey.SyncDictionaryErrorForGenericStructKey/MyGenericStruct`1<System.Single> in SyncList",
                "WeaverSyncDictionaryTests.SyncDictionaryErrorForGenericStructKey.SyncDictionaryErrorForGenericStructKey/MyGenericStructDictionary");
        }

        [Test]
        public void SyncDictionaryErrorForGenericStructItem()
        {
            HasError("Can not create Serialize or Deserialize for generic element in MyGenericStructDictionary. Override virtual methods with custom Serialize and Deserialize to use WeaverSyncDictionaryTests.SyncDictionaryErrorForGenericStructItem.SyncDictionaryErrorForGenericStructItem/MyGenericStruct`1<System.Single> in SyncList",
                "WeaverSyncDictionaryTests.SyncDictionaryErrorForGenericStructItem.SyncDictionaryErrorForGenericStructItem/MyGenericStructDictionary");
        }

        [Test]
        public void SyncDictionaryErrorForGenericStructKeyWithCustomDeserializeOnly()
        {
            HasError("Can not create Serialize or Deserialize for generic element in MyGenericStructDictionary. Override virtual methods with custom Serialize and Deserialize to use WeaverSyncDictionaryTests.SyncDictionaryErrorForGenericStructKeyWithCustomDeserializeOnly.SyncDictionaryErrorForGenericStructKeyWithCustomDeserializeOnly/MyGenericStruct`1<System.Single> in SyncList",
                "WeaverSyncDictionaryTests.SyncDictionaryErrorForGenericStructKeyWithCustomDeserializeOnly.SyncDictionaryErrorForGenericStructKeyWithCustomDeserializeOnly/MyGenericStructDictionary");
        }

        [Test]
        public void SyncDictionaryErrorForGenericStructItemWithCustomDeserializeOnly()
        {
            HasError("Can not create Serialize or Deserialize for generic element in MyGenericStructDictionary. Override virtual methods with custom Serialize and Deserialize to use WeaverSyncDictionaryTests.SyncDictionaryErrorForGenericStructItemWithCustomDeserializeOnly.SyncDictionaryErrorForGenericStructItemWithCustomDeserializeOnly/MyGenericStruct`1<System.Single> in SyncList",
                "WeaverSyncDictionaryTests.SyncDictionaryErrorForGenericStructItemWithCustomDeserializeOnly.SyncDictionaryErrorForGenericStructItemWithCustomDeserializeOnly/MyGenericStructDictionary");
        }

        [Test]
        public void SyncDictionaryErrorForGenericStructKeyWithCustomSerializeOnly()
        {
            HasError("Can not create Serialize or Deserialize for generic element in MyGenericStructDictionary. Override virtual methods with custom Serialize and Deserialize to use MyGenericStruct`1 in SyncList",
                "WeaverSyncDictionaryTests.SyncDictionaryErrorForGenericStructKeyWithCustomSerializeOnly.SyncDictionaryErrorForGenericStructKeyWithCustomSerializeOnly/MyGenericStructDictionary");
        }

        [Test]
        public void SyncDictionaryErrorForGenericStructItemWithCustomSerializeOnly()
        {
            HasError("Can not create Serialize or Deserialize for generic element in MyGenericStructDictionary. Override virtual methods with custom Serialize and Deserialize to use MyGenericStruct`1 in SyncList",
                "WeaverSyncDictionaryTests.SyncDictionaryErrorForGenericStructItemWithCustomSerializeOnly.SyncDictionaryErrorForGenericStructItemWithCustomSerializeOnly/MyGenericStructDictionary");
        }

        [Test]
        public void SyncDictionaryGenericStructKeyWithCustomMethods()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void SyncDictionaryGenericStructItemWithCustomMethods()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void SyncDictionaryErrorWhenUsingGenericInNetworkBehaviour()
        {
            HasError("Cannot use generic SyncObject someDictionary directly in NetworkBehaviour. Create a class and inherit from the generic SyncObject instead",
                "WeaverSyncDictionaryTests.SyncDictionaryErrorWhenUsingGenericInNetworkBehaviour.SyncDictionaryErrorWhenUsingGenericInNetworkBehaviour/SomeSyncDictionary`2<System.Int32,System.String> WeaverSyncDictionaryTests.SyncDictionaryErrorWhenUsingGenericInNetworkBehaviour.SyncDictionaryErrorWhenUsingGenericInNetworkBehaviour::someDictionary");
        }
    }
}
