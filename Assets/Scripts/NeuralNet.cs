using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNet
{

    // 30% a neuron will mutate each generation
    public float mutationChance = 0.3f;
    public float mutationValue = 0.1f;
    // layer info allows the network to be scaleable 
    public int[] layerInfo = { 5, 4, 4, 2 };
    public float minWeight = -1;
    public float maxWeight = 1;
    public NetworkLayer[] netLayers;

    // create an initial network
    public NeuralNet()
    {
        // set number of layers
        netLayers = new NetworkLayer[layerInfo.Length - 1];

        for (int x = 0; x < netLayers.Length; x++)
        {
            // create the network layers
            netLayers[x] = new NetworkLayer(layerInfo[x], layerInfo[x + 1]);
            // generate random weights for the new neurons
            netLayers[x].GenerateWeights(minWeight, maxWeight);
        }
    }

    // create a new network using two parent networks
    public NeuralNet(NeuralNet dad, NeuralNet mum)
    {
        netLayers = new NetworkLayer[layerInfo.Length - 1];

        for (int x = 0; x < netLayers.Length; x++)
        {
            // create the network layers
            netLayers[x] = new NetworkLayer(layerInfo[x], layerInfo[x + 1]);
            // generate random weights for the new neurons
            mutationValue = Random.Range(0.05f, 0.2f);
            netLayers[x].GenerateWeightsChild(dad.netLayers[x].neuronWeights, mum.netLayers[x].neuronWeights, mutationValue, mutationChance);
        }
    }

    public void Mutate()
    {
        for (int x = 0; x < netLayers.Length; x++)
        {
            netLayers[x].Mutate(mutationValue, mutationChance);
        }
    }

    // the brain part where the inputs generate an output for car controls
    public float[] GenerateOutputs(float[] carInputs)
    {
        float[] input = carInputs;

        // pass the inputs through each layer to generate a final output
        foreach (NetworkLayer layer in netLayers)
        {
            input = layer.GenerateOutput(input);
        }

        return input;
    }

    // class for creating new network layers
    public class NetworkLayer
    {
        // layer values
        int neurons;
        int outputs;
        public float[,] neuronWeights;

        // create a new network layer
        public NetworkLayer(int currentNodes, int nextNodes)
        {
            // set the layer up with the layer info passed in
            this.neurons = currentNodes;
            this.outputs = nextNodes;

            // create array to store neurons
            neuronWeights = new float[currentNodes, outputs];
        }

        // loop through for all the weights and assign each a random value
        public void GenerateWeights(float min, float max)
        {
            for (int x = 0; x < neuronWeights.GetLength(0); x++)
            {
                for (int y = 0; y < neuronWeights.GetLength(1); y++)
                {
                    neuronWeights[x, y] = Random.Range(min, max);
                }
            }
        }

        // loop through for all the weights and assign each a random value
        public void GenerateWeightsChild(float[,] dad, float[,] mum, float mutationVal, float mutationChance)
        {
            for (int x = 0; x < neuronWeights.GetLength(0); x++)
            {
                for (int y = 0; y < neuronWeights.GetLength(1); y++)
                {
                    float parent = Random.Range(0.0f, 1.0f);
                    // if random is between 0 - 0.5 use fathers genes
                    if (parent <= 0.5)
                    {
                        neuronWeights[x, y] = dad[x, y];
                    }
                    // if random is between 0.5 and 1 use mothers genes
                    else
                    {
                        neuronWeights[x, y] = mum[x, y];
                    }

                    // float used for percentage check between 1 and 0 (100% - 0%)
                    float mutationCheck = Random.Range(0.0f, 1.0f);

                    // mutate up if check is below half the chance
                    if (mutationCheck < (mutationChance / 2) && neuronWeights[x, y] < 1 - mutationVal)
                    {
                            neuronWeights[x, y] += mutationVal;
                    }
                    // mutate down if its in the other half
                    else if (mutationCheck < mutationChance && neuronWeights[x, y] > -1 + mutationVal)
                    {
                            neuronWeights[x, y] -= mutationVal;
                    }
                }
            }
        }

        // mutate this neural net
        public void Mutate(float mutationVal, float mutationChance)
        {
            for (int x = 0; x < neuronWeights.GetLength(0); x++)
            {
                for (int y = 0; y < neuronWeights.GetLength(1); y++)
                {
                    // float used for percentage check between 1 and 0 (100% - 0%)
                    float mutationCheck = Random.Range(0.0f, 1.0f);

                    // mutate up if check is below half the chance
                    if (mutationCheck < (mutationChance / 2) && neuronWeights[x, y] < 1 - mutationVal)
                    {
                        neuronWeights[x, y] += mutationVal;
                    }
                    // mutate down if its in the other half
                    else if (mutationCheck < mutationChance && neuronWeights[x, y] > -1 + mutationVal)
                    {
                        neuronWeights[x, y] -= mutationVal;
                    }
                }
            }
        }

        // calculates the neuron output and activates the neuron
        public float[] GenerateOutput(float[] carInputs)
        {
            float[] output = new float[outputs];

            // loop through for each neuron
            for (int x = 0; x < neurons; x++)
            {
                // loop through for each output neuron
                for (int y = 0; y < outputs; y++)
                {
                    // might be wrong way round
                    output[y] += carInputs[x] * neuronWeights[x, y];
                }
            }

            // finsh by activating the neuron
            for (int x = 0; x < output.Length; x++)
            {
                output[x] = sigmoid(output[x]); // neuron activation
            }

            // return the output array
            return output;
        }

        // neuron activation function
        float sigmoid(float x)
        {
            return 1 / (1 + Mathf.Exp(-(float)x));
        }

        float relu(float x)
        {
            if (x < 0)
            {
                return 0;
            }
            return x / 3;
        }
    }
}
