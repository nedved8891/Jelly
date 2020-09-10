using UnityEngine;
using System.Collections;
using DG.Tweening;


#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class Alignment : MonoBehaviour
{
	public enum vAlignment
	{
		none,
		top,
		center,
		bottom
	}
	public enum hAlignment
	{
		none,
		left,
		center,
		right
	}

	public bool
		relative = false;

	private Vector2 
		c;

	private float
		h,
		w;

	public vAlignment vAlign = vAlignment.none;
	public hAlignment hAlign = hAlignment.none;

	public float xOffset = 0f;
	public float yOffset = 0f;


	private void GetBorders ()
	{
		c = new Vector2 ((GetScreen.right + GetScreen.left) * 0.5f, (GetScreen.top + GetScreen.bottom) * 0.5f);
		w = GetScreen.right - c.x;
		h = GetScreen.top - c.y;
	}


	public void SetAlignment ()
	{
		GetBorders ();
		Vector3 pos = transform.position;
		if (hAlign == hAlignment.left) {
			pos.x = c.x - w; // GetScreen.left;
		} else if (hAlign == hAlignment.right) {
			pos.x = w; //GetScreen.right;
		} else if (hAlign == hAlignment.center) {
			pos.x = c.x; //(GetScreen.right + GetScreen.left) * 0.5f;
		}

		if (vAlign == vAlignment.top) {
			pos.y = h; //GetScreen.top;
		} else if (vAlign == vAlignment.bottom) {
			pos.y = c.y - h; //GetScreen.bottom;
		} else if (vAlign == vAlignment.center) {
			pos.y = c.y; //(GetScreen.top + GetScreen.bottom) * 0.5f;
		}
		if (relative) {
			if (hAlign != hAlignment.center) {
				pos.x *= xOffset;
			} else {
				pos.x += w * xOffset;
			}

			if (vAlign != vAlignment.center) {
				pos.y *= yOffset;
			} else {
				pos.y += h * yOffset;				
			}
		} else {
			pos = pos + new Vector3 (xOffset, yOffset);
		}
		transform.position = pos;
	}

	public void GetRelativePosition ()
	{
		vAlign = vAlignment.center;
		hAlign = hAlignment.center;
		GetBorders ();
		Vector3 pos = transform.position;
		if (w != 0) {
			xOffset = (pos.x - c.x) / w;
		}
		if (h != 0) {
			yOffset = (pos.y - c.y) / h;			
		}
	}

	void Start ()
	{
		SetAlignment ();
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(Alignment))]
	[CanEditMultipleObjects]
	public class AlignerEditor : Editor
	{
		public override void OnInspectorGUI ()
		{
			DrawDefaultInspector ();
			
			Alignment Aligner = (Alignment)target;
			if (GUILayout.Button ("Align")) {
				Aligner.SetAlignment ();
			}

			if (Aligner.relative) {
				if (GUILayout.Button ("GetRelativePosition")) {
					Aligner.GetRelativePosition ();
				}
			}
		}
	}

	//void LateUpdate()
	//{
	//	print ("LateUpdate");
	//	SetAlignment ();
	//}

	#endif

	/// /////////////////////
	public delegate void PipeIsMovedDelegate();
	public static event PipeIsMovedDelegate PipeIsMoved;
	public void Show(float time = 0.5f, float offsetX = 0){
		DOVirtual.Float ( xOffset, offsetX, time, ChangeMoneyValue ).OnComplete(()=>{
		});
	}

	public void Hide(float time = 1, float offsetX = -2){
		DOVirtual.Float ( xOffset, offsetX, time, ChangeMoneyValue ).OnComplete(()=>{
			SetOffset (100, 100);
		});
	}

	private void ChangeMoneyValue (float value )
	{
		xOffset = value;
		SetAlignment ();
	}

	public void SetOffset(float x, float y){
		xOffset = x;
		yOffset = y;

		SetAlignment ();
	}
	/// /////////////////////
}


