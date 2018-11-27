
using System;

abstract public class GameModel {

    protected System.Random _rnd;

    abstract public void InitGame();

    abstract public void ResetGame(int seed);

    //return move score
    abstract public bool MakeMove(int input);

    abstract public void GameStateToInputs(float[] inputsToFill);

    abstract public float GetScore();

}
