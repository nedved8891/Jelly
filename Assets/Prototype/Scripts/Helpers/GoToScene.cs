using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BSH_Prototype;
using DG.Tweening;
using UnityEngine.Advertisements;

public class GoToScene : MonoBehaviour {

	public Scenes target;

	public void _GoToScene (){
		AudioController.PlaySound ("Button", AudioController.StreamGroup.FX, 1f);
		SceneLoader.Instance.SwitchToScene (target);
	}
}
