using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class PlayEndingScene : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private string nextSceneAfterCutscene = ""; // 비워두면 안 넘어감

    private void Start()
    {
        if (director == null) director = GetComponent<PlayableDirector>();

        if (director != null)
        {
            director.Play();
            if (!string.IsNullOrEmpty(nextSceneAfterCutscene))
                StartCoroutine(CoLoadAfter((float)director.duration));
        }
    }

    private IEnumerator CoLoadAfter(float sec)
    {
        yield return new WaitForSeconds(sec);
        SceneManager.LoadScene(nextSceneAfterCutscene);
    }
}
