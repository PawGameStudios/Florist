using System;
using System.Collections;
using UnityEngine;

public class Timer : MonoSingleton<Timer>
{
    public static double CurrentTotalSeconds
    {
        get
        {
            DateTime epochStart = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (DateTime.UtcNow - epochStart).TotalSeconds;
        }
    }
    public static Action TimeTickSeconds;
    public static Action TimeTickMiliseconds;
    private WaitForSeconds _waitMs = new(.1f);

    private void OnEnable()
    {
        StartCoroutine(Tick());
    }

    private IEnumerator Tick()
    {
        int time = 0;
        while (true)
        {
            yield return _waitMs;
            time++;
            if (time % 10 == 0)
            {
                TimeTickSeconds?.Invoke();
                time = 0;
            }
            TimeTickMiliseconds?.Invoke();
        }
    }
}
