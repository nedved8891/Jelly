using UnityEngine;
using UnityEngine.UI;
using BSH_Prototype;

[RequireComponent(typeof(Button))]
public class GoToSceneButton : ButtonClickHandler
{
	public Scenes target;

    protected override void OnButtonClick ()
    {
		SceneLoader.Instance.SwitchToScene (target);
    }

	public void Click(){
		SceneLoader.Instance.SwitchToScene ( target );
	}
}
