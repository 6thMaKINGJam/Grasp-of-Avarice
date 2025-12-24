using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPCgem : MonoBehaviour
{
    [SerializeField] private GameObject DialoguePanel;      
    [SerializeField] private TextMeshProUGUI DialogueText; 
    [SerializeField] private GameObject coinWall;
    [TextArea] [SerializeField] private string message;
    [SerializeField] private int neededGem = 5;

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
        
        if (other.CompareTag("Gem")){
            Debug.Log("코인에 닿음");
            Destroy(other.gameObject);
            neededGem -= 1;

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
            if (neededGem > 0){
                DialogueText.text = $"Gem {neededGem} more needed";
                Debug.Log("Gem " + neededGem + "개 더 필요");
            }
            else{
                DialogueText.text = "Door Open";
                Debug.Log("문 열림");
                if (coinWall != null)    Destroy(coinWall);
            }
        }
        
    }
}