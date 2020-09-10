using UnityEngine;
using UnityEngine.UI;
using System.Collections;


namespace BSH_Prototype
{
	public static class RectTransformUtils
	{

		public static void CenterPivot (this RectTransform rt)
		{
			Vector2 old_pivot = rt.pivot;
			Vector2 new_pivot = Vector2.one * 0.5f;
			rt.pivot = new_pivot;
			Vector2 delta = new_pivot - old_pivot;
			rt.anchoredPosition += MultiplyV2 ( new Vector2 [] { delta, rt.sizeDelta, rt.localScale } );
		}

		public static Vector2 MultiplyV2 (Vector2 [] items)
		{
			Vector2 res = Vector2.one;
			if (items != null)
			{
				for (int i = 0; i < items.Length; i++)
				{
					res.x *= items [i].x;
					res.y *= items [i].y;
				}
			}
			return res;
		}
	}
}
