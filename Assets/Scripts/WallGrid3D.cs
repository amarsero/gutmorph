using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WallGrid3D : Gridable3D<Wall3D>
{
    private static WallDictionary _walls = new WallDictionary();
    public override void Refresh()
    {
        _walls.Clear();
        foreach (Wall3D wall in Grid3D.Instance.transform.GetComponentsInChildren<Wall3D>())
        {
            Vector3 pos = wall.transform.position.ToFixedHalfVector3();
            if (_walls.ContainsKey(pos))
            {
                Debug.Log($"2 walls on the same space at {pos}");
                continue;
            }
            _walls.Add(pos, wall);
        }
        Debug.Log($"There are: {_walls.Count} walls");
    }

    public override Wall3D Add(Vector3 position, GameObject prefab)
    {
        position = position.ToFixedHalfVector3();

        Wall3D prefabWall = prefab.GetComponent<Wall3D>();

        if (prefabWall == null)
        {
            throw new ArgumentException("gameObject is not a Wall3D");
        }

        if (!FitsInPosition(position, prefab.GetComponent<Wall3D>()))
        {
            Debug.Log("Already a wall in place");
            return null;
        }

        Quaternion rot = Quaternion.identity;
        if (Math.Abs(position.x % 1f) > 0.2f)
        {
            rot = Quaternion.Euler(0, 90, 0);
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, Grid3D.Instance.transform);
        go.transform.position = position;
        go.transform.rotation = rot;

        Wall3D newWall = go.GetComponent<Wall3D>();

        foreach (Vector3 wallPos in Grid3D.GetWallSizePositions(position, newWall.Size))
        {
            _walls.Add(wallPos, newWall);
        }

        return newWall;
    }

    public override void Remove(Vector3 position)
    {
        position = position.ToFixedHalfVector3();

        if (!_walls.ContainsKey(position))
        {
            Debug.Log("No wall to remove");
            return;
        }

        Wall3D wall = _walls[position];

        foreach (Vector3 wallPos in Grid3D.GetWallSizePositions(position, wall.Size))
        {
            _walls.Remove(wallPos);
        }

        UnityEngine.Object.DestroyImmediate(wall.gameObject);
    }

    public override bool FitsInPosition(Vector3 position, Wall3D component)
    {
        foreach (Vector3 wall in Grid3D.GetWallSizePositions(position, component.Size))
        {
            if (_walls.ContainsKey(wall))
            {
                return false;
            }
        }
        return true;
    }

    public override Wall3D this[Vector3 index]
    {
        get
        {
            _walls.TryGetValue(index, out Wall3D value);
            return value;
        }
        set => _walls[index] = value;
    }

}
