using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameVisualizerNumberReader : BaseGameVisualizer {

    public float TimBetweenFramesSec;

    public Image DisplayImage;
    public Text DisplayGuess;
    public Text DisplayLabel;

    private Texture2D DisplayTexture;

    private float _lastMoveTime = 0;


    void Update() {

        if (IsPlaying && _lastMoveTime < Time.time - TimBetweenFramesSec) {

            SrcSimulation.Game.IsTraining = false;
            IsPlaying &= !SrcSimulation.PlayOneMove();
            SrcSimulation.Game.IsTraining = true;

            GameNumberReader game = (SrcSimulation.Game as GameNumberReader);
            byte pixel;
            int count = 0;
            for (int i = 0; i < game.imageSideSize; i++) {
                for (int j = 0; j < game.imageSideSize; j++) {
                    pixel = GameNumberReader.ImagesTest[game.ImagesOffset + game.lastIndex * game.imageArea + count]; //-1 because the index moved
                    DisplayTexture.SetPixel(j, game.imageSideSize - 1 - i, new Color(pixel / 255f, pixel / 255f, pixel / 255f));
                    count++;
                }

            }


            DisplayTexture.Apply();
            


            DisplayLabel.text = GameNumberReader.LabelsTest[game.LabelsOffset + game.lastIndex].ToString(); //-1 because the index moved
            DisplayGuess.text = game.lastGuess.ToString(); //-1 because the index moved

            _lastMoveTime = Time.time;
            

        }

        

    }

    override internal void InitGameVis() {
        DisplayTexture = new Texture2D(28, 28);
        DisplayImage.sprite = Sprite.Create(DisplayTexture, new Rect(0, 0, 28, 28), new Vector2());
    }

    override internal void ResetGame(int seed) {
        SrcSimulation.Game.ResetGame(seed);
    }
}
