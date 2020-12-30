using System;
using UnityEngine;

public class TimerSvc : MonoSingleton<TimerSvc>
{
    private PETimer _timer = null;
    public void InitSvc()
    {
        _timer = new PETimer();
        _timer.SetLog((string info) =>
        {
            Debug.Log(info);
        });
    }

    public void Update()
    {
        _timer.Update();
    }

    public int AddTimeTask(Action<int> callback, double delay, PETimeUnit timeUnit = PETimeUnit.Millisecond, int count = 1)
    {
        return _timer.AddTimeTask(callback, delay, timeUnit, count);
    }

    public double GetNowTime()
    {
        return _timer.GetMillisecondsTime();
    }
}
