using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarMovement : MonoBehaviour
{
    [Header("Car Settings")]
    [Tooltip("Speed at which the car will accelerate")]
    public float carSpeed = 50.0f;
    [Tooltip("Speed at which the car will brake")]
    public float brakeSpeed = 500.0f;
    [Tooltip("Max steer angle the car can achieve")]
    public float maxSteerAngle = 30.0f;
   // [Tooltip("Max speed the car can achieve")]
   // public float maxCarSpeed = 200.0f;

    [Header("Car Info")]
    public Transform wheelCollidersParentTransform;
    public List<Transform> wheelTransforms;
    [SerializeField]
    float steeringAngle, horizontalInput, brakeInput, verticalInput = .0f;

    private List<WheelCollider> wheelColliders;
    private enum WheelPosition { FrontRight, RearRight, FrontLeft, RearLeft };


    // Start is called before the first frame update
    private void Start()
    {
        wheelColliders = new List<WheelCollider>();

        for(int i = 0; i < wheelCollidersParentTransform.childCount; i++)
        {
            wheelColliders.Add(wheelCollidersParentTransform.GetChild(i).GetComponent<WheelCollider>());
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        GetInput();
        Steer();
        Accelerate();
        UpdateWheelPoses(); 
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        brakeInput = Input.GetAxis("Jump");
    }

    private void Accelerate()
    {
        wheelColliders[(int)WheelPosition.FrontLeft].motorTorque = verticalInput * carSpeed;
        wheelColliders[(int)WheelPosition.FrontRight].motorTorque = verticalInput * carSpeed;

        wheelColliders[(int)WheelPosition.FrontLeft].brakeTorque = brakeInput * brakeSpeed;
        wheelColliders[(int)WheelPosition.FrontRight].brakeTorque = brakeInput * brakeSpeed;
    }

    private void Steer()
    {
        steeringAngle = maxSteerAngle * horizontalInput;

        wheelColliders[(int)WheelPosition.FrontLeft].steerAngle = steeringAngle;
        wheelColliders[(int)WheelPosition.FrontRight].steerAngle = steeringAngle;
    }

    private void UpdateWheelPoses()
    {
        for (int i = 0; i < wheelTransforms.Count; i++)
        {
            Vector3 pos = wheelTransforms[i].position;
            Quaternion quat = wheelTransforms[i].rotation;

            wheelColliders[i].GetWorldPose(out pos, out quat);
            wheelTransforms[i].position = pos;
            wheelTransforms[i].rotation = quat;
        }
    }
}
