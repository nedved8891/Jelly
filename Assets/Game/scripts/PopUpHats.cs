using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PopUpHats : MonoBehaviour
{
   public GameObject panel;

   public Transform hat;

   public Image imgHat;

   public List<Sprite> hats;

   public Transform shine;

   private void OnEnable()
   {
      CalendarExample.OnShowPopUpHat += Show;
   }

   private void OnDisable()
   {
      CalendarExample.OnShowPopUpHat -= Show;
   }

   private void Show(int value)
   {
      imgHat.sprite = hats[value];
      
      panel.SetActive(true);

      shine.DORotate(new Vector3(0,0,180), 3).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);

      hat.DOScale(Vector3.one, 0.3f).OnComplete(() =>
      {
         DOVirtual.DelayedCall(2, Hide);
      });
   }

   private void Hide()
   {
      panel.SetActive(false);
   }
}
