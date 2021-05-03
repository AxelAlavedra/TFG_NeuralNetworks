using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Axel.NeuralNetworks
{
    /// <summary>
    /// Agent abstract class for Neural Network based behaviours.
    /// </summary>
    public class SLAgent : MonoBehaviour
    {
        [Header("Agent")]
        [Tooltip("If the player will control the agent or not")]
        public bool playerControlled = false;
        [Tooltip("If the agent is training or not")]
        public bool training = false;
        [Tooltip("If the agent actions are recorder or not")]
        public bool recordActions = false;
        [Tooltip("The neural network the agent is using")]
        public NeuralNetwork brain = null;
        [Tooltip("The data file used for training")]
        public TextAsset dataFile = null;
        [Tooltip("The epochs performed for training")]
        public int epochs = 10000;

        bool save = false;

        [System.Serializable]
        public struct DataPacket
        {
            public float[] input;
            public float[] output;
        }
        [JsonProperty]
        List<DataPacket> data;

        protected virtual void Start()
        {
            if (brain != null)
            {
                string filePath = Application.dataPath + "/NeuralNetworks/NN/SupervisedLearning/Brains/" + brain.config.identifier + ".json";
                if(System.IO.File.Exists(filePath))
                {
                    string json = System.IO.File.ReadAllText(filePath);
                    brain = JsonConvert.DeserializeObject<NeuralNetwork>(json);
                    training = false;
                } 
                else
                {
                    brain.Init();
                }
            }

            if (recordActions)
                data = new List<DataPacket>();

            //If the neural network is training proceed to Train with selected method.
            if (training)
                Train();
        }

        /// <summary>
        /// Function to add the inputs required for the Neural Network of this behaviour to work.
        /// </summary>
        /// <param name="input">Array to fill with inputs for the NN</param>
        protected virtual void AddObservationsInput(ref float[] input)
        {
            throw new System.NotImplementedException("AddInput function was not implemented on child class");
        }

        /// <summary>
        /// Function gets called when an output for the agent behaviour has been obtained. This output can be NN or Player.
        /// </summary>
        /// <param name="output">The output arrray of the Agent</param>
        protected virtual void OnOutputReceived(float[] output)
        {
            throw new System.NotImplementedException("OnOutputReceived function was not implemented on child class");
        }

        /// <summary>
        /// Function to add inputs of the Player to the Agent behaviour. OnOutputReceived gets called with this input.
        /// </summary>
        /// <param name="input"></param>
        protected virtual void AddPlayerInput(ref float[] input)
        {
            throw new System.NotImplementedException("PlayerInput function was not implemented on child class");
        }

        /// <summary>
        /// Function that gets called when the cycle of training ends, so the Agent can get reset to original state and start training again.
        /// </summary>
        protected virtual void OnReset()
        {
            throw new System.NotImplementedException("OnReset function was not implemented on child class");
        }

        /// <summary>
        /// Function to save the actions performed by the agent.
        /// </summary>
        /// <param name="input">Array to fill with inputs for the NN</param>
        protected virtual void SaveActions(float[] input, float[] output)
        {
            DataPacket pkt;
            pkt.input = new float[input.Length];
            pkt.output = new float[output.Length];
            input.CopyTo(pkt.input, 0);
            output.CopyTo(pkt.output, 0);

            data.Add(pkt);
        }

        /// <summary>
        /// Method that will perform the selected method of Supervised Learning training
        /// </summary>
        protected virtual void Train()
        {
            if (dataFile == null)
            {
                Debug.LogError("No training data linked to the Agent. Can't perform training");
                return;
            }
            string json = dataFile.text;
            List<DataPacket> container = JsonConvert.DeserializeObject<List<DataPacket>>(json);
            
            for (int i = 0; i < epochs; i++)
            {
                foreach (var item in container)
                {
                    brain.BackPropagate(item.input, item.output);
                }
            }
            training = false;
            save = true;
        }


        private void FixedUpdate()
        {
            float[] output = new float[brain.config.outputSize];
            float[] input = new float[brain.config.inputSize];

            //Gather the input from the agent (sensors and others)
            AddObservationsInput(ref input);

            //If the agent is being player controlled, we use the player input as output for the agent behaviour.
            if (playerControlled)
                AddPlayerInput(ref output);
            else
            {
                //Feed the input to the Neural Network to obtain an output
                output = brain.FeedForward(input);
            }

            
            if (recordActions)
            {
                SaveActions(input, output);
            }
                

            //Send output to the agent behaviour
            OnOutputReceived(output);

            if (Input.GetKeyDown(KeyCode.F1))
                OnReset();
        }

        private void OnApplicationQuit()
        {
            string json;
            if (recordActions)
            {
                json = JsonConvert.SerializeObject(data);
                System.IO.File.WriteAllText(Application.dataPath + "/NeuralNetworks/NN/SupervisedLearning/TrainingData/" + brain.config.identifier + ".json", json);
            }
            
            if(save)
            {
                json = JsonConvert.SerializeObject(brain);
                System.IO.File.WriteAllText(Application.dataPath + "/NeuralNetworks/NN/SupervisedLearning/Brains/" + brain.config.identifier + ".json", json);
            }
        }
    }
}

