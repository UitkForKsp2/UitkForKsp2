using ReduxLib.GameInterfaces;
using UnityEngine.UIElements;

namespace UitkForKsp2.API.Localization;

/// <summary>
/// Global UITK binding converter for translating strings that use the '#Key/Path' convention.
/// </summary>
[PublicAPI]
public static class LocalizationConverter
{
    public const string GroupName = "Localization";

    private static bool _isRegistered;

    public static void Register()
    {
        if (_isRegistered)
        {
            return;
        }

        var group = new ConverterGroup(GroupName);
        group.AddConverter((ref string value) => Convert(value));
        ConverterGroups.RegisterConverterGroup(group);
        _isRegistered = true;
    }

    public static string Convert(string value)
    {
        if (string.IsNullOrEmpty(value) || value[0] != '#')
        {
            return value;
        }

        string localizationKey = value.TrimStart('#');
        return ILocalizer.Instance?.GetTranslation(localizationKey) ?? value;
    }
}
