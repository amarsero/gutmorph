using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WallGrid3D : GridableGrid3D<Wall3D>
{
    public override IEnumerable<Vector3> GetSizePositions(Vector3 position, Vector3 size)
    {
        return Grid3D.GetWallSizePositions(position, size);
    }

    public override Vector3 FixToPosition(Vector3 position)
    {
        return position.ToFixedHalfVector3();
    }

    public override Quaternion FixRotation(Vector3 position)
    {
        Quaternion rot = Quaternion.identity;
        if (Math.Abs(position.x % 1f) > 0.2f)
        {
            rot = Quaternion.Euler(0, 90, 0);
        }
        return rot;
    }
}
