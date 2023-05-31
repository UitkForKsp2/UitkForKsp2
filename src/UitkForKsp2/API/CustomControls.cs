using System.Reflection;
using UnityEngine.UIElements;

namespace UitkForKsp2.API;

/// <summary>
/// API for everything related to custom UI controls.
/// </summary>
[PublicAPI]
public static class CustomControls
{
    /// <summary>
    /// Register all custom controls from the given assembly using their <see cref="IUxmlFactory"/>.
    /// </summary>
    /// <param name="assembly">The assembly to register the controls from.</param>
    public static void RegisterFromAssembly(Assembly assembly)
    {
        assembly.GetTypes()
            .Where(type => typeof(IUxmlFactory).IsAssignableFrom(type)
                           && !type.IsInterface
                           && !type.IsAbstract
                           && !type.IsGenericType)
            .ToList()
            .ForEach(type =>
            {
                var factory = Activator.CreateInstance(type) as IUxmlFactory;
                VisualElementFactoryRegistry.RegisterFactory(factory);
            });
    }
}