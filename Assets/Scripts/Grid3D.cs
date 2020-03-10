﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using System.ComponentModel;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine.Tilemaps;


[Serializable]
public class TileDictionary : Dictionary<Vector3Int, Tile3D> { }

[Serializable]
public class WallDictionary : Dictionary<Vector3, Wall3D> { }

[ExecuteInEditMode]
public class Grid3D : MonoBehaviour
{
    [SerializeField]
    private GameObject objeto;

    [SerializeField]
    private GameObject wallPrefab;

    [SerializeField]
    private Vector2Int gridSize;

    private static TileDictionary tiles = new TileDictionary();
    private static WallDictionary walls = new WallDictionary();

    [HideInInspector]
    [SerializeField]
    private Plane plane;
    private Ray ray;

    [SerializeField]
    private int _currentLevel;
    public int CurrentLevel
    {
        get => _currentLevel;
        set { _currentLevel = value; plane = new Plane(Vector3.up, -value); }
    }

    [SerializeField]
    private bool editTiles;

    private bool insideWindow;

    [SerializeField]
    [HideInInspector]
    private int toolbarSelected = 0;

    private Vector2 mousePos;

    private void OnEnable()
    {
        //if (!Application.isEditor)
        //{
        //    Destroy(this);
        //}
        SceneView.duringSceneGui += OnScene;
        plane = new Plane(Vector3.up, -CurrentLevel); 
        gizmoColor = new Color(1, 0.92f, 0.15f, 0.2f);
        variationsFloats = new[] { -2f, -1f, 0f, 1f };
        RefreshTiles();
        RefreshWalls();

    }

    public void RefreshWalls()
    {
        foreach (var wall in transform.GetComponentsInChildren<Wall3D>())
        {
            Vector3 pos = wall.transform.position.ToFixedHalfVector3();
            if (!walls.ContainsKey(pos))
                walls.Add(pos, wall);
        }
        Debug.Log($"There are: {walls.Count} walls");
    }

    public void RefreshTiles()
    {
        tiles.Clear();
        foreach (var tile in transform.GetComponentsInChildren<Tile3D>())
        {
            Vector3Int pos = tile.transform.position.ToFixedVector3Int();
            if (!tiles.ContainsKey(pos))
                tiles.Add(pos, tile);
        }
        Debug.Log($"There are: {tiles.Count} tiles");
    }

