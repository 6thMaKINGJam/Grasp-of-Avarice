using UnityEngine;
using UnityEngine.SceneManagement;

public class NextMap : MonoBehaviour
{
    private int neededKey = 1;
    [SerializeField] private string nextSceneName;
    private bool canGo = false;

    private void Update()
    {
        if (canGo && neededKey <= 0 && Input.GetKeyDown(KeyCode.W))
        {
            canGo = false;

            if (SpawnManager.Instance != null)
                SpawnManager.Instance.PrepareForNewScene();

            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Key"))
        {
            Debug.Log("키로 문을 열어보쟈");
            Destroy(other.gameObject);
            neededKey--;
        }

        if (other.CompareTag("Player"))
        {
            canGo = true;

            if (neededKey > 0)
                Debug.Log("열쇠가 필요하다!");
            else
                Debug.Log("W 키를 눌러 이동");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            canGo = false;
    }
}
