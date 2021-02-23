using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Axel.NeuralNetworks
{
    [Serializable]
    /// <summary>
    /// Contains the configuration needed to create a Neural Network
    /// </summary>
    public class NeuralNetworkConfiguration : MonoBehaviour
    {
        [Tooltip("The name identifier for this NN")]
        [SerializeField]
        internal string identifier;
        [Tooltip("The amount of input neurons the NN will use")]
        [SerializeField]
        internal int inputSize;
        [Tooltip("The amount of output neurons the NN will use")]
        [SerializeField]
        internal int outputSize;
        [Tooltip("The amount of neurons for each hidden Layer the NN will use")]
        [SerializeField]
        internal int[] hiddenLayerSize;

        /// <summary>
        /// Contains the amount of neurons of each layer in the NN Config
        /// </summary>
        internal int[] layers;

        public void Awake()
        {
            //Create layers array for easier accessibility of amount of layers and their sizes.
            layers = new int[2 + hiddenLayerSize.Length];
            layers[0] = inputSize;
            for (int i = 0; i < hiddenLayerSize.Length; i++)
            {
                layers[i + 1] = hiddenLayerSize[i];
            }
            layers[hiddenLayerSize.Length + 1] = outputSize;
        }
    }
}
