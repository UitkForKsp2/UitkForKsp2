using System;
using System.Reflection;
using UitkForKsp2.API;
using UnityEngine.UIElements;

namespace UitkForKsp2.Panel;

internal static class PanelFactory
{
    private static readonly MethodInfo ApplyPanelSettings = typeof(PanelSettings).GetMethod(
        "ApplyPanelSettings",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
    )!;

    public static PanelSettings CreateForWindow(WindowOptions options)
    {
        PanelSettings baseAsset = options.UseStockScale
            ? UitkForKsp2Plugin.PanelSettings
            : UitkForKsp2Plugin.FixedPanelSettings;

        PanelSettings panelSettings = UnityObject.Instantiate(baseAsset)!;

        string name = options.WindowId ?? Guid.NewGuid().ToString();
        panelSettings.name = $"PS[{(options.UseStockScale ? "Scaled" : "Fixed")}]::{name}";

        return panelSettings;
    }

    public static void Apply(PanelSettings panelSettings)
    {
        if (panelSettings == null)
        {
            return;
        }

        ApplyPanelSettings.Invoke(panelSettings, Array.Empty<object>());
    }
}