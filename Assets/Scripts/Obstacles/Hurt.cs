using UnityEngine;

public class Hurt : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        var life = collision.collider.GetComponent<PlayerLife>();
        if (life == null) return;

        life.TakeDamage(1);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var life = other.GetComponent<PlayerLife>();
        if (life == null) return;

        life.TakeDamage(1);
    }
}
