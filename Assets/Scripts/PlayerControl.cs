using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public GameObject SelectedGameObject;

    private GameObject _selectionCursorGO;
    public GameObject SelectionCursorPrefab;
    public Material GlowinMaterial;
    private Vector3 _offset = new Vector3(0, 0.01f);
    private int layerMask;
    private Soldier _selectedSoldier;
    private List<Material> _selectedPreviousMaterials;

    void Start()
    {
        layerMask = LayerMask.GetMask("Selectable");
        _selectionCursorGO = (GameObject)PrefabUtility.InstantiatePrefab(SelectionCursorPrefab, transform);
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


    private void MoveSelectionVisual(Vector3 planePosition)
    {
        _selectionCursorGO.SetActive(true);
        _selectionCursorGO.transform.position = planePosition.ToFixedVector3() + _offset;
    }

    private void ClearSelection()
    {
        SelectedGameObject = null;

        if (_selectedPreviousMaterials != null && _selectedSoldier != null)
        {
            Renderer[] renderers = _selectedSoldier.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material = _selectedPreviousMaterials[i];
            }
        }

        _selectedSoldier = null;
    }

    /// <summary>
    /// Tries to select an object.
    /// </summary>
    /// <param name="hitObject">GameObject to try to select</param>
    /// <returns>True if successful</returns>
    private bool SelectObject(GameObject hitObject)
    {
        bool soldierFound = FindSoldier();

        if (!soldierFound) return soldierFound;
        
        Renderer[] renderers = _selectedSoldier.GetComponentsInChildren<Renderer>();
        _selectedPreviousMaterials = new List<Material>(renderers.Length);

        foreach (Renderer renderer in renderers)
        {
            _selectedPreviousMaterials.Add(renderer.material);
            renderer.material = GlowinMaterial;
        }

        _selectedSoldier.GetComponentsInChildren<Renderer>();

        return soldierFound;

        bool FindSoldier()
        {
            Soldier soldier = hitObject.GetComponent<Soldier>();

            if (soldier != null)
            {
                SetSoldier();
                return true;
            }

            Floor3D floor = hitObject.GetComponent<Floor3D>();
            if (floor != null)
            {
                soldier = Grid3D.Instance.EntityGrid[floor.transform.position]?.GetComponent<Soldier>();
                if (soldier != null)
                {
                    SetSoldier();
                    return true;
                }
            }
            return false;

            void SetSoldier()
            {
                ClearSelection();
                _selectedSoldier = soldier;
            }
        }
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
