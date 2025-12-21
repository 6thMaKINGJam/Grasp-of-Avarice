using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        // 인벤토리 이벤트 구독
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnWeightLevelChanged += UpdateAnimation;
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnWeightLevelChanged -= UpdateAnimation;
    }

    private void UpdateAnimation(int level)
    {
        if (_animator != null)
        {
            // 애니메이터의 WeightLevel 파라미터를 0, 1, 2로 변경
            _animator.SetInteger("WeightLevel", level);
        }
    }
}