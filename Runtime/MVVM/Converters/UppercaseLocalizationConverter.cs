namespace UitkForKsp2.MVVM.Converters;

/// <summary>
/// UITK binding converter for translating '#Key/Path' strings and displaying the result in uppercase.
/// </summary>
[PublicAPI]
public sealed class UppercaseLocalizationConverter : IConverter<string, string>
{
    public string Convert(string value)
    {
        return LocalizationConverter.TryTranslate(value, out string localized)
            ? localized.ToUpper()
            : value;
    }
}