using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlowMaster : MonoBehaviour {

    public BaseGameVisualizer GameVis;
    public GameObject LearnScreen;

    public Canvas MenuCanvas;
    public Canvas LearnCanvas;
    public Canvas GameCanvas;

    public enum ScreenModes { Menu, Play, Learn};

    public LearningManager LearningMngr;

    private ScreenModes _screenMode;

    public ScreenModes ScreenMode {
        get {
            return _screenMode;
        }

        set {
            switch (value) {
                case ScreenModes.Menu:
                    MenuCanvas.gameObject.SetActive(true);
                    LearnCanvas.gameObject.SetActive(false);
                    GameCanvas.gameObject.SetActive(false);
                    break;
                case ScreenModes.Play:
                    MenuCanvas.gameObject.SetActive(false);
                    LearnCanvas.gameObject.SetActive(false);
                    GameCanvas.gameObject.SetActive(true);
                    break;
                case ScreenModes.Learn:
                    MenuCanvas.gameObject.SetActive(false);
                    LearnCanvas.gameObject.SetActive(true);
                    GameCanvas.gameObject.SetActive(false);
                    break;
            }

            _screenMode = value;
        }
    }

    public void StartLearning() {
        ScreenMode = ScreenModes.Learn;
        LearningMngr.IsLearning = true;
        GameVis.IsPlaying = false;
    }

    public void StartReplay() {
        ScreenMode = ScreenModes.Play;
        GameVis.ResetGame(Simulation.Seeds[0]);
        GameVis.IsPlaying = true;
    }

    public void BackToMenu() {
        LearningMngr.IsLearning = false;
        GameVis.IsPlaying = false;
        ScreenMode = ScreenModes.Menu;
    }

    // Use this for initialization
    void Start () {
        BackToMenu();
        LearningMngr.Init();
        GameVis.SrcSimulation = LearningMngr.Simulations[0];
        GameVis.InitGameVis();
        ScreenMode = ScreenModes.Menu;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
