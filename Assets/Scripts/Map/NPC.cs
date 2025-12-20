using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPC : MonoBehaviour
{
    [SerializeField] private GameObject DialoguePanel;      
    [SerializeField] private TextMeshProUGUI DialogueText; 
    [TextArea] [SerializeField] private string message;

    private void Awake()
    {
        if (DialoguePanel != null) DialoguePanel.SetActive(false);
        if (DialogueText != null) DialogueText.text = message;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (DialoguePanel != null) DialoguePanel.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (DialoguePanel != null) DialoguePanel.SetActive(false);
    }
}
