using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private GameObject _selectionVisualGO;
    public GameObject _selectionVisualPrefab;
    private Vector3 _offset = new Vector3(0,0.01f);

    void Start()
    {
    }
    void Update()
    {
        if (Grid3D.MouseOnPlane(new Vector2(Input.mousePosition.x,-Input.mousePosition.y + Camera.main.pixelHeight), out Vector3 planePosition))
        {
            MoveSelectionVisual(planePosition);
        }
        if (Input.GetMouseButtonDown(0))
        {
        }
    }

    private void MoveSelectionVisual(Vector3 planePosition)
    {
        if (_selectionVisualGO == null)
        {
            _selectionVisualGO = (GameObject)PrefabUtility.InstantiatePrefab(_selectionVisualPrefab, transform);
        }

        _selectionVisualGO.transform.position = planePosition.ToFixedVector3() + _offset;
    }
}
