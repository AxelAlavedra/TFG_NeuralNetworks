using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Axel.NeuralNetworks
{
    public class SLCarAgent : SLAgent
    {
        Vector3 startPosition;
        Quaternion startRotation;
        public RaySensor raySensor;
        public CarMovement carMovement;


        #region AgentFunctions
        public override void OnOutputReceived(float[] output)
        {
            // output 0 horizontal input - steer
            // output 1 vertical input - accelerate
            float horizontalInput = output[0];
            float verticalInput = output[1];

            carMovement.Move(verticalInput, horizontalInput, 0.0f);
        }

        public override void AddObservationsInput(ref float[] input)
        {
            float[] sensorInput = raySensor.AnalyzeSensors(); //input of 0 means far from collider, input of 1 means close to collider
            for(int i = 0; i < sensorInput.Length; i++)
            {
                input[i] = sensorInput[i];
            }

            input[sensorInput.Length] = carMovement.rigidBody.velocity.magnitude;
        }

        public override void AddPlayerInput(ref float[] input)
        {
            input[0] = Input.GetAxis("Horizontal");
            input[1] = Input.GetAxis("Vertical");
            //input[2] = Input.GetAxis("Jump");
        }

        public override void OnReset()
        {
            carMovement.Reset(startPosition, startRotation);
        }
        #endregion

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();

            startPosition = transform.position;
            startRotation = transform.rotation;
        }
    }
}

