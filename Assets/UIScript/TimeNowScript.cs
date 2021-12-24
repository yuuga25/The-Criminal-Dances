using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class TimeNowScript : MonoBehaviour
{
    public Text TimeText_Year;
    public Text TimeText_Month;
    public Text TimeText_Day;
    public Text TimeText_DayOfWeek;
    public Text TimeText_Time;
    private void Update()
    {
        TimeText_Year.text = DateTime.Now.Year.ToString();
        TimeText_Month.text = DateTime.Now.Month.ToString();
        TimeText_Day.text = DateTime.Now.Day.ToString();
        TimeText_DayOfWeek.text = DateTime.Now.DayOfWeek.ToString();
        var h = DateTime.Now.Hour.ToString("00");
        var m = DateTime.Now.Minute.ToString("00");
        var s = DateTime.Now.Second.ToString("00");
        var ms = DateTime.Now.Millisecond.ToString("000");
        TimeText_Time.text = $"{h}:{m}:{s}.{ms}";
    }
}
