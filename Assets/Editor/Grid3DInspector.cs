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

    private GUIStyle textGuiStyle = new GUIStyle();

    private string[] toolbarStrings = { "Add Tile", "Remove Tile", "Add Wall", "Remove Wall" };

    void OnEnable()
    {
        // Fetch the objects from the GameObject script to display in the inspector
        gridSize = serializedObject.FindProperty("gridSize");
        prefabTile = serializedObject.FindProperty("prefabTile");
        tiles = serializedObject.FindProperty("tiles");
        currentLevel = serializedObject.FindProperty("_currentLevel");
        plane = serializedObject.FindProperty("plane");
        toolbarSelected = serializedObject.FindProperty("toolbarSelected");

        textGuiStyle.alignment = TextAnchor.MiddleCenter;
        textGuiStyle.fontSize = 14;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

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
        GUILayout.BeginHorizontal();
        toolbarSelected.intValue = GUILayout.Toolbar(toolbarSelected.intValue, toolbarStrings);
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh"))
        {
            ((Grid3D)target).RefreshTiles();
            ((Grid3D)target).RefreshWalls();
        }
        GUILayout.EndHorizontal();


        SerializedProperty prop = serializedObject.GetIterator();
        while (prop.NextVisible(true))
        {
            EditorGUILayout.PropertyField(prop);
        }
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