using System.Collections;
using UnityEngine;
using TMPro;

public class GoToEnding : MonoBehaviour
{
    [SerializeField] float showDistance = 6f;
    [SerializeField] Transform player;

    [Header("Bubble")]
    [SerializeField] GameObject bubbleRoot;
    [SerializeField] TextMeshProUGUI bubbleText;
    [TextArea] [SerializeField] string message = "이제 모자를 내려둘 때다.";
    [SerializeField] float charInterval = 0.03f;

    bool shownOnce;
    bool endingStarted;

    void Start()
    {
        if (bubbleRoot) bubbleRoot.SetActive(false);
        if (bubbleText) bubbleText.text = "";

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void Update()
    {
        if (shownOnce || player == null) return;

        if (Vector2.Distance(transform.position, player.position) <= showDistance)
        {
            shownOnce = true;

            if (bubbleRoot) bubbleRoot.SetActive(true);
            if (bubbleText) StartCoroutine(TypeRoutine());

            ItemController.Instance?.SetEndingDropPermission(true);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (endingStarted) return;

        Item item = other.GetComponent<Item>();
        if (item == null) return;

        if (item.itemData != null && item.itemData.itemType == ItemType.Hat)
        {
            endingStarted = true;
            StartCoroutine(LoadEndingDelayed());
        }
    }

    IEnumerator TypeRoutine()
    {
        bubbleText.text = "";
        foreach (char c in message)
        {
            bubbleText.text += c;
            yield return new WaitForSeconds(charInterval);
        }
    }

    IEnumerator LoadEndingDelayed()
    {
        yield return new WaitForSeconds(1.5f);
        SceneLoader.Load("End");
    }
}
