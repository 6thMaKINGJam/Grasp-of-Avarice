using UnityEngine;
using UnityEngine.SceneManagement;

public class NextMap : MonoBehaviour
{
    [SerializeField] private string nextSceneName;
    private bool canGo = false;

    private void Update()
    {
        if (canGo && Input.GetKeyDown(KeyCode.W))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canGo = true;
            Debug.Log("W 키를 눌러 이동");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canGo = false;
        }
    }
}
