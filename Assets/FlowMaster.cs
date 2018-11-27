using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlowMaster : MonoBehaviour {

    public BaseGameVisualizer PlayScreen;
    public GameObject LearnScreen;

    public Button LearnButt;
    public Button PlayButt;
    public Button StopButt;
    public Button LoadButt;

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
                    PlayScreen.gameObject.SetActive(false);
                    LearnScreen.gameObject.SetActive(false);
                    LearnButt.gameObject.SetActive(true);
                    PlayButt.gameObject.SetActive(true);
                    StopButt.gameObject.SetActive(false);
                    LoadButt.gameObject.SetActive(true); 
                    break;
                case ScreenModes.Play:
                    PlayScreen.gameObject.SetActive(true);
                    LearnScreen.gameObject.SetActive(false);
                    LearnButt.gameObject.SetActive(false);
                    PlayButt.gameObject.SetActive(false);
                    StopButt.gameObject.SetActive(true);
                    LoadButt.gameObject.SetActive(false);
                    break;
                case ScreenModes.Learn:
                    PlayScreen.gameObject.SetActive(false);
                    LearnScreen.gameObject.SetActive(true);
                    LearnButt.gameObject.SetActive(false);
                    PlayButt.gameObject.SetActive(false);
                    StopButt.gameObject.SetActive(true);
                    LoadButt.gameObject.SetActive(false);
                    break;
            }

            _screenMode = value;
        }
    }

    public void StartLearning() {
        ScreenMode = ScreenModes.Learn;
        LearningMngr.IsLearning = true;
        PlayScreen.IsPlaying = false;
    }

    public void StartReplay() {
        ScreenMode = ScreenModes.Play;
        PlayScreen.ResetGame();
        PlayScreen.IsPlaying = true;
    }

    public void BackToMenu() {
        LearningMngr.IsLearning = false;
        PlayScreen.IsPlaying = false;
        ScreenMode = ScreenModes.Menu;
    }

    // Use this for initialization
    void Start () {
        BackToMenu();
        LearningMngr.Init();
        PlayScreen.SrcSimulation = LearningMngr.ParentSimulation;
        PlayScreen.InitGameVis();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
