namespace Nod3r.Collections;

/// <summary>
/// A simple implementation of an object pool.
/// Allows to reuse heap allocated objects.
/// </summary>
public sealed class ObjectPool<T>
{
    private readonly Stack<T> _data;

    private readonly Func<T> _factory;

    /// <summary>
    /// Creates a new <see cref="ObjectPool{T}"/> instance.
    /// </summary>
    /// <param name="factory">The factory method to construct empty instances of T.</param>
    /// <param name="capacity">Starting capacity of the object pool.</param>
    public ObjectPool(Func<T> factory, int capacity = 4)
    {
        _data = new Stack<T>(capacity);
        _factory = factory;
        for (int i = 0; i < capacity; i++)
        {
            _data.Push(factory());
        }
    }
    
    /// <summary>
    /// Gets an item from the <see cref="ObjectPool{T}"/> or creates
    /// a new one using a factory method.
    /// </summary>
    public T Get()
    {
        if (!_data.TryPop(out var value))
            value = _factory.Invoke();
        
        return value;
    }

    /// <summary>
    /// Returns an item to the pool.
    /// </summary>
    /// <remarks>
    /// The method doesn't do anything to clear it the object;
    /// either ensure that all <see cref="Get"/> calls rewrite
    /// the previous state, or clear it manually before returning.
    /// </remarks>
    public void Return(T value)
    {
        _data.Push(value);
    }
}
