using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Axel.NeuralNetworks
{
    public class NNGraph : MonoBehaviour
    {
        [Header("Neural Network")]
        public NeuralNetwork brain = null;

        [Header("UI Config")]
        public bool draw = false;
        public GameObject neuronPrefab;
        public GameObject layerPrefab;
        public Gradient neuronGradient;

        GameObject[] layers;
        Image[][] neurons;

        public void Initialize(NeuralNetwork newBrain)
        {
            if (newBrain == null)
                return;

            brain = newBrain;
            layers = new GameObject[brain.config.layers.Length];
            neurons = new Image[brain.config.layers.Length][];

            for (int i = 0; i < brain.config.layers.Length; i++)
            {
                GameObject layerGO = Instantiate(layerPrefab, this.transform);
                layers[i] = layerGO;
                neurons[i] = new Image[brain.config.layers[i]];

                for(int j = 0; j < brain.config.layers[i]; j++)
                {
                    neurons[i][j] = Instantiate(neuronPrefab, layerGO.transform).GetComponent<Image>();
                }

                layerGO.SetActive(false);
            }
        }

        private void Update()
        {
            if(draw && brain != null)
            {
                if(!layers[0].activeSelf)
                {
                    foreach (var layer in layers)
                    {
                        layer.SetActive(true);
                    }
                }

                for(int i = 0; i < brain.neurons.Length; i++)
                {
                    for (int j = 0; j < brain.neurons[i].Length; j++)
                    {
                        Debug.Log((brain.neurons[i][j] + 1) / 2);
                        neurons[i][j].color = neuronGradient.Evaluate((brain.neurons[i][j] + 1) / 2);
                    }
                }
            }
        }
    }
}


