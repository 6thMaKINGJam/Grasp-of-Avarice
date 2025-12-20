using UnityEngine;

public class PlayerLife : MonoBehaviour
{
    public int hp = 1;

    public void TakeDamage(int dmg)
    {
        print("Take Damage »£√‚µ ");
        hp -= dmg;
        if (hp <= 0)
            SpawnManager.Instance.Respawn(this);
    }

    public void ResetLifeToOne()
    {
        hp = 1;
    }
}
