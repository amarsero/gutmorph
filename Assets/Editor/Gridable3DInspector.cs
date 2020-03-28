using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

public class Gridable3DInspector : Editor
{
    private readonly string[] _selectionGridPositionsString = new[] {"↺", "X+", "Y+", "Z+", "↻", "X-", "Y-", "Z-"};

    private Gridable3D _gridable;

    protected void OnEnable()
    {
        
    }
    
    public override void OnInspectorGUI()
    {
        _gridable = target as Gridable3D;
        if (target == null) return;

        switch (GUILayout.SelectionGrid(-1, _selectionGridPositionsString, 4))
        {
            case 0: //Rotate left
                Rotate(Vector3.up);
                break;
            case 1: //Move X+
                Move(Vector3.right);
                break;
            case 2: //Move Y+
                Move(Vector3.up);
                break;
            case 3: //Move Z+
                Move(Vector3.forward);
                break;
            case 4: //Rotate Right
                Rotate(Vector3.down);
                break;
            case 5: //Move X-
                Move(Vector3.left);
                break;
            case 6: //Move Y-
                Move(Vector3.down);
                break;
            case 7: //Move Z-
                Move(Vector3.back);
                break;
            default:
                break;
        }
    }

    private void Rotate(Vector3 up)
    {
        GridableGrid3D<Gridable3D> grid = Grid3D.Instance.GetGridableGridFor(_gridable);
        if (grid.Rotate(_gridable, Vector3Int.up))
        {
            _gridable.transform.position = _gridable.GridPosition;
            _gridable.transform.rotation = Quaternion.Euler(_gridable.transform.rotation.eulerAngles + up * 90);
        }
    }

    private void Move(Vector3 relativeMovement)
    {
        GridableGrid3D<Gridable3D> grid = Grid3D.Instance.GetGridableGridFor(_gridable);
        if (grid.Move(_gridable, _gridable.GridPosition + relativeMovement))
        {
            _gridable.transform.position = _gridable.GridPosition;
        }
    }
}
