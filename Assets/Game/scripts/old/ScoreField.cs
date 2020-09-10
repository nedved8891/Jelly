using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreField : MonoBehaviour {

	public GameObject img;

	void OnEnable() {
		GameManager.OnGameStarted += GameManager_OnGameStarted;;
		TapController.OnPlayerDied += TapController_OnPlayerDied;;
	}

	void OnDisable() {
		GameManager.OnGameStarted -= GameManager_OnGameStarted;
		TapController.OnPlayerDied -= TapController_OnPlayerDied;
	}

	void TapController_OnPlayerDied ()
	{
		img.SetActive (false);
	}

	void GameManager_OnGameStarted ()
	{
		img.SetActive (true);
	}
}
