using UitkForKsp2.MVVM.Commands;
using UitkForKsp2.MVVM.Converters;
using UitkForKsp2.MVVM.Core;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2
{
    [RequireComponent(typeof(UIDocument))]
    public class BindingTestView : MonoBehaviour
    {
        private BindingTestViewModel _vm;

        public BindingTestView()
        {
            ConverterGroupRegistry.Register(new GreetingConverterGroup());
        }

        private void Start()
        {
            _vm = new BindingTestViewModel();
            GetComponent<UIDocument>().rootVisualElement.dataSource = _vm;
        }
    }

    public class BindingTestViewModel : ViewModelBase
    {
        private string _username;

        [CreateProperty]
        public string Username
        {
            get => _username;
            set => SetField(ref _username, value);
        }

        private int _counter;

        [CreateProperty]
        public string CounterText => $"Counter: {_counter}";

        [CreateProperty]
        public RelayCommand IncrementCommand { get; }

        private int _boundInteger;

        [CreateProperty]
        public int BoundInteger
        {
            get => _boundInteger;
            set => SetField(ref _boundInteger, value);
        }

        public BindingTestViewModel()
        {
            IncrementCommand = new RelayCommand(() =>
            {
                _counter++;
                Notify(nameof(CounterText));
            });
        }
    }

    public sealed class GreetingConverter : IConverter<string, string>
    {
        public string Convert(string username) => $"Hello, {username}!";
    }

    public sealed class GreetingConverterGroup : IConverterGroup
    {
        public string Id => "GreetingConverter";

        public string DisplayName => "Greeting Converter";

        public string Description => "Formats a username as a greeting.";

        public bool RefreshOnLocalizationChange => false;

        public void RegisterConverters(ConverterGroupBuilder builder) => builder.Add(new GreetingConverter());
    }
}
