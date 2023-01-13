using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform playerCam;
    float speed = 0.45f;
    Transform playerTransform;

    private void Start()
    {
        playerTransform = Player.instance.transform;
    }

    private void FixedUpdate()
    {
        transform.position = playerCam.position;

        Vector3 direction = playerTransform.position - transform.position;
        Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, speed);
    }
}
