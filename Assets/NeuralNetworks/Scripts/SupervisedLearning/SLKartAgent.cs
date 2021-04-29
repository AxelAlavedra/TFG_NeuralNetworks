using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Axel.NeuralNetworks
{
    public class SLKartAgent : SLAgent
    {
        Vector3 startPosition;
        Quaternion startRotation;

        [Header("Kart Agent")]
        public RaySensor raySensor;
        public KartMovement kartMovement;
        public CheckpointHandler checkpointHandler;

        #region AgentFunctions
        protected override void OnOutputReceived(float[] output)
        {
            // output 0 horizontal input - steer
            // output 1 vertical input - accelerate
            float horizontalInput = output[0];
            float verticalInput = output[1];

            kartMovement.SetInputs(verticalInput, horizontalInput);
        }

        protected override void AddObservationsInput(ref float[] input)
        {
            float[] sensorInput = raySensor.AnalyzeSensors(); //input of 0 means far from collider, input of 1 means close to collider
            for (int i = 0; i < sensorInput.Length; i++)
            {
                input[i] = sensorInput[i];
            }

            //input[sensorInput.Length] = kartMovement.sphere.velocity.magnitude;
        }

        protected override void AddPlayerInput(ref float[] input)
        {
            input[0] = Input.GetAxis("Horizontal");
            input[1] = Input.GetAxis("Vertical");
        }

        protected override void OnReset()
        {
            kartMovement.Reset(startPosition, startRotation);
        }
        #endregion

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();

            checkpointHandler.AssignStartCheckpoint(out startPosition, out startRotation);
            kartMovement.Reset(startPosition, startRotation);
        }
    }
}

