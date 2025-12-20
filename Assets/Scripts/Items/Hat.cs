using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hat : Item
{
    public override void OnPickedUp()
    {
        base.OnPickedUp();
        Debug.Log("소중하고 멋진 모자를 주웠다!");
    }
}
