using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MovementController : MonoBehaviour
{
    private Vector2 input;
    private Rigidbody rb;

    private bool _movementPaused = false;

    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float max_speed = 20f;
    [SerializeField]
    private float slowAmount = 0.04f;
    private float sideSpeedMultiplier = 0.8f;
    private float backSpeedMultiplier = 0.5f;

    [SerializeField]
    private Transform cam;
    [SerializeField]
    private float minTurnSpeed = 0.05f, maxTurnSpeed = 0.4f;

    private bool grounded = false;
    [SerializeField]
    private LayerMask groundLayer;
    [SerializeField]
    private Transform feet;
    [SerializeField]
    private float groundedDistance = 0.4f;
    private float jumpCD = 0.1f;
    private bool jumpOnCD = false;
    [SerializeField]
    private float jumpPower = 100f;

    private AnimationManager animationManager;
    private PlayerAbilities playerAbilities;

    private bool freeCam = false;

    [SerializeField]
    private Quaternion localAvatarRotation;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animationManager = GetComponent<AnimationManager>();
        playerAbilities = GetComponent<PlayerAbilities>();
    }

    private void FixedUpdate()
    {
        grounded = IsGrounded();

        if (grounded && !_movementPaused)
        {
            // sideways movement is slower than forward
            rb.AddForce(force: input.x
                               * sideSpeedMultiplier
                               * speed
                               * transform.right, ForceMode.Acceleration);
            // backward movement is much slower than forward
            rb.AddForce(transform.forward * (input.y > 0f ? speed : speed * backSpeedMultiplier) * input.y, ForceMode.Acceleration);
        }

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, max_speed);

        float velocityTurnModifier = 1f - (rb.velocity.magnitude / max_speed);
        velocityTurnModifier = Mathf.Lerp(minTurnSpeed, maxTurnSpeed, velocityTurnModifier);

        if(!freeCam)
            transform.forward = Vector3.Lerp(transform.forward, Vector3.Cross(cam.right, Vector3.up), velocityTurnModifier);

            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, slowAmount);
    }

    public void MoveInput(InputAction.CallbackContext context)
    {
            input = context.ReadValue<Vector2>();
    }

    public void JumpInput(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started)
        {
            if(grounded && !jumpOnCD)
            {
                // Jump here
                rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                // Inform animation manager
                animationManager.Jump();

                StartCoroutine(JumpCooldown());
            }
        }
    }

    IEnumerator JumpCooldown()
    {
        jumpOnCD = true;

        yield return new WaitForSeconds(jumpCD);
        jumpOnCD = false;
    }

    private bool IsGrounded()
    {
        if (Physics.Raycast(feet.position, Vector3.down, groundedDistance, groundLayer))
        {
            return true;
        }

        return false;
    }

    public bool GetGrounded() => grounded;

    public Vector2 GetInput()
    {
        return input;
    }

    public float GetMaxSpeed() => max_speed;

    public void FreeCamInput(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                    freeCam = true;
                    break;
            case InputActionPhase.Canceled:
                    freeCam = false;
                    break;
        }
    }

    public void MovementPaused(bool paused)
    {
        _movementPaused = paused;
    }

    public Vector3 GetFocus()
    {
        throw new NotImplementedException();

    }

    public void Knockback(Vector3 source, int power)
    {
        throw new NotImplementedException();
    }
}
