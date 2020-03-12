using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private Vector3 pos;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Grid3D.MouseOnPlane((Vector2) Input.mousePosition, out Vector3 planePosition))
            {
                pos = planePosition;
                Debug.Log(pos);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(pos, 1);
    }
}
