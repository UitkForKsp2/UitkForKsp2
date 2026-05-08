namespace UitkForKsp2.MVVM.Converters;

public sealed class LocalizationGroup : IConverterGroup
{
    public const string GroupId = "Localization";

    public string Id => GroupId;

    public string DisplayName => "Localization";

    public string Description => "Translates strings that use the '#Key/Path' convention.";

    public bool RefreshOnLocalizationChange => true;

    public void RegisterConverters(ConverterGroupBuilder builder)
    {
        builder.Add(new LocalizationConverter());
    }
}

public sealed class UppercaseLocalizationGroup : IConverterGroup
{
    public const string GroupId = "UppercaseLocalization";

    public string Id => GroupId;

    public string DisplayName => "Uppercase Localization";

    public string Description =>
        "Translates strings that use the '#Key/Path' convention and uppercases the displayed value.";

    public bool RefreshOnLocalizationChange => true;

    public void RegisterConverters(ConverterGroupBuilder builder)
    {
        builder.Add(new UppercaseLocalizationConverter());
    }
}
