using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Musikbackground : MonoBehaviour {

	AudioSource audio;

	// Use this for initialization
	void Awake () {
		audio = GetComponent<AudioSource> ();
	}
	
	// Update is called once per frame
	void Start () {
		audio.volume = PlayerPrefs.GetFloat("MusicVolume", 1);
	}
}
