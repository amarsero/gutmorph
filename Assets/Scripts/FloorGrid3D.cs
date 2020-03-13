using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class FloorGrid3D : Gridable3D<Floor3D>
{
    [SerializeField] 
    private Dictionary<Vector3, Floor3D> _floors = new Dictionary<Vector3, Floor3D>();
    
    public override void Refresh()
    {
        _floors.Clear();
        foreach (Floor3D tile in Grid3D.Instance.transform.GetComponentsInChildren<Floor3D>())
        {
            Vector3 pos = tile.transform.position.ToFixedVector3();
            if (_floors.ContainsKey(pos))
            {
                Debug.Log($"2 floors on the same space at {pos}");
                continue;
            }
            _floors.Add(pos, tile);
        }
        Debug.Log($"There are: {_floors.Count} floors");
    }

    public override void Add(Vector3 position, GameObject prefab)
    {
        position = position.ToFixedVector3();

        Floor3D prefabFloor = prefab.GetComponent<Floor3D>();

        if (prefabFloor == null)
        {
            throw new ArgumentException("gameObject is not a Floor3D");
        }

        if (!FitsInPosition(position, prefab.GetComponent<Floor3D>()))
        {
            Debug.LogWarning("Already a floor in place");
            return;
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, Grid3D.Instance.transform);
        go.transform.position = position;

        Floor3D newFloor = go.GetComponent<Floor3D>();

        foreach (Vector3 floorPos in Grid3D.GetTileSizePositions(position, newFloor.Size))
        {
            _floors.Add(floorPos, newFloor);
        }
    }

    public override void Remove(Vector3 position)
    {
        position = position.ToFixedVector3();

        if (!_floors.ContainsKey(position))
        {
            Debug.Log("No floor to remove");
            return;
        }

        Floor3D tile = _floors[position];
        foreach (Vector3 tilePos in Grid3D.GetTileSizePositions(position, tile.Size))
        {
            _floors.Remove(tilePos);
        }
        UnityEngine.Object.DestroyImmediate(tile.gameObject);
    }

    public override bool FitsInPosition(Vector3 position, Floor3D component)
    {
        foreach (Vector3 floor in Grid3D.GetTileSizePositions(position, component.Size))
        {
            if (_floors.ContainsKey(floor))
            {
                return false;
            }
        }
        return true;
    }
}
