using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GridableGrid3D<T> where T : Gridable3D
{
    private Dictionary<Vector3, Gridable3D> _items = new Dictionary<Vector3, Gridable3D>();

    public void Refresh()
    {
        _items.Clear();
        foreach (T tile in Grid3D.Instance.transform.GetComponentsInChildren<T>())
        {
            Vector3 pos = FixToPosition(tile.transform.position);
            if (_items.ContainsKey(pos))
            {
                Debug.Log($"2 {nameof(T)} on the same space at {pos}");
                continue;
            }
            _items.Add(pos, tile);
            tile.GridPosition = pos;
        }
        //Debug.Log($"There are: {_items.Count} {typeof(T)}");
    }
    public virtual T Add(Vector3 position, GameObject prefab)
    {
        T prefabItem = prefab.GetComponent<T>();

        position = FixToPosition(position);

        if (prefabItem == null)
        {
            throw new ArgumentException($"gameObject is not a {nameof(T)}");
        }

        Quaternion newRotation = FixRotation(position);


        if (!FitsInPosition(position, newRotation, prefabItem))
        {
            Debug.LogWarning($"Already a {nameof(T)} in place");
            return null;
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, Grid3D.Instance.transform);
        go.transform.position = position;
        T newItem = go.GetComponent<T>();
        newItem.GridPosition = position;

        foreach (Vector3 itemPos in GetSizePositions(position, newItem.Size))
        {
            _items.Add(itemPos, newItem);
        }

        return newItem;
    }

    public void Remove(Vector3 position)
    {
        position = FixToPosition(position);

        if (!_items.ContainsKey(position))
        {
            Debug.Log($"No {nameof(T)} to remove");
            return;
        }

        T tile = (T)_items[position];
        foreach (Vector3 tilePos in GetSizePositions(position, tile.Size))
        {
            _items.Remove(tilePos);
        }
        UnityEngine.Object.DestroyImmediate(tile.gameObject);
    }

    public bool FitsInPosition(Vector3 position, Quaternion rotation, Gridable3D gridable)
    {
        foreach (Vector3 item in GetSizePositions(position, gridable.Size))
        {
            if (_items.ContainsKey(FixToPosition(item)))
            {
                return false;
            }
        }
        return true;
    }

    public bool FitsInOrContainsItself(Vector3 position, Gridable3D gridable)
    {
        foreach (Vector3 item in GetSizePositions(position, gridable.Size))
        {
            if (!_items.ContainsKey(item)) continue;
            if (_items[item] == gridable) continue;

            return false;
        }
        return true;
    }

    /// <summary>
    /// Tries to move the gridable to position. Returns if it was successful.
    /// </summary>
    /// <param name="gridable"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public bool Move(T gridable, Vector3 to)
    {
        if (gridable == null)
        {
            Debug.LogWarning($"{nameof(T)} to move is null");
            return false;
        }

        to = FixToPosition(to);

        foreach (GridableGrid3D<Gridable3D> gridableGrid3D in Grid3D.Instance.GetCollisionLayersForGridable(gridable))
        {
            if (!gridableGrid3D.FitsInPosition(to, gridable.transform.rotation, gridable))
            {
                Debug.Log($"{nameof(T)} can't move there'");
                return false;
            }
        }

        Vector3 dif = to - gridable.GridPosition;
        foreach (Vector3 tileSizePosition in GetSizePositions(gridable.GridPosition, gridable.Size))
        {
            _items.Remove(tileSizePosition);
            _items.Add(tileSizePosition + dif, gridable);
        }
        gridable.GridPosition = to;

        return true;
    }

    public T this[Vector3 index]
    {
        get
        {
            _items.TryGetValue(index, out Gridable3D value);
            return value as T;
        }
        set => _items[index] = value;
    }

    public static explicit operator GridableGrid3D<Gridable3D>(GridableGrid3D<T> v)
    {
        return new GridableGrid3D<Gridable3D> { _items = v._items };
    }

    public virtual  Vector3 FixToPosition(Vector3 position)
    {
        return position.ToFixedVector3();
    }

    public virtual IEnumerable<Vector3> GetSizePositions(Vector3 position, Vector3 size)
    {
        return Grid3D.GetTileSizePositions(position, size);
    }
    
    /// <summary>
    /// Tries to rotate the gridable around axis. Returns if it was successful.
    /// </summary>
    /// <param name="gridable"></param>
    /// <param name="rotationAxis"></param>
    /// <returns></returns>
    public bool Rotate(Gridable3D gridable, Vector3Int rotationAxis)
    {
        if (gridable == null)
        {
            Debug.LogWarning($"{nameof(T)} to move is null");
            return false;
        }

        Vector3Int newSize = Vector3Int.RoundToInt(Vector3.Cross(gridable.Size, rotationAxis));
        Vector3 newPosition = gridable.GridPosition;

        newSize += gridable.Size * rotationAxis;

        if (newSize.x < 0)
        {
            newSize.x *= -1;
        }
        if (newSize.y < 0)
        {
            newSize.y *= -1;
        }
        if (newSize.z < 0)
        {
            newSize.z *= -1;
        }

        foreach (GridableGrid3D<Gridable3D> gridableGrid3D in Grid3D.Instance.GetCollisionLayersForGridable(gridable))
        {
            if (!gridableGrid3D.FitsInOrContainsItself(newPosition, gridable))
            {
                Debug.Log($"{nameof(T)} can't rotate there'");
                return false;
            }
        }

        foreach (Vector3 tileSizePosition in GetSizePositions(gridable.GridPosition, gridable.Size))
        {
            _items.Remove(tileSizePosition);
        }

        gridable.Size = newSize;
        gridable.GridPosition = newPosition;

        foreach (Vector3 tileSizePosition in GetSizePositions(gridable.GridPosition, gridable.Size))
        {
            _items.Add(tileSizePosition, gridable);
        }

        return true;
    }

    public virtual Quaternion FixRotation(Vector3 position)
    {
        return Quaternion.identity;
    }
}
