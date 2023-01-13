using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    private MovementController moveController;

    private Rigidbody rb;
    private PlayerAbilities.OnAnimationFinished animationFinishedCallback;

    private void Start()
    {
        moveController = GetComponent<MovementController>();
        rb = GetComponent<Rigidbody>();

        StartCoroutine(SetAnimationParameters());
        animator.applyRootMotion = false;
    }

    private IEnumerator SetAnimationParameters()
    {
        while (Application.isPlaying)
        {
            UpdateSpeed();
            UpdateGrounded();

            yield return new WaitForFixedUpdate();
        }

    }

    private void UpdateGrounded()
    {
        animator.SetBool("Grounded", moveController.GetGrounded());
    }

    private void UpdateSpeed()
    {
        animator.SetFloat("Speed", rb.velocity.magnitude);
        Vector3 localSpeed = transform.InverseTransformDirection(rb.velocity);
        float forwardPercentOfMax =  (localSpeed.z / moveController.GetMaxSpeed());
        float rightPercentOfMax =  (localSpeed.x / moveController.GetMaxSpeed());
        animator.SetFloat("ForwardSpeed", forwardPercentOfMax);
        animator.SetFloat("RightSpeed", rightPercentOfMax);
    }

    public void Jump()
    {
        animator.SetTrigger("Jump");
    }

    public void AnimateAbility(PlayerAbilities.AbilityHotkey abilityIndex, PlayerAbilities.OnAnimationFinished animationFinished)
    {
        animationFinishedCallback = animationFinished;

        switch (abilityIndex)
        {
            case PlayerAbilities.AbilityHotkey.PRIMARY:
                animator.SetTrigger("PrimaryAbility");
                break;
            case PlayerAbilities.AbilityHotkey.MOVEMENT:
                animator.SetTrigger("Roll");
                break;
        }
    }

    public void SubscribeToAnimationEnd(PlayerAbilities.OnAnimationFinished subscriber)
    {
        animationFinishedCallback += subscriber;

    }


    public void AnimationFinished()
    {
        animationFinishedCallback.Invoke();
    }
} 
