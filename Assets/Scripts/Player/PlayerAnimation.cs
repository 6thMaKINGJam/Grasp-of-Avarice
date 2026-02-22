using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float debugInterval = 0.5f;

    private float _debugTimer;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        bool isMoving = horizontal != 0; 
        animator.SetBool("IsRightWalking", isMoving);
        
        float vertical = Input.GetAxisRaw("Vertical");
        print(vertical);
        bool isClimbingKey = (Mathf.Abs(vertical)!=0f) && animator.GetBool("IsClimbing");
        animator.SetBool("IsClimbingKey", isClimbingKey);
    }
}
