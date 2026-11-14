namespace Nod3r.Collections;

public interface IGenStorage<T>
{
    /// <summary>
    /// Sets a slot in the storage to the specified value.
    /// </summary>
    void Set(GenId id, T value);
}