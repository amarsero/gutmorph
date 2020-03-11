using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
    private GameObject prefabTile;

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
        if (!Application.isEditor)
        {
            return;
        }
        SceneView.duringSceneGui += OnScene;
        plane = new Plane(Vector3.up, -CurrentLevel); 
        gizmoColor = new Color(1, 0.92f, 0.15f, 0.2f);
        variationsFloats = new[] { -2f, -1f, 0f, 1f };
        RefreshTiles();
        RefreshWalls();
    }

    public void RefreshWalls()
    {
        walls.Clear();
        foreach (Wall3D wall in transform.GetComponentsInChildren<Wall3D>())
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
        foreach (Tile3D tile in transform.GetComponentsInChildren<Tile3D>())
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

        if (insideWindow && editTiles && Selection.activeGameObject.GetComponent<Grid3D>() != null)
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
                            AddTileAtPosition(new Vector3(hitPoint.x, hitPoint.y, hitPoint.z).ToFixedVector3Int(), prefabTile);
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
                e.Use();
                Selection.activeGameObject = gameObject;
            }
        }
    }

    private static void RemoveTileAtPosition(Vector3Int position)
    {
        if (!tiles.ContainsKey(position))
        {
            Debug.Log("No tile to remove");
            return;
        }

        Tile3D tile = tiles[position];
        foreach (Vector3Int tilePos in GetTileSizePositions(position,tile.Size))
        {
            tiles.Remove(tilePos);
        }
        DestroyImmediate(tile.gameObject);
    }

    private static void AddTileAtPosition(Vector3Int position, GameObject prefabTile)
    {
        if (!TileFitsInPosition(position, prefabTile.GetComponent<Tile3D>()))
        {
            Debug.LogWarning("Already a tile in place");
            return;
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefabTile, FindObjectOfType<Grid3D>().transform);
        go.transform.position = position;

        Tile3D newTile = go.GetComponent<Tile3D>();
        
        foreach (Vector3Int tilePos in GetTileSizePositions(position, newTile.Size))
        {
            tiles.Add(tilePos, newTile);
        }
    }

    private static bool TileFitsInPosition(Vector3Int position, Tile3D tile3D)
    {
        foreach (Vector3Int tile in GetTileSizePositions(position, tile3D.Size))
        {
            if (tiles.ContainsKey(tile))
            {
                return false;
            }
        }
        return true;
    }

    private static bool WallFitsInPosition(Vector3 position, Wall3D wall3D)
    {
        foreach (Vector3 wall in GetWallSizePositions(position, wall3D.Size))
        {
            if (walls.ContainsKey(wall))
            {
                return false;
            }
        }
        return true;
    }

    private static IEnumerable<Vector3Int> GetTileSizePositions(Vector3Int position, Vector3Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; x < size.y; x++)
            {
                for (int z = 0; x < size.z; x++)
                {
                    yield return new Vector3Int(x,y,z) + position;
                }
            }
        }
    }

    private static IEnumerable<Vector3> GetWallSizePositions(Vector3 position, Vector3Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; x < size.y; x++)
            {
                for (int z = 0; x < size.z; x++)
                {
                    yield return new Vector3(x, y, z) + position;
                }
            }
        }
    }

    private void AddWallAtPosition(Vector3 position)
    {
        if (!WallFitsInPosition(position, wallPrefab.GetComponent<Wall3D>()))
        {
            Debug.Log("Already a wall in place");
            return;
        }

        Quaternion rot = Quaternion.identity;
        if (Math.Abs(position.x % 1f) > 0.2f)
        {
            rot = Quaternion.Euler(0, 90, 0);
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(wallPrefab, transform);
        go.transform.position = position;
        go.transform.rotation = rot;
        
        Wall3D newWall = go.GetComponent<Wall3D>();

        foreach (Vector3 wallPos in GetWallSizePositions(position, newWall.Size))
        {
            walls.Add(wallPos, newWall);
        }

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

        foreach (Vector3 wallPos in GetWallSizePositions(position, wall.Size))
        {
            walls.Remove(wallPos);
        }

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

                if (toolbarSelected == 0 || toolbarSelected == 1 || toolbarSelected == 2 || toolbarSelected == 3)
                {
                    foreach (float varX in variationsFloats)
                    {
                        foreach (float varZ in variationsFloats)
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

        float xRem = Math.Abs(pos.x % 1);
        bool xFar = xRem > 0.5f;
        xRem = xFar ? 1 - xRem : xRem;
        float zRem = Math.Abs(pos.z % 1);
        bool zFar = zRem > 0.5f;
        zRem = zFar ? 1 - zRem : zRem;

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

