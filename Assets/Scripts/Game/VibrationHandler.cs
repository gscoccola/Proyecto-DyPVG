using UnityEngine;
using System.Runtime.InteropServices;

#if UNITY_ANDROID
using CandyCoded.HapticFeedback;
#endif


public class VibrationHandler : Singleton<VibrationHandler>
{
#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void Vibrate(int ms);
#endif

    public void LightVibrate()
    {
#if UNITY_WEBGL
            //Vibrate(50);
#elif UNITY_ANDROID
        HapticFeedback.LightFeedback();
#endif

    }

    public void MediumVibrate()
    {
#if  UNITY_WEBGL
            //Vibrate(100);
#elif UNITY_ANDROID
        HapticFeedback.MediumFeedback();
#endif

    }

    public void HeavyVibrate()
    {
#if  UNITY_WEBGL
            //Vibrate(250);
#elif UNITY_ANDROID
        HapticFeedback.HeavyFeedback();

#endif
    }
}
