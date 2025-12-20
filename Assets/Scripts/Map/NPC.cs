using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPC : MonoBehaviour
{
    [SerializeField] private GameObject DialoguePanel;      
    [SerializeField] private TextMeshProUGUI DialogueText; 
    [TextArea] [SerializeField] private string message;
    [SerializeField] private int neededCoin = 2;

    private void Awake()
    {
        if (DialoguePanel != null) DialoguePanel.SetActive(false);
        if (DialogueText != null) DialogueText.text = message;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (DialoguePanel != null) DialoguePanel.SetActive(true);
            UpdateDialogueText();
        }
        
        if (other.CompareTag("Coin")){
            Debug.Log("코인에 닿음");
            Destroy(other.gameObject);
            neededCoin -= 1;

            UpdateDialogueText();
        }
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (DialoguePanel != null) DialoguePanel.SetActive(false);
    }
    private void UpdateDialogueText(){
        if (DialogueText != null){
            if (neededCoin > 0){
                DialogueText.text = $"Coin {neededCoin} more needed";
                Debug.Log("Coin " + neededCoin + "개 더 필요");
            }
        }
        else{
            DialogueText.text = "Door Open";
            Debug.Log("문 열림");
        }
    }
}