using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class NumberReader : MonoBehaviour {

    public Image DisplayImage;
    public Text DisplayNumber;

    private Texture2D DisplayTexture;

    private byte[] ImagesTest;


	// Use this for initialization
	void Start () {

        ImagesTest = File.ReadAllBytes("Assets/t10k-images.idx3-ubyte");

        for (int i = 0; i < 100; i++) {

            print(ImagesTest[i].ToString("X")); 
        }


        int offset = 4 * 4 + Random.Range(0, 200) * 28 * 28;



        DisplayTexture = new Texture2D(28, 28);

        for (int i = DisplayTexture.width - 1; i >=0 ; i--) {
            for (int j = 0; j < DisplayTexture.height; j++) {
                DisplayTexture.SetPixel(j, i, new Color(ImagesTest[offset] / 255f , ImagesTest[offset] / 255f, ImagesTest[offset] / 255f));
                offset += 1;
            }
        }
        DisplayTexture.Apply();
        DisplayImage.sprite = Sprite.Create(DisplayTexture, new Rect(0, 0, 28, 28), new Vector2());

        
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
