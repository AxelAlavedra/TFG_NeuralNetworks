using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Axel.NeuralNetworks
{
    /// <summary>
    /// Agent abstract class for Neural Network based behaviours.
    /// </summary>
    public class Agent : MonoBehaviour
    {
        [Header("Agent")]
        [Tooltip("If the player will control the agent or not")]
        public bool playerControlled = false;
        [Tooltip("If the agent is training or not")]
        public bool training = false;
        [Tooltip("If the agent actions are recorder or not")]
        public bool record = false;
        [Tooltip("NN Graph for visualization of the Neural Network")]
        public NNGraph graph = null;
        [Tooltip("The neural network the agent is using")]
        public NeuralNetwork brain = null;

        bool save = false;

        [System.Serializable]
        public struct RecordPacket
        {
            public float[] input;
            public float[] output;
        }

        [System.Serializable]
        public struct RecordContainer
        {
            public List<RecordPacket> dataList;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="_dataList">Data list value</param>
            public RecordContainer(List<RecordPacket> _dataList)
            {
                dataList = _dataList;
            }
        }
        public List<RecordPacket> records;

        //ToDo (Axel): Make NN initialization always be called, without having to call base Start on child class.
        protected virtual void Start()
        {
            //NeuralNetworkConfiguration NNConfig = GetComponent<NeuralNetworkConfiguration>();
            if (brain != null)
            {
                string filePath = Application.persistentDataPath + "/Brains/" + brain.config.identifier + ".json";
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


            // ToDo (Axel): improve UI initialization
            if (graph != null)
                graph.Initialize(brain);

            if (record)
                records = new List<RecordPacket>();

            //If the neural network is training proceed to Train with selected method.
            if (training)
                Train();
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

        /// <summary>
        /// Function to save the actions performed by the agent.
        /// </summary>
        /// <param name="input">Array to fill with inputs for the NN</param>
        public virtual void SaveActions(float[] input, float[] output)
        {
            RecordPacket pkt;
            pkt.input = new float[input.Length];
            pkt.output = new float[output.Length];
            input.CopyTo(pkt.input, 0);
            output.CopyTo(pkt.output, 0);

            records.Add(pkt);
        }

        public void Train()
        {
            RecordContainer container = JsonUtility.FromJson<RecordContainer>(System.IO.File.ReadAllText(Application.dataPath + "/NeuralNetworks/JSON/Records/" + brain.config.identifier + ".json"));
            
            for (int i = 0; i < 10000; i++)
            {
                foreach (var item in container.dataList)
                {
                    brain.BackPropagate(item.input, item.output);
                }

                /*brain.BackPropagate(new float[] { 0, 0, 0 }, new float[] { 0, 1 });
                brain.BackPropagate(new float[] { 1, 0, 0 }, new float[] { -1, 1 });
                brain.BackPropagate(new float[] { 0, 1, 0 }, new float[] { 0, -1 });
                brain.BackPropagate(new float[] { 0, 0, 1 }, new float[] { 1, 1 });
                brain.BackPropagate(new float[] { 1, 1, 0 }, new float[] { -1, 0 });
                brain.BackPropagate(new float[] { 0, 1, 1 }, new float[] { 1, 0 });
                brain.BackPropagate(new float[] { 1, 0, 1 }, new float[] { 0, 0.75f });
                brain.BackPropagate(new float[] { 1, 1, 1 }, new float[] { 0, -1 });*/

                // Backpropagation
                // 5 sensors
                // sensor 0 right
                // sensor 1 front-right
                // sensor 2 front
                // sensor 3 front-left
                // sensor 4 left
                /*brain.BackPropagate(new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 1 });
                brain.BackPropagate(new float[] { 1, 0, 0, 0, 0 }, new float[] { -1, 0 });
                brain.BackPropagate(new float[] { 0, 1, 0, 0, 0 }, new float[] { -1.0f, 0.0f });
                brain.BackPropagate(new float[] { 0, 0, 1, 0, 0 }, new float[] { 0, -1 });
                brain.BackPropagate(new float[] { 0, 0, 0, 1, 0 }, new float[] { 1.0f, 0.0f });
                brain.BackPropagate(new float[] { 0, 0, 0, 0, 1 }, new float[] { 1, 0 });
                brain.BackPropagate(new float[] { 1, 1, 0, 0, 0 }, new float[] { -1, 0.0f });
                brain.BackPropagate(new float[] { 1, 1, 1, 0, 0 }, new float[] { -1, -0.5f });
                brain.BackPropagate(new float[] { 1, 1, 1, 1, 0 }, new float[] { -1, -0.5f });
                brain.BackPropagate(new float[] { 0, 0, 0, 1, 1 }, new float[] { 1, 0.0f });
                brain.BackPropagate(new float[] { 0, 0, 1, 1, 1 }, new float[] { 1, -0.5f });
                brain.BackPropagate(new float[] { 0, 1, 1, 1, 1 }, new float[] { 1, -0.5f });
                brain.BackPropagate(new float[] { 1, 0, 0, 0, 1 }, new float[] { 0, 0.75f });
                brain.BackPropagate(new float[] { 1, 1, 0, 0, 1 }, new float[] { -1.0f, 0.0f });
                brain.BackPropagate(new float[] { 1, 1, 1, 0, 1 }, new float[] { -1.0f, -0.5f });
                brain.BackPropagate(new float[] { 1, 0, 0, 1, 1 }, new float[] { 1.0f, 0.0f });
                brain.BackPropagate(new float[] { 1, 0, 1, 1, 1 }, new float[] { 1.0f, -0.5f });
                brain.BackPropagate(new float[] { 0, 1, 0, 1, 1 }, new float[] { 1, 0.0f });
                brain.BackPropagate(new float[] { 1, 1, 0, 1, 0 }, new float[] { -1, 0.0f });
                brain.BackPropagate(new float[] { 1, 1, 0, 1, 1 }, new float[] { 0, 0.5f });
                brain.BackPropagate(new float[] { 1, 1, 1, 1, 1 }, new float[] { 0, -1 });*/
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

            
            if (record)
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
            if (record)
            {
                RecordContainer container = new RecordContainer(records);
                json = JsonUtility.ToJson(container);
                System.IO.File.WriteAllText(Application.dataPath + "/NeuralNetworks/JSON/Records/" + brain.config.identifier + ".json", json);
            }
            
            if(save)
            {
                json = JsonConvert.SerializeObject(brain);
                System.IO.File.WriteAllText(Application.persistentDataPath + "/NeuralNetworks/JSON/Brains/" + brain.config.identifier + ".json", json);
            }
        }
    }
}

