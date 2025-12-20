using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Scripting;

public class FeatherUI : MonoBehaviour
{
    // Start is called before the first frame update
    public static FeatherUI Instance;
    [SerializeField] private TextMeshProUGUI featherCountText;
    [SerializeField] public int currentFeatherCount = 0;
    private void Awake()
    {
        Instance = this;
    }
    public void UpdateFeatherCount()
    {
        currentFeatherCount++;
        if(featherCountText != null)    featherCountText.text = "Feathers: " + currentFeatherCount.ToString();
        else Debug.LogWarning("FeatherUI: featherCountText is not assigned!");
    }

}
