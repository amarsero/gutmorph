using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Tile3D : MonoBehaviour, IGridSelectable
{
    [SerializeField]
    private Vector3Int _size;
    public Vector3Int Size => _size;

    public GameObject GameObject => gameObject;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
