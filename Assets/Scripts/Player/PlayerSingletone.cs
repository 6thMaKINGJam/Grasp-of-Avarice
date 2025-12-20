using UnityEngine;

public class PlayerSingleton : MonoBehaviour
{
    public static Transform Tr;

    private void Awake()
    {
        if (Tr != null && Tr != transform)
        {
            Destroy(gameObject);
            return;
        }

        Tr = transform;
        DontDestroyOnLoad(gameObject);
    }
}
