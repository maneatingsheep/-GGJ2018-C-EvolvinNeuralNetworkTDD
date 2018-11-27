

using System;

public class Simulation {


    public GameModel Game;
    public NuralNetworkModel NuralNetwork;

    public static int[] Seeds;

    internal int CurrentGameIteration;


    public int GamesPerIteration;

    public float[] Scores;
    public float MaxGameScore = 0;
    public float OverallScore = 0;

    internal Simulation() {
        Game = ClassFactory.CreateGameModel();
        Game.InitGame();
        NuralNetwork = new NuralNetworkModel();
    }

    internal void CreateNextGen(Simulation parentSimulation, System.Random rnd) {
        NuralNetworkModel.PrepareNextGen(parentSimulation.NuralNetwork, NuralNetwork, rnd);
    }

    internal void RunOneTime() {
        
        for (int i = 0; i < Scores.Length; i++) {
            Scores[i] = 0;
        }
        
        for (CurrentGameIteration = 0; CurrentGameIteration < GamesPerIteration; CurrentGameIteration++) {
            Game.ResetGame(Seeds[CurrentGameIteration]);
            bool isGoodMove = true;
            while (isGoodMove) {
                isGoodMove = !PlayOneMove();
            }

            Scores[CurrentGameIteration] = Game.GetScore();
        }

        OverallScore = 0;
        for (int i = 0; i < Scores.Length; i++) {
            OverallScore += Scores[i];
        }
        OverallScore /= Scores.Length;

    }

    internal bool PlayOneMove() {
        Game.GameStateToInputs(NuralNetwork.InputLayer);
        int networkOutput = NuralNetwork.CalculateMove();
        return Game.MakeMove(networkOutput);
    }
}
