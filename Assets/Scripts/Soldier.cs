using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Soldier : Entity3D
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Move(Vector3 position)
    {
        if (!CheckEmptySpace(position))
        {
            return;
        }
        transform.DOMove(position.ToFixedVector3(), 1).SetSpeedBased(true);


    }

    private bool CheckEmptySpace(Vector3 position)
    {
        if (Grid3D.Instance.entityGrid[position] == null && Grid3D.Instance.tileGrid[position] == null)
        {
            return true;
        }

        return false;
    }
}
