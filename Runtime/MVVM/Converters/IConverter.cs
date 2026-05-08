using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace UitkForKsp2.MVVM.Converters;

public interface IConverter
{
}

public interface IConverter<in TFrom, out TTo> : IConverter
{
    public TTo Convert(TFrom from);
}

public interface IConverterGroup
{
    public string Id { get; }

    public string DisplayName { get; }

    public string Description { get; }

    public bool RefreshOnLocalizationChange { get; }

    public void RegisterConverters(ConverterGroupBuilder builder);
}

public sealed class ConverterGroupBuilder
{
    private readonly ConverterGroup _group;

    public ConverterGroupBuilder(string id, string displayName, string description)
    {
        _group = new ConverterGroup(id, displayName, description);
    }

    public ConverterGroupBuilder Add<TFrom, TTo>(IConverter<TFrom, TTo> converter)
    {
        _group.AddConverter((ref TFrom value) => converter.Convert(value));
        return this;
    }

    public ConverterGroup Build() => _group;
}

public static class ConverterGroupRegistry
{
    private static readonly HashSet<string> RegisteredGroups = new(StringComparer.Ordinal);
    private static readonly HashSet<string> LocalizationRefreshGroups = new(StringComparer.Ordinal);

    public static void Register(params IConverterGroup[] groups)
    {
        foreach (IConverterGroup group in groups)
        {
            Register(group);
        }
    }

    public static void Register(IConverterGroup group)
    {
        if (RegisteredGroups.Contains(group.Id))
        {
            return;
        }

        var builder = new ConverterGroupBuilder(group.Id, group.DisplayName, group.Description);
        group.RegisterConverters(builder);
        ConverterGroups.RegisterConverterGroup(builder.Build());
        RegisteredGroups.Add(group.Id);

        if (group.RefreshOnLocalizationChange)
        {
            LocalizationRefreshGroups.Add(group.Id);
        }
    }

    public static bool RefreshesOnLocalizationChange(string groupId)
    {
        return LocalizationRefreshGroups.Contains(groupId);
    }
}
