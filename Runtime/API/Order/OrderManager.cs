using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2.API.Order;

/// <summary>
/// Provides functionality to manage and maintain the sorting order of per-window <see cref="PanelSettings"/>
/// instances (and Canvases), allowing them to be registered, brought to the front, and unregistered. Each window
/// owns its PanelSettings, and that asset's <c>sortingOrder</c> is what governs z-order both among UITK panels and
/// relative to uGUI canvases. The <see cref="PanelRenderer"/> overloads operate on the renderer's PanelSettings.
/// </summary>
public static class OrderManager
{
    private const int InitialSortingOrder = 1000;
    private const int MaxSortingOrder = 1_000_000;

    private static int _top = InitialSortingOrder;
    private static readonly HashSet<object> Known = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        _top = InitialSortingOrder;
    }

    private static int Next()
    {
        if (_top > MaxSortingOrder)
        {
            RenumberAll();
        }

        return _top++;
    }

    /// <summary>
    /// Registers a <see cref="PanelRenderer"/> window to the order manager by its PanelSettings.
    /// </summary>
    /// <param name="renderer">The window renderer to register. If null, no action is taken.</param>
    public static void Register(PanelRenderer renderer)
    {
        if (renderer != null)
        {
            Register(renderer.panelSettings);
        }
    }

    /// <summary>
    /// Registers a PanelSettings instance to the SortManager.
    /// </summary>
    /// <param name="panelSettings">The PanelSettings instance to register. If null, no action is taken.</param>
    public static void Register(PanelSettings panelSettings)
    {
        if (panelSettings == null || !Known.Add(panelSettings))
        {
            return;
        }

        panelSettings.sortingOrder = Next();
    }

    /// <summary>
    /// Registers a Canvas instance to the SortManager.
    /// </summary>
    /// <param name="canvas">
    /// The Canvas instance to register. If null, or if it is already registered, no action is taken.
    /// </param>
    public static void Register(Canvas canvas)
    {
        if (canvas == null || !Known.Add(canvas))
        {
            return;
        }

        canvas.overrideSorting = true;
        canvas.sortingOrder = Next();
    }

    /// <summary>
    /// Brings a registered <see cref="PanelRenderer"/> window to the front by assigning its PanelSettings the
    /// highest sorting order.
    /// </summary>
    /// <param name="renderer">
    /// The window renderer to bring to the front. If null or not registered, no action is taken.
    /// </param>
    public static void BringToFront(PanelRenderer renderer)
    {
        if (renderer != null)
        {
            BringToFront(renderer.panelSettings);
        }
    }

    /// <summary>
    /// Brings a registered PanelSettings instance to the front by assigning it the highest sorting order.
    /// </summary>
    /// <param name="panelSettings">
    /// The PanelSettings instance to bring to the front. If null or not registered, no action is taken.
    /// </param>
    public static void BringToFront(PanelSettings panelSettings)
    {
        if (panelSettings == null || !Known.Contains(panelSettings))
        {
            return;
        }

        panelSettings.sortingOrder = Next();
    }

    /// <summary>
    /// Brings a registered Canvas instance to the front by assigning it the highest sorting order.
    /// </summary>
    /// <param name="canvas">
    /// The Canvas instance to bring to the front. If null or not registered, no action is taken.
    /// </param>
    public static void BringToFront(Canvas canvas)
    {
        if (canvas == null || !Known.Contains(canvas))
        {
            return;
        }

        canvas.sortingOrder = Next();
    }

    /// <summary>
    /// Unregisters a <see cref="PanelRenderer"/> window from the order manager by its PanelSettings.
    /// </summary>
    /// <param name="renderer">
    /// The window renderer to unregister. If null or not registered, no action is taken.
    /// </param>
    public static void Unregister(PanelRenderer renderer)
    {
        if (renderer != null)
        {
            Unregister(renderer.panelSettings);
        }
    }

    /// <summary>
    /// Unregisters a PanelSettings instance from the SortManager.
    /// </summary>
    /// <param name="panelSettings">
    /// The PanelSettings instance to unregister. If null or not registered, no action is taken.
    /// </param>
    public static void Unregister(PanelSettings panelSettings)
    {
        if (panelSettings == null)
        {
            return;
        }

        Known.Remove(panelSettings);
    }

    /// <summary>
    /// Unregisters a Canvas instance from the SortManager.
    /// </summary>
    /// <param name="canvas">
    /// The Canvas instance to unregister. If null or not registered, no action is taken.
    /// </param>
    public static void Unregister(Canvas canvas)
    {
        if (canvas == null)
        {
            return;
        }

        Known.Remove(canvas);
    }

    private static void RenumberAll()
    {
        var alive = new List<(object obj, int order, int tiebreaker)>();
        foreach (object knownObject in Known.ToArray())
        {
            if (IsDestroyed(knownObject))
            {
                Known.Remove(knownObject);
                continue;
            }

            if (!TryGetOrder(knownObject, out int order))
            {
                continue;
            }

            int tiebreaker = GetTiebreaker(knownObject);
            alive.Add((knownObject, order, tiebreaker));
        }

        alive.Sort((a, b) =>
        {
            int comparison = a.order.CompareTo(b.order);
            return comparison != 0 ? comparison : a.tiebreaker.CompareTo(b.tiebreaker);
        });

        int current = InitialSortingOrder;
        foreach ((object obj, _, _) in alive)
        {
            SetOrder(obj, current++);
        }
        _top = current;
    }

    private static bool IsDestroyed(object? o)
    {
        if (o is UnityObject uo)
        {
            return uo == null;
        }

        return o == null;
    }

    private static bool TryGetOrder(object o, out int order)
    {
        switch (o)
        {
            case PanelSettings ps:
                order = Mathf.CeilToInt(ps.sortingOrder);
                return true;
            case Canvas c:
                order = c.sortingOrder;
                return true;
        }
        order = 0;
        return false;
    }

    private static void SetOrder(object o, int order)
    {
        switch (o)
        {
            case PanelSettings ps:
                ps.sortingOrder = order;
                break;
            case Canvas c:
                c.overrideSorting = true;
                c.sortingOrder = order;
                break;
        }
    }

    private static int GetTiebreaker(object o)
    {
        return o is UnityObject uo ? uo.GetEntityId().GetHashCode() : o.GetHashCode();
    }
}