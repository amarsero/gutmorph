using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gridable3D<T>
{
    public abstract void Refresh();
    public abstract T Add(Vector3 position, GameObject prefab);
    public abstract void Remove(Vector3 position);
    public abstract bool FitsInPosition(Vector3 position, T component); 

}
