using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSpriteForFish : MonoBehaviour {

	public List<Sprite> sprites;

	// Use this for initialization
	void Start () {
		GetComponent<SpriteRenderer>().sprite = sprites[PlayerPrefs.GetInt("BackgroundIndex")];
	}
	

}
