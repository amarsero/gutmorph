using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class GridableGrid3D<T> where T : Gridable3D
{
    private readonly Dictionary<Vector3, T> _items = new Dictionary<Vector3, T>();
    public void Refresh()
    {
        _items.Clear();
        foreach (T tile in Grid3D.Instance.transform.GetComponentsInChildren<T>())
        {
            Vector3 pos = tile.FixToPosition(tile.transform.position);
            if (_items.ContainsKey(pos))
            {
                Debug.Log($"2 {typeof(T)} on the same space at {pos}");
                continue;
            }
            _items.Add(pos, tile);

        }
        //Debug.Log($"There are: {_items.Count} {typeof(T)}");
    }
    public T Add(Vector3 position, GameObject prefab)
    {
        T prefabItem = prefab.GetComponent<T>();

        position = prefabItem.FixToPosition(position);

        if (prefabItem == null)
        {
            throw new ArgumentException($"gameObject is not a {typeof(T)}");
        }

        if (!FitsInPosition(position, prefab.GetComponent<T>()))
        {
            Debug.LogWarning($"Already a {typeof(T)} in place");
            return null;
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, Grid3D.Instance.transform);
        go.transform.position = position;
        T newItem = go.GetComponent<T>();
        newItem.GridPosition = position;

        foreach (Vector3 itemPos in newItem.GetSizePositions(position, newItem.Size))
        {
            _items.Add(itemPos, newItem);
        }

        return newItem;
    }

    public void Remove(Vector3 position)
    {
        position = position.ToFixedVector3();

        if (!_items.ContainsKey(position))
        {
            Debug.Log($"No {typeof(T)} to remove");
            return;
        }

        T tile = _items[position];
        foreach (Vector3 tilePos in tile.GetSizePositions(position, tile.Size))
        {
            _items.Remove(tilePos);
        }
        UnityEngine.Object.DestroyImmediate(tile.gameObject);
    }

    public bool FitsInPosition(Vector3 position, Gridable3D component)
    {
        foreach (Vector3 item in component.GetSizePositions(position, component.Size))
        {
            if (_items.ContainsKey(item))
            {
                return false;
            }
        }
        return true;
    }

    public void Move(T who, Vector3 to)
    {
        if (who == null)
        {
            Debug.LogWarning($"{typeof(T)} to move is null");
            return;
        }
        to = who.FixToPosition(to);
        Vector3 dif = to - who.GridPosition;
        foreach (Vector3 tileSizePosition in who.GetSizePositions(who.GridPosition, who.Size))
        {
            _items.Remove(tileSizePosition);
            _items.Add(tileSizePosition + dif, who);
        }
        who.GridPosition = to;
    }

    public T this[Vector3 index]
    {
        get
        {
            _items.TryGetValue(index, out T value);
            return value;
        }
        set => _items[index] = value;
    }
}
