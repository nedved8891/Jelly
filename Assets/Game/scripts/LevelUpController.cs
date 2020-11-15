using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpController : MonoBehaviour
{
    public Transform shine;

    public Text _txt;
    
    private void OnEnable()
    {
        GameController.OnLevelUp += Show;
    }

    private void OnDisable()
    {
        GameController.OnLevelUp -= Show;
    }

    private void Show(string txt = "Level Up")
    {
        if(shine)
            shine.localScale = Vector3.one;

        if (_txt)
        {
            _txt.text = txt;
            _txt.transform.DOScale(1, 0.3f).SetLoops(4, LoopType.Yoyo).OnComplete(Hide);
        }

        shine.DORotate(new Vector3(0,0,180), 1).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
    }

    private void Hide()
    {
        shine.localScale = Vector3.zero;
        _txt.transform.localScale = Vector3.zero;
    }
}
