using System;
using System.Reflection;
using UitkForKsp2.MVVM.Commands;
using Unity.Properties;
using UnityEngine.UIElements;

namespace UitkForKsp2.MVVM.Bindings
{
    /// <summary>
    /// Binds any UI Toolkit event on any VisualElement to an ICommand or ICommand&lt;TEvent&gt;
    /// on the data source. Works with every EventBase subclass - clicks, pointer, keyboard,
    /// focus, change, drag, navigation, and more.
    ///
    /// UXML usage:
    /// <code>
    ///   &lt;!-- Parameterless: event fires command, data discarded --&gt;
    ///   &lt;EventBinding property="name" data-source-path="HoverCommand" event-type="PointerEnterEvent" /&gt;
    ///
    ///   &lt;!-- Typed: ViewModel receives full event object --&gt;
    ///   &lt;EventBinding property="value" data-source-path="KeyDownCommand" event-type="KeyDownEvent" /&gt;
    ///
    ///   &lt;!-- ChangeEvent special case: extracts newValue automatically --&gt;
    ///   &lt;EventBinding property="value" data-source-path="VolumeCommand" event-type="ChangeEvent&amp;lt;System.Single&amp;gt;" /&gt;
    /// </code>
    ///
    /// The "property" attribute is required by the infrastructure but unused - set it to any valid property (e.g. "name").
    /// Reflection runs only once per command instance - subsequent updates only sync enabled state.
    /// </summary>
    [UxmlObject]
    public partial class EventBinding : CustomBinding
    {
        /// <summary>Path to the ICommand property on the data source, e.g. "VolumeChangedCommand".</summary>
        [UxmlAttribute("data-source-path")]
        public string DataSourcePath { get; set; }

        /// <summary>UI Toolkit event class name. E.g. "ClickEvent", "KeyDownEvent", "ChangeEvent&lt;System.Single&gt;".</summary>
        [UxmlAttribute("event-type")]
        public string EventType { get; set; } = "ClickEvent";

        private ICallbackBridge _bridge;
        private object _currentCommand;
        private VisualElement _target;

        public EventBinding()
        {
            updateTrigger = BindingUpdateTrigger.OnSourceChanged;
        }

        protected override BindingResult Update(in BindingContext context)
        {
            object? source = context.dataSource;
            if (source == null)
            {
                return Fail("No data source.");
            }

            if (!PropertyContainer.TryGetValue<object, object>(
                    ref source,
                    new PropertyPath(DataSourcePath),
                    out object? commandObj
                ))
            {
                return Fail($"Cannot resolve '{DataSourcePath}' on data source.");
            }

            VisualElement? element = context.targetElement;

            if (!ReferenceEquals(commandObj, _currentCommand))
            {
                Teardown();

                Type? eventType = EventTypeResolver.Resolve(EventType);
                if (eventType == null)
                {
                    return Fail(
                        $"Cannot resolve event type '{EventType}'. Use the exact class name, e.g. " +
                        $"'PointerEnterEvent'. For generics: 'ChangeEvent<System.Single>'."
                    );
                }

                _bridge = CreateBridge(eventType, commandObj);
                if (_bridge == null)
                {
                    return Fail($"Command at '{DataSourcePath}' must implement ICommand or ICommand<{EventType}>.");
                }

                _bridge.Register(element);
                _currentCommand = commandObj;
                _target = element;
            }

            _bridge?.SyncEnabled(_target);
            return new BindingResult(BindingStatus.Success);
        }

        protected override void OnDeactivated(in BindingActivationContext context)
        {
            Teardown();
            base.OnDeactivated(in context);
        }

        private void Teardown()
        {
            if (_bridge != null && _target != null)
            {
                _bridge.Unregister(_target);
            }

            _bridge = null;
            _currentCommand = null;
            _target = null;
        }

        // ----------------------------------------------------------------
        //  Bridge factory
        // ----------------------------------------------------------------

        private static ICallbackBridge CreateBridge(Type eventType, object command)
        {
            // ChangeEvent<T> + ICommand<T> -> extract newValue
            if (IsChangeEvent(eventType))
            {
                Type? valueType = GetChangeValueType(eventType);
                if (valueType != null)
                {
                    Type cmdForValue = typeof(ICommand<>).MakeGenericType(valueType);
                    if (cmdForValue.IsInstanceOfType(command))
                    {
                        Type bridgeType = typeof(CallbackBridgeChangeValue<,>).MakeGenericType(eventType, valueType);
                        return (ICallbackBridge)Activator.CreateInstance(bridgeType, command);
                    }
                }
            }

            // ICommand<TEvent> -> pass full event
            Type cmdForEvent = typeof(ICommand<>).MakeGenericType(eventType);
            if (cmdForEvent.IsInstanceOfType(command))
            {
                Type bridgeType = typeof(CallbackBridgeTyped<>).MakeGenericType(eventType);
                return (ICallbackBridge)Activator.CreateInstance(bridgeType, command);
            }

            // ICommand -> parameterless
            if (command is ICommand)
            {
                Type bridgeType = typeof(CallbackBridgeParameterless<>).MakeGenericType(eventType);
                return (ICallbackBridge)Activator.CreateInstance(bridgeType, command);
            }

            return null;
        }

