using System;
using UnityEditor;
using UnityEngine;

public class EntityGrid3D : Gridable3D<Entity3D>
{
    EntityDictionary entitys = new EntityDictionary();

    public override void Refresh()
    {
        entitys.Clear();
        foreach (Entity3D entity in Grid3D.Instance.transform.GetComponentsInChildren<Entity3D>())
        {
            Vector3 pos = entity.transform.position.ToFixedVector3();
            if (entitys.ContainsKey(pos))
            {
                Debug.Log($"2 entitys on the same space at {pos}");
                continue;
            }
            entitys.Add(pos, entity);
        }
        Debug.Log($"There are: {entitys.Count} entitys");
    }

    public override void Add(Vector3 position, GameObject prefab)
    {
        position = position.ToFixedVector3();

        Entity3D prefabEntity = prefab.GetComponent<Entity3D>();

        if (prefabEntity == null)
        {
            throw new ArgumentException("gameObject is not a Entity3D");
        }

        if (!FitsInPosition(position, prefab.GetComponent<Entity3D>()))
        {
            Debug.LogWarning("Already a entity in place");
            return;
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, Grid3D.Instance.transform);
        go.transform.position = position;

        Entity3D newEntity = go.GetComponent<Entity3D>();

        foreach (Vector3 entityPos in Grid3D.GetTileSizePositions(position, newEntity.Size))
        {
            entitys.Add(entityPos, newEntity);
        }
    }

    public override void Remove(Vector3 position)
    {
        position = position.ToFixedVector3();

        if (!entitys.ContainsKey(position))
        {
            Debug.Log("No entity to remove");
            return;
        }

        Entity3D entity = entitys[position];
        foreach (Vector3 entityPos in Grid3D.GetTileSizePositions(position, entity.Size))
        {
            entitys.Remove(entityPos);
        }
        UnityEngine.Object.DestroyImmediate(entity.gameObject);
    }

    public override bool FitsInPosition(Vector3 position, Entity3D component)
    {
        foreach (Vector3 entity in Grid3D.GetTileSizePositions(position, component.Size))
        {
            if (entitys.ContainsKey(entity))
            {
                return false;
            }
        }
        return true;
    }
}
