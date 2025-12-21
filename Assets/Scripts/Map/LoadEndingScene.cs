using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadEndingScene : MonoBehaviour
{
    [SerializeField] private string endingSceneName = "Ending";
    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        SceneManager.LoadScene(endingSceneName);
    }
}
