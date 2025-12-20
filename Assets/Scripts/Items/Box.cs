using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : Item
{
    public override void OnPicked(GameObject picker) // OnPicked ±¸Çö
    {
        Destroy(gameObject);
    }
}
