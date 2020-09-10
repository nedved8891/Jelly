using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using BSH_Prototype;
using Spine;

public class PlayButtonWithAnimation : MonoBehaviour {

	public Scenes target;

	SkeletonAnimation skltn;

	/// <summary>
	/// Vstanovyty pidpysku na eventy
	/// </summary>
	public void OnEnable() {
		EasyTouch.On_TouchStart += OnTouchDown;
	}

	/// <summary>
	/// Vtratyty pidpysku na eventy
	/// </summary>
	public void OnDisable() {
		EasyTouch.On_TouchStart -= OnTouchDown;
	}

	// Use this for initialization
	void Start () {
		skltn = GetComponent<SkeletonAnimation> ();
		skltn.state.SetAnimation (0, "Idle", true);

		skltn.state.Complete += OnCompleteAnimation;
	}
	
	public void OnTouchDown(Gesture gesture) {
		if (gesture.pickedObject == gameObject) {
			skltn.state.SetAnimation (0, "OnClick", false);
			AudioController.PlaySound ("Button");
		}
	}

	public void OnCompleteAnimation(TrackEntry trackEntry){
		if (trackEntry.animation.name == "OnClick") {
			skltn.state.SetAnimation (0, "Idle", true);
			SceneLoader.Instance.SwitchToScene ( target );
		}
	}
}
