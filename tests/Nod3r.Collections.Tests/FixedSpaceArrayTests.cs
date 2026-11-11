namespace Nod3r.Collections.Tests;

[TestFixture, TestOf(typeof(FixedSpaceArray<>))]
public sealed class FixedSpaceArrayTests
{
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(4)]
    [TestCase(8)]
    public void DefaultConstructor_RegistersPowerOfTwoLengthsUpToCapacity(int length)
    {
        var sut = new FixedSpaceArray<int>(capacity: 8, metaCapacity: 2);

        var span = sut.AllocateSpan(length, out _);

        Assert.That(span.Length, Is.EqualTo(length));
    }

    [Test]
    public void AddSupportedCapacity_ReturnsFalseForDuplicateAndTrueForNewLength()
    {
        var sut = new FixedSpaceArray<int>(
            capacity: 2,
            metaCapacity: 1,
            allocatedCapacities: [1, 2]);

        Assert.That(sut.AddSupportedCapacity(1), Is.False);
        Assert.That(sut.AddSupportedCapacity(3), Is.True);
    }

    [Test]
    public void AllocateSpan_UsesRequestedLengthAndGrowsBackingArrayWhenRequired()
    {
        var sut = new FixedSpaceArray<int>(
            capacity: 1,
            metaCapacity: 1,
            allocatedCapacities: [1, 2]);

        var first = sut.AllocateSpan(1, out var firstId);
        first[0] = 123;
        var second = sut.AllocateSpan(2, out _);

        Assert.That(second.Length, Is.EqualTo(2));
        Assert.That(sut.UsedLength, Is.EqualTo(3));
        Assert.That(sut.Capacity, Is.GreaterThanOrEqualTo(3));
        Assert.That(sut.GetSpan(firstId)[0], Is.EqualTo(123),
            "Existing allocations must remain intact after backing-array growth.");
    }

    [Test]
    public void Allocate_RejectsUnsupportedLength()
    {
        var sut = new FixedSpaceArray<int>(
            capacity: 4,
            metaCapacity: 2,
            allocatedCapacities: [1, 2, 4]);

        Assert.Throws<ArgumentException>(() => sut.Allocate(3, out _));
    }

    [Test]
    public void Free_ReusesOnlyBlocksOfTheSameLength()
    {
        var sut = new FixedSpaceArray<int>(
            capacity: 8,
            metaCapacity: 2,
            allocatedCapacities: [1, 2]);

        _ = sut.AllocateSpan(2, out var twoElementId);
        _ = sut.AllocateSpan(1, out _);
        Assert.That(sut.UsedLength, Is.EqualTo(3));

        Assert.That(sut.Free(twoElementId), Is.True);
        _ = sut.AllocateSpan(1, out _);

        Assert.That(sut.UsedLength, Is.EqualTo(4),
            "A one-element allocation must not consume a freed two-element block.");
    }

    [Test]
    public void Free_ThenAllocate_ReusesDataBlockAndMetadataSlotWithNewGeneration()
    {
        var sut = new FixedSpaceArray<int>(
            capacity: 2,
            metaCapacity: 1,
            allocatedCapacities: [2]);

        var original = sut.AllocateSpan(2, out var originalId);
        original[0] = 10;
        Assert.That(sut.Free(originalId), Is.True);

        sut.AllocateSpan(2, out var replacementId);

        Assert.That(sut.UsedLength, Is.EqualTo(2));
        Assert.That(replacementId.Index, Is.EqualTo(originalId.Index));
        Assert.That(replacementId.Generation, Is.Not.EqualTo(originalId.Generation));
        Assert.That(sut.GetSpan(replacementId).Length, Is.EqualTo(2));
    }

    [Test]
    public void RepeatedReuse_DoesNotConsumeNewMetadataIds()
    {
        var sut = new FixedSpaceArray<int>(
            capacity: 1,
            metaCapacity: 1,
            allocatedCapacities: [1]);

        _ = sut.Allocate(1, out var id);
        for (var i = 0; i < 10; i++)
        {
            Assert.That(sut.Free(id), Is.True);
            _ = sut.Allocate(1, out id);
            Assert.That(id.Index, Is.EqualTo(0));
        }

        _ = sut.Allocate(1, out var secondConcurrentId);

        Assert.That(secondConcurrentId.Index, Is.EqualTo(1));
        Assert.That(sut.MetaCapacity, Is.LessThanOrEqualTo(4),
            "Reusing one slot must not make the metadata index grow on every reuse.");
    }

    [Test]
    public void Free_RejectsDoubleFreeAndStaleGeneration()
    {
        var sut = new FixedSpaceArray<int>(
            capacity: 1,
            metaCapacity: 1,
            allocatedCapacities: [1]);
        _ = sut.Allocate(1, out var id);

        Assert.That(sut.Free(id), Is.True);
        Assert.That(sut.Free(id), Is.False);
        Assert.Throws<ArgumentException>(() => sut.Get(id));

        _ = sut.Allocate(1, out var newId);
        Assert.That(sut.Free(id), Is.False,
            "The previous generation must not free the new allocation in the reused slot.");
        Assert.That(sut.Free(newId), Is.True);
    }

    [Test]
    public void Get_RejectsDefaultInvalidId()
    {
        var sut = new FixedSpaceArray<int>();

        Assert.Throws<ArgumentException>(() => sut.Get(default));
    }

    [Test]
    public void Free_ClearsReferenceElementsBeforeBlockReuse()
    {
        var sut = new FixedSpaceArray<object>(
            capacity: 1,
            metaCapacity: 1,
            allocatedCapacities: [1]);
        var first = sut.AllocateSpan(1, out var id);
        first[0] = new object();

        Assert.That(sut.Free(id), Is.True);
        var reused = sut.AllocateSpan(1, out _);

        Assert.That(reused[0], Is.Null);
    }

    [Test]
    public void EnsureCapacity_AndEnsureMetaCapacity_RejectNegativeValues()
    {
        var sut = new FixedSpaceArray<int>();

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.EnsureCapacity(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.EnsureMetaCapacity(-1));
    }

    [Test]
    public void EnsureCapacityAndEnsureMetaCapacity_NeverShrinkExistingArrays()
    {
        var sut = new FixedSpaceArray<int>(capacity: 8, metaCapacity: 4);

        sut.EnsureCapacity(2);
        sut.EnsureMetaCapacity(1);

        Assert.That(sut.Capacity, Is.EqualTo(8));
        Assert.That(sut.MetaCapacity, Is.EqualTo(4));
    }

    [Test]
    public void EnsureCapacityAndEnsureMetaCapacity_GrowToAtLeastRequestedSize()
    {
        var sut = new FixedSpaceArray<int>(capacity: 1, metaCapacity: 1);

        sut.EnsureCapacity(9);
        sut.EnsureMetaCapacity(7);

        Assert.That(sut.Capacity, Is.GreaterThanOrEqualTo(9));
        Assert.That(sut.MetaCapacity, Is.GreaterThanOrEqualTo(7));
    }
}