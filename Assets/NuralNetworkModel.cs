using System;

[Serializable]
public class NuralNetworkModel {



    internal static int TOTAL_LAYERS_NUM = 5;
    internal static int HIDDEN_LAYERS_SIZE = 64;
    internal static int INPUT_SIZE = 16;
    internal static int OUTPUT_SIZE = 4;

    internal static float BIG_VARIATION_SIZE;// = 0.5f;
    internal static float SMALL_VARIATION_SIZE;// = 0.1f; //0.15f worked well
    internal static float OFFSET_VARIATION_SIZE;// = 0.01f; 
    internal static float HALF_BIG_VARIATION_SIZE;
    internal static float HALF_SMALL_VARIATION_SIZE;
    internal static float HALF_OFFSET_VARIATION_SIZE;

    internal static float MUTATION_NONE = 0.0f;
    internal static float MUTATION_SMALL = 1.0f;
    internal static float MUTATION_BIG = 0.0f;
    internal static float MUTATION_SIGN_FLIP = 0.0f;
    internal static float MUTATION_ACTIVATION_CHANGE = 0.0f;


    public enum ActivationFunctions { TanH, Linear, ReLU };

    public float[][] Layers;
    public float[][][] Weights;
    public ActivationFunctions[][][] ActivationFunction;
    public float[] Offsets;


    private const float CONNECTION_INIT_VAL = 0f;
    private const ActivationFunctions ACTIVATION_TYPE_INIT_VAL = ActivationFunctions.TanH;


