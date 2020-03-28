using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class Grid3D : MonoBehaviour
{
    [SerializeField]
    private GameObject defaultFloorPrefab;
    [SerializeField]
    private GameObject defaultTilePrefab;
    [SerializeField]
    private GameObject defaultWallPrefab;

    public FloorGrid3D FloorGrid = new FloorGrid3D();
    public TileGrid3D TileGrid = new TileGrid3D();
    public WallGrid3D WallGrid = new WallGrid3D();
    public EntityGrid3D EntityGrid = new EntityGrid3D();

    public static Grid3D Instance;

    public static Plane plane;

    [SerializeField]
    private static int _currentLevel;
    private static readonly System.Reactive.Subjects.BehaviorSubject<int> _currentLevelSubject = new BehaviorSubject<int>(0);
    public static IObservable<object> CurrenLevelObservable = _currentLevelSubject.Select(x => (object)x);
    public static int CurrentLevel
    {
        get => _currentLevel;
        set
        {
            _currentLevel = value;
            plane = new Plane(Vector3.up, -value);
            _currentLevelSubject.OnNext(value);
        }
    }

    public bool editTiles;

    private bool insideWindow;

    [SerializeField]
    [HideInInspector]
    public int toolbarSelected = 0;

    private Vector2 mousePos;

    private void OnEnable()
    {
        Instance = this;
        if (!Application.isEditor)
        {
            return;
        }
        SceneView.duringSceneGui += OnScene;
        plane = new Plane(Vector3.up, -CurrentLevel);
        _gizmoColor = new Color(1, 0.92f, 0.15f, 0.2f);

        Refresh();
    }

    public void Refresh()
    {
        EntityGrid.Refresh();
        FloorGrid.Refresh();
        TileGrid.Refresh();
        WallGrid.Refresh();
    }

    public void IncreaseLevel() => CurrentLevel += 1;
    public void DecreaseLevel() => CurrentLevel -= 1;

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

        if (insideWindow && editTiles && Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Grid3D>() != null)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            mousePos = e.mousePosition;
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (MouseOnPlane(mousePos, out Vector3 hitPoint))
                {
                    switch (toolbarSelected)
                    {
                        case 0: //add floor
                            FloorGrid.Add(hitPoint, defaultFloorPrefab);
                            break;
                        case 1: //add tile
                            TileGrid.Add(hitPoint, defaultTilePrefab);
                            break;
                        case 2: //add wall
                            WallGrid.Add(hitPoint, defaultWallPrefab);
                            break;
                        case 3: //remove floor
                            FloorGrid.Remove(hitPoint);
                            break;
                        case 4: //remove tile
                            TileGrid.Remove(hitPoint);
                            break;
                        case 5: //remove wall
                            WallGrid.Remove(hitPoint);
                            break;
                        default:
                            break;
                    }
                }
                e.Use();
                Selection.activeGameObject = Instance.gameObject;
            }
        }
    }

    public static IEnumerable<Vector3> GetTileSizePositions(Vector3 position, Vector3 size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    yield return new Vector3(x, y, z) + position;
                }
            }
        }
    }

    public static IEnumerable<Vector3> GetWallSizePositions(Vector3 position, Vector3 size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    yield return new Vector3(x, y, z) + position;
                }
            }
        }
    }

    //Returns true and the position of the mouse in the plane, else false
    public static bool MouseOnPlane(Vector2 mousePosition, out Vector3 planePosition)
    {
        Camera camera = Camera.current ?? Camera.main;
        Vector3 point = new Vector3(mousePosition.x,
            mousePosition.y);

        if (Camera.current != null)
        {
            point.y = -point.y + camera.pixelHeight;
        }

        var ray = camera.ScreenPointToRay(point);

        if (plane.Raycast(ray, out float enter))
        {
            //Get the point that is clicked 
            planePosition = ray.GetPoint(enter);
            return true;
        }

        planePosition = Vector3.zero;
        return false;
    }

    Color _gizmoColor;
    private readonly float[] _variationsFloats = new[] { -2f, -1f, 0f, 1f };
    private readonly float[] _centerVariations = new float[] { -1f, 0f };

    private void OnDrawGizmos()
    {
        if (!insideWindow || !editTiles || !MouseOnPlane(mousePos, out Vector3 planePos)) return;

        Gizmos.color = _gizmoColor;

        if (true || toolbarSelected == 0 || toolbarSelected == 1 || toolbarSelected == 2 || toolbarSelected == 3)
        {
            foreach (float varX in _variationsFloats)
            {
                foreach (float varZ in _variationsFloats)
                {
                    Vector3 pos = planePos.ToFixedVector3() + new Vector3(varX, 0, varZ);

                    Gizmos.DrawRay(pos, Vector3.back * 2);
                    Gizmos.DrawRay(pos, Vector3.forward * 2);
                    Gizmos.DrawRay(pos, Vector3.left * 2);
                    Gizmos.DrawRay(pos, Vector3.right * 2);
                    if (_centerVariations.Any(x => x == varX) && _centerVariations.Any(z => z == varZ))
                    {
                        Gizmos.DrawRay(pos, Vector3.up);
                    }
                }
            }
        }
    }

    //public GridableGrid3D<Gridable3D> GetGridableGridFor(Type gridable)
    //{
    //    switch (gridable)
    //    {
    //    }
    //}

    public GridableGrid3D<Gridable3D> GetGridableGridFor(Gridable3D gridable)
    {
        switch (gridable)
        {
            case Entity3D entity:
                return (GridableGrid3D<Gridable3D>)EntityGrid;
            case Tile3D tile:
                return (GridableGrid3D<Gridable3D>)TileGrid;
            case Wall3D wall:
                return (GridableGrid3D<Gridable3D>)WallGrid;
            case Floor3D floor:
                return (GridableGrid3D<Gridable3D>)FloorGrid;
            default:
                throw new ArgumentException(nameof(gridable));
            case null:
                throw new ArgumentNullException(nameof(gridable));
        }
    }


    public List<GridableGrid3D<Gridable3D>> GetCollisionLayersForGridable(Gridable3D gridable)
    {
        switch (gridable)
        {
            case Entity3D entity3D:
            case Tile3D tile3D:
                return new List<GridableGrid3D<Gridable3D>> { (GridableGrid3D<Gridable3D>)EntityGrid, (GridableGrid3D<Gridable3D>)TileGrid };
            case Floor3D floor3D:
                return new List<GridableGrid3D<Gridable3D>> { (GridableGrid3D<Gridable3D>)FloorGrid };
            case Wall3D wall3D:
                return new List<GridableGrid3D<Gridable3D>> { (GridableGrid3D<Gridable3D>)WallGrid };
            default:
                throw new ArgumentException(nameof(gridable));
            case null:
                throw new ArgumentNullException(nameof(gridable));
        }
    }
}

