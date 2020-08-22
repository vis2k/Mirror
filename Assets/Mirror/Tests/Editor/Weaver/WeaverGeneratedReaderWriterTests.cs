using NUnit.Framework;

namespace Mirror.Weaver.Tests
{
    public class WeaverGeneratedReaderWriterTests : WeaverTestsBuildFromTestName
    {
        [SetUp]
        public override void TestSetup()
        {
            WeaverAssembler.AddReferencesByAssemblyName(new string[] { "WeaverTestExtraAssembly.dll" });

            base.TestSetup();
        }

        [Test]
        public void CreatesForStructs()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void CreatesForClass()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void CreatesForClassInherited()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void CreatesForClassWithValidConstructor()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void GivesErrorForClassWithNoValidConstructor()
        {
            HasError("SomeOtherData can't be deserialized because it has no default constructor",
                "GeneratedReaderWriter.GivesErrorForClassWithNoValidConstructor.SomeOtherData");
        }

        [Test]
        public void CreatesForInheritedFromScriptableObject()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void CreatesForStructFromDifferentAssemblies()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void CreatesForClassFromDifferentAssemblies()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void CreatesForClassFromDifferentAssembliesWithValidConstructor()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void CanUseCustomReadWriteForTypesFromDifferentAssemblies()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void GivesErrorWhenUsingUnityAsset()
        {
            HasError("Material can't be deserialized because it has no default constructor",
                "UnityEngine.Material");
        }

        [Test]
        public void GivesErrorWhenUsingObject()
        {
            // TODO: decide if we want to block sending of Object
            // would only want to be send as an arg as a base type for an Inherited object
            HasError("Cannot generate writer for Object. Use a supported type or provide a custom writer",
                "UnityEngine.Object");
            HasError("Cannot generate reader for Object. Use a supported type or provide a custom reader",
                "UnityEngine.Object");
        }

        [Test]
        public void GivesErrorWhenUsingScriptableObject()
        {
            // TODO: decide if we want to block sending of ScripableObject
            // would only want to be send as an arg as a base type for an Inherited object
            HasError("Cannot generate writer for ScriptableObject. Use a supported type or provide a custom writer",
                "UnityEngine.ScriptableObject");
            HasError("Cannot generate reader for ScriptableObject. Use a supported type or provide a custom reader",
                "UnityEngine.ScriptableObject");
        }

        [Test]
        public void GivesErrorWhenUsingMonoBehaviour()
        {
            HasError("Cannot generate writer for component type MonoBehaviour. Use a supported type or provide a custom writer",
                "UnityEngine.MonoBehaviour");
            HasError("Cannot generate reader for component type MonoBehaviour. Use a supported type or provide a custom reader",
                "UnityEngine.MonoBehaviour");
        }

        [Test]
        public void GivesErrorWhenUsingTypeInheritedFromMonoBehaviour()
        {
            HasError("Cannot generate writer for component type MyBehaviour. Use a supported type or provide a custom writer",
                "GeneratedReaderWriter.GivesErrorWhenUsingTypeInheritedFromMonoBehaviour.MyBehaviour");
            HasError("Cannot generate reader for component type MyBehaviour. Use a supported type or provide a custom reader",
                "GeneratedReaderWriter.GivesErrorWhenUsingTypeInheritedFromMonoBehaviour.MyBehaviour");
        }

        [Test]
        public void ExcludesNonSerializedFields()
        {
            // we test this by having a not allowed type in the class, but mark it with NonSerialized
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void GivesErrorWhenUsingInterface()
        {
            HasError("Cannot generate writer for interface IData. Use a supported type or provide a custom writer",
                "GeneratedReaderWriter.GivesErrorWhenUsingInterface.IData");
            HasError("Cannot generate reader for interface IData. Use a supported type or provide a custom reader",
                "GeneratedReaderWriter.GivesErrorWhenUsingInterface.IData");
        }

        [Test]
        public void CanUseCustomReadWriteForInterfaces()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void CreatesForEnums()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void CreatesForArraySegment()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void CreatesForStructArraySegment()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void GivesErrorForJaggedArray()
        {
            Assert.That(weaverErrors, Contains.Item($"Int32[][] is an unsupported type. Jagged and multidimensional arrays are not supported (at System.Int32[][])"));
        }

        [Test]
        public void GivesErrorForMultidimensionalArray()
        {
            Assert.That(weaverErrors, Contains.Item($"Int32[0...,0...] is an unsupported type. Jagged and multidimensional arrays are not supported (at System.Int32[0...,0...])"));
        }

        [Test]
        public void GivesErrorForInvalidArrayType()
        {
            HasError("Cannot generate writer for Array because element MonoBehaviour does not have a writer. Use a supported type or provide a custom writer",
                "UnityEngine.MonoBehaviour[]");
            HasError("Cannot generate reader for Array because element MonoBehaviour does not have a reader. Use a supported type or provide a custom reader",
                "UnityEngine.MonoBehaviour[]");
        }

        [Test]
        public void GivesErrorForInvalidArraySegmentType()
        {
            HasError("Cannot generate writer for ArraySegment because element MonoBehaviour does not have a writer. Use a supported type or provide a custom writer",
                "System.ArraySegment`1<UnityEngine.MonoBehaviour>");
            HasError("Cannot generate reader for ArraySegment because element MonoBehaviour does not have a reader. Use a supported type or provide a custom reader",
                "System.ArraySegment`1<UnityEngine.MonoBehaviour>");
        }

        [Test]
        public void CreatesForList()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void CreatesForStructList()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void GivesErrorForInvalidListType()
        {
            HasError("Cannot generate writer for List because element MonoBehaviour does not have a writer. Use a supported type or provide a custom writer",
                "System.Collections.Generic.List`1<UnityEngine.MonoBehaviour>");
            HasError("Cannot generate reader for List because element MonoBehaviour does not have a reader. Use a supported type or provide a custom reader",
                "System.Collections.Generic.List`1<UnityEngine.MonoBehaviour>");
        }
    }
}
