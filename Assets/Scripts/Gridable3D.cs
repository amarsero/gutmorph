using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class Gridable3D<T> where T : MonoBehaviour, ISizeable
{
    private readonly Dictionary<Vector3, T> _items = new Dictionary<Vector3, T>();
    public void Refresh()
    {
        _items.Clear();
        foreach (T tile in Grid3D.Instance.transform.GetComponentsInChildren<T>())
        {
            Vector3 pos = FixToPosition(tile.transform.position);
            if (_items.ContainsKey(pos))
            {
                Debug.Log($"2 {typeof(T)} on the same space at {pos}");
                continue;
            }
            _items.Add(pos, tile);
        }
        Debug.Log($"There are: {_items.Count} {typeof(T)}");
    }
    public T Add(Vector3 position, GameObject prefab)
    {
        position = FixToPosition(position);

        T prefabItem = prefab.GetComponent<T>();

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

        foreach (Vector3 itemPos in GetSizePositions(position, newItem.Size))
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
        foreach (Vector3 tilePos in GetSizePositions(position, tile.Size))
        {
            _items.Remove(tilePos);
        }
        UnityEngine.Object.DestroyImmediate(tile.gameObject);
    }

    public bool FitsInPosition(Vector3 position, T component)
    {
        foreach (Vector3 item in GetSizePositions(position, component.Size))
        {
            if (_items.ContainsKey(item))
            {
                return false;
            }
        }
        return true;
    }

    public void Move(Vector3 from, Vector3 to)
    {
        T obj = this[from];
        Vector3 dif = to - from;
        foreach (Vector3 tileSizePosition in GetSizePositions(from, obj.Size))
        {
            this[tileSizePosition] = null;
            this[tileSizePosition + dif] = obj;
        }
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

    public virtual Vector3 FixToPosition(Vector3 position)
    {
        return position.ToFixedVector3();
    }

    public virtual IEnumerable<Vector3> GetSizePositions(Vector3 position, Vector3Int size)
    {
        return Grid3D.GetTileSizePositions(position, size);
    }
}
