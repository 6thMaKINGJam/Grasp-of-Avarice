using UnityEngine;

public class PlayerAnchor : MonoBehaviour
{
    [SerializeField] private Transform target;                 // Player
    [SerializeField] private RectTransform uiAnchor;           // PlayerViewportAnchor
    [SerializeField] private Camera cam;
    [SerializeField] private float smooth = 12f;

    private Vector3 _offsetWorld;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void Start()
    {
        RecalculateOffset();
    }

    private void LateUpdate()
    {
        if (target == null || uiAnchor == null || cam == null) return;

        // 부드럽게 따라가기
        Vector3 desired = target.position + _offsetWorld;
        transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * smooth);
    }

    public void RecalculateOffset()
    {
        if (target == null || uiAnchor == null || cam == null) return;

        // "UI 앵커가 가리키는 화면 좌표"를 월드 좌표로 변환
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, uiAnchor.position);

        // 카메라의 z 거리 기준으로 월드 변환
        float zDist = Mathf.Abs(cam.transform.position.z - target.position.z);
        Vector3 worldAtAnchor = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDist));

        // 플레이어가 앵커 위치에 오도록 하는 카메라 오프셋 계산
        _offsetWorld = cam.transform.position - worldAtAnchor;
        _offsetWorld.z = cam.transform.position.z - target.position.z; 
    }
}
