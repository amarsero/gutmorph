using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Soldier : Entity3D
{
    // Update is called once per frame
    void Update()
    {
        //base.Update();
    }

    public void Move(Vector3 position)
    {
        if (Grid3D.Instance.EntityGrid.Move(this, position))
        {
            transform.DOMove(GridPosition, 1).SetSpeedBased(true);
        }
    }
}
