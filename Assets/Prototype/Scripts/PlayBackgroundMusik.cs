using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayBackgroundMusik : MonoBehaviour {

	// Use this for initialization
	void Start () {
		AudioController.PlayMusic ("Background", 0.2f);
	}

}
