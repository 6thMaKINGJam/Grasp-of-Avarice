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
        bool isMoving = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
        animator.SetBool("IsRightWalking", isMoving);

        bool isClimbingKey = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) && animator.GetBool("IsClimbing");
        animator.SetBool("IsClimbingKey", isClimbingKey);
    }
}
