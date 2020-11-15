using System;
using System.Collections.Generic;
using DG.Tweening;
using GleyDailyRewards;
using UnityEngine;
using UnityEngine.UI;

public class CalendarExample : MonoBehaviour
{
    public delegate void ShowPopUpHat(int value);
    public static ShowPopUpHat OnShowPopUpHat;
    
    public Text UIRewardText;
    private int reward;

    private DailyRewardsSettings dailyRewardsSettings;

    public List<Sprite> hats;

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void Start()
    {
        //uncomment this to clear your save
        //PlayerPrefs.DeleteAll();
        
        dailyRewardsSettings = Resources.Load<DailyRewardsSettings>("DailyRewardsSettingsData");

        if (PlayerPrefs.GetInt("Hat", 0) < 4 && (!PlayerPrefs.HasKey("DailyRewardSavedTime") || (PlayerPrefs.GetString("DailyRewardSavedTime") != DateTime.Now.ToBinary().ToString())))
        {
            DOVirtual.DelayedCall(1.5f, ShowCalendar);
            
            if(NotificationsManager.Instance)
                NotificationsManager.Instance.CreateNotification("Jelly", "Take the bonus!", DateTime.Now.AddSeconds(dailyRewardsSettings.hours * 360 + dailyRewardsSettings.minutes * 60));
            else
            {
                Debug.Log("NotificationsManager.Instance not found");
            }
        }
       
        //You can add this listener anywhere in your code and your method will be called every time a Day Button is clicked
        GleyDailyRewards.Calendar.AddClickListener(CalendarButtonClicked);
    }

    /// <summary>
    /// Triggered every time a day button is clicked
    /// </summary>
    /// <param name="dayNumber">current clicked day</param>
    /// <param name="rewardValue">the reward value for current day</param>
    /// <param name="rewardSprite">the sprite of the reward</param>
    private void CalendarButtonClicked(int dayNumber, int rewardValue, Sprite rewardSprite)
    {
        Debug.Log("Click " + dayNumber + " " + rewardValue);

        if (dayNumber == 5)
        {
            var idHat = PlayerPrefs.GetInt("Hat", 0);
            if (idHat < 5)
            {
                idHat++;
                OnShowPopUpHat(idHat);

                PlayerPrefs.SetInt("Hat", idHat);
                PlayerPrefs.Save();
            }
        }
        
        reward += rewardValue;
        UIRewardText.text = reward.ToString();
    }


    public void ShowCalendar()
    {
        //call this method anywhere in your code to open the Calendar Popup
        GleyDailyRewards.Calendar.Show();
    }
}




