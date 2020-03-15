using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Soldier : Entity3D
{

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        //base.Update();
    }

    public void Move(Vector3 position)
    {
        position = position.ToFixedVector3();
        if (!CheckEmptySpace(position))
        {
            return;
        }
        Grid3D.Instance.EntityGrid.Move(this, position);
        transform.DOMove(position, 1).SetSpeedBased(true);
    }

    private bool CheckEmptySpace(Vector3 position)
    {
        return Grid3D.Instance.EntityGrid.FitsInPosition(position, this) && Grid3D.Instance.TileGrid.FitsInPosition(position, this);
    }
}
