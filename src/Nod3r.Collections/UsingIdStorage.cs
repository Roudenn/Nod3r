namespace Nod3r.Collections;

/// <summary>
/// A version of <see cref="GenIdStorage{T}"/> with a mutatable enumerator.
/// Instead of elements it returns <see cref="StorageHandle{T}"/>s,
/// which can be mutated to then automatically set the value back on MoveNext() iteration.
/// </summary>
/// <example>
/// <code>
/// var storage = new UsingIdStorage{int}();
/// storage.Add(1);
/// storage.Add(2);
/// storage.Add(3);
///         
/// foreach (var handle in storage)
/// {
///     // After the MoveNext() iteration it will be 1 in the storage
///     handle.Data = 1;
/// }
/// </code>
/// </example>
public sealed class UsingIdStorage<T> : GenIdStorage<T>
{
    /// <summary>
    /// Gets a storage handle enumerator.
    /// </summary>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }
    
    /// <summary>
    /// A custom enumerator that returns handles for stored objects
    /// and sets them back to the storage automatically when an iteration finishes.
    /// </summary>
    public struct Enumerator(GenIdStorage<T> owner)
    {
        private readonly T[] _data = owner.Data;
        private readonly int[] _dense = owner.Dense;
        private readonly uint[] _generations = owner.Generations;
        private readonly ObjectPool<StorageHandle<T>> _handlePool = owner.HandlePool;
        private readonly int _count = owner.Count;
        private int _index = -1;
        
        /// <summary>
        /// RHandle for the current element.
        /// </summary>
        public StorageHandle<T> Current { get; private set; }
        
        // Oh my god, what is this?
        private StorageHandle<T> Next => _handlePool.Get().With(_data[_dense[_index]], new GenId(_dense[_index], _generations[_dense[_index]]));

        /// <summary>
        /// Advances the loop to the next element in the storage.
        /// </summary>
        public bool MoveNext()
        {
            if (_index < 0)
            {
                _index++;
                Current = Next;
                return _index < _count;
            }
            
            Current.Dispose(); // Set the previous element to the storage
            Current = Next;
            return _index++ < _count;
        }
    }
}
