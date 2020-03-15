using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gridable3D<T> where T : class, ISizeable
{
    public abstract void Refresh();
    public abstract T Add(Vector3 position, GameObject prefab);
    public abstract void Remove(Vector3 position);
    public abstract bool FitsInPosition(Vector3 position, T component);

    public void Move(Vector3 from, Vector3 to)
    {
        T obj = this[from];
        Vector3 dif = to - from;
        foreach (Vector3 tileSizePosition in Grid3D.GetTileSizePositions(from, obj.Size))
        {
            this[tileSizePosition] = null;
            this[tileSizePosition + dif] = obj;
        }
    }

    public abstract T this[Vector3 index] { get; set; }
}
