#nullable enable
using UnityEngine;

namespace MonoSandbox.Behaviours.Managers;

public class ThemeManager
{
    // PlacementHandling.cs
    public static Color PlacementHandlingColor = new Color(0.569f, 0.392f, 0.745f, 0.4509804f); // default
    // ItemButton.cs
    public static Color ItemButtonColor1 = new Color32(145, 100, 190, 255); // then piped to new Color32(215, 225, 239, 255);
    public static Color ItemButtonColor2 = new Color32(215, 225, 239, 255); // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    // PageButton.cs
    public static Color PageButtonColor1 = new Color32(145, 100, 190, 255);
    public static Color PageButtonColor2 = new Color32(215, 225, 239, 255);
    // Managers_FreezeManager.cs
    public static Color FreezeManagerColor1 = new Color(0.569f, 0.392f, 0.745f, 0.4509804f);
    public static Color FreezeManagerColor2 = new Color(0.8314f, 0.2471f, 0.1569f, 0.4509804f);
    // ThrusterManager.cs
    public static Color ThrusterManagerColor1 = new Color(0.569f, 0.392f, 0.745f, 0.4509804f);
    public static Color ThrusterManagerColor2 = new Color(0.8314f, 0.2471f, 0.1569f, 0.4509804f);
    
    // -------------------------------------------------------------------------------------------------------- \\

    public static void PurpleTheme() // default
    {
        PlacementHandlingColor = new Color(0.569f, 0.392f, 0.745f, 0.4509804f);
        ItemButtonColor1 = new Color32(145, 100, 190, 255);
        ItemButtonColor2 = new Color32(215, 225, 239, 255);
        PageButtonColor1 = new Color32(145, 100, 190, 255);
        PageButtonColor2 = new Color32(215, 225, 239, 255);
        FreezeManagerColor1 = new Color(0.569f, 0.392f, 0.745f, 0.4509804f);
        FreezeManagerColor2 = new Color(0.8314f, 0.2471f, 0.1569f, 0.4509804f);
        ThrusterManagerColor1 = new Color(0.569f, 0.392f, 0.745f, 0.4509804f);
        ThrusterManagerColor2 = new Color(0.8314f, 0.2471f, 0.1569f, 0.4509804f);
    }

    public static void RedTheme() // my theme
    {
        PlacementHandlingColor = new Color(0.745f, 0.392f, 0.392f, 0.4509804f);
        ItemButtonColor1 = new Color32(190, 100, 100, 255);
        ItemButtonColor2 = new Color32(215, 225, 239, 255);
        PageButtonColor1 = new Color32(190, 100, 100, 255);
        PageButtonColor2 = new Color32(215, 225, 239, 255);
        FreezeManagerColor1 = new Color(0.745f, 0.392f, 0.392f, 0.4509804f);
        FreezeManagerColor2 = new Color(0.8314f, 0.2471f, 0.1569f, 0.4509804f);
        ThrusterManagerColor1 = new Color(0.745f, 0.392f, 0.392f, 0.4509804f);
        ThrusterManagerColor2 = new Color(0.8314f, 0.2471f, 0.1569f, 0.4509804f);
    }

    public static void BlueTheme() // og theme
    {
        PlacementHandlingColor = new Color(0.392f, 0.722f, 0.820f, 0.4509804f);
        ItemButtonColor1 = new Color32(71, 121, 196, 255);
        ItemButtonColor2 = new Color32(215, 225, 239, 255);
        PageButtonColor1 = new Color32(71, 121, 196, 255);
        PageButtonColor2 = new Color32(215, 225, 239, 255);
        FreezeManagerColor1 = new Color(0.392f, 0.722f, 0.820f, 0.4509804f);
        FreezeManagerColor2 = new Color(0.8314f, 0.2471f, 0.1569f, 0.4509804f);
        ThrusterManagerColor1 = new Color(0.392f, 0.722f, 0.820f, 0.4509804f);
        ThrusterManagerColor2 = new Color(0.8314f, 0.2471f, 0.1569f, 0.4509804f);
    }
    
    public static void GreenTheme() // cool new green theme
    {
        PlacementHandlingColor = new Color(0.41f, 0.57f, 0.42f, 0.4509804f);
        ItemButtonColor1 = new Color32(104, 145, 106, 255);
        ItemButtonColor2 = new Color32(215, 225, 239, 255);
        PageButtonColor1 = new Color32(104, 145, 106, 255);
        PageButtonColor2 = new Color32(215, 225, 239, 255);
        FreezeManagerColor1 = new Color(0.41f, 0.57f, 0.42f, 0.4509804f);
        FreezeManagerColor2 = new Color(0.8314f, 0.2471f, 0.1569f, 0.4509804f);
        ThrusterManagerColor1 = new Color(0.41f, 0.57f, 0.42f, 0.4509804f);
        ThrusterManagerColor2 = new Color(0.8314f, 0.2471f, 0.1569f, 0.4509804f);
    }
}

/*
 * placement handeling: new Color(0.569f, 0.392f, 0.745f, 0.4509804f);
 * item buttons: new Color32(145, 100, 190, 255) : new Color32(215, 225, 239, 255);
 * page buttons: new Color32(145, 100, 190, 255) : new Color32(215, 225, 239, 255);
 * freeze manager: new Color(0.569f, 0.392f, 0.745f, 0.4509804f) : new Color(0.8314f, 0.2471f, 0.1569f, 0.4509804f);
 * thruster manager: new Color(0.569f, 0.392f, 0.745f, 0.4509804f) : new Color(0.8314f, 0.2471f, 0.1569f, 0.4509804f);
*/