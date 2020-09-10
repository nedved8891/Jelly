using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class CoinScript : MonoBehaviour {

	public int index;

	SkeletonAnimation star;

	Collider2D coll;

	void Awake(){
		star = GetComponent<SkeletonAnimation> ();
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
		star.state.TimeScale = 0;
	}

	void GameManager_OnGameStarted ()
	{
		star.state.TimeScale = 1;
		if (Random.Range(0, 100) > 50){
			star.state.SetAnimation (0, "Idle", true);
		}else{
			star.state.SetAnimation (0, "Idle2", true);
		}
	}
}
