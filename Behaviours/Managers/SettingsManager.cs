#nullable enable
using Menu.Mods;

namespace MonoSandbox.Behaviours.Managers;

public class SettingsManager
{
    // ------------- fly speeds -------------- \\
    public static void SlowFly()
    {
        ModsManager.ActualFlySpeed = 4f;
        ModsManager.acceleration = 0.13f;
    }
    
    public static void MediumFly()
    {
        ModsManager.ActualFlySpeed = 8f;
        ModsManager.acceleration = 0.17f;
    }
    
    public static void FastFly()
    {
        ModsManager.ActualFlySpeed = 15f;
        ModsManager.acceleration = 0.4f;
    }
    // ------------- fly speeds -------------- \\
    
    // ------------- speedboost -------------- \\

    public static void SmallSpeedboost()
    {
        ModsManager.speedboost = 9.5f;
    }

    public static void MediumSpeedboost()
    {
        ModsManager.speedboost = 11.5f;
    }

    public static void LargeSpeedboost()
    {
        ModsManager.speedboost = 15.5f;
    }
    // ------------- speedboost -------------- \\
}