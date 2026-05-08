using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2.Panel;

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