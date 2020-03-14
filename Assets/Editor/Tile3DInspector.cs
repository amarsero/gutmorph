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
        foreach (GameObject prefab in Resources.LoadAll<GameObject>("Tiles/"))
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

        DrawDefaultInspector();

        GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
        foreach (GameObject prefab in prefabs)
        {
            Texture2D preview = AssetPreview.GetAssetPreview(prefab);
            if (preview == null) continue; //This removes invisible tiles (BaseTile)
            if (GUILayout.Button(preview, icons, GUILayout.Width(96), GUILayout.Height(96)))
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
        foreach (GameObject selected in gameObjects)
        {
            if (selected.GetComponent<Tile3D>() == null) continue;

            int siblingIndex = selected.transform.GetSiblingIndex();
            Vector3 pos = selected.transform.position;
            Grid3D.Instance.tileGrid.Remove(selected.transform.position);

            GameObject newObject = Grid3D.Instance.tileGrid.Add(pos, prefab).gameObject;

            // -- if for some reason Unity couldn't perform your request, print an error
            if (newObject == null)
            {
                Debug.LogError($"Error instantiating prefab {prefab.name}");
                return;
            }

            newObject.transform.SetSiblingIndex(siblingIndex);
            newGameObjects.Add(newObject);

            // -- now delete the old prefab
        }
        Selection.objects = newGameObjects.ToArray();
    }
}
