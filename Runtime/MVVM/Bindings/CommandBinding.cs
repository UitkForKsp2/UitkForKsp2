using UitkForKsp2.MVVM.Commands;
using Unity.Properties;
using UnityEngine.UIElements;

namespace UitkForKsp2.MVVM.Bindings
{
    /// <summary>
    /// Convenience binding for Button.clicked -> ICommand. Wires the button's
    /// clicked event and enable state to a parameterless command on the data source.
    /// Composite controls can use target-name to bind a named descendant button.
    ///
    /// UXML usage:
    /// <code>
    ///   &lt;ui:Button name="buy-btn" text="Buy"&gt;
    ///       &lt;Bindings&gt;
    ///           &lt;CommandBinding property="text" data-source-path="BuyCommand" /&gt;
    ///       &lt;/Bindings&gt;
    ///   &lt;/ui:Button&gt;
    /// </code>
    /// The "property" attribute is required by the infrastructure but unused - set it
    /// to any valid property (by convention "name"). For non-Button elements, use EventBinding.
    /// </summary>
    [UxmlObject]
    public partial class CommandBinding : CustomBinding
    {
        /// <summary>Path to the ICommand property on the data source, e.g. "BuyCommand".</summary>
        [UxmlAttribute("data-source-path")]
        public string DataSourcePath { get; set; }

        /// <summary>
        /// Optional descendant Button name. When omitted, the binding target must be a Button.
        /// </summary>
        [UxmlAttribute("target-name")]
        public string TargetName { get; set; } = string.Empty;

        private ICommand _currentCommand;
        private Button _button;

        public CommandBinding()
        {
            updateTrigger = BindingUpdateTrigger.OnSourceChanged;
        }

        protected override BindingResult Update(in BindingContext context)
        {
            if (!TryGetTargetButton(context.targetElement, out Button button))
            {
                return new BindingResult(
                    BindingStatus.Failure,
                    "CommandBinding must be on a Button or specify target-name for a descendant Button. " +
                    "Use EventBinding with event-type=\"ClickEvent\" for other elements."
                );
            }

            object? source = context.dataSource;
            if (source == null)
            {
                return new BindingResult(BindingStatus.Failure, "No data source found for CommandBinding.");
            }

            if (!PropertyContainer.TryGetValue<object, object>(
                    ref source,
                    new PropertyPath(DataSourcePath),
                    out object? value
                ))
            {
                return new BindingResult(BindingStatus.Failure, $"Cannot resolve '{DataSourcePath}' on data source.");
            }

            if (value is not ICommand command)
            {
                return new BindingResult(BindingStatus.Failure, $"'{DataSourcePath}' does not implement ICommand.");
            }

            if (!ReferenceEquals(command, _currentCommand) || !ReferenceEquals(button, _button))
            {
                Unbind();
                _currentCommand = command;
                _button = button;
                button.clicked += OnClicked;
                command.CanExecuteChanged += OnCanExecuteChanged;
            }

            button.SetEnabled(command.CanExecute());
            return new BindingResult(BindingStatus.Success);
        }

        protected override void OnDeactivated(in BindingActivationContext context)
        {
            Unbind();
            base.OnDeactivated(in context);
        }

        private void Unbind()
        {
            if (_currentCommand == null)
            {
                return;
            }

            if (_button != null)
            {
                _button.clicked -= OnClicked;
            }

            _currentCommand.CanExecuteChanged -= OnCanExecuteChanged;
            _currentCommand = null;
            _button = null;
        }

        private void OnClicked() => _currentCommand?.Execute();
        private void OnCanExecuteChanged() => _button?.SetEnabled(_currentCommand?.CanExecute() ?? false);

        private bool TryGetTargetButton(VisualElement targetElement, out Button button)
        {
            if (!string.IsNullOrWhiteSpace(TargetName))
            {
                button = targetElement?.Q<Button>(TargetName);
                return button != null;
            }

            button = targetElement as Button;
            return button != null;
        }
    }
}
