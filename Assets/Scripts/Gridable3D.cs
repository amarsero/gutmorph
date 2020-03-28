using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
[ExecuteInEditMode]
public class Gridable3D : MonoBehaviour, ISizeable
{
    [SerializeField]
    protected Vector3Int _size;
    public Vector3Int Size
    {
        get => _size;
        set => _size = value;
    }

    public Vector3 GridPosition;

}
