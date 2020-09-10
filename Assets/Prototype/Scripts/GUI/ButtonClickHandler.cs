using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// ChangeLog
/// - fixed method name OnButtonCLick > OnButtonClick
/// - fixed centeriong pivot
/// - made button component protected
/// </summary>

namespace BSH_Prototype
{

	[RequireComponent ( typeof ( Button ) )]
	public class ButtonClickHandler :MonoBehaviour
	{
		public bool
			center_pivot = true;

		protected Button button;
		protected RectTransform rectTransform;

		virtual protected void Awake ()
		{
			rectTransform = GetComponent<RectTransform> ( );
			button = GetComponent<Button> ( );
			button.onClick.AddListener ( OnButtonClick );
			Animator anim = GetComponent<Animator> ( );
			if (anim != null)
			{
				anim.updateMode = AnimatorUpdateMode.UnscaledTime;
			}
			if (center_pivot)
			{
				rectTransform.CenterPivot ( );
			}
		}

		virtual protected void OnButtonClick ()
		{

		}
	}
}
