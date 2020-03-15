﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public GameObject SelectedGameObject;

    private GameObject _selectionCursorGO;
    public GameObject _selectionCursorPrefab;
    private Vector3 _offset = new Vector3(0, 0.01f);
    private int layerMask;
    private Soldier _selectedSoldier;

    void Start()
    {
        layerMask = LayerMask.GetMask("Selectable");
        _selectionCursorGO = (GameObject)PrefabUtility.InstantiatePrefab(_selectionCursorPrefab, transform);
    }
    void Update()
    {
        GameObjectSelection();
    }

    private void GameObjectSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        GameObject hitObject = null;

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 1500f, layerMask))
        {
            hitObject = GetSelectable(hitInfo.transform.gameObject);
        }
        if (SelectedGameObject == null)
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
                if (SelectObject(hitObject))
                {
                    MoveSelectionVisual(hitObject.transform.position);
                }
                else
                {
                    if (_selectedSoldier != null)
                    {
                        _selectedSoldier.Move(hitInfo.transform.position);
                    }
                }
            }
            else
            {
                ClearSelection();
            }
        }
    }

    private static GameObject GetSelectable(GameObject hitObject)
    {
        if (hitObject == null) return null;

        IGridSelectable selectable = hitObject.GetComponent<IGridSelectable>();
        if (selectable != null) return selectable.GameObject;

        selectable = hitObject.GetComponentInParent<IGridSelectable>();

        return selectable?.GameObject;
    }
    
    //TODO: Change material of selected object so it glows
    //TODO: Change size of selection cursor according selected object size (And make it smaller on height)
    //TODO: Make movement and rotation adjustments to TileGrid and the rest (So it can move in tile fashion)
    //TODO: Do a ghost of tiles before placing

    //TODO: Player movememt

    private void MoveSelectionVisual(Vector3 planePosition)
    {
        _selectionCursorGO.SetActive(true);
        _selectionCursorGO.transform.position = planePosition.ToFixedVector3() + _offset;
    }

    private void ClearSelection()
    {
        SelectedGameObject = null;
        _selectedSoldier = null;
    }

    /// <summary>
    /// Tries to select an object.
    /// </summary>
    /// <param name="hitObject">GameObject to try to select</param>
    /// <returns>True if successful</returns>
    private bool SelectObject(GameObject hitObject)
    {
        Soldier soldier = hitObject.GetComponent<Soldier>();
        if (soldier != null)
        {
            _selectedSoldier = soldier;
            return true;
        }

        Floor3D floor = hitObject.GetComponent<Floor3D>();
        if (floor != null)
        {
            Entity3D entity3D = Grid3D.Instance.entityGrid[floor.transform.position]?.GetComponent<Soldier>();
            if (entity3D != null)
            {
                _selectedSoldier = (Soldier)entity3D;
                return true;
            }
        }
        return false;
    }

    private void ScaleCursor()
    {
        //if (seLectedObject != null)
        //{
        //    Renderer[] rs = mm.seLectedObject.GetComponentsInChiLdren<Renderer>();
        //    Bounds bigBounds = new Bounds(); //Do not use new Bounds(); Use the first Bounds as base for the others
        //    foreach (Renderer r in rs)
        //    {
        //        bigBounds.EncapsuLate(r.bounds);
        //    }

        //    // This "diameter” only works correctly For relatively circular or square objects
        //    float diameter = bigBounds.size.z;
        //    diameter *= 1.25f;
        //    this.transfbrm.position = bigBounds.Center; //Might wanna not use the y component (height)
        //    this.transfbrm.LocaLScaLe = new Vector3(diameter, 1, diameter);
        //}
    }
}
