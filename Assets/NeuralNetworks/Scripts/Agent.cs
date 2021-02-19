using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Axel.NeuralNetworks
{
    /// <summary>
    /// Agent abstract class for Neural Network based behaviours.
    /// </summary>
    [RequireComponent(typeof(NeuralNetworkConfiguration))]
    public class Agent : MonoBehaviour
    {
        [Header("Neural Network")]
        [Tooltip("If the player will control the agent or not")]
        public bool playerControlled = false;
        [Tooltip("If the agent is training or not")]
        public bool training = false;
        [Tooltip("NN Graph for visualization of the Neural Network")]
        public NNGraph graph;
        [Tooltip("The neural network the agent is using")]
        private NeuralNetwork brain;

        //ToDo (Axel): Make NN initialization always be called, without having to call base Start on child class.
        protected virtual void Start()
        {
            NeuralNetworkConfiguration NNConfig = GetComponent<NeuralNetworkConfiguration>();
            brain = new NeuralNetwork(NNConfig);

            // ToDo (Axel): improve UI initialization
            if (graph != null)
                graph.Initialize(brain);
        }

        /// <summary>
        /// Function to add the inputs required for the Neural Network of this behaviour to work.
        /// </summary>
        /// <param name="input">Array to fill with inputs for the NN</param>
        public virtual void AddObservationsInput(ref float[] input)
        {
            throw new System.NotImplementedException("AddInput function was not implemented on child class");
        }

        /// <summary>
        /// Function gets called when an output for the agent behaviour has been obtained. This output can be NN or Player.
        /// </summary>
        /// <param name="output">The output arrray of the Agent</param>
        public virtual void OnOutputReceived(float[] output)
        {
            throw new System.NotImplementedException("OnOutputReceived function was not implemented on child class");
        }

        /// <summary>
        /// Function to add inputs of the Player to the Agent behaviour. OnOutputReceived gets called with this input.
        /// </summary>
        /// <param name="input"></param>
        public virtual void AddPlayerInput(ref float[] input)
        {
            throw new System.NotImplementedException("PlayerInput function was not implemented on child class");
        }

        /// <summary>
        /// Function that gets called when the cycle of training ends, so the Agent can get reset to original state and start training again.
        /// </summary>
        public virtual void OnReset()
        {
            throw new System.NotImplementedException("OnReset function was not implemented on child class");
        }

        public void Train()
        {
            // Backpropagation
            // 5 sensors
            // sensor 0 right
            // sensor 1 front-right
            // sensor 2 front
            // sensor 3 front-left
            // sensor 4 left
            brain.BackPropagate(new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 1 });
            brain.BackPropagate(new float[] { 1, 0, 0, 0, 0 }, new float[] { 0, 1 });
            brain.BackPropagate(new float[] { 0, 1, 0, 0, 0 }, new float[] { -1.0f, 0.5f });
            brain.BackPropagate(new float[] { 0, 0, 1, 0, 0 }, new float[] { 0, -1 });
            brain.BackPropagate(new float[] { 0, 0, 0, 1, 0 }, new float[] { 1.0f, 0.5f });
            brain.BackPropagate(new float[] { 0, 0, 0, 0, 1 }, new float[] { 0, 1 });
            brain.BackPropagate(new float[] { 1, 1, 0, 0, 0 }, new float[] { -1, 0.5f });
            brain.BackPropagate(new float[] { 1, 1, 1, 0, 0 }, new float[] { -1, -1 });
            brain.BackPropagate(new float[] { 1, 1, 1, 1, 0 }, new float[] { 0, -1 });
            brain.BackPropagate(new float[] { 0, 0, 0, 1, 1 }, new float[] { 1, 0.5f });
            brain.BackPropagate(new float[] { 0, 0, 1, 1, 1 }, new float[] { 1, -1 });
            brain.BackPropagate(new float[] { 0, 1, 1, 1, 1 }, new float[] { 0, -1 });
            brain.BackPropagate(new float[] { 1, 0, 0, 0, 1 }, new float[] { 0, 1 });
            brain.BackPropagate(new float[] { 1, 1, 0, 0, 1 }, new float[] { -1.0f, 0.5f });
            brain.BackPropagate(new float[] { 1, 1, 1, 0, 1 }, new float[] { -1.0f, -1 });
            brain.BackPropagate(new float[] { 1, 0, 0, 1, 1 }, new float[] { 1.0f, 0.5f });
            brain.BackPropagate(new float[] { 1, 0, 1, 1, 1 }, new float[] { 1.0f, -1 });
            brain.BackPropagate(new float[] { 1, 1, 0, 1, 1 }, new float[] { 0, 1 });
            brain.BackPropagate(new float[] { 1, 1, 1, 1, 1 }, new float[] { 0, -1 });

            //Todo Reinforced Learning Training
        }

        private void FixedUpdate()
        {
            float[] output = new float[brain.config.outputSize];

            //If the agent is being player controlled, we use the player input as output for the agent behaviour.
            if (playerControlled)
                AddPlayerInput(ref output);
            else
            {
                float[] input = new float[brain.config.inputSize];

                //Gather the input from the agent (sensors and others)
                AddObservationsInput(ref input);

                //Feed the input to the Neural Network to obtain an output
                output = brain.FeedForward(input);
            }

            //Send output to the agent behaviour
            OnOutputReceived(output);

            //If the neural network is training proceed to Train with selected method.
            if (training)
                Train();
        }
    }
}

