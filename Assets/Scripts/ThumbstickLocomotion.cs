using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OVR;

public class ThumbstickLocomotion : MonoBehaviour
{
    public float speed;

    private Rigidbody player;

    void Start()
    {
        player = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Vector2 joystickAxisL = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick, OVRInput.Controller.LTouch);
        player.position += (transform.right * joystickAxisL.x + transform.forward * joystickAxisL.y) * Time.deltaTime * speed;
    }
}
