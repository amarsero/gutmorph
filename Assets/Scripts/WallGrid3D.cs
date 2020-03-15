using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WallGrid3D : Gridable3D<Wall3D>
{
    public override Vector3 FixToPosition(Vector3 position)
    {
        return position.ToFixedHalfVector3();
    }

    public override IEnumerable<Vector3> GetSizePositions(Vector3 position, Vector3Int size)
    {
        return Grid3D.GetWallSizePositions(position, size);
    }
}
