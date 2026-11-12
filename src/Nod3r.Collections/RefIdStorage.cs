namespace Nod3r.Collections;

/// <summary>
/// A version of <see cref="GenIdStorage{T}"/> with a ref struct enumerator.
/// </summary>
/// <example>
/// <code>
/// var storage = new RefIdStorage{int}();
/// storage.Add(1);
/// storage.Add(2);
/// storage.Add(3);
///         
/// foreach (var num in storage)
/// {
///     // 'num' value is immutable
/// }
///         
/// foreach (ref var num in storage)
/// {
///     // Declaring as 'ref var'
///     // allows to mutate the elements
///     num++;
/// }
/// </code>
/// </example>
public sealed class RefIdStorage<T> : GenIdStorage<T>
{
    /// <summary>
    /// Gets a ref enumerator of this structure.
    /// </summary>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }
    
    /// <summary>
    /// A custom enumerator that returns direct references to stored objects.
    /// </summary>
    public ref struct Enumerator(GenIdStorage<T> owner)
    {
        private readonly T[] _data = owner.Data;
        private readonly int[] _dense = owner.Dense;
        private readonly int _count = owner.Count;
        private int _index = -1;
        
        /// <summary>
        /// Reference to the current element.
        /// </summary>
        public ref T Current => ref _data[_dense[_index]];

        /// <summary>
        /// Advances the loop to the next element in the storage.
        /// </summary>
        public bool MoveNext()
        {
            return _index++ < _count;
        }
    }
}
