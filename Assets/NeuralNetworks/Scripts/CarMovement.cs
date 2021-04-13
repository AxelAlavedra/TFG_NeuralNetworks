using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarMovement : MonoBehaviour
{
    [Header("Car Settings")]
    public bool firstPersonCamera = false;
    [Tooltip("Speed at which the car will accelerate")]
    public float carSpeed = 100.0f;
    [Tooltip("Speed at which the car will brake")]
    public float brakeSpeed = 500.0f;
    [Tooltip("Max steer angle the car can achieve")]
    public float maxSteerAngle = 30.0f;

    [Header("Car Info")]
    [Tooltip("The parent gameObject of the wheels")]
    public Transform wheelCollidersParentTransform;
    [Tooltip("All the wheel transforms of the car")]
    public List<Transform> wheelTransforms;

    private List<WheelCollider> wheelColliders;

    private enum WheelPosition { FrontRight, RearRight, FrontLeft, RearLeft };
    private GameObject mainCamera;
    public Rigidbody rigidBody;


    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main.gameObject;
        rigidBody = GetComponent<Rigidbody>();

        wheelColliders = new List<WheelCollider>();
        for (int i = 0; i < wheelCollidersParentTransform.childCount; i++)
        {
            wheelColliders.Add(wheelCollidersParentTransform.GetChild(i).GetComponent<WheelCollider>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (firstPersonCamera && mainCamera.activeInHierarchy)
            mainCamera.SetActive(false);
        else if (!firstPersonCamera && !mainCamera.activeInHierarchy)
            mainCamera.SetActive(true);
    }

    public void Move(float accelerateInput, float horizontalInput, float brakeInput)
    {
        Accelerate(accelerateInput, brakeInput);
        Steer(horizontalInput);
        UpdateWheelPoses();
    }

    private void Accelerate(float accelerateInput, float brakeInput)
    {
        wheelColliders[(int)WheelPosition.FrontLeft].motorTorque = accelerateInput * carSpeed;
        wheelColliders[(int)WheelPosition.FrontRight].motorTorque = accelerateInput * carSpeed;

        wheelColliders[(int)WheelPosition.FrontLeft].brakeTorque = brakeInput * brakeSpeed;
        wheelColliders[(int)WheelPosition.FrontRight].brakeTorque = brakeInput * brakeSpeed;
    }

    private void Steer(float horizontalInput)
    {
        float steeringAngle = maxSteerAngle * horizontalInput;

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

    public void Reset(Vector3 resetPosition, Quaternion resetRotation)
    {
        transform.position = resetPosition;
        transform.rotation = resetRotation;

        wheelColliders[(int)WheelPosition.FrontLeft].motorTorque = 0;
        wheelColliders[(int)WheelPosition.FrontRight].motorTorque = 0;

        wheelColliders[(int)WheelPosition.FrontLeft].brakeTorque = 0;
        wheelColliders[(int)WheelPosition.FrontRight].brakeTorque = 0;

        wheelColliders[(int)WheelPosition.FrontLeft].steerAngle = 0;
        wheelColliders[(int)WheelPosition.FrontRight].steerAngle = 0;

        rigidBody.velocity = Vector3.zero;
        UpdateWheelPoses();
    }
}
