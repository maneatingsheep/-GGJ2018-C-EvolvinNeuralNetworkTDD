

using System;
using UnityEngine;

public class Game2048 : GameModel {

    public enum Direction { Up, Down, Left, Right };

    internal LogicTile[][] Tiles;

    private float _score;
    private float _moveCount;

    internal const int SIDE_LEN = 4;
    internal const int STARTING_PICIES = 2;
    private float[] SPAWNING_CHANCE = new float[] { 0.85f, 0.15f };


    private int ScanInitialX;
    private int ScanInitialY;
    private int SlowScanDirectionX;
    private int SlowScanDirectionY;
    private int FastScanDirectionX;
    private int FastScanDirectionY;
    private int ShiftDirectionX;
    private int ShiftDirectionY;

    private int ScannerX;
    private int ScannerY;
    private int ShiftScannerX;
    private int ShiftScannerY;


    override public void InitGame() {
        Tiles = new LogicTile[SIDE_LEN][];

        for (int i = 0; i < SIDE_LEN * SIDE_LEN; i++) {
            int coll = i % SIDE_LEN;
            int row = Mathf.FloorToInt(i / SIDE_LEN);

            if (coll == 0) {
                Tiles[row] = new LogicTile[SIDE_LEN];
            }

            Tiles[row][coll] = new LogicTile();

        }
    }

    override public void ResetGame(int seed) {
        
        _rnd = new System.Random(seed);
        
        for (int i = 0; i < SIDE_LEN; i++) {
            for (int j = 0; j < SIDE_LEN; j++) {
                Tiles[i][j].NumValue = 0;
                Tiles[i][j].PossibleAction = LogicTile.ActionType.None;
                Tiles[i][j].Finlize();

            }
        }

        for (int p = 0; p < STARTING_PICIES; p++) {
            SpawnTile(1);
        }

        _score = 0;
        _moveCount = 0;


    }

    private void SpawnTile(int tileValue) {
        if (tileValue == -1) {
            double spawnChance = _rnd.NextDouble();

            float currentBar = 0f;
            bool chanceMet = false;
            for (int s = 0; s < SPAWNING_CHANCE.Length; s++) {
                currentBar += SPAWNING_CHANCE[s];

                if (spawnChance <= currentBar) {
                    tileValue = s + 1;
                    chanceMet = true;
                    break;
                }
            }
            if (!chanceMet) {
                tileValue = SPAWNING_CHANCE.Length;
            }
        }

        int emptyPlaces = 0;
        for (int i = 0; i < SIDE_LEN; i++) {
            for (int j = 0; j < SIDE_LEN; j++) {
                if (Tiles[i][j].NumValue == 0) {
                    emptyPlaces++;
                }
            }
        }


        int wantedPosition = _rnd.Next(emptyPlaces);

        int index = 0;
        for (int i = 0; i < SIDE_LEN; i++) {
            for (int j = 0; j < SIDE_LEN; j++) {

                if (Tiles[i][j].NumValue == 0) {

                    if (index == wantedPosition) {

                        Tiles[i][j].NumValue = tileValue; ;
                        Tiles[i][j].PossibleAction = LogicTile.ActionType.New;
                        Tiles[i][j].Finlize();

                        i = SIDE_LEN;
                        j = SIDE_LEN;


                    } else {
                        index++;

                    }
                }


            }
        }
    }


    //return if this is the game ending move
    override public bool MakeMove(int input) {
        switch (input) {
            case 0: return MakeMove(Direction.Up);
            case 1: return MakeMove(Direction.Down);
            case 2: return MakeMove(Direction.Left);
            default: return MakeMove(Direction.Right);
        }
    }