        private static bool IsChangeEvent(Type t) =>
            t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ChangeEvent<>);

        private static Type GetChangeValueType(Type t) =>
            IsChangeEvent(t) ? t.GetGenericArguments()[0] : null;

        private static BindingResult Fail(string msg) => new(BindingStatus.Failure, msg);

        // ================================================================
        //  Bridge variants - instantiated via MakeGenericType, IL2CPP-safe
        // ================================================================

        private interface ICallbackBridge
        {
            void Register(VisualElement element);
            void Unregister(VisualElement element);
            void SyncEnabled(VisualElement element);
        }

        [UnityEngine.Scripting.Preserve]
        private class CallbackBridgeParameterless<TEvent> : ICallbackBridge
            where TEvent : EventBase<TEvent>, new()
        {
            private readonly ICommand _command;
            private readonly EventCallback<TEvent> _callback;
            private Action _canExecCallback;

            public CallbackBridgeParameterless(object command)
            {
                _command = (ICommand)command;
                _callback = _ => _command.Execute();
            }

            public void Register(VisualElement el)
            {
                el.RegisterCallback(_callback);
                _canExecCallback = () => SyncEnabled(el);
                _command.CanExecuteChanged += _canExecCallback;
            }

            public void Unregister(VisualElement el)
            {
                el.UnregisterCallback(_callback);
                if (_canExecCallback != null)
                {
                    _command.CanExecuteChanged -= _canExecCallback;
                }

                _canExecCallback = null;
            }

            public void SyncEnabled(VisualElement el) => el.SetEnabled(_command.CanExecute());
        }

        [UnityEngine.Scripting.Preserve]
        private class CallbackBridgeTyped<TEvent> : ICallbackBridge
            where TEvent : EventBase<TEvent>, new()
        {
            private readonly ICommand<TEvent> _command;
            private readonly EventCallback<TEvent> _callback;
            private Action _canExecCallback;

            public CallbackBridgeTyped(object command)
            {
                _command = (ICommand<TEvent>)command;
                _callback = evt => _command.Execute(evt);
            }

            public void Register(VisualElement el)
            {
                el.RegisterCallback(_callback);
                _canExecCallback = () => SyncEnabled(el);
                _command.CanExecuteChanged += _canExecCallback;
            }

            public void Unregister(VisualElement el)
            {
                el.UnregisterCallback(_callback);
                if (_canExecCallback != null)
                {
                    _command.CanExecuteChanged -= _canExecCallback;
                }

                _canExecCallback = null;
            }

            public void SyncEnabled(VisualElement el) => el.SetEnabled(_command.CanExecute());
        }

        [UnityEngine.Scripting.Preserve]
        private class CallbackBridgeChangeValue<TEvent, TValue> : ICallbackBridge
            where TEvent : EventBase<TEvent>, new()
        {
            private readonly ICommand<TValue> _command;
            private readonly EventCallback<TEvent> _callback;
            private Action _canExecCallback;

            private static readonly PropertyInfo NewValueProp =
                typeof(TEvent).GetProperty("newValue", BindingFlags.Public | BindingFlags.Instance);

            public CallbackBridgeChangeValue(object command)
            {
                _command = (ICommand<TValue>)command;
                _callback = evt =>
                {
                    if (NewValueProp != null)
                    {
                        _command.Execute((TValue)NewValueProp.GetValue(evt));
                    }
                };
            }

            public void Register(VisualElement el)
            {
                el.RegisterCallback(_callback);
                _canExecCallback = () => SyncEnabled(el);
                _command.CanExecuteChanged += _canExecCallback;
            }

            public void Unregister(VisualElement el)
            {
                el.UnregisterCallback(_callback);
                if (_canExecCallback != null)
                {
                    _command.CanExecuteChanged -= _canExecCallback;
                }

                _canExecCallback = null;
            }

            public void SyncEnabled(VisualElement el) => el.SetEnabled(_command.CanExecute());
        }
    }
}