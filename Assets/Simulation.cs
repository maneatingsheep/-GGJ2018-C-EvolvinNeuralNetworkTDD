

using UnityEngine;
using static NuralNetworkModel;

public class Simulation {


    public GameModel Game;
    public NuralNetworkModel NuralNetwork;

    public static int[] Seeds;

    internal int CurrentGameIteration;


    private int gamesPerIteration;

    public float[] Scores;
    public float MaxGameScore = 0;
    public float OverallScore = 0;
    public float AvgScore = 0;
    public float BestScore = 0;
    public float Fitness = 0;

    public int GamesPerIteration {
        get {
            return gamesPerIteration;
        }

        set {
            gamesPerIteration = value;
            Scores = new float[gamesPerIteration];
        }
    }

    internal Simulation(System.Random rnd) {
        Game = ClassFactory.CreateGameModel();
        Game.InitGame();
        NuralNetwork = new NuralNetworkModel(rnd);
    }

    internal void RunOneTime() {
        
        for (int i = 0; i < gamesPerIteration; i++) {
            Scores[i] = 0;
        }
        
        for (CurrentGameIteration = 0; CurrentGameIteration < gamesPerIteration; CurrentGameIteration++) {
            Game.ResetGame(Seeds[CurrentGameIteration]);
            bool isGoodMove = true;
            while (isGoodMove) {
                isGoodMove = !PlayOneMove();
            }

            Scores[CurrentGameIteration] = Game.GetScore();
        }

        AvgScore = 0;
        BestScore = 0;
        for (int i = 0; i < gamesPerIteration; i++) {
            AvgScore += Scores[i];
            BestScore = Mathf.Max(Scores[i], BestScore);
        }
        AvgScore /= Scores.Length;
        //BestScore /= Scores.Length;
        //OverallScore = AvgScore + BestScore;
        OverallScore = AvgScore;

    }

    internal bool PlayOneMove() {
        Game.GameStateToInputs(NuralNetwork.InputLayer);
        NetworkOutput[] networkOutputs = NuralNetwork.CalculateMove();

        /*bool moveMade = false;
        for (int i = 0; i < networkOutputs.Length; i++) {
            moveMade |= Game.MakeMove(networkOutputs[i].Position);
            if (moveMade) {
                break;
            }
        }

        return !moveMade;*/

        return !Game.MakeMove(networkOutputs[0].Position);
    }
}
