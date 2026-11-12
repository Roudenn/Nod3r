using System.Collections;

namespace Nod3r.Collections;

/// <summary>
/// A version of <see cref="GenIdStorage{T}"/> that implements <see cref="IEnumerable{T}"/>
/// and allows to iterate through its elements.
/// </summary>
public sealed class EnumerableIdStorage<T> : GenIdStorage<T>, IEnumerable<T>
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }
    
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return new Enumerator(this);
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(this);
    }
    
    /// <summary>
    /// A custom enumerator that returns direct references to stored objects.
    /// </summary>
    public struct Enumerator(GenIdStorage<T> owner) : IEnumerator<T>
    {
        private readonly T[] _data = owner.Data;
        private readonly int[] _dense = owner.Dense;
        private readonly int _count = owner.Count;
        private int _index = -1;
        
        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        public T Current => _data[_dense[_index]];
        
        object? IEnumerator.Current => Current;

        /// <inheritdoc/>
        public bool MoveNext()
        {
            _index++;
            return _index < _count;
        }
        
        /// <inheritdoc/>
        public void Reset() { }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}