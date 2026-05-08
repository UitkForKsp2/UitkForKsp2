using System;
using System.Linq;
using System.Reflection;
using ReduxLib.Engine;
using UitkForKsp2.API;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2.Panel;

internal static class PanelFactory
{
    private static readonly MethodInfo ApplyPanelSettings = typeof(PanelSettings).GetMethod(
        "ApplyPanelSettings",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
    )!;

    private static readonly PropertyInfo PanelProperty = typeof(PanelSettings).GetProperty(
        "panel",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
    )!;

    private static readonly PropertyInfo SelectableGameObjectProperty = PanelProperty.PropertyType.GetProperty(
        "selectableGameObject",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
    )!;

    public static PanelSettings CreateForWindow(WindowOptions options)
    {
        #if UNITY_EDITOR
        PanelSettings baseAsset = AssetDatabase.FindAssets("t:PanelSettings KerbalPanelSettings")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<PanelSettings>)
            .First(ps => ps.name == (options.UseStockScale ? "KerbalPanelSettings" : "FixedKerbalPanelSettings"));
        #else
        PanelSettings baseAsset = options.UseStockScale
            ? UitkForKsp2Plugin.PanelSettings
            : UitkForKsp2Plugin.FixedPanelSettings;
        #endif

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
        object panel = PanelProperty.GetValue(panelSettings);
        if (panel == null)
        {
            return;
        }

        var go = SelectableGameObjectProperty.GetValue(panel) as GameObject;
        if (go == null)
        {
            return;
        }

        go.layer = Layers.LayerUi;
    }
}