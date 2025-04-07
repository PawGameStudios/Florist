using System;
using System.Collections.Generic;
using MEC;

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

    private void OnEnable()
    {
        Timing.RunCoroutine(Tick().CancelWith(gameObject));
    }

    private IEnumerator<float> Tick()
    {
        int time = 0;
        while (true)
        {
            yield return Timing.WaitForSeconds(.1f);
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
