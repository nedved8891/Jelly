using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetricaController : MonoBehaviour
{
    public static MetricaController Instance;

    IYandexAppMetrica metrica;

    private PopUp popupWindow = new PopUp();

    private const string DEFAULT_EVENT = "level_start";
    private const string DEFAULT_KEY = "key";
    private const string DEFAULT_VALUE = "value";

    private static string eventValue = DEFAULT_EVENT;
    private Dictionary<string, object> eventParameters = new Dictionary<string, object>();

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        metrica = AppMetrica.Instance;
    }

    private void OnGUI()
    {
        //popupWindow.onGUI();
    }

    public void LevelStart(int _level = 1)
    {

        //Debug.LogError("level " + _level + " start");
        eventValue = "level_start";

        eventParameters ["level"] = _level;

        AppMetrica.Instance.ReportEvent(eventValue,eventParameters);

        SendBuffer();

        //popupWindow.showPopup("Report ("  + eventValue  + " ) + with params");
    }

    public void LevelFinish(int _level = 1, int _time = 0, bool isWin = false, int levelPervent = 0)
    {
        //Debug.LogError("level " + _level + " finish");
        //Debug.LogError("level finish params ");
        eventValue = "level_finish";

        eventParameters ["level"] = _level;
        eventParameters ["result"] = isWin ? "win":"loose";
        eventParameters ["time"] = _time;
        eventParameters ["progress"] = isWin ? "100": levelPervent.ToString();
        eventParameters ["skin"] = "no_skin";
        AppMetrica.Instance.ReportEvent(eventValue, eventParameters);

        SendBuffer();

        //foreach (var k in eventParameters.Keys)
        //{
        //    Debug.LogError("Param " + k + ", value " + eventParameters [k]);
        //}

        //popupWindow.showPopup("Report (" + eventValue + " ) + with params");
    }

    public void SendBuffer()
    {
        metrica.SendEventsBuffer();
    }

    /// <summary>
    /// событие отказа продолжать за просмотр рекламы (у тех у кого реклама еще есть, а у тех у кого нет просто отказ продолжить играть после первой неудачной попытки)
    /// </summary>
    public void ContinueAdsCancel()
    {
        eventValue = "continue_ads_cancel";

        AppMetrica.Instance.ReportEvent(eventValue);

        SendBuffer();
    }

    /// <summary>
    /// сворачивание приложения во время просмотра рекламы для продолжения игры
    /// </summary>
    public void ContinueAdsExit()
    {
        eventValue = "continue_ads_exit";

        AppMetrica.Instance.ReportEvent(eventValue);

        SendBuffer();
    }

    /// <summary>
    /// попытка купить отсутствие рекламы
    /// </summary>
    public void TryingToBuyNoADs()
    {
        eventValue = "TryingToBuyNoADs";

        AppMetrica.Instance.ReportEvent(eventValue);

        SendBuffer();
    }

    /// <summary>
    /// успешно посмотрел рекламу при старте и начал играть
    /// </summary>
    public void StartAdsViewSuccess()
    {
        eventValue = "start_ads_view_ success";

        AppMetrica.Instance.ReportEvent(eventValue);

        SendBuffer();
    }

    /// <summary>
    /// сворачивание приложения/выход на рекламе при старте
    /// </summary>
    public void StartAdsFail()
    {
        eventValue = "start_ads_fail";

        AppMetrica.Instance.ReportEvent(eventValue);

        SendBuffer();
    }

    /// <summary>
    /// успешно посмотрел рекламу за продолжение игры
    /// </summary>
    public void ContinueAdsViewComplete()
    {
        eventValue = "continue_ads_view_complete";

        AppMetrica.Instance.ReportEvent(eventValue);

        SendBuffer();
    }
    
    /// <summary>
    /// успешно посмотрел рекламу за продолжение игры
    /// </summary>
    public void InterstitialAdsViewSuccess()
    {
        eventValue = "interstitial_ads_view_success";

        AppMetrica.Instance.ReportEvent(eventValue);

        SendBuffer();
    }
   
    /// <summary>
    /// сворачивание приложения/выход на межстраничной рекламе после проигрыша
    /// </summary>
    public void  InterstitialAdsFail()
    {
        eventValue = " interstitial_ads_fail";

        AppMetrica.Instance.ReportEvent(eventValue);

        SendBuffer();
    }
}
