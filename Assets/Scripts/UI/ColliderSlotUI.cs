using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ColliderSlotUI : MonoBehaviour
{
    [SerializeField] private GameObject image;

    private void Awake()
    {
        Image[] images = GetComponentsInChildren<Image>(true);
        image = images[0].gameObject;
        print("이미지 연결됨");
    }

    public void SetVisualsActive(bool active)
    {
        if (image != null)
        {
            image.SetActive(active);
        }
    }
}