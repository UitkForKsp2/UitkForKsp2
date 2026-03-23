using System;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace UitkForKsp2.MVVM.Core
{
    /// <summary>
    /// Optional base class for ViewModels. Provides SetField to notify the
    /// binding system on change, eliminating per-property boilerplate.
    /// ViewModels only need to inherit this, not INotifyBindablePropertyChanged directly.
    /// </summary>
    public abstract class ViewModelBase : INotifyBindablePropertyChanged
    {
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        /// <summary>
        /// Sets a backing field and notifies the binding system if the value changed.
        /// Uses [CallerMemberName], never pass the property name manually.
        /// Returns true if the value changed.
        /// </summary>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(propertyName));
            return true;
        }

        /// <summary>
        /// Manually raises propertyChanged for computed or derived properties.
        /// </summary>
        protected void Notify([CallerMemberName] string propertyName = null)
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(propertyName));
        }
    }
}