namespace Nod3r.Collections.Tests;

[TestFixture, TestOf(typeof(SpanList<>))]
public sealed class SpanListTests
{
    [Test]
    public void Add_AppendsUntilCapacityAndThenReturnsFalse()
    {
        var owner = CreateOwner<int>(2);
        var list = owner.Allocate(2, out _);

        Assert.That(list.Add(10), Is.True);
        Assert.That(list.Add(20), Is.True);
        Assert.That(list.Add(30), Is.False);

        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list.Span.ToArray(), Is.EqualTo(new[] { 10, 20 }));
    }

    [Test]
    public void Get_RestoresCountAndContentsOfPreviouslyAllocatedList()
    {
        var owner = CreateOwner<int>(3);
        var list = owner.Allocate(3, out var id);
        Assert.That(list.Add(10), Is.True);
        Assert.That(list.Add(20), Is.True);

        var retrieved = owner.Get(id);

        Assert.That(retrieved.Count, Is.EqualTo(2));
        Assert.That(retrieved.Span.ToArray(), Is.EqualTo(new[] { 10, 20 }));
    }

    [Test]
    public void Remove_FirstElement_ShiftsRemainingElementsAndDecrementsCount()
    {
        var owner = CreateOwner<int>(3);
        var list = owner.Allocate(3, out _);
        AddRange(ref list, 1, 2, 3);

        Assert.That(list.Remove(1), Is.True);

        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list.Span.ToArray(), Is.EqualTo(new[] { 2, 3 }));
    }

    [Test]
    public void Remove_MiddleElement_ShiftsRemainingElementsAndDecrementsCount()
    {
        var owner = CreateOwner<int>(4);
        var list = owner.Allocate(4, out _);
        AddRange(ref list, 1, 2, 3, 4);

        Assert.That(list.Remove(2), Is.True);

        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list.Span.ToArray(), Is.EqualTo(new[] { 1, 3, 4 }));
    }

    [Test]
    public void Remove_LastElement_DecrementsCount()
    {
        var owner = CreateOwner<int>(3);
        var list = owner.Allocate(3, out _);
        AddRange(ref list, 1, 2, 3);

        Assert.That(list.Remove(3), Is.True);

        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list.Span.ToArray(), Is.EqualTo(new[] { 1, 2 }));
    }

    [Test]
    public void Remove_MissingElementReturnsFalseWithoutChangingContents()
    {
        var owner = CreateOwner<int>(3);
        var list = owner.Allocate(3, out _);
        AddRange(ref list, 1, 2, 3);

        Assert.That(list.Remove(99), Is.False);

        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list.Span.ToArray(), Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void Remove_UsesProvidedEqualityComparer()
    {
        var owner = CreateOwner<string>(2);
        var list = owner.Allocate(2, out _);
        Assert.That(list.Add("Alpha"), Is.True);
        Assert.That(list.Add("Beta"), Is.True);

        Assert.That(list.Remove("ALPHA", StringComparer.OrdinalIgnoreCase), Is.True);

        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(list.Span[0], Is.EqualTo("Beta"));
    }

    [Test]
    public void Remove_ClearsVacatedReferenceSlot()
    {
        var owner = CreateOwner<object>(3);
        var list = owner.Allocate(3, out var id);
        var first = new object();
        var middle = new object();
        var last = new object();
        Assert.That(list.Add(first), Is.True);
        Assert.That(list.Add(middle), Is.True);
        Assert.That(list.Add(last), Is.True);

        Assert.That(list.Remove(middle), Is.True);

        var fullAllocation = owner.GetSpan(id);
        Assert.That(fullAllocation[0], Is.SameAs(first));
        Assert.That(fullAllocation[1], Is.SameAs(last));
        Assert.That(fullAllocation[2], Is.Null);
    }

    [Test]
    public void Enumerator_CurrentDoesNotAdvanceAndMoveNextAdvances()
    {
        var owner = CreateOwner<int>(3);
        var list = owner.Allocate(3, out _);
        AddRange(ref list, 10, 20, 30);
        var enumerator = list.GetEnumerator();

        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(10));
        Assert.That(enumerator.Current, Is.EqualTo(10),
            "Reading Current more than once must not advance the enumerator.");

        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(20));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(30));
        Assert.That(enumerator.MoveNext(), Is.False);
    }

    [Test]
    public void Enumerator_SupportsForeachPattern()
    {
        var owner = CreateOwner<int>(3);
        var list = owner.Allocate(3, out _);
        AddRange(ref list, 10, 20, 30);
        var actual = new List<int>();

        foreach (var item in list)
            actual.Add(item);

        Assert.That(actual, Is.EqualTo(new[] { 10, 20, 30 }));
    }

    [Test]
    public void ListHandle_RejectsUseAfterItsAllocationIsFreed()
    {
        var owner = CreateOwner<int>(2);
        var list = owner.Allocate(2, out var id);
        Assert.That(list.Add(1), Is.True);
        Assert.That(owner.Free(id), Is.True);

        Assert.Throws<ArgumentException>(() => list.Contains(1));
    }

    private static FixedSpaceArray<T> CreateOwner<T>(int capacity)
    {
        return new FixedSpaceArray<T>(
            capacity: capacity,
            metaCapacity: 2,
            allocatedCapacities: new[] { capacity });
    }

    private static void AddRange<T>(ref SpanList<T> list, params T[] values)
    {
        foreach (var value in values)
            Assert.That(list.Add(value), Is.True);
    }
}