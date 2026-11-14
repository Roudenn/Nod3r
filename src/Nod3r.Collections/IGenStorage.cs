namespace Nod3r.Collections;

public interface IGenStorage<T>
{
    /// <summary>
    /// Sets a slot in the storage to the specified value.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    void Set(GenId id, T value);
    
    internal void ReturnHandle(StorageHandle<T> handle);
}