using UnityEngine;
using System;

[Serializable]
public class LandingBonus 
{
	//How far from the center of the platform we landed
	public float landDistance = 0.5f;

	//How many points we get when landing at this distance
	public int bonusValue = 100;
}
