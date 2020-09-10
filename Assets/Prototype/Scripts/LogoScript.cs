using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using BSH_Prototype;

public class LogoScript : MonoBehaviour {

	[Header("Час через скльки секунд покинути сцену логотипу")]
	public float time = 1;

	// Use this for initialization
	void Start () {
		DOVirtual.DelayedCall (time, () => SceneLoader.Instance.SwitchToScene(SceneLoader.Instance.first_scene));
	}
}
