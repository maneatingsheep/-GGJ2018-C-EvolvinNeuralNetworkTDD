using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameVisualizer : MonoBehaviour {

    public Simulation SrcSimulation;

    public bool IsPlaying;

    abstract internal void InitGameVis();

    abstract internal void ResetGame();

}
