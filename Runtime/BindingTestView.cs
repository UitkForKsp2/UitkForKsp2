using UitkForKsp2.MVVM.Commands;
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
            GreetingConverter.Register();
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

    public static class GreetingConverter
    {
        public static void Register()
        {
            var group = new ConverterGroup("GreetingConverter");
            group.AddConverter((ref string username) => $"Hello, {username}!");
            ConverterGroups.RegisterConverterGroup(group);
        }
    }
}