    public bool MakeMove(Direction direction) {

        switch (direction) {
            case Direction.Up:
                ScanInitialX = 0;
                ScanInitialY = 0;
                SlowScanDirectionX = 0;
                SlowScanDirectionY = 1;
                FastScanDirectionX = 1;
                FastScanDirectionY = 0;
                break;
            case Direction.Down:
                ScanInitialX = 0;
                ScanInitialY = SIDE_LEN - 1;
                SlowScanDirectionX = 0;
                SlowScanDirectionY = -1;
                FastScanDirectionX = 1;
                FastScanDirectionY = 0;

                break;
            case Direction.Left:
                ScanInitialX = 0;
                ScanInitialY = 0;
                SlowScanDirectionX = 1;
                SlowScanDirectionY = 0;
                FastScanDirectionX = 0;
                FastScanDirectionY = 1;
                break;
            default:
                ScanInitialX = SIDE_LEN - 1;
                ScanInitialY = 0;
                SlowScanDirectionX = -1;
                SlowScanDirectionY = 0;
                FastScanDirectionX = 0;
                FastScanDirectionY = 1;
                break;
        }

        ShiftDirectionX = SlowScanDirectionX * -1;
        ShiftDirectionY = SlowScanDirectionY * -1;

        ScannerX = ScanInitialX;
        ScannerY = ScanInitialY;

        bool moveMade = false;
        int mergeScore = 0;

        for (int i = 0; i < SIDE_LEN * SIDE_LEN; i++) {

            //shift tile scanner.x scanner .y
            if (Tiles[ScannerX][ScannerY].NumValue > 0) {
                ShiftScannerX = ScannerX;
                ShiftScannerY = ScannerY;

                bool inBounds = true;
                bool isTargetEmpty = true;

                while (inBounds && isTargetEmpty) {

                    bool merge = false;

                    ShiftScannerX += ShiftDirectionX;
                    ShiftScannerY += ShiftDirectionY;

                    inBounds = ShiftScannerX >= 0 && ShiftScannerX < SIDE_LEN && ShiftScannerY >= 0 && ShiftScannerY < SIDE_LEN;

                    if (inBounds) {
                        isTargetEmpty = (Tiles[ShiftScannerX][ShiftScannerY].NumValue == 0);

                        merge = !isTargetEmpty && Tiles[ScannerX][ScannerY].NumValue == Tiles[ShiftScannerX][ShiftScannerY].NumValue && !Tiles[ShiftScannerX][ShiftScannerY].Merged;
                    }

                    if (merge) {
                        Tiles[ShiftScannerX][ShiftScannerY].Merged = true;
                        mergeScore += (int)Mathf.Pow(2, Tiles[ShiftScannerX][ShiftScannerY].NumValue);
                        Tiles[ShiftScannerX][ShiftScannerY].PossibleAction = LogicTile.ActionType.Grow;
                    }

                    moveMade |= ((inBounds && isTargetEmpty) || merge);


                    if (!inBounds || !isTargetEmpty) {

                        if (!merge) {
                            ShiftScannerX -= ShiftDirectionX;
                            ShiftScannerY -= ShiftDirectionY;
                        }
                        int numValue = Tiles[ScannerX][ScannerY].NumValue + ((merge) ? 1 : 0);

                        Tiles[ScannerX][ScannerY].NumValue = 0;
                        Tiles[ShiftScannerX][ShiftScannerY].NumValue = numValue;

                    }

                }

            }

            //progress scanner
            bool isBoreder = (i % SIDE_LEN) == SIDE_LEN - 1;
            ScannerX = ScannerX + ((isBoreder) ? (SlowScanDirectionX + (FastScanDirectionX * -(SIDE_LEN - 1))) : (FastScanDirectionX));
            ScannerY = ScannerY + ((isBoreder) ? (SlowScanDirectionY + (FastScanDirectionY * -(SIDE_LEN - 1))) : (FastScanDirectionY));
        }

        for (int i = 0; i < SIDE_LEN; i++) {
            for (int j = 0; j < SIDE_LEN; j++) {
                Tiles[i][j].Finlize();
            }
        }

        if (moveMade) {
            SpawnTile(-1);
            _moveCount++;
        }

        if (moveMade) {
            _score += ((mergeScore <= 0) ? 0 : mergeScore);
        }

        return moveMade;

    }

    override public void GameStateToInputs(float[] inputsToFill) {
        //fill input
        for (int i = 0; i < Tiles.Length; i++) {
            for (int j = 0; j < Tiles[i].Length; j++) {
                //inputsToFill[i * SIDE_LEN + j] = Mathf.Pow(2, Tiles[i][j].NumValue) / 2048f;
                inputsToFill[i * SIDE_LEN + j] = Tiles[i][j].NumValue;
            }
        }
    }

    override public float GetScore() {
        //return (_moveCount * 0.1f) + _score / Mathf.Max(_moveCount, 1);
        //return _score + _moveCount/** 0.01f*/;
        return (float)Math.Pow(_moveCount, 2);

    }

}
