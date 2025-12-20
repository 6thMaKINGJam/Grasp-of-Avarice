using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Box : Item
{
    [Header("Editor convenience")]
    [SerializeField] private bool ensureEffectorAndRb = true;

    private BoxCollider2D _collider;
    private Rigidbody2D _rb;
    private PlatformEffector2D _effector;

    // 기본 박스 생성 시 설정
    // (itemData를 사용하게 되면 필요함)
    private void Reset()
    {
        // 에디터에서 컴포넌트 추가 시 기본 설정
        if (GetComponent<BoxCollider2D>() == null) // BoxCollider2D
            gameObject.AddComponent<BoxCollider2D>();

        if (ensureEffectorAndRb)
        {
            if (GetComponent<PlatformEffector2D>() == null) // PlatformEffector2D
                gameObject.AddComponent<PlatformEffector2D>();

            if (GetComponent<Rigidbody2D>() == null) // Rigidbody2D
            {
                var rb = gameObject.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;
                rb.freezeRotation = true;
            }
        }
    }

    private void Awake()
    {
        // 필요한 컴포넌트 기본 설정
        _collider = GetComponent<BoxCollider2D>();
        if (_collider == null) _collider = gameObject.AddComponent<BoxCollider2D>();

        // 위에서 아래로만 충돌 처리하도록 설정
        _collider.isTrigger = false;
        _collider.usedByEffector = true;

        _effector = GetComponent<PlatformEffector2D>();
        if (_effector == null)
            _effector = gameObject.AddComponent<PlatformEffector2D>();

        // 기본 설정: 위에서 착지 가능, 아래에서 통과 가능
        _effector.useOneWay = true;
        _effector.useOneWayGrouping = true;
        _effector.surfaceArc = 90f; // 위쪽 사분원 영역에서만 충돌 처리

        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
            _rb = gameObject.AddComponent<Rigidbody2D>();

        // 기본적으로 Static으로 둬서 플레이어에 밀리지 않게 함
        _rb.bodyType = RigidbodyType2D.Static;
        _rb.freezeRotation = true;
    }
}
