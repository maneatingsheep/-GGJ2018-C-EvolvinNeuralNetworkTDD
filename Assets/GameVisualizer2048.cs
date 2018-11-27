using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameVisualizer2048 : BaseGameVisualizer {
    public float TimBetweenFramesSec;

    public VisualTile[] BoardTiles;

    public bool DoUseNuttons;
         

    private float _lastMoveTime = 0;

    override internal void InitGameVis() {
        for (int i = 0; i < BoardTiles.Length; i++) {
            BoardTiles[i].Src = (SrcSimulation.Game as Game2048).Tiles[Mathf.FloorToInt(i / 4)][i % 4];
        }
    }

	override internal void ResetGame() {
        SrcSimulation.Game.ResetGame(0);
        RenderTiles();
    }

    void Update() {
        if (DoUseNuttons) {
            if (UseButtons()) {
                RenderTiles();
            }
        } else {
            if (IsPlaying && _lastMoveTime < Time.time - TimBetweenFramesSec) {
                _lastMoveTime = Time.time;
                IsPlaying &= !SrcSimulation.PlayOneMove();

                RenderTiles();
            }
        }
       
    }

    private void RenderTiles() {
        for (int i = 0; i < BoardTiles.Length; i++) {
            BoardTiles[i].Render();
        }
    }

    public bool UseButtons() {
        bool keyPressed = false;
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            SrcSimulation.Game.MakeMove(0);
            keyPressed = true;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            SrcSimulation.Game.MakeMove(1);
            keyPressed = true;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            SrcSimulation.Game.MakeMove(2);
            keyPressed = true;
        } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            SrcSimulation.Game.MakeMove(3);
            keyPressed = true;
        }

        return keyPressed;
    }

}
