using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseManager : MonoBehaviour
{
    public static MouseManager instance; // static instance

    [SerializeField]
    private InputActionAsset input;

    [SerializeField] LayerMask ground;
    private bool _waiting = true;

    Camera cam;
    Vector3 mousepos;
    Vector2 mouseMovement;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        StartCoroutine(WaitForCamera());

        mousepos = Vector3.zero;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (_waiting)
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, ground))
        {
            mousepos = hit.point;
        }

        mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        Cursor.lockState = UIManager.instance.isElementOpen ? CursorLockMode.Confined : CursorLockMode.Locked;
        Cursor.visible = Cursor.lockState == CursorLockMode.Confined;
      
    }

    public Vector3 GetMousePosition() => mousepos;
    public Vector2 GetMouseMovement() => mouseMovement;

    IEnumerator WaitForCamera()
    {
        while(_waiting)
        {
            if (Camera.main != null)
            {
                cam = Camera.main;
                _waiting = false;
            }

            yield return new WaitForSeconds(0.4f);
        }

    }
}
