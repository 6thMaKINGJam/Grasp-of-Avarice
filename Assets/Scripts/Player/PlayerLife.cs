using UnityEngine;

public class PlayerLife : MonoBehaviour
{
    public int hp = 1;

    public void TakeDamage(int dmg)
    {
        print("Take Damage ȣ���");
        hp -= dmg;
        if (hp <= 0)
        {
            AudioManager.Instance?.PlaySfx(SfxType.GameOver);
            SpawnManager.Instance.Respawn(this);
        }
    }   

    public void ResetLifeToOne()
    {
        hp = 1;
    }
}
