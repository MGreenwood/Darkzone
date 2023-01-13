using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookController : MonoBehaviour
{
    [SerializeField]
    private float sensitivity = 10f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!UIManager.instance.isElementOpen)
        {
            transform.Rotate(Vector3.up, MouseManager.instance.GetMouseMovement().x * sensitivity, Space.World);
            Vector3 lookRotation = transform.localRotation.eulerAngles;
            lookRotation.x = ClampAngle(lookRotation.x, -40, 40);
            transform.localRotation = Quaternion.Euler(lookRotation);
            
            transform.Rotate(Vector3.right, -MouseManager.instance.GetMouseMovement().y * sensitivity, Space.Self);
        }
    }

    public static float ClampAngle(float current, float min, float max)
    {
        float dtAngle = Mathf.Abs(((min - max) + 180) % 360 - 180);
        float hdtAngle = dtAngle * 0.5f;
        float midAngle = min + hdtAngle;
     
        float offset = Mathf.Abs(Mathf.DeltaAngle(current, midAngle)) - hdtAngle;
        if (offset > 0)
            current = Mathf.MoveTowardsAngle(current, midAngle, offset);
        return current;
    }
}
