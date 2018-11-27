using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class VisualTile : MonoBehaviour {

    public Text Display;

    public LogicTile Src;

    public void Render() {

        switch (Src.PossibleAction) {
            case LogicTile.ActionType.None: Display.color = Color.black; break;
            case LogicTile.ActionType.Grow: Display.color = Color.green; break;
            case LogicTile.ActionType.New: Display.color = Color.red; break;
        }

        if (Src.NumValue > 0) {

            Display.text = Mathf.Pow(2, Src.NumValue).ToString();
        } else {
            Display.text = "";
        }
        
    }

   
}
