using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Wall3D : MonoBehaviour, ISizeable
{
    [SerializeField]
    private Vector3Int _size;
    public Vector3Int Size => _size;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
