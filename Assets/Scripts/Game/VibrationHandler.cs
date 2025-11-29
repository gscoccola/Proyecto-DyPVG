using UnityEngine;
using System.Runtime.InteropServices;

#if !UNITY_WEBGL
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
#else
        HapticFeedback.LightFeedback();
#endif

    }

    public void MediumVibrate()
    {
#if  UNITY_WEBGL
            //Vibrate(100);
#else
        HapticFeedback.MediumFeedback();
#endif

    }

    public void HeavyVibrate()
    {
#if  UNITY_WEBGL
            //Vibrate(250);
#else
        HapticFeedback.HeavyFeedback();

#endif
    }
}
