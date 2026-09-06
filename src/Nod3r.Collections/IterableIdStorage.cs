namespace Nod3r.Collections;

/// <summary>
/// Implementation of <see cref="GenIdStorage{T}"/> with a built-in ref enumerator.
/// </summary>
public sealed class IterableIdStorage<T> : GenIdStorage<T>
{
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

        public bool MoveNext()
        {
            _index++;
            return _index < _count;
        }
            
        public ref T Current => ref _data[_dense[_index]];
    }
}
