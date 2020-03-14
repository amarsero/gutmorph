using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Grid3D)), CanEditMultipleObjects]
public class Grid3DInspector : Editor
{
    private SerializedProperty prefabTile;
    private SerializedProperty gridSize;
    private SerializedProperty tiles;
    private SerializedProperty currentLevel;
    private SerializedProperty toolbarSelected;
    private SerializedProperty plane;

    private Grid3D grid;

    private GUIStyle textGuiStyle = new GUIStyle();

    private string[] toolbarStrings = { "Add Floor", "Add Tile", "Add Wall", "Remove Floor", "Remove Tile", "Remove Wall" };

    void OnEnable()
    {
        // Fetch the objects from the GameObject script to display in the inspector
        gridSize = serializedObject.FindProperty("gridSize");
        prefabTile = serializedObject.FindProperty("prefabTile");
        tiles = serializedObject.FindProperty("tiles");
        currentLevel = serializedObject.FindProperty("_currentLevel");
        plane = serializedObject.FindProperty("plane");
        toolbarSelected = serializedObject.FindProperty("toolbarSelected");
        toolbarStrings = new string[]{ "Add Floor", "Add Tile", "Add Wall", "Remove Floor", "Remove Tile", "Remove Wall" };

        textGuiStyle.alignment = TextAnchor.MiddleCenter;
        textGuiStyle.fontSize = 14;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        grid = Grid3D.Instance;
        //ShowGridSizeButtons();

        //Level
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Level: {Grid3D.CurrentLevel}");
        if (GUILayout.Button("↓"))
        {
            Grid3D.CurrentLevel -= 1;
        }
        if (GUILayout.Button("↑"))
        {
            Grid3D.CurrentLevel += 1;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        //Edit buttons
        grid.toolbarSelected = GUILayout.SelectionGrid(grid.editTiles ? grid.toolbarSelected : -1, toolbarStrings, 3);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh"))
        {
            grid.Refresh();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        DrawDefaultInspector();

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowGridSizeButtons()
    {
        GUILayout.BeginHorizontal();
        // 2 axis
        for (int i = 0; i < 2; i++)
        {
            GUILayout.BeginVertical();

            // + and - of axis
            for (int j = 0; j < 2; j++)
            {
                
                GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(((char)('X'+i)).ToString() + ((char)('+'+j*2)).ToString(), textGuiStyle);
                        GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                        GUILayout.BeginHorizontal();
                            GUILayout.Button("+");
                            GUILayout.Button("-");
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                            GUILayout.Button("↑");
                            GUILayout.Button("↓");
                        GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
    }
}