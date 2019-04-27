
using System;

abstract public class GameModel {

    protected System.Random _rnd;
    internal bool IsTraining = true;

    abstract public void InitGame();

    abstract public void ResetGame(int seed);

    //return is finalmove
    abstract public bool MakeMove(int input);

    abstract public void GameStateToInputs(float[] inputsToFill);

    abstract public float GetScore();

}
