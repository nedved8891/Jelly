using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EatScript : MonoBehaviour {
	
	public int index;

	Tween twn;

	Collider2D coll;

	void Awake(){
		coll = GetComponentInChildren<Collider2D> ();
	}

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
		if (twn != null) {
			twn.Kill ();
			twn = null;
		}

		transform.localScale = Vector3.one;
	}

	void GameManager_OnGameStarted ()
	{
		twn = transform.DOScale (new Vector3(1.4f, 1.4f, 1.4f), 1).SetLoops(-1, LoopType.Yoyo);
	}
}
