using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameNumberReader : GameModel {

    static public byte[] ImagesTest;
    static public byte[] LabelsTest;
    static public byte[] ImagesTrain;
    static public byte[] LabelsTrain;

    static private bool isInit = false;

    public int imgIndex;
    public int lastIndex;
    public int imgCount;
    private int score;

    public int ImagesOffset = 16;
    public int LabelsOffset = 8;

    public int imageSideSize = 28;
    public int imageArea = 28 * 28;

    public int lastGuess;


    override public void InitGame() {
        StaticInit();
    }

    private static void StaticInit() {
        if (isInit) return;
        isInit = true;
        ImagesTest = File.ReadAllBytes("D:/projects/independent project/EvolvinNeuralNetworkTDD/Testimages/t10k-images.idx3-ubyte");
        LabelsTest = File.ReadAllBytes("D:/projects/independent project/EvolvinNeuralNetworkTDD/Testimages/t10k-labels.idx1-ubyte");
    }

    override public void ResetGame(int seed) {
        lastIndex = 0;
        _rnd = new System.Random(seed);
        imgCount = 0;
        score = 0;
    }

    //return is final move
    override public bool MakeMove(int input) {

        lastGuess = input;

        if (LabelsTest[LabelsOffset + imgIndex] == input) {
            score++;
        }

        imgCount++;

        lastIndex = imgIndex;

        imgIndex = _rnd.Next(3000);
        /*if (!IsTraining) {
            imgIndex += 3001;
        }*/

        return imgCount < 500;
    }

    override public void GameStateToInputs(float[] inputsToFill) {
        
        for (int i = 0; i < imageArea; i++)  {
            inputsToFill[i] = ImagesTest[ImagesOffset + imgIndex * imageArea + i] / 255f;
        }
    }

   

    override public float GetScore() {
        return score;
    }
}
