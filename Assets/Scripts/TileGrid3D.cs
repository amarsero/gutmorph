using System;
using UnityEditor;
using UnityEngine;

public class TileGrid3D : Gridable3D<Tile3D>
{
    TileDictionary _tiles = new TileDictionary();

    public override void Refresh()
    {
        _tiles.Clear();
        foreach (Tile3D tile in Grid3D.Instance.transform.GetComponentsInChildren<Tile3D>())
        {
            Vector3 pos = tile.transform.position.ToFixedVector3();
            if (_tiles.ContainsKey(pos))
            {
                Debug.Log($"2 tiles on the same space at {pos}");
                continue;
            }
            _tiles.Add(pos, tile);
        }
        Debug.Log($"There are: {_tiles.Count} tiles");
    }

    public override Tile3D Add(Vector3 position, GameObject prefab)
    {
        position = position.ToFixedVector3();

        Tile3D prefabTile = prefab.GetComponent<Tile3D>();

        if (prefabTile == null)
        {
            throw new ArgumentException("gameObject is not a Tile3D");
        }

        if (!FitsInPosition(position, prefab.GetComponent<Tile3D>()))
        {
            Debug.LogWarning("Already a tile in place");
            return null;
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, Grid3D.Instance.transform);
        go.transform.position = position;

        Tile3D newTile = go.GetComponent<Tile3D>();

        foreach (Vector3 tilePos in Grid3D.GetTileSizePositions(position, newTile.Size))
        {
            _tiles.Add(tilePos, newTile);
        }

        return newTile;
    }

    public override void Remove(Vector3 position)
    {
        position = position.ToFixedVector3();

        if (!_tiles.ContainsKey(position))
        {
            Debug.Log("No tile to remove");
            return;
        }

        Tile3D tile = _tiles[position];
        foreach (Vector3 tilePos in Grid3D.GetTileSizePositions(position, tile.Size))
        {
            _tiles.Remove(tilePos);
        }
        UnityEngine.Object.DestroyImmediate(tile.gameObject);
    }

    public override bool FitsInPosition(Vector3 position, Tile3D component)
    {
        foreach (Vector3 tile in Grid3D.GetTileSizePositions(position, component.Size))
        {
            if (_tiles.ContainsKey(tile))
            {
                return false;
            }
        }
        return true;
    }

    public override Tile3D this[Vector3 index]
    {
        get
        {
            _tiles.TryGetValue(index, out Tile3D value);
            return value;
        }
        set => _tiles[index] = value;
    }
}
