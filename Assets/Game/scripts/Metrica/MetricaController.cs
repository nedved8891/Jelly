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
}
