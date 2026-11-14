#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
namespace Nod3r.Collections.Tests;

[TestFixture]
public sealed class GenIdStorageTests
{
    [Test]
    public void GetUsing_Test()
    {
        var storage = new GenIdStorage<TestStruct>();
        storage.Add(new TestStruct(), out var id);
        StorageHandle<TestStruct> handle;
        using (handle = storage.GetUsing(id))
        {
            handle.Data.Foo1 = 1;
        }
        Assert.That(storage[id].Foo1, Is.EqualTo(1));

        using (handle)
        {
            handle.Data.Foo2 = 2;
        }
        
        Assert.That(storage[id].Foo2, Is.EqualTo(2));
    }

    [Test]
    public void UsingIdStorage_Test()
    {
        var storage = new UsingIdStorage<TestStruct>();
        storage.Add(new TestStruct(), out _);
        storage.Add(new TestStruct(), out _);
        storage.Add(new TestStruct(), out _);
        storage.Add(new TestStruct(), out _);

        foreach (var handle in storage)
        {
            handle.Data.Foo1 = 1;
        }
        
        foreach (var handle in storage)
        {
            Assert.That(handle.Data.Foo1, Is.EqualTo(1));
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
