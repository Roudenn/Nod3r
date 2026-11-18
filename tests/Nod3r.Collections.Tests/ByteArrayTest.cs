namespace Nod3r.Collections.Tests;

[TestFixture]
public sealed class ByteArrayTest
{
    [Test]
    public void TestAssignment()
    {
        var array = new ByteArray(16);
        array[0] = true;
        array[9] = true;
        array[7] = true;
        array[10] = true;
        array[15] = true;
        Assert.That(array[0], Is.EqualTo(true));
        Assert.That(array[9], Is.EqualTo(true));
        Assert.That(array[15], Is.EqualTo(true));
        
        array[0] = false;
        array[5] = false;
        array[8] = false;
        array[9] = false;
        array[15] = false;
        Assert.That(array[0], Is.EqualTo(false));
        Assert.That(array[9], Is.EqualTo(false));
        Assert.That(array[15], Is.EqualTo(false));
    }
}
