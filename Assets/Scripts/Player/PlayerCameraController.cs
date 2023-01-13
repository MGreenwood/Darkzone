using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField]
    private float lookSpeed = 1f;
    private Vector2 lookInput;
    [SerializeField]
    private float maxLookAngle = 70f, minLookAngle = -70f;
    [SerializeField]
    private float distance = 10f, maxDist = 15f, minDist = 8f;

    [SerializeField]
    private Transform camTransform;
    private Camera cam;
    [SerializeField]
    private Rigidbody playerRB;
    [SerializeField]
    private MovementController moveController;
    [SerializeField]
    private Transform anchor;
    private AnimationManager animManager;

    [SerializeField]
    LayerMask cameraLayerCollide;

    private float baseFOV, maxFOV, currentFOV;

    private void Start()
    {
        baseFOV = camTransform.GetComponent<Camera>().fieldOfView;
        cam = camTransform.GetComponent<Camera>();
        baseFOV = cam.fieldOfView;
        maxFOV = baseFOV * 1.3f;
        currentFOV = baseFOV;
    }

    public void Look(InputAction.CallbackContext context)
    {
        lookInput += context.ReadValue<Vector2>();
    }

    public void Zoom(InputAction.CallbackContext context)
    {
        distance -= context.ReadValue<Vector2>().y;
    }

    private void FixedUpdate()
    {
        if (!UIManager.instance.isElementOpen)
        {
            transform.RotateAround(anchor.position, anchor.up, lookSpeed * lookInput.x);
            transform.RotateAround(anchor.position, transform.right, lookSpeed * -lookInput.y);
        }
        AvoidClip();

        // clamp look rotation
        ClampLookRotation();

        lookInput = Vector2.zero;
        transform.position = Vector3.Lerp(transform.position, anchor.position, 0.3f);

        currentFOV = Mathf.Lerp(baseFOV, maxFOV, playerRB.velocity.magnitude / moveController.GetMaxSpeed());
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, currentFOV, 0.1f);
    }

    private void AvoidClip()
    {
        distance = Mathf.Clamp(distance, minDist, maxDist); 
        Vector3 playerToCamDir = (camTransform.position - transform.position).normalized;
        RaycastHit hit;
        if(Physics.Raycast(transform.position, playerToCamDir, out hit, distance, cameraLayerCollide))
        {
            if(Vector3.Distance(transform.position, camTransform.position) > .5f)
                camTransform.position = hit.point + (-playerToCamDir * .1f);
            else
                camTransform.position = hit.point;
        }
        else
        {
            camTransform.position = Vector3.Lerp(camTransform.position, transform.position + playerToCamDir * distance, 0.1f);
        }
    }

    private void ClampLookRotation()
    {
        Vector3 lookRotation = transform.localRotation.eulerAngles;
        lookRotation = new Vector3(ClampAngle(lookRotation.x), lookRotation.y, 0f);
        transform.localRotation = Quaternion.Euler(lookRotation);
    }

    public float ClampAngle(float angle)
    {
        float start = (minLookAngle + maxLookAngle) * 0.5f - 180;
        float floor = Mathf.FloorToInt((angle - start) / 360) * 360;
        minLookAngle += floor;
        maxLookAngle += floor;
        return Mathf.Clamp(angle, minLookAngle, maxLookAngle);
    }
}
