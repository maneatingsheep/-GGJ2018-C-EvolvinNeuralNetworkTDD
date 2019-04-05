using System;

[Serializable]
public class NuralNetworkModel {



    internal static int TOTAL_LAYERS_NUM = 5;
    internal static int HIDDEN_LAYERS_SIZE = 100;
    internal static int INPUT_SIZE = 16;
    internal static int OUTPUT_SIZE = 4;

    internal static float MUTATION_RATE = 0.06f;
    internal static float HALF_MUTATION_RATE = 0.03f;

    internal static float MUTATION_CHANCE = 0.07f;
    
    public enum ActivationFunctions { TanH, ReLU };

    public float[][] Layers;
    public float[][][] Weights;
    public float[][] Biases;


    private const float CONNECTION_INIT_VAL = 0f;
    

    private NetworkOutput[] _networkOutputs;

    public NuralNetworkModel(System.Random rnd) {
       
        //weights
        Weights = new float[TOTAL_LAYERS_NUM - 1][][];
        Biases = new float[TOTAL_LAYERS_NUM - 1][];

        for (int l = 0; l < TOTAL_LAYERS_NUM - 1; l++) {

            if (l == 0) {
                Weights[l] = new float[INPUT_SIZE][];
            } else {
                Weights[l] = new float[HIDDEN_LAYERS_SIZE][];
            }


            for (int i = 0; i < Weights[l].Length; i++) {
                if (l == TOTAL_LAYERS_NUM - 1 - 1) {
                    Weights[l][i] = new float[OUTPUT_SIZE];
                } else {
                    Weights[l][i] = new float[HIDDEN_LAYERS_SIZE];

                }
                for (int j = 0; j < Weights[l][i].Length; j++) {
                    Weights[l][i][j] = (float)rnd.NextDouble() * 0.2f - 0.1f;
                }
            }

            if (l == TOTAL_LAYERS_NUM - 1 - 1) {
                Biases[l] = new float[OUTPUT_SIZE];
            } else {
                Biases[l] = new float[HIDDEN_LAYERS_SIZE];

            }

            for (int i = 0; i < Biases[l].Length; i++) {
                Biases[l][i] = (float)rnd.NextDouble() * 0.2f - 0.1f;

            }




        }

        //activators

        Layers = new float[TOTAL_LAYERS_NUM][];

        for (int i = 0; i < TOTAL_LAYERS_NUM; i++) {
            if (i == 0) {
                Layers[i] = new float[INPUT_SIZE];
            } else if (i == TOTAL_LAYERS_NUM - 1) {
                Layers[i] = new float[OUTPUT_SIZE];
            } else {
                Layers[i] = new float[HIDDEN_LAYERS_SIZE];
            }
        }

        //ResetMutationRate();

        _networkOutputs = new NetworkOutput[OUTPUT_SIZE];
        for (int i = 0; i < _networkOutputs.Length; i++) {
            _networkOutputs[i] = new NetworkOutput();

        }
}


    /*internal static void ReduceMutationRate() {
        MUTATION_RATE /= 1.8f;
        HALF_MUTATION_RATE = MUTATION_RATE / 2f;
    }

    internal static void ResetMutationRate() {
        MUTATION_RATE = 0.10f;
        HALF_MUTATION_RATE = MUTATION_RATE / 2f;
    }*/

    public static void PrepareNextGen(NuralNetworkModel parent1, NuralNetworkModel parent2, NuralNetworkModel child, System.Random rnd) {

        for (int l = 0; l < child.Weights.Length; l++) {
            
            for (int j = 0; j < child.Weights[l].Length; j++) {
                
                for (int k = 0; k < child.Weights[l][j].Length; k++) {
                    child.Weights[l][j][k] = BreedOneNode(parent1.Weights[l][j][k], parent1.Weights[l][j][k], rnd);
                }
            }

            for (int i = 0; i < child.Biases[l].Length; i++) {
                child.Biases[l][i] = BreedOneNode(parent1.Biases[l][i], parent2.Biases[l][i], rnd);
            }

        }

    }

    private static float BreedOneNode(float parent1, float parent2, System.Random rnd) {
        float retVal = parent1;
        double parentChance = rnd.NextDouble();
        if (parentChance > 0.5f) {
            retVal = parent2;
        }

        double mutationChance = rnd.NextDouble();
        if (mutationChance < MUTATION_CHANCE) {
            retVal +=  ((float)rnd.NextDouble() * MUTATION_RATE) - HALF_MUTATION_RATE;
        }
        
        return retVal;
    }


    internal static void Duplicate(NuralNetworkModel src, NuralNetworkModel dest) {

        for (int i = 0; i < src.Weights.Length; i++) {

            for (int j = 0; j < src.Weights[i].Length; j++) {
                for (int k = 0; k < src.Weights[i][j].Length; k++) {
                    dest.Weights[i][j][k] = src.Weights[i][j][k];
                }
            }

            for (int k = 0; k < src.Biases.Length; k++) {
                for (int j = 0; j < src.Biases[k].Length; j++) {
                    dest.Biases[k][j] = src.Biases[k][j];
                }
            }
        }

    }

    internal NetworkOutput[] CalculateMove() {

        //NormlizeLayer(Layers[0]);

        //fill layers
        for (int l = 0; l < TOTAL_LAYERS_NUM - 1; l++) {
            

            for (int j = 0; j < Layers[l + 1].Length; j++) {

                Layers[l + 1][j] = 0;

                for (int i = 0; i < Layers[l].Length; i++) {
                    Layers[l + 1][j] += (Layers[l][i] * Weights[l][i][j]);
                    //Layers[l + 1][j] += Activation((Layers[l][i] * Weights[l][i][j]), ActivationFunction[l + 1][j]);
                }

                //Layers[l + 1][j] /= Layers[l].Length;

                Layers[l + 1][j] += Biases[l][j];

                Layers[l + 1][j] = (float)Math.Tanh(Layers[l + 1][j]);

                //Layers[l + 1][j] = 1 / (1 + (float)Math.Exp(-Layers[l + 1][j]));


            }
        }


        for (int i = 0; i < OUTPUT_SIZE; i++) {
            _networkOutputs[i].Position = i;
            _networkOutputs[i].Value = Layers[Layers.Length - 1][i];
        }

        Array.Sort<NetworkOutput>(_networkOutputs, (a, b) => {
            float diff = b.Value - a.Value;
            return (diff > 0) ? (1) : ((diff < 0) ? -1 : 0);
        });

        return _networkOutputs;
    }


    public float[] InputLayer {
        get {
            return Layers[0];
        }
    }

    
}

[Serializable]
public class NetworkOutput {
    public int Position;
    public float Value;
}
