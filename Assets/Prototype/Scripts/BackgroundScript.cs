using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScript : MonoBehaviour {

	public List<Sprite> sprites;

	SpriteRenderer sRender;

	int indx;
	// Use this for initialization
	void Awake () {
		sRender = GetComponent<SpriteRenderer> ();
		indx = Random.Range (0, sprites.Count);
		PlayerPrefs.SetInt("BackgroundIndex", indx);
		sRender.sprite = sprites [indx];
	}
}
