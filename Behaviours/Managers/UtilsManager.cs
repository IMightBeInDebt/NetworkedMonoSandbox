#nullable enable
using UnityEngine;

namespace MonoSandbox.Behaviours.Managers;

public class UtilsManager
{
    public static void YesRain()
    {
        for (int i = 0; i < BetterDayNightManager.instance.weatherCycle.Length; i++)
            BetterDayNightManager.instance.weatherCycle[i] = BetterDayNightManager.WeatherType.Raining;
        NoSnow();
    }

    public static void NoRain()
    {
        for (int i = 0; i < BetterDayNightManager.instance.weatherCycle.Length; i++)
            BetterDayNightManager.instance.weatherCycle[i] = BetterDayNightManager.WeatherType.None;
    }

    public static void YesSnow() // directly ripped from seralyth so like
    {
        GameObject snowObject = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Environment/WeatherDayNight").transform.Find("snow").gameObject;
        snowObject.SetActive(true);
        snowObject.transform.position += Vector3.one * (true ? 0.004f : -0.004f);
        snowObject.GetComponent<TimeOfDayDependentAudio>().enabled = !true;
        snowObject.transform.Find("snow partic").gameObject.SetActive(true);
        NoRain();
    }

    public static void NoSnow()
    {
        GameObject snowObject = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Environment/WeatherDayNight").transform.Find("snow").gameObject;
        snowObject.SetActive(false);
        snowObject.transform.position += Vector3.one * (false ? 0.004f : -0.004f);
        snowObject.GetComponent<TimeOfDayDependentAudio>().enabled = !false;
        snowObject.transform.Find("snow partic").gameObject.SetActive(false);
    }
}