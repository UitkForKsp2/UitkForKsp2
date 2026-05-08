using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

internal class UITestController : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset testUxml;

    private void Start()
    {
        Window.Create(
            WindowOptions.Default with { IsHidingEnabled = false },
            testUxml
        );
    }
}