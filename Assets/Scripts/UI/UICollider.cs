using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICollider : MonoBehaviour
{
    public bool isHittingWall = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Collider"))
        {
            isHittingWall = true;
            print("collider hit");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Collider"))
        {
            isHittingWall = false;
            print("collider hit ≈ª√‚");
        }
    }
}