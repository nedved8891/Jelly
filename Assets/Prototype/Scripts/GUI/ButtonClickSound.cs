using UnityEngine;
using UnityEngine.UI;
using BSH_Prototype;

public class ButtonClickSound : MonoBehaviour
{
	public new bool enabled { get; set; }

	public void EnableListener( bool enabled )
	{
		this.enabled = enabled;
	}

	private void Awake()
	{
		enabled = true;
		Button button = GetComponent<Button>();
		if (button)
		{
			button.onClick.AddListener( PlayOnClick );
		}
		else
		{
			Toggle toggle = GetComponent<Toggle>();
			if (toggle)
			{
				toggle.onValueChanged.AddListener( ToggleChanged );
			}
		}
	}

	private void ToggleChanged( bool isOn )
	{
		PlayOnClick();
	}

	public void PlayOnClick()
	{
		if (enabled)
		{
			if (PlayerPrefs.GetFloat("SoundVolume") == 1)
				AudioController.PlaySound ("Button");
		}
	}
}
