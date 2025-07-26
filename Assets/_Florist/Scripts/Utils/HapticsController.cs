using UnityEngine;
using CandyCoded.HapticFeedback;

public class HapticsController : MonoBehaviour
{
    public static HapticsController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void PlayButtonHaptic()
    {
        if (Instance == null)
        {
            Debug.LogWarning("HapticsController instance is null. Make sure it is initialized before calling PlayButtonHaptic.");
            return;
        }

        HapticFeedback.HeavyFeedback();
    }

    public static void PlayMediumHaptic()
    {
        if (Instance == null)
        {
            Debug.LogWarning("HapticsController instance is null. Make sure it is initialized before calling PlaySuccessHaptic.");
            return;
        }

        HapticFeedback.MediumFeedback();
    }

    public static void PlayLightHaptic()
    {
        if (Instance == null)
        {
            Debug.LogWarning("HapticsController instance is null. Make sure it is initialized before calling PlaySuccessHaptic.");
            return;
        }

        HapticFeedback.LightFeedback();
    }
}
