using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScelectSprite : MonoBehaviour {

	public List<Sprite> sprites;

	SpriteRenderer sRender;

	// Use this for initialization
	void Awake () {
		sRender = GetComponent<SpriteRenderer> ();	
	}

	void Start(){
		sRender.sprite = sprites[(int)Random.Range(0.0f, sprites.Count - 1)];
	}

}
