using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gridable3D : MonoBehaviour, ISizeable
{
    [SerializeField]
    protected Vector3Int _size;
    public Vector3Int Size => _size;
    public Vector3 GridPosition;

    protected void Start()
    {
        GridPosition = FixToPosition(transform.position);
    }
    
    public virtual Vector3 FixToPosition(Vector3 position)
    {
        return position.ToFixedVector3();
    }

    public virtual IEnumerable<Vector3> GetSizePositions(Vector3 position, Vector3Int size)
    {
        return Grid3D.GetTileSizePositions(position, size);
    }
}
