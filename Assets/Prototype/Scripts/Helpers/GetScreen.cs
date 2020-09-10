using UnityEngine;
using System.Collections;

public static class GetScreen
{
	private static string cameraName = "GUI Camera";

	public static Vector3 ScreenToWorldPoint (Vector3 point)
	{
		Camera camera = Camera.main;
		for (int i = 0; i < Camera.allCamerasCount; i++) {
			if (Camera.allCameras [i].name == cameraName) {
				camera = Camera.allCameras [i];
			}
		}
		return camera.ScreenToWorldPoint (point);
	}

	public static Vector2 WorldToScreenPoint (Vector3 point)
	{
		Camera camera = Camera.main;
		for (int i = 0; i < Camera.allCamerasCount; i++) {
			if (Camera.allCameras [i].name == cameraName) {
				camera = Camera.allCameras [i];
			}
		}
		return camera.WorldToScreenPoint (point);
	}

	public static float left {
		get {
			return ScreenToWorldPoint (new Vector3 (0, 0, 0)).x;
		}
	}

	public static float right {
		get {
//			if (!Application.isPlaying) 
#if UNITY_EDITOR			
			return Camera.main.orthographicSize * 2 * Camera.main.aspect * 0.5f;
#else
			return ScreenToWorldPoint (new Vector3 (Screen.width, Screen.height, 0)).x;
#endif
		}
	}

	public static float bottom {
		get {
			return ScreenToWorldPoint (new Vector3 (0, 0, 0)).y;
		}
	}

	public static float top {
		get {
//			if (!Application.isPlaying)
#if UNITY_EDITOR						
			return Camera.main.orthographicSize;
//			else
#else
			return ScreenToWorldPoint (new Vector3 (Screen.width, Screen.height, 0)).y;
#endif
		}
	}

	public static float width {
		get {
			return right - left;
		}
	}

	public static float height {
		get {
			return top - bottom;
		}
	}
}
