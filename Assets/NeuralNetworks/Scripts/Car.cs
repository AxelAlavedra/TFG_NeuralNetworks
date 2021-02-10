using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Axel.NeuralNetworks
{
    [RequireComponent(typeof(Rigidbody))]
    public class Car : MonoBehaviour
    {
        [Header("Car Settings")]
        [Tooltip("If the player will control the car or not")]
        public bool playerControlled = false;
        [Tooltip("Speed at which the car will accelerate")]
        public float carSpeed = 50.0f;
        [Tooltip("Speed at which the car will brake")]
        public float brakeSpeed = 500.0f;
        [Tooltip("Max steer angle the car can achieve")]
        public float maxSteerAngle = 30.0f;
        //[Tooltip("Max speed the car can achieve")]
        //public float maxCarSpeed = 200.0f;

        [Header("Car Info")]
        [Tooltip("The parent gameObject of the wheels")]
        public Transform wheelCollidersParentTransform;
        [Tooltip("All the wheel transforms of the car")]
        public List<Transform> wheelTransforms;
        [SerializeField]
        float steeringAngle, horizontalInput, brakeInput, verticalInput = .0f;

        private List<WheelCollider> wheelColliders;
        private enum WheelPosition { FrontRight, RearRight, FrontLeft, RearLeft };

        NeuralNetwork brain;
        public RaySensor raySensor;

        // Start is called before the first frame update
        private void Start()
        {
            if (playerControlled)
                Camera.main.gameObject.SetActive(false);
            else
            {
                NeuralNetworkConfiguration NNConfig = GetComponent<NeuralNetworkConfiguration>();
                brain = new NeuralNetwork(NNConfig);
            }

            wheelColliders = new List<WheelCollider>();

            for (int i = 0; i < wheelCollidersParentTransform.childCount; i++)
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
            if (playerControlled)
            {
                horizontalInput = Input.GetAxis("Horizontal");
                verticalInput = Input.GetAxis("Vertical");
                brakeInput = Input.GetAxis("Jump");
            }
            else
            {
                // Use NN to obtain input
                float[] output = brain.FeedForward(raySensor.AnalyzeSensors()); //Analyze sensors and send result as input for the NN
                horizontalInput = output[0];
                verticalInput = output[1];
            }
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
}

