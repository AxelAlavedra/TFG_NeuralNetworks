using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Axel.NeuralNetworks
{
    [Serializable]
    public struct NeuralNetworkConfig
    {
        [Tooltip("The name identifier for this NN")]
        [SerializeField]
        public string identifier;
        [Tooltip("The amount of input neurons the NN will use")]
        [SerializeField]
        public int inputSize;
        [Tooltip("The amount of output neurons the NN will use")]
        [SerializeField]
        public int outputSize;
        [Tooltip("The amount of neurons for each hidden Layer the NN will use")]
        [SerializeField]
        public int[] hiddenLayerSize;
        [Tooltip("The learning rate for the network to use")]
        [SerializeField]
        public float learningRate;
    }


    [CreateAssetMenu(fileName = "NeuralNetwork", menuName = "Neural Networks/Neural Network")]
    public class NeuralNetwork : ScriptableObject
    {
        [SerializeField]
        /// <summary>
        /// The configuration used on this Neural Network
        /// </summary>
        public NeuralNetworkConfig config;     
        
        [JsonProperty]
        int[] layers;
        [JsonProperty]
        float[][] neurons;
        [JsonProperty]
        float[][] biases;
        [JsonProperty]
        float[][][] weights;

        [JsonProperty]
        float cost = 0;
        [JsonProperty]
        float learningRate = 0.01f;

        public void Init()
        {
            InitLayers();
            InitNeurons();
            InitWeights();
            InitBiases();
        }

        private void InitLayers()
        {
            //Create layers array for easier accessibility of amount of layers and their sizes.
            layers = new int[2 + config.hiddenLayerSize.Length];
            layers[0] = config.inputSize;
            for (int i = 0; i < config.hiddenLayerSize.Length; i++)
            {
                layers[i + 1] = config.hiddenLayerSize[i];
            }
            layers[config.hiddenLayerSize.Length + 1] = config.outputSize;

            learningRate = config.learningRate;
        }

        private void InitNeurons()
        {
            //Create the neurons of the Neural Network based on the configuration established.
            neurons = new float[layers.Length][];
            for (int i = 0; i < layers.Length; i++)
            {
                neurons[i] = new float[layers[i]]; //Allocate memory for the neurons of this layers based on the config.
            }
        }

        private void InitWeights()
        {
            //We create weights for each connection of each neuron in the hidden layers and output layer
            weights = new float[layers.Length - 1][][]; // -1 since we dont need weights for Input Layer
            for (int i = 1; i < layers.Length; i++)
            {
                weights[i - 1] = new float[layers[i]][]; //Allocate memory for the amount of neurons in this layer
                int neuronsInPreviousLayer = layers[i - 1]; //Obtain amount of neurons on previous layer so we know how many connections we need
                for (int j = 0; j < layers[i]; j++) //Loop through each neuron of this layer
                {
                    weights[i - 1][j] = new float[neuronsInPreviousLayer]; //Allocate memory for each weight required, based on the amount of neurons of previous layer
                    for (int k = 0; k < neuronsInPreviousLayer; k++) //Loop through each weight
                    {
                        weights[i - 1][j][k] = UnityEngine.Random.Range(-.5f, .5f); //Initialize each weight with a random between -0.5f and 0.5f
                    }
                }
            }
        }

        private void InitBiases()
        {
            //We create biases for each neuron in hidden layers and output layer.
            biases = new float[layers.Length - 1][]; // -1 since we dont need biases for Input Layer
            for (int i = 1; i < layers.Length; i++) //We start i = 1 since we dont need bias on our input layer neurons
            {
                biases[i - 1] = new float[layers[i]]; //Allocate memory for the biases of this layer based on the size written in config
                for(int j = 0; j < layers[i]; j++)
                {
                    biases[i - 1][j] = UnityEngine.Random.Range(-.5f,.5f); //Initialize each bias with a random between -0.5f and 0.5f
                }
            }
        }
  
        public float[] FeedForward(float[] input)
        {
            //Make sure the input received size is same as expected
            if (input.Length != config.inputSize)
            {
                throw new Exception("Error at FeedForward of Neural Network, input size expected was " + config.inputSize + " but received a size of " + input.Length);
            }

            //Update input layer with new input
            for (int i = 0; i < config.inputSize; i++)
            {
                neurons[0][i] = input[i];
            }

            //Feedforward
            for (int i = 1; i < layers.Length; i++) //Iterate each layer of the network except the input
            {
                for (int j = 0; j < neurons[i].Length; j++) //Iterate each neuron of the layer
                {
                    float value = 0f;
                    for (int k = 0; k < weights[i - 1][j].Length; k++) //Iterate each weight of the neuron
                    {
                        value += weights[i - 1][j][k] * neurons[i - 1][k]; //Multiply weight with the value of the neuron linked to on the previous layer
                    }
                    neurons[i][j] = Activate(value + biases[i - 1][j], "tanh"); //Add bias value and apply activate function to calculate new neuron value
                }
            }

            return neurons[neurons.Length - 1]; //Return output from the network
        }

        public float Activate(float value, string function)
        {
            float ret = 0.0f;

            switch (function) //Check for what activation function to use and apply it
            {
                case "tanh":
                    ret = (float)Math.Tanh(value);
                    break;
            }

            return ret; //Return the value calculated by the activation function
        }

        public void BackPropagate(float[] inputs, float[] expected)
        {
            float[] output = FeedForward(inputs); //runs feed forward to ensure neurons are populated correctly

            cost = 0;
            for (int i = 0; i < output.Length; i++) cost += (float)Math.Pow(output[i] - expected[i], 2);//calculated cost of network
            cost = cost / 2; //this value is not used in calculations, rather used to identify the performance of the network

            float[][] gamma;


            List<float[]> gammaList = new List<float[]>();
            for (int i = 0; i < layers.Length; i++)
            {
                gammaList.Add(new float[layers[i]]);
            }
            gamma = gammaList.ToArray();//gamma initialization

            int layer = layers.Length - 2;
            for (int i = 0; i < output.Length; i++) gamma[layers.Length - 1][i] = (output[i] - expected[i]) * ActivateDerivative(output[i], "tanh"); //Gamma calculation
            for (int i = 0; i < layers[layers.Length - 1]; i++)//calculates the w' and b' for the last layer in the network
            {
                biases[layers.Length - 2][i] -= gamma[layers.Length - 1][i] * learningRate;
                for (int j = 0; j < layers[layers.Length - 2]; j++)
                {

                    weights[layers.Length - 2][i][j] -= gamma[layers.Length - 1][i] * neurons[layers.Length - 2][j] * learningRate;
                }
            }

            for (int i = layers.Length - 2; i > 0; i--) //runs on all hidden layers
            {
                layer = i - 1;
                for (int j = 0; j < layers[i]; j++) //outputs
                {
                    gamma[i][j] = 0;
                    for (int k = 0; k < gamma[i + 1].Length; k++)
                    {
                        gamma[i][j] += gamma[i + 1][k] * weights[i][k][j];
                    }
                    gamma[i][j] *= ActivateDerivative(neurons[i][j], "tanh"); //calculate gamma
                }
                for (int j = 0; j < layers[i]; j++) //itterate over outputs of layer
                {
                    biases[i - 1][j] -= gamma[i][j] * learningRate; //modify biases of network
                    for (int k = 0; k < layers[i - 1]; k++) //iterate over inputs to layer
                    {
                        weights[i - 1][j][k] -= gamma[i][j] * neurons[i - 1][k] * learningRate; //modify weights of network
                    }
                }
            }
        }
        public float ActivateDerivative(float value, string function)
        {
            float ret = 0.0f;

            switch (function) //Check for what activation function to use and apply it
            {
                case "tanh":
                    ret = 1 - (value * value);
                    break;
            }

            return ret; //Return the value calculated by the activation function
        }
    }
}
