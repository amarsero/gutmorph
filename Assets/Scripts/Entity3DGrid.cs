using System;
using UnityEditor;
using UnityEngine;

public class EntityGrid3D : Gridable3D<Entity3D>
{
    EntityDictionary _entities = new EntityDictionary();

    public override void Refresh()
    {
        _entities.Clear();
        foreach (Entity3D entity in Grid3D.Instance.transform.GetComponentsInChildren<Entity3D>())
        {
            Vector3 pos = entity.transform.position.ToFixedVector3();
            if (_entities.ContainsKey(pos))
            {
                Debug.Log($"2 entitys on the same space at {pos}");
                continue;
            }
            _entities.Add(pos, entity);
        }
        Debug.Log($"There are: {_entities.Count} entities");
    }
    
    //returns new object if successful, else null
    public override Entity3D Add(Vector3 position, GameObject prefab)
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
            return null;
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, Grid3D.Instance.transform);
        go.transform.position = position;

        Entity3D newEntity = go.GetComponent<Entity3D>();

        foreach (Vector3 entityPos in Grid3D.GetTileSizePositions(position, newEntity.Size))
        {
            _entities.Add(entityPos, newEntity);
        }

        return newEntity;
    }

    public override void Remove(Vector3 position)
    {
        position = position.ToFixedVector3();

        if (!_entities.ContainsKey(position))
        {
            Debug.Log("No entity to remove");
            return;
        }

        Entity3D entity = _entities[position];
        foreach (Vector3 entityPos in Grid3D.GetTileSizePositions(position, entity.Size))
        {
            _entities.Remove(entityPos);
        }
        UnityEngine.Object.DestroyImmediate(entity.gameObject);
    }

    public override bool FitsInPosition(Vector3 position, Entity3D component)
    {
        foreach (Vector3 entity in Grid3D.GetTileSizePositions(position, component.Size))
        {
            if (_entities.ContainsKey(entity))
            {
                return false;
            }
        }
        return true;
    }

    public override Entity3D this[Vector3 index]
    {
        get
        {
            _entities.TryGetValue(index, out Entity3D value);
            return value;
        }
        set => _entities[index] = value;
    }
}
