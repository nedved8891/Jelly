using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BSH_Prototype; 

public class LoadLevelScript : MonoBehaviour {

	/// <summary>
	/// Loads the URL.
	/// </summary>
	/// <param name="urlName">URL/URI</param>
	public void LoadURL(string urlName)
	{
		Application.OpenURL(urlName);
	}

	/// <summary>
	/// Loads the level.
	/// </summary>
	/// <param name="levelName">Level name.</param>
	public void LoadMainMenu()
	{
		GameController.Instance.MainMenu ();
	}

	/// <summary>
	/// Restarts the current level.
	/// </summary>
	public void RestartLevel()
	{
		GameController.Instance.Restart ();
	}
}
