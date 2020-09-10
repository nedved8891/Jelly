using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class SetSkins : MonoBehaviour {

	SkeletonAnimation fish;

	public enum SkinsName
	{
		Fish1,
		Fish2,
		Fish3,
		Fish4,
		Fish5
	}

	// Use this for initialization
	void Awake () {
		fish = GetComponent<SkeletonAnimation> ();

		fish.skeleton.SetSkin (((SkinsName)Random.Range (0, 4)).ToString());
	}
}
