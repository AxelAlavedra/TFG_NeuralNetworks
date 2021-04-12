using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Axel.NeuralNetworks
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarAgent : Agent
    {
        [Header("Car Settings")]
        public bool firstPersonCamera = false;
        [Tooltip("Speed at which the car will accelerate")]
        public float carSpeed = 50.0f;
        [Tooltip("Speed at which the car will brake")]
        public float brakeSpeed = 500.0f;
        [Tooltip("Max steer angle the car can achieve")]
        public float maxSteerAngle = 30.0f;
        //[Tooltip("Max speed the car can achieve")]
        //public float maxCarSpeed = 200.0f;
        Vector3 startPosition;
        Quaternion startRotation;
        public RaySensor raySensor;

        [Header("Car Info")]
        [Tooltip("The parent gameObject of the wheels")]
        public Transform wheelCollidersParentTransform;
        [Tooltip("All the wheel transforms of the car")]
        public List<Transform> wheelTransforms;
        [SerializeField]
        float horizontalInput, brakeInput, verticalInput = .0f;

        private List<WheelCollider> wheelColliders;

        private enum WheelPosition { FrontRight, RearRight, FrontLeft, RearLeft };
        private GameObject mainCamera;
        private Rigidbody rigidBody;


        #region AgentFunctions
        public override void OnOutputReceived(float[] output)
        {
            // output 0 horizontal input - steer
            // output 1 vertical input - accelerate
            horizontalInput = output[0];
            verticalInput = output[1];

            Steer();
            Accelerate();
            UpdateWheelPoses();
        }

        public override void AddObservationsInput(ref float[] input)
        {
            float[] sensorInput = raySensor.AnalyzeSensors(); //input of 0 means far from collider, input of 1 means close to collider
            for(int i = 0; i < sensorInput.Length; i++)
            {
                input[i] = sensorInput[i];
            }

            input[sensorInput.Length] = rigidBody.velocity.magnitude;
        }

        public override void AddPlayerInput(ref float[] input)
        {
            input[0] = Input.GetAxis("Horizontal");
            input[1] = Input.GetAxis("Vertical");
            //input[2] = Input.GetAxis("Jump");
        }

        public override void OnReset()
        {
            transform.position = startPosition;
            transform.rotation = startRotation;

            wheelColliders[(int)WheelPosition.FrontLeft].motorTorque = 0;
            wheelColliders[(int)WheelPosition.FrontRight].motorTorque = 0;

            wheelColliders[(int)WheelPosition.FrontLeft].brakeTorque = 0;
            wheelColliders[(int)WheelPosition.FrontRight].brakeTorque = 0;

            wheelColliders[(int)WheelPosition.FrontLeft].steerAngle = 0;
            wheelColliders[(int)WheelPosition.FrontRight].steerAngle = 0;

            rigidBody.velocity = Vector3.zero;
            UpdateWheelPoses();
        }
        #endregion

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();

            mainCamera = Camera.main.gameObject;
            rigidBody = GetComponent<Rigidbody>();

            startPosition = transform.position;
            startRotation = transform.rotation;

            wheelColliders = new List<WheelCollider>();
            for (int i = 0; i < wheelCollidersParentTransform.childCount; i++)
            {
                wheelColliders.Add(wheelCollidersParentTransform.GetChild(i).GetComponent<WheelCollider>());
            }
        }

        private void Update()
        {
            if (firstPersonCamera && mainCamera.activeInHierarchy)
                mainCamera.SetActive(false);
            else if(!firstPersonCamera && !mainCamera.activeInHierarchy)
                mainCamera.SetActive(true);
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
    }
}