    public NuralNetworkModel() {
       
        //weights
        Weights = new float[TOTAL_LAYERS_NUM][][];
        ActivationFunction = new ActivationFunctions[TOTAL_LAYERS_NUM][][];
        Offsets = new float[TOTAL_LAYERS_NUM];

        for (int l = 0; l < TOTAL_LAYERS_NUM; l++) {
            Offsets[l] = 0;

            if (l == 0) {
                Weights[l] = new float[INPUT_SIZE][];
                ActivationFunction[l] = new ActivationFunctions[INPUT_SIZE][];
            } else {
                Weights[l] = new float[HIDDEN_LAYERS_SIZE][];
                ActivationFunction[l] = new ActivationFunctions[HIDDEN_LAYERS_SIZE][];
            }

            for (int i = 0; i < Weights[l].Length; i++) {
                if (l == TOTAL_LAYERS_NUM - 1) {
                    Weights[l][i] = new float[OUTPUT_SIZE];
                    ActivationFunction[l][i] = new ActivationFunctions[OUTPUT_SIZE];
                } else {
                    Weights[l][i] = new float[HIDDEN_LAYERS_SIZE];
                    ActivationFunction[l][i] = new ActivationFunctions[HIDDEN_LAYERS_SIZE];

                }
                for (int j = 0; j < Weights[l][i].Length; j++) {
                    Weights[l][i][j] = CONNECTION_INIT_VAL;
                    ActivationFunction[l][i][j] = ACTIVATION_TYPE_INIT_VAL;
                }
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

        ResetSteps();
    }

    public static void PrepareNextGen(NuralNetworkModel parent, NuralNetworkModel child, System.Random rnd) {

        for (int i = 0; i < child.Weights.Length; i++) {
            if (i > 0) {
                child.Offsets[i] = parent.Offsets[i] + ((float)rnd.NextDouble() * OFFSET_VARIATION_SIZE) - HALF_OFFSET_VARIATION_SIZE;
            }
            for (int j = 0; j < child.Weights[i].Length; j++) {
                for (int k = 0; k < child.Weights[i][j].Length; k++) {

                    float mutationType = (float)rnd.NextDouble();
                    if (mutationType < MUTATION_NONE) {
                        //nothing happanes
                    } else if (mutationType < MUTATION_SMALL) {
                        //small mutation
                        child.Weights[i][j][k] = parent.Weights[i][j][k] + ((float)rnd.NextDouble() * SMALL_VARIATION_SIZE) - HALF_SMALL_VARIATION_SIZE;
                    } else if (mutationType < MUTATION_BIG) {
                        //big mutation
                        child.Weights[i][j][k] = parent.Weights[i][j][k] + ((float)rnd.NextDouble() * BIG_VARIATION_SIZE) - HALF_BIG_VARIATION_SIZE;
                    } else if (mutationType < MUTATION_SIGN_FLIP) {
                        //flip sign
                        child.Weights[i][j][k] = -parent.Weights[i][j][k];
                    } else if (mutationType < MUTATION_ACTIVATION_CHANGE) {
                        //change activation
                        child.ActivationFunction[i][j][k] = (child.ActivationFunction[i][j][k] == ActivationFunctions.TanH) ? ActivationFunctions.Linear : ActivationFunctions.TanH;
                    }
                }
            }
        }


    }

    internal static void ChangeSteps(bool isConverging) {
        if (isConverging) {
            SMALL_VARIATION_SIZE /= 1.7f;
            OFFSET_VARIATION_SIZE /= 1.7f;
            BIG_VARIATION_SIZE /= 1.7f;
        } else {
            SMALL_VARIATION_SIZE *= 1.4f;
            OFFSET_VARIATION_SIZE *= 1.4f;
            BIG_VARIATION_SIZE *= 1.4f;
        }
        
        
        HALF_BIG_VARIATION_SIZE = BIG_VARIATION_SIZE / 2f;
        HALF_SMALL_VARIATION_SIZE = SMALL_VARIATION_SIZE / 2f;
        HALF_OFFSET_VARIATION_SIZE = OFFSET_VARIATION_SIZE / 2f;
    }

    internal static void ResetSteps() {
        SMALL_VARIATION_SIZE = 0.6f;
        OFFSET_VARIATION_SIZE = 0.01f;
        BIG_VARIATION_SIZE = 1.0f;

        HALF_BIG_VARIATION_SIZE = BIG_VARIATION_SIZE / 2f;
        HALF_SMALL_VARIATION_SIZE = SMALL_VARIATION_SIZE / 2f;
        HALF_OFFSET_VARIATION_SIZE = OFFSET_VARIATION_SIZE / 2f;
    }

    internal static void Duplicate(NuralNetworkModel src, NuralNetworkModel dest) {

        for (int i = 0; i < src.Weights.Length; i++) {

            dest.Offsets[i] = src.Offsets[i];

            for (int j = 0; j < src.Weights[i].Length; j++) {
                for (int k = 0; k < src.Weights[i][j].Length; k++) {
                    dest.Weights[i][j][k] = src.Weights[i][j][k];
                    dest.ActivationFunction[i][j][k] = src.ActivationFunction[i][j][k];
                }
            }
        }

    }

    internal int CalculateMove() {

        //fill layers
        for (int l = 0; l < TOTAL_LAYERS_NUM - 1; l++) {
            if (l > 0) {
                //NormlizeLayer(_layers[l]);
            }

            for (int j = 0; j < Layers[l + 1].Length; j++) {
                for (int i = 0; i < Layers[l].Length; i++) {
                    if (i == 0) {
                        Layers[l + 1][j] = 0;
                    }
                    //_layers[l + 1][j] += _layers[l][i] * VariationModels[CurrentVariant]._Weights[l][i][j];
                    Layers[l + 1][j] += Activation(Layers[l][i], Weights[l][i][j], ActivationFunction[l][i][j]);
                }

                Layers[l + 1][j] += Offsets[l];
            }
        }

        int bestIndex = 0;
        float higestOutput = float.MinValue;

        for (int i = 0; i < Layers[Layers.Length - 1].Length; i++) {
            
            if (Layers[Layers.Length - 1][i] > higestOutput) {
                higestOutput = Layers[Layers.Length - 1][i];
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private float Activation(float input, float weight, ActivationFunctions activationType) {
        switch (activationType) {
            case ActivationFunctions.Linear:
                return input * weight;
            case ActivationFunctions.TanH:
                return (float)Math.Tanh(input) * weight;
            case ActivationFunctions.ReLU:
                return (input < 0) ? 0 : (input * weight);
            default:
                return 0;
        }
        
        
    }

    /*private void NormlizeLayer(float[] layer) {

        float minVal = float.MaxValue;
        float maxVal = float.MinValue;
        for (int i = 0; i < layer.Length; i++) {
            if (layer[i] < minVal) {
                minVal = layer[i];
            }
            if (layer[i] > maxVal) {
                maxVal = layer[i];
            }
        }

        float scale = (maxVal - minVal);

        for (int i = 0; i < layer.Length; i++) {

            layer[i] = (layer[i] - minVal) / scale;
            //layer[i] = (layer[i] - minVal);

        }
    }*/

    public float[] InputLayer {
        get {
            return Layers[0];
        }
    }
}