    void OnScene(SceneView scene)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseEnterWindow)
        {
            insideWindow = true;
        }
        else if (e.type == EventType.MouseLeaveWindow)
        {
            insideWindow = false;
        }
        //if (e.type != EventType.Layout && e.type != EventType.Repaint && e.type != EventType.MouseMove)
        //    Debug.Log($"insideWindow: {insideWindow}, editTiles:{editTiles}, Selected:{Selection.activeGameObject == gameObject}, e.Type:{e.type}");

        if (insideWindow && editTiles && Selection.activeGameObject == gameObject)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            mousePos = e.mousePosition;
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                ray = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x,
                    -e.mousePosition.y + Camera.current.pixelHeight));

                //Initialise the enter variable

                if (plane.Raycast(ray, out float enter))
                {
                    //Get the point that is clicked 
                    Vector3 hitPoint = ray.GetPoint(enter);

                    switch (toolbarSelected)
                    {
                        case 0: //add tile
                            AddTileAtPosition(new Vector3(hitPoint.x, hitPoint.y, hitPoint.z).ToFixedVector3Int());
                            break;
                        case 1: //remove tile
                            RemoveTileAtPosition(new Vector3(hitPoint.x, hitPoint.y, hitPoint.z).ToFixedVector3Int());
                            break;
                        case 2: //add wall
                            Debug.Log($"Clicked on: {hitPoint}");
                            AddWallAtPosition(new Vector3(hitPoint.x, hitPoint.y, hitPoint.z).ToFixedHalfVector3());
                            break;
                        case 3: //remove wall
                            RemoveWallAtPosition(new Vector3(hitPoint.x, hitPoint.y, hitPoint.z).ToFixedHalfVector3());
                            break;
                        default:
                            break;
                    }
                }
                Selection.activeGameObject = gameObject;
            }
        }
    }

    private void RemoveTileAtPosition(Vector3Int position)
    {
        if (!tiles.ContainsKey(position))
        {
            Debug.Log("No tile to remove");
            return;
        }


        Tile3D tile = tiles[position];
        tiles.Remove(position);
        DestroyImmediate(tile.gameObject);
    }

    private void AddTileAtPosition(Vector3Int position)
    {
        if (tiles.ContainsKey(position))
        {
            Debug.Log("Already a tile in place");
            return;
        }
        
        //Tile3D newTile = Instantiate(objeto, position, Quaternion.identity, transform).GetComponent<Tile3D>();

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(objeto, transform);
        go.transform.position = position;

        Tile3D newTile = go.GetComponent<Tile3D>();
        tiles.Add(position, newTile);
    }

    private void AddWallAtPosition(Vector3 position)
    {
        if (walls.ContainsKey(position))
        {
            Debug.Log("Already a wall in place");
            return;
        }

        Quaternion rot = Quaternion.identity;
        if (position.z % 1f > 0f)
        {
            rot = Quaternion.Euler(0, 90, 0);
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(wallPrefab, transform);
        go.transform.position = position;
        go.transform.rotation = rot;
        
        Wall3D newWall = go.GetComponent<Wall3D>();
        walls.Add(position, newWall);

        Debug.Log($"Wall at {position} rotation: {rot == Quaternion.identity}");
    }

    private void RemoveWallAtPosition(Vector3 position)
    {
        if (!walls.ContainsKey(position))
        {
            Debug.Log("No wall to remove");
            return;
        }


        Wall3D wall = walls[position];
        walls.Remove(position);
        DestroyImmediate(wall.gameObject);
    }

    Color gizmoColor;
    private float[] variationsFloats;
    private float[] centerVariations = new float[] {-1f, 0f};
    void OnDrawGizmos()
    {
        if (insideWindow && editTiles)
        {
            ray = Camera.current.ScreenPointToRay(new Vector3(mousePos.x,
                -mousePos.y + Camera.current.pixelHeight));

            if (plane.Raycast(ray, out float enter))
            {
                //Get the point that is clicked 
                Vector3 hitPoint = ray.GetPoint(enter);
                Gizmos.color = gizmoColor;

                if (toolbarSelected == 0 || toolbarSelected == 1)
                {
                    foreach (var varX in variationsFloats)
                    {
                        foreach (var varZ in variationsFloats)
                        {
                            Vector3 pos = hitPoint.ToFixedVector3Int() + new Vector3(varX,0, varZ);

                            Gizmos.DrawRay(pos, Vector3.back * 2);
                            Gizmos.DrawRay(pos, Vector3.forward * 2);
                            Gizmos.DrawRay(pos, Vector3.left * 2);
                            Gizmos.DrawRay(pos, Vector3.right * 2);
                            if (centerVariations.Any(x => x == varX) && centerVariations.Any(x => x == varZ))
                            {
                                Gizmos.DrawRay(pos, Vector3.up);
                            }
                        }
                    }
                }
            }
        }
    }

}

public static class Grid3DExtensionMethods
{
    public static Vector3 ToFixedHalfVector3(this Vector3 pos)
    {
        Vector3 final = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), Mathf.Round(pos.z));

        float xRem = pos.x % 1;
        bool xFar = xRem > 0.5f;
        xRem = xFar ? 1 - xRem : xRem;
        float zRem = pos.z % 1;
        bool zFar = zRem > 0.5f;
        xRem = zFar ? 1 - zRem : zRem;

        if (xRem > zRem)
        {
            if (xFar)
                final.x = final.x + 0.5f;
            else
                final.x = final.x - 0.5f;

        }
        else
        {
            if (zFar)
                final.z = final.z + 0.5f;
            else
                final.z = final.z - 0.5f;
        }

        return final;
    }

    public static Vector3Int ToFixedVector3Int(this Vector3 pos)
    {
        return new Vector3Int(Mathf.CeilToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.CeilToInt(pos.z));
    }
}





#endif