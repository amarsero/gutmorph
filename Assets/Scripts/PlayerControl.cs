using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public GameObject SelectedGameObject;

    private GameObject _selectionCursorGO;
    public GameObject _selectionCursorPrefab;
    private Vector3 _offset = new Vector3(0, 0.01f);

    void Start()
    {
    }
    void Update()
    {
        GameObjectSelection();



    }

    private void GameObjectSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        GameObject hitObject = null;

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            hitObject = hitInfo.transform.parent.gameObject;
        }
        if (SelectedGameObject == null && hitObject != null)
        {
            if (hitObject != null)
            {
                MoveSelectionVisual(hitObject.transform.position);
            }
            else
            {
                _selectionCursorGO.SetActive(false);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (hitObject != null)
            {
                SelectObject(hitObject);
            }
            else
            {
                ClearSelection();
            }
        }
    }

    //TODO: Select entities
    //TODO: Change material of selected object
    //TODO: Change size of selection cursor according selected object size (And make it smaller on height)

    private void MoveSelectionVisual(Vector3 planePosition)
    {
        if (_selectionCursorGO == null)
        {
            _selectionCursorGO = (GameObject)PrefabUtility.InstantiatePrefab(_selectionCursorPrefab, transform);
        }

        _selectionCursorGO.SetActive(true);
        _selectionCursorGO.transform.position = planePosition.ToFixedVector3() + _offset;
    }

    private void ClearSelection()
    {
        SelectedGameObject = null;
    }

    private void SelectObject(GameObject hitObject)
    {
        SelectedGameObject = hitObject;
    }
}
