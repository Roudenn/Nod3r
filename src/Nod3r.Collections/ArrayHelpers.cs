namespace Nod3r.Collections;

public static class ArrayHelpers
{
    /// <summary>
    /// Ensures that an array has a specified capacity.
    /// If the length of the array is less than capacity,
    /// it's resized to fit that capacity.
    /// </summary>
    /// <param name="array">The target array.</param>
    /// <param name="capacity">Minimal capacity to ensure for this array.</param>
    /// <returns>New array length.</returns>
    public static int EnsureArrayCapacity<T>(ref T[] array, int capacity)
    {
        if ((uint) capacity < (uint) array.Length)
            return array.Length;
        
        Array.Resize(ref array, Math.Max(Math.Max(array.Length, 2) * 2, capacity));
        return array.Length;
    }
    
    /// <summary>
    /// Creates a new instance of a double nested array,
    /// where each sub-array in the main array is initialized with a specified capacity.
    /// </summary>
    /// <param name="capacity">Capacity of the first layer (amount of sub-arrays).</param>
    /// <param name="layerCapacity">Capacity of second layers, (sizes of all sub-arrays)</param>
    /// <param name="array">The result array.</param>
    public static void InitArray<T>(int capacity, int layerCapacity, out T[][] array)
    {
        array = new T[capacity][];
        for (int i = 0; i < capacity; i++)
        {
            array[i] = new T[layerCapacity];
        }
    }
    
    /// <summary>
    /// Resizes a double nested array and initializes new arrays from start to end.
    /// </summary>
    /// <param name="array">The target double-nested array.</param>
    /// <param name="capacity">New length of the nested array.</param>
    /// <param name="layerCapacity">Length of every new sub-array.</param>
    public static void ResizeCapacity<T>(ref T[][] array, int capacity, int layerCapacity)
    {
        int startIdx = array.Length;
        Array.Resize(ref array, capacity);
        for (int i = startIdx; i < capacity; i++)
        {
            array[i] = new T[layerCapacity];
        }
    }
}
