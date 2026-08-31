namespace Nod3r.Collections.Tests.Collections;

[TestFixture]
public sealed class FlatArrayTests
{
    [Test]
    public void TestIteration()
    {
        var storage = new GenIdStorage<TestStruct>();
        storage.Allocate(out var nodeId) = new TestStruct();
        storage.Free(nodeId);
        foreach (ref var test in storage)
        {
            
        }
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
