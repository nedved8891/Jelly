using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BSH_Prototype;

public class InitSceneController : MonoBehaviour {
	void Start () {
		
		SceneLoader.Instance.SwitchToScene (SceneLoader.Instance.logo_scene);	
	}
}
