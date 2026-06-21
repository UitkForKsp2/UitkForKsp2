using UitkForKsp2.API.Order;
using UnityEngine.UIElements;

namespace UitkForKsp2.API.Manipulator;

/// <summary>
/// Allows bringing a UITK window to the front when clicked.
/// </summary>
public class OrderManipulator : PointerManipulator
{
    private readonly PanelRenderer _renderer;

    /// <summary>
    /// Creates a new <see cref="OrderManipulator"/> instance for the given window renderer.
    /// </summary>
    /// <param name="renderer">The window renderer to manage the order for.</param>
    public OrderManipulator(PanelRenderer renderer)
    {
        _renderer = renderer;
        OrderManager.Register(renderer);
        activators!.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
    }

    /// <summary>
    /// Registers the necessary callbacks on the target element.
    /// </summary>
    protected override void RegisterCallbacksOnTarget()
    {
        target!.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
        target!.RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
    }

    /// <summary>
    /// Unregisters the callbacks from the target element.
    /// </summary>
    protected override void UnregisterCallbacksFromTarget()
    {
        target!.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target!.UnregisterCallback<MouseDownEvent>(OnMouseDown);
    }

    private void BringToFront() => OrderManager.BringToFront(_renderer);

    private void OnPointerDown(PointerDownEvent e)
    {
        BringToFront();
    }

    private void OnMouseDown(MouseDownEvent e)
    {
        BringToFront();
    }
}