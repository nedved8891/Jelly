using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Toggles a sound source when clicked on. It also records the sound state (on/off) in a PlayerPrefs. 
/// In order to detect clicks you need to attach this script to a UI Button and set the proper OnClick() event.
/// </summary>
public class ToggleSounds : MonoBehaviour
{
	//The tag of the sound object
	public string soundObjectTag = "GameController";

	// The source of the sound
	public Transform soundObject;

	// The PlayerPrefs name of the sound
	public string playerPref = "SoundVolume";

	// The index of the current value of the sound
	public float currentState = 1;

	void Awake()
	{
		if ( !soundObject && soundObjectTag != string.Empty )    soundObject = GameObject.FindGameObjectWithTag(soundObjectTag).transform;

		// Get the current state of the sound from PlayerPrefs
		if( soundObject )
			currentState = PlayerPrefs.GetFloat(playerPref, soundObject.GetComponent<AudioSource>().volume);
		else   
			currentState = PlayerPrefs.GetFloat(playerPref, currentState);
	
		// Set the sound in the sound source
		SetSound();
	}

	void SetSound()
	{
		Debug.Log ("currentState: " + currentState);
		if ( !soundObject && soundObjectTag != string.Empty )    soundObject = GameObject.FindGameObjectWithTag(soundObjectTag).transform;

		// Set the sound in the PlayerPrefs
		PlayerPrefs.SetFloat(playerPref, currentState);

		Color newColor = GetComponent<Image>().material.color;

		// Update the graphics of the button image to fit the sound state
		if( currentState == 1 )
			newColor.a = 1;
		else
			newColor.a = 0.5f;

		GetComponent<Image>().color = newColor;

		// Set the value of the sound state to the source object
		if( soundObject ) 
			soundObject.GetComponent<AudioSource>().volume = currentState;
	}

	/// <summary>
	/// Toggle the sound. Cycle through all sound modes and set the volume and icon accordingly
	/// </summary>
	public void ToggleSound()
	{
		currentState = 1 - currentState;
	
		SetSound();
	}

	/// <summary>
	/// Starts the sound source.
	/// </summary>
	void StartSound()
	{
		if( soundObject )
			soundObject.GetComponent<AudioSource>().Play();
	}
}
