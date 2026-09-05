#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
namespace Nod3r.Collections.Tests.Collections;

[TestFixture]
public sealed class GenIdStorageTests
{
    [Test]
    public void TestIteration()
    {
        var storage = new GenIdStorage<TestStruct>();
    }

    private struct TestStruct
    {
        public int Foo1;
        public int Foo2;
        public int Foo3;
        public int Foo4;
        public int Foo5;
        public int Foo6;
        public int Foo7;
        public int Foo8;
    }
}
