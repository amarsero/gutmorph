using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Tile3D)), CanEditMultipleObjects]
public class Tile3DInspector : Editor
{
    GUIStyle icons = new GUIStyle();
    HashSet<GameObject> prefabs = new HashSet<GameObject>();
    void OnEnable()
    {
        foreach (var prefab in Resources.LoadAll<GameObject>("Tiles/"))
        {
            if (PrefabUtility.GetPrefabAssetType(prefab) != PrefabAssetType.NotAPrefab)
            {
                prefabs.Add(prefab);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty prop = serializedObject.GetIterator();
        while (prop.NextVisible(true))
        {
            EditorGUILayout.PropertyField(prop);
        }

        GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
        foreach (var prefab in prefabs)
        {
            if (GUILayout.Button(AssetPreview.GetAssetPreview(prefab), icons, GUILayout.Width(96), GUILayout.Height(96)))
            {
                PrefabSelected(prefab);
                GUILayout.EndHorizontal();
                return;
            }
        }
        GUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
                                                                                                                                                             
    private void PrefabSelected(GameObject prefab)
    {
        GameObject[] gameObjects = Selection.gameObjects;
        List<GameObject> newGameObjects = new List<GameObject>(gameObjects.Length);
        foreach (var selected in gameObjects)
        {
            if (selected.GetComponent<Tile3D>() == null) continue;

            GameObject newObject = (GameObject) PrefabUtility.InstantiatePrefab(prefab);

            // -- if for some reason Unity couldn't perform your request, print an error
            if (newObject == null)
            {
                Debug.LogError($"Error instantiating prefab {prefab.name}");
                return;
            }

            // -- set up "undo" features for the new prefab, like setting up the old transform
            
            Undo.RegisterCreatedObjectUndo(newObject, "Replace With Prefabs");
            newObject.transform.parent = selected.transform.parent;
            newObject.transform.localPosition = selected.transform.localPosition;
            newObject.transform.localRotation = selected.transform.localRotation;
            newObject.transform.localScale = selected.transform.localScale;
            newObject.transform.SetSiblingIndex(selected.transform.GetSiblingIndex());
            newGameObjects.Add(newObject);

            // -- now delete the old prefab
            Undo.DestroyObjectImmediate(selected);
        }
        Selection.objects = newGameObjects.ToArray();
    }
}
