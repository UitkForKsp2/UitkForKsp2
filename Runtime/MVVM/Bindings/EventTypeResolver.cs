using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace UitkForKsp2.MVVM.Bindings
{
    /// <summary>
    /// Resolves event type name strings from UXML attributes to System.Type objects.
    /// Supports simple names ("KeyDownEvent") and generic names ("ChangeEvent&lt;System.Single&gt;").
    /// Results are cached so reflection runs only once per unique type string.
    /// </summary>
    public static class EventTypeResolver
    {
        private static readonly Dictionary<string, Type> Cache = new();

        private static readonly System.Reflection.Assembly UIElementsAssembly =
            typeof(VisualElement).Assembly;

        public static Type Resolve(string eventTypeName)
        {
            if (string.IsNullOrWhiteSpace(eventTypeName))
            {
                return null;
            }

            if (Cache.TryGetValue(eventTypeName, out Type? cached))
            {
                return cached;
            }

            Type resolved = ResolveInternal(eventTypeName);
            Cache[eventTypeName] = resolved;
            return resolved;
        }

        private static Type ResolveInternal(string name)
        {
            int angleBracket = name.IndexOf('<');
            if (angleBracket > 0)
            {
                return ResolveGeneric(name, angleBracket);
            }

            return UIElementsAssembly.GetType($"UnityEngine.UIElements.{name}")
                ?? Type.GetType(name)
                ?? Type.GetType($"UnityEngine.UIElements.{name}, UnityEngine.UIElementsModule");
        }

        private static Type ResolveGeneric(string name, int angleBracket)
        {
            string outerName = name[..angleBracket];
            string argName = name.Substring(angleBracket + 1, name.Length - angleBracket - 2);

            Type? genericDef = UIElementsAssembly.GetType($"UnityEngine.UIElements.{outerName}`1");
            if (genericDef == null)
            {
                return null;
            }

            Type? argType = Type.GetType(argName)
                ?? Type.GetType($"UnityEngine.UIElements.{argName}")
                ?? UIElementsAssembly.GetType($"UnityEngine.UIElements.{argName}");

            return argType == null ? null : genericDef.MakeGenericType(argType);
        }
    }
}