using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2.Panel;

/// <summary>
/// Owns the per-window <see cref="PanelSettings"/> instance created for a window and destroys it when the window
/// GameObject is destroyed, so each window can control its own z-order (sortingOrder) relative to other UITK panels
/// and uGUI canvases.
/// </summary>
internal sealed class PanelSettingsOwner : MonoBehaviour
{
    public PanelSettings? Owned;

    private void OnDestroy()
    {
        if (Owned == null)
        {
            return;
        }

        Destroy(Owned);
        Owned = null!;
    }
}
