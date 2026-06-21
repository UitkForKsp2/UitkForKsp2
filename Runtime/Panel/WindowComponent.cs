using System;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2.Panel;

/// <summary>
/// Companion component attached to every window created through <see cref="Window"/>. Because
/// <see cref="PanelRenderer.rootVisualElement"/> is internal, this component captures the resolved root, runs the
/// one-time window setup, and exposes the root to consumers and to library subsystems (Dialog, DocumentLocalization).
/// </summary>
/// <remarks>
/// The PanelRenderer UI-reload callback only fires once the renderer attaches to its panel, which happens on the next
/// UI update tick (deferred). To preserve the synchronous "root available immediately after Window.Create" contract
/// that consumers rely on, the root is also resolved synchronously through the internal <c>rootVisualElement</c>
/// property right after activation. The reload callback then re-resolves on live reload / disable-enable cycles.
/// </remarks>
[DisallowMultipleComponent]
internal sealed class WindowComponent : MonoBehaviour
{
    private PanelRenderer _renderer = null!;
    private WindowOptions _options;
    private VisualTreeAsset? _uxml;
    private VisualElement? _programmaticRoot;
    private bool _wired;
    private VisualElement? _lastPanelRoot;

    /// <summary>
    /// The PanelRenderer's per-window container (analogous to the old <c>UIDocument.rootVisualElement</c>).
    /// </summary>
    public VisualElement? PanelRoot { get; private set; }

    /// <summary>
    /// The unwrapped window root (analogous to the old <c>UIDocument.rootVisualElement[0]</c>).
    /// </summary>
    public VisualElement? WindowRoot { get; private set; }

    public PanelRenderer Renderer => _renderer;

    /// <summary>
    /// Raised whenever the UI is (re)loaded with a freshly built tree once the window root is resolved. Used to
    /// re-run localization after a live reload. Not raised for a plain disable/enable cycle where content is preserved.
    /// </summary>
    public event Action<VisualElement>? RootResolved;

    public void Initialize(
        PanelRenderer renderer,
        WindowOptions options,
        VisualTreeAsset? uxml,
        VisualElement? programmaticRoot
    )
    {
        _renderer = renderer;
        _options = options;
        _uxml = uxml;
        _programmaticRoot = programmaticRoot;

        // Re-resolve on live reload / re-enable. May also fire immediately if a root+panel already exist.
        _renderer.RegisterUIReloadCallback(OnReload);
    }

    /// <summary>
    /// Resolves the root synchronously from the internal <c>rootVisualElement</c> (created during activation), so the
    /// root is available immediately after <see cref="Window.Create"/> returns.
    /// </summary>
    public void ResolveNow()
    {
        // PanelRenderer defers cloning its UXML to the next UI tick; force it now so the root + content are
        // available synchronously after Window.Create (matching the old UIDocument behaviour).
        _renderer.ForceBuild();

        if (_renderer.GetRendererRoot() is { } panelRoot)
        {
            Resolve(panelRoot);
        }
    }

    private void OnReload(PanelRenderer renderer, VisualElement panelRoot)
    {
        Resolve(panelRoot);
    }

    private void Resolve(VisualElement panelRoot)
    {
        bool isNewTree = !ReferenceEquals(panelRoot, _lastPanelRoot);
        _lastPanelRoot = panelRoot;

        PanelRoot = panelRoot;
        Window.ConfigureDocumentRoot(panelRoot);

        VisualElement? windowRoot;
        if (_uxml == null)
        {
            // Programmatic path: PanelRenderer hands us an empty container; insert the caller's root once.
            if (_programmaticRoot != null && _programmaticRoot.parent != panelRoot)
            {
                _programmaticRoot.RemoveFromHierarchy();
                panelRoot.Add(_programmaticRoot);
            }

            windowRoot = _programmaticRoot;
        }
        else
        {
            // UXML path: PanelRenderer already cloned visualTreeAsset into panelRoot.
            windowRoot = Window.ResolveWindowRoot(panelRoot);
        }

        WindowRoot = windowRoot;

        if (windowRoot == null)
        {
            return;
        }

        if (!_wired)
        {
            _wired = true;
            Window.SetupRootElement(windowRoot, _renderer, _options);
        }

        // Only notify on a genuinely new tree, so the synchronous resolve and the immediate deferred callback for the
        // same root don't double-fire, and a plain re-enable (content preserved) doesn't redundantly re-localize.
        if (isNewTree)
        {
            RootResolved?.Invoke(windowRoot);
        }
    }

    private void OnDestroy()
    {
        if (_renderer != null)
        {
            _renderer.UnregisterUIReloadCallback(OnReload);
        }
    }
}
