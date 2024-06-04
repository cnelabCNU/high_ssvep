using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public Camera Camera2Follow;
    public float CameraDistance = 3.0F;
    public float smoothTime = 0.3F;
    private Vector3 velocity = Vector3.zero;
    private Transform target;

    void Awake()
    {
        target = Camera2Follow.transform;
    }


    void Update()
    {
        // Define my target position in front of the camera ->
        Vector3 targetPosition = target.TransformPoint(new Vector3(0, 0, CameraDistance));

        // Smoothly move my object towards that position ->
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // version 1: my object's rotation is always facing to camera with no dampening  ->
        transform.LookAt(transform.position + Camera2Follow.transform.rotation * Vector3.forward, Camera2Follow.transform.rotation * Vector3.up);

        // version 2 : my object's rotation isn't finished synchronously with the position smooth.damp ->
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target.rotation, 35 * Time.deltaTime);
    }
}
