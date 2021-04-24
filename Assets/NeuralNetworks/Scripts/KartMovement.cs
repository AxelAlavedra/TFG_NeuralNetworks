using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartMovement : MonoBehaviour
{
    public Rigidbody sphere;
    public Transform kartModel;
    public bool firstPersonCamera;

    float speed, currentSpeed;
    float rotate, currentRotate;
    float horizontalInput;

    [Header("Parameters")]
    public float acceleration = 35f;
    public float steering = 23f;
    public float gravity = 10f;
   // public LayerMask layerMask;

    [Header("Model Parts")]
    public Transform frontWheels;
    public Transform backWheels;
    public Transform steeringWheel;

    private GameObject mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main.gameObject;

        Physics.IgnoreCollision(GetComponent<BoxCollider>(), sphere.GetComponent<SphereCollider>(), true);
    }

    void Update()
    {
        if (firstPersonCamera && mainCamera.activeInHierarchy)
            mainCamera.SetActive(false);
        else if (!firstPersonCamera && !mainCamera.activeInHierarchy)
            mainCamera.SetActive(true);
    }

    private void FixedUpdate()
    {
        //Acceleration
        currentSpeed = Mathf.SmoothStep(currentSpeed, speed, Time.fixedDeltaTime * 12.0f);
        currentRotate = Mathf.Lerp(currentRotate, rotate, Time.fixedDeltaTime * 4.0f);

        //Forward Acceleration
        sphere.AddForce(-kartModel.transform.right * currentSpeed, ForceMode.Acceleration);

        //Gravity
        sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
        
        //Follow Collider
        transform.position = sphere.transform.position;

        //Steering
        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, transform.eulerAngles.y + currentRotate, 0), Time.deltaTime * 5f);
    }

    public void Steer(int direction, float amount)
    {
        rotate = (steering * direction) * amount;
    }

    public void SetInputs(float verticalInput, float horizontalInput)
    {
        this.horizontalInput = horizontalInput;

        //Accelerate
        speed = acceleration * verticalInput;

        //Steer
        if (sphere.velocity.magnitude > 0.025f)
            rotate = (steering * (horizontalInput > 0 ? 1 : -1)) * Mathf.Abs(horizontalInput);
        else 
            rotate = 0;

        //Animations
        kartModel.localEulerAngles = Vector3.Lerp(kartModel.localEulerAngles, new Vector3(0, 90 + (horizontalInput * 15), kartModel.localEulerAngles.z), .2f);

        //b) Wheels
        frontWheels.localEulerAngles = new Vector3(0, (horizontalInput * 15), frontWheels.localEulerAngles.z);
        frontWheels.localEulerAngles += new Vector3(0, 0, sphere.velocity.magnitude / 2);
        backWheels.localEulerAngles += new Vector3(0, 0, sphere.velocity.magnitude / 2);

        //c) Steering Wheel
        steeringWheel.localEulerAngles = new Vector3(-25, 90, ((horizontalInput * 45)));
    }

    public void Reset(Vector3 startPosition, Quaternion startRotation)
    {
        speed = currentSpeed = rotate = currentRotate = 0.0f;
        horizontalInput = 0.0f;

        sphere.velocity = Vector3.zero;

        transform.position = sphere.position = startPosition;
        transform.rotation = sphere.rotation = startRotation;
    }
}
