using System;

[Serializable]
public class NuralNetworkModel {

    internal static int TOTAL_LAYERS_NUM = 5;
    internal static int HIDDEN_LAYERS_SIZE = 64;
    internal static int INPUT_SIZE = 16;
    internal static int OUTPUT_SIZE = 4;

    internal static float BIG_VARIATION_SIZE = 0.5f;
    internal static float SMALL_VARIATION_SIZE = 0.1f; //0.15f worked well
    internal static float HALF_BIG_VARIATION_SIZE;
    internal static float HALF_SMALL_VARIATION_SIZE;
    internal static float[] mutationChance = new float[] { 0.0f, 1.0f, 0.9995f };


    internal float[][] _layers;
    public float[][][] _Weights;


    private const float CONNECTION_INIT_VAL = 0f;

    public NuralNetworkModel() {
        HALF_BIG_VARIATION_SIZE = BIG_VARIATION_SIZE / 2f;
        HALF_SMALL_VARIATION_SIZE = SMALL_VARIATION_SIZE / 2f;

        //weights
        _Weights = new float[TOTAL_LAYERS_NUM][][];

        for (int l = 0; l < TOTAL_LAYERS_NUM; l++) {
            if (l == 0) {
                _Weights[l] = new float[INPUT_SIZE][];
            } else {
                _Weights[l] = new float[HIDDEN_LAYERS_SIZE][];
            }
            for (int i = 0; i < _Weights[l].Length; i++) {
                if (l == TOTAL_LAYERS_NUM - 1) {
                    _Weights[l][i] = new float[OUTPUT_SIZE];
                } else {
                    _Weights[l][i] = new float[HIDDEN_LAYERS_SIZE];

                }
                for (int j = 0; j < _Weights[l][i].Length; j++) {
                    _Weights[l][i][j] = CONNECTION_INIT_VAL;
                }
            }
        }

        //activators

        _layers = new float[TOTAL_LAYERS_NUM][];

        for (int i = 0; i < TOTAL_LAYERS_NUM; i++) {
            if (i == 0) {
                _layers[i] = new float[INPUT_SIZE];
            } else if (i == TOTAL_LAYERS_NUM - 1) {
                _layers[i] = new float[OUTPUT_SIZE];
            } else {
                _layers[i] = new float[HIDDEN_LAYERS_SIZE];
            }
        }
    }

    public static void PrepareNextGen(NuralNetworkModel parent, NuralNetworkModel child, System.Random rnd) {

        for (int i = 0; i < child._Weights.Length; i++) {
            for (int j = 0; j < child._Weights[i].Length; j++) {
                for (int k = 0; k < child._Weights[i][j].Length; k++) {

                    float mutationType = (float)rnd.NextDouble();
                    if (mutationType < mutationChance[0]) {
                        //nothing happanes
                    } else if (mutationType < mutationChance[1]) {
                        //big mutation
                        child._Weights[i][j][k] = parent._Weights[i][j][k] + ((float)rnd.NextDouble() * SMALL_VARIATION_SIZE) - HALF_SMALL_VARIATION_SIZE;
                    } else if (mutationType < mutationChance[2]) {

                        child._Weights[i][j][k] = parent._Weights[i][j][k] + ((float)rnd.NextDouble() * BIG_VARIATION_SIZE) - HALF_BIG_VARIATION_SIZE;
                    } else {
                        child._Weights[i][j][k] = -parent._Weights[i][j][k];
                    }


                }
            }
        }


    }

    internal static void Duplicate(NuralNetworkModel src, NuralNetworkModel dest) {

        for (int i = 0; i < src._Weights.Length; i++) {
            for (int j = 0; j < src._Weights[i].Length; j++) {
                for (int k = 0; k < src._Weights[i][j].Length; k++) {
                    dest._Weights[i][j][k] = src._Weights[i][j][k];
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

            for (int i = 0; i < _layers[l].Length; i++) {
                for (int j = 0; j < _layers[l + 1].Length; j++) {
                    if (i == 0) {
                        _layers[l + 1][j] = 0;
                    }
                    //_layers[l + 1][j] += _layers[l][i] * VariationModels[CurrentVariant]._Weights[l][i][j];
                    _layers[l + 1][j] += Activation(_layers[l][i], _Weights[l][i][j]);
                }
            }
        }

        int bestIndex = 0;
        float higestOutput = float.MinValue;

        for (int i = 0; i < _layers[_layers.Length - 1].Length; i++) {
            
            if (_layers[_layers.Length - 1][i] > higestOutput) {
                higestOutput = _layers[_layers.Length - 1][i];
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private float Activation(float input, float weight) {
        return (float)Math.Tanh(input) * weight;
    }

    private void NormlizeLayer(float[] layer) {

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
    }

    public float[] InputLayer {
        get {
            return _layers[0];
        }
    }
}