public static class Grid3DExtensionMethods
{
    public static Vector3 ToFixedHalfVector3(this Vector3 pos)
    {
        Vector3 final = new Vector3(Mathf.Round(pos.x * 2), Mathf.Round(pos.y * 2), Mathf.Round(pos.z * 2));
        final /= 2;

        float xRem = Math.Abs(pos.x % 1);
        bool xFar = xRem > 0.5f;
        xRem = xFar ? 1 - xRem : xRem;
        float zRem = Math.Abs(pos.z % 1);
        bool zFar = zRem > 0.5f;
        zRem = zFar ? 1 - zRem : zRem;

        if (xRem > 0.25f && zRem > 0.25f || xRem < 0.25f && zRem < 0.25f)
        {
            if (xRem > zRem)
            {
                if (xFar)
                {
                    final.x = final.x - Mathf.Sign(pos.x) * 0.5f;
                }
                else
                {
                    final.x = final.x + Mathf.Sign(pos.x) * 0.5f;
                }
            }
            else
            {
                if (zFar)
                {
                    final.z = final.z - Mathf.Sign(pos.z) * 0.5f;
                }
                else
                {
                    final.z = final.z + Mathf.Sign(pos.z) * 0.5f;
                }
            }
        }

        return final;
    }
    public static Vector3 ToFixedVector3(this Vector3 pos)
    {
        return new Vector3(Mathf.CeilToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.CeilToInt(pos.z));
    }
}

