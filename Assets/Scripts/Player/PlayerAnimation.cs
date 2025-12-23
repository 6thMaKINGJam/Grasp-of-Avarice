using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

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
        bool isClimbingKey = (vertical != 0) && animator.GetBool("IsClimbing");
        animator.SetBool("IsClimbingKey", isClimbingKey);
    }
}
