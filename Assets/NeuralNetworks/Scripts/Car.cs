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

        [Header("Neural Network")]
        public RaySensor raySensor;
        public bool training = false;
        public NNGraph graph;
        NeuralNetwork brain;
        Vector3 startPosition;
        Quaternion startRotation;

        // Start is called before the first frame update
        private void Start()
        {
            startPosition = transform.position;
            startRotation = transform.rotation;

            if (playerControlled)
                Camera.main.gameObject.SetActive(false);
            else
            {
                NeuralNetworkConfiguration NNConfig = GetComponent<NeuralNetworkConfiguration>();
                brain = new NeuralNetwork(NNConfig);
                if (graph != null) graph.Initialize(brain);
            }

            wheelColliders = new List<WheelCollider>();

            for (int i = 0; i < wheelCollidersParentTransform.childCount; i++)
            {
                wheelColliders.Add(wheelCollidersParentTransform.GetChild(i).GetComponent<WheelCollider>());
            }
        }

        private void FixedUpdate()
        {
            if(Input.GetKeyDown(KeyCode.F1))
            {
                transform.position = startPosition;
                transform.rotation = startRotation;

                wheelColliders[(int)WheelPosition.FrontLeft].motorTorque = 0;
                wheelColliders[(int)WheelPosition.FrontRight].motorTorque = 0;

                wheelColliders[(int)WheelPosition.FrontLeft].brakeTorque = 0;
                wheelColliders[(int)WheelPosition.FrontRight].brakeTorque = 0;

                wheelColliders[(int)WheelPosition.FrontLeft].steerAngle = 0;
                wheelColliders[(int)WheelPosition.FrontRight].steerAngle = 0;

                GetComponent<Rigidbody>().velocity = Vector3.zero;
                UpdateWheelPoses();
            }

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
                // output 0 horizontal input - steer
                // output 1 vertical input - accelerate
                float[] input = raySensor.AnalyzeSensors(); //input of 0 means far from collider, input of 1 means close to collider
                float[] output = brain.FeedForward(input); //Analyze sensors and send result as input for the NN
                horizontalInput = output[0];
                verticalInput = output[1];

                if(training)
                {
                    // 5 sensors
                    // sensor 0 right
                    // sensor 1 front-right
                    // sensor 2 front
                    // sensor 3 front-left
                    // sensor 4 left
                    brain.BackPropagate(new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 1 });
                    brain.BackPropagate(new float[] { 1, 0, 0, 0, 0 }, new float[] { 0, 1 });
                    brain.BackPropagate(new float[] { 0, 1, 0, 0, 0 }, new float[] { -1.0f, 0.5f});
                    brain.BackPropagate(new float[] { 0, 0, 1, 0, 0 }, new float[] { 0, -1});
                    brain.BackPropagate(new float[] { 0, 0, 0, 1, 0 }, new float[] { 1.0f, 0.5f});
                    brain.BackPropagate(new float[] { 0, 0, 0, 0, 1 }, new float[] { 0, 1});
                    brain.BackPropagate(new float[] { 1, 1, 0, 0, 0 }, new float[] { -1, 0.5f});
                    brain.BackPropagate(new float[] { 1, 1, 1, 0, 0 }, new float[] { -1, -1});
                    brain.BackPropagate(new float[] { 1, 1, 1, 1, 0 }, new float[] { 0, -1});
                    brain.BackPropagate(new float[] { 0, 0, 0, 1, 1 }, new float[] { 1, 0.5f });
                    brain.BackPropagate(new float[] { 0, 0, 1, 1, 1 }, new float[] { 1, -1});
                    brain.BackPropagate(new float[] { 0, 1, 1, 1, 1 }, new float[] { 0, -1});
                    brain.BackPropagate(new float[] { 1, 0, 0, 0, 1 }, new float[] { 0, 1 });
                    brain.BackPropagate(new float[] { 1, 1, 0, 0, 1 }, new float[] { -1.0f, 0.5f});
                    brain.BackPropagate(new float[] { 1, 1, 1, 0, 1 }, new float[] { -1.0f, -1});
                    brain.BackPropagate(new float[] { 1, 0, 0, 1, 1 }, new float[] { 1.0f, 0.5f });
                    brain.BackPropagate(new float[] { 1, 0, 1, 1, 1 }, new float[] { 1.0f, -1});
                    brain.BackPropagate(new float[] { 1, 1, 0, 1, 1 }, new float[] { 0, 1 });
                    brain.BackPropagate(new float[] { 1, 1, 1, 1, 1 }, new float[] { 0, -1});
                }                  
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

