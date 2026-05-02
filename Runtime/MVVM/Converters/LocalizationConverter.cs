using ReduxLib.GameInterfaces;

namespace UitkForKsp2.MVVM.Converters;

/// <summary>
/// UITK binding converter for translating strings that use the '#Key/Path' convention.
/// </summary>
[PublicAPI]
public sealed class LocalizationConverter : IConverter<string, string>
{
    public string Convert(string value) => Translate(value);

    public static string Translate(string value)
    {
        return TryTranslate(value, out string translation) ? translation : value;
    }

    public static bool TryTranslate(string value, out string translation)
    {
        if (string.IsNullOrEmpty(value) || value[0] != '#')
        {
            translation = value;
            return false;
        }

        string localizationKey = value.TrimStart('#');
        string? localizedValue = ILocalizer.Instance?.GetTranslation(localizationKey);
        if (localizedValue == null)
        {
            translation = value;
            return false;
        }

        translation = localizedValue;
        return true;
    }
}
