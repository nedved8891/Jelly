using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NotificationSamples;

public class NotificationsManager : MonoBehaviour
{
    public static NotificationsManager Instance;
    
    public float timeShow = 86400;
    
    [SerializeField] private GameNotificationsManager notificationsManager;
    private int notificationDelay;

    private void Initializedotifications()
    {
        GameNotificationChannel channel = new GameNotificationChannel("mntutorial", "Mobile Notification", "Just a notification");
        notificationsManager.Initialize(channel);
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Initializedotifications();
        CreateNotification();
    }

    public void OnTimeInput(string text)
    {
        if (int.TryParse(text, out int sec))
        {
            notificationDelay = sec;
        }
    }

    public void CreateNotification()
    {
        CreateNotification("Jelly", "Play now", DateTime.Now.AddSeconds(timeShow));
    }
    
    public void CreateNotification(string title, string body, DateTime time)
    {
        Debug.Log("Start Notification \""+ body +"\"");
        var notification = notificationsManager.CreateNotification();

        if (notification != null)
        {
            notification.Title = title;
            notification.Body = body;
            notification.DeliveryTime = time;
            notification.SmallIcon = "icon_0";
            notificationsManager.ScheduleNotification(notification);
        }
    }
}
